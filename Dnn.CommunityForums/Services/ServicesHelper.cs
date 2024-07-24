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
using DotNetNuke.Entities.Users;
namespace DotNetNuke.Modules.ActiveForums.Services
{
    internal static class ServicesHelper
    {
        internal static bool IsAuthorized(int portalId, int moduleId, int forumId, SecureActions permissionRequired, UserInfo userInfo)
        {
            DotNetNuke.Modules.ActiveForums.Entities.ForumInfo fi = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(forumId, moduleId);
            string roles;
            switch (permissionRequired)
            {
                case SecureActions.View:
                    roles = fi.Security.View;
                    break;
                case SecureActions.Read:
                    roles = fi.Security.Read;
                    break;
                case SecureActions.Create:
                    roles = fi.Security.Create;
                    break;
                case SecureActions.Reply:
                    roles = fi.Security.Reply;
                    break;
                case SecureActions.Edit:
                    roles = fi.Security.Edit;
                    break;
                case SecureActions.Delete:
                    roles = fi.Security.Delete;
                    break;
                case SecureActions.Lock:
                    roles = fi.Security.Lock;
                    break;
                case SecureActions.Pin:
                    roles = fi.Security.Pin;
                    break;
                case SecureActions.Attach:
                    roles = fi.Security.Attach;
                    break;
                case SecureActions.Poll:
                    roles = fi.Security.Poll;
                    break;
                case SecureActions.Block:
                    roles = fi.Security.Block;
                    break;
                case SecureActions.Trust:
                    roles = fi.Security.Trust;
                    break;
                case SecureActions.Subscribe:
                    roles = fi.Security.Subscribe;
                    break;
                case SecureActions.Announce:
                    roles = fi.Security.Announce;
                    break;
                case SecureActions.Tag:
                    roles = fi.Security.Tag;
                    break;
                case SecureActions.Categorize:
                    roles = fi.Security.Categorize;
                    break;
                case SecureActions.Prioritize:
                    roles = fi.Security.Prioritize;
                    break;
                case SecureActions.ModApprove:
                    roles = fi.Security.ModApprove;
                    break;
                case SecureActions.ModMove:
                    roles = fi.Security.ModMove;
                    break;
                case SecureActions.ModSplit:
                    roles = fi.Security.ModSplit;
                    break;
                case SecureActions.ModDelete:
                    roles = fi.Security.ModDelete;
                    break;
                case SecureActions.ModUser:
                    roles = fi.Security.ModUser;
                    break;
                case SecureActions.ModEdit:
                    roles = fi.Security.ModEdit;
                    break;
                default:
                    return false;
            }
            return (DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(roles, userInfo.UserID, portalId));
        }
    }
}