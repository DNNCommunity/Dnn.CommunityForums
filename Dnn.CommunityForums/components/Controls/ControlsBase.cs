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

namespace DotNetNuke.Modules.ActiveForums.Controls
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Web;
    using System.Web.UI;

    public class ControlsBase : ForumBase
    {
        private string template;
        private string currentView = "forumview";
        private bool parseTemplate = false;

        [Description("Template for display"), PersistenceMode(PersistenceMode.InnerProperty)]
        public string DisplayTemplate
        {
            get
            {
                if (string.IsNullOrEmpty(this.template) && !string.IsNullOrEmpty(this.TemplateFile))
                {
                    if (!string.IsNullOrEmpty(this.ControlConfig.TemplatePath))
                    {
                        this.template = this.ControlConfig.TemplatePath + this.TemplateFile;
                    }
                    else
                    {
                        this.template = this.TemplateFile;
                    }

                    this.template = Utilities.GetTemplate(this.Page.ResolveUrl(this.template));
                    this.template = Utilities.ParseTokenConfig(this.ForumModuleId, this.template, "default", this.ControlConfig);
                }

                return this.template;
            }

            set
            {
                this.template = value;
            }
        }

        public string CurrentView
        {
            get
            {
                return this.currentView;
            }

            set
            {
                this.currentView = value;
            }
        }

        public bool ParseTemplateFile
        {
            get
            {
                return this.parseTemplate;
            }

            set
            {
                this.parseTemplate = value;
            }
        }
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public int DataPageId
        {
            get
            {
                if (HttpContext.Current.Request.QueryString[ParamKeys.PageId] == null)
                {
                    return 1;
                }
                else
                {
                    return int.Parse(HttpContext.Current.Request.QueryString[ParamKeys.PageId].ToString());
                }
            }
        }

        protected override void OnInit(EventArgs e)
        {
            try
            {
                base.OnInit(e);

                if (this.ParseTemplateFile)
                {
                    if (!string.IsNullOrEmpty(this.DisplayTemplate))
                    {
                        Control ctl = this.Page.ParseControl(this.DisplayTemplate);
                        this.LinkControls(ctl.Controls);
                        this.Controls.Add(ctl);
                    }
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                throw;
            }
        }

        private void LinkControls(ControlCollection ctrls)
        {
            try
            {
                foreach (Control ctrl in ctrls)
                {
                    if (ctrl is Controls.ForumRow)
                    {
                        ((Controls.ForumRow)ctrl).UserRoles = this.ForumUser.UserRoles;
                    }

                    if (ctrl is Controls.ControlsBase)
                    {
                        ((Controls.ControlsBase)ctrl).ControlConfig = this.ControlConfig;
                        ((Controls.ControlsBase)ctrl).ForumInfo = this.ForumInfo;
                    }

                    if (ctrl.Controls.Count > 0)
                    {
                        this.LinkControls(ctrl.Controls);
                    }
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                throw;
            }
        }
    }
}
