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

    using DotNetNuke.Security.Permissions;

    public partial class _default : ForumBase
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.btnContinue.Visible = ModulePermissionController.HasModulePermission(this.ModuleConfiguration.ModulePermissions, "EDIT");
            this.btnContinue.Click += new System.EventHandler(this.btnContinue_Click);
        }

        private void btnContinue_Click(object sender, System.EventArgs e)
        {
            ForumsConfig fc = new ForumsConfig();
            bool init = false;
            init = fc.ForumsInit(this.PortalId, this.ModuleId);
            if (init == true)
            {
                DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "AFINSTALLED", init.ToString());
                DataCache.ClearAllCache(this.ModuleId);
                DataCache.ClearAllCacheForTabId(this.TabId);
                this.Response.Redirect(this.EditUrl());
            }
        }
    }
}
