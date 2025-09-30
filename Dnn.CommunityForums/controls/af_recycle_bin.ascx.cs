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
    using System.Linq;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Collections;
    using DotNetNuke.Modules.ActiveForums.Enums;
    using DotNetNuke.Modules.ActiveForums.Extensions.WebForms;

    public partial class af_recycle_bin : ForumBase
    {
        protected global::System.Web.UI.WebControls.GridView dgrdRestoreView;
        protected global::System.Web.UI.WebControls.Button btnRestoreAll;
        protected global::System.Web.UI.WebControls.Button btnEmptyRecycleBin;

        class RecycleBinData
        {
            public int PortalId { get; set; }

            public int ForumId { get; set; }

            public int TopicId { get; set; }

            public int ReplyId { get; set; }

            public bool IsReply { get; set; }

            public bool IsTopic { get; set; }

            public string Subject { get; set; }

            public string ForumName { get; set; }

            public string AuthorName { get; set; }

            public DateTime DateCreated { get; set; }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.dgrdRestoreView.Columns[3].HeaderText = DotNetNuke.Modules.ActiveForums.Utilities.GetSharedResource("[RESX:Reply]");
            this.dgrdRestoreView.Columns[4].HeaderText = DotNetNuke.Modules.ActiveForums.Utilities.GetSharedResource("[RESX:Forum]");
            this.dgrdRestoreView.Columns[5].HeaderText = DotNetNuke.Modules.ActiveForums.Utilities.GetSharedResource("[RESX:Subject]");
            this.dgrdRestoreView.Columns[6].HeaderText = DotNetNuke.Modules.ActiveForums.Utilities.GetSharedResource("[RESX:Author]");
            this.dgrdRestoreView.Columns[7].HeaderText = DotNetNuke.Modules.ActiveForums.Utilities.GetSharedResource("[RESX:DateCreated]");

            //this.dgrdRestoreView.PageIndexChanging += this.RestoreViewGridRowPageIndexChanging;
            this.dgrdRestoreView.RowCommand += this.RestoreViewGrid_OnRowCommand;
            this.dgrdRestoreView.RowDataBound += this.OnRestoreViewGridRowDataBound;
            this.btnEmptyRecycleBin.Click += this.btnEmptyRecycleBin_Click;
            this.btnRestoreAll.Click += this.btnRestoreAll_Click;
        }

        protected void btnEmptyRecycleBin_Click(object sender, System.EventArgs e)
        {
            var topicController = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController(this.ForumModuleId);
            var replyController = new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController(this.ForumModuleId);

            // process replies first, then topics
            var recycleData = this.GetData();
            recycleData.Where(content => content.IsReply).ForEach(content =>
            {
                replyController.Reply_Delete(this.PortalId, content.ForumId, content.TopicId, content.ReplyId, DeleteBehavior.Remove);
            });
            recycleData.Where(content => content.IsTopic).ForEach(content =>
            {
                topicController.DeleteById(content.TopicId, DeleteBehavior.Remove);
            });
            this.BindRecycleData();
        }

        protected void btnRestoreAll_Click(object sender, System.EventArgs e)
        {
            var topicController = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController(this.ForumModuleId);
            var replyController = new DotNetNuke.Modules.ActiveForums.Controllers.ReplyController(this.ForumModuleId);

            // process topics first, then replies
            var recycleData = this.GetData();
            recycleData.Where(content => content.IsTopic).ForEach(content =>
            {
                topicController.Restore(content.PortalId, content.ForumId, content.TopicId);
            });
            recycleData.Where(content => content.IsReply).ForEach(content =>
            {
                replyController.Restore(this.PortalId, content.ForumId, content.TopicId, content.ReplyId);
            });
            this.BindRecycleData();
        }

        protected void RestoreViewGrid_OnRowCommand(object source, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "Restore")
            {
                this.BindRecycleData();
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {
                int _pageSize = this.MainSettings.PageSize;
                if (this.UserInfo.UserID > 0)
                {
                    _pageSize = this.UserDefaultPageSize;
                }

                if (_pageSize < 5)
                {
                    _pageSize = 10;
                }


                if (this.UserId > 0 && this.ForumUser.GetIsMod(this.ForumModuleId))
                {
                    this.BindRecycleData();
                }
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }
        }

        private void BindRecycleData()
        {
            var recycleData = this.GetData().ToList();
            this.dgrdRestoreView.DataSource = recycleData;
            this.dgrdRestoreView.DataBind();
            this.dgrdRestoreView.PageSize = this.dgrdRestoreView.Rows.Count;
            this.btnEmptyRecycleBin.Enabled = recycleData.Any();
            this.btnRestoreAll.Enabled = recycleData.Any();
            this.dgrdRestoreView.WrapGridViewInDataTableNet(this.PortalSettings, this.UserInfo);
        }

        private IEnumerable<RecycleBinData> GetData()
        {
            var restoreData = new DotNetNuke.Modules.ActiveForums.Controllers.ContentController().Find("WHERE IsDeleted = 1 AND ModuleId = @0", this.ForumModuleId);
            return restoreData.OrderByDescending(content => content.DateUpdated).Select(content =>
            {
                return new RecycleBinData
                {
                    TopicId = content.Post.TopicId,
                    ReplyId = content.Post.ReplyId,
                    IsTopic = content.Post.IsTopic,
                    IsReply = content.Post.IsReply,
                    Subject = content.Subject,
                    ForumName = content.Post.Forum.ForumName,
                    ForumId = content.Post.ForumId,
                    PortalId = content.Post.PortalId,
                    AuthorName = content.AuthorName,
                    DateCreated = content.DateCreated,
                };
            });
        }

        protected void OnRestoreViewGridRowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                var restoreData = e.Row.DataItem as RecycleBinData;
                foreach (TableCell cell in e.Row.Cells)
                {
                    foreach (Control cellControl in cell.Controls)
                    {
                        if (cellControl is Button)
                        {
                            var restoreButton = cellControl as Button;
                            restoreButton.Text = DotNetNuke.Modules.ActiveForums.Utilities.GetSharedResource("[RESX:Restore]");
                            if (!(restoreButton == null))
                            {
                                restoreButton.Enabled = true;
                                if (restoreData.IsReply)
                                {
                                    restoreButton.Attributes.Add("onclick", $"amaf_replyRestore({this.ForumModuleId},{restoreData.ForumId},{restoreData.TopicId},{restoreData.ReplyId});return RemoveRow(this);");
                                }
                                else
                                {
                                    restoreButton.Attributes.Add("onclick", $"amaf_topicRestore({this.ForumModuleId},{restoreData.ForumId},{restoreData.TopicId});return RemoveRow(this);");
                                }
                            }
                        }
                    }
                }
            }
        }

        //protected void RestoreViewGridRowPageIndexChanging(object sender, GridViewPageEventArgs e)
        //{
        //    this.dgrdRestoreView.PageIndex = e.NewPageIndex;
        //    this.dgrdRestoreView.DataBind();
        //}
    }
}
