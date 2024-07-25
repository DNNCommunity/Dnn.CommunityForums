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

    public class TopicBrowser
    {
        private int _portalId = -1;

        public int PortalId
        {
            get
            {
                return this._portalId;
            }

            set
            {
                this._portalId = value;
            }
        }

        private int _moduleId = -1;

        public int ModuleId
        {
            get
            {
                return this._moduleId;
            }

            set
            {
                this._moduleId = value;
            }
        }

        private int _tabId = -1;

        public int TabId
        {
            get
            {
                return this._tabId;
            }

            set
            {
                this._tabId = value;
            }
        }

        private string _forumIds = string.Empty;

        public string ForumIds
        {
            get
            {
                return this._forumIds;
            }

            set
            {
                this._forumIds = value;
            }
        }

        private int _forumId = -1;

        public int ForumId
        {
            get
            {
                return this._forumId;
            }

            set
            {
                this._forumId = value;
            }
        }

        private int _forumGroupId = -1;

        public int ForumGroupId
        {
            get
            {
                return this._forumGroupId;
            }

            set
            {
                this._forumGroupId = value;
            }
        }

        private int _parentForumId = -1;

        public int ParentForumId
        {
            get
            {
                return this._parentForumId;
            }

            set
            {
                this._parentForumId = value;
            }
        }

        private int _topicId = -1;

        public int TopicId
        {
            get
            {
                return this._topicId;
            }

            set
            {
                this._topicId = value;
            }
        }

        private string _topic = string.Empty;

        public string Topic
        {
            get
            {
                return this._topic;
            }

            set
            {
                this._topic = value;
            }
        }

        private string _template = string.Empty;

        public string Template
        {
            get
            {
                return this._template;
            }

            set
            {
                this._template = value;
            }
        }

        private string _headerTemplate = string.Empty;

        public string HeaderTemplate
        {
            get
            {
                return this._headerTemplate;
            }

            set
            {
                this._headerTemplate = value;
            }
        }

        private string _footerTemplate = string.Empty;

        public string FooterTemplate
        {
            get
            {
                return this._footerTemplate;
            }

            set
            {
                this._footerTemplate = value;
            }
        }

        public User ForumUser { get; set; }

        private int _categoryId = -1;

        public int CategoryId
        {
            get
            {
                return this._categoryId;
            }

            set
            {
                this._categoryId = value;
            }
        }

        private int _tagId = -1;

        public int TagId
        {
            get
            {
                return this._tagId;
            }

            set
            {
                this._tagId = value;
            }
        }

        private int _pageIndex = 1;

        public int PageIndex
        {
            get
            {
                return this._pageIndex;
            }

            set
            {
                this._pageIndex = value;
            }
        }

        private int _pageSize = 20;

        public int PageSize
        {
            get
            {
                return this._pageSize;
            }

            set
            {
                this._pageSize = value;
            }
        }

        private string _itemCss = "aftb-topic";

        public string ItemCss
        {
            get
            {
                return this._itemCss;
            }

            set
            {
                this._itemCss = value;
            }
        }

        private string _altItemCSS = "aftb-topic-alt";

        public string AltItemCSS
        {
            get
            {
                return this._altItemCSS;
            }

            set
            {
                this._altItemCSS = value;
            }
        }

        private bool _useAjax = false;

        public bool UseAjax
        {
            get
            {
                return this._useAjax;
            }

            set
            {
                this._useAjax = value;
            }
        }

        private string _imagePath = string.Empty;

        public string ImagePath
        {
            get
            {
                return this._imagePath;
            }

            set
            {
                this._imagePath = value;
            }
        }

        private bool _maintainPage = false;

        public bool MaintainPage
        {
            get
            {
                return this._maintainPage;
            }

            set
            {
                this._maintainPage = value;
            }
        }

        private SettingsInfo _mainSettings = null;
        private bool _canEdit = false;

        public int UserId { get; set; } = -1;

        public string Render()
        {
            string fs = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForumsForUser(this.ForumUser.UserRoles, this.PortalId, this.ModuleId, "CanEdit");
            if (!string.IsNullOrEmpty(fs))
            {
                this._canEdit = true;
            }

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            string forumPrefix = string.Empty;
            string groupPrefix = string.Empty;
            this._mainSettings = SettingsBase.GetModuleSettings(this.ModuleId);
            if (this._mainSettings.URLRewriteEnabled)
            {
                if (this.ForumId > 0)
                {
                    DotNetNuke.Modules.ActiveForums.Entities.ForumInfo f = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(this.ForumId, this.ModuleId);
                    if (f != null)
                    {
                        forumPrefix = f.PrefixURL;
                        groupPrefix = f.ForumGroup.PrefixURL;
                    }
                }
                else if (this.ForumGroupId > 0)
                {
                    DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo g = new DotNetNuke.Modules.ActiveForums.Controllers.ForumGroupController().GetById(this.ForumGroupId, this.ModuleId);
                    if (g != null)
                    {
                        groupPrefix = g.PrefixURL;
                    }
                }
            }

            string tmp = string.Empty;
            Data.Topics db = new Data.Topics();
            int recordCount = 0;
            int i = 0;
            sb.Append(this.HeaderTemplate);
            using (IDataReader dr = db.TopicsList(this.PortalId, this.PageIndex, this.PageSize, this.ForumIds, this.CategoryId, this.TagId))
            {
                while (dr.Read())
                {
                    if (recordCount == 0)
                    {
                        recordCount = int.Parse(dr["RecordCount"].ToString());
                    }

                    tmp = this.ParseDataRow(dr, this.Template);
                    if (i % 2 == 0)
                    {
                        tmp = tmp.Replace("[ROWCSS]", this.ItemCss);
                    }
                    else
                    {
                        tmp = tmp.Replace("[ROWCSS]", this.AltItemCSS);
                    }

                    i += 1;
                    sb.Append(tmp);
                }

                dr.Close();
            }

            sb.Append(this.FooterTemplate);
            int pageCount = Convert.ToInt32(System.Math.Ceiling((double)recordCount / this.PageSize));
            ControlUtils cUtils = new ControlUtils();
            int id = this.TagId > 0 ? this.TagId : this.CategoryId;
            string otherPrefix = id > 0
                ? Utilities.CleanName(new DotNetNuke.Modules.ActiveForums.Controllers.TagController().GetById(id).TagName)
                : string.Empty;
            sb.Append(cUtils.BuildPager(this.TabId, this.ModuleId, groupPrefix, forumPrefix, this.ForumGroupId, this.ForumId, this.TagId, this.CategoryId, otherPrefix, this.PageIndex, pageCount));
            return sb.ToString();
        }

        private string ParseDataRow(IDataRecord row, string tmp)
        {
            try
            {
                tmp = tmp.Replace("[AVATAR]", "[AF:AVATAR]");
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

                ControlUtils cUtils = new ControlUtils();
                Author auth = new Author();
                string columnPrefix = "Topic";
                if (Convert.ToInt32(row["ReplyId"].ToString()) > 0)
                {
                    columnPrefix = "Reply";
                    auth.DisplayName = row[columnPrefix + "AuthorDisplayName"].ToString();
                }
                else
                {
                    auth.DisplayName = row["TopicAuthorName"].ToString();
                }

                auth.AuthorId = int.Parse(row[columnPrefix + "AuthorId"].ToString());

                auth.LastName = row[columnPrefix + "AuthorLastName"].ToString();
                auth.FirstName = row[columnPrefix + "AuthorFirstName"].ToString();
                auth.Username = row[columnPrefix + "AuthorUsername"].ToString();

                tmp = tmp.Replace("[TOPICURL]", cUtils.TopicURL(row, this.TabId, this.ModuleId));
                tmp = tmp.Replace("[FORUMURL]", cUtils.ForumURL(row, this.TabId, this.ModuleId));
                if (int.Parse(row["LastAuthorId"].ToString()) == -1)
                {
                    try
                    {
                        DotNetNuke.Entities.Portals.PortalSettings portalSettings = Utilities.GetPortalSettings(this.PortalId);
                        tmp = tmp.Replace("[LASTAUTHOR]", UserProfiles.GetDisplayName(portalSettings, this.ModuleId, true, this.ForumUser.Profile.IsMod, this.ForumUser.IsAdmin || this.ForumUser.IsSuperUser, -1, auth.Username, auth.FirstName, auth.LastName, auth.DisplayName));
                    }
                    catch (Exception ex)
                    {
                        tmp = tmp.Replace("[LASTAUTHOR]", "anon");
                    }

                }
                else
                {
                    DotNetNuke.Entities.Portals.PortalSettings portalSettings = Utilities.GetPortalSettings(this.PortalId);
                    tmp = tmp.Replace("[LASTAUTHOR]", UserProfiles.GetDisplayName(portalSettings, this.ModuleId, true, this.ForumUser.Profile.IsMod, this.ForumUser.IsAdmin || this.ForumUser.IsSuperUser, int.Parse(row["LastAuthorId"].ToString()), auth.Username, auth.FirstName, auth.LastName, auth.DisplayName));
                }

                if (this._canEdit)
                {
                    tmp = tmp.Replace("[AF:QUICKEDITLINK]", "<span class=\"af-icon16 af-icon16-gear\" onclick=\"amaf_quickEdit(" + this.ModuleId + "," + this.ForumId + row["TopicId"].ToString() + ");\"></span>");
                }
                else
                {
                    tmp = tmp.Replace("[AF:QUICKEDITLINK]", string.Empty);
                }

                //

                tmp = tmp.Replace("[TOPICSTATE]", cUtils.TopicState(row));
                var sAvatar = UserProfiles.GetAvatar(auth.AuthorId, this._mainSettings.AvatarWidth, this._mainSettings.AvatarHeight);

                tmp = tmp.Replace("[AF:AVATAR]", sAvatar);
                return tmp;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }

    }
}
