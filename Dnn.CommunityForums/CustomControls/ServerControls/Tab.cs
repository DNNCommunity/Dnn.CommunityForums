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
    public class Tab
    {
        private string text;
        private string cSSClass;
        private string onClick = string.Empty;
        private string controlKey = string.Empty;
        private TabContent tabContent;

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

        public string CSSClass
        {
            get
            {
                return this.cSSClass;
            }

            set
            {
                this.cSSClass = value;
            }
        }

        public string OnClientClick
        {
            get
            {
                return this.onClick;
            }

            set
            {
                this.onClick = value;
            }
        }

        public string ControlKey
        {
            get
            {
                return this.controlKey;
            }

            set
            {
                this.controlKey = value;
            }
        }

        public TabContent Content
        {
            get
            {
                return this.tabContent;
            }

            set
            {
                this.tabContent = value;
            }
        }
    }

    public class TabContent : System.Web.UI.Control
    {
    }
}
