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

    [ParseChildren(true, ""), ToolboxData("<{0}:toggle runat=server></{0}:toggle>")]
    public class Toggle : WebControl
    {
        private string key;
        private string imagePath;
        private string cssClassOn;
        private string cssClassOff;
        private int toggleBehavior;
        private bool isVisible = true;

        public int ToggleBehavior
        {
            get
            {
                return this.toggleBehavior;
            }

            set
            {
                this.toggleBehavior = value;
            }
        }

        public string Key
        {
            get
            {
                return this.key;
            }

            set
            {
                this.key = value;
            }
        }

        public string ImagePath
        {
            get
            {
                return this.imagePath;
            }

            set
            {
                this.imagePath = value;
            }
        }

        public string CssClassOn
        {
            get
            {
                return this.cssClassOn;
            }

            set
            {
                this.cssClassOn = value;
            }
        }

        public string CssClassOff
        {
            get
            {
                return this.cssClassOff;
            }

            set
            {
                this.cssClassOff = value;
            }
        }

        public bool IsVisible
        {
            get
            {
                return this.isVisible;
            }

            set
            {
                this.isVisible = value;
            }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (this.IsVisible)
            {
                writer.Write("<div id=\"imgGroup" + this.Key + "\" class=\"" + this.CssClassOn + "\" onclick=\"toggleGroup('" + this.Key + "','" + this.CssClassOn + "','" + this.CssClassOff + "');\"></div>");
            }
            else
            {
                writer.Write("<div id=\"imgGroup" + this.Key + "\" class=\"" + this.CssClassOff + "\" onclick=\"toggleGroup('" + this.Key + "','" + this.CssClassOn + "','" + this.CssClassOff + "');\"></div>");
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.EnableViewState = false;
        }
    }

    [ParseChildren(true, ""), ToolboxData("<{0}:toggledisplay runat=server></{0}:toggledisplay>")]
    public class ToggleDisplay : CompositeControl
    {
        private ToggleContent content;
        private string key;
        private bool isVisible = true;

        public bool IsVisible
        {
            get
            {
                return this.isVisible;
            }

            set
            {
                this.isVisible = value;
            }
        }

        public string Key
        {
            get
            {
                return this.key;
            }

            set
            {
                this.key = value;
            }
        }

        public override System.Web.UI.ControlCollection Controls
        {
            get
            {
                this.EnsureChildControls();
                return base.Controls;
            }
        }

        protected override void CreateChildControls()
        {
            if (this.Content != null)
            {
                this.Controls.Clear();
                this.Controls.Add(this.Content);
            }
        }

        [Description("Initial content to render."), DefaultValue(null, ""), Browsable(false), NotifyParentProperty(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), PersistenceMode(PersistenceMode.InnerProperty)]
        public ToggleContent Content
        {
            get
            {
                this.EnsureChildControls();
                return this.content;
            }

            set
            {
                this.content = value;
            }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            writer.Write("<div id=\"group" + this.Key + "\" class=\"" + this.CssClass + "\" style=\"display:");
            if (this.IsVisible)
            {
                writer.Write("block");
            }
            else
            {
                writer.Write("none");
            }

            writer.Write(";\">");
            try
            {
                this.Content.RenderControl(writer);
            }
            catch (Exception ex)
            {
            }

            writer.Write("</div>");

            // writer.Write(Text)
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (this.Context == null || this.Page == null)
            {
                return;
            }

            if (this.Content != null)
            {
                this.Controls.Add(this.Content);
            }

            // EnableViewState = False
        }
    }

    [ToolboxItem(false)]
    public class ToggleContent : Control
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // EnableViewState = False
        }
    }
}
