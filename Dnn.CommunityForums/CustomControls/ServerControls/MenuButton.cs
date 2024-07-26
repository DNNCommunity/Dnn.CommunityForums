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
    using System.ComponentModel;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    [DefaultProperty("Text"), ToolboxData("<{0}:MenuButton runat=server></{0}:MenuButton>")]
    public class MenuButton : WebControl
    {
        public enum ExpandDirections
        {
            DownRight,
            DownLeft,
            UpRight,
            UpLeft,
        }

        #region Private Member Variables

        private MenuContent menu;

        #endregion

        #region Properties

        public MenuContent Menu
        {
            get
            {
                this.EnsureChildControls();
                return this.menu ?? (this.menu = new MenuContent());
            }

            set
            {
                this.menu = value;
            }
        }

        public string Text { get; set; }

        public string MenuCss { get; set; }

        public int MenuWidth { get; set; }

        public int MenuHeight { get; set; }

        public string MenuOverflow { get; set; }

        public int AnimationSteps { get; set; }

        public int AnimationDelay { get; set; }

        public int OffsetLeft { get; set; }

        public int OffsetTop { get; set; }

        public ExpandDirections ExpandDirection { get; set; }

        #endregion

        public MenuButton()
        {
            this.MenuCss = null;
            this.MenuWidth = 100;
            this.MenuHeight = 100;
            this.MenuOverflow = "hidden";
            this.OffsetTop = 16;
            this.OffsetLeft = 0;
            this.AnimationSteps = 5;
            this.AnimationDelay = 20;
            this.ExpandDirection = ExpandDirections.DownRight;
        }

        protected override void Render(HtmlTextWriter output)
        {
            output.AddAttribute("class", this.CssClass);
            output.AddAttribute("onclick", "window." + this.ClientID + ".Toggle()");
            output.AddAttribute("id", this.ClientID);
            output.RenderBeginTag(HtmlTextWriterTag.Div);
            output.Write(this.Text);
            output.RenderEndTag();

            output.AddAttribute("class", this.MenuCss);
            output.AddStyleAttribute("position", "absolute");
            output.AddStyleAttribute("display", "none");
            output.AddStyleAttribute("overflow", this.MenuOverflow);
            output.AddAttribute("id", this.ClientID + "_div");
            output.RenderBeginTag(HtmlTextWriterTag.Div);

            if (this.Menu != null)
            {
                this.Menu.RenderControl(output);
            }

            output.RenderEndTag();

            var script = "<script type=\"text/javascript\">window." + this.ClientID + "=new ActiveMenuButton('" + this.ClientID + "'," + this.MenuWidth + "," + this.MenuHeight + "," + this.AnimationSteps + "," + this.AnimationDelay + "," + this.OffsetTop + "," + this.OffsetLeft + "," + (int)this.ExpandDirection + ");</script>";
            output.Write(script);
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (this.Menu == null)
            {
                return;
            }

            this.Controls.Clear();
            this.Controls.Add(this.Menu);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!this.Page.ClientScript.IsClientScriptIncludeRegistered("AMMenu"))
            {
                this.Page.ClientScript.RegisterClientScriptInclude("AMMenu", this.Page.ClientScript.GetWebResourceUrl(this.GetType(), "DotNetNuke.Modules.ActiveForums.CustomControls.Resources.MenuButton.js"));
            }
        }

        public class MenuContent : Control { }
    }

}
