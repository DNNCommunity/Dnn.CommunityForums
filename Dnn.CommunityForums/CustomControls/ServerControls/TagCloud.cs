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
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Text;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    [ToolboxData("<{0}:TagCloud runat=server></{0}:TagCloud>")]
    public class TagCloud : WebControl
    {
        public int TabId { get; set; } = -1;

        public int PortalId { get; set; } = -1;

        public int ModuleId { get; set; } = -1;

        public string CSSOne { get; set; } = "tagcssone";

        public string CSSTwo { get; set; } = "tagcsstwo";

        public string CSSThree { get; set; } = "tagcssthree";

        public string ForumIds { get; set; } = string.Empty;

        public int TagCount { get; set; } = 15;

        protected override void Render(HtmlTextWriter writer)
        {
            DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo forumUser = null;
            if (string.IsNullOrEmpty(this.ForumIds))
            {
                forumUser = forumUser = new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ModuleId).GetUserFromHttpContext(this.PortalId, this.ModuleId);
                if (string.IsNullOrEmpty(forumUser.UserForums))
                {
                    this.ForumIds = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForumsForUser(forumUser.UserPermSet, this.PortalId, this.ModuleId);
                }
                else
                {
                    this.ForumIds = forumUser.UserForums;
                }
            }

            SettingsInfo _mainSettings = SettingsBase.GetModuleSettings(this.ModuleId);
            Data.Common db = new Data.Common();
            IDataReader dr = db.TagCloud_Get(this.PortalId, this.ModuleId, this.ForumIds, this.TagCount);
            ControlUtils ctlUtils = new ControlUtils();
            string sURL = string.Empty;
            while (dr.Read())
            {
                int priority = 1;
                string tagName = string.Empty;
                string css = string.Empty;
                priority = int.Parse(dr["Priority"].ToString());
                tagName = dr["TagName"].ToString();
                switch (priority)
                {
                    case 1:
                        css = this.CSSOne;
                        break;
                    case 2:
                        css = this.CSSTwo;
                        break;
                    case 3:
                        css = this.CSSThree;
                        break;
                }

                writer.Write("<span class=\"" + css + "\">");
                writer.Write("<a href=\"");
                sURL = ctlUtils.BuildUrl(this.PortalId, this.TabId, this.ModuleId, string.Empty, string.Empty, -1, -1, int.Parse(dr["TagID"].ToString()), -1, Utilities.CleanName(tagName), 1, -1, -1);
                writer.Write(sURL);
                writer.Write("\" title=\"" + System.Net.WebUtility.HtmlEncode(tagName) + "\">" + tagName + "</a></span> ");
            }

            dr.Close();
            dr.Dispose();
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.EnableViewState = false;
        }
    }
}
