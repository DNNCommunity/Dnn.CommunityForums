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
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Collections;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Social.Subscriptions.Entities;
    using DotNetNuke.UI.UserControls;
    using DotNetNuke.UI.WebControls;

    public partial class profile_mysubscriptions : ForumBase
    {
        private int UID { get; set; }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.dgrdTopicSubs.Columns[3].HeaderText = DotNetNuke.Modules.ActiveForums.Utilities.GetSharedResource("[RESX:Group].Text");
            this.dgrdTopicSubs.Columns[4].HeaderText = DotNetNuke.Modules.ActiveForums.Utilities.GetSharedResource("[RESX:Forum].Text");
            this.dgrdTopicSubs.Columns[5].HeaderText = DotNetNuke.Modules.ActiveForums.Utilities.GetSharedResource("[RESX:Topic].Text");
            this.dgrdTopicSubs.Columns[6].HeaderText = DotNetNuke.Modules.ActiveForums.Utilities.GetSharedResource("[RESX:LastPost].Text");
            this.dgrdForumSubs.Columns[3].HeaderText = DotNetNuke.Modules.ActiveForums.Utilities.GetSharedResource("[RESX:Group].Text");
            this.dgrdForumSubs.Columns[4].HeaderText = DotNetNuke.Modules.ActiveForums.Utilities.GetSharedResource("[RESX:Forum].Text");
            this.dgrdForumSubs.Columns[5].HeaderText = DotNetNuke.Modules.ActiveForums.Utilities.GetSharedResource("[RESX:LastPost].Text");

            this.dgrdTopicSubs.PageIndexChanging += this.TopicSubsGridRow_PageIndexChanging;
            this.dgrdTopicSubs.RowDataBound += this.OnTopicSubsGridRowDataBound;
            this.dgrdForumSubs.PageIndexChanging += this.ForumSubsGridRow_PageIndexChanging;
            this.dgrdForumSubs.RowDataBound += this.OnForumSubsGridRowDataBound;
            this.btnSubscribeAll.Click += this.btnSubscribeAll_Click;
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

                this.dgrdForumSubs.PageSize = _pageSize;
                this.dgrdTopicSubs.PageSize = _pageSize;

                // TODO: Add moderator functionality to edit a user's subscriptions; this currently is just for a user to edit own subscriptions
                this.UID = this.Request.QueryString[Literals.UserId] != null ? Convert.ToInt32(this.Request.QueryString[Literals.UserId]) : this.UserInfo.UserID;
                if (this.UserId == this.UID | this.ForumUser.GetIsMod(this.ForumModuleId))
                {
                    this.BindTopicSubs();
                    this.BindForumSubs();
                }
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }
        }

        private void BindTopicSubs()
        {
            var subscribedTopics = new DotNetNuke.Modules.ActiveForums.Controllers.SubscriptionController().SubscribedTopics(this.PortalId, this.ForumModuleId, this.UID).OrderBy(s => s.Forum.ForumGroup.SortOrder).ThenBy(s => s.Forum.SortOrder);
            subscribedTopics.ForEach(s =>
            {
                s.Subscribed = true;
            });
            this.dgrdTopicSubs.DataSource = subscribedTopics.ToList();
            this.dgrdTopicSubs.DataBind();
        }

        private void BindForumSubs()
        {
            this.dgrdForumSubs.DataSource = this.GetSubscriptions().ToList();
            this.dgrdForumSubs.DataBind();
        }

        private void btnSubscribeAll_Click(object sender, System.EventArgs e)
        {
            this.GetSubscriptions().Where(s => !s.Subscribed).ForEach(s =>
            {
                new DotNetNuke.Modules.ActiveForums.Controllers.SubscriptionController().Subscribe(s.PortalId, s.ModuleId, this.UID, s.ForumId);
            });
            this.BindForumSubs();
        }

        private IEnumerable<Entities.SubscriptionInfo> GetSubscriptions()
        {
            var userPermSet = new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ForumModuleId).GetByUserId(this.PortalId, this.UID).UserPermSet;
            var availableForumsString = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForumsForUser(this.PortalId, this.ForumModuleId, this.ForumUser);
            var availableForums = availableForumsString.Split(separator: new[] { ';' }, options: StringSplitOptions.RemoveEmptyEntries).Select(forum =>
            {
                var Forum = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(int.Parse(forum), this.ForumModuleId);
                return new { ForumId = int.Parse(forum), Forum };
            });

            var forumSubscriptions = new DotNetNuke.Modules.ActiveForums.Controllers.SubscriptionController().SubscribedForums(this.PortalId, this.ForumModuleId, this.UID);
            var subscribedForums = forumSubscriptions.Select(forumSubscription =>
            {
                return new { forumSubscription.Id, forumSubscription.ForumId };
            });
            var mergedSubscriptions = from af in availableForums
                                      join sf in subscribedForums
                                      on af.ForumId equals sf.ForumId into merged
                                      from ms in merged.DefaultIfEmpty()
                                      where af.Forum.FeatureSettings.AllowSubscribe || ms == null || ms.Id != 0
                                      select new DotNetNuke.Modules.ActiveForums.Entities.SubscriptionInfo(ms?.Id ?? 0, this.PortalId, this.ForumModuleId, af.ForumId, 0, 1, this.UID, af.Forum.GroupName, af.Forum.ForumName, string.Empty, af.Forum.LastPostDateTime, ms != null);
            return mergedSubscriptions;
        }

        protected void OnForumSubsGridRowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                DotNetNuke.Modules.ActiveForums.Entities.SubscriptionInfo subscriptionInfo = e.Row.DataItem as DotNetNuke.Modules.ActiveForums.Entities.SubscriptionInfo;
                foreach (TableCell cell in e.Row.Cells)
                {
                    foreach (Control cellControl in cell.Controls)
                    {
                        if (cellControl is CheckBox)
                        {
                            var chkBox = cellControl as CheckBox;
                            if (!(chkBox == null))
                            {
                                chkBox.Attributes.Add("onclick", "amaf_forumSubscribe(" + this.ForumModuleId + "," + subscriptionInfo.ForumId + ");");
                                chkBox.Enabled = true;
                            }
                        }
                    }
                }
            }
        }

        protected void OnTopicSubsGridRowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                DotNetNuke.Modules.ActiveForums.Entities.SubscriptionInfo subscriptionInfo = e.Row.DataItem as DotNetNuke.Modules.ActiveForums.Entities.SubscriptionInfo;
                foreach (TableCell cell in e.Row.Cells)
                {
                    foreach (Control cellControl in cell.Controls)
                    {
                        if (cellControl is CheckBox)
                        {
                            var chkBox = cellControl as CheckBox;
                            if (!(chkBox == null))
                            {
                                chkBox.Attributes.Add("onclick", "amaf_topicSubscribe(" + this.ForumModuleId + "," + subscriptionInfo.ForumId + "," + subscriptionInfo.TopicId + ");");
                                chkBox.Enabled = true;
                            }
                        }
                    }
                }
            }
        }

        protected void ForumSubsGridRow_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            this.dgrdForumSubs.PageIndex = e.NewPageIndex;
            this.dgrdForumSubs.DataBind();
        }

        protected void TopicSubsGridRow_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            this.dgrdTopicSubs.PageIndex = e.NewPageIndex;
            this.dgrdTopicSubs.DataBind();
        }
    }
}
