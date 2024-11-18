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
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Web.UI.WebControls;

    public partial class profile_mypreferences : ForumBase
    {
        public int UID { get; set; }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);

            if (this.Request.QueryString[Literals.UserId] == null)
            {
                this.UID = this.UserInfo.UserID;
            }
            else
            {
                this.UID = Convert.ToInt32(this.Request.QueryString[Literals.UserId]);
            }

            if (this.UID > 0 && !this.Page.IsPostBack)
            {
                var ui = new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ForumModuleId).GetByUserId(this.PortalId, this.UID);
                this.drpPrefDefaultSort.SelectedIndex = this.drpPrefDefaultSort.Items.IndexOf(this.drpPrefDefaultSort.Items.FindByValue(ui.PrefDefaultSort.Trim()));
                this.drpPrefPageSize.SelectedIndex = this.drpPrefPageSize.Items.IndexOf(this.drpPrefPageSize.Items.FindByValue(ui.PrefPageSize.ToString()));

                this.chkPrefJumpToLastPost.Checked = ui.PrefJumpLastPost;
                this.chkPrefTopicSubscribe.Checked = ui.PrefTopicSubscribe;
                this.chkPrefBlockAvatars.Checked = ui.PrefBlockAvatars;
                this.chkPrefBlockSignatures.Checked = ui.PrefBlockSignatures;
                this.txtSignature.Text = ui.Signature;
            }
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public string GetString(string key) => Utilities.GetSharedResource(key);

        private void btnSave_Click(object sender, System.EventArgs e)
        {
            if (this.UserId == this.UID || this.ForumUser.CurrentUserType == CurrentUserTypes.Admin || this.ForumUser.CurrentUserType == CurrentUserTypes.SuperUser)
            {
                var upi = new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ForumModuleId).GetByUserId(this.PortalId, this.UID);
                if (upi != null)
                {
                    upi.PrefDefaultSort = Utilities.XSSFilter(this.drpPrefDefaultSort.SelectedItem.Value, true);
                    upi.PrefPageSize = Convert.ToInt32((Convert.ToInt32(this.drpPrefPageSize.SelectedValue) < 5) ? 5 : Convert.ToInt32(this.drpPrefPageSize.SelectedValue));
                    upi.PrefDefaultShowReplies = false;
                    upi.PrefJumpLastPost = this.chkPrefJumpToLastPost.Checked;
                    upi.PrefTopicSubscribe = this.chkPrefTopicSubscribe.Checked;
                    upi.PrefSubscriptionType = SubscriptionTypes.Instant;
                    upi.PrefBlockAvatars = this.chkPrefBlockAvatars.Checked;
                    upi.PrefBlockSignatures = this.chkPrefBlockSignatures.Checked;
                    if (this.MainSettings.AllowSignatures == 1 || this.MainSettings.AllowSignatures == 0)
                    {
                        upi.Signature = Utilities.XSSFilter(this.txtSignature.Text, true);
                        upi.Signature = Utilities.StripHTMLTag(upi.Signature);
                        upi.Signature = System.Web.HttpUtility.HtmlEncode(upi.Signature);
                    }
                    else if (this.MainSettings.AllowSignatures == 2)
                    {
                        upi.Signature = Utilities.XSSFilter(this.txtSignature.Text, false);
                    }
                    new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ForumModuleId).Save<int>(upi, upi.UserId);

                    this.Response.Redirect(this.NavigateUrl(this.TabId), false);
                    this.Context.ApplicationInstance.CompleteRequest();
                }
            }
        }
    }
}
