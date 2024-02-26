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
using DotNetNuke.Data;
using DotNetNuke.Modules.ActiveForums.Data;
using DotNetNuke.Modules.ActiveForums.Entities;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Journal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace DotNetNuke.Modules.ActiveForums.Controllers
{
    internal class TopicController : DotNetNuke.Modules.ActiveForums.Controllers.RepositoryControllerBase<DotNetNuke.Modules.ActiveForums.Entities.TopicInfo>
    {
        public DotNetNuke.Modules.ActiveForums.Entities.TopicInfo GetById(int topicId)
        {
            DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti = base.GetById(topicId);
            if (ti != null)
            {
                if (ti.Content == null)
                {
                    ti.Content = new DotNetNuke.Modules.ActiveForums.Controllers.ContentController().GetById(ti.ContentId);
                }
                //forum.ForumGroup = forum.ForumGroupId > 0 ? new DotNetNuke.Modules.ActiveForums.Controllers.ForumGroupController().GetForumGroup(moduleId, forum.ForumGroupId) : null;
                //forum.ForumSettings = (Hashtable)DataCache.GetSettings(moduleId, forum.ForumSettingsKey, string.Format(CacheKeys.ForumSettingsByKey, moduleId, forum.ForumSettingsKey), !ignoreCache);
                //forum.Security = new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController().GetById(forum.PermissionsId);
                //if (forum.HasProperties)
                //{
                //    var propC = new PropertiesController();
                //    forum.Properties = propC.ListProperties(portalId, 1, forumId);
                //}
            }
            return ti;
        }
        public int Save(DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti, int id)
        {
            UserProfileController.Profiles_ClearCache(ti.ModuleId, ti.Content.AuthorId);
            if (ti.IsApproved && ti.Author.AuthorId > 0)
            {
                var uc = new Data.Profiles();
                uc.Profile_UpdateTopicCount(ti.Forum.PortalId, ti.Author.AuthorId);
            }
            //TODO: convert to use DAL2
            return Convert.ToInt32(DataProvider.Instance().Topics_Save(ti.Forum.PortalId, ti.TopicId, ti.ViewCount, ti.ReplyCount, ti.IsLocked, ti.IsPinned, ti.TopicIcon, ti.StatusId, ti.IsApproved, ti.IsDeleted, ti.IsAnnounce, ti.IsArchived, ti.AnnounceStart, ti.AnnounceEnd, ti.Content.Subject.Trim(), ti.Content.Body.Trim(), ti.Content.Summary.Trim(), ti.Content.DateCreated, ti.Content.DateUpdated, ti.Content.AuthorId, ti.Content.AuthorName, ti.Content.IPAddress, (int)ti.TopicType, ti.Priority, ti.TopicUrl, ti.TopicData));
        }
        public void DeleteById(int TopicId)
        {
            DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(TopicId);
            if (ti != null)
            {
                base.DeleteById(TopicId);
                Utilities.UpdateModuleLastContentModifiedOnDate(ti.ModuleId);
                DataCache.CacheClearPrefix(ti.ModuleId, string.Format(CacheKeys.ForumViewPrefix, ti.ModuleId));
                try
                {
                    var objectKey = string.Format("{0}:{1}", ti.ForumId, TopicId);
                    JournalController.Instance.DeleteJournalItemByKey(ti.PortalId, objectKey);
                }
                catch (Exception ex)
                {
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                }

                if (SettingsBase.GetModuleSettings(ti.ModuleId).DeleteBehavior != 0)
                    return;

                // If it's a hard delete, delete associated attachments
                var attachmentController = new Data.AttachController();
                var fileManager = FileManager.Instance;
                var folderManager = FolderManager.Instance;
                var attachmentFolder = folderManager.GetFolder(ti.PortalId, "activeforums_Attach");

                foreach (var attachment in attachmentController.ListForPost(TopicId, null))
                {
                    attachmentController.Delete(attachment.AttachmentId);

                    var file = attachment.FileId.HasValue ? fileManager.GetFile(attachment.FileId.Value) : fileManager.GetFile(attachmentFolder, attachment.FileName);

                    // Only delete the file if it exists in the attachment folder
                    if (file != null && file.FolderId == attachmentFolder.FolderID)
                        fileManager.DeleteFile(file);
                }
            }

        }
    }
    
}
