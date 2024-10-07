// Copyright (c) 2013-2024 by DNN Community
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
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Text;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    [ToolboxData("<{0}:TopMembers runat=server></{0}:TopMembers>")]
    public class TopMembers : WebControl
    {
        private int portalId = -1;
        private int rows = 10;
        private DisplayTemplate itemTemplate;
        private DisplayTemplate headerTemplate;
        private DisplayTemplate footerTemplate;

        public int PortalId
        {
            get
            {
                return this.portalId;
            }

            set
            {
                this.portalId = value;
            }
        }

        public int Rows
        {
            get
            {
                return this.rows;
            }

            set
            {
                this.rows = value;
            }
        }

        [Description("Template for display"), PersistenceMode(PersistenceMode.InnerProperty)]
        public DisplayTemplate ItemTemplate
        {
            get
            {
                return this.itemTemplate;
            }

            set
            {
                this.itemTemplate = value;
            }
        }

        [Description("Template for display"), PersistenceMode(PersistenceMode.InnerProperty)]
        public DisplayTemplate HeaderTemplate
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
        public DisplayTemplate FooterTemplate
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

        protected override void Render(HtmlTextWriter writer)
        {
            string sHeaderTemplate = string.Empty;
            string sFooterTemplate = string.Empty;
            if (this.HeaderTemplate != null)
            {
                sHeaderTemplate = this.HeaderTemplate.Text;
            }

            if (this.FooterTemplate != null)
            {
                sFooterTemplate = this.FooterTemplate.Text;
            }

            string sTemplate = "[DISPLAYNAME]";
            Data.Common db = new Data.Common();
            IDataReader dr = db.TopMembers_Get(this.PortalId, this.Rows);
            if (this.ItemTemplate != null)
            {
                sTemplate = this.ItemTemplate.Text;
            }

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            if (string.IsNullOrEmpty(this.CssClass))
            {
                this.CssClass = "aflist2";
            }

            while (dr.Read())
            {
                string sOut = sTemplate;
                sOut = sOut.Replace("[DISPLAYNAME]", dr["DisplayName"].ToString());
                sb.Append(sOut);
            }

            dr.Close();
            dr.Dispose();
            if (!string.IsNullOrEmpty(sb.ToString()))
            {
                writer.Write(sHeaderTemplate);
                writer.Write(sb.ToString());
                writer.Write(sFooterTemplate);
            }
        }
    }
}
