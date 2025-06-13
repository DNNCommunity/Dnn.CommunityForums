// Copyright (c) by DNN Community
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

using DotNetNuke.Common.Utilities;

namespace DotNetNuke.Modules.ActiveForums.Services.Avatars
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Services.Scheduling;

    public class AvatarRefreshQueue : DotNetNuke.Services.Scheduling.SchedulerClient
    {
        public AvatarRefreshQueue(ScheduleHistoryItem scheduleHistoryItem)
        {
            this.ScheduleHistoryItem = scheduleHistoryItem;
        }

        public override void DoWork()
        {
            try
            {
                foreach (DotNetNuke.Abstractions.Portals.IPortalInfo portal in DotNetNuke.Entities.Portals.PortalController.Instance.GetPortals())
                {
                    foreach (ModuleInfo module in DotNetNuke.Entities.Modules.ModuleController.Instance.GetModules(portal.PortalId))
                    {
                        if (module.DesktopModule.ModuleName.Trim().ToLowerInvariant().Equals(Globals.ModuleName.ToLowerInvariant()))
                        {
                            if (ForumBase.GetModuleSettings(module.ModuleID).AvatarRefreshEnabled)
                            {

                                if (ForumBase.GetModuleSettings(module.ModuleID).AvatarRefreshType.Equals(Globals.AvatarRefreshGravatar))
                                {
                                    var intQueueCount = RefreshGravatars(module.PortalID, module.ModuleID);
                                    this.ScheduleHistoryItem.Succeeded = true;
                                    this.ScheduleHistoryItem.AddLogNote(string.Concat("Processed ", intQueueCount, " avatar refresh requests"));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.ScheduleHistoryItem.Succeeded = false;
                this.ScheduleHistoryItem.AddLogNote(string.Concat("Avatar Refresh Queue Failed. ", ex));
                this.Errored(ref ex);
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        private static int RefreshGravatars(int portalId, int moduleId)
        {
            var intQueueCount = 0;
            try
            {
                var avatarProvider = new GravatarAvatarProvider();
                GetBatch(portalId, moduleId).ForEach(forumUser =>
                {
                    var completed = RefreshAvatar(portalId: portalId, moduleId: moduleId, avatarProvider: avatarProvider, forumUser: forumUser);
                    if (completed)
                    {
                        intQueueCount += 1;
                    }
                });
                return intQueueCount;
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                return -1;
            }
        }

        private static List<DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo> GetBatch(int portalId, int moduleId)
        {
            try
            {
                return new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(moduleId: moduleId).Get().Where(u => (u.PortalId == portalId && !u.AvatarDisabled) &&
                                                                                                                                ((string.IsNullOrEmpty(u.UserInfo.Profile.GetPropertyValue("Photo")) && !u.AvatarLastRefresh.HasValue) || /* anyone without an avatar who has never had their avatar refreshed */
                                                                                                                                 (u.AvatarLastRefresh.HasValue && DateTime.UtcNow.Subtract(u.AvatarLastRefresh.Value).TotalDays > 90))) /* or anyone whose avatar was last refreshed more than 90 days ago */
                                                                                                                                .OrderByDescending(u => u.AvatarLastRefresh).Take(50).ToList();
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                return null;
            }
        }

        private static bool RefreshAvatar(int portalId, int moduleId, DotNetNuke.Modules.ActiveForums.Services.Avatars.IAvatarProvider avatarProvider, DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo forumUser)
        {
            try
            {
                if (forumUser == null || string.IsNullOrEmpty(forumUser.Email))
                {
                    return false;
                }

                // If the user already has an avatar and it was not updated via this refresh process, they updated themselves, so skip them
                if (!string.IsNullOrEmpty(forumUser.UserInfo.Profile.GetPropertyValue("Photo")))
                {
                    var usersAvatarFileId = Utilities.SafeConvertInt(forumUser.UserInfo.Profile.GetPropertyValue("Photo"), DotNetNuke.Common.Utilities.Null.NullInteger);
                    if (!forumUser.AvatarFileId.HasValue || usersAvatarFileId != forumUser.AvatarFileId.Value)
                    {
                        forumUser.AvatarLastRefresh = DateTime.UtcNow;
                        new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(moduleId: moduleId).Update(forumUser);
                        return false;
                    }
                }

                var (contentType, avatarImage, lastModifiedDateTime) = avatarProvider.GetAvatarImageAsync(forumUser.Email).GetAwaiter().GetResult();
                if (avatarImage != null && avatarImage.Length > 0)
                {
                    // Save avatar image to correct image type file based on contentType
                    var fileExtension = ".img";
                    switch (contentType.ToLowerInvariant())
                    {
                        case "image/png":
                            fileExtension = ".png";
                            break;
                        case "image/jpeg":
                        case "image/jpg":
                            fileExtension = ".jpg";
                            break;
                        case "image/gif":
                            fileExtension = ".gif";
                            break;
                        case "image/bmp":
                            fileExtension = ".bmp";
                            break;
                        case "image/webp":
                            fileExtension = ".webp";
                            break;
                            // Add more types as needed
                    }

                    var fileName = $"avatar_{forumUser.UserInfo.UserID}_{DateTime.UtcNow:yyyyMMddHHmmss}{fileExtension}";

                    var userFolder = DotNetNuke.Services.FileSystem.FolderManager.Instance.GetUserFolder(forumUser.UserInfo);
                    var avatarFile = DotNetNuke.Services.FileSystem.FileManager.Instance.AddFile(folder: userFolder, fileName: fileName, fileContent: new System.IO.MemoryStream(avatarImage), overwrite: true, checkPermissions: false, contentType: contentType, createdByUserID: forumUser.UserId);

                    forumUser.UserInfo.Profile.SetProfileProperty("Photo", avatarFile.FileId.ToString());

                    DotNetNuke.Entities.Users.UserController.UpdateUser(portalId: portalId, user: forumUser.UserInfo, loggedAction: true);

                    var log = new DotNetNuke.Services.Log.EventLog.LogInfo { LogTypeKey = DotNetNuke.Abstractions.Logging.EventLogType.SCHEDULER_EVENT_PROGRESSING.ToString() };
                    log.LogProperties.Add(new LogDetailInfo("Module", Globals.ModuleFriendlyName));
                    var message = string.Format(Utilities.GetSharedResource("[RESX:GravatarRefreshed]"), forumUser.UserInfo.DisplayName);
                    log.AddProperty("Message", message);
                    DotNetNuke.Services.Log.EventLog.LogController.Instance.AddLog(log);

                    forumUser.AvatarSourceLastModified = lastModifiedDateTime;
                    forumUser.AvatarFileId = avatarFile.FileId; // Set the avatar file ID to the new file created
                }

                forumUser.AvatarLastRefresh = DateTime.UtcNow;
                new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(moduleId: moduleId).Update(forumUser);

                return true;
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }

            return false;
        }
    }
}
