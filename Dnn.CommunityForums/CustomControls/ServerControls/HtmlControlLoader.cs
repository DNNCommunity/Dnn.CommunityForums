﻿// Copyright (c) by DNN Community
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
    using System.ComponentModel;
    using System.Web.UI;

    [DefaultProperty("Text"), ToolboxData("<{0}:HtmlControlLoader runat=server></{0}:HtmlControlLoader>")]
    public class HtmlControlLoader : Control
    {
        public string ControlId { get; set; }

        public string Height { get; set; }

        public string Width { get; set; }

        public string Name { get; set; }

        public string FilePath { get; set; }

        protected override void Render(HtmlTextWriter writer)
        {
            this.EnableViewState = false;
            this.FilePath = DotNetNuke.Modules.ActiveForums.Utilities.MapPath(this.FilePath);
            string sControl = Utilities.GetFile(this.FilePath);
            sControl = sControl.Replace("{id}", this.ControlId);
            sControl = sControl.Replace("{height}", this.Height);
            sControl = sControl.Replace("{width}", this.Width);
            sControl = sControl.Replace("{name}", this.Name);
            writer.Write(sControl);
        }
    }
}
