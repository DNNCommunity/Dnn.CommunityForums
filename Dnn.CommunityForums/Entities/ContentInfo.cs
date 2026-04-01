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

namespace DotNetNuke.Modules.ActiveForums.Entities
{
    using System;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Security.Policy;
    using System.Text.RegularExpressions;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel.DataAnnotations;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Modules.ActiveForums.ViewModels;
    using DotNetNuke.Services.FileSystem;

    [TableName("activeforums_Content")]
    [PrimaryKey("ContentId", AutoIncrement = true)]
    public class ContentInfo
    {
        [IgnoreColumn] private string cacheKeyTemplate => CacheKeys.ContentInfo;

        private DotNetNuke.Modules.ActiveForums.Entities.IPostInfo postInfo;

        public int ContentId { get; set; }

        public string Subject { get; set; } = string.Empty;

        public string Summary { get; set; } = string.Empty;

        public string Body { get; set; } = string.Empty;

        public DateTime DateCreated { get; set; } /* TODO: Once Reply_Save etc. moved from stored procedures to DAL2 for saving, update this to auto-set dates */

        public DateTime DateUpdated { get; set; } = DateTime.UtcNow; /* TODO: Once Reply_Save etc. moved from stored procedures to DAL2 for saving, update this to auto-set dates */

        public int AuthorId { get; set; }

        public string AuthorName { get; set; }

        public bool IsDeleted { get; set; }

        public string IPAddress { get; set; }

        public int ContentItemId { get; set; }

        public int ModuleId { get; set; }

        [IgnoreColumn]
        public DotNetNuke.Modules.ActiveForums.Entities.IPostInfo Post
        {
            get
            {
                if (this.postInfo == null)
                {
                    this.postInfo = this.GetPost();
                    this.UpdateCache();
                }

                return this.postInfo;
            }
            set => this.postInfo = value;
        }

        internal DotNetNuke.Modules.ActiveForums.Entities.IPostInfo GetPost()
        {
            if (this.postInfo == null)
            {
                this.postInfo = (DotNetNuke.Modules.ActiveForums.Entities.IPostInfo)new DotNetNuke.Modules.ActiveForums.Controllers.TopicController(this.ModuleId).GetByContentId(this.ContentId);
                if (this.postInfo == null)
                {
                    this.postInfo = new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController(this.ModuleId).GetByContentId(this.ContentId);
                }
            }

            return this.postInfo;
        }

        internal string GetCacheKey() => string.Format(this.cacheKeyTemplate, this.ModuleId, this.ContentId);

        internal void UpdateCache() => DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheStore(this.ModuleId, this.GetCacheKey(), this);

        internal void ExtractEmbeddedImages()
        {
            var embeddedImagesFolder = DotNetNuke.Services.FileSystem.FolderManager.Instance.GetFolder(this.Post.PortalId, string.Format(Globals.EmbeddedImagesFolderNameFormatString, this.ModuleId, this.ContentId)) ?? DotNetNuke.Services.FileSystem.FolderManager.Instance.AddFolder(this.Post.PortalId, string.Format(Globals.EmbeddedImagesFolderNameFormatString, this.ModuleId, this.ContentId));
            _ = DotNetNuke.Services.FileSystem.FolderManager.Instance.GetFolder(this.Post.PortalId, string.Format(Globals.ContentFolderNameFormatString, this.ModuleId, this.ContentId)) ?? DotNetNuke.Services.FileSystem.FolderManager.Instance.AddFolder(this.Post.PortalId, string.Format(Globals.ContentFolderNameFormatString, this.ModuleId, this.ContentId));
            try
            {
                const string Base64ImagePattern = @"(?<tag><img src=""(?<src>data:(?<mimetype>image\/[a-zA-Z]+);base64,(?<content>[A-Za-z0-9+\/]+=*)"")[^>]*>)";
                var matches = RegexUtils.GetCachedRegex(Base64ImagePattern, RegexOptions.Compiled & RegexOptions.IgnoreCase).Matches(this.Body);
                foreach (Match match in matches)
                {
                    if (match.Groups["content"].Success)
                    {
                        string mimeType = match.Groups["mimetype"].Value;
                        string base64Data = match.Groups["content"].Value;
                        byte[] imageBytes = Convert.FromBase64String(base64Data);
                        using (var ms = new MemoryStream())
                        {
                            ms.Write(imageBytes, 0, imageBytes.Length);
                            System.Drawing.Image image = System.Drawing.Image.FromStream(ms);
                            var fileType = string.Empty;
                            switch (mimeType.ToLowerInvariant())
                            {
                                case "image/jpeg":
                                case "image/jpg":
                                    fileType = System.Drawing.Imaging.ImageFormat.Jpeg.ToString();
                                    image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                                    break;
                                case "image/gif":
                                    fileType = System.Drawing.Imaging.ImageFormat.Gif.ToString();
                                    image.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
                                    break;
                                case "image/png":
                                case "image/x-png":
                                    fileType = System.Drawing.Imaging.ImageFormat.Png.ToString();
                                    image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                                    break;
                                default:
                                    // Unsupported image type
                                    return;
                            }

                            var fileName = $"embedded_{this.ContentId}-{DateTime.UtcNow:yyyyMMddHHmmssffffff}.{fileType.ToLowerInvariant()}";
                            var file = DotNetNuke.Services.FileSystem.FileManager.Instance.AddFile(embeddedImagesFolder, fileName, ms);

                            var attachment = new DotNetNuke.Modules.ActiveForums.Entities.AttachmentInfo
                            {
                                ContentId = this.ContentId,
                                UserId = this.AuthorId,
                                FileId = file.FileId,
                                FileName = file.FileName,
                                ContentType = file.ContentType,
                                FileSize = file.Size,
                                DateAdded = DateTime.UtcNow,
                                DateUpdated = DateTime.UtcNow,
                                DisplayInline = true,
                            };
                            new DotNetNuke.Modules.ActiveForums.Controllers.AttachmentController().Save(attachment);
                            var width = file.Width;
                            var height = file.Height;
                            if (width > this.Post.Forum.FeatureSettings.MaxImageWidth ||
                                height > this.Post.Forum.FeatureSettings.MaxImageHeight)
                            {
                                if (width > 0 && height > 0)
                                {
                                    var widthRatio = (double)this.Post.Forum.FeatureSettings.MaxImageWidth / width;
                                    var heightRatio = (double)this.Post.Forum.FeatureSettings.MaxImageHeight / height;
                                    var ratio = Math.Min(widthRatio, heightRatio);
                                    width = (int)(width * ratio);
                                    height = (int)(height * ratio);
                                }
                                else
                                {
                                    width = this.Post.Forum.FeatureSettings.MaxImageWidth;
                                    height = this.Post.Forum.FeatureSettings.MaxImageHeight;
                                }
                            }

                            var tag = Utilities.ResolveUrlInTag($"<img src=\"https://{this.Post.Forum.PortalSettings.DefaultPortalAlias}{DotNetNuke.Services.FileSystem.FileManager.Instance.GetUrl(file)}\" width=\"{width}\" height=\"{height}\" loading=\"lazy\" />", this.Post.Forum.PortalSettings.DefaultPortalAlias, this.Post.Forum.PortalSettings.SSLEnabled);
                            this.Body = this.Body.Replace(match.Groups["tag"].Value, tag);
                            new DotNetNuke.Modules.ActiveForums.Controllers.ContentController().Save(this, this.ContentId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Modules.ActiveForums.Exceptions.LogException(ex);
            }
        }

        internal void RemoveAttachments()
        {
            var attachmentController = new DotNetNuke.Modules.ActiveForums.Controllers.AttachmentController();
            var fileManager = DotNetNuke.Services.FileSystem.FileManager.Instance;
            var folderManager = DotNetNuke.Services.FileSystem.FolderManager.Instance;
            var attachmentFolder = folderManager.GetFolder(this.Post.PortalId, string.Format(DotNetNuke.Modules.ActiveForums.Globals.AttachmentsFolderNameFormatString, this.ModuleId, this.ContentId));
            var embeddedImagesFolder = folderManager.GetFolder(this.Post.PortalId, string.Format(Globals.EmbeddedImagesFolderNameFormatString, this.ModuleId, this.ContentId));
            var legacyAttachmentFolder = folderManager.GetFolder(this.Post.PortalId, DotNetNuke.Modules.ActiveForums.Globals.LegacyAttachmentsFolderName);

            foreach (var attachment in attachmentController.GetByContentId(this.ContentId))
            {
                attachmentController.DeleteById(attachment.AttachmentId);

                var file = attachment.FileId.HasValue && attachment.FileId.Value > 0 ? fileManager.GetFile(attachment.FileId.Value) : fileManager.GetFile(attachmentFolder, attachment.FileName);
                if (file != null && (
                            (attachmentFolder != null && file.FolderId == attachmentFolder?.FolderID) ||
                            (legacyAttachmentFolder != null && file.FolderId == legacyAttachmentFolder.FolderID) ||
                            (embeddedImagesFolder != null && file.FolderId == embeddedImagesFolder.FolderID)))
                {
                    fileManager.DeleteFile(file);
                }
            }

            if (embeddedImagesFolder != null && !folderManager.GetFiles(embeddedImagesFolder).Any())
            {
                folderManager.DeleteFolder(embeddedImagesFolder);
            }

            if (attachmentFolder != null && !folderManager.GetFiles(attachmentFolder).Any())
            {
                folderManager.DeleteFolder(attachmentFolder);
            }

            var contentFolder = folderManager.GetFolder(this.Post.PortalId, string.Format(DotNetNuke.Modules.ActiveForums.Globals.ContentFolderNameFormatString, this.ModuleId, this.ContentId));
            if (contentFolder != null && !folderManager.GetFiles(contentFolder).Any())
            {
                folderManager.DeleteFolder(contentFolder);
            }
        }
    }
}
