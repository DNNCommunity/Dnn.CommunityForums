//
// Community Forums
// Copyright (c) 2013-2021
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
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetNuke.Common.Utilities;

namespace DotNetNuke.Modules.ActiveForums.Controls
{
    [ToolboxData("<{0}:ForumLoader runat=server></{0}:ForumLoader>")]
    public class ForumLoader : ForumBase
    {
        private Forum fi;
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {

                if (ForumId > 0 && ForumModuleId == -1)
                {
                    ForumController fc = new ForumController();
                    fi = fc.Forums_Get(PortalId, ForumModuleId, ForumId, true);
                    ForumModuleId = fi.ModuleId;
                }
                if (ForumModuleId > 0)
                {
                    DotNetNuke.Entities.Modules.ModuleInfo modInfo = new DotNetNuke.Entities.Modules.ModuleInfo();
                    modInfo.TabID = TabId;
                    modInfo.ModuleID = ModuleId;
                    modInfo.PortalID = PortalId;
                    modInfo.DesktopModule.Permissions = this.ModuleConfiguration.DesktopModule.Permissions;

                    ForumBase objModule = (ForumBase)(LoadControl("~/desktopmodules/ActiveForums/classic.ascx"));
                    if (objModule != null)
                    {
                        objModule.ModuleConfiguration = modInfo;
                        objModule.ID = Path.GetFileNameWithoutExtension("~/desktopmodules/ActiveForums/classic.ascx");
                        objModule.ForumModuleId = ForumModuleId;
                        objModule.ForumTabId = ForumTabId;
                        objModule.ForumInfo = fi;
                        objModule.ForumId = ForumId;
                        objModule.ForumGroupId = ForumGroupId;
                        objModule.DefaultForumViewTemplateId = DefaultForumViewTemplateId;
                        objModule.DefaultTopicsViewTemplateId = DefaultTopicsViewTemplateId;
                        objModule.DefaultTopicViewTemplateId = DefaultTopicViewTemplateId;
                        objModule.ParentForumId = ParentForumId;
                        objModule.ForumIds = ForumIds;
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
