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

    public partial class profile_adminsettings : ProfileBase
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.cbAdmin.CallbackEvent += this.cbAdmin_Callback;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo forumUserInfo = this.ForumUserInfo;
            if (forumUserInfo == null & this.UID > 0)
            {
                forumUserInfo = new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ForumModuleId).GetByUserId(this.PortalId, this.UID);
            }

            if (forumUserInfo != null)
            {
                this.txtRewardPoints.Text = forumUserInfo.RewardPoints.ToString();
                this.txtUserCaption.Text = forumUserInfo.UserCaption;
                this.chkDisableSignature.Checked = forumUserInfo.SignatureDisabled;
                this.chkDisableAttachments.Checked = forumUserInfo.AttachDisabled;
                this.chkDisableAvatar.Checked = forumUserInfo.AvatarDisabled;
                this.chkMonitor.Checked = forumUserInfo.AdminWatch;
                this.drpDefaultTrust.SelectedIndex = this.drpDefaultTrust.Items.IndexOf(this.drpDefaultTrust.Items.FindByValue(forumUserInfo.TrustLevel.ToString()));
                this.txtRewardPoints.Attributes.Add("onkeypress", "return onlyNumbers(event);");
            }
        }

        private void cbAdmin_Callback(object sender, Modules.ActiveForums.Controls.CallBackEventArgs e)
        {
            if (!(this.ForumUser.CurrentUserType == CurrentUserTypes.Anon) && !(this.ForumUser.CurrentUserType == CurrentUserTypes.Auth))
            {
                DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo forumUserInfo = new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ForumModuleId).GetByUserId(this.PortalId, this.UID);
                if (forumUserInfo != null)
                {
                    forumUserInfo.RewardPoints = Convert.ToInt32(e.Parameters[1]);
                    forumUserInfo.UserCaption = e.Parameters[2].ToString();
                    forumUserInfo.SignatureDisabled = Convert.ToBoolean(e.Parameters[3]);
                    forumUserInfo.AvatarDisabled = Convert.ToBoolean(e.Parameters[4]);
                    forumUserInfo.TrustLevel = Convert.ToInt32(e.Parameters[5]);
                    forumUserInfo.AdminWatch = Convert.ToBoolean(e.Parameters[6]);
                    forumUserInfo.AttachDisabled = Convert.ToBoolean(e.Parameters[7]);
                    DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.Save(forumUserInfo);
                }
            }
        }
    }
}
