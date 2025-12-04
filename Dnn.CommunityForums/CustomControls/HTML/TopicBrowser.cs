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

    public class TopicBrowser
    {
        public int PortalId { get; set; } = -1;

        public int ModuleId { get; set; } = -1;

        public int TabId { get; set; } = -1;

        public string ForumIds { get; set; } = string.Empty;

        public int ForumId { get; set; } = -1;

        public int ForumGroupId { get; set; } = -1;

        public int ParentForumId { get; set; } = -1;

        public int TopicId { get; set; } = -1;

        public string Topic { get; set; } = string.Empty;

        public string Template { get; set; } = string.Empty;

        public string HeaderTemplate { get; set; } = string.Empty;

        public string FooterTemplate { get; set; } = string.Empty;

        public DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo ForumUser { get; set; }

        public int CategoryId { get; set; } = -1;

        public int TagId { get; set; } = -1;

        public int PageIndex { get; set; } = 1;

        public int PageSize { get; set; } = 20;

        public string ItemCss { get; set; } = "aftb-topic";

        public string AltItemCSS { get; set; } = "aftb-topic-alt";

        public string ImagePath { get; set; } = string.Empty;

        public bool MaintainPage { get; set; } = false;

        private ModuleSettings _mainSettings = null;
        private bool _canEdit = false;

        public string Render()
        {
            string fs = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForumsForUser(this.PortalId, this.ModuleId, this.ForumUser, DotNetNuke.Modules.ActiveForums.SecureActions.Edit);
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
            sb.Append(cUtils.BuildPager(this.PortalId, this.TabId, this.ModuleId, groupPrefix, forumPrefix, this.ForumGroupId, this.ForumId, this.TagId, this.CategoryId, otherPrefix, this.PageIndex, pageCount));
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
                            value = Utilities.GetUserFormattedDateTime(Convert.ToDateTime(row[i].ToString()), this.PortalId, this.ForumUser.UserId);
                            break;
                    }

                    tmp = tmp.Replace(k, value);
                }

                ControlUtils cUtils = new ControlUtils();
                DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo auth = new DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo(this.ModuleId);
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

                DotNetNuke.Entities.Portals.PortalSettings portalSettings = Utilities.GetPortalSettings(this.PortalId);
                tmp = tmp.Replace("[TOPICURL]", cUtils.TopicURL(row, this.TabId, this.ModuleId));
                tmp = tmp.Replace("[FORUMURL]", cUtils.ForumURL(row, this.TabId, this.ModuleId));
                if (int.Parse(row["LastAuthorId"].ToString()) == -1)
                {
                    try
                    {
                        tmp = tmp.Replace("[LASTAUTHOR]", DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.GetDisplayName(portalSettings, this._mainSettings, this.ForumUser.GetIsMod(this.ModuleId), this.ForumUser.IsAdmin || this.ForumUser.IsSuperUser, -1, auth.Username, auth.FirstName, auth.LastName, auth.DisplayName));
                    }
                    catch (Exception ex)
                    {
                        tmp = tmp.Replace("[LASTAUTHOR]", "anon");
                    }
                }
                else
                {
                    tmp = tmp.Replace("[LASTAUTHOR]", DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.GetDisplayName(portalSettings, this._mainSettings, this.ForumUser.GetIsMod(this.ModuleId), this.ForumUser.IsAdmin || this.ForumUser.IsSuperUser, int.Parse(row["LastAuthorId"].ToString()), auth.Username, auth.FirstName, auth.LastName, auth.DisplayName));
                }

                if (this._canEdit)
                {
                    tmp = tmp.Replace("[AF:QUICKEDITLINK]", "<span class=\"af-icon16 af-icon16-gear\" onclick=\"amaf_quickEdit(" + this.ModuleId + "," + this.ForumId + row["TopicId"].ToString() + ");\"></span>");
                }
                else
                {
                    tmp = tmp.Replace("[AF:QUICKEDITLINK]", string.Empty);
                }

                tmp = tmp.Replace("[TOPICSTATE]", cUtils.TopicState(row));
                var sAvatar = DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.GetAvatar(portalSettings, auth.AuthorId, this._mainSettings.AvatarWidth, this._mainSettings.AvatarHeight);

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
