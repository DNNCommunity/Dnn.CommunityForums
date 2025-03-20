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
    using System.ComponentModel;
    using System.Text;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using DotNetNuke.Web.Client.ClientResourceManagement;

    [DefaultProperty("Text"), ToolboxData("<{0}:RequiredFieldValidator runat=server></{0}:RequiredFieldValidator>")]
    public class RequiredFieldValidator : WebControl
    {
        #region Delcarations
        private string text;
        private string controlToValidate;
        private string validationGroup;
        private string defaultValue;

        #endregion
        #region Properties
        public string Text
        {
            get
            {
                return this.text;
            }

            set
            {
                this.text = value;
            }
        }

        public string ControlToValidate
        {
            get
            {
                return this.controlToValidate;
            }

            set
            {
                this.controlToValidate = value;
            }
        }

        public string ValidationGroup
        {
            get
            {
                return this.validationGroup;
            }

            set
            {
                this.validationGroup = value;
            }
        }

        public string DefaultValue
        {
            get
            {
                return this.defaultValue;
            }

            set
            {
                this.defaultValue = value;
            }
        }

        #endregion
        protected override void Render(System.Web.UI.HtmlTextWriter output)
        {
            if (this.Enabled)
            {
                output.AddAttribute("class", this.CssClass);
                output.AddAttribute("id", this.ClientID);
                output.RenderBeginTag(HtmlTextWriterTag.Span);
                output.RenderEndTag();
                StringBuilder sb = new StringBuilder();
                sb.Append("<script>");
                sb.Append("if(!window.AMPage){window.AMPage=new AMValidator();};");
                Control ctrl = new Control();
                ctrl = this.Parent.FindControl(this.ControlToValidate);

                // Text = Text.Replace("'", "\'")
                sb.Append("AMPage.Add('" + this.ClientID + "','" + ctrl.ClientID + "','" + this.ValidationGroup + "',null,null,null,'" + this.Text + "','" + this.DefaultValue + "');");
                sb.Append("</script>");
                output.Write(sb.ToString());
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ClientResourceManager.RegisterScript(this.Page, Globals.ModulePath + "customcontrols/resources/Validation.js", 102);
        }
    }
}
