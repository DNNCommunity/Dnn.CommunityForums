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
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Web;
    using System.Web.UI;

    using DotNetNuke.Framework;
    using DotNetNuke.Web.Client.ClientResourceManagement;

    public partial class ControlPanel : ActiveAdminBase
    {
        #region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.cbShell.CallbackEvent += this.cbShell_Callback;
            this.cbModal.CallbackEvent += this.cbModal_Callback;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.IsCallBack = this.cbShell.IsCallback;

            this.btnReturn.ClientSideScript = "window.location.href = '" + Utilities.NavigateURL(this.TabId) + "';";
            this.cbModal.LoadingTemplate = this.GetLoadingTemplateSmall();
            Hashtable settings = DotNetNuke.Entities.Modules.ModuleController.Instance.GetModule(moduleId: this.ModuleId, tabId: this.TabId, ignoreCache: false).ModuleSettings;
            if (Convert.ToBoolean(settings["AFINSTALLED"]) == false)
            {
                try
                {
                    var fc = new ForumsConfig();
                    bool configComplete = fc.ForumsInit(this.PortalId, this.ModuleId);
                    DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(this.ModuleId, "AFINSTALLED", configComplete.ToString());
                }
                catch (Exception ex)
                {
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                }
            }

            ClientResourceManager.RegisterStyleSheet(this.Page, "~/DesktopModules/ActiveForums/ControlPanel.css");
            ClientResourceManager.RegisterStyleSheet(this.Page, string.Concat("~/DesktopModules/ActiveForums/themes/", this.MainSettings.Theme, "/jquery-ui.min.css"));
            ClientResourceManager.RegisterStyleSheet(this.Page, filePath: $"{Globals.ModulePath}Resources/font-awesome-4.7.0/css/font-awesome.min.css", priority: 10);

            this.lblProd.Visible = true;
            this.lblCopy.Visible = true;
            // TODO: this should be resources instead of harcoded text?
            this.lblProd.Text = string.Concat("DNN Community Forums ", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);
            this.lblCopy.Text = string.Concat("&copy; ", DateTime.UtcNow.Year, " DNN Community");

            try
            {
                if (!this.Page.IsPostBack)
                {
                    this.GetControl(this.CurrentView, this.Params, this.IsCallBack);
                }

            }
            catch (Exception ex)
            {

                if (this.Request.QueryString["cptry"] == null)
                {
                    string sURL = this.EditUrl(string.Empty, string.Empty, "EDIT", "cptry=1");
                    this.Response.Redirect(sURL);
                }
                else
                {
                    DotNetNuke.Services.Exceptions.Exceptions.ProcessModuleLoadException(this, ex);
                }
            }

            ClientResourceManager.RegisterScript(this.Page, "~/desktopmodules/activeforums/scripts/json2009.min.js", 101);
            ClientResourceManager.RegisterScript(this.Page, "~/desktopmodules/activeforums/scripts/jquery.history.js", 102);
            ClientResourceManager.RegisterScript(this.Page, "~/desktopmodules/activeforums/scripts/afadmin.js", 103);
            ClientResourceManager.RegisterScript(this.Page, "~/desktopmodules/activeforums/scripts/jquery.listreorder.js", 104);
            ClientResourceManager.RegisterScript(this.Page, "~/desktopmodules/activeforums/active/amlib.js", 105);

            string lang = "en-US";
            if (this.Request.QueryString["language"] != null)
            {
                lang = this.Request.QueryString["language"];
            }

            if (string.IsNullOrEmpty(lang))
            {
                lang = this.PortalSettings.DefaultLanguage;
            }

            if (string.IsNullOrEmpty(lang))
            {
                lang = "en-US";
            }

            string adminHandler = VirtualPathUtility.ToAbsolute("~/desktopmodules/activeforums/handlers/adminhelper.ashx") + "?TabId=" + this.TabId.ToString() + "&PortalId=" + this.PortalId.ToString() + "&moduleid=" + this.ModuleId + "&language=" + lang;
            var sb = new StringBuilder();
            sb.AppendLine("var asScriptPath = '" + VirtualPathUtility.ToAbsolute("~/desktopmodules/activeforums/scripts/") + "';");
            sb.AppendFormat("var afAdminHandlerURL = '{0}';", adminHandler);
            sb.AppendLine("var af_imgPath = '" + VirtualPathUtility.ToAbsolute(Globals.ModuleImagesPath) + "';");
            string sLoadImg;
            sLoadImg = "var afSpinLg = new Image();afSpinLg.src='" + VirtualPathUtility.ToAbsolute("~/desktopmodules/activeforums/images/spinner-lg.gif") + "';";
            sLoadImg += "var afSpin = new Image();afSpin.src='" + VirtualPathUtility.ToAbsolute("~/desktopmodules/activeforums/images/spinner.gif") + "';";
            sb.AppendLine(sLoadImg);
            this.Page.ClientScript.RegisterStartupScript(this.Page.GetType(), "afscripts", sb.ToString(), true);

            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();

        }

        public void cbShell_Callback(object sender, Controls.CallBackEventArgs e)
        {
            try
            {
                string sOptions = string.Empty;
                if (e.Parameters[1] != null)
                {
                    sOptions = e.Parameters[1];
                }

                this.GetControl(e.Parameters[0], sOptions, true);
                if (e.Parameters.Length != 3)
                {
                    var stringWriter = new System.IO.StringWriter();
                    var htmlWriter = new HtmlTextWriter(stringWriter);
                    this.plhControlPanel.RenderControl(e.Output);
                }

            }
            catch (Exception ex)
            {

                if (this.Request.QueryString["cptry"] == null)
                {
                    string sURL = this.EditUrl(string.Empty, string.Empty, "EDIT", "cptry=1");
                    this.Response.Redirect(sURL);
                }
                else
                {
                    DotNetNuke.Services.Exceptions.Exceptions.ProcessModuleLoadException(this, ex);
                }
            }

        }

        private void cbModal_Callback(object sender, Controls.CallBackEventArgs e)
        {
            switch (e.Parameters[0].ToLowerInvariant())
            {
                case "load":
                    this.plhModal.Controls.Clear();
                    string ctlPath = string.Empty;
                    string ctrl = e.Parameters[1].ToLowerInvariant();
                    string ctlParams = e.Parameters[2].ToLowerInvariant();
                    this.LoadModal(ctrl, ctlParams);
                    break;
                case "clear":
                    this.plhModal.Controls.Clear();
                    break;
            }

            this.plhModal.RenderControl(e.Output);
        }

        #endregion
        #region Private Methods
        private void GetControl(string view, string options, bool isCallback)
        {
            try
            {
                this.plhControlPanel.Controls.Clear();
                string ctlPath;
                if (view == "undefined")
                {
                    view = "home";
                }

                this.CurrentView = view;

                this.Params = options;
                ctlPath = string.Concat("~/DesktopModules/ActiveForums/controls/admin_", view, ".ascx");
                var ctl = (ActiveAdminBase)this.LoadControl(ctlPath);
                ctl.ID = view;
                ctl.ModuleConfiguration = this.ModuleConfiguration;

                if (options != string.Empty)
                {
                    ctl.Params = options;
                }

                if (!this.plhControlPanel.Controls.Contains(ctl))
                {
                    this.plhControlPanel.Controls.Add(ctl);
                }
            }
            catch (Exception ex)
            {
                this.CurrentView = null;
                this.Params = null;

                if (this.Request.QueryString["cptry"] == null)
                {
                    string sURL = this.EditUrl(string.Empty, string.Empty, "EDIT", "cptry=1");
                    this.Response.Redirect(sURL);
                }
                else
                {
                    DotNetNuke.Services.Exceptions.Exceptions.ProcessModuleLoadException(this, ex);
                }
            }

        }

        private void LoadModal(string ctrl, string @params = "")
        {
            this.plhModal.Controls.Clear();
            string ctlPath;

            ctlPath = string.Concat("~/DesktopModules/activeforums/controls/", ctrl, ".ascx");
            var ctl = (ActiveAdminBase)this.LoadControl(ctlPath);
            ctl.ID = ctrl;
            ctl.ModuleConfiguration = this.ModuleConfiguration;

            if (@params != string.Empty)
            {
                ctl.Params = @params;
            }

            if (!this.plhModal.Controls.Contains(ctl))
            {
                this.plhModal.Controls.Add(ctl);
            }
        }
        #endregion

    }
}
