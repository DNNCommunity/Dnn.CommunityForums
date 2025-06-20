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
    using System.Data;
    using System.Text;

    public class CategoriesList
    {
        public int PortalId { get; set; } = -1;

        public int ModuleId { get; set; } = -1;

        public int TabId { get; set; } = -1;

        public int ForumId { get; set; } = -1;

        public int ForumGroupId { get; set; } = -1;

        public string SelectedValues { get; set; } = string.Empty;

        public string Template { get; set; } = string.Empty;

        public string HeaderTemplate { get; set; } = string.Empty;

        public string FooterTemplate { get; set; } = string.Empty;

        public int SelectedCategory { get; set; } = -1;

        public string CSSClass { get; set; } = "afn-category";

        public CategoriesList(int portalId, int moduleId)
        {
            this.PortalId = portalId;
            this.ModuleId = moduleId;
        }

        public CategoriesList(int portalId, int moduleId, int forumId, int forumGroupId)
        {
            this.PortalId = portalId;
            this.ModuleId = moduleId;
            this.ForumId = forumId;
            this.ForumGroupId = forumGroupId;
        }

        public string RenderView()
        {
            StringBuilder sb = new StringBuilder();
            DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forumInfo = null;
            string groupPrefix = string.Empty;
            string forumPrefix = string.Empty;
            if (this.ForumId > 0)
            {
                forumInfo = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(this.ForumId, this.ModuleId);
                if (forumInfo != null)
                {
                    groupPrefix = forumInfo.ForumGroup.PrefixURL;
                    forumPrefix = forumInfo.PrefixURL;
                }
            }

            ControlUtils cUtils = new ControlUtils();
            using (IDataReader dr = DataProvider.Instance().Categories_List(this.PortalId, this.ModuleId, 0, 200, "ASC", "CategoryName", this.ForumId, this.ForumGroupId))
            {
                dr.NextResult();
                while (dr.Read())
                {
                    string tmp = this.Template;
                    string categoryName = dr["CategoryName"].ToString();
                    tmp = tmp.Replace("[CATEGORYURL]", cUtils.BuildUrl(this.PortalId, this.TabId, this.ModuleId, groupPrefix, forumPrefix, this.ForumGroupId, this.ForumId, -1, int.Parse(dr["CategoryId"].ToString()), Utilities.CleanName(categoryName), 1, -1, -1));
                    tmp = tmp.Replace("[CATEGORYNAME]", categoryName);
                    if (int.Parse(dr["CategoryId"].ToString()) == this.SelectedCategory)
                    {
                        tmp = tmp.Replace("[CSSCLASS]", this.CSSClass + "-selected");
                    }
                    else
                    {
                        tmp = tmp.Replace("[CSSCLASS]", this.CSSClass);
                    }

                    sb.Append(tmp);
                }

                dr.Close();
            }

            if (sb.Length > 0)
            {
                sb.Insert(0, this.HeaderTemplate);
                sb.Append(this.FooterTemplate);
            }

            return sb.ToString();
        }

        public string RenderEdit()
        {
            StringBuilder sb = new StringBuilder();
            string sSelected = string.Empty;
            using (IDataReader dr = DataProvider.Instance().Categories_List(this.PortalId, this.ModuleId, 0, 200, "ASC", "CategoryName", this.ForumId, this.ForumGroupId))
            {
                dr.NextResult();
                while (dr.Read())
                {
                    sb.Append("<li>");
                    sb.Append("<input type=\"checkbox\"");
                    if (this.IsSelected(Utilities.SafeConvertInt(dr["CategoryId"].ToString())))
                    {
                        sb.Append(" checked=\"checked\" ");
                        sSelected += dr["CategoryId"].ToString() + ";";
                    }

                    sb.Append(" onclick=\"amaf_catSelect(this);\" value=\"" + dr["CategoryId"].ToString() + "\" id=\"amaf_cat_" + dr["CategoryId"].ToString() + "\" />");
                    sb.Append(dr["CategoryName"].ToString());
                    sb.Append("</li>");
                }

                dr.Close();
            }

            if (sb.Length > 0)
            {
                sb.Insert(0, "<ul class=\"af-catlist\">");
                sb.Append("</ul>");
                sb.Append("<input type=\"hidden\" name=\"amaf-catselect\" id=\"amaf-catselect\" value=\"" + sSelected + "\" />");
            }

            return sb.ToString();
        }

        private bool IsSelected(int categoryId)
        {
            if (!string.IsNullOrEmpty(this.SelectedValues))
            {
                foreach (string s in this.SelectedValues.Split(';'))
                {
                    if (!string.IsNullOrEmpty(s))
                    {
                        if (Utilities.SafeConvertInt(s.Trim()) == categoryId)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
