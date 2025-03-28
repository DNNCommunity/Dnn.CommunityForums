﻿// Copyright (c) by DNN Community
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

    [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]

    public class UserController
    {
        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public User GetUser(int PortalId, int ModuleId) => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public User DNNGetCurrentUser(int PortalId, int ModuleId) => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        private User GetDNNUser(int portalId, int userId) => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        private User GetDNNUser(int portalId, string userName) => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public User GetDNNUser(string userName) => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public User GetUser(int PortalId, int ModuleId, int userId) => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public User GetUser(int PortalId, int ModuleId, string userName) => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public User FillProfile(int PortalId, int ModuleId, User u) => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public UserProfileInfo Profiles_Get(int PortalId, int ModuleId, int UserId) => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public Hashtable GetUserProperties(DotNetNuke.Entities.Users.UserInfo dnnUser) => throw new NotImplementedException();
    }
}
