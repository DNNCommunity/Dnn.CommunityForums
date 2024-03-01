//
// Community Forums
// Copyright (c) 2013-2021
// by DNN Community
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
//
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

namespace DotNetNuke.Modules.ActiveForums
{
    public partial class profile_mysubscriptions : ForumBase
    {
        private int UID { get; set; }
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            dgrdTopicSubs.Columns[3].HeaderText = DotNetNuke.Modules.ActiveForums.Utilities.GetSharedResource("[RESX:Group].Text");
            dgrdTopicSubs.Columns[4].HeaderText = DotNetNuke.Modules.ActiveForums.Utilities.GetSharedResource("[RESX:Forum].Text");
            dgrdTopicSubs.Columns[5].HeaderText = DotNetNuke.Modules.ActiveForums.Utilities.GetSharedResource("[RESX:Topic].Text");
            dgrdTopicSubs.Columns[6].HeaderText = DotNetNuke.Modules.ActiveForums.Utilities.GetSharedResource("[RESX:LastPost].Text");
            dgrdForumSubs.Columns[3].HeaderText = DotNetNuke.Modules.ActiveForums.Utilities.GetSharedResource("[RESX:Group].Text");
            dgrdForumSubs.Columns[4].HeaderText = DotNetNuke.Modules.ActiveForums.Utilities.GetSharedResource("[RESX:Forum].Text");
            dgrdForumSubs.Columns[5].HeaderText = DotNetNuke.Modules.ActiveForums.Utilities.GetSharedResource("[RESX:LastPost].Text");

            this.dgrdTopicSubs.PageIndexChanging += this.TopicSubsGridRow_PageIndexChanging;
            this.dgrdTopicSubs.RowDataBound += this.OnTopicSubsGridRowDataBound;
            this.dgrdForumSubs.PageIndexChanging += this.ForumSubsGridRow_PageIndexChanging;
            this.dgrdForumSubs.RowDataBound += this.OnForumSubsGridRowDataBound;
            this.btnSubscribeAll.Click += btnSubscribeAll_Click;
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {   // TODO: Add moderator functionality to edit a user's subscriptions; this currently is just for a user to edit own subscriptions
                UID = Request.QueryString["UserId"] == null ? UserInfo.UserID : Convert.ToInt32(Request.QueryString["UserId"]);
                if (this.UserId == UID | UserIsMod)
                {
                    BindTopicSubs();
                    BindForumSubs();
                }
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }
        }
        private void BindTopicSubs()
        {
            ForumController fc = new DotNetNuke.Modules.ActiveForums.ForumController();
            TopicsController tc = new DotNetNuke.Modules.ActiveForums.TopicsController();
            var subscribedTopics = new DotNetNuke.Modules.ActiveForums.Controllers.SubscriptionController().SubscribedTopics(PortalId, ForumModuleId, this.UID); ;
            subscribedTopics.ForEach(s =>
            {
                s.ForumName = fc.GetForum(PortalId, ForumModuleId, s.ForumId).ForumName;
                s.ForumGroupName = fc.GetForum(PortalId, ForumModuleId, s.ForumId).GroupName;
                var topic = tc.Topics_Get(PortalId, ForumModuleId, s.TopicId);
                s.Subject = topic.Content.Subject;
                s.LastPostDate = topic.Content.DateUpdated;
                s.Subscribed = true;
            });
            dgrdTopicSubs.DataSource = subscribedTopics.ToList();
            dgrdTopicSubs.DataBind();
        }
        private void BindForumSubs()
        {
            var fc = new DotNetNuke.Modules.ActiveForums.ForumController();
            var roles = new UserController().GetUser(PortalId, ForumModuleId, UID).UserRoles;
            var availableForumsString = fc.GetForumsForUser(roles, PortalId, ForumModuleId);
            var availableForums = availableForumsString.Split(separator: new[] { ';' }, options: StringSplitOptions.RemoveEmptyEntries).Select(forum =>
            {

                var Forum = fc.GetForum(PortalId, ForumModuleId, int.Parse(forum));
                return new { ForumId = int.Parse(forum), Forum };
            });

            var forumSubscriptions = new DotNetNuke.Modules.ActiveForums.Controllers.SubscriptionController().SubscribedForums(PortalId, ForumModuleId, this.UID);
            var subscribedForums = forumSubscriptions.Select(forumSubscription =>
            {
                return new { forumSubscription.Id, forumSubscription.ForumId };
            });
            var mergedSubscriptions = from af in availableForums
                                      join sf in subscribedForums
                                      on af.ForumId equals sf.ForumId into merged
                                      from ms in merged.DefaultIfEmpty()
                                      where (af.Forum.AllowSubscribe || ms == null || ms.Id != 0)
                                      select new DotNetNuke.Modules.ActiveForums.Entities.SubscriptionInfo((ms?.Id ?? 0), PortalId, ForumModuleId, af.ForumId, 0, 1, this.UID, af.Forum.GroupName, af.Forum.ForumName, string.Empty, af.Forum.LastPostDateTime, (ms != null));
            dgrdForumSubs.DataSource = mergedSubscriptions.ToList();
            dgrdForumSubs.DataBind();
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
                                chkBox.Attributes.Add("onclick", "amaf_forumSubscribe(" + ForumModuleId + "," + subscriptionInfo.ForumId + ");");
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
                                chkBox.Attributes.Add("onclick", "amaf_topicSubscribe(" + ForumModuleId + "," + subscriptionInfo.ForumId + "," + subscriptionInfo.TopicId + ");");
                                chkBox.Enabled = true;
                            }
                        }
                    }
                }
            }
        }
        protected void ForumSubsGridRow_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            dgrdForumSubs.PageIndex = e.NewPageIndex;
            dgrdForumSubs.DataBind();
        }
        protected void TopicSubsGridRow_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            dgrdTopicSubs.PageIndex = e.NewPageIndex;
            dgrdTopicSubs.DataBind();
        }
        private void btnSubscribeAll_Click(object sender, System.EventArgs e)
        {
            var fc = new DotNetNuke.Modules.ActiveForums.ForumController();
            var roles = new UserController().GetUser(PortalId, ForumModuleId, UID).UserRoles;
            var availableForumsString = fc.GetForumsForUser(roles, PortalId, ForumModuleId);
            var availableForums = availableForumsString.Split(separator: new[] { ';' }, options: StringSplitOptions.RemoveEmptyEntries).Select(forum =>
            {
                var Forum = fc.GetForum(PortalId, ForumModuleId, int.Parse(forum));
                return new { ForumId = int.Parse(forum), Forum };
            });

            var forumSubscriptions = new DotNetNuke.Modules.ActiveForums.Controllers.SubscriptionController().SubscribedForums(PortalId, ForumModuleId, UID);
            var subscribedForums = forumSubscriptions.Select(forumSubscription =>
            {
                return new { forumSubscription.Id, forumSubscription.ForumId };
            });
            var mergedSubscriptions = from af in availableForums
                                      join sf in subscribedForums
                                      on af.ForumId equals sf.ForumId into merged
                                      from ms in merged.DefaultIfEmpty()
                                      where (af.Forum.AllowSubscribe || ms == null || ms.Id != 0)
                                      select new DotNetNuke.Modules.ActiveForums.Entities.SubscriptionInfo((ms?.Id ?? 0), PortalId, ForumModuleId, af.ForumId, 0, 1, UID, af.Forum.GroupName, af.Forum.ForumName, string.Empty, af.Forum.LastPostDateTime, (ms != null));
            mergedSubscriptions.Where(s => !s.Subscribed).ForEach(s =>
            {
               new DotNetNuke.Modules.ActiveForums.Controllers.SubscriptionController().Subscribe(s.PortalId, s.ModuleId, UID, s.ForumId);
            });
            BindForumSubs();
        }
    }
}
