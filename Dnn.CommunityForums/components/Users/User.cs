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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace DotNetNuke.Modules.ActiveForums
{
    public class User
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public bool IsAdmin { get; set; } = false;
        public bool IsSuperUser { get; set; } = false;

        public UserProfileInfo Profile { get; set; } = null;

        public System.Collections.Hashtable Properties { get; set; } = null;

        public int UserId { get; set; } = -1;
        public string UserName { get; set; } = string.Empty;

        public string UserRoles { get; set; }
        public bool PrefBlockSignatures => Profile.PrefBlockSignatures;
        public bool PrefBlockAvatars => Profile.PrefBlockAvatars;
        public int PostCount => Profile.PostCount;
        public int TrustLevel => Profile.TrustLevel;
        public bool PrefTopicSubscribe => Profile.PrefTopicSubscribe;
        public CurrentUserTypes CurrentUserType => Profile.CurrentUserType;
        public string UserForums { get; set; } = string.Empty;
        public User()
        {
            UserId = -1;
            IsSuperUser = false;
            IsAdmin = false;
            Profile = new UserProfileInfo();
            UserRoles = Globals.DefaultAnonRoles + "|-1;||";
        }
    }
}

