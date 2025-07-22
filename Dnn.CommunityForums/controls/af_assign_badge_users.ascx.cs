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

    public partial class af_assign_badge_users : ForumBase
    {
        private int? badgeId { get; set; }
        private int fakeForumId { get; set; }

        private DotNetNuke.Modules.ActiveForums.Entities.BadgeInfo badge { get; set; }

        protected global::System.Web.UI.WebControls.Label lblBadgesAssigned;
        protected global::System.Web.UI.WebControls.GridView dgrdBadgeUsers;

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.fakeForumId = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetForums(this.ForumModuleId).ToList().First().ForumID; /* this is needed to send a valid forum Id to the Web API */
            this.badgeId = this.Request.QueryString[ParamKeys.BadgeId] != null ? Convert.ToInt32(this.Request.QueryString[ParamKeys.BadgeId]) : -1;
            this.badge = new DotNetNuke.Modules.ActiveForums.Controllers.BadgeController().GetById((int)this.badgeId);
            this.lblBadgesAssigned.Text = string.Format(DotNetNuke.Modules.ActiveForums.Utilities.GetSharedResource("[RESX:BadgeUsersAssigned]"), badge.Name);
            this.dgrdBadgeUsers.Columns[3].HeaderText = DotNetNuke.Modules.ActiveForums.Utilities.GetSharedResource("[RESX:Username]");

            this.dgrdBadgeUsers.PageIndexChanging += this.BadgeUsersGridRowPageIndexChanging;
            this.dgrdBadgeUsers.RowDataBound += this.OnBadgeUsersGridRowDataBound;
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

                this.dgrdBadgeUsers.PageSize = _pageSize;

                if (this.UserId > 0 && this.ForumUser.GetIsMod(this.ForumModuleId))
                {
                    this.BindBadgeUsers();
                }
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }
        }

        private void BindBadgeUsers()
        {
            this.dgrdBadgeUsers.DataSource = this.GetBadges().ToList();
            this.dgrdBadgeUsers.DataBind();
        }


        private IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.UserBadgeInfo> GetBadges()
        {
            var availableUsers = new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ForumModuleId).Get().Where(u => u.PortalId == this.PortalId).Select(forumUser =>
            {
                return new { forumUser.UserId, forumUser };
            });

            var assignedBadges = new DotNetNuke.Modules.ActiveForums.Controllers.UserBadgeController(this.PortalId, this.ForumModuleId).GetForBadge((int)this.badgeId);
            var assignedBadgeIds = assignedBadges.Select(userBadge =>
            {
                return new { userBadge.UserBadgeId, userBadge.UserId };
            });
            var mergedBadges = from availUsers in availableUsers
                join asgnedBadges in assignedBadgeIds on availUsers.UserId equals asgnedBadges.UserId into merged
                from mrgdBadges in merged.DefaultIfEmpty()
                //where mrgdBadges == null || mrgdBadges.UserBadgeId != 0
                select new DotNetNuke.Modules.ActiveForums.Entities.UserBadgeInfo(userBadgeId: mrgdBadges?.UserBadgeId ?? 0, userId: availUsers.UserId, userName: availUsers.forumUser.DisplayName, badgeId: (int)this.badgeId, badgeName: this.badge.Name, portalId: this.PortalId, moduleId: this.ForumModuleId, assigned: mrgdBadges != null);
            return mergedBadges;
        }

        protected void OnBadgeUsersGridRowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                DotNetNuke.Modules.ActiveForums.Entities.UserBadgeInfo userBadgeInfo = e.Row.DataItem as DotNetNuke.Modules.ActiveForums.Entities.UserBadgeInfo;
                foreach (TableCell cell in e.Row.Cells)
                {
                    foreach (Control cellControl in cell.Controls)
                    {
                        if (cellControl is CheckBox)
                        {
                            var chkBox = cellControl as CheckBox;
                            if (!(chkBox == null))
                            {
                                chkBox.Attributes.Add("onclick", $"amaf_badgeAssign({this.ForumModuleId},{this.fakeForumId},{userBadgeInfo.BadgeId},{userBadgeInfo.UserId},$('#{cellControl.ClientID}').is(':checked'));");
                                chkBox.Enabled = true;
                            }
                        }
                    }
                }
            }
        }

        protected void BadgeUsersGridRowPageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            this.dgrdBadgeUsers.PageIndex = e.NewPageIndex;
            this.dgrdBadgeUsers.DataBind();
        }
    }
}
