﻿// Copyright (c) by DNN Community
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
    using System.Diagnostics;
    using System.IO;
    using System.Text.RegularExpressions;

    using DotNetNuke.Modules.ActiveForums.Extensions;
    using DotNetNuke.Services.FileSystem;

    public class af_viewer : Framework.PageBase
    {
        #region  Web Form Designer Generated Code

        // This call is required by the Web Form Designer.
        [DebuggerStepThrough]
        private void InitializeComponent()
        {
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // CODEGEN: This method call is required by the Web Form Designer
            // Do not modify it using the code editor.
            this.InitializeComponent();
        }

        #endregion

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            var attachmentId = Utilities.SafeConvertInt(this.Request.Params["AttachmentID"], -1);// Used for new attachments where the attachment is the actual file link (shouldn't appear in posts)
            var attachFileId = Utilities.SafeConvertInt(this.Request.Params["AttachID"], -1); // Used for legacy attachments where the attachid was actually the file id. (appears in posts)
            var portalId = Utilities.SafeConvertInt(this.Request.Params["PortalID"], -1);
            var moduleId = Utilities.SafeConvertInt(this.Request.Params["ModuleID"], -1);

            if (this.Page.IsPostBack || (attachmentId < 0 && attachFileId < 0) || portalId < 0 || moduleId < 0)
            {
                this.Response.StatusCode = 400;
                this.Response.Write("Invalid Request");
                this.Response.End();
                return;
            }

            // Get the attachment including the "Can Read" permission for the associated content id.
            var attachment = new Data.AttachController().Get(attachmentId, attachFileId, true);

            // Make sure the attachment exists
            if (attachment == null)
            {
                this.Response.StatusCode = 404;
                this.Response.Write("Not Found");
                this.Response.End();
                return;
            }

            // Make sure the user has read access
            DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo u = new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(moduleId).GetUserFromHttpContext(portalId, moduleId);
            if (u == null || !DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasAccess(attachment.CanRead, u.UserPermSet))
            {
                this.Response.StatusCode = 401;
                this.Response.Write("Unauthorized");
                this.Response.End();
                return;
            }

            // Get the filename with the unique identifier prefix removed.
            var filename = Regex.Replace(attachment.FileName.TextOrEmpty(), @"__\d+__\d+__", string.Empty);

            // Some legacy attachments may still be stored in the DB.
            if (attachment.FileData != null)
            {
                this.Response.ContentType = attachment.ContentType;

                if (attachmentId > 0)
                {
                    this.Response.AddHeader("Content-Disposition", "attachment; filename=" + this.Server.HtmlEncode(filename));
                }
                else // Handle legacy inline attachments a bit differently
                {
                    this.Response.AddHeader("Content-Disposition", "filename=" + this.Server.HtmlEncode(filename));
                }

                this.Response.BinaryWrite(attachment.FileData);
                this.Response.End();
                return;
            }

            var fileManager = FileManager.Instance;

            string filePath = null;

            // If there is a file id, access the file using the file manager
            if (attachment.FileId.HasValue && attachment.FileId.Value > 0)
            {
                var file = fileManager.GetFile(attachment.FileId.Value);
                if (file != null)
                {
                    filePath = file.PhysicalPath;
                }
            }

            // Otherwise check the attachments directory (current and legacy)
            else
            {
                filePath = Utilities.MapPath(this.PortalSettings.HomeDirectory + "activeforums_Attach/") + attachment.FileName;

                // This is another check to support legacy attachments.
                if (!File.Exists(filePath))
                {
                    filePath = Utilities.MapPath(this.PortalSettings.HomeDirectory + "NTForums_Attach/") + attachment.FileName;
                }
            }

            // At this point, we should have a valid file path
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                this.Response.StatusCode = 404;
                this.Response.Write("Not Found");
                this.Response.End();
                return;
            }

            var length = attachment.FileSize;
            if (length <= 0)
            {
                length = new System.IO.FileInfo(filePath).Length;
            }

            this.Response.Clear();
            this.Response.ContentType = attachment.ContentType;

            if (attachmentId > 0)
            {
                this.Response.AddHeader("Content-Disposition", "attachment; filename=" + this.Server.HtmlEncode(filename));
            }
            else // Handle legacy inline attachments a bit differently
            {
                this.Response.AddHeader("Content-Disposition", "filename=" + this.Server.HtmlEncode(filename));
            }

            this.Response.AddHeader("Content-Length", length.ToString());
            this.Response.WriteFile(filePath);
            this.Response.Flush();
            this.Response.Close();
            this.Response.End();
        }
    }
}
