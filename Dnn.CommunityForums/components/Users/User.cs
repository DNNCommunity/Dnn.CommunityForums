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
        private bool _isAdmin = false;
        private bool _isSuperUser = false;
        private UserProfileInfo _profile = null;
        private Hashtable _properties = null;
        private int _userId = -1;
        private string _userName = string.Empty;
        private string _userRoles;
        private string _lastName = string.Empty;
        private string _firstName = string.Empty;
        private string _displayName = string.Empty;
        private string _email = string.Empty;
        private DateTime _dateUpdated;
        private DateTime _dateCreated;
        private string _userForums = string.Empty;

        public string FirstName
        {
            get
            {
                return this._firstName;
            }

            set
            {
                this._firstName = value;
            }
        }

        public string LastName
        {
            get
            {
                return this._lastName;
            }

            set
            {
                this._lastName = value;
            }
        }

        public string DisplayName
        {
            get
            {
                return this._displayName;
            }

            set
            {
                this._displayName = value;
            }
        }

        public string Email
        {
            get
            {
                return this._email;
            }

            set
            {
                this._email = value;
            }
        }

        public DateTime DateCreated
        {
            get
            {
                return this._dateCreated;
            }

            set
            {
                this._dateCreated = value;
            }
        }

        public DateTime DateUpdated
        {
            get
            {
                return this._dateUpdated;
            }

            set
            {
                this._dateUpdated = value;
            }
        }

        public bool IsAdmin
        {
            get
            {
                return this._isAdmin;
            }

            set
            {
                this._isAdmin = value;
            }
        }

        public bool IsSuperUser
        {
            get
            {
                return this._isSuperUser;
            }

            set
            {
                this._isSuperUser = value;
            }
        }

        public UserProfileInfo Profile
        {
            get
            {
                return this._profile;
            }

            set
            {
                this._profile = value;
            }
        }

        public System.Collections.Hashtable Properties
        {
            get
            {
                return this._properties;
            }

            set
            {
                this._properties = value;
            }
        }

        public int UserId
        {
            get
            {
                return this._userId;
            }

            set
            {
                this._userId = value;
            }
        }

        public string UserName
        {
            get
            {
                return this._userName;
            }

            set
            {
                this._userName = value;
            }
        }

        public string UserRoles
        {
            get
            {
                return this._userRoles;
            }

            set
            {
                this._userRoles = value;
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
                return this._userForums;
            }

            set
            {
                this._userForums = value;
            }
        }

        public User()
        {
            this._userId = -1;
            this._isSuperUser = false;
            this._isAdmin = false;
            this._profile = new UserProfileInfo();
            this._userRoles = Globals.DefaultAnonRoles + "|-1;||";
        }
    }
}
