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
    using System.Web.UI;

    public partial class admin_manageforums : ActiveAdminBase
    {
        public string imgOn = string.Empty;
        public string imgOff = string.Empty;
        public string ctlView = string.Empty;

        #region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.cbForumEditor.CallbackEvent += this.cbForumEditor_Callback;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.imgOn = this.Page.ResolveUrl(Globals.ModulePath + "images/admin_check.png");
            this.imgOff = this.Page.ResolveUrl(Globals.ModulePath + "images/admin_stop.png");

            this.litButtons.Text = "<div class=\"amcplnkbtn\" onclick=\"LoadView('manageforums_forumeditor','0|G');\">[RESX:NewForumGroup]</div><div class=\"amcplnkbtn\" onclick=\"LoadView('manageforums_forumeditor','0|F');\">[RESX:NewForum]</div>";

            this.GetControl("admin_manageforums_home", string.Empty);
        }

        private void cbForumEditor_Callback(object sender, Controls.CallBackEventArgs e)
        {
            try
            {
                string sOptions = string.Empty;
                if (e.Parameters[1] != null)
                {
                    sOptions = e.Parameters[1];
                }

                this.GetControl(e.Parameters[0], sOptions);
                System.IO.StringWriter stringWriter = new System.IO.StringWriter();
                HtmlTextWriter htmlWriter = new HtmlTextWriter(stringWriter);
                this.plhForumEditor.RenderControl(e.Output);
            }
            catch (Exception ex)
            {
            }
        }

        #endregion

        #region Private Methods
        private void GetControl(string view, string options)
        {
            try
            {
                this.plhForumEditor.Controls.Clear();
                string ctlPath = string.Empty;
                string ctlId = string.Empty;
                if (view == "admin_manageforums_home")
                {
                    ctlPath = this.Page.ResolveUrl(Globals.ModulePath + "controls/admin_manageforums_home.ascx");
                    ctlId = "admin_manageforums_home";
                }
                else
                {
                    ctlPath = this.Page.ResolveUrl(Globals.ModulePath + "controls/admin_manageforums_forumeditor.ascx");
                    ctlId = "admin_manageforums_forumeditor";
                }

                ActiveAdminBase ctl = (ActiveAdminBase)this.LoadControl(ctlPath);
                ctl.ID = ctlId;
                ctl.ModuleConfiguration = this.ModuleConfiguration;

                if (!(options == string.Empty))
                {
                    ctl.Params = options;
                }

                if (!this.plhForumEditor.Controls.Contains(ctl))
                {
                    this.plhForumEditor.Controls.Add(ctl);
                }
            }
            catch (Exception ex)
            {
                LiteralControl lit = new LiteralControl();
                lit.Text = ex.Message;
                this.plhForumEditor.Controls.Add(lit);
            }
        }

        #endregion

    }
}
