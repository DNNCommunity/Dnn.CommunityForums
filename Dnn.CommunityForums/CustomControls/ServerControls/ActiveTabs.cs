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

    [ParseChildren(true), PersistChildren(true), ToolboxData("<{0}:ActiveTabs runat=server></{0}:ActiveTabs>")]
    public class ActiveTabs : System.Web.UI.WebControls.CompositeControl
    {

        // Private _tabs As TabCollection = Nothing
        private List<Tab> tabs;
        private string imagesPath;
        private string targetDiv = string.Empty;
        private string contentHeight;
        private string tabDisplayCSS = "amTabDisplay";
        private string tabStripCSS = "amTabStrip";
        private string tabTextCSS = "amTabText";
        private string tabTextOverCSS = "amTabTextOver";
        private string tabTextSelCSS = "amTabTextSel";
        private string contentBackGround = "FFFFFF";
        private LoadTypes loadType = LoadTypes.Window;

        public enum LoadTypes : int
        {
            Window,
            Shell
        }

        #region  Constructor
        public ActiveTabs()
        {
            this.tabs = new List<Tab>();
        }

        #endregion
        [Description("Tabs to render."), NotifyParentProperty(true), MergableProperty(false), PersistenceMode(PersistenceMode.InnerProperty), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public List<Tab> Tabs // TabCollection
        {
            get
            {
                if (this.tabs == null)
                {
                    this.tabs = new List<Tab>();
                }

                return this.tabs;
            }

            set
            {
                this.tabs = value;
            }
        }

        public string ImagesPath
        {
            get
            {
                return this.imagesPath;
            }

            set
            {
                this.imagesPath = value;
            }
        }

        public string TargetDiv
        {
            get
            {
                return this.targetDiv;
            }

            set
            {
                this.targetDiv = value;
            }
        }

        [Bindable(true), Category("Appearance"), DefaultValue("")]
        public string TabDisplayCSS
        {
            get
            {
                return this.tabDisplayCSS;
            }

            set
            {
                this.tabDisplayCSS = value;
            }
        }

        [Bindable(true), Category("Appearance"), DefaultValue("")]
        public string TabStripCSS
        {
            get
            {
                return this.tabStripCSS;
            }

            set
            {
                this.tabStripCSS = value;
            }
        }

        [Bindable(true), Category("Appearance"), DefaultValue("")]
        public string TabTextCSS
        {
            get
            {
                return this.tabTextCSS;
            }

            set
            {
                this.tabTextCSS = value;
            }
        }

        [Bindable(true), Category("Appearance"), DefaultValue("")]
        public string TabTextOverCSS
        {
            get
            {
                return this.tabTextOverCSS;
            }

            set
            {
                this.tabTextOverCSS = value;
            }
        }

        [Bindable(true), Category("Appearance"), DefaultValue("")]
        public string TabTextSelCSS
        {
            get
            {
                return this.tabTextSelCSS;
            }

            set
            {
                this.tabTextSelCSS = value;
            }
        }

        [Bindable(true), Category("Appearance"), DefaultValue("")]
        public string ContentBackGround
        {
            get
            {
                return this.contentBackGround;
            }

            set
            {
                this.contentBackGround = value;
            }
        }

        public int SelectedIndex
        {
            get
            {
                return Convert.ToInt32((this.ViewState["tabIndex"] == null) ? 0 : this.ViewState["tabIndex"]);
            }

            set
            {
                this.ViewState["tabIndex"] = value;
            }
        }

        public string ContentHeight
        {
            get
            {
                return this.contentHeight;
            }

            set
            {
                this.contentHeight = value;
            }
        }

        public LoadTypes LoadType
        {
            get
            {
                return this.loadType;
            }

            set
            {
                this.loadType = value;
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

        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            if (this.Tabs != null && this.Visible == true)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                int i = 0;
                writer = new HtmlTextWriter(writer, string.Empty);
                this.AddAttributesToRender(writer);
                foreach (Tab tab in this.Tabs)
                {

                    writer.AddAttribute(HtmlTextWriterAttribute.Id, "div" + tab.ControlKey);
                    writer.AddAttribute(HtmlTextWriterAttribute.Onclick, "am_toggleTab(this);");
                    if (i == 0)
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Class, "amtabsel");
                    }
                    else
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Class, "amtab");
                    }

                    writer.RenderBeginTag(HtmlTextWriterTag.Div);
                    writer.AddAttribute(HtmlTextWriterAttribute.Id, "div" + tab.ControlKey + "_text");
                    if (i == 0)
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Class, "amtabseltext");
                    }
                    else
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Class, "amtabtext");
                    }

                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "amtabtext");
                    writer.RenderBeginTag(HtmlTextWriterTag.Div);
                    writer.Write(tab.Text);
                    writer.RenderEndTag();
                    writer.RenderEndTag();
                    i += 1;
                }

                writer.AddAttribute(HtmlTextWriterAttribute.Class, "amtabcontent");
                writer.AddAttribute(HtmlTextWriterAttribute.Id, "amtabcontent");
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
                i = 0;
                foreach (Tab tab in this.Tabs)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Id, "div" + tab.ControlKey + "_amcnt");
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "amtabdisplay");
                    if (i == 0)
                    {
                        writer.AddStyleAttribute("display", "block");
                    }
                    else
                    {
                        writer.AddStyleAttribute("display", "none");
                    }

                    writer.AddStyleAttribute("text-align", "left");
                    writer.RenderBeginTag(HtmlTextWriterTag.Div);
                    if (tab.Content != null)
                    {
                        // For Each ctl As Control In tab.Content.Controls
                        //    ctl.RenderControl(writer)
                        //    'tab.Content.RenderControl(writer)
                        // Next
                        tab.Content.RenderControl(writer);

                    }

                    writer.RenderEndTag();
                    i += 1;
                }

                writer.RenderEndTag();
            }

        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (this.Context == null || this.Page == null)
            {
                return;
            }

            if (this.Tabs != null)
            {
                foreach (var _Tab in this.Tabs)
                {
                    if (_Tab.Content != null)
                    {
                        this.Controls.Add(_Tab.Content);
                    }

                }

                // Try

                // Catch ex As Exception

                // End Try
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (this.Visible == true)
            {

                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.Append("var am_selectedTab;");
                sb.Append("function am_getSelectedTab() {");
                sb.Append("return am_selectedTab;");
                sb.Append("};");
                sb.Append("function am_toggleTab(tab) {");
                sb.Append("if (am_selectedTab != tab.id) {");
                sb.Append("am_selectedTab = tab.id;");
                sb.Append("var obj = document.getElementsByTagName('div');");
                sb.Append("for (var i = 0; i < obj.length; i++) {");
                sb.Append("var el = obj[i];");
                sb.Append("if (el.id.indexOf('_amcnt') > 1) {");
                sb.Append("el.style.display = 'none';");
                sb.Append("};");
                sb.Append("if (el.id.indexOf('_text') > 1) {");
                sb.Append("el.className = 'amtabtext';");
                sb.Append("};");
                sb.Append("if (el.className == 'amtabsel') {");
                sb.Append("el.className = 'amtab';");
                sb.Append("};");
                sb.Append("};");
                sb.Append("var tabContent = document.getElementById(tab.id + '_amcnt');");
                sb.Append("var tabtext = document.getElementById(tab.id + '_text');");
                sb.Append("var tab = document.getElementById(tab.id);");
                sb.Append("tab.className = 'amtabsel';");
                sb.Append("tabtext.className = 'amtabseltext';");
                sb.Append("tabContent.style.display = 'block';");
                sb.Append("};");
                sb.Append("};");

                this.Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "TabScripts", sb.ToString(), true);
            }
        }
    }
}
