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
using System.Data;

using System.Threading;
using System.Web;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.UI.UserControls;

namespace DotNetNuke.Modules.ActiveForums
{
    [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]

    public class UserController
    {
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public User GetUser(int PortalId, int ModuleId) => (User)new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController().GetUser(PortalId, ModuleId);
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public User DNNGetCurrentUser(int PortalId, int ModuleId) => (User)new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController().DNNGetCurrentUser(PortalId, ModuleId);
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        private User GetDNNUser(int portalId, int userId) => (User)new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController().GetByUserId(userId);
        private User GetDNNUser(int portalId, string userName) => (User)new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController().GetDNNUser(portalId, userName);
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public User GetDNNUser(string userName) => (User)new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController().GetDNNUser(userName);
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public User GetUser(int PortalId, int ModuleId, int userId) => (User)new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController().GetUser(PortalId, ModuleId);
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public User GetUser(int PortalId, int ModuleId, string userName) => (User)new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController().GetUser(PortalId, ModuleId, userName);
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public User FillProfile(int PortalId, int ModuleId, User u) => throw new NotImplementedException();
        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public UserProfileInfo Profiles_Get(int PortalId, int ModuleId, int UserId) => throw new NotImplementedException();
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public Hashtable GetUserProperties(DotNetNuke.Entities.Users.UserInfo dnnUser) => throw new NotImplementedException();
    }
}