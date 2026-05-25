// Copyright (c) by DNN Community
//
// DNN Community licenses this file to you under the MIT license.
//
// See the LICENSE file in the project root for more information.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions
// of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

namespace DotNetNuke.Modules.ActiveForums
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Modules.ActiveForums.Entities;
    using DotNetNuke.Modules.ActiveForums.Enums;
    using DotNetNuke.Modules.ActiveForums.Extensions;
    using DotNetNuke.Modules.ActiveForums.Helpers;
    using DotNetNuke.Modules.ActiveForums.Services;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Web.Api;
    using DotNetNuke.Web.Api.Internal;

    public class AttachmentController : ControllerBase<AttachmentController>
    {
        [HttpPost]
        [DnnAuthorize]
        //[ForumsAuthorize(SecureActions.Attach)]
        [IFrameSupportedValidateAntiForgeryToken]
        //public Task<HttpResponseMessage> UploadFile(int forumId)
        public Task<HttpResponseMessage> UploadFile()
        {
            // This method uploads an attachment to a temporary directory and returns a JSON object containing information about the original file
            // including the temporary file name.  When the post is saved/updated, the temporary file is moved to the appropriate attachment directory

            // Have to a reference to these variables as the internal reference isn't available.
            // in the async result.
            var request = this.Request;
            var portalSettings = this.PortalSettings;
            var userInfo = portalSettings.UserInfo;
            var forumUser = new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ActiveModule.ModuleID).GetByUserId(this.ActiveModule.PortalID, userInfo.UserID);

            if (!request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotAcceptable));
            }

            var folderManager = DotNetNuke.Services.FileSystem.FolderManager.Instance;
            const string uploadPath = Globals.AttachmentUploadsFolderName;
            var uploadFolder = folderManager.GetFolder(this.ActiveModule.PortalID, uploadPath) ?? folderManager.AddFolder(this.ActiveModule.PortalID, uploadPath);

            var provider = new MultipartFormDataStreamProvider(uploadFolder.PhysicalPath);

            var task = request.Content.ReadAsMultipartAsync(provider).ContinueWith(t =>
            {
                if (t.IsFaulted || t.IsCanceled)
                {
                    throw new HttpResponseException(HttpStatusCode.InternalServerError);
                }

                // Make sure a temp file was uploaded and that it exists
                var file = provider.FileData.FirstOrDefault();
                if (file == null || string.IsNullOrWhiteSpace(file.LocalFileName) || !File.Exists(file.LocalFileName))
                {
                    return request.CreateErrorResponse(HttpStatusCode.NoContent, "No File Found");
                }

                // Get the file name without the full path
                var localFileName = Path.GetFileName(file.LocalFileName).EmptyIfNull();

                // Check to make sure that a forum was specified and that the the user has upload permissions
                // This is only an initial check, it will be done again when the file is saved to a post.
                int forumId;
                if (!int.TryParse(provider.FormData["forumId"], out forumId))
                {
                    File.Delete(file.LocalFileName);
                    return request.CreateErrorResponse(HttpStatusCode.BadRequest, "Forum Not Specified");
                }

                // Make sure that we can find the forum and that attachments are allowed
                var forum = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Forums_Get(this.ActiveModule.PortalID, this.ActiveModule.ModuleID, forumId, true, -1);

                if (forum == null || !forum.FeatureSettings.AllowAttach)
                {
                    File.Delete(file.LocalFileName);
                    return request.CreateErrorResponse(HttpStatusCode.BadRequest, "Forum Not Found");
                }

                // Make sure the user has permissions to attach files
                if (forumUser == null || !DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(forum.Security.ViewRoleIds, forumUser.UserRoleIds))
                {
                    File.Delete(file.LocalFileName);
                    return request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Not Authorized");
                }

                // Make sure that the file size does not exceed the limit (in KB) for the forum
                // Have to do this since content length is not available when using MultipartFormDataStreamProvider
                var di = new DirectoryInfo(uploadFolder.PhysicalPath);
                var fileSize = di.GetFiles(localFileName)[0].Length;

                var maxAllowedFileSize = (long)forum.FeatureSettings.AttachMaxSize * 1024;

                if ((forum.FeatureSettings.AttachMaxSize > 0) && (fileSize > maxAllowedFileSize))
                {
                    File.Delete(file.LocalFileName);
                    return request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Exceeds Max File Size");
                }

                // Get the original file name from the content disposition header
                var fileName = file.Headers.ContentDisposition.FileName.Replace("\"", string.Empty);

                if (string.IsNullOrWhiteSpace(fileName))
                {
                    File.Delete(file.LocalFileName);
                    return request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "Invalid File");
                }

                // Make sure we have an acceptable extension type.
                // Check against both the forum configuration and the host configuration
                var extension = Path.GetExtension(fileName).EmptyIfNull().Replace(".", string.Empty).ToLower();
                var isForumAllowedExtension = string.IsNullOrWhiteSpace(forum.FeatureSettings.AttachTypeAllowed) || forum.FeatureSettings.AttachTypeAllowed.Replace(".", string.Empty).Split(',').Any(val => val == extension);
                if (string.IsNullOrEmpty(extension) || !isForumAllowedExtension || !Host.AllowedExtensionWhitelist.IsAllowedExtension(extension))
                {
                    File.Delete(file.LocalFileName);
                    return request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "File Type Not Allowed");
                }

                var fileNameOnly = Path.GetFileNameWithoutExtension(fileName);
                var attachmentFolder = folderManager.GetUserFolder(userInfo);
                var fileManager = FileManager.Instance;
                IFileInfo ufile = null;
                string sExt = Path.GetExtension(fileName);

                if (sExt.ToLower().Equals(".jpg", StringComparison.InvariantCultureIgnoreCase) ||
                    sExt.ToLower().Equals(".bmp", StringComparison.InvariantCultureIgnoreCase) ||
                    sExt.ToLower().Equals(".png", StringComparison.InvariantCultureIgnoreCase) ||
                    sExt.ToLower().Equals(".jpeg", StringComparison.InvariantCultureIgnoreCase))
                {
                    ImageFormat imf;

                    var img = System.Drawing.Image.FromFile(file.LocalFileName);

                    var maxWidth = forum.FeatureSettings.MaxImageWidth;
                    var maxHeight = forum.FeatureSettings.MaxImageHeight;

                    int imgWidth = img.Width;
                    int imgHeight = img.Height;

                    var ratioWidth = (double)imgWidth / maxWidth;
                    var ratioHeight = (double)imgHeight / maxHeight;

                    switch (sExt.ToLower())
                    {
                        case ".png":
                            {
                                imf = ImageFormat.Png;
                                break;
                            }

                        case ".bmp":
                            {
                                imf = ImageFormat.Bmp;
                                break;
                            }

                        default:
                            {
                                imf = ImageFormat.Jpeg;
                                break;
                            }
                    }

                    MemoryStream mst = new MemoryStream();

                    if (ratioWidth > 1 || ratioHeight > 1)
                    {
                        if (ratioWidth > ratioHeight)
                        {
                            imgWidth = maxWidth;
                            imgHeight = (int)Math.Round(imgHeight / ratioWidth);
                        }
                        else if (ratioWidth < ratioHeight)
                        {
                            imgHeight = maxHeight;
                            imgWidth = (int)Math.Round(imgWidth / ratioHeight);
                        }
                        else
                        {
                            imgWidth = maxWidth;
                            imgHeight = maxHeight;
                        }
                    }

                    Bitmap res = new Bitmap(imgWidth, imgHeight);
                    using (Graphics gr = Graphics.FromImage(res))
                    {
                        gr.Clear(Color.Transparent);
                        gr.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        gr.DrawImage(img, new Rectangle(0, 0, imgWidth, imgHeight), new Rectangle(0, 0, img.Width, img.Height), GraphicsUnit.Pixel);
                        gr.Dispose();
                    }

                    img.Dispose();
                    res.Save(mst, imf);
                    res.Dispose();

                    fileName = DotNetNuke.Modules.ActiveForums.Controllers.AttachmentController.CreateUniqueFileName(attachmentFolder.PhysicalPath, fileName);
                    ufile = fileManager.AddFile(attachmentFolder, fileName, (Stream)mst);
                    mst.Close();
                }
                else
                {
                    using (var fileStream = new FileStream(file.LocalFileName, FileMode.Open, FileAccess.Read))
                    {
                        fileName = DotNetNuke.Modules.ActiveForums.Controllers.AttachmentController.CreateUniqueFileName(attachmentFolder.PhysicalPath, fileName);
                        ufile = fileManager.AddFile(attachmentFolder, fileName, fileStream);
                    }
                }

                // IE<=9 Hack - can't return application/json
                var mediaType = "application/json";
                if (!request.Headers.Accept.Any(h => h.MediaType.Equals("application/json", StringComparison.OrdinalIgnoreCase)))
                {
                    mediaType = "text/html";
                }

                File.Delete(file.LocalFileName);

                if (ufile != null)
                {
                    var result = new DotNetNuke.Modules.ActiveForums.Entities.ClientAttachment()
                    {
                        FileId = ufile.FileId,
                        ContentType = file.Headers.ContentType.MediaType,
                        FileName = fileName,
                        FileSize = ufile.Size,
                        UploadId = localFileName,
                    };

                    return this.Request.CreateResponse(HttpStatusCode.Accepted, result, mediaType);
                }
                else
                {
                    return request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "No File Found");
                }
            });

            return task;
        }

        [HttpGet]
        [DnnAuthorize]
        [ForumsAuthorize(SecureActions.Attach)]
        public HttpResponseMessage GetUserFileUrl(int forumId, int fileId)
        {
            var fileManager = FileManager.Instance;
            var file = fileManager.GetFile(fileId);

            if (file == null)
            {
                return this.Request.CreateResponse(HttpStatusCode.Accepted, "File not found");
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, Utilities.ResolveUrl($"https://{this.PortalSettings.DefaultPortalAlias}{fileManager.GetUrl(file)}", portalSettings: this.PortalSettings));
        }
    }
}
