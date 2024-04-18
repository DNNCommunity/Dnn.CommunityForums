//
// Community Forums
// Copyright (c) 2013-2021
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
// 
using System.Linq;
using DotNetNuke.Data;
using DotNetNuke.Services.Journal;

namespace DotNetNuke.Modules.ActiveForums.Controllers
{
    internal static class UserController
    {
        private class ContentForUser
        {
            internal int ForumId { get; set; }
            internal int TopicId { get; set; }
            internal int ReplyId { get; set; }
        }
        internal static void BanUser(int PortalId, int ModuleId, int UserId)
        {
            if (UserId > -1)
            {
                string sql = "SELECT ft.ForumId, ft.TopicId, r.ReplyId " +
                    "FROM {databaseOwner}[{objectQualifier}activeforums_ForumTopics] ft " +
                    "INNER JOIN {databaseOwner}[{objectQualifier}activeforums_Topics] t " +
                    "ON t.TopicId = ft.TopicId " +
                    "INNER JOIN {databaseOwner}[{objectQualifier}activeforums_Replies] r " +
                    "ON r.TopicId = t.TopicId " +
                    "INNER JOIN {databaseOwner}[{objectQualifier}activeforums_Content] c " +
                    "ON c.ContentId = r.ContentId " +
                    "WHERE c.AuthorId = @0 AND c.ModuleId = @1 " +
                    "UNION " +
                    "SELECT ft.ForumId, ft.TopicId, 0 AS ReplyId " +
                    "FROM {databaseOwner}[{objectQualifier}activeforums_ForumTopics] ft " +
                    "INNER JOIN {databaseOwner}[{objectQualifier}activeforums_Topics] t " +
                    "ON t.TopicId = ft.TopicId " +
                    "INNER JOIN {databaseOwner}[{objectQualifier}activeforums_Content] c " +
                    "ON c.ContentId = t.ContentId " +
                    "WHERE c.AuthorId = @0 AND c.ModuleId = @1";

                var contentForBannedUser = DataContext.Instance().ExecuteQuery<ContentForUser>(System.Data.CommandType.Text, sql, UserId, ModuleId).ToList();
                string objectKey;
                contentForBannedUser.ForEach(c =>
                {
                    objectKey = c.ReplyId < 1 ? $"{c.ForumId}:{c.TopicId}" : $"{c.ForumId}:{c.TopicId}:{c.ReplyId}";
                    if (JournalController.Instance.GetJournalItemByKey(PortalId, objectKey) != null)
                    {
                        JournalController.Instance.DeleteJournalItemByKey(PortalId, objectKey);
                    }
                });
               
                DataProvider.Instance().Topics_Delete_For_User(ModuleId: ModuleId, UserId: UserId, DelBehavior: SettingsBase.GetModuleSettings(ModuleId).DeleteBehavior);
                DotNetNuke.Entities.Users.UserInfo user = DotNetNuke.Entities.Users.UserController.GetUserById(portalId: PortalId, userId: UserId);
                user.Membership.Approved = false;
                DotNetNuke.Entities.Users.UserController.UpdateUser(portalId: PortalId, user: user, loggedAction: true);
                DataCache.CacheClearPrefix(ModuleId, string.Format("AF-FV-{0}-{1}", PortalId, ModuleId));
            }
        }
    }
}
