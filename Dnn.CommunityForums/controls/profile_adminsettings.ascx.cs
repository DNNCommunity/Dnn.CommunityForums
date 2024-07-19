//
// Community Forums
// Copyright (c) 2013-2024
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
using DotNetNuke.Modules.ActiveForums.Entities;

namespace DotNetNuke.Modules.ActiveForums
{
    public partial class profile_adminsettings : ProfileBase
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            cbAdmin.CallbackEvent += cbAdmin_Callback;
        }

        protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
            DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo forumUserInfo = ForumUserInfo;
            if (forumUserInfo == null & UID > 0)
            {
                forumUserInfo = new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController().GetById(UID);
            }

            if (forumUserInfo != null)
            {
                txtRewardPoints.Text = forumUserInfo.RewardPoints.ToString();
                txtUserCaption.Text = forumUserInfo.UserCaption;
                chkDisableSignature.Checked = forumUserInfo.SignatureDisabled;
                chkDisableAttachments.Checked = forumUserInfo.AttachDisabled;
                chkDisableAvatar.Checked = forumUserInfo.AvatarDisabled;
                chkMonitor.Checked = forumUserInfo.AdminWatch;
                drpDefaultTrust.SelectedIndex = drpDefaultTrust.Items.IndexOf(drpDefaultTrust.Items.FindByValue(forumUserInfo.TrustLevel.ToString()));
                txtRewardPoints.Attributes.Add("onkeypress", "return onlyNumbers(event);");
            }

        }

        private void cbAdmin_Callback(object sender, Modules.ActiveForums.Controls.CallBackEventArgs e)
        {
            if (!(CurrentUserType == CurrentUserTypes.Anon) && !(CurrentUserType == CurrentUserTypes.Auth))
            {
                DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo forumUserInfo = new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController().GetById(UID);
                if (forumUserInfo != null)
                {
                    forumUserInfo.RewardPoints = Convert.ToInt32(e.Parameters[1]);
                    forumUserInfo.UserCaption = e.Parameters[2].ToString();
                    forumUserInfo.SignatureDisabled = Convert.ToBoolean(e.Parameters[3]);
                    forumUserInfo.AvatarDisabled = Convert.ToBoolean(e.Parameters[4]);
                    forumUserInfo.TrustLevel = Convert.ToInt32(e.Parameters[5]);
                    forumUserInfo.AdminWatch = Convert.ToBoolean(e.Parameters[6]);
                    forumUserInfo.AttachDisabled = Convert.ToBoolean(e.Parameters[7]);
                    DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.Save(ForumModuleId, forumUserInfo);
                }
            }
        }
    }
}
