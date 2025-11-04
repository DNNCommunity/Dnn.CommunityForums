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

using System.Web.UI;

namespace DotNetNuke.Modules.ActiveForums
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Web.UI.WebControls;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Modules.ActiveForums.Data;
    using DotNetNuke.Modules.ActiveForums.Entities;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Services.Social.Notifications;
    using Microsoft.ApplicationBlocks.Data;

    public class ForumsConfig
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ForumsConfig));

        public string sPath = DotNetNuke.Modules.ActiveForums.Utilities.MapPath(string.Concat(Globals.ModulePath, "config/defaultsetup.config"));

        public bool ForumsInit(int portalId, int moduleId)
        {
            try
            {
                // Initial Settings
                this.LoadSettings(portalId, moduleId);

                // Add Default Status
                this.LoadFilters(portalId, moduleId);

                // Add Default Steps
                this.LoadRanks(portalId, moduleId);

                // Add Default Forums
                this.LoadDefaultForums(portalId, moduleId);

                // Create "User Banned" core messaging notification type new in 08.01.00
                ForumsConfig.Install_BanUser_NotificationType_080100();

                // Create "like notification" core messaging notification type new in 08.02.00
                ForumsConfig.Install_LikeNotificationType_080200();

                // Create "Pin notification" core messaging notification type new in 08.02.00
                ForumsConfig.Install_PinNotificationType_080200();

                // Create "badge notification" core messaging notification type new in 09.02.00
                ForumsConfig.Install_BadgeNotificationType_090200();

                // Create default badges new in 09.02.00
                new ForumsConfig().Install_DefaultBadges_090201();
                return true;
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                return false;
            }
        }

        private void LoadSettings(int portalId, int moduleId)
        {
            try
            {
                var objModules = new DotNetNuke.Entities.Modules.ModuleController();
                var xDoc = new System.Xml.XmlDocument();
                xDoc.Load(this.sPath);
                if (xDoc != null)
                {
                    System.Xml.XmlNode xRoot = xDoc.DocumentElement;
                    System.Xml.XmlNodeList xNodeList = xRoot.SelectNodes("//mainsettings/setting");
                    if (xNodeList.Count > 0)
                    {
                        int i;
                        for (i = 0; i < xNodeList.Count; i++)
                        {
                            objModules.UpdateModuleSetting(moduleId, xNodeList[i].Attributes["name"].Value, xNodeList[i].Attributes["value"].Value);
                        }
                    }
                }

                objModules.UpdateModuleSetting(moduleId, SettingKeys.IsInstalled, "True");
                objModules.UpdateModuleSetting(moduleId, "NeedsConvert", "False");
                try
                {
                    System.Globalization.DateTimeFormatInfo nfi = new System.Globalization.CultureInfo("en-US", true).DateTimeFormat;

                    objModules.UpdateModuleSetting(moduleId, SettingKeys.InstallDate, DateTime.UtcNow.ToString(new System.Globalization.CultureInfo("en-US")));
                }
                catch (Exception ex)
                {
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void LoadFilters(int portalId, int moduleId)
        {
            DotNetNuke.Modules.ActiveForums.Controllers.FilterController.ImportFilter(portalId, moduleId);
        }

        private void LoadRanks(int portalId, int moduleId)
        {
            try
            {
                var xDoc = new System.Xml.XmlDocument();
                xDoc.Load(this.sPath);
                if (xDoc != null)
                {
                    System.Xml.XmlNode xRoot = xDoc.DocumentElement;
                    System.Xml.XmlNodeList xNodeList = xRoot.SelectNodes("//ranks/rank");
                    if (xNodeList.Count > 0)
                    {
                        int i;
                        for (i = 0; i < xNodeList.Count; i++)
                        {
                            DataProvider.Instance().Ranks_Save(portalId, moduleId, -1, xNodeList[i].Attributes["rankname"].Value, Convert.ToInt32(xNodeList[i].Attributes["rankmin"].Value), Convert.ToInt32(xNodeList[i].Attributes["rankmax"].Value), xNodeList[i].Attributes["rankimage"].Value);
                        }
                    }
                }
            }
            catch
            {
                // do nothing?
            }
        }

        private void LoadDefaultForums(int portalId, int moduleId)
        {
            var xDoc = new System.Xml.XmlDocument();
            xDoc.Load(this.sPath);
            if (xDoc != null)
            {
                System.Xml.XmlNode xRoot = xDoc.DocumentElement;
                System.Xml.XmlNodeList xNodeList = xRoot.SelectNodes("//defaultforums/groups/group");
                if (xNodeList.Count > 0)
                {

                    Install_Upgrade_ForumDefaultSettingsAndSecurity(portalId: portalId, moduleId: moduleId);
                    var portalSettings = DotNetNuke.Modules.ActiveForums.Utilities.GetPortalSettings(portalId: portalId);
                    for (int i = 0; i < xNodeList.Count; i++)
                    {
                        var gi = new DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo
                        {
                            ModuleId = moduleId,
                            ForumGroupId = -1,
                            GroupName = xNodeList[i].Attributes["groupname"].Value,
                            PrefixURL = xNodeList[i].Attributes["prefixurl"].Value,
                            Active = xNodeList[i].Attributes["active"].Value == "1",
                            Hidden = xNodeList[i].Attributes["hidden"].Value == "1",
                            SortOrder = i,
                            GroupSettingsKey = $"M{moduleId}",
                            PermissionsId = SettingsBase.GetModuleSettings(moduleId).DefaultPermissionId,
                        };
                        var gc = new DotNetNuke.Modules.ActiveForums.Controllers.ForumGroupController();
                        int groupId = gc.Groups_Save(portalId, gi, true, true, true);
                        gi = gc.GetById(groupId, moduleId);
                        if (groupId != -1)
                        {
                            if (xNodeList[i].HasChildNodes)
                            {
                                System.Xml.XmlNodeList cNodes = xNodeList[i].ChildNodes;
                                for (int c = 0; c < cNodes.Count; c++)
                                {
                                    var fi = new DotNetNuke.Modules.ActiveForums.Entities.ForumInfo(portalSettings)
                                    {
                                        ForumID = -1,
                                        ModuleId = moduleId,
                                        ForumGroupId = groupId,
                                        ParentForumId = 0,
                                        ForumName = cNodes[c].Attributes["forumname"].Value,
                                        ForumDesc = cNodes[c].Attributes["forumdesc"].Value,
                                        PrefixURL = cNodes[c].Attributes["prefixurl"].Value,
                                        Active = cNodes[c].Attributes["active"].Value == "1",
                                        Hidden = cNodes[c].Attributes["hidden"].Value == "1",
                                        SortOrder = c,
                                        ForumSettingsKey = $"G{groupId}",
                                        PermissionsId = SettingsBase.GetModuleSettings(moduleId).DefaultPermissionId,
                                    };
                                    new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().Forums_Save(portalId, fi, true, true, true);
                                }
                            }
                        }
                    }
                }
            }
        }

        internal void Install_Or_Upgrade_RelocateDefaultThemeToLegacy_080000()
        {
            try
            {
                DotNetNuke.Modules.ActiveForums.Utilities.CopyFolder(new System.IO.DirectoryInfo(Utilities.MapPath(Globals.ThemesPath + "_default")), new System.IO.DirectoryInfo(Utilities.MapPath(Globals.ThemesPath + "_legacy")));
                DotNetNuke.Modules.ActiveForums.Utilities.DeleteFolder(new System.IO.DirectoryInfo(Utilities.MapPath(Globals.ThemesPath + "_default")));
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        internal void Install_Or_Upgrade_RenameThemeCssFiles_080000()
        {
            try
            {
                var di = new System.IO.DirectoryInfo(Utilities.MapPath(Globals.ThemesPath));
                System.IO.DirectoryInfo[] themeFolders = di.GetDirectories();
                foreach (System.IO.DirectoryInfo themeFolder in themeFolders)
                {
                    foreach (var fullFilePathName in System.IO.Directory.EnumerateFiles(path: themeFolder.FullName, searchPattern: "module.css", searchOption: System.IO.SearchOption.TopDirectoryOnly))
                    {
                        System.IO.File.Copy(fullFilePathName, fullFilePathName.Replace("module.css", "theme.css"), true);
                        System.IO.File.Delete(fullFilePathName);
                    }
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        internal void Upgrade_Templates_080000()
        {
            if (!System.IO.Directory.Exists(Utilities.MapPath(Globals.TemplatesPath)))
            {
                System.IO.Directory.CreateDirectory(Utilities.MapPath(Globals.TemplatesPath));
            }

            if (!System.IO.Directory.Exists(Utilities.MapPath(Globals.DefaultTemplatePath)))
            {
                System.IO.Directory.CreateDirectory(Utilities.MapPath(Globals.DefaultTemplatePath));
            }

            var di = new System.IO.DirectoryInfo(Utilities.MapPath(Globals.ThemesPath));
            System.IO.DirectoryInfo[] themeFolders = di.GetDirectories();
            foreach (System.IO.DirectoryInfo themeFolder in themeFolders)
            {
                if (!System.IO.Directory.Exists(themeFolder.FullName + "/templates"))
                {
                    System.IO.Directory.CreateDirectory(themeFolder.FullName + "/templates");
                    TemplateController tc = new TemplateController();
                    foreach (TemplateInfo templateInfo in tc.Template_List(-1, -1))
                    {
                        /* during upgrade, explicitly (re-)load template text from database rather than Template_List API since API loads template using fallback/default logic and doesn't yet have the upgraded template text */
                        /* if installing version 8.2 or greater, only convert specific templates */
                        if ((Globals.ModuleVersion < new Version(8, 2)) || 
                            ((templateInfo.TemplateType == Templates.TemplateTypes.ForumView) || 
                             (templateInfo.TemplateType == Templates.TemplateTypes.TopicView) || 
                             (templateInfo.TemplateType == Templates.TemplateTypes.TopicsView) || 
                             (templateInfo.TemplateType == Templates.TemplateTypes.TopicForm) || 
                             (templateInfo.TemplateType == Templates.TemplateTypes.ReplyForm) || 
                             (templateInfo.TemplateType == Templates.TemplateTypes.Profile) || 
                             (templateInfo.TemplateType == Templates.TemplateTypes.PostInfo) || 
                             (templateInfo.TemplateType == Templates.TemplateTypes.QuickReplyForm))
                            )
                        {
                            IDataReader dr = DataProvider.Instance().Templates_Get(templateInfo.TemplateId, templateInfo.PortalId, templateInfo.ModuleId);
                            while (dr.Read())
                            {
                                try
                                {
                                    /* convert only legacy html portion of the template and save without encoding */
                                    string template = Convert.ToString(dr["Template"]).Replace("[TRESX:", "[RESX:");
                                    if (template.Contains("<html>"))
                                    {
                                        string sHTML;
                                        var xDoc = new System.Xml.XmlDocument();
                                        xDoc.LoadXml(template);
                                        System.Xml.XmlNode xNode;
                                        System.Xml.XmlNode xRoot = xDoc.DocumentElement;
                                        xNode = xRoot.SelectSingleNode("/template/html");
                                        sHTML = xNode.InnerText;
                                        template = sHTML;
                                    }

                                    templateInfo.Template = System.Net.WebUtility.HtmlDecode(template);
                                    tc.Template_Save(templateInfo);
                                }
                                catch (Exception ex)
                                {
                                    DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                                }
                            }
                        }
                    }
                }
            }
        }

        internal void ArchiveOrphanedAttachments()
        {
            var di = new System.IO.DirectoryInfo(DotNetNuke.Modules.ActiveForums.Utilities.MapPath("~/portals"));
            System.IO.DirectoryInfo[] attachmentFolders = di.GetDirectories("activeforums_Attach", System.IO.SearchOption.AllDirectories);

            foreach (System.IO.DirectoryInfo attachmentFolder in attachmentFolders)
            {
                if (!System.IO.Directory.Exists(string.Concat(attachmentFolder.FullName, "\\orphaned")))
                {
                    System.IO.Directory.CreateDirectory(string.Concat(attachmentFolder.FullName, "\\orphaned"));
                }

                List<string> attachmentFullFileNames = System.IO.Directory.EnumerateFiles(path: attachmentFolder.FullName, searchPattern: "*", searchOption: System.IO.SearchOption.AllDirectories).ToList<string>();
                List<string> attachmentFileNames = new List<string>();

                foreach (string attachmentFileName in attachmentFullFileNames)
                {
                    attachmentFileNames.Add(new System.IO.FileInfo(attachmentFileName).Name);
                }

                List<string> databaseFileNames = new List<string>();
                string connectionString = new Connection().connectionString;
                string dbPrefix = new Connection().dbPrefix;

                using (IDataReader dr = SqlHelper.ExecuteReader(connectionString, CommandType.Text, $"SELECT FileName FROM {dbPrefix}Attachments ORDER BY FileName"))
                {
                    while (dr.Read())
                    {
                        databaseFileNames.Add(Utilities.SafeConvertString(dr["FileName"]));
                    }

                    dr.Close();
                }

                foreach (string attachmentFileName in attachmentFileNames)
                {
                    if (!databaseFileNames.Contains(attachmentFileName))
                    {
                        System.IO.File.Copy(string.Concat(attachmentFolder.FullName, "\\", attachmentFileName), string.Concat(attachmentFolder.FullName, "\\orphaned\\", attachmentFileName), true);
                        System.IO.File.Delete(string.Concat(attachmentFolder.FullName, "\\", attachmentFileName));
                    }
                }
            }
        }

        internal static void FillMissingTopicUrls_070012()
        {
            string connectionString = new Connection().connectionString;
            string dbPrefix = new Connection().dbPrefix;

            using (IDataReader dr = SqlHelper.ExecuteReader(connectionString, CommandType.Text, $"SELECT f.PortalId,f.ModuleId,ft.ForumId,t.topicId,c.Subject FROM {dbPrefix}Topics t INNER JOIN {dbPrefix}ForumTopics ft ON ft.TopicId = t.TopicId INNER JOIN {dbPrefix}Content c ON c.ContentId = t.ContentId INNER JOIN {dbPrefix}Forums f ON f.ForumId = ft.ForumId WHERE t.URL = ''"))
            {
                while (dr.Read())
                {
                    int portalId = Utilities.SafeConvertInt(dr["PortalId"]);
                    int moduleId = Utilities.SafeConvertInt(dr["ModuleId"]);
                    int forumId = Utilities.SafeConvertInt(dr["ForumId"]);
                    int topicId = Utilities.SafeConvertInt(dr["TopicId"]);
                    string subject = Utilities.SafeConvertString(dr["Subject"]);
                    DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forumInfo = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(forumId, moduleId);
                    var tc = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController(moduleId);
                    DotNetNuke.Modules.ActiveForums.Entities.TopicInfo topicInfo = tc.GetById(topicId);
                    topicInfo.TopicUrl = DotNetNuke.Modules.ActiveForums.Controllers.UrlController.BuildTopicUrlSegment(portalId: portalId, moduleId: moduleId, topicId: topicId, subject: subject, forumInfo: forumInfo);
                    tc.Update(topicInfo);
                }

                dr.Close();
            }
        }

        internal static void Install_BanUser_NotificationType_080100()
        {
            string notificationTypeName = Globals.BanUserNotificationType;
            string notificationTypeDescription = Globals.BanUserNotificationTypeDescription;
            int deskModuleId = DesktopModuleController.GetDesktopModuleByFriendlyName(Globals.ModuleFriendlyName).DesktopModuleID;

            NotificationType type = new NotificationType { Name = notificationTypeName, Description = notificationTypeDescription, DesktopModuleId = deskModuleId };
            if (NotificationsController.Instance.GetNotificationType(notificationTypeName) == null)
            {
                NotificationsController.Instance.CreateNotificationType(type);
            }
        }

        internal static void Merge_Permissions_080200()
        {
            /* SQL for 08.02.00 will append two permissions sets being merged and put "::::" between them, ex. 0;|||::::2;|||
             * this method will separate them into two pieces, 0;||| and 2;||| and then merge them back together as 0;2;|||
             */
            foreach (var perms in new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController().Get())
            {
                string unmergedPerms = perms.Lock;
                perms.Lock = Merge_PermSet_080200(perms.Lock);
                new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController().Update(perms);
                var log = new DotNetNuke.Services.Log.EventLog.LogInfo { LogTypeKey = DotNetNuke.Abstractions.Logging.EventLogType.ADMIN_ALERT.ToString() };
                log.LogProperties.Add(new LogDetailInfo("Module", Globals.ModuleFriendlyName));
                string message = $"Merged LOCK permissions from: {unmergedPerms} to {perms.Lock}";
                log.AddProperty("Message", message);
                DotNetNuke.Services.Log.EventLog.LogController.Instance.AddLog(log);

                unmergedPerms = perms.Pin;
                perms.Pin = Merge_PermSet_080200(perms.Pin);
                new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController().Update(perms);
                log = new DotNetNuke.Services.Log.EventLog.LogInfo { LogTypeKey = DotNetNuke.Abstractions.Logging.EventLogType.ADMIN_ALERT.ToString() };
                log.LogProperties.Add(new LogDetailInfo("Module", Globals.ModuleFriendlyName));
                message = $"Merged Pin permissions from: {unmergedPerms} to {perms.Pin}";
                log.AddProperty("Message", message);
                DotNetNuke.Services.Log.EventLog.LogController.Instance.AddLog(log);

                unmergedPerms = perms.Delete;
                perms.Delete = Merge_PermSet_080200(perms.Delete);
                new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController().Update(perms);
                log = new DotNetNuke.Services.Log.EventLog.LogInfo { LogTypeKey = DotNetNuke.Abstractions.Logging.EventLogType.ADMIN_ALERT.ToString() };
                log.LogProperties.Add(new LogDetailInfo("Module", Globals.ModuleFriendlyName));
                message = $"Merged Delete permissions from: {unmergedPerms} to {perms.Delete}";
                log.AddProperty("Message", message);
                DotNetNuke.Services.Log.EventLog.LogController.Instance.AddLog(log);

                unmergedPerms = perms.Edit;
                perms.Edit = Merge_PermSet_080200(perms.Edit);
                new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController().Update(perms);
                log = new DotNetNuke.Services.Log.EventLog.LogInfo { LogTypeKey = DotNetNuke.Abstractions.Logging.EventLogType.ADMIN_ALERT.ToString() };
                log.LogProperties.Add(new LogDetailInfo("Module", Globals.ModuleFriendlyName));
                message = $"Merged Edit permissions from: {unmergedPerms} to {perms.Edit}";
                log.AddProperty("Message", message);
                DotNetNuke.Services.Log.EventLog.LogController.Instance.AddLog(log);
            }
        }

        private static string Merge_PermSet_080200(string tempSet)
        {
            string newSet = tempSet;
            if (!string.IsNullOrEmpty(tempSet) && tempSet.Contains("::::"))
            {
                try
                {
                    string oldSet1 = tempSet.Split("::::".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0];
                    string oldSet2 = tempSet.Split("::::".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1];

                    List<string> oldSet1permSet = oldSet1.Split('|').ToList();
                    List<string> oldSet1authRoles = oldSet1permSet[0].Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
                    List<string> oldSet1authUsers = new List<string>();
                    List<string> oldSet1authGroups = new List<string>();

                    if (!(string.IsNullOrEmpty(oldSet1permSet[1])) && oldSet1permSet[1].Contains(";"))
                    {
                        oldSet1authUsers = oldSet1permSet[1].Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
                    }

                    if (!(string.IsNullOrEmpty(oldSet1permSet[2])) && oldSet1permSet[2].Contains(";"))
                    {
                        oldSet1authGroups = oldSet1permSet[2].Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
                    }

                    List<string> oldSet2permSet = oldSet2.Split('|').ToList();
                    List<string> oldSet2authRoles = oldSet2permSet[0].Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
                    List<string> oldSet2authUsers = new List<string>();
                    List<string> oldSet2authGroups = new List<string>();

                    if (!(string.IsNullOrEmpty(oldSet2permSet[1])) && oldSet2permSet[1].Contains(";"))
                    {
                        oldSet2authUsers = oldSet2permSet[1].Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
                    }

                    if (!(string.IsNullOrEmpty(oldSet2permSet[2])) && oldSet2permSet[2].Contains(";"))
                    {
                        oldSet2authGroups = oldSet2permSet[2].Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
                    }

                    List<string> newAuthRoles = oldSet1authRoles;
                    foreach (string oldSet2authRole in oldSet2authRoles)
                    {
                        if (!newAuthRoles.Contains(oldSet2authRole))
                        {
                            newAuthRoles.Add(oldSet2authRole);
                        }
                    }

                    List<string> newAuthUsers = oldSet1authUsers;
                    foreach (string oldSet2authUser in oldSet2authUsers)
                    {
                        if (!newAuthUsers.Contains(oldSet2authUser))
                        {
                            newAuthUsers.Add(oldSet2authUser);
                        }
                    }

                    List<string> newAuthGroups = oldSet1authGroups;
                    foreach (string oldSet2authGroup in oldSet2authGroups)
                    {
                        if (!newAuthGroups.Contains(oldSet2authGroup))
                        {
                            newAuthGroups.Add(oldSet2authGroup);
                        }
                    }

                    string newRoles = string.Join(";", newAuthRoles);
                    if (!string.IsNullOrEmpty(newRoles))
                    {
                        newRoles += ";";
                    }

                    // newRoles = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.SortPermissionSetMembers(newRoles);

                    string newUsers = string.Join(";", newAuthUsers);
                    if (!string.IsNullOrEmpty(newUsers))
                    {
                        newUsers += ";";
                    }

                    // newUsers = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.SortPermissionSetMembers(newUsers);

                    string newGroups = string.Join(";", newAuthGroups);
                    if (!string.IsNullOrEmpty(newGroups))
                    {
                        newGroups += ";";
                    }

                    // newGroups = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.SortPermissionSetMembers(newGroups);

                    newSet = string.Concat(newRoles, "|", newUsers, "|", newGroups, "|");
                }
                catch (Exception ex)
                {
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                }
            }

            return newSet;
        }

        internal static void Upgrade_EmailNotificationSubjectTokens_080200()
        {
            foreach (DotNetNuke.Abstractions.Portals.IPortalInfo portal in DotNetNuke.Entities.Portals.PortalController.Instance.GetPortals())
            {
                foreach (ModuleInfo module in DotNetNuke.Entities.Modules.ModuleController.Instance.GetModules(portal.PortalId))
                {
                    if (module.DesktopModule.ModuleName.Trim().Equals(Globals.ModuleName.Trim(), StringComparison.InvariantCultureIgnoreCase))
                    {
                        try
                        {
                            TemplateController tc = new TemplateController();
                            foreach (TemplateInfo templateInfo in tc.Template_List(-1, -1))
                            {
                                if (templateInfo.TemplateType == Templates.TemplateTypes.Email)
                                {
                                    try
                                    {
                                        var portalSettings = PortalSettings.Current;
                                        if (portalSettings == null)
                                        {
                                            portalSettings = Utilities.GetPortalSettings(portal.PortalId);
                                        }

                                        templateInfo.Subject = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.MapLegacyEmailNotificationTokenSynonyms(new StringBuilder(templateInfo.Subject), portalSettings, portalSettings.DefaultLanguage).ToString();
                                        DotNetNuke.Modules.ActiveForums.Controllers.SettingsController.SaveSetting(module.ModuleID, $"M{module.ModuleID}", ForumSettingKeys.EmailNotificationSubjectTemplate, templateInfo.Subject);
                                        DotNetNuke.Modules.ActiveForums.DataCache.ClearAllCache(module.ModuleID);
                                    }
                                    catch (Exception ex)
                                    {
                                        Logger.Error(ex.Message, ex);
                                        DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex.Message, ex);
                            DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                        }
                    }
                }
            }
        }

        internal static void Upgrade_RelocateSqlFiles_080200()
        {
            try
            {
                DotNetNuke.Services.Log.EventLog.LogInfo log;
                string message;

                var di = new System.IO.DirectoryInfo(Utilities.MapPath($"{Globals.ModulePath}sql/sql/"));
                if (di.Exists)
                {
                    foreach (var fullFilePathName in System.IO.Directory.EnumerateFiles(path: di.FullName, searchPattern: "*.SqlDataProvider", searchOption: System.IO.SearchOption.TopDirectoryOnly))
                    {
                        if (fullFilePathName.ToLowerInvariant().Contains("uninstall"))
                        {
                            System.IO.File.Delete(fullFilePathName);
                            log = new DotNetNuke.Services.Log.EventLog.LogInfo { LogTypeKey = DotNetNuke.Abstractions.Logging.EventLogType.ADMIN_ALERT.ToString() };
                            log.LogProperties.Add(new LogDetailInfo("Module", Globals.ModuleFriendlyName));
                            message = $"During upgrade, removed old file {fullFilePathName}";
                            log.AddProperty("Message", message);
                            DotNetNuke.Services.Log.EventLog.LogController.Instance.AddLog(log);
                        }
                        else
                        {
                            System.IO.File.Copy(fullFilePathName, $"{Utilities.MapPath($"{Globals.ModulePath}sql/{new System.IO.FileInfo(fullFilePathName).Name}")}", true);
                            System.IO.File.Delete(fullFilePathName);
                            log = new DotNetNuke.Services.Log.EventLog.LogInfo { LogTypeKey = DotNetNuke.Abstractions.Logging.EventLogType.ADMIN_ALERT.ToString() };
                            log.LogProperties.Add(new LogDetailInfo("Module", Globals.ModuleFriendlyName));
                            message = $"During upgrade, moved {fullFilePathName} to {Utilities.MapPath($"{Globals.ModulePath}sql/{new System.IO.FileInfo(fullFilePathName).Name}")}";
                            log.AddProperty("Message", message);
                            DotNetNuke.Services.Log.EventLog.LogController.Instance.AddLog(log);
                        }
                    }

                    di.Delete();
                    log = new DotNetNuke.Services.Log.EventLog.LogInfo { LogTypeKey = DotNetNuke.Abstractions.Logging.EventLogType.ADMIN_ALERT.ToString() };
                    log.LogProperties.Add(new LogDetailInfo("Module", Globals.ModuleFriendlyName));
                    message = $"During upgrade, removed {di.FullName}";
                    log.AddProperty("Message", message);
                    DotNetNuke.Services.Log.EventLog.LogController.Instance.AddLog(log);
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        internal static void Install_Upgrade_CreateForumDefaultSettingsAndSecurity_080200()
        {
            try
            {
                foreach (DotNetNuke.Abstractions.Portals.IPortalInfo portal in DotNetNuke.Entities.Portals.PortalController.Instance.GetPortals())
                {
                    foreach (ModuleInfo module in DotNetNuke.Entities.Modules.ModuleController.Instance.GetModules(portal.PortalId))
                    {
                        if (module.DesktopModule.ModuleName.Trim().ToLowerInvariant() == Globals.ModuleName.ToLowerInvariant())
                        {
                            Install_Upgrade_ForumDefaultSettingsAndSecurity(portalId: module.PortalID, moduleId: module.ModuleID);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        internal static void Install_Upgrade_ForumDefaultSettingsAndSecurity(int portalId, int moduleId)
        {
            try
            {
                var portalSettings = Utilities.GetPortalSettings(portalId);
                var permissions = new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController().CreateDefaultPermissions(portalSettings, moduleId);
                DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(moduleId, SettingKeys.DefaultPermissionId, permissions.PermissionsId.ToString());

                string sKey = $"M{moduleId}";
                DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(moduleId, SettingKeys.DefaultSettingsKey, sKey);
                if (string.IsNullOrEmpty(SettingsBase.GetModuleSettings(moduleId).DefaultFeatureSettings.EmailNotificationSubjectTemplate))
                {
                    DotNetNuke.Modules.ActiveForums.Controllers.SettingsController.SaveSetting(moduleId, sKey, ForumSettingKeys.EmailNotificationSubjectTemplate, "[FORUMAUTHOR:DISPLAYNAME] [POSTEDORREPLIEDTO] [SUBSCRIBEDFORUMORTOPICSUBJECTFORUMNAME] on [PORTAL:PORTALNAME]");
                }

                DotNetNuke.Modules.ActiveForums.Controllers.SettingsController.SaveSetting(moduleId, sKey, ForumSettingKeys.EmailAddress, string.Empty);
                DotNetNuke.Modules.ActiveForums.Controllers.SettingsController.SaveSetting(moduleId, sKey, ForumSettingKeys.UseFilter, "true");
                DotNetNuke.Modules.ActiveForums.Controllers.SettingsController.SaveSetting(moduleId, sKey, ForumSettingKeys.AllowPostIcon, "false");
                DotNetNuke.Modules.ActiveForums.Controllers.SettingsController.SaveSetting(moduleId, sKey, ForumSettingKeys.AllowLikes, "true");
                DotNetNuke.Modules.ActiveForums.Controllers.SettingsController.SaveSetting(moduleId, sKey, ForumSettingKeys.AllowEmoticons, "false");
                DotNetNuke.Modules.ActiveForums.Controllers.SettingsController.SaveSetting(moduleId, sKey, ForumSettingKeys.AllowScript, "false");
                DotNetNuke.Modules.ActiveForums.Controllers.SettingsController.SaveSetting(moduleId, sKey, ForumSettingKeys.IndexContent, "true");
                DotNetNuke.Modules.ActiveForums.Controllers.SettingsController.SaveSetting(moduleId, sKey, ForumSettingKeys.AllowRSS, "true");
                DotNetNuke.Modules.ActiveForums.Controllers.SettingsController.SaveSetting(moduleId, sKey, ForumSettingKeys.AllowAttach, "true");
                DotNetNuke.Modules.ActiveForums.Controllers.SettingsController.SaveSetting(moduleId, sKey, ForumSettingKeys.AttachCount, "3");
                DotNetNuke.Modules.ActiveForums.Controllers.SettingsController.SaveSetting(moduleId, sKey, ForumSettingKeys.AttachMaxSize, "1000");
                DotNetNuke.Modules.ActiveForums.Controllers.SettingsController.SaveSetting(moduleId, sKey, ForumSettingKeys.AttachTypeAllowed, "txt,tiff,pdf,xls,xlsx,doc,docx,ppt,pptx");

                DotNetNuke.Modules.ActiveForums.Controllers.SettingsController.SaveSetting(moduleId, sKey, ForumSettingKeys.AttachMaxHeight, "450");
                DotNetNuke.Modules.ActiveForums.Controllers.SettingsController.SaveSetting(moduleId, sKey, ForumSettingKeys.AttachMaxWidth, "450");
                DotNetNuke.Modules.ActiveForums.Controllers.SettingsController.SaveSetting(moduleId, sKey, ForumSettingKeys.AttachAllowBrowseSite, "true");
                DotNetNuke.Modules.ActiveForums.Controllers.SettingsController.SaveSetting(moduleId, sKey, ForumSettingKeys.MaxAttachHeight, "800");
                DotNetNuke.Modules.ActiveForums.Controllers.SettingsController.SaveSetting(moduleId, sKey, ForumSettingKeys.MaxAttachWidth, "800");
                DotNetNuke.Modules.ActiveForums.Controllers.SettingsController.SaveSetting(moduleId, sKey, ForumSettingKeys.AttachInsertAllowed, "false");
                DotNetNuke.Modules.ActiveForums.Controllers.SettingsController.SaveSetting(moduleId, sKey, ForumSettingKeys.ConvertingToJpegAllowed, "false");
                DotNetNuke.Modules.ActiveForums.Controllers.SettingsController.SaveSetting(moduleId, sKey, ForumSettingKeys.AllowHTML, "true");
                DotNetNuke.Modules.ActiveForums.Controllers.SettingsController.SaveSetting(moduleId, sKey, ForumSettingKeys.EditorType, ((int)EditorTypes.HTMLEDITORPROVIDER).ToString());
                DotNetNuke.Modules.ActiveForums.Controllers.SettingsController.SaveSetting(moduleId, sKey, ForumSettingKeys.EditorMobile, ((int)EditorTypes.HTMLEDITORPROVIDER).ToString());

                DotNetNuke.Modules.ActiveForums.Controllers.SettingsController.SaveSetting(moduleId, sKey, ForumSettingKeys.EditorHeight, "350");
                DotNetNuke.Modules.ActiveForums.Controllers.SettingsController.SaveSetting(moduleId, sKey, ForumSettingKeys.EditorWidth, "99%");
                DotNetNuke.Modules.ActiveForums.Controllers.SettingsController.SaveSetting(moduleId, sKey, ForumSettingKeys.EditorToolbar, "bold,italic,underline,quote");
                DotNetNuke.Modules.ActiveForums.Controllers.SettingsController.SaveSetting(moduleId, sKey, ForumSettingKeys.EditorStyle, "2");
                DotNetNuke.Modules.ActiveForums.Controllers.SettingsController.SaveSetting(moduleId, sKey, ForumSettingKeys.IsModerated, "false");
                DotNetNuke.Modules.ActiveForums.Controllers.SettingsController.SaveSetting(moduleId, sKey, ForumSettingKeys.DefaultTrustLevel, "0");
                DotNetNuke.Modules.ActiveForums.Controllers.SettingsController.SaveSetting(moduleId, sKey, ForumSettingKeys.AutoTrustLevel, "0");
                DotNetNuke.Modules.ActiveForums.Controllers.SettingsController.SaveSetting(moduleId, sKey, ForumSettingKeys.ModApproveNotify, "0");
                DotNetNuke.Modules.ActiveForums.Controllers.SettingsController.SaveSetting(moduleId, sKey, ForumSettingKeys.ModRejectNotify, "0");
                DotNetNuke.Modules.ActiveForums.Controllers.SettingsController.SaveSetting(moduleId, sKey, ForumSettingKeys.ModMoveNotify, "0");
                DotNetNuke.Modules.ActiveForums.Controllers.SettingsController.SaveSetting(moduleId, sKey, ForumSettingKeys.ModDeleteNotify, "0");
                DotNetNuke.Modules.ActiveForums.Controllers.SettingsController.SaveSetting(moduleId, sKey, ForumSettingKeys.ModAlertNotify, "0");

                DotNetNuke.Modules.ActiveForums.DataCache.ClearAllCache(moduleId);
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        internal static void Install_LikeNotificationType_080200()
        {
            string notificationTypeName = Globals.LikeNotificationType;
            string notificationTypeDescription = Globals.LikeNotificationTypeDescription;
            int deskModuleId = DesktopModuleController.GetDesktopModuleByFriendlyName(Globals.ModuleFriendlyName).DesktopModuleID;

            NotificationType type = new NotificationType { Name = notificationTypeName, Description = notificationTypeDescription, DesktopModuleId = deskModuleId };
            if (NotificationsController.Instance.GetNotificationType(notificationTypeName) == null)
            {
                NotificationsController.Instance.CreateNotificationType(type);
            }
        }

        internal static void Install_PinNotificationType_080200()
        {
            string notificationTypeName = Globals.PinNotificationType;
            string notificationTypeDescription = Globals.PinNotificationTypeDescription;
            int deskModuleId = DesktopModuleController.GetDesktopModuleByFriendlyName(Globals.ModuleFriendlyName).DesktopModuleID;

            NotificationType type = new NotificationType { Name = notificationTypeName, Description = notificationTypeDescription, DesktopModuleId = deskModuleId };
            if (NotificationsController.Instance.GetNotificationType(notificationTypeName) == null)
            {
                NotificationsController.Instance.CreateNotificationType(type);
            }
        }

        internal static void Upgrade_PermissionSets_090000()
        {
            foreach (var perms in new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController().Get())
            {
                perms.Announce = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIds(DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIdsFromPermSet(string.IsNullOrEmpty(perms.Announce) ? string.Empty : perms.Announce.Replace(":", ";")));
                perms.Attach = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIds(DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIdsFromPermSet(string.IsNullOrEmpty(perms.Attach) ? string.Empty : perms.Attach.Replace(":", ";")));
                perms.Categorize = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIds(DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIdsFromPermSet(string.IsNullOrEmpty(perms.Categorize) ? string.Empty : perms.Categorize.Replace(":", ";")));
                perms.Create = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIds(DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIdsFromPermSet(string.IsNullOrEmpty(perms.Create) ? string.Empty : perms.Create.Replace(":", ";")));
                perms.Delete = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIds(DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIdsFromPermSet(string.IsNullOrEmpty(perms.Delete) ? string.Empty : perms.Delete.Replace(":", ";")));
                perms.Edit = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIds(DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIdsFromPermSet(string.IsNullOrEmpty(perms.Edit) ? string.Empty : perms.Edit.Replace(":", ";")));
                perms.Lock = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIds(DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIdsFromPermSet(string.IsNullOrEmpty(perms.Lock) ? string.Empty : perms.Lock.Replace(":", ";")));
                perms.ManageUsers = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIds(DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIdsFromPermSet(string.IsNullOrEmpty(perms.ManageUsers) ? string.Empty : perms.ManageUsers.Replace(":", ";")));
                perms.Moderate = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIds(DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIdsFromPermSet(string.IsNullOrEmpty(perms.Moderate) ? string.Empty : perms.Moderate.Replace(":", ";")));
                perms.Move = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIds(DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIdsFromPermSet(string.IsNullOrEmpty(perms.Move) ? string.Empty : perms.Move.Replace(":", ";")));
                perms.Pin = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIds(DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIdsFromPermSet(string.IsNullOrEmpty(perms.Pin) ? string.Empty : perms.Pin.Replace(":", ";")));
                perms.Poll = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIds(DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIdsFromPermSet(string.IsNullOrEmpty(perms.Poll) ? string.Empty : perms.Poll.Replace(":", ";")));
                perms.Prioritize = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIds(DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIdsFromPermSet(string.IsNullOrEmpty(perms.Prioritize) ? string.Empty : perms.Prioritize.Replace(":", ";")));
                perms.Read = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIds(DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIdsFromPermSet(string.IsNullOrEmpty(perms.Read) ? string.Empty : perms.Read.Replace(":", ";")));
                perms.Reply = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIds(DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIdsFromPermSet(string.IsNullOrEmpty(perms.Reply) ? string.Empty : perms.Reply.Replace(":", ";")));
                perms.Split = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIds(DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIdsFromPermSet(string.IsNullOrEmpty(perms.Split) ? string.Empty : perms.Split.Replace(":", ";")));
                perms.Subscribe = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIds(DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIdsFromPermSet(string.IsNullOrEmpty(perms.Subscribe) ? string.Empty : perms.Subscribe.Replace(":", ";")));
                perms.Tag = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIds(DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIdsFromPermSet(string.IsNullOrEmpty(perms.Tag) ? string.Empty : perms.Tag.Replace(":", ";")));
                perms.Trust = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIds(DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIdsFromPermSet(string.IsNullOrEmpty(perms.Trust) ? string.Empty : perms.Trust.Replace(":", ";")));
                perms.View = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIds(DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIdsFromPermSet(string.IsNullOrEmpty(perms.View) ? string.Empty : perms.View.Replace(":", ";")));
                new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController().Update(perms);
            }
        }

        internal static void Install_BadgeNotificationType_090200()
        {
            string notificationTypeName = Globals.BadgeNotificationType;
            string notificationTypeDescription = Globals.BadgeNotificationTypeDescription;
            int deskModuleId = DesktopModuleController.GetDesktopModuleByFriendlyName(Globals.ModuleFriendlyName).DesktopModuleID;

            NotificationType type = new NotificationType { Name = notificationTypeName, Description = notificationTypeDescription, DesktopModuleId = deskModuleId };
            if (NotificationsController.Instance.GetNotificationType(notificationTypeName) == null)
            {
                NotificationsController.Instance.CreateNotificationType(type);
            }
        }

        internal void Install_DefaultBadges_090201()
        {
            try
            {
                foreach (DotNetNuke.Abstractions.Portals.IPortalInfo portal in DotNetNuke.Entities.Portals.PortalController.Instance.GetPortals())
                {
                    foreach (ModuleInfo module in DotNetNuke.Entities.Modules.ModuleController.Instance.GetModules(portal.PortalId))
                    {
                        if (module.DesktopModule.ModuleName.Trim().Equals(Globals.ModuleName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            if (SettingsBase.GetModuleSettings(module.ModuleID).ModeIsStandard)
                            {
                                if (new DotNetNuke.Modules.ActiveForums.Controllers.BadgeController().Get(module.ModuleID).Count().Equals(0))
                                {
                                    var defaultBadgesFolder = DotNetNuke.Services.FileSystem.FolderManager.Instance.GetFolder(portal.PortalId, Globals.DefaultBadgesFolderName) ?? DotNetNuke.Services.FileSystem.FolderManager.Instance.AddFolder(portal.PortalId, Globals.DefaultBadgesFolderName);
                                    var xDoc = new System.Xml.XmlDocument();
                                    xDoc.Load(this.sPath);
                                    if (xDoc != null)
                                    {
                                        System.Xml.XmlNode xRoot = xDoc.DocumentElement;
                                        System.Xml.XmlNodeList xNodeList = xRoot.SelectNodes("//badges/badge");
                                        if (xNodeList.Count > 0)
                                        {
                                            int i;
                                            for (i = 0; i < xNodeList.Count; i++)
                                            {
                                                var fileId = -1;
                                                try
                                                {
                                                    var imageFile = DotNetNuke.Modules.ActiveForums.Utilities.MapPath(xNodeList[i].Attributes["image"].Value);
                                                    if (System.IO.File.Exists(imageFile))
                                                    {
                                                        var fileName = System.IO.Path.GetFileName(imageFile);
                                                        using (var fileStream = new FileStream(imageFile, FileMode.Open, FileAccess.Read))
                                                        {
                                                            fileId = DotNetNuke.Services.FileSystem.FileManager.Instance.AddFile(defaultBadgesFolder, fileName, fileStream, true).FileId;
                                                        }
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    DotNetNuke.Modules.ActiveForums.Exceptions.LogException(ex);
                                                }

                                                try
                                                {
                                                    var badge = new DotNetNuke.Modules.ActiveForums.Entities.BadgeInfo
                                                    {
                                                        Name = xNodeList[i].Attributes["name"].Value,
                                                        Description = xNodeList[i].Attributes["description"].Value,
                                                        BadgeMetric = (DotNetNuke.Modules.ActiveForums.Enums.BadgeMetric)Utilities.SafeConvertInt(xNodeList[i].Attributes["badgemetric"].Value),
                                                        ModuleId = module.ModuleID,
                                                        OneTimeAward = Utilities.SafeConvertBool(xNodeList[i].Attributes["onetimeaward"].Value, true),
                                                        SortOrder = Utilities.SafeConvertInt(xNodeList[i].Attributes["sortorder"].Value),
                                                        Threshold = Utilities.SafeConvertInt(xNodeList[i].Attributes["threshold"].Value),
                                                        IntervalDays = Utilities.SafeConvertInt(xNodeList[i].Attributes["intervaldays"].Value),
                                                        SendAwardNotification = true,
                                                        ImageMarkup = xNodeList[i].Attributes["imagemarkup"].Value,
                                                        FileId = fileId,
                                                    };
                                                    badge.SuppresssAwardNotificationOnBackfill = !badge.OneTimeAward && !badge.BadgeMetric.Equals(DotNetNuke.Modules.ActiveForums.Enums.BadgeMetric.BadgeMetricManual);
                                                    new DotNetNuke.Modules.ActiveForums.Controllers.BadgeController().Insert(badge);
                                                }
                                                catch (Exception ex)
                                                {
                                                    DotNetNuke.Modules.ActiveForums.Exceptions.LogException(ex);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Modules.ActiveForums.Exceptions.LogException(ex);
            }
        }
    }
}
