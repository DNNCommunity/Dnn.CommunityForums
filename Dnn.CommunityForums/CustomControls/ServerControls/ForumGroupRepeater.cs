﻿//
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

    [ToolboxData("<{0}:ForumGroupRepeater runat=server></{0}:ForumGroupRepeater>")]
    public class ForumGroupRepeater : ControlsBase
    {
        public enum RepeatDirections : int
        {
            Vertical,
            Horizontal
        }

        private RepeatDirections repeatDirection;
        private int repeatColumns = 1;
        private string headerTemplate;
        private string footerTemplate;
        private string noResults;
        private int toggleBehavior = 0;

        public RepeatDirections RepeatDirection
        {
            get
            {
                return this.repeatDirection;
            }

            set
            {
                this.repeatDirection = value;
            }
        }

        public int RepeatColumns
        {
            get
            {
                return this.repeatColumns;
            }

            set
            {
                this.repeatColumns = value;
            }
        }

        [Description("Template for display"), PersistenceMode(PersistenceMode.InnerProperty)]
        public string HeaderTemplate
        {
            get
            {
                return this.headerTemplate;
            }

            set
            {
                this.headerTemplate = value;
            }
        }

        [Description("Template for display"), PersistenceMode(PersistenceMode.InnerProperty)]
        public string FooterTemplate
        {
            get
            {
                return this.footerTemplate;
            }

            set
            {
                this.footerTemplate = value;
            }
        }

        [Description("Template for display"), PersistenceMode(PersistenceMode.InnerProperty)]
        public string NoResultsTemplate
        {
            get
            {
                return this.noResults;
            }

            set
            {
                this.noResults = value;
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.EnableViewState = false;
        }

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

        protected override void Render(HtmlTextWriter writer)
        {
            // writer.Write(Text)
            writer.Write(this.HeaderTemplate);
            int i = 0;
            if (this.ForumData != null)
            {
                string tmp = this.DisplayTemplate;
                System.Xml.XmlNodeList xGroups = this.ForumData.SelectNodes("//groups/group");
                ForumDisplay fd = null;
                foreach (System.Xml.XmlNode xNode in xGroups)
                {
                    int groupId = int.Parse(xNode.Attributes["groupid"].Value.ToString());
                    fd = new ForumDisplay();
                    fd.DisplayTemplate = this.DisplayTemplate;
                    fd.ForumGroupId = groupId;
                    fd.ControlConfig = this.ControlConfig;
                    fd.ModuleConfiguration = this.ModuleConfiguration;

                    // fd.ForumData = ForumData
                    if (i == 0 && this.ToggleBehavior == 1)
                    {
                        fd.ToggleBehavior = 0;
                    }
                    else if (i > 0 && this.ToggleBehavior == 1)
                    {
                        fd.ToggleBehavior = 1;
                    }

                    this.Controls.Add(fd);
                    fd.RenderControl(writer);
                    i += 1;
                }
            }
            else
            {
                writer.Write(this.NoResultsTemplate);
            }

            writer.Write(this.FooterTemplate);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

        }
    }

}
