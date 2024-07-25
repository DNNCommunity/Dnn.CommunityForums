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

    [DefaultProperty("Text"), ToolboxData("<{0}:RequiredFieldValidator runat=server></{0}:RequiredFieldValidator>")]
    public class RequiredFieldValidator : WebControl
    {
        #region Delcarations
        private string _Text;
        private string _ControlToValidate;
        private string _ValidationGroup;
        private string _DefaultValue;

        #endregion
        #region Properties
        public string Text
        {
            get
            {
                return this._Text;
            }

            set
            {
                this._Text = value;
            }
        }

        public string ControlToValidate
        {
            get
            {
                return this._ControlToValidate;
            }

            set
            {
                this._ControlToValidate = value;
            }
        }

        public string ValidationGroup
        {
            get
            {
                return this._ValidationGroup;
            }

            set
            {
                this._ValidationGroup = value;
            }
        }

        public string DefaultValue
        {
            get
            {
                return this._DefaultValue;
            }

            set
            {
                this._DefaultValue = value;
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
                //Text = Text.Replace("'", "\'")
                sb.Append("AMPage.Add('" + this.ClientID + "','" + ctrl.ClientID + "','" + this.ValidationGroup + "',null,null,null,'" + this.Text + "','" + this.DefaultValue + "');");
                sb.Append("</script>");
                output.Write(sb.ToString());
            }

        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!this.Page.ClientScript.IsClientScriptIncludeRegistered("AMValidation"))
            {
                this.Page.ClientScript.RegisterClientScriptInclude("AMValidation", this.Page.ClientScript.GetWebResourceUrl(this.GetType(), "DotNetNuke.Modules.ActiveForums.CustomControls.Resources.Validation.js"));
            }
        }
    }
}
