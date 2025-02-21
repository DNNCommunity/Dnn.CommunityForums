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
    using System.Data;

    public class TopicViewer
    {
        private int portalId = -1;

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

        private int moduleId = -1;

        public int ModuleId
        {
            get
            {
                return this.moduleId;
            }

            set
            {
                this.moduleId = value;
            }
        }

        private int tabId = -1;

        public int TabId
        {
            get
            {
                return this.tabId;
            }

            set
            {
                this.tabId = value;
            }
        }

        private string forumIds = string.Empty;

        public string ForumIds
        {
            get
            {
                return this.forumIds;
            }

            set
            {
                this.forumIds = value;
            }
        }

        private int forumGroupId = -1;

        public int ForumGroupId
        {
            get
            {
                return this.forumGroupId;
            }

            set
            {
                this.forumGroupId = value;
            }
        }

        private int parentForumId = -1;

        public int ParentForumId
        {
            get
            {
                return this.parentForumId;
            }

            set
            {
                this.parentForumId = value;
            }
        }

        private int topicId = -1;

        public int TopicId
        {
            get
            {
                return this.topicId;
            }

            set
            {
                this.topicId = value;
            }
        }

        private string topic = string.Empty;

        public string Topic
        {
            get
            {
                return this.topic;
            }

            set
            {
                this.topic = value;
            }
        }

        private string template = string.Empty;

        public string Template
        {
            get
            {
                return this.template;
            }

            set
            {
                this.template = value;
            }
        }

        private string headerTemplate = string.Empty;

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

        private string footerTemplate = string.Empty;

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

        public User ForumUser { get; set; }

        private int pageIndex = 1;

        public int PageIndex
        {
            get
            {
                return this.pageIndex;
            }

            set
            {
                this.pageIndex = value;
            }
        }

        private int pageSize = 20;

        public int PageSize
        {
            get
            {
                return this.pageSize;
            }

            set
            {
                this.pageSize = value;
            }
        }

        private string itemCss = "aftb-topic";

        public string ItemCss
        {
            get
            {
                return this.itemCss;
            }

            set
            {
                this.itemCss = value;
            }
        }

        private string altItemCSS = "aftb-topic-alt";

        public string AltItemCSS
        {
            get
            {
                return this.altItemCSS;
            }

            set
            {
                this.altItemCSS = value;
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
