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
    using System.Data;
    using System.Text;

    public class CategoriesList
    {
        private int _PortalId = -1;

        public int PortalId
        {
            get
            {
                return this._PortalId;
            }

            set
            {
                this._PortalId = value;
            }
        }

        private int _ModuleId = -1;

        public int ModuleId
        {
            get
            {
                return this._ModuleId;
            }

            set
            {
                this._ModuleId = value;
            }
        }

        private int _TabId = -1;

        public int TabId
        {
            get
            {
                return this._TabId;
            }

            set
            {
                this._TabId = value;
            }
        }

        private int _ForumId = -1;

        public int ForumId
        {
            get
            {
                return this._ForumId;
            }

            set
            {
                this._ForumId = value;
            }
        }

        private int _ForumGroupId = -1;

        public int ForumGroupId
        {
            get
            {
                return this._ForumGroupId;
            }

            set
            {
                this._ForumGroupId = value;
            }
        }

        private string _SelectedValues = string.Empty;

        public string SelectedValues
        {
            get
            {
                return this._SelectedValues;
            }

            set
            {
                this._SelectedValues = value;
            }
        }

        private string _Template = string.Empty;

        public string Template
        {
            get
            {
                return this._Template;
            }

            set
            {
                this._Template = value;
            }
        }

        private string _HeaderTemplate = string.Empty;

        public string HeaderTemplate
        {
            get
            {
                return this._HeaderTemplate;
            }

            set
            {
                this._HeaderTemplate = value;
            }
        }

        private string _FooterTemplate = string.Empty;

        public string FooterTemplate
        {
            get
            {
                return this._FooterTemplate;
            }

            set
            {
                this._FooterTemplate = value;
            }
        }

        private int _SelectedCategory = -1;

        public int SelectedCategory
        {
            get
            {
                return this._SelectedCategory;
            }

            set
            {
                this._SelectedCategory = value;
            }
        }

        private string _CSSClass = "afn-category";

        public string CSSClass
        {
            get
            {
                return this._CSSClass;
            }

            set
            {
                this._CSSClass = value;
            }
        }

        public CategoriesList(int PortalId, int ModuleId)
        {
            this.PortalId = PortalId;
            this.ModuleId = ModuleId;
        }

        public CategoriesList(int PortalId, int ModuleId, int ForumId, int ForumGroupId)
        {
            this.PortalId = PortalId;
            this.ModuleId = ModuleId;
            this.ForumId = ForumId;
            this.ForumGroupId = ForumGroupId;
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
            using (IDataReader dr = DataProvider.Instance().Tags_List(this.PortalId, this.ModuleId, true, 0, 200, "ASC", "TagName", this.ForumId, this.ForumGroupId))
            {
                dr.NextResult();
                while (dr.Read())
                {
                    string tmp = this.Template;
                    string categoryName = dr["TagName"].ToString();
                    tmp = tmp.Replace("[CATEGORYURL]", cUtils.BuildUrl(this.TabId, this.ModuleId, groupPrefix, forumPrefix, this.ForumGroupId, this.ForumId, -1, int.Parse(dr["TagId"].ToString()), Utilities.CleanName(categoryName), 1, -1, -1));
                    tmp = tmp.Replace("[CATEGORYNAME]", categoryName);
                    if (int.Parse(dr["TagId"].ToString()) == this.SelectedCategory)
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
            using (IDataReader dr = DataProvider.Instance().Tags_List(this.PortalId, this.ModuleId, true, 0, 200, "ASC", "TagName", this.ForumId, this.ForumGroupId))
            {
                dr.NextResult();
                while (dr.Read())
                {
                    sb.Append("<li>");
                    sb.Append("<input type=\"checkbox\"");
                    if (this.IsSelected(Utilities.SafeConvertInt(dr["TagId"].ToString())))
                    {
                        sb.Append(" checked=\"checked\" ");
                        sSelected += dr["TagId"].ToString() + ";";
                    }

                    sb.Append(" onclick=\"amaf_catSelect(this);\" value=\"" + dr["TagId"].ToString() + "\" id=\"amaf_cat_" + dr["TagId"].ToString() + "\" />");
                    sb.Append(dr["TagName"].ToString());
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

        private bool IsSelected(int CategoryId) /* Category is stored in Tag table with 'isCategory' = true */
        {
            if (string.IsNullOrEmpty(this.SelectedValues))
            {
                return false;
            }
            else
            {
                foreach (string s in this.SelectedValues.Split(';'))
                {
                    if (! string.IsNullOrEmpty(s))
                    {
                        if (Utilities.SafeConvertInt(s.Trim()) == CategoryId)
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
