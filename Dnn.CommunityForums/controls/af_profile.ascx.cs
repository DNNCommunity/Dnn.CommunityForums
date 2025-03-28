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

    public partial class af_profile : ForumBase
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            string sDisplayName = string.Empty;
            int tUid = -1;
            if (this.Request.Params[ParamKeys.UserId] != null)
            {
                if (Utilities.IsNumeric(this.Request.Params[ParamKeys.UserId]))
                {
                    tUid = Convert.ToInt32(this.Request.Params[ParamKeys.UserId]);
                    DotNetNuke.Entities.Users.UserInfo ui = DotNetNuke.Entities.Users.UserController.Instance.GetUser(this.PortalId, tUid);
                    if (ui != null)
                    {
                        sDisplayName = DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.GetDisplayName(this.PortalSettings, this.MainSettings, false, false, ui.UserID, ui.Username, ui.FirstName, ui.LastName, ui.DisplayName);
                    }
                }
            }
            else
            {
                tUid = this.UserId;
                sDisplayName = DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.GetDisplayName(this.PortalSettings, this.MainSettings, false, false, this.UserId, this.UserInfo.Username, this.UserInfo.FirstName, this.UserInfo.LastName, this.UserInfo.DisplayName);
            }

            this.lblHeader.Text = string.Format(Utilities.GetSharedResource("[RESX:ProfileForUser]"), sDisplayName);
            if (this.MainSettings.UseSkinBreadCrumb)
            {
                Environment.UpdateBreadCrumb(this.Page.Controls, "<a href=\"" + Utilities.NavigateURL(this.TabId, string.Empty, new string[] { $"{ParamKeys.ViewType}={Views.Profile}", $"{ParamKeys.UserId}=" + tUid.ToString() }) + "\">" + this.lblHeader.Text + "</a>");
            }

            DotNetNuke.Framework.CDefault tempVar = this.BasePage;
            Environment.UpdateMeta(ref tempVar, "[VALUE] - " + this.lblHeader.Text, "[VALUE]", "[VALUE]");
            SettingsBase ctl = null;
            ctl = (SettingsBase)new DotNetNuke.Modules.ActiveForums.Controls.UserProfile();
            ctl.ModuleConfiguration = this.ModuleConfiguration;
            if (!(this.Params == string.Empty))
            {
                ctl.Params = this.Params;
            }

            this.plhProfile.Controls.Add(ctl);
        }
    }
}
