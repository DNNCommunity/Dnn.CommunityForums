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

namespace DotNetNuke.Modules.ActiveForums.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Mail;
    using System.Reflection;
    using System.Text.RegularExpressions;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Services.FileSystem;

    /// <summary>
    /// Controller for managing Attachments in the DNN Community Forums module.
    /// </summary>
    internal class AttachmentController : DotNetNuke.Modules.ActiveForums.Controllers.RepositoryControllerBase<DotNetNuke.Modules.ActiveForums.Entities.AttachmentInfo>
    {
        private readonly DotNetNuke.Services.FileSystem.IFolderManager folderManager;
        private readonly DotNetNuke.Services.FileSystem.IFileManager fileManager;
        private readonly DotNetNuke.Modules.ActiveForums.Controllers.ContentController contentController;

        public AttachmentController()
            : this(
                  DotNetNuke.Services.FileSystem.FolderManager.Instance,
                  DotNetNuke.Services.FileSystem.FileManager.Instance,
                  new DotNetNuke.Modules.ActiveForums.Controllers.ContentController())
        {
        }

        public AttachmentController(
            DotNetNuke.Services.FileSystem.IFolderManager folderManager,
            DotNetNuke.Services.FileSystem.IFileManager fileManager,
            DotNetNuke.Modules.ActiveForums.Controllers.ContentController contentController)
        {
            this.folderManager = folderManager;
            this.fileManager = fileManager;
            this.contentController = contentController;
        }

        internal IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.AttachmentInfo> GetByContentId(int contentId)
        {
            return this.Find("WHERE ContentId = @0", contentId);
        }

        internal new void Delete(DotNetNuke.Modules.ActiveForums.Entities.AttachmentInfo attachmentInfo)
        {
            base.Delete(attachmentInfo);
        }

        internal new DotNetNuke.Modules.ActiveForums.Entities.AttachmentInfo Insert(DotNetNuke.Modules.ActiveForums.Entities.AttachmentInfo attachmentInfo)
        {
            base.Insert(attachmentInfo);
            return this.GetById(attachmentInfo.AttachmentId);
        }

        internal new DotNetNuke.Modules.ActiveForums.Entities.AttachmentInfo Update(DotNetNuke.Modules.ActiveForums.Entities.AttachmentInfo attachmentInfo)
        {
            base.Update(attachmentInfo);
            return this.GetById(attachmentInfo.AttachmentId);
        }

        internal new DotNetNuke.Modules.ActiveForums.Entities.AttachmentInfo Save(DotNetNuke.Modules.ActiveForums.Entities.AttachmentInfo attachmentInfo)
        {
            this.Save(attachmentInfo, attachmentInfo.AttachmentId);
            return this.GetById(attachmentInfo.AttachmentId);
        }

        internal void RelocateAttachment(DotNetNuke.Modules.ActiveForums.Entities.AttachmentInfo attachment)
        {
            if (attachment == null)
            {
                return;
            }

            DotNetNuke.Services.FileSystem.IFileInfo file = null;

            var content = this.contentController.GetById(attachment.ContentId, DotNetNuke.Common.Utilities.Null.NullInteger);
            if (content != null)
            {
                if (attachment.DisplayInline == true)
                {
                    this.RelocateInlineAttachment(attachment, content);
                    return;
                }

                var attachmentFolder =
                        this.folderManager.GetFolder(content.Post.PortalId, string.Format(Globals.AttachmentsFolderNameFormatString, content.ModuleId, content.ContentId)) ??
                        this.folderManager.AddFolder(content.Post.PortalId, string.Format(Globals.AttachmentsFolderNameFormatString, content.ModuleId, content.ContentId));

                if (attachment.FileId.HasValue && attachment.FileId > 0)
                {
                    file = this.fileManager.GetFile((int)attachment.FileId);

                    /* if the file is not set as an inline attachment, check if the content contains the original URL for the file.
                     * If it does, set the attachment to be an inline attachment and move it to the embedded images folder.
                     * This is necessary to ensure that existing content with inline attachments continues to work after we start storing inline attachments in a separate folder.
                     */

                    var originalUrl = this.fileManager.GetUrl(file);
                    if (content.Body.ToLowerInvariant().Contains(originalUrl.ToLowerInvariant()))
                    {
                        if (!attachment.DisplayInline)
                        {
                            attachment.DisplayInline = true;
                            this.Update(attachment);
                        }

                        this.RelocateInlineAttachment(attachment, content);
                        return;
                    }

                    // if file uploaded by a host/superuser and portalid is not set, set the portalid to the content's portalid before moving the file
                    if (file.PortalId != content.Post.PortalId)
                    {
                        file.PortalId = content.Post.PortalId;
                        file = this.fileManager.UpdateFile(file);
                    }

                    file = this.fileManager.MoveFile(file, attachmentFolder);
                }
                else
                {
                    var attachmentsPath = Utilities.MapPath(content.Post.Forum.PortalSettings.HomeDirectory + $"{Globals.LegacyAttachmentsFolderName}/");
                    if (!System.IO.File.Exists(attachmentsPath + attachment.FileName))
                    {
                        // If the file doesn't exist in the legacy attachment location, attempt to find it in the user's folder (legacy location for user-uploaded attachments)
                        var userInfo = UserController.GetUserById(content.Post.PortalId, attachment.UserId);
                        var userFolder = this.folderManager.GetUserFolder(userInfo);
                        if (this.fileManager.FileExists(userFolder, attachment.FileName))
                        {
                            attachmentsPath = userFolder.PhysicalPath;
                        }
                    }

                    if (!System.IO.File.Exists(attachmentsPath + attachment.FileName))
                    {
                        return;
                    }

                    byte[] fileBytes = System.IO.File.ReadAllBytes(Utilities.MapPath(attachmentsPath + attachment.FileName));
                    using (var ms = new MemoryStream())
                    {
                        ms.Write(fileBytes, 0, fileBytes.Length);
                        ms.Position = 0;

                        attachment.FileName = DotNetNuke.Modules.ActiveForums.Controllers.AttachmentController.CreateUniqueFileName(attachmentFolder.PhysicalPath, attachment.FileName);
                        file = this.fileManager.AddFile(attachmentFolder, attachment.FileName, ms, true);
                    }

                    attachment.FileId = file.FileId;
                }

                attachment.FileSize = file.Size;
                this.Update(attachment);
            }
        }

        internal void RelocateInlineAttachment(DotNetNuke.Modules.ActiveForums.Entities.AttachmentInfo attachment, DotNetNuke.Modules.ActiveForums.Entities.ContentInfo content)
        {
            if (attachment == null || attachment.DisplayInline == false)
            {
                return;
            }

            DotNetNuke.Services.FileSystem.IFileInfo file = null;

            var originalUrl = string.Empty;

            if (content != null)
            {
                var embeddedImagesFolder =
                        this.folderManager.GetFolder(content.Post.PortalId, string.Format(Globals.EmbeddedImagesFolderNameFormatString, content.ModuleId, content.ContentId)) ??
                        this.folderManager.AddFolder(content.Post.PortalId, string.Format(Globals.EmbeddedImagesFolderNameFormatString, content.ModuleId, content.ContentId));

                if (attachment.FileId.HasValue && attachment.FileId > 0)
                {
                    file = this.fileManager.GetFile((int)attachment.FileId);
                    originalUrl = this.fileManager.GetUrl(file);

                    // if file uploaded by a host/superuser and portalid is not set, set the portalid to the content's portalid before moving the file
                    if (file.PortalId != content.Post.PortalId)
                    {
                        file.PortalId = content.Post.PortalId;
                        file = this.fileManager.UpdateFile(file);
                    }

                    file = this.fileManager.MoveFile(file, embeddedImagesFolder);
                }
                else
                {
                    var attachmentsPath = Utilities.MapPath(content.Post.Forum.PortalSettings.HomeDirectory + $"{Globals.LegacyAttachmentsFolderName}/");
                    if (!System.IO.File.Exists(attachmentsPath + attachment.FileName))
                    {
                        // If the file doesn't exist in the legacy attachment location, attempt to find it in the user's folder (legacy location for user-uploaded attachments)
                        var userInfo = DotNetNuke.Entities.Users.UserController.Instance.GetUserById(content.Post.PortalId, attachment.UserId);
                        var userFolder = this.folderManager.GetUserFolder(userInfo);
                        if (this.fileManager.FileExists(userFolder, attachment.FileName))
                        {
                            attachmentsPath = userFolder.PhysicalPath;
                        }
                    }

                    if (!System.IO.File.Exists(attachmentsPath + attachment.FileName))
                    {
                        return;
                    }

                    using (var imageFile = System.Drawing.Image.FromFile(attachmentsPath + attachment.FileName))
                    {
                        using (var ms = new MemoryStream())
                        {
                            // pick save format by comparing RawFormat GUIDs
                            var raw = imageFile.RawFormat;
                            var saveFormat = System.Drawing.Imaging.ImageFormat.Png;

                            if (System.Drawing.Imaging.ImageFormat.Jpeg.Guid == raw.Guid)
                            {
                                saveFormat = System.Drawing.Imaging.ImageFormat.Jpeg;
                            }
                            else if (System.Drawing.Imaging.ImageFormat.Gif.Guid == raw.Guid)
                            {
                                saveFormat = System.Drawing.Imaging.ImageFormat.Gif;
                            }
                            else if (System.Drawing.Imaging.ImageFormat.Bmp.Guid == raw.Guid)
                            {
                                saveFormat = System.Drawing.Imaging.ImageFormat.Bmp;
                            }

                            imageFile.Save(ms, saveFormat);
                            ms.Position = 0;

                            attachment.FileName = DotNetNuke.Modules.ActiveForums.Controllers.AttachmentController.CreateUniqueFileName(embeddedImagesFolder.PhysicalPath, attachment.FileName);
                            file = this.fileManager.AddFile(embeddedImagesFolder, attachment.FileName, ms, true);
                            attachment.FileId = file.FileId;
                        }

                        System.IO.File.Delete(attachmentsPath + attachment.FileName);
                    }
                }

                attachment.ContentType = file.ContentType;
                attachment.FileSize = file.Size;
                this.Update(attachment);

                var width = file.Width;
                var height = file.Height;
                if (width > content.Post.Forum.FeatureSettings.MaxImageWidth ||
                    height > content.Post.Forum.FeatureSettings.MaxImageHeight)
                {
                    if (width > 0 && height > 0)
                    {
                        var widthRatio = (double)content.Post.Forum.FeatureSettings.MaxImageWidth / width;
                        var heightRatio = (double)content.Post.Forum.FeatureSettings.MaxImageHeight / height;
                        var ratio = Math.Min(widthRatio, heightRatio);
                        width = (int)(width * ratio);
                        height = (int)(height * ratio);
                    }
                    else
                    {
                        width = content.Post.Forum.FeatureSettings.MaxImageWidth;
                        height = content.Post.Forum.FeatureSettings.MaxImageHeight;
                    }
                }

                const string ImgSrcPattern = @"(?<tag><img.*?src=""(?<src>[^>""].*?)"".*?>)";
                var matches = RegexUtils.GetCachedRegex(ImgSrcPattern, RegexOptions.Compiled & RegexOptions.IgnoreCase).Matches(content.Body);
                foreach (Match match in matches)
                {
                    if (match.Groups["tag"].Success && (match.Groups["tag"].Value.EndsWith(file.FileName, StringComparison.InvariantCultureIgnoreCase) || (!string.IsNullOrEmpty(originalUrl) && match.Groups["tag"].Value.ToLowerInvariant().Contains(originalUrl.ToLowerInvariant()))))
                    {
                        var tag = Utilities.ResolveUrlInTag($"<img src=\"https://{content.Post.Forum.PortalSettings.DefaultPortalAlias}{this.fileManager.GetUrl(file)}\" width=\"{width}\" height=\"{height}\" loading=\"lazy\" />", content.Post.Forum.PortalSettings.DefaultPortalAlias, content.Post.Forum.PortalSettings.SSLEnabled);
                        content.Body = content.Body.Replace(match.Groups["tag"].Value, tag);
                        this.contentController.Save(content, content.ContentId);
                    }
                }
            }
        }

        internal static string CreateUniqueFileName(string folder, string fileName)
        {
            var index = 0;
            var baseName = System.IO.Path.GetFileNameWithoutExtension(fileName);
            var extension = System.IO.Path.GetExtension(fileName).ToLowerInvariant();
            fileName = baseName + extension;

            while (System.IO.File.Exists(System.IO.Path.Combine(folder, fileName)))
            {
                index++;
                fileName = string.Format(Globals.AttachmentFileNameFormatString, baseName, index, extension);
            }

            return fileName;
        }
    }
}
