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

namespace DotNetNuke.Modules.ActiveForums.Entities
{
    /// <summary>
    /// Author is really the same as a user. Just separated out to make code more understandable.
    /// </summary>
	public class AuthorInfo
    {
        private DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo forumUserInfo;

        public AuthorInfo()
        {
            this.forumUserInfo = new DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo();
        }

        public AuthorInfo(ForumUserInfo forumUserInfo)
        {
            this.forumUserInfo = forumUserInfo;
        }

        public AuthorInfo(int portalId, int moduleId, int userId)
        {
            this.forumUserInfo = new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(-1).GetByUserId(portalId, userId);
        }

        public DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo ForumUser { get => this.forumUserInfo; set => this.forumUserInfo = value; }

        public int AuthorId
        {
            get => this.forumUserInfo.UserId;
            set => this.forumUserInfo.UserId = value;
        }

        public new string FirstName
        {
            get => this.forumUserInfo.UserInfo.FirstName;
            set => this.forumUserInfo.UserInfo.FirstName = value;
        }

        public new string LastName
        {
            get => this.forumUserInfo.UserInfo.LastName;
            set => this.forumUserInfo.UserInfo.LastName = value;
        }

        public new string DisplayName
        {
            get => this.forumUserInfo.UserInfo.DisplayName;
            set => this.forumUserInfo.UserInfo.DisplayName = value;
        }

        public new string Username
        {
            get => this.forumUserInfo.UserInfo.Username;
            set => this.forumUserInfo.UserInfo.Username = value;
        }

        public new string Email
        {
            get => this.forumUserInfo.UserInfo.Email;
            set => this.forumUserInfo.UserInfo.Email = value;
        }

    }
}
