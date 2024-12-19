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
    using System.Web.UI;

    [DefaultProperty("Text"), ToolboxData("<{0}:ScriptLoader runat=server></{0}:ScriptLoader>")]
    public class ScriptLoader : Control
    {
        #region Properties

        public bool TextSuggest { get; set; }

        public bool ActiveGrid { get; set; }

        public bool Callback { get; set; }

        public bool DatePicker { get; set; }

        public bool RequiredFieldValidator { get; set; }

        #endregion

        public ScriptLoader()
        {
            this.RequiredFieldValidator = false;
            this.DatePicker = false;
            this.Callback = false;
            this.ActiveGrid = false;
            this.TextSuggest = false;
        }

        #region Subs/Functions

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            /*if (TextSuggest && !(Page.ClientScript.IsClientScriptIncludeRegistered("AMTextSuggest")))
            {
                Page.ClientScript.RegisterClientScriptInclude("AMTextSuggest", Page.ClientScript.GetWebResourceUrl(Me.GetType, "TextSuggest.js"))
            }*/
            if (this.ActiveGrid && !this.Page.ClientScript.IsClientScriptIncludeRegistered("AMActiveGrid"))
            {
                this.Page.ClientScript.RegisterClientScriptInclude("AMActiveGrid", this.Page.ClientScript.GetWebResourceUrl(this.GetType(), "DotNetNuke.Modules.ActiveForums.CustomControls.Resources.ActiveGrid.js"));
            }

            if (this.Callback && !this.Page.ClientScript.IsClientScriptIncludeRegistered("AMCallback"))
            {
                this.Page.ClientScript.RegisterClientScriptInclude("AMCallback", this.Page.ClientScript.GetWebResourceUrl(this.GetType(), "DotNetNuke.Modules.ActiveForums.CustomControls.Resources.cb.js"));
            }

            if (this.DatePicker && !this.Page.ClientScript.IsClientScriptIncludeRegistered("AMDatePicker"))
            {
                this.Page.ClientScript.RegisterClientScriptInclude("AMDatePicker", this.Page.ClientScript.GetWebResourceUrl(this.GetType(), "DotNetNuke.Modules.ActiveForums.CustomControls.Resources.DatePicker.js"));
            }

            if (this.RequiredFieldValidator && !this.Page.ClientScript.IsClientScriptIncludeRegistered("AMValidation"))
            {
                this.Page.ClientScript.RegisterClientScriptInclude("AMValidation", this.Page.ClientScript.GetWebResourceUrl(this.GetType(), "DotNetNuke.Modules.ActiveForums.CustomControls.Resources.Validation.js"));
            }

            if (!this.Page.ClientScript.IsClientScriptIncludeRegistered("AMMenu"))
            {
                this.Page.ClientScript.RegisterClientScriptInclude("AMMenu", this.Page.ClientScript.GetWebResourceUrl(this.GetType(), "DotNetNuke.Modules.ActiveForums.CustomControls.Resources.MenuButton.js"));
            }
        }

        #endregion

    }
}
