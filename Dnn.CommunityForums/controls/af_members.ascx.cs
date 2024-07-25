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
namespace DotNetNuke.Modules.ActiveForums
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Web.UI;

    public partial class af_members : ForumBase
    {

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            lblHeader.Text = Utilities.GetSharedResource("[RESX:MemberDirectory]");
            string sMode = MainSettings.MemberListMode;
            if (!UserInfo.IsSuperUser)
            {
                if (sMode == "DISABLED")
                {
                    Response.Redirect(Utilities.NavigateURL(TabId));
                }
                if (!Request.IsAuthenticated & (sMode == "ENABLEDREG" || sMode == "ENABLEDMOD"))
                {
                    Response.Redirect(Utilities.NavigateURL(TabId));
                }
                else if (Request.IsAuthenticated && sMode == "ENABLEDMOD" && !UserIsMod)
                {
                    Response.Redirect(Utilities.NavigateURL(TabId));
                }
            }

            SettingsBase ctl = null;
            ctl = (SettingsBase)(new DotNetNuke.Modules.ActiveForums.Controls.Members());
            ctl.ModuleConfiguration = this.ModuleConfiguration;
            if (!(this.Params == string.Empty))
            {
                ctl.Params = this.Params;
            }
            DotNetNuke.Framework.CDefault tempVar = this.BasePage;
            Environment.UpdateMeta(ref tempVar, "[VALUE] - " + lblHeader.Text, "[VALUE]", "[VALUE]");
            plhMembers.Controls.Add(ctl);

        }
    }
}
