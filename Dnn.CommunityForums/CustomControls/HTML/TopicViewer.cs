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

    public class TopicViewer
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

        private string _ForumIds = string.Empty;

        public string ForumIds
        {
            get
            {
                return this._ForumIds;
            }

            set
            {
                this._ForumIds = value;
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

        private int _ParentForumId = -1;

        public int ParentForumId
        {
            get
            {
                return this._ParentForumId;
            }

            set
            {
                this._ParentForumId = value;
            }
        }

        private int _TopicId = -1;

        public int TopicId
        {
            get
            {
                return this._TopicId;
            }

            set
            {
                this._TopicId = value;
            }
        }

        private string _Topic = string.Empty;

        public string Topic
        {
            get
            {
                return this._Topic;
            }

            set
            {
                this._Topic = value;
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

        public User ForumUser {get; set;}

        private int _PageIndex = 1;

        public int PageIndex
        {
            get
            {
                return this._PageIndex;
            }

            set
            {
                this._PageIndex = value;
            }
        }

        private int _PageSize = 20;

        public int PageSize
        {
            get
            {
                return this._PageSize;
            }

            set
            {
                this._PageSize = value;
            }
        }

        private string _ItemCss = "aftb-topic";

        public string ItemCss
        {
            get
            {
                return this._ItemCss;
            }

            set
            {
                this._ItemCss = value;
            }
        }

        private string _AltItemCSS = "aftb-topic-alt";

        public string AltItemCSS
        {
            get
            {
                return this._AltItemCSS;
            }

            set
            {
                this._AltItemCSS = value;
            }
        }

        public int UserId { get; set; } = -1;

        public string Render()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            Data.Topics db = new Data.Topics();

            using (IDataReader dr = db.TopicWithReplies(this.PortalId, this.TopicId, this.PageIndex, this.PageSize))
            {
                while (dr.Read())
                {
                    this.Template = this.ParseTopic(dr, this.Template);
                }

                dr.NextResult();
                string rtemplate = TemplateUtils.GetTemplateSection(this.Template, "[REPLIES]", "[/REPLIES]");
                while (dr.Read())
                {
                    sb.Append(this.ParseReply(dr, rtemplate));
                }

                dr.Close();
            }

            this.Template = TemplateUtils.ReplaceSubSection(this.Template, sb.ToString(), "[REPLIES]", "[/REPLIES]");
            return this.Template;
        }

        private string ParseTopic(IDataRecord row, string tmp)
        {
            tmp = this.ParseDataRow(row, tmp);
            ControlUtils cUtils = new ControlUtils();
            tmp = tmp.Replace("[TOPICURL]", cUtils.TopicURL(row, this.TabId, this.ModuleId));
            tmp = tmp.Replace("[FORUMURL]", cUtils.ForumURL(row, this.TabId, this.ModuleId));
            tmp = tmp.Replace("[TOPICSTATE]", cUtils.TopicState(row));
            return tmp;
        }

        private string ParseReply(IDataRecord row, string tmp)
        {
            return this.ParseDataRow(row, tmp);
        }

        private string ParseDataRow(IDataRecord row, string tmp)
        {
            try
            {
                for (int i = 0; i < row.FieldCount; i++)
                {
                    string name = row.GetName(i);
                    string k = "[" + name.ToUpperInvariant() + "]";
                    string value = row[i].ToString();
                    switch (row[i].GetType().ToString())
                    {
                        case "System.DateTime":
                            value = Utilities.GetUserFormattedDateTime(Convert.ToDateTime(row[i].ToString()), this.PortalId, this.UserId);
                            break;
                    }

                    tmp = tmp.Replace(k, value);
                }

                return tmp;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }
    }
}
