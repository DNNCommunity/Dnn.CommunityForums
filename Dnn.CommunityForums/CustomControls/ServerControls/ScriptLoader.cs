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
    using DotNetNuke.Web.Client.ClientResourceManagement;

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
            ClientResourceManager.RegisterScript(this.Page, Globals.ModulePath + "customcontrols/resources/ActiveGrid.js", 102);
            ClientResourceManager.RegisterScript(this.Page, Globals.ModulePath + "customcontrols/resources/cb.js", 102);
            ClientResourceManager.RegisterScript(this.Page, Globals.ModulePath + "customcontrols/resources/datepicker.js", 102);
            ClientResourceManager.RegisterScript(this.Page, Globals.ModulePath + "customcontrols/resources/Validation.js", 102);
            ClientResourceManager.RegisterScript(this.Page, Globals.ModulePath + "customcontrols/resources/MenuButton.js", 102);
        }

        #endregion

    }
}
