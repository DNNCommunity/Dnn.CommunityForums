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

    public partial class af_assign_user_badges : ForumBase
    {
        private int UID { get; set; }
        protected global::System.Web.UI.WebControls.Label lblUser;
        protected global::System.Web.UI.UpdatePanel upOptions;
        protected global::System.Web.UI.WebControls.Label lblThreadsTracked;
        protected global::System.Web.UI.WebControls.Label lblForumsTracked;
        protected global::System.Web.UI.WebControls.GridView dgrdUserBadges;
        protected global::System.Web.UI.WebControls.Button btnSubscribeAll;

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.dgrdUserBadges.Columns[3].HeaderText = DotNetNuke.Modules.ActiveForums.Utilities.GetSharedResource("[RESX:Badge].Text");

            this.dgrdUserBadges.PageIndexChanging += this.UserBadgesGridRowPageIndexChanging;
            this.dgrdUserBadges.RowDataBound += this.OnUserBadgesGridRowDataBound;
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

                this.dgrdUserBadges.PageSize = _pageSize;

                this.UID = this.Request.QueryString[Literals.UserId] != null ? Convert.ToInt32(this.Request.QueryString[Literals.UserId]) : -1;

                if (this.UserId > 0 && this.ForumUser.GetIsMod(this.ForumModuleId))
                {
                    this.BindUserBadges();
                }
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }
        }


        private void BindUserBadges()
        {
            this.dgrdUserBadges.DataSource = this.GetBadges().ToList();
            this.dgrdUserBadges.DataBind();
        }


        private IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.UserBadgeInfo> GetBadges()
        {
            var availableBadges = new DotNetNuke.Modules.ActiveForums.Controllers.BadgeController().Get(this.ForumModuleId).Select(badge =>
            {
                return new { badge.BadgeId, badge };
            });

            var assignedBadges = new DotNetNuke.Modules.ActiveForums.Controllers.UserBadgeController(this.PortalId, this.ForumModuleId).GetForUser(this.UID);
            var assignedBadgeIds = assignedBadges.Select(userBadge =>
            {
                return new { userBadge.UserBadgeId, userBadge.BadgeId };
            });
            var mergedBadges = from availBadges in availableBadges
                                      join asgnedBadges in assignedBadgeIds
                                      on availBadges.BadgeId equals asgnedBadges.BadgeId into merged
                                      from mrgdBadges in merged.DefaultIfEmpty()
                                      where mrgdBadges == null || mrgdBadges.UserBadgeId != 0
                                      select new DotNetNuke.Modules.ActiveForums.Entities.UserBadgeInfo(mrgdBadges?.UserBadgeId ?? 0, mrgdBadges.BadgeId, availBadges.badge.Name, this.UID, this.PortalId, this.ForumModuleId, mrgdBadges != null);
            return mergedBadges;
        }

        protected void OnUserBadgesGridRowDataBound(object sender, GridViewRowEventArgs e)
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
                                chkBox.Attributes.Add("onclick", "amaf_badgeAssign(" + this.ForumModuleId + "," + userBadgeInfo.BadgeId + "," + userBadgeInfo.UserId + "," + chkBox.Checked + ");");
                                chkBox.Enabled = true;
                            }
                        }
                    }
                }
            }
        }

        protected void UserBadgesGridRowPageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            this.dgrdUserBadges.PageIndex = e.NewPageIndex;
            this.dgrdUserBadges.DataBind();
        }
    }
}
