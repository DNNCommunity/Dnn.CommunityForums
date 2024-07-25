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

    public class User
    {
        private bool isAdmin = false;
        private bool isSuperUser = false;
        private UserProfileInfo profile = null;
        private Hashtable properties = null;
        private int userId = -1;
        private string userName = string.Empty;
        private string userRoles;
        private string lastName = string.Empty;
        private string firstName = string.Empty;
        private string displayName = string.Empty;
        private string email = string.Empty;
        private DateTime dateUpdated;
        private DateTime dateCreated;
        private string userForums = string.Empty;

        public string FirstName
        {
            get
            {
                return this.firstName;
            }

            set
            {
                this.firstName = value;
            }
        }

        public string LastName
        {
            get
            {
                return this.lastName;
            }

            set
            {
                this.lastName = value;
            }
        }

        public string DisplayName
        {
            get
            {
                return this.displayName;
            }

            set
            {
                this.displayName = value;
            }
        }

        public string Email
        {
            get
            {
                return this.email;
            }

            set
            {
                this.email = value;
            }
        }

        public DateTime DateCreated
        {
            get
            {
                return this.dateCreated;
            }

            set
            {
                this.dateCreated = value;
            }
        }

        public DateTime DateUpdated
        {
            get
            {
                return this.dateUpdated;
            }

            set
            {
                this.dateUpdated = value;
            }
        }

        public bool IsAdmin
        {
            get
            {
                return this.isAdmin;
            }

            set
            {
                this.isAdmin = value;
            }
        }

        public bool IsSuperUser
        {
            get
            {
                return this.isSuperUser;
            }

            set
            {
                this.isSuperUser = value;
            }
        }

        public UserProfileInfo Profile
        {
            get
            {
                return this.profile;
            }

            set
            {
                this.profile = value;
            }
        }

        public System.Collections.Hashtable Properties
        {
            get
            {
                return this.properties;
            }

            set
            {
                this.properties = value;
            }
        }

        public int UserId
        {
            get
            {
                return this.userId;
            }

            set
            {
                this.userId = value;
            }
        }

        public string UserName
        {
            get
            {
                return this.userName;
            }

            set
            {
                this.userName = value;
            }
        }

        public string UserRoles
        {
            get
            {
                return this.userRoles;
            }

            set
            {
                this.userRoles = value;
            }
        }

        public bool PrefBlockSignatures
        {
            get
            {
                return this.Profile.PrefBlockSignatures;
            }
        }

        public bool PrefBlockAvatars
        {
            get
            {
                return this.Profile.PrefBlockAvatars;
            }
        }

        public int PostCount
        {
            get
            {
                return this.Profile.PostCount;
            }
        }

        public int TrustLevel
        {
            get
            {
                return this.Profile.TrustLevel;
            }
        }

        public bool PrefTopicSubscribe
        {
            get
            {
                return this.Profile.PrefTopicSubscribe;
            }
        }

        public CurrentUserTypes CurrentUserType
        {
            get
            {
                return this.Profile.CurrentUserType;
            }
        }

        public string UserForums
        {
            get
            {
                return this.userForums;
            }

            set
            {
                this.userForums = value;
            }
        }

        public User()
        {
            this.userId = -1;
            this.isSuperUser = false;
            this.isAdmin = false;
            this.profile = new UserProfileInfo();
            this.userRoles = Globals.DefaultAnonRoles + "|-1;||";
        }
    }
}
