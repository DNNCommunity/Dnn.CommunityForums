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
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Linq;
using System.Text.RegularExpressions;
using DotNetNuke.Common.Controls;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Instrumentation;
using DotNetNuke.Modules.ActiveForums.Data;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Journal;
using DotNetNuke.Services.Search.Entities;
using log4net;

namespace DotNetNuke.Modules.ActiveForums
{
    #region Topics Controller
    public class TopicsController : DotNetNuke.Entities.Modules.ModuleSearchBase, DotNetNuke.Entities.Modules.IUpgradeable
    {
        private static readonly DotNetNuke.Instrumentation.ILog Logger = LoggerSource.Instance.GetLogger(typeof(TopicsController));
        [Obsolete("Deprecated in Community Forums. Scheduled removal in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.TopicController.QuickCreate()")]
        public int Topic_QuickCreate(int PortalId, int ModuleId, int ForumId, string Subject, string Body, int UserId, string DisplayName, bool IsApproved, string IPAddress) => DotNetNuke.Modules.ActiveForums.Controllers.TopicController.QuickCreate(PortalId, ModuleId, ForumId, Subject, Body, UserId, DisplayName, IsApproved, IPAddress);
        [Obsolete("Deprecated in Community Forums. Scheduled removal in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Replies_Split()")]
        public void Replies_Split(int OldTopicId, int NewTopicId, string listreplies, bool isNew) => DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Replies_Split(OldTopicId, NewTopicId, listreplies, isNew);
        [Obsolete("Deprecated in Community Forums. Scheduled removal in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Save(TopicInfo ti)")]
        public int TopicSave(int PortalId, TopicInfo ti) => DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Save(ti);
        [Obsolete(message: "Deprecated in Community Forums. Scheduled removal in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Save(TopicInfo ti)")]
        public int TopicSave(int PortalId, int ModuleId, DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti) => DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Save(ti);
        [Obsolete(message: "Deprecated in Community Forums. Scheduled removal in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.TopicController.SaveToForum(int ModuleId, int ForumId, int TopicId, int LastReplyId)")]
        public int Topics_SaveToForum(int ForumId, int TopicId, int PortalId, int ModuleId) => DotNetNuke.Modules.ActiveForums.Controllers.TopicController.SaveToForum(ModuleId, ForumId, TopicId, -1);
        [Obsolete(message: "Deprecated in Community Forums. Scheduled removal in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.TopicController.SaveToForum(int ModuleId, int ForumId, int TopicId, int LastReplyId)")]
        public int Topics_SaveToForum(int ForumId, int TopicId, int PortalId, int ModuleId, int LastReplyId) => DotNetNuke.Modules.ActiveForums.Controllers.TopicController.SaveToForum(ModuleId, ForumId, TopicId, LastReplyId);
        [Obsolete("Deprecated in Community Forums. Scheduled removal in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.TopicController.GetById(int TopicId)")]
        public DotNetNuke.Modules.ActiveForums.Entities.TopicInfo Topics_Get(int PortalId, int ModuleId, int TopicId) => new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(TopicId);
        [Obsolete("Deprecated in Community Forums. Scheduled removal in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.TopicController.GetById(int TopicId)")]
        public DotNetNuke.Modules.ActiveForums.Entities.TopicInfo Topics_Get(int PortalId, int ModuleId, int TopicId, int ForumId, int UserId, bool WithSecurity) => new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(TopicId);
        [Obsolete("Deprecated in Community Forums. Scheduled removal in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.TopicController.DeleteById(int TopicId)")]
        public void Topics_Delete(int PortalId, int ModuleId, int ForumId, int TopicId, int DelBehavior) => new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().DeleteById(TopicId);
        [Obsolete(message: "Deprecated in Community Forums. Scheduled removal in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Move(int TopicId, int NewForumId)")] 
        public void Topics_Move(int PortalId, int ModuleId, int ForumId, int TopicId) => DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Move(TopicId, ForumId);
        [Obsolete(message: "Deprecated in Community Forums. Scheduled removal in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Approve(int TopicId)")]
        public DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ApproveTopic(int PortalId, int TabId, int ModuleId, int ForumId, int TopicId) => DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Approve(TopicId);
        [Obsolete("Deprecated in Community Forums. Moved to Utilities and changed to internal in 10.00.00.")]
        public void UpdateModuleLastContentModifiedOnDate(int ModuleId) => Utilities.UpdateModuleLastContentModifiedOnDate(ModuleId);

        #region ModuleSearchBase

        public override IList<SearchDocument> GetModifiedSearchDocuments(ModuleInfo moduleInfo, DateTime beginDateUtc)
        {
            var ms = new SettingsInfo { MainSettings = moduleInfo.ModuleSettings };
            /* if not using soft deletes, remove and rebuild entire index; 
			   note that this "internals" method is suggested by blog post (https://www.dnnsoftware.com/community-blog/cid/154913/integrating-with-search-introducing-modulesearchbase#Comment106)
			   and also is used by the Community Links module (https://github.com/DNNCommunity/DNN.Links/blob/development/Components/FeatureController.cs)
			*/
            if (ms.DeleteBehavior != 1)
            {
                DotNetNuke.Services.Search.Internals.InternalSearchController.Instance.DeleteSearchDocumentsByModule(moduleInfo.PortalID, moduleInfo.ModuleID, moduleInfo.ModuleDefID);
                beginDateUtc = SqlDateTime.MinValue.Value.AddDays(1);
            }

            /* since this code runs without HttpContext, get https:// by looking at page settings */
            bool isHttps = DotNetNuke.Entities.Tabs.TabController.Instance.GetTab(moduleInfo.TabID, moduleInfo.PortalID).IsSecure;
            bool useFriendlyURLs = Utilities.UseFriendlyURLs(moduleInfo.ModuleID);
            string primaryPortalAlias = DotNetNuke.Entities.Portals.PortalAliasController.Instance.GetPortalAliasesByPortalId(moduleInfo.PortalID).FirstOrDefault(x => x.IsPrimary).HTTPAlias;

            Dictionary<int, string> AuthorizedRolesForForum = new Dictionary<int, string>();
            Dictionary<int, string> ForumUrlPrefixes = new Dictionary<int, string>();

            List<string> roles = new List<string>();
            foreach (DotNetNuke.Security.Roles.RoleInfo r in DotNetNuke.Security.Roles.RoleController.Instance.GetRoles(portalId: moduleInfo.PortalID))
            {
                roles.Add(r.RoleName);
            }
            string roleIds = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIds(moduleInfo.PortalID, roles.ToArray());
            roleIds = string.Concat(roleIds, DotNetNuke.Common.Globals.glbRoleAllUsers, ";", DotNetNuke.Common.Globals.glbRoleUnauthUser);
            string queryString = string.Empty;
            System.Text.StringBuilder qsb = new System.Text.StringBuilder();
            List<SearchDocument> searchDocuments = new List<SearchDocument>();
            IDataReader dr = null;
            try
            {
                dr = DataProvider.Instance().Search_DotNetNuke(moduleInfo.ModuleID, beginDateUtc);
                while (dr.Read())
                {
                    string subject = dr["Subject"].ToString();
                    string description = string.Empty;
                    string body = dr["Body"].ToString();
                    List<string> tags = dr["Tags"].ToString().Split(",".ToCharArray()).ToList();
                    DateTime dateupdated = Convert.ToDateTime(dr["DateUpdated"]);
                    int authorid = Convert.ToInt32(dr["AuthorId"]);
                    bool isDeleted = Convert.ToBoolean(dr["isDeleted"]);
                    bool isApproved = Convert.ToBoolean(dr["isApproved"]);
                    int contentid = Convert.ToInt32(dr["ContentId"]);
                    int forumid = Convert.ToInt32(dr["ForumId"]);
                    int forumGroupId = Convert.ToInt32(dr["ForumGroupId"]);
                    int topicid = Convert.ToInt32(dr["TopicId"]);
                    int replyId = Convert.ToInt32(dr["ReplyId"]);
                    string topicURL = dr["TopicUrl"].ToString();
                    string forumGroupUrlPrefix = dr["ForumGroupUrlPrefix"].ToString();
                    string forumUrlPrefix = dr["ForumUrlPrefix"].ToString();
                    int jumpid = (replyId > 0) ? replyId : topicid;
                    body = Common.Utilities.HtmlUtils.Clean(body, false);
                    if (!(string.IsNullOrEmpty(body)))
                    {
                        description = body.Length > 100 ? body.Substring(0, 100) + "..." : body;
                    };
                    DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forumInfo = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(forumid, moduleInfo.ModuleID);

                    // NOTE: indexer is called from scheduler and has no httpcontext 
                    // so any code that relies on HttpContext cannot be used...

                    string link = new ControlUtils().BuildUrl(moduleInfo.TabID, moduleInfo.ModuleID, forumGroupUrlPrefix, forumUrlPrefix, forumGroupId, forumid, topicid, topicURL, -1, -1, string.Empty, 1, contentid, forumInfo.SocialGroupId);
                    if (!(string.IsNullOrEmpty(link)) && !(link.StartsWith("http")))
                    {
                        link = (isHttps ? "https://" : "http://") + primaryPortalAlias + link;
                    }
                    queryString = qsb.Clear().Append(ParamKeys.ForumId).Append("=").Append(forumid).Append("&").Append(ParamKeys.TopicId).Append("=").Append(topicid).Append("&").Append(ParamKeys.ViewType).Append("=").Append(Views.Topic).Append("&").Append(ParamKeys.ContentJumpId).Append("=").Append(jumpid).ToString();
                    string permittedRolesCanView = string.Empty;
                    if (!AuthorizedRolesForForum.TryGetValue(forumid, out permittedRolesCanView))
                    {
                        string canView = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.WhichRolesCanViewForum(moduleInfo.ModuleID, forumid, roleIds);
                        permittedRolesCanView = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetNamesForRoles(moduleInfo.PortalID, string.Join(";", canView.Split(":".ToCharArray())));
                        AuthorizedRolesForForum.Add(forumid, permittedRolesCanView);
                    }
                    var searchDoc = new SearchDocument
                    {
                        UniqueKey = moduleInfo.ModuleID.ToString() + "-" + contentid.ToString(),
                        AuthorUserId = authorid,
                        PortalId = moduleInfo.PortalID,
                        Title = subject,
                        Description = description,
                        Body = body,
                        Url = link,
                        QueryString = queryString,
                        ModifiedTimeUtc = dateupdated,
                        Tags = (tags.Count > 0 ? tags : null),
                        Permissions = permittedRolesCanView,
                        IsActive = (isApproved && !isDeleted)
                    };
                    searchDocuments.Add(searchDoc);
                };
                dr.Close();
                return searchDocuments;
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return null;
            }
            finally
            {
                if (dr != null)
                {
                    if (!dr.IsClosed)
                    {
                        dr.Close();
                    }
                }
            }

        }
        #endregion

        #region "IUpgradeable"
        public string UpgradeModule(string Version)
        {
            switch (Version)
            {
                case "07.00.07":
                    try
                    {
                        var fc = new ForumsConfig();
                        fc.ArchiveOrphanedAttachments();
                    }
                    catch (Exception ex)
                    {
                        LogError(ex.Message, ex);
                        Exceptions.LogException(ex);
                        return "Failed";
                    }

                    break;
                case "07.00.11":
                    try
                    {
                        DotNetNuke.Modules.ActiveForums.Helpers.UpgradeModuleSettings.MoveSettings_070011();
                    }
                    catch (Exception ex)
                    {
                        LogError(ex.Message, ex);
                        Exceptions.LogException(ex);
                        return "Failed";
                    }

                    break;
                case "07.00.12":
                    try
                    {
                        ForumsConfig.FillMissingTopicUrls();
                    }
                    catch (Exception ex)
                    {
                        LogError(ex.Message, ex);
                        Exceptions.LogException(ex);
                        return "Failed";
                    }

                    break;
                case "08.00.00":
                    try
                    {
                        var fc = new ForumsConfig();
                        fc.Install_Or_Upgrade_MoveTemplates();
                        fc.Install_Or_Upgrade_RenameThemeCssFiles();
                        fc.Install_Or_Upgrade_RelocateDefaultThemeToLegacy();
                        ForumsConfig.FillMissingTopicUrls(); /* for anyone upgrading from 07.00.12-> 08.00.00 */
                    }
                    catch (Exception ex)
                    {
                        LogError(ex.Message, ex);
                        Exceptions.LogException(ex);
                        return "Failed";
                    }

                    break;
                case "08.01.00":
                    try
                    {
                        DotNetNuke.Modules.ActiveForums.Helpers.UpgradeModuleSettings.DeleteObsoleteModuleSettings_080100();
                        DotNetNuke.Modules.ActiveForums.Helpers.UpgradeModuleSettings.UpgradeSocialGroupForumConfigModuleSettings_080100();
                        ForumsConfig.Install_BanUser_NotificationType_080100();
                    }
                    catch (Exception ex)
                    {
                        LogError(ex.Message, ex);
                        Exceptions.LogException(ex);
                        return "Failed";
                    }

                    break;
                default:
                    break;
            }
            return Version;
        }
        private void LogError(string message, Exception ex)
        {
            if (ex != null)
            {
                Logger.Error(message, ex);
                if (ex.InnerException != null)
                {
                    Logger.Error(ex.InnerException.Message, ex.InnerException);
                }
            }
            else
            {
                Logger.Error(message);
            }
        }
        #endregion

    }

    #endregion
}

