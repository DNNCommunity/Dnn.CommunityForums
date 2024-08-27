// Copyright (c) 2013-2024 by DNN Community
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
    using System.Net.Http.Formatting;
    using System.Net.Http.Headers;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Script.Serialization;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Modules.ActiveForums.DAL2;
    using DotNetNuke.Modules.ActiveForums.Extensions;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Web.Api;
    using DotNetNuke.Web.Api.Internal;

    [DnnAuthorize]
    [ValidateAntiForgeryToken]
    public class ForumServiceController : DnnApiController
    {
        public HttpResponseMessage CreateThumbnail(CreateThumbnailDTO dto)
        {
            var fileManager = FileManager.Instance;
            var folderManager = FolderManager.Instance;

            var file = fileManager.GetFile(dto.FileId);

            if (file == null)
            {
                return this.Request.CreateResponse(HttpStatusCode.NotFound, "File Not Found");

                // return Json(new {Result = "error"});
            }

            var folder = folderManager.GetFolder(file.FolderId);

            var ext = file.Extension;
            if (!ext.StartsWith("."))
            {
                ext = "." + ext;
            }

            var sizedPhoto = file.FileName.Replace(ext, "_" + dto.Width.ToString(CultureInfo.InvariantCulture) + "x" + dto.Height.ToString(CultureInfo.InvariantCulture) + ext);

            var newPhoto = fileManager.AddFile(folder, sizedPhoto, fileManager.GetFileContent(file));
            ImageUtils.CreateImage(newPhoto.PhysicalPath, dto.Height, dto.Width);
            newPhoto = fileManager.UpdateFile(newPhoto);

            return this.Request.CreateResponse(HttpStatusCode.OK, newPhoto.ToJson());
        }

        public string EncryptTicket(EncryptTicketDTO dto)
        {
            var x = Common.Globals.LinkClick(dto.Url, this.ActiveModule.TabID, this.ActiveModule.ModuleID, false);
            return UrlUtils.EncryptParameter(UrlUtils.GetParameterValue(dto.Url));
        }

        [HttpPost]
        [IFrameSupportedValidateAntiForgeryToken]
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

            const string uploadPath = "activeforums_Upload";

            var folderManager = FolderManager.Instance;
            if (!folderManager.FolderExists(this.ActiveModule.PortalID, uploadPath))
            {
                folderManager.AddFolder(this.ActiveModule.PortalID, uploadPath);
            }

            var folder = folderManager.GetFolder(this.ActiveModule.PortalID, uploadPath);

            var provider = new MultipartFormDataStreamProvider(folder.PhysicalPath);

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
                var localFileName = Path.GetFileName(file.LocalFileName).TextOrEmpty();

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

                if (forum == null || !forum.AllowAttach)
                {
                    File.Delete(file.LocalFileName);
                    return request.CreateErrorResponse(HttpStatusCode.BadRequest, "Forum Not Found");
                }

                // Make sure the user has permissions to attach files
                if (forumUser == null || !DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(forum.Security.Attach, forumUser.UserRoles))
                {
                    File.Delete(file.LocalFileName);
                    return request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Not Authorized");
                }

                // Make sure that the file size does not exceed the limit (in KB) for the forum
                // Have to do this since content length is not available when using MultipartFormDataStreamProvider
                var di = new DirectoryInfo(folder.PhysicalPath);
                var fileSize = di.GetFiles(localFileName)[0].Length;

                var maxAllowedFileSize = (long)forum.AttachMaxSize * 1024;

                if ((forum.AttachMaxSize > 0) && (fileSize > maxAllowedFileSize))
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
                var extension = Path.GetExtension(fileName).TextOrEmpty().Replace(".", string.Empty).ToLower();
                var isForumAllowedExtension = string.IsNullOrWhiteSpace(forum.AttachTypeAllowed) || forum.AttachTypeAllowed.Replace(".", string.Empty).Split(',').Any(val => val == extension);
                if (string.IsNullOrEmpty(extension) || !isForumAllowedExtension || !Host.AllowedExtensionWhitelist.IsAllowedExtension(extension))
                {
                    File.Delete(file.LocalFileName);
                    return request.CreateErrorResponse(HttpStatusCode.NotAcceptable, "File Type Not Allowed");
                }

                const string newFileName = "{0}_{1}{2}";
                var fileNameOnly = Path.GetFileNameWithoutExtension(fileName);
                var userFolder = folderManager.GetUserFolder(userInfo);
                var attachmentFolder = folderManager.GetFolder(userFolder.FolderID);
                var fileManager = FileManager.Instance;
                IFileInfo ufile = null;
                string sExt = Path.GetExtension(fileName);

                if (sExt.ToLower() == ".jpg" || sExt.ToLower() == ".bmp" || sExt.ToLower() == ".png" || sExt.ToLower() == ".jpeg")
                {
                    var sExtOut = ".jpg";
                    ImageFormat imf, imfout = ImageFormat.Jpeg;

                    Image img = Image.FromFile(file.LocalFileName);
                    Image nimg;

                    var maxWidth = forum.MaxAttachWidth;
                    var maxHeight = forum.MaxAttachHeight;

                    int imgWidth = img.Width;
                    int imgHeight = img.Height;

                    var ratioWidth = (double)imgWidth / maxWidth;
                    var ratioHeight = (double)imgHeight / maxHeight;

                    switch (sExt.ToLower())
                    {
                        case ".png":
                            {
                                imf = ImageFormat.Png;
                                if (!forum.ConvertingToJpegAllowed)
                                {
                                    sExtOut = ".png";
                                    imfout = ImageFormat.Png;
                                }

                                break;
                            }

                        case ".bmp": imf = ImageFormat.Bmp; break;
                        default: imf = ImageFormat.Jpeg; break;
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
                    res.Save(mst, imfout);
                    res.Dispose();

                    var index = 0;
                    fileName = fileNameOnly + sExtOut;

                    while (fileManager.FileExists(attachmentFolder, fileName))
                    {
                        index++;
                        fileName = string.Format(newFileName, fileNameOnly, index, sExtOut);
                    }

                    ufile = fileManager.AddFile(attachmentFolder, fileName, (Stream)mst);
                    mst.Close();
                }
                else
                {
                    using (var fileStream = new FileStream(file.LocalFileName, FileMode.Open, FileAccess.Read))
                    {
                        var index = 0;
                        while (fileManager.FileExists(attachmentFolder, fileName))
                        {
                            index++;
                            fileName = string.Format(newFileName, fileNameOnly, index, sExt);
                        }

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
                    var result = new ClientAttachment()
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
        public HttpResponseMessage GetUserFileUrl(int fileId)
        {
            var fileManager = FileManager.Instance;
            var file = fileManager.GetFile(fileId);

            if (file == null)
            {
                return this.Request.CreateResponse(HttpStatusCode.Accepted, "File not found");
            }

            const string fullpath = "/Portals/{0}/{1}{2}";
            var userInfo = this.PortalSettings.UserInfo;
            string portalid;

            if (userInfo.IsSuperUser)
            {
                portalid = "_default";
            }
            else
            {
                portalid = userInfo.PortalID.ToString();
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, string.Format(fullpath, portalid, file.Folder, file.FileName));
        }

        /*public class GetUserFileUrlDTO
        {
            public int FileId { get; set; }
        }*/

        [HttpGet]
        public HttpResponseMessage GetTopicList(int forumId)
        {
            var portalSettings = this.PortalSettings;
            var userInfo = portalSettings.UserInfo;

            DataSet ds = DataProvider.Instance().UI_TopicsView(portalSettings.PortalId, this.ActiveModule.ModuleID, forumId, userInfo.UserID, 0, 20, userInfo.IsSuperUser, SortColumns.ReplyCreated);
            if (ds.Tables.Count > 0)
            {
                DataTable dtTopics = ds.Tables[3];

                Dictionary<string, string> rows = new Dictionary<string, string>();
                foreach (DataRow dr in dtTopics.Rows)
                {
                    rows.Add(dr["TopicId"].ToString(), dr["Subject"].ToString());
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, rows.ToJson());
            }

            return this.Request.CreateResponse(HttpStatusCode.NotFound);
        }

        [HttpGet]
        public HttpResponseMessage GetForumsList()
        {
            var portalSettings = this.PortalSettings;
            var userInfo = portalSettings.UserInfo;
            var forumUser = new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ActiveModule.ModuleID).GetByUserId(this.ActiveModule.PortalID, userInfo.UserID);
            Dictionary<string, string> rows = new Dictionary<string, string>();
            foreach (DotNetNuke.Modules.ActiveForums.Entities.ForumInfo fi in new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().Get(this.ActiveModule.ModuleID).Where(f => !f.Hidden && !f.ForumGroup.Hidden && (this.UserInfo.IsSuperUser || DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(f.Security.View, forumUser.UserRoles))))
            {
                rows.Add(fi.ForumID.ToString(), fi.ForumName.ToString());
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, rows.ToJson());
        }

        public class CreateSplitDTO
        {
            public int OldTopicId { get; set; }

            public int NewTopicId { get; set; }

            public int NewForumId { get; set; }

            public string Subject { get; set; }

            public string Replies { get; set; }
        }

        [HttpPost]
        public HttpResponseMessage CreateSplit(CreateSplitDTO dto)
        {
            if (dto.NewTopicId == dto.OldTopicId)
            {
                return this.Request.CreateResponse(HttpStatusCode.OK);
            }

            var portalSettings = this.PortalSettings;
            var userInfo = portalSettings.UserInfo;
            var forumUser = new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ActiveModule.ModuleID).GetByUserId(this.ActiveModule.PortalID, userInfo.UserID);

            var forum_out = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Forums_Get(portalSettings.PortalId, this.ActiveModule.ModuleID, 0, true, dto.OldTopicId);
            var forum_in = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(dto.NewForumId, this.ActiveModule.ModuleID);
            if (forum_out != null && forum_in != null)
            {
                var ti = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(dto.OldTopicId);
                if (ti != null)
                {
                    bool hasCreatePerm;

                    if (forum_out == forum_in)
                    {
                        hasCreatePerm = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(forum_out.Security.Create, forumUser.UserRoles);
                    }
                    else
                    {
                        hasCreatePerm = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(forum_out.Security.Create, forumUser.UserRoles) && DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(forum_in.Security.Create, forumUser.UserRoles);
                    }

                    var canSplit = (ti.Content.AuthorId == userInfo.UserID && DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(forum_out.Security.Split, forumUser.UserRoles)) || userInfo.IsAdmin || userInfo.IsSuperUser ||
                                   (DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(forum_out.Security.Moderate, forumUser.UserRoles) && DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(forum_out.Security.Split, forumUser.UserRoles));

                    if (hasCreatePerm && canSplit)
                    {
                        int topicId;

                        if (dto.NewTopicId < 1)
                        {
                            var subject = Utilities.CleanString(portalSettings.PortalId, dto.Subject, false, EditorTypes.TEXTBOX, false, false, this.ActiveModule.ModuleID, string.Empty, false);
                            var replies = dto.Replies.Split('|');
                            var firstReply = DotNetNuke.Modules.ActiveForums.Controllers.ReplyController.GetReply(Convert.ToInt32(replies[0]));
                            var firstContent = new DotNetNuke.Modules.ActiveForums.Controllers.ContentController().GetById(firstReply.ContentId);
                            topicId = DotNetNuke.Modules.ActiveForums.Controllers.TopicController.QuickCreate(portalSettings.PortalId, this.ActiveModule.ModuleID, dto.NewForumId, subject, string.Empty, firstContent.AuthorId, firstContent.AuthorName, true, this.Request.GetIPAddress());
                            DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Replies_Split(dto.OldTopicId, topicId, dto.Replies, true);
                        }
                        else
                        {
                            topicId = dto.NewTopicId;
                            DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Replies_Split(dto.OldTopicId, topicId, dto.Replies, false);
                        }

                        return this.Request.CreateResponse(HttpStatusCode.OK, topicId);
                    }

                    return this.Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
            }
            return this.Request.CreateResponse(HttpStatusCode.BadRequest);
        }

        public class CreateThumbnailDTO
        {
            public int FileId { get; set; }

            public int Height { get; set; }

            public int Width { get; set; }
        }

        public class EncryptTicketDTO
        {
            public string Url { get; set; }
        }
    }
}
