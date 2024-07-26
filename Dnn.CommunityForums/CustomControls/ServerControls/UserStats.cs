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
namespace DotNetNuke.Modules.ActiveForums.Controls
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Text;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework;

    [DefaultProperty("Text"), ToolboxData("<{0}:UserStats runat=server></{0}:UserStats>")]
    public class UserStats : WebControl
    {
        private DisplayTemplate template;
        private int userId = -1;
        private int moduleId = -1;

        public DisplayTemplate Template
        {
            get
            {
                return this.template;
            }

            set
            {
                this.template = value;
            }
        }

        public int UserId
        {
            get
            {
                return this.userId;
            }

            set
            {
                try
                {
                    this.userId = value;
                }
                catch (Exception ex)
                {
                    this.userId = -1;
                }
            }
        }

        public int ModuleId
        {
            get
            {
                return this.moduleId;
            }

            set
            {
                this.moduleId = value;
            }
        }

        protected override void RenderContents(HtmlTextWriter writer)
        {
            if (this.UserId == -1)
            {
                return;
            }

            try
            {
                string output = string.Empty;
                PortalSettings ps = DotNetNuke.Modules.ActiveForums.Utilities.GetPortalSettings();

                DotNetNuke.Entities.Users.UserInfo cu = DotNetNuke.Entities.Users.UserController.Instance.GetCurrentUserInfo();
                string imagePath = string.Empty;
                int portalId = ps.PortalId;
                string tmp = string.Empty;
                if (this.Template == null)
                {
                    tmp = "<span class=\"aslabelsmbold\">[RESX:Posts]:</span> [AF:PROFILE:POSTCOUNT]<br />" + "<span class=\"aslabelsmbold\">[RESX:RankName]:</span> [AF:PROFILE:RANKNAME]<br />" + "<span class=\"aslabelsmbold\">[RESX:RankDisplay]:</span> [AF:PROFILE:RANKDISPLAY] <br />" + "<span class=\"aslabelsmbold\">[RESX:LastUpdate]:</span> [AF:PROFILE:DATELASTACTIVITY:d] <br />" + "<span class=\"aslabelsmbold\">[RESX:MemberSince]:</span> [AF:PROFILE:DATECREATED:d]";
                }
                else
                {
                    tmp = this.Template.Text;
                }

                if (this.ModuleId == -1)
                {
                    foreach (DotNetNuke.Entities.Modules.ModuleInfo mi in DotNetNuke.Entities.Modules.ModuleController.Instance.GetModules(portalId))
                    {
                        if (mi.DesktopModule.ModuleName.ToUpperInvariant() == Globals.ModuleName.ToUpperInvariant())
                        {
                            this.ModuleId = mi.ModuleID;
                            break;
                        }
                    }
                }

                output = TemplateUtils.ParseProfileTemplate(tmp, this.UserId, portalId, this.ModuleId, cu.UserID, Utilities.GetTimeZoneOffsetForUser(portalId, this.UserId));
                output = Utilities.LocalizeControl(output);
                writer.Write(output);
            }
            catch (Exception ex)
            {
                writer.Write(ex.Message);
            }
        }
    }
}
