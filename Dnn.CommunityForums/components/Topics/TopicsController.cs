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

namespace DotNetNuke.Modules.ActiveForums
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlTypes;
    using System.Globalization;
    using System.Linq;
    using System.Web.UI.WebControls;
    using System.Xml.Linq;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Modules.ActiveForums.Controllers;
    using DotNetNuke.Modules.ActiveForums.Entities;
    using DotNetNuke.Modules.ActiveForums.Extensions;
    using DotNetNuke.Modules.ActiveForums.Helpers;
    using DotNetNuke.Modules.ActiveForums.Services.Cache;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Services.Search.Entities;

    #region Topics Controller
    public class TopicsController : DotNetNuke.Entities.Modules.ModuleSearchBase, DotNetNuke.Entities.Modules.IUpgradeable, DotNetNuke.Entities.Modules.IPortable
    {
        private static readonly DotNetNuke.Instrumentation.ILog Logger = LoggerSource.Instance.GetLogger(typeof(TopicsController));
        private readonly IPortalAliasService portalAliasService;
        private readonly IPortalSettings portalSettings;
        private readonly IPortalController portalController;

        public TopicsController()
            : this(
                  new DotNetNuke.Entities.Portals.PortalAliasController(),
                  ServiceLocator<IPortalController, PortalController>.Instance,
                  ServiceLocator<IPortalController, PortalController>.Instance.GetCurrentSettings())
        {
        }

        public TopicsController(IPortalAliasService portalAliasService, IPortalController portalController, IPortalSettings portalSettings)
        {
            this.portalAliasService = portalAliasService;
            this.portalController = portalController;
            this.portalSettings = portalSettings;
        }

        #region "IPortable"
        public string ExportModule(int moduleID)
        {
            return DotNetNuke.Modules.ActiveForums.Helpers.ImportExportHelper.ExportModule(moduleId: moduleID);
        }

        public void ImportModule(int moduleID, string content, string version, int userID)
        {
            DotNetNuke.Modules.ActiveForums.Helpers.ImportExportHelper.ImportModule(moduleId: moduleID, content: content, version: version, userId: userID);
        }

        #endregion "IPortable"

        #region "ModuleSearchBase"

        public override IList<SearchDocument> GetModifiedSearchDocuments(ModuleInfo moduleInfo, DateTime beginDateUtc)
        {
            /* since this code runs without HttpContext, get https:// by looking at page settings */
            bool isHttps = DotNetNuke.Entities.Tabs.TabController.Instance.GetTab(moduleInfo.TabID, moduleInfo.PortalID).IsSecure;
            bool useFriendlyURLs = Utilities.UseFriendlyURLs(moduleInfo.ModuleID);
            string primaryPortalAlias = this.portalAliasService.GetPortalAliasesByPortalId(moduleInfo.PortalID).FirstOrDefault(x => x.IsPrimary).HttpAlias;

            Dictionary<int, string> authorizedRolesForForum = new Dictionary<int, string>();
            Dictionary<int, string> forumUrlPrefixes = new Dictionary<int, string>();

            List<string> roles = new List<string>();
            foreach (DotNetNuke.Security.Roles.RoleInfo r in DotNetNuke.Security.Roles.RoleController.Instance.GetRoles(portalId: moduleInfo.PortalID))
            {
                roles.Add(r.RoleName);
            }

            var portalSettings = new DotNetNuke.Modules.ActiveForums.Helpers.PortalSettingsHelper().GetPortalSettings(moduleInfo.PortalID);
            string roleIds = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetPortalRoleIds(moduleInfo.PortalID, roles.ToArray());
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
                    body = DotNetNuke.Common.Utilities.HtmlUtils.Clean(body, false);
                    if (!string.IsNullOrEmpty(body))
                    {
                        description = body.Length > 100 ? body.Substring(0, 100) + "..." : body;
                    }

                    DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forumInfo = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Instance.GetById(moduleInfo.ModuleID, forumid);

                    // NOTE: indexer is called from scheduler and has no httpcontext
                    // so any code that relies on HttpContext cannot be used...
                    string link = new ControlUtils().BuildUrl(portalId: moduleInfo.PortalID, tabId: moduleInfo.TabID, moduleId: moduleInfo.ModuleID, groupPrefix: forumGroupUrlPrefix, forumPrefix: forumUrlPrefix, forumGroupId: forumGroupId, forumID: forumid, topicId: topicid, topicURL: topicURL, tagId: -1, categoryId: -1, otherPrefix: string.Empty, pageId: 1, contentId: contentid, socialGroupId: forumInfo.SocialGroupId);
                    if (!string.IsNullOrEmpty(link) && !link.StartsWith("http"))
                    {
                        link = (isHttps ? "https://" : "http://") + primaryPortalAlias + link;
                    }

                    queryString = qsb.Clear().Append($"{ParamKeys.ForumId}={forumid}&{ParamKeys.TopicId}={topicid}&{ParamKeys.ViewType}={Views.Topic}&{ParamKeys.ContentJumpId}={jumpid}").ToString();
                    string permittedRolesCanView = string.Empty;
                    if (!authorizedRolesForForum.TryGetValue(forumid, out permittedRolesCanView))
                    {
                        var delimiter = ";";
                        var viewRolesAsDelimitedString = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIdsForRequestedAccess(moduleId: moduleInfo.ModuleID, permissionsId: forumid, requestedAccess: SecureActions.View).FromHashSetToDelimitedString(delimiter);
                        permittedRolesCanView = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetNamesForRoles(portalSettings: portalSettings, roles: viewRolesAsDelimitedString, delimiter: delimiter);
                        authorizedRolesForForum.Add(forumid, permittedRolesCanView);
                    }

                    var searchDoc = new SearchDocument
                    {
                        UniqueKey = $"{moduleInfo.ModuleID}-{contentid}",
                        ModuleId = moduleInfo.ModuleID,
                        AuthorUserId = authorid,
                        PortalId = moduleInfo.PortalID,
                        Title = subject,
                        Description = description,
                        Body = body,
                        Url = link,
                        QueryString = queryString,
                        ModifiedTimeUtc = dateupdated,
                        Tags = tags.Count > 0 ? tags : null,
                        NumericKeys = new Dictionary<string, int> { { "ForumId", forumid }, { "TopicId", topicid }, { "ReplyId", replyId }, { "ContentId", contentid }, { "AuthorUserId", authorid } },
                        TabId = moduleInfo.TabID,
                        Permissions = permittedRolesCanView,
                        IsActive = isApproved && !isDeleted,
                    };
                    searchDocuments.Add(searchDoc);
                }

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

        #endregion "ModuleSearchBase"

        #region "IUpgradeable"
        public string UpgradeModule(string Version)
        {
            switch (Version)
            {
                case "07.00.07":
                    try
                    {
                        var fc = new ForumsConfig();
                        fc.ArchiveOrphanedAttachments_070007();
                    }
                    catch (Exception ex)
                    {
                        this.LogError(ex.Message, ex);
                        Exceptions.LogException(ex);
                        return "Failed";
                    }

                    break;
                case "07.00.11":
                    try
                    {
                        DotNetNuke.Modules.ActiveForums.Helpers.Upgrades.MoveSettings_070011();
                    }
                    catch (Exception ex)
                    {
                        this.LogError(ex.Message, ex);
                        Exceptions.LogException(ex);
                        return "Failed";
                    }

                    break;
                case "07.00.12":
                    try
                    {
                        ForumsConfig.FillMissingTopicUrls_070012();
                    }
                    catch (Exception ex)
                    {
                        this.LogError(ex.Message, ex);
                        Exceptions.LogException(ex);
                        return "Failed";
                    }

                    break;
                case "08.00.00":
                    try
                    {
                        DotNetNuke.Modules.ActiveForums.Helpers.Upgrades.Upgrade_Templates_080000();
                        var fc = new ForumsConfig();
                        fc.Install_Or_Upgrade_RenameThemeCssFiles_080000();
                        fc.Install_Or_Upgrade_RelocateDefaultThemeToLegacy_080000();
                        ForumsConfig.FillMissingTopicUrls_070012(); /* for anyone upgrading from 07.00.12-> 08.00.00 */
                    }
                    catch (Exception ex)
                    {
                        this.LogError(ex.Message, ex);
                        Exceptions.LogException(ex);
                        return "Failed";
                    }

                    break;
                case "08.01.00":
                    try
                    {
                        DotNetNuke.Modules.ActiveForums.Helpers.Upgrades.DeleteObsoleteModuleSettings_080100();
                        DotNetNuke.Modules.ActiveForums.Helpers.Upgrades.UpgradeSocialGroupForumConfigModuleSettings_080100();
                        ForumsConfig.Install_BanUser_NotificationType_080100();
                    }
                    catch (Exception ex)
                    {
                        this.LogError(ex.Message, ex);
                        Exceptions.LogException(ex);
                        return "Failed";
                    }

                    break;
                case "08.02.00":
                    try
                    {
                        ForumsConfig.Merge_Permissions_080200();
                        DotNetNuke.Modules.ActiveForums.Helpers.Upgrades.UpgradeSocialGroupForumConfigModuleSettings_080200();
                        DotNetNuke.Modules.ActiveForums.Helpers.Upgrades.Upgrade_EmailNotificationSubjectTokens_080200();
                        ForumsConfig.Upgrade_RelocateSqlFiles_080200();
                        ForumsConfig.Install_Upgrade_CreateForumDefaultSettingsAndSecurity_080200();
                        DotNetNuke.Modules.ActiveForums.Helpers.Upgrades.AddUrlPrefixLikes_080200();
                        ForumsConfig.Install_LikeNotificationType_080200();
                        ForumsConfig.Install_PinNotificationType_080200();
                    }
                    catch (Exception ex)
                    {
                        this.LogError(ex.Message, ex);
                        Exceptions.LogException(ex);
                        return "Failed";
                    }

                    break;
                case "09.00.00":
                    try
                    {
                        var log = new DotNetNuke.Services.Log.EventLog.LogInfo { LogTypeKey = DotNetNuke.Abstractions.Logging.EventLogType.ADMIN_ALERT.ToString() };
                        log.LogProperties.Add(new LogDetailInfo("Module", Globals.ModuleFriendlyName));
                        var message = $"Removing obsolete module settings for {Version}";
                        log.AddProperty("Message", message);
                        DotNetNuke.Services.Log.EventLog.LogController.Instance.AddLog(log);
                        DotNetNuke.Modules.ActiveForums.Helpers.Upgrades.DeleteObsoleteModuleSettings_090000();
                        log = new DotNetNuke.Services.Log.EventLog.LogInfo { LogTypeKey = DotNetNuke.Abstractions.Logging.EventLogType.ADMIN_ALERT.ToString() };
                        log.LogProperties.Add(new LogDetailInfo("Module", Globals.ModuleFriendlyName));
                        message = $"Upgrading permissions for {Version}";
                        log.AddProperty("Message", message);
                        DotNetNuke.Services.Log.EventLog.LogController.Instance.AddLog(log);
                        ForumsConfig.Upgrade_PermissionSets_090000();
                    }
                    catch (Exception ex)
                    {
                        this.LogError(ex.Message, ex);
                        Exceptions.LogException(ex);
                        return "Failed";
                    }

                    break;
                case "09.01.00":
                    try
                    {
                        DotNetNuke.Modules.ActiveForums.Helpers.Upgrades.DeleteObsoleteModuleSettings_090100();
                        DotNetNuke.Modules.ActiveForums.Helpers.Upgrades.AddAvatarModuleSettings_090100();
                    }
                    catch (Exception ex)
                    {
                        this.LogError(ex.Message, ex);
                        Exceptions.LogException(ex);
                        return "Failed";
                    }

                    break;
                case "09.02.00":
                    try
                    {
                        ForumsConfig.Install_BadgeNotificationType_090200();
                    }
                    catch (Exception ex)
                    {
                        this.LogError(ex.Message, ex);
                        Exceptions.LogException(ex);
                        return "Failed";
                    }

                    break;
                case "09.02.01":
                    try
                    {
                        DotNetNuke.Modules.ActiveForums.Helpers.Upgrades.UpgradeSocialGroupForumConfigModuleSettings_090201();
                        new ForumsConfig().Install_DefaultBadges_090201(upgrading: true);
                    }
                    catch (Exception ex)
                    {
                        this.LogError(ex.Message, ex);
                        Exceptions.LogException(ex);
                        return "Failed";
                    }

                    break;
                case "09.03.00":
                    try
                    {
                        ForumsConfig.Install_UserMentionNotificationType_090300();
                        DotNetNuke.Modules.ActiveForums.Helpers.Upgrades.UpgradeSocialGroupForumConfigModuleSettings_090300();
                    }
                    catch (Exception ex)
                    {
                        this.LogError(ex.Message, ex);
                        Exceptions.LogException(ex);
                        return "Failed";
                    }

                    break;
                case "09.05.00":
                    try
                    {
                        ForumsConfig.Reset_DNN_Search_Documents_090500();
                    }
                    catch (Exception ex)
                    {
                        this.LogError(ex.Message, ex);
                        Exceptions.LogException(ex);
                        return "Failed";
                    }

                    break;
                case "09.06.00":
                    try
                    {
                        DotNetNuke.Modules.ActiveForums.Helpers.Upgrades.DeleteObsoleteModuleSettings_090600();
                        ForumsConfig.Upgrade_EnsureVanityNames_090600();
                    }
                    catch (Exception ex)
                    {
                        this.LogError(ex.Message, ex);
                        Exceptions.LogException(ex);
                        return "Failed";
                    }

                    break;
                case "09.07.00":
                    try
                    {
                        var fc = new ForumsConfig();
                        fc.RemoveLegacyAvatarsFolder_090700();
                        fc.RelocateAttachments_090700();
                    }
                    catch (Exception ex)
                    {
                        this.LogError(ex.Message, ex);
                        Exceptions.LogException(ex);
                        return "Failed";
                    }

                    break;
                case "10.00.00":
                    try
                    {
                        DotNetNuke.Modules.ActiveForums.Helpers.Upgrades.DeleteObsoleteModuleSettings_100000();
                        DotNetNuke.Modules.ActiveForums.Helpers.Upgrades.Remove_TemplatesTable_100000();

                        /* ensure permissions are upgraded anyone upgrading from earlier than 09.00.00 to 10.00.00; 
                         * "GroupKey" on settings table was changed to "SettingsKey" in 09.02.00 so the 09.00.00 upgrade task failed for anyone upgrading from earlier than 09.00.00 to 09.02.00->09.08.00;
                         * ALSO handles additional "Mention" permission */
                        DotNetNuke.Modules.ActiveForums.Helpers.Upgrades.Upgrade_PermissionSets_100000();
                    }
                    catch (Exception ex)
                    {
                        this.LogError(ex.Message, ex);
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
