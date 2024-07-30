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

            UserProfileInfo ui = this.UserProfile;
            if (ui == null & this.UID > 0)
            {
                UserController up = new UserController();
                ui = up.GetUser(this.PortalId, this.ForumModuleId, this.UID).Profile;
            }

            if (ui != null)
            {
                this.txtRewardPoints.Text = ui.RewardPoints.ToString();
                this.txtUserCaption.Text = ui.UserCaption;
                this.chkDisableSignature.Checked = ui.SignatureDisabled;
                this.chkDisableAttachments.Checked = ui.AttachDisabled;
                this.chkDisableAvatar.Checked = ui.AvatarDisabled;
                this.chkMonitor.Checked = ui.AdminWatch;
                this.drpDefaultTrust.SelectedIndex = this.drpDefaultTrust.Items.IndexOf(this.drpDefaultTrust.Items.FindByValue(ui.TrustLevel.ToString()));
                this.txtRewardPoints.Attributes.Add("onkeypress", "return onlyNumbers(event);");
            }
        }

        private void cbAdmin_Callback(object sender, Modules.ActiveForums.Controls.CallBackEventArgs e)
        {
            if (!(this.CurrentUserType == CurrentUserTypes.Anon) && !(this.CurrentUserType == CurrentUserTypes.Auth))
            {
                UserProfileController upc = new UserProfileController();
                UserController uc = new UserController();
                UserProfileInfo upi = uc.GetUser(this.PortalId, this.ForumModuleId, this.UID).Profile;
                if (upi != null)
                {
                    upi.RewardPoints = Convert.ToInt32(e.Parameters[1]);
                    upi.UserCaption = e.Parameters[2].ToString();
                    upi.SignatureDisabled = Convert.ToBoolean(e.Parameters[3]);
                    upi.AvatarDisabled = Convert.ToBoolean(e.Parameters[4]);
                    upi.TrustLevel = Convert.ToInt32(e.Parameters[5]);
                    upi.AdminWatch = Convert.ToBoolean(e.Parameters[6]);
                    upi.AttachDisabled = Convert.ToBoolean(e.Parameters[7]);
                    upc.Profiles_Save(upi);
                }
            }
        }
    }
}
