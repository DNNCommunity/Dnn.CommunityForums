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

namespace DotNetNuke.Modules.ActiveForums.Controls
{
    using System;
    using System.IO;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    [ToolboxData("<{0}:ForumLoader runat=server></{0}:ForumLoader>")]
    public class ForumLoader : ForumBase
    {
        private DotNetNuke.Modules.ActiveForums.Entities.ForumInfo fi;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                if (this.ForumId > 0 && this.ForumModuleId == -1)
                {
                    this.fi = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(this.ForumId, this.ForumModuleId);
                    this.ForumModuleId = this.fi.ModuleId;
                }

                if (this.ForumModuleId > 0)
                {
                    DotNetNuke.Entities.Modules.ModuleInfo modInfo = new DotNetNuke.Entities.Modules.ModuleInfo();
                    modInfo.TabID = this.TabId;
                    modInfo.ModuleID = this.ModuleId;
                    modInfo.PortalID = this.PortalId;
                    modInfo.DesktopModule.Permissions = this.ModuleConfiguration.DesktopModule.Permissions;

                    ForumBase objModule = (ForumBase)this.LoadControl("~/desktopmodules/ActiveForums/classic.ascx");
                    if (objModule != null)
                    {
                        objModule.ModuleConfiguration = modInfo;
                        objModule.ID = Path.GetFileNameWithoutExtension("~/desktopmodules/ActiveForums/classic.ascx");
                        objModule.ForumModuleId = this.ForumModuleId;
                        objModule.ForumTabId = this.ForumTabId;
                        objModule.ForumInfo = this.fi;
                        objModule.ForumId = this.ForumId;
                        objModule.ForumGroupId = this.ForumGroupId;
                        objModule.ParentForumId = this.ParentForumId;
                        objModule.ForumIds = this.ForumIds;
                        this.Controls.Add(objModule);
                    }
                }
                else
                {
                    Label lblMessage = new Label();
                    lblMessage.Text = "Please access the Module Settings page to configure this module.";
                    lblMessage.CssClass = "NormalRed";
                    this.Controls.Add(lblMessage);
                }
            }
            catch (Exception exc)
            {
                DotNetNuke.Services.Exceptions.Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
    }
}
