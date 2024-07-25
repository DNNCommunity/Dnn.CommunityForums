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

namespace DotNetNuke.Modules.ActiveForums
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Xml;

    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Localization;

    public class ForumBase : SettingsBase
    {
        #region Private Member Variables

        private int? _forumId;
        private int? _forumGroupId;
        private int? _postId;
        private int? _topicId; // = -1;
        private int? _replyId;
        private int? _quoteId;
        private int? _authorid;
        private bool? _jumpToLastPost;
        private string _templatePath = string.Empty;
        private DotNetNuke.Modules.ActiveForums.Entities.ForumInfo _foruminfo;
        private XmlDocument _forumData;

        private bool? _canRead;
        private bool? _canView;
        private bool? _canCreate;
        private bool? _canReply;

        #endregion

        #region Public Properties

        public XmlDocument ForumData
        {
            get => this._forumData ?? (this._forumData = this.ControlConfig != null ? new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetForumListXML(this.ControlConfig.PortalId, this.ControlConfig.ModuleId) : new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetForumListXML(this.PortalId, this.ModuleId));
            set => this._forumData = value;
        }

        public ControlsConfig ControlConfig { get; set; }

        public string ThemePath => this.Page.ResolveUrl(this.MainSettings.ThemeLocation);

        public string ForumIds { get; set; } = string.Empty;

        public int DefaultForumViewTemplateId { get; set; } = -1;

        public int DefaultTopicsViewTemplateId { get; set; } = -1;

        public int DefaultTopicViewTemplateId { get; set; } = -1;

        public string DefaultView { get; set; } = Views.ForumView;

        public bool JumpToLastPost
        {
            get
            {
                if (!this.Request.IsAuthenticated)
                {
                    return false;
                }

                if (!this._jumpToLastPost.HasValue)
                {
                    this._jumpToLastPost = this.UserController.GetUser(this.PortalId, this.UserId).Profile.PrefJumpLastPost;
                }

                return this._jumpToLastPost.Value;
            }
        }

        public DateTime UserLastAccess
        {
            get
            {
                if (!this.Request.IsAuthenticated)
                {
                    return Utilities.NullDate();
                }

                var lastAccess = this.Session[string.Concat(this.UserId.ToString(), this.ModuleId, "LastAccess")];
                return lastAccess == null ? Utilities.NullDate() : Convert.ToDateTime(lastAccess);
            }

            set
            {
                this.Session[string.Concat(this.UserId.ToString(), this.ModuleId, "LastAccess")] = value;
            }
        }

        public int PostId
        {
            get
            {
                // If the id has already been set, return it.
                if (this._postId.HasValue)
                {
                    return this._postId.Value;
                }

                // If there is no id in the query string, set it to the default value and return it.
                var queryPostId = this.Request.QueryString[ParamKeys.PostId];
                if (string.IsNullOrWhiteSpace(queryPostId))
                {
                    this._postId = 0;
                    return this._postId.Value;
                }

                // If there is a hash tag in the query value, remove it and anything after it before parsing.
                var hashIndex = queryPostId.IndexOf("#", 0, StringComparison.Ordinal);
                if (hashIndex >= 0)
                {
                    queryPostId = queryPostId.Substring(0, hashIndex);
                }

                // Try to parse the id, if it doesn't work, return the default value.
                int parsedPostId;
                this._postId = int.TryParse(queryPostId, out parsedPostId) ? parsedPostId : 0;

                return this._postId.Value;
            }
        }

        public int TopicId
        {
            get
            {
                // If the id has already been set, return it.
                if (this._topicId.HasValue)
                {
                    return this._topicId.Value;
                }

                // If there is no id in the query string, set it to the default value and return it.
                var queryTopicId = this.Request.QueryString[ParamKeys.TopicId];
                if (string.IsNullOrWhiteSpace(queryTopicId))
                {
                    this._topicId = this.PostId > 0 ? this.PostId : -1; // If we have no topic id, but we do have a post id, use that instead.  Need to track down where this is used.
                    return this._topicId.Value;
                }

                // If there is a hash tag in the query value, remove it and anything after it before parsing.
                var hashIndex = queryTopicId.IndexOf("#", 0, StringComparison.Ordinal);
                if (hashIndex >= 0)
                {
                    queryTopicId = queryTopicId.Substring(0, hashIndex);
                }

                // Try to parse the id, if it doesn't work, return the default value.
                int parsedTopicId;
                this._topicId = int.TryParse(queryTopicId, out parsedTopicId) ? parsedTopicId : -1;

                return this._topicId.Value;
            }

            set
            {
                this._topicId = value;
            }
        }

        public int ReplyId
        {
            get
            {
                // If the id has already been set, return it.
                if (this._replyId.HasValue)
                {
                    return this._replyId.Value;
                }

                // If there is no id in the query string, set it to the default value and return it.
                var queryReplyId = this.Request.QueryString[ParamKeys.ReplyId];
                if (string.IsNullOrWhiteSpace(queryReplyId))
                {
                    this._replyId = 0;
                    return this._replyId.Value;
                }

                // If there is a hash tag in the query value, remove it and anything after it before parsing.
                var hashIndex = queryReplyId.IndexOf("#", 0, StringComparison.Ordinal);
                if (hashIndex >= 0)
                {
                    queryReplyId = queryReplyId.Substring(0, hashIndex);
                }

                // Try to parse the id, if it doesn't work, return the default value.
                int parsedReplyId;
                this._replyId = int.TryParse(queryReplyId, out parsedReplyId) ? parsedReplyId : 0;

                return this._replyId.Value;
            }

            set
            {
                this._replyId = value;
            }
        }

        public int QuoteId
        {
            get
            {
                // If the id has already been set, return it.
                if (this._quoteId.HasValue)
                {
                    return this._quoteId.Value;
                }

                // If there is no id in the query string, set it to the default value and return it.
                var queryQuoteId = this.Request.QueryString[ParamKeys.QuoteId];
                if (string.IsNullOrWhiteSpace(queryQuoteId))
                {
                    this._quoteId = 0;
                    return this._quoteId.Value;
                }

                // If there is a hash tag in the query value, remove it and anything after it before parsing.
                var hashIndex = queryQuoteId.IndexOf("#", 0, StringComparison.Ordinal);
                if (hashIndex >= 0)
                {
                    queryQuoteId = queryQuoteId.Substring(0, hashIndex);
                }

                // Try to parse the id, if it doesn't work, return the default value.
                int parsedQuoteId;
                this._quoteId = int.TryParse(queryQuoteId, out parsedQuoteId) ? parsedQuoteId : 0;

                return this._quoteId.Value;
            }

            set
            {
                this._quoteId = value;
            }
        }

        public int ForumId
        {
            get
            {
                // If the id has already been set, return it.
                if (this._forumId.HasValue)
                {
                    return this._forumId.Value;
                }

                // Set out default value
                this._forumId = -1;

                // If there is an id in the query string, parse it
                var queryForumId = this.Request.QueryString[ParamKeys.ForumId];
                if (!string.IsNullOrWhiteSpace(queryForumId))
                {
                    // Try to parse the id, if it doesn't work, return the default value.
                    int parsedForumId;
                    this._forumId = int.TryParse(queryForumId, out parsedForumId) ? parsedForumId : 0;
                }

                // If we don't have a forum id at this point, try and pull it from "forumid" in the query string
                if (this._forumId < 1)
                {
                    queryForumId = this.Request.QueryString[Literals.ForumId];
                    if (!string.IsNullOrWhiteSpace(queryForumId))
                    {
                        // Try to parse the id, if it doesn't work, return the default value.
                        int parsedForumId;
                        this._forumId = int.TryParse(queryForumId, out parsedForumId) ? parsedForumId : 0;
                    }
                }

                // If we still don't have a forum id, but we have a topic id, look up the forum id
                if (this._forumId < 1 & this.TopicId > 0)
                {
                    this._forumId = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Forum_GetByTopicId(this.TopicId);
                }

                return this._forumId.Value;
            }

            set
            {
                this._forumId = value;
            }
        }

        public int AuthorId
        {
            get
            {
                // If the id has already been set, return it.
                if (this._authorid.HasValue)
                {
                    return this._authorid.Value;
                }

                // Set out default value
                this._authorid = -1;

                // If there is an id in the query string, parse it
                var queryAuthorId = this.Request.QueryString[ParamKeys.AuthorId];
                if (!string.IsNullOrWhiteSpace(queryAuthorId))
                {
                    // Try to parse the id, if it doesn't work, return the default value.
                    int parsedAuthorId;
                    this._authorid = int.TryParse(queryAuthorId, out parsedAuthorId) ? parsedAuthorId : 0;
                }

                // If we don't have a user id at this point, try and pull it from "authorid" in the query string
                if (this._authorid < 1)
                {
                    queryAuthorId = this.Request.QueryString["authorid"];
                    if (!string.IsNullOrWhiteSpace(queryAuthorId))
                    {
                        // Try to parse the id, if it doesn't work, return the default value.
                        int parsedAuthorId;
                        this._authorid = int.TryParse(queryAuthorId, out parsedAuthorId) ? parsedAuthorId : 0;
                    }
                }

                return this._authorid.Value;
            }

            set
            {
                this._authorid = value;
            }
        }

        public int ForumGroupId
        {
            get
            {
                // If the id has already been set, return it.
                if (this._forumGroupId.HasValue)
                {
                    return this._forumGroupId.Value;
                }

                // If there is no id in the query string, set it to the default value and return it.
                var queryForumGroupId = this.Request.QueryString[ParamKeys.GroupId];
                if (string.IsNullOrWhiteSpace(queryForumGroupId))
                {
                    this._forumGroupId = -1;
                    return this._forumGroupId.Value;
                }

                // If there is a hash tag in the query value, remove it and anything after it before parsing.
                var hashIndex = queryForumGroupId.IndexOf("#", 0, StringComparison.Ordinal);
                if (hashIndex >= 0)
                {
                    queryForumGroupId = queryForumGroupId.Substring(0, hashIndex);
                }

                // Try to parse the id, if it doesn't work, return the default value.
                int parsedForumGroupId;
                this._forumGroupId = int.TryParse(queryForumGroupId, out parsedForumGroupId) ? parsedForumGroupId : 0;

                return this._forumGroupId.Value;
            }

            set
            {
                this._forumGroupId = value;
            }
        }

        public int ParentForumId { get; set; } = -1;

        public string TemplateFile { get; set; } = string.Empty;

        public DotNetNuke.Modules.ActiveForums.Entities.ForumInfo ForumInfo
        {
            get
            {
                return this._foruminfo ?? (this._foruminfo = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Forums_Get(this.PortalId, this.ForumModuleId, this.ForumId, true, this.TopicId));
            }

            set
            {
                this._foruminfo = value;
            }
        }

        public int SocialGroupId { get; set; }

        public bool CanRead => this._canRead ?? this.SecurityCheck("read");

        public bool CanView => this._canView ?? this.SecurityCheck("view");

        public bool CanCreate
        {
            get
            {
                if (!this._canCreate.HasValue)
                {
                    // The basic security check trumps everything.
                    if (!this.SecurityCheck("create"))
                    {
                        this._canCreate = false;
                    }

                    // Admins and trusted users shall pass!
                    else if (this.ForumUser.IsAdmin || this.ForumUser.IsSuperUser || DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(this.ForumInfo.Security.Trust, this.ForumUser.UserRoles))
                    {
                        this._canCreate = true;
                    }

                    // If CreatePostCount is not set, no need to go further
                    else if (this.ForumInfo.CreatePostCount <= 0)
                    {
                        this._canCreate = true;
                    }

                    // If we don't have a valid user, there is no way they could meed the minumum post count requirement
                    else if (this.ForumUser.UserId <= 0)
                    {
                        this._canCreate = false;
                    }
                    else
                    {
                        this._canCreate = this.ForumUser.PostCount >= this.ForumInfo.CreatePostCount;
                    }
                }

                return this._canCreate.Value;
            }
        }

        public bool CanReply
        {
            get
            {
                if (!this._canReply.HasValue)
                {
                    // The basic security check trumps everything.
                    if (!this.SecurityCheck("reply"))
                    {
                        this._canReply = false;
                    }

                    // Admins and trusted users shall pass!
                    else if (this.ForumUser.IsAdmin || this.ForumUser.IsSuperUser || DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(this.ForumInfo.Security.Trust, this.ForumUser.UserRoles))
                    {
                        this._canReply = true;
                    }

                    // If ReplyPostCount is not set, no need to go further
                    else if (this.ForumInfo.ReplyPostCount <= 0)
                    {
                        this._canReply = true;
                    }

                    // If we don't have a valid user, there is no way they could meed the minumum post count requirement
                    else if (this.ForumUser.UserId <= 0)
                    {
                        this._canReply = false;
                    }
                    else
                    {
                        this._canReply = this.ForumUser.PostCount >= this.ForumInfo.ReplyPostCount;
                    }
                }

                return this._canReply.Value;
            }
        }
        #endregion

        #region Helper Methods
        private bool SecurityCheck(string secType)
        {
            if (this.ForumUser == null)
            {
                return false;
            }

            var xNode = this.ForumData.SelectSingleNode(string.Concat("//forums/forum[@forumid='" + this.ForumId, "']/security/", secType));

            if (xNode == null)
            {
                return false;
            }

            var secRoles = xNode.InnerText;

            return DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(secRoles, this.ForumUser.UserRoles);
        }

        protected string GetSharedResource(string key)
        {
            return Localization.GetString(key, Globals.SharedResourceFile);
        }

        internal bool IsHtmlPermitted(HTMLPermittedUsers permittedMode, bool userIsTrusted, bool userIsModerator)
        {
            if (permittedMode == HTMLPermittedUsers.AllUsers)
            {
                return true;
            }

            if (permittedMode == HTMLPermittedUsers.AuthenticatedUsers && this.Request.IsAuthenticated)
            {
                return true;
            }

            if (permittedMode == HTMLPermittedUsers.TrustedUsers && userIsTrusted)
            {
                return true;
            }

            if (permittedMode == HTMLPermittedUsers.Moderators && userIsModerator)
            {
                return true;
            }

            if (permittedMode == HTMLPermittedUsers.Administrators && ModulePermissionController.HasModulePermission(this.ModuleConfiguration.ModulePermissions, "EDIT"))
            {
                return true;
            }

            return false;
        }

        #endregion

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // If printmode, simply exit.
            if (this.Request.QueryString["dnnprintmode"] != null)
            {
                return;
            }

            var p = new List<string>();

            var viewType = this.Request.Params[ParamKeys.ViewType];

            // Topic View
            if (this.TopicId > 0 && (viewType == Views.Topic))
            {
                p.Add(string.Concat(ParamKeys.TopicId, "=", this.TopicId));

                var firstNewPost = this.Request.Params[ParamKeys.FirstNewPost];
                if (!string.IsNullOrWhiteSpace(firstNewPost))
                {
                    p.Add(string.Concat(ParamKeys.FirstNewPost, "=", firstNewPost));
                }

                var contentJumpId = this.Request.Params[ParamKeys.ContentJumpId];
                if (!string.IsNullOrWhiteSpace(contentJumpId))
                {
                    p.Add(string.Concat(ParamKeys.ContentJumpId, "=", contentJumpId));
                }

                var pageId = this.Request.Params[ParamKeys.PageId];
                if (!string.IsNullOrWhiteSpace(pageId))
                {
                    int parsedPageId;
                    if (int.TryParse(pageId, out parsedPageId) && parsedPageId > 1)
                    {
                        p.Add(string.Concat(ParamKeys.PageId, "=", pageId));
                    }
                }

                var pageJumpId = this.Request.Params[ParamKeys.PageJumpId];
                if (!string.IsNullOrWhiteSpace(pageJumpId))
                {
                    int parsedPageJumpId;
                    if (int.TryParse(pageJumpId, out parsedPageJumpId) && parsedPageJumpId > 1)
                    {
                        p.Add(string.Concat(ParamKeys.PageJumpId, "=", pageJumpId));
                    }
                }

                var sort = this.Request.QueryString[ParamKeys.Sort];
                if (!string.IsNullOrWhiteSpace(sort))
                {
                    var defaultSort = (this.Request.IsAuthenticated &&
                                       !string.IsNullOrWhiteSpace(this.ForumUser.Profile.PrefDefaultSort))
                                          ? this.ForumUser.Profile.PrefDefaultSort.ToUpperInvariant().Trim()
                                          : "ASC";

                    sort = sort.ToUpperInvariant();
                    if ((sort != defaultSort) && (sort == "ASC" || sort == "DESC"))
                    {
                        p.Add(string.Concat(ParamKeys.Sort, "=", sort));
                    }
                }
            }

            // Topics View
            else if (this.ForumId > 0 && viewType == Views.Topics)
            {
                p.Add(string.Concat(ParamKeys.ForumId, "=", this.ForumId));

                var pageId = this.Request.Params[ParamKeys.PageId];
                if (!string.IsNullOrWhiteSpace(pageId))
                {
                    int parsedPageId;
                    if (int.TryParse(pageId, out parsedPageId) && parsedPageId > 1)
                    {
                        p.Add(string.Concat(ParamKeys.PageId, "=", pageId));
                    }
                }

                var pageJumpId = this.Request.Params[ParamKeys.PageJumpId];
                if (!string.IsNullOrWhiteSpace(pageJumpId))
                {
                    int parsedPageJumpId;
                    if (int.TryParse(pageJumpId, out parsedPageJumpId) && parsedPageJumpId > 1)
                    {
                        p.Add(string.Concat(ParamKeys.PageJumpId, "=", pageJumpId));
                    }
                }
            }

            if (p.Count <= 0)
            {
                return;
            }

            var sURL = Utilities.NavigateURL(this.TabId, string.Empty, p.ToArray());
            if (string.IsNullOrEmpty(sURL))
            {
                return;
            }

            this.Response.Clear();
            this.Response.Status = "301 Moved Permanently";
            this.Response.AddHeader("Location", sURL);
            this.Response.End();
        }
    }
}
