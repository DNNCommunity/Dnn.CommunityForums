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
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Entities.Modules;

    public class ActiveAdminBase : DotNetNuke.Entities.Modules.PortalModuleBase
    {
        private string currentView = string.Empty;
        private DateTime cacheUpdatedTime;
        public const string RequiredImage = Globals.ModulePath + "images/error.gif";

        #region Constants
        internal const string ViewKey = "afcpView";
        internal const string ParamKey = "afcpParams";
        internal const string DefaultView = "home";
        #endregion

        public string Params { get; set; } = string.Empty;

        public bool IsCallBack { get; set; }

        public string HostURL
        {
            get
            {
                object obj = DataCache.SettingsCacheRetrieve(this.ModuleId, string.Format(CacheKeys.HostUrl, this.ModuleId));
                if (obj == null)
                {
                    string sURL;
                    if (this.Request.IsSecureConnection)
                    {
                        sURL = string.Concat("https://", Common.Globals.GetDomainName(this.Request), "/");
                    }
                    else
                    {
                        sURL = string.Concat("http://", Common.Globals.GetDomainName(this.Request), "/");
                    }

                    DataCache.SettingsCacheStore(this.ModuleId, string.Format(CacheKeys.HostUrl, this.ModuleId), sURL, DateTime.UtcNow.AddMinutes(30));
                    return sURL;
                }

                return Convert.ToString(obj);
            }
        }

        protected string GetSharedResource(string key)
        {
            return Utilities.GetSharedResource(key, true);
        }

        public SettingsInfo MainSettings
        {
            get
            {
                return new SettingsInfo { ModuleId = this.ModuleId, MainSettings = new ModuleController().GetModule(moduleID: this.ModuleId).ModuleSettings };
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.LocalResourceFile = Globals.ControlPanelResourceFile;
        }

        public string LocalizeControl(string controlText)
        {
            return Utilities.LocalizeControl(controlText, true);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            var stringWriter = new System.IO.StringWriter();
            var htmlWriter = new HtmlTextWriter(stringWriter);
            base.Render(htmlWriter);
            string html = stringWriter.ToString();
            html = this.LocalizeControl(html);
            writer.Write(html);
        }

        public Controls.ClientTemplate GetLoadingTemplateSmall()
        {
            var template = new Controls.ClientTemplate { ID = "LoadingTemplate" };
            template.Controls.Add(new LiteralControl(string.Concat("<div style=\"text-align:center;font-family:Tahoma;font-size:10px;\"><img src=\"", this.Page.ResolveUrl("~/DesktopModules/ActiveForums/images/spinner.gif"), "\" align=\"absmiddle\" alt=\"Loading\" />Loading...</div>")));
            return template;
        }

        public void BindTemplateDropDown(DropDownList drp, Templates.TemplateTypes templateType, string defaultText, string defaultValue)
        {
            var tc = new TemplateController();
            drp.DataTextField = "Title";
            drp.DataValueField = "TemplateID";
            drp.DataSource = tc.Template_List(this.PortalId, this.ModuleId, templateType);
            drp.DataBind();
            drp.Items.Insert(0, new ListItem(defaultText, defaultValue));
        }

        public string CurrentView
        {
            get
            {
                if (this.Session[ViewKey] != null)
                {
                    return this.Session[ViewKey].ToString();
                }

                if (this.currentView != string.Empty)
                {
                    return this.currentView;
                }

                return DefaultView;
            }

            set
            {
                this.Session[ViewKey] = value;
                this.currentView = value;
            }
        }
    }
}
