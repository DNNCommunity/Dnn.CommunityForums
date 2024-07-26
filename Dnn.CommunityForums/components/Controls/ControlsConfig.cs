// Copyright (c) 2013-2024 by DNN Community
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

namespace DotNetNuke.Modules.ActiveForums
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;

    public class ControlsConfig
    {
        #region Private Members
        private int portalId;
        private int pageId = -1;
        private int moduleId;
        private int forumId;
        private int topicId;
        private int contentId;
        private string appPath;
        private string themePath;
        private User user;
        private string templatePath;
        private string defaultViewRoles;
        private string profileLink;
        private string membersLink;
        private string adminRoles;

        #endregion
        #region Public Properties
        public string AdminRoles
        {
            get
            {
                return this.adminRoles;
            }

            set
            {
                this.adminRoles = value;
            }
        }

        public string AppPath
        {
            get
            {
                return this.appPath;
            }

            set
            {
                this.appPath = value;
            }
        }

        public int ContentId
        {
            get
            {
                return this.contentId;
            }

            set
            {
                this.contentId = value;
            }
        }

        public string DefaultViewRoles
        {
            get
            {
                return this.defaultViewRoles;
            }

            set
            {
                this.defaultViewRoles = value;
            }
        }

        public int ForumId
        {
            get
            {
                return this.forumId;
            }

            set
            {
                this.forumId = value;
            }
        }

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

        public string MembersLink
        {
            get
            {
                return this.membersLink;
            }

            set
            {
                this.membersLink = value;
            }
        }

        public int PageId
        {
            get
            {
                return this.pageId;
            }

            set
            {
                this.pageId = value;
            }
        }

        public string ProfileLink
        {
            get
            {
                return this.profileLink;
            }

            set
            {
                this.profileLink = value;
            }
        }

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

        public string TemplatePath
        {
            get
            {
                return this.templatePath;
            }

            set
            {
                this.templatePath = value;
            }
        }

        public string ThemePath
        {
            get
            {
                return this.themePath;
            }

            set
            {
                this.themePath = value;
            }
        }

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

        public User User
        {
            get
            {
                return this.user;
            }

            set
            {
                this.user = value;
            }
        }

        #endregion

    }
}
