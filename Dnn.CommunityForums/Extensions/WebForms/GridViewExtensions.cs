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

namespace DotNetNuke.Modules.ActiveForums.Extensions.WebForms
{
    using System.Data;
    using System.Resources;
    using System.Web.UI.WebControls;

    using DotNetNuke.Web.Client;
    using DotNetNuke.Web.Client.ClientResourceManagement;

    public static class GridViewExtensions
    {
        public static void WrapGridViewInDataTableNet(this GridView gridView, Abstractions.Portals.IPortalSettings portalSettings, DotNetNuke.Entities.Users.UserInfo userInfo)
        {
            if (gridView.Rows.Count > 0)
            {
                ClientResourceManager.RegisterScript(gridView.Page, Globals.ModulePath + "Resources/datatables.net-2.3.3/Scripts/dataTables.min.js", FileOrder.Js.DefaultPriority + 2, "DnnPageHeaderProvider");
                ClientResourceManager.RegisterScript(gridView.Page, Globals.ModulePath + "Resources/datatables.net-dt-2.3.3/Scripts/dataTables.dataTables.min.js", FileOrder.Js.DefaultPriority + 3, "DnnPageHeaderProvider");
                ClientResourceManager.RegisterStyleSheet(gridView.Page, Globals.ModulePath + "Resources/datatables.net-dt-2.3.3/Content/dataTables.dataTables.min.css", priority: 11);
                gridView.HeaderRow.TableSection = TableRowSection.TableHeader;
                var lang = DetermineAlternateCultureLanguage(gridView, portalSettings, userInfo);
                var sb = new System.Text.StringBuilder();
                sb.Append("$(document).ready(function () { new DataTable('#" + gridView.ClientID + "'");
                if (!string.IsNullOrEmpty(lang) && !lang.Equals("en-US"))
                {
                    sb.Append(", { language: { url: '" + gridView.Page.ResolveUrl(Globals.ModulePath + "Resources/datatables.net-plugins-2.3.3/i18n/" + lang + ".json") + "' } }");
                }

                sb.Append("); } );");
                gridView.Page.ClientScript.RegisterStartupScript(gridView.Page.GetType(), "dcfgridviewwrapper", sb.ToString(), true);
            }
        }

        private static string DetermineAlternateCultureLanguage(GridView gridView, Abstractions.Portals.IPortalSettings portalSettings, DotNetNuke.Entities.Users.UserInfo userInfo)
        {
            var lang = string.Empty;
            if (gridView.Page.Request.QueryString["language"] != null)
            {
                lang = gridView.Page.Request.QueryString["language"];
            }

            if (string.IsNullOrEmpty(lang))
            {
                lang = userInfo.Profile.PreferredLocale;
            }

            if (string.IsNullOrEmpty(lang))
            {
                lang = portalSettings.DefaultLanguage;
            }

            return lang;
        }
    }
}
