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
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;

    public class ControlsConfig
    {
#region Private Members
        private int _PortalId;
        private int _pageId = -1;
        private int _ModuleId;
        private int _forumId;
        private int _topicId;
        private int _contentId;
        private string _appPath;
        private string _themePath;
        private User _user;
        private string _templatePath;
        private string _defaultViewRoles;
        private string _profileLink;
        private string _membersLink;
        private string _adminRoles;

#endregion
#region Public Properties
        public string AdminRoles
        {
            get
            {
                return this._adminRoles;
            }

            set
            {
                this._adminRoles = value;
            }
        }

        public string AppPath
        {
            get
            {
                return this._appPath;
            }

            set
            {
                this._appPath = value;
            }
        }

        public int ContentId
        {
            get
            {
                return this._contentId;
            }

            set
            {
                this._contentId = value;
            }
        }

        public string DefaultViewRoles
        {
            get
            {
                return this._defaultViewRoles;
            }

            set
            {
                this._defaultViewRoles = value;
            }
        }

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

        public string MembersLink
        {
            get
            {
                return this._membersLink;
            }

            set
            {
                this._membersLink = value;
            }
        }

        public int PageId
        {
            get
            {
                return this._pageId;
            }

            set
            {
                this._pageId = value;
            }
        }

        public string ProfileLink
        {
            get
            {
                return this._profileLink;
            }

            set
            {
                this._profileLink = value;
            }
        }

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

        public string TemplatePath
        {
            get
            {
                return this._templatePath;
            }

            set
            {
                this._templatePath = value;
            }
        }

        public string ThemePath
        {
            get
            {
                return this._themePath;
            }

            set
            {
                this._themePath = value;
            }
        }

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

        public User User
        {
            get
            {
                return this._user;
            }

            set
            {
                this._user = value;
            }
        }

#endregion

    }
}
