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
    using System.Data.SqlTypes;
    using System.IO;
    using System.Linq;
    using System.Net.Mail;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web.UI.WebControls;

    using DotNetNuke.Collections;
    using DotNetNuke.Common.Controls;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Modules.ActiveForums.Data;
    using DotNetNuke.Modules.ActiveForums.Enums;
    using DotNetNuke.Modules.ActiveForums.ViewModels;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Services.Social.Notifications;
    using DotNetNuke.UI.UserControls;

    using log4net;

    using Microsoft.ApplicationBlocks.Data;

    using static System.ComponentModel.Design.ObjectSelectorEditor;

    public class ForumsConfig
    {
        private static readonly DotNetNuke.Instrumentation.ILog Logger = LoggerSource.Instance.GetLogger(typeof(ForumsConfig));

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

                // Create default badges new in 09.02.01
                new ForumsConfig().Install_DefaultBadges_090201(upgrading: false);

                // Create "user mention notification" core messaging notification type new in 09.03.00
                ForumsConfig.Install_UserMentionNotificationType_090300();

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
                    var portalSettings = new DotNetNuke.Modules.ActiveForums.Helpers.PortalSettingsHelper().GetPortalSettings(portalId: portalId);
                    for (var i = 0; i < xNodeList.Count; i++)
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
                        var groupId = gc.Groups_Save(portalId, gi, true, true, true);
                        gi = gc.GetById(groupId, moduleId);
                        if (groupId != -1)
                        {
                            if (xNodeList[i].HasChildNodes)
                            {
                                System.Xml.XmlNodeList cNodes = xNodeList[i].ChildNodes;
                                for (var c = 0; c < cNodes.Count; c++)
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
                                        ForumSettingsKey = $"M{moduleId}",
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
                                    var template = Convert.ToString(dr["Template"]).Replace("[TRESX:", "[RESX:");
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

        internal void ArchiveOrphanedAttachments_070007()
        {
            var di = new System.IO.DirectoryInfo(DotNetNuke.Modules.ActiveForums.Utilities.MapPath("~/portals"));
            System.IO.DirectoryInfo[] attachmentFolders = di.GetDirectories(Globals.LegacyAttachmentsFolderName, System.IO.SearchOption.AllDirectories);

            foreach (System.IO.DirectoryInfo attachmentFolder in attachmentFolders)
            {
                if (!System.IO.Directory.Exists(string.Concat(attachmentFolder.FullName, "\\orphaned")))
                {
                    System.IO.Directory.CreateDirectory(string.Concat(attachmentFolder.FullName, "\\orphaned"));
                }

                List<string> attachmentFullFileNames = System.IO.Directory.EnumerateFiles(path: attachmentFolder.FullName, searchPattern: "*", searchOption: System.IO.SearchOption.AllDirectories).ToList<string>();
                List<string> attachmentFileNames = new List<string>();

                foreach (var attachmentFileName in attachmentFullFileNames)
                {
                    attachmentFileNames.Add(new System.IO.FileInfo(attachmentFileName).Name);
                }

                var databaseFileNames = DotNetNuke.Data.DataContext.Instance().ExecuteQuery<string>(System.Data.CommandType.Text, "SELECT FileName FROM {databaseOwner}{objectQualifier}activeforums_Attachments ORDER BY FileName").ToList();

                foreach (var attachmentFileName in attachmentFileNames)
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
            var connectionString = new Connection().connectionString;
            var dbPrefix = new Connection().dbPrefix;

            using (IDataReader dr = SqlHelper.ExecuteReader(connectionString, CommandType.Text, $"SELECT f.PortalId,f.ModuleId,ft.ForumId,t.topicId,c.Subject FROM {dbPrefix}Topics t INNER JOIN {dbPrefix}ForumTopics ft ON ft.TopicId = t.TopicId INNER JOIN {dbPrefix}Content c ON c.ContentId = t.ContentId INNER JOIN {dbPrefix}Forums f ON f.ForumId = ft.ForumId WHERE t.URL = ''"))
            {
                while (dr.Read())
                {
                    var portalId = Utilities.SafeConvertInt(dr["PortalId"]);
                    var moduleId = Utilities.SafeConvertInt(dr["ModuleId"]);
                    var forumId = Utilities.SafeConvertInt(dr["ForumId"]);
                    var topicId = Utilities.SafeConvertInt(dr["TopicId"]);
                    var subject = Utilities.SafeConvertString(dr["Subject"]);
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
            var notificationTypeName = Globals.BanUserNotificationType;
            var notificationTypeDescription = Globals.BanUserNotificationTypeDescription;
            var deskModuleId = DesktopModuleController.GetDesktopModuleByFriendlyName(Globals.ModuleFriendlyName).DesktopModuleID;

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
                var unmergedPerms = perms.Lock;
                perms.Lock = Merge_PermSet_080200(perms.Lock);
                new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController().Update(perms);
                var log = new DotNetNuke.Services.Log.EventLog.LogInfo { LogTypeKey = DotNetNuke.Abstractions.Logging.EventLogType.ADMIN_ALERT.ToString() };
                log.LogProperties.Add(new LogDetailInfo("Module", Globals.ModuleFriendlyName));
                var message = $"Merged LOCK permissions from: {unmergedPerms} to {perms.Lock}";
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
            var newSet = tempSet;
            if (!string.IsNullOrEmpty(tempSet) && tempSet.Contains("::::"))
            {
                try
                {
                    var oldSet1 = tempSet.Split("::::".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0];
                    var oldSet2 = tempSet.Split("::::".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[1];

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
                    foreach (var oldSet2authRole in oldSet2authRoles)
                    {
                        if (!newAuthRoles.Contains(oldSet2authRole))
                        {
                            newAuthRoles.Add(oldSet2authRole);
                        }
                    }

                    List<string> newAuthUsers = oldSet1authUsers;
                    foreach (var oldSet2authUser in oldSet2authUsers)
                    {
                        if (!newAuthUsers.Contains(oldSet2authUser))
                        {
                            newAuthUsers.Add(oldSet2authUser);
                        }
                    }

                    List<string> newAuthGroups = oldSet1authGroups;
                    foreach (var oldSet2authGroup in oldSet2authGroups)
                    {
                        if (!newAuthGroups.Contains(oldSet2authGroup))
                        {
                            newAuthGroups.Add(oldSet2authGroup);
                        }
                    }

                    var newRoles = string.Join(";", newAuthRoles);
                    if (!string.IsNullOrEmpty(newRoles))
                    {
                        newRoles += ";";
                    }

                    // newRoles = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.SortPermissionSetMembers(newRoles);

                    var newUsers = string.Join(";", newAuthUsers);
                    if (!string.IsNullOrEmpty(newUsers))
                    {
                        newUsers += ";";
                    }

                    // newUsers = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.SortPermissionSetMembers(newUsers);

                    var newGroups = string.Join(";", newAuthGroups);
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
                                            portalSettings = new DotNetNuke.Modules.ActiveForums.Helpers.PortalSettingsHelper().GetPortalSettings(portal.PortalId);
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
                var portalSettings = new DotNetNuke.Modules.ActiveForums.Helpers.PortalSettingsHelper().GetPortalSettings(portalId);
                var permissions = new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController().CreateDefaultPermissions(portalSettings, moduleId);
                DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(moduleId, SettingKeys.DefaultPermissionId, permissions.PermissionsId.ToString());

                var sKey = $"M{moduleId}";
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
                DotNetNuke.Modules.ActiveForums.Controllers.SettingsController.SaveSetting(moduleId, sKey, ForumSettingKeys.AttachCount, "4");
                DotNetNuke.Modules.ActiveForums.Controllers.SettingsController.SaveSetting(moduleId, sKey, ForumSettingKeys.AttachMaxSize, "2048");
                DotNetNuke.Modules.ActiveForums.Controllers.SettingsController.SaveSetting(moduleId, sKey, ForumSettingKeys.AttachTypeAllowed, "txt,tiff,pdf,xls,xlsx,doc,docx,ppt,pptx,png,jpg,gif");

                DotNetNuke.Modules.ActiveForums.Controllers.SettingsController.SaveSetting(moduleId, sKey, ForumSettingKeys.AttachAllowBrowseSite, "true");
                DotNetNuke.Modules.ActiveForums.Controllers.SettingsController.SaveSetting(moduleId, sKey, ForumSettingKeys.MaxImageHeight, "800");
                DotNetNuke.Modules.ActiveForums.Controllers.SettingsController.SaveSetting(moduleId, sKey, ForumSettingKeys.MaxImageWidth, "800");
                DotNetNuke.Modules.ActiveForums.Controllers.SettingsController.SaveSetting(moduleId, sKey, ForumSettingKeys.AllowHTML, "true");
                DotNetNuke.Modules.ActiveForums.Controllers.SettingsController.SaveSetting(moduleId, sKey, ForumSettingKeys.EditorType, ((int)EditorType.DNNCKEDITOR4PLUSFORUMSPLUGINS).ToString());

                DotNetNuke.Modules.ActiveForums.Controllers.SettingsController.SaveSetting(moduleId, sKey, ForumSettingKeys.UserMentions, "true");
                DotNetNuke.Modules.ActiveForums.Controllers.SettingsController.SaveSetting(moduleId, sKey, ForumSettingKeys.UserMentionVisibility, ((int)DotNetNuke.Modules.ActiveForums.Enums.UserMentionVisibility.RegisteredUsers).ToString());

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
            var notificationTypeName = Globals.LikeNotificationType;
            var notificationTypeDescription = Globals.LikeNotificationTypeDescription;
            var deskModuleId = DesktopModuleController.GetDesktopModuleByFriendlyName(Globals.ModuleFriendlyName).DesktopModuleID;

            NotificationType type = new NotificationType { Name = notificationTypeName, Description = notificationTypeDescription, DesktopModuleId = deskModuleId };
            if (NotificationsController.Instance.GetNotificationType(notificationTypeName) == null)
            {
                NotificationsController.Instance.CreateNotificationType(type);
            }
        }

        internal static void Install_PinNotificationType_080200()
        {
            var notificationTypeName = Globals.PinNotificationType;
            var notificationTypeDescription = Globals.PinNotificationTypeDescription;
            var deskModuleId = DesktopModuleController.GetDesktopModuleByFriendlyName(Globals.ModuleFriendlyName).DesktopModuleID;

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
            var notificationTypeName = Globals.BadgeNotificationType;
            var notificationTypeDescription = Globals.BadgeNotificationTypeDescription;
            var deskModuleId = DesktopModuleController.GetDesktopModuleByFriendlyName(Globals.ModuleFriendlyName).DesktopModuleID;

            NotificationType type = new NotificationType { Name = notificationTypeName, Description = notificationTypeDescription, DesktopModuleId = deskModuleId };
            if (NotificationsController.Instance.GetNotificationType(notificationTypeName) == null)
            {
                NotificationsController.Instance.CreateNotificationType(type);
            }
        }

        internal void Install_DefaultBadges_090201(bool upgrading)
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
                                if (!new Controllers.BadgeController().Get(module.ModuleID).Any())
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
                                                        SuppresssAwardNotificationOnBackfill = upgrading ? true : false,
                                                        SortOrder = Utilities.SafeConvertInt(xNodeList[i].Attributes["sortorder"].Value),
                                                        Threshold = Utilities.SafeConvertInt(xNodeList[i].Attributes["threshold"].Value),
                                                        IntervalDays = Utilities.SafeConvertInt(xNodeList[i].Attributes["intervaldays"].Value),
                                                        SendAwardNotification = true,
                                                        ImageMarkup = xNodeList[i].Attributes["imagemarkup"].Value,
                                                        FileId = fileId,
                                                    };
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

        internal static void Install_UserMentionNotificationType_090300()
        {
            var notificationTypeName = Globals.UserMentionNotificationType;
            var notificationTypeDescription = Globals.UserMentionNotificationTypeDescription;
            var deskModuleId = DesktopModuleController.GetDesktopModuleByFriendlyName(Globals.ModuleFriendlyName).DesktopModuleID;

            NotificationType type = new NotificationType { Name = notificationTypeName, Description = notificationTypeDescription, DesktopModuleId = deskModuleId };
            if (NotificationsController.Instance.GetNotificationType(notificationTypeName) == null)
            {
                NotificationsController.Instance.CreateNotificationType(type);
            }
        }

        internal static void Upgrade_AddUserMentionVisibilityForumSetting_090300()
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

        internal static void Reset_DNN_Search_Documents_090500()
        {
            try
            {
                foreach (DotNetNuke.Abstractions.Portals.IPortalInfo portal in DotNetNuke.Entities.Portals.PortalController.Instance.GetPortals())
                {
                    foreach (ModuleInfo module in DotNetNuke.Entities.Modules.ModuleController.Instance.GetModules(portal.PortalId))
                    {
                        if (module.DesktopModule.ModuleName.Trim().Equals(Globals.ModuleName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            try
                            {
                                DotNetNuke.Services.Search.Internals.InternalSearchController.Instance.DeleteSearchDocumentsByModule(portal.PortalId, module.ModuleID, module.ModuleDefID);
                                module.LastContentModifiedOnDate = SqlDateTime.MinValue.Value.AddDays(1);
                                DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModule(module);
                            }
                            catch (Exception ex)
                            {
                                DotNetNuke.Modules.ActiveForums.Exceptions.LogException(ex);
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

        internal static void Upgrade_EnsureVanityNames_090600()
        {
            try
            {
                foreach (DotNetNuke.Abstractions.Portals.IPortalInfo portal in DotNetNuke.Entities.Portals.PortalController.Instance.GetPortals())
                {
                    foreach (ModuleInfo module in DotNetNuke.Entities.Modules.ModuleController.Instance.GetModules(portal.PortalId))
                    {
                        if (module.DesktopModule.ModuleName.Trim().Equals(Globals.ModuleName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            if (SettingsBase.GetModuleSettings(module.ModuleID).URLRewriteEnabled)
                            {
                                try
                                {
                                    new DotNetNuke.Modules.ActiveForums.Controllers.ForumGroupController().Get().Where(forumGroup => forumGroup.ModuleId.Equals(module.ModuleID)).ForEach(forumGroup =>
                                    {
                                        if (string.IsNullOrEmpty(forumGroup.PrefixURL))
                                        {
                                            try
                                            {
                                                forumGroup.PrefixURL = $"G{forumGroup.ForumGroupId}";
                                                new DotNetNuke.Modules.ActiveForums.Controllers.ForumGroupController().Update(forumGroup);
                                            }
                                            catch (Exception ex)
                                            {
                                                DotNetNuke.Modules.ActiveForums.Exceptions.LogException(ex);
                                            }
                                        }
                                    });
                                    new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().Get().Where(forum => forum.ModuleId.Equals(module.ModuleID)).ForEach(forum =>
                                    {
                                        if (string.IsNullOrEmpty(forum.PrefixURL))
                                        {
                                            try
                                            {
                                                forum.PrefixURL = $"F{forum.ForumID}";
                                                new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().Update(forum);
                                            }
                                            catch (Exception ex)
                                            {
                                                DotNetNuke.Modules.ActiveForums.Exceptions.LogException(ex);
                                            }
                                        }

                                    });
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
            catch (Exception ex)
            {
                DotNetNuke.Modules.ActiveForums.Exceptions.LogException(ex);
            }
        }

        internal void RemoveLegacyAvatarsFolder_090700()
        {
            var folderManager = DotNetNuke.Services.FileSystem.FolderManager.Instance;
            {
                foreach (DotNetNuke.Abstractions.Portals.IPortalInfo portal in DotNetNuke.Entities.Portals.PortalController.Instance.GetPortals())
                {
                    var legacyAvatarsFolder = folderManager.GetFolder(portal.PortalId, Globals.LegacyAvatarsFolderName);
                    if (legacyAvatarsFolder != null)
                    {
                        try
                        {
                            var log = new DotNetNuke.Services.Log.EventLog.LogInfo { LogTypeKey = DotNetNuke.Abstractions.Logging.EventLogType.HOST_ALERT.ToString() };
                            log.LogProperties.Add(new LogDetailInfo("Module", Globals.ModuleFriendlyName));
                            var message = $"Removing legacy folder: {legacyAvatarsFolder.PhysicalPath}";
                            log.AddProperty("Message", message);
                            DotNetNuke.Services.Log.EventLog.LogController.Instance.AddLog(log);
                            folderManager.DeleteFolder(legacyAvatarsFolder);
                        }
                        catch (Exception ex)
                        {
                            DotNetNuke.Modules.ActiveForums.Exceptions.LogException(ex);
                        }
                    }
                }
            }
        }

        internal void RelocateAttachments_090700()
        {
            foreach (DotNetNuke.Abstractions.Portals.IPortalInfo portal in DotNetNuke.Entities.Portals.PortalController.Instance.GetPortals())
            {
                foreach (ModuleInfo module in DotNetNuke.Entities.Modules.ModuleController.Instance.GetModules(portal.PortalId))
                {
                    if (module.DesktopModule.ModuleName.Trim().Equals(Globals.ModuleName.Trim(), StringComparison.InvariantCultureIgnoreCase))
                    {
                        try
                        {
                            if (!DotNetNuke.Services.FileSystem.FolderManager.Instance.FolderExists(portal.PortalId, Globals.ContentFolderNameBase))
                            {
                                DotNetNuke.Services.FileSystem.FolderManager.Instance.AddFolder(portal.PortalId, Globals.ContentFolderNameBase);
                            }
                        }
                        catch (Exception ex)
                        {
                            DotNetNuke.Modules.ActiveForums.Exceptions.LogException(ex);
                        }

                        try
                        {
                            if (!DotNetNuke.Services.FileSystem.FolderManager.Instance.FolderExists(portal.PortalId, Globals.AttachmentUploadsFolderName))
                            {
                                DotNetNuke.Services.FileSystem.FolderManager.Instance.AddFolder(portal.PortalId, Globals.AttachmentUploadsFolderName);
                            }
                        }
                        catch (Exception ex)
                        {
                            DotNetNuke.Modules.ActiveForums.Exceptions.LogException(ex);
                        }
                    }
                }
            }

            var attachmentController = new DotNetNuke.Modules.ActiveForums.Controllers.AttachmentController();
            foreach (var attach in attachmentController.Get().ToList())
            {
                this.RelocateAttachment_090700(attach);
            }

            // After all attachments have been relocated, move any remaining files from the old location to an orphaned folder the new location, then delete the old folders
            var di = new System.IO.DirectoryInfo(DotNetNuke.Modules.ActiveForums.Utilities.MapPath("~/portals"));
            System.IO.DirectoryInfo[] attachmentFolders = di.GetDirectories(Globals.LegacyAttachmentsFolderName, System.IO.SearchOption.AllDirectories);

            foreach (System.IO.DirectoryInfo attachmentFolder in attachmentFolders)
            {
                if (System.IO.Directory.Exists(string.Concat(attachmentFolder.FullName, "\\orphaned")))
                {
                    System.IO.Directory.Move(string.Concat(attachmentFolder.FullName, "\\orphaned"), string.Concat(attachmentFolder.FullName.Replace(Globals.LegacyAttachmentsFolderName, Globals.ContentFolderNameBase), "\\orphaned"));
                }

                var newAttachmentOrphansFolder = string.Concat(attachmentFolder.FullName.Replace(Globals.LegacyAttachmentsFolderName, Globals.ContentFolderNameBase), "\\orphaned\\");
                if (!System.IO.Directory.Exists(newAttachmentOrphansFolder))
                {
                    System.IO.Directory.CreateDirectory(newAttachmentOrphansFolder);
                }

                List<string> attachmentFullFileNames = System.IO.Directory.EnumerateFiles(path: attachmentFolder.FullName, searchPattern: "*", searchOption: System.IO.SearchOption.AllDirectories).ToList<string>();
                foreach (var attachmentFullFileName in attachmentFullFileNames)
                {
                    var attachmentFileName = new System.IO.FileInfo(attachmentFullFileName).Name;
                    var destinationFileName = System.IO.Path.Combine(newAttachmentOrphansFolder, attachmentFileName);
                    var log = new DotNetNuke.Services.Log.EventLog.LogInfo { LogTypeKey = DotNetNuke.Abstractions.Logging.EventLogType.HOST_ALERT.ToString() };
                    log.LogProperties.Add(new LogDetailInfo("Module", Globals.ModuleFriendlyName));
                    var message = $"Moving orphaned attachment file {attachmentFileName} from {attachmentFolder.FullName} to {newAttachmentOrphansFolder}";
                    log.AddProperty("Message", message);
                    DotNetNuke.Services.Log.EventLog.LogController.Instance.AddLog(log);
                    System.IO.File.Copy(attachmentFullFileName, destinationFileName, true);
                    System.IO.File.Delete(attachmentFullFileName);
                }

                attachmentFolder.Delete(true);
            }

            /* remove legacy upload and attachment folders that are registered in DNN file manager */
            var folderManager = DotNetNuke.Services.FileSystem.FolderManager.Instance;
            {
                foreach (DotNetNuke.Abstractions.Portals.IPortalInfo portal in DotNetNuke.Entities.Portals.PortalController.Instance.GetPortals())
                {
                    var legacyUploadFolder = folderManager.GetFolder(portal.PortalId, Globals.LegacyAttachmentUploadsFolderName);
                    if (legacyUploadFolder != null)
                    {
                        try
                        {
                            var log = new DotNetNuke.Services.Log.EventLog.LogInfo { LogTypeKey = DotNetNuke.Abstractions.Logging.EventLogType.HOST_ALERT.ToString() };
                            log.LogProperties.Add(new LogDetailInfo("Module", Globals.ModuleFriendlyName));
                            var message = $"Removing legacy folder: {legacyUploadFolder.PhysicalPath}";
                            log.AddProperty("Message", message);
                            DotNetNuke.Services.Log.EventLog.LogController.Instance.AddLog(log);
                            folderManager.DeleteFolder(legacyUploadFolder);
                        }
                        catch (Exception ex)
                        {
                            DotNetNuke.Modules.ActiveForums.Exceptions.LogException(ex);
                        }
                    }

                    var legacyAttachFolder = folderManager.GetFolder(portal.PortalId, Globals.LegacyAttachmentsFolderName);
                    if (legacyAttachFolder != null)
                    {
                        try
                        {
                            var log = new DotNetNuke.Services.Log.EventLog.LogInfo { LogTypeKey = DotNetNuke.Abstractions.Logging.EventLogType.HOST_ALERT.ToString() };
                            log.LogProperties.Add(new LogDetailInfo("Module", Globals.ModuleFriendlyName));
                            var message = $"Removing legacy folder: {legacyAttachFolder.PhysicalPath}";
                            log.AddProperty("Message", message);
                            DotNetNuke.Services.Log.EventLog.LogController.Instance.AddLog(log);
                            folderManager.DeleteFolder(legacyAttachFolder);
                        }
                        catch (Exception ex)
                        {
                            DotNetNuke.Modules.ActiveForums.Exceptions.LogException(ex);
                        }
                    }
                }
            }

            /* remove legacy upload and attachment folders that are not registered in DNN file manager */
            System.IO.DirectoryInfo[] legacyUploadFolders = di.GetDirectories(Globals.LegacyAttachmentUploadsFolderName, System.IO.SearchOption.AllDirectories);
            foreach (System.IO.DirectoryInfo legacyUploadFolder in legacyUploadFolders)
            {
                try
                {
                    var log = new DotNetNuke.Services.Log.EventLog.LogInfo { LogTypeKey = DotNetNuke.Abstractions.Logging.EventLogType.HOST_ALERT.ToString() };
                    log.LogProperties.Add(new LogDetailInfo("Module", Globals.ModuleFriendlyName));
                    var message = $"Removing legacy folder: {legacyUploadFolder.FullName}";
                    log.AddProperty("Message", message);
                    DotNetNuke.Services.Log.EventLog.LogController.Instance.AddLog(log);
                    legacyUploadFolder.Delete(recursive: true);
                }
                catch (Exception ex)
                {
                    DotNetNuke.Modules.ActiveForums.Exceptions.LogException(ex);
                }
            }

            System.IO.DirectoryInfo[] legacyAttachFolders = di.GetDirectories(Globals.LegacyAttachmentsFolderName, System.IO.SearchOption.AllDirectories);
            foreach (System.IO.DirectoryInfo legacyAttachFolder in legacyAttachFolders)
            {
                try
                {
                    var log = new DotNetNuke.Services.Log.EventLog.LogInfo { LogTypeKey = DotNetNuke.Abstractions.Logging.EventLogType.HOST_ALERT.ToString() };
                    log.LogProperties.Add(new LogDetailInfo("Module", Globals.ModuleFriendlyName));
                    var message = $"Removing legacy folder: {legacyAttachFolder.FullName}";
                    log.AddProperty("Message", message);
                    DotNetNuke.Services.Log.EventLog.LogController.Instance.AddLog(log);
                    legacyAttachFolder.Delete(recursive: true);
                }
                catch (Exception ex)
                {
                    DotNetNuke.Modules.ActiveForums.Exceptions.LogException(ex);
                }
            }

            try
            {
                var log = new DotNetNuke.Services.Log.EventLog.LogInfo { LogTypeKey = DotNetNuke.Abstractions.Logging.EventLogType.HOST_ALERT.ToString() };
                log.LogProperties.Add(new LogDetailInfo("Module", Globals.ModuleFriendlyName));
                var message = $"dropping column FileData from activeforums_Attachments table";
                log.AddProperty("Message", message);
                DotNetNuke.Services.Log.EventLog.LogController.Instance.AddLog(log);

                DotNetNuke.Data.DataContext.Instance().Execute(System.Data.CommandType.Text, "IF EXISTS(SELECT * FROM sys.columns WHERE [name] = N'FileData' AND [object_id] = OBJECT_ID(N'{databaseOwner}[{objectQualifier}activeforums_Attachments]')) ALTER TABLE {databaseOwner}[{objectQualifier}activeforums_Attachments] DROP COLUMN FileData");
            }
            catch (Exception ex)
            {
                DotNetNuke.Modules.ActiveForums.Exceptions.LogException(ex);
            }
        }

        private void RelocateAttachment_090700(DotNetNuke.Modules.ActiveForums.Entities.AttachmentInfo attachment)
        {
            if (attachment == null)
            {
                return;
            }

            var originalAttachmentFileName = attachment.FileName;
            var fileManager = DotNetNuke.Services.FileSystem.FileManager.Instance;
            var folderManager = DotNetNuke.Services.FileSystem.FolderManager.Instance;

            DotNetNuke.Services.FileSystem.IFileInfo file = null;

            var content = new DotNetNuke.Modules.ActiveForums.Controllers.ContentController().GetById(attachment.ContentId, DotNetNuke.Common.Utilities.Null.NullInteger);
            if (content != null)
            {
                if (attachment.DisplayInline == true)
                {
                    this.RelocateInlineAttachment_090700(attachment, content);
                    return;
                }

                try
                {
                    var legacyAttachmentsPath = Utilities.MapPath(content.Post.Forum.PortalSettings.HomeDirectory + $"{Globals.LegacyAttachmentsFolderName}/");
                    var orphanedAttachmentsPath = string.Concat(legacyAttachmentsPath, "\\orphaned\\");
                    var attachmentFolder =
                            folderManager.GetFolder(content.Post.PortalId, string.Format(Globals.AttachmentsFolderNameFormatString, content.ModuleId, content.ContentId)) ??
                            folderManager.AddFolder(content.Post.PortalId, string.Format(Globals.AttachmentsFolderNameFormatString, content.ModuleId, content.ContentId));

                    if (attachment.FileId.HasValue && attachment.FileId > 0)
                    {
                        file = fileManager.GetFile((int)attachment.FileId);

                        if (file == null)
                        {
                            var log = new DotNetNuke.Services.Log.EventLog.LogInfo { LogTypeKey = DotNetNuke.Abstractions.Logging.EventLogType.HOST_ALERT.ToString() };
                            log.LogProperties.Add(new LogDetailInfo("Module", Globals.ModuleFriendlyName));
                            var message = $"Unable to locate file with file id {attachment.FileId} and name {attachment.FileName} for user with id {attachment.UserId} while relocating attachment with id {attachment.AttachmentId} and content id {attachment.ContentId}; removing attachment record";
                            log.AddProperty("Message", message);
                            DotNetNuke.Services.Log.EventLog.LogController.Instance.AddLog(log);
                            new DotNetNuke.Modules.ActiveForums.Controllers.AttachmentController().Delete(attachment);
                            return;
                        }

                        /* file already in correct location; nothing to do */
                        if (file.FolderId == attachmentFolder.FolderID)
                        {
                                return;
                        }

                        /* if the file is not set as an inline attachment, check if the content contains the original URL for the file.
                        * If it does, set the attachment to be an inline attachment and move it to the embedded images folder.
                        * This is necessary to ensure that existing content with inline attachments continues to work after we start storing inline attachments in a separate folder.
                        */

                        var originalUrl = fileManager.GetUrl(file);
                        if (content.Body.ToLowerInvariant().Contains(originalUrl.ToLowerInvariant()))
                        {
                            if (!attachment.DisplayInline)
                            {
                                attachment.DisplayInline = true;
                                new DotNetNuke.Modules.ActiveForums.Controllers.AttachmentController().Update(attachment);
                            }

                            this.RelocateInlineAttachment_090700(attachment, content);
                            return;
                        }

                        // if file uploaded by a host/superuser and portalid is not set, set the portalid to the content's portalid before moving the file
                        if (file.PortalId != content.Post.PortalId)
                        {
                            file.PortalId = content.Post.PortalId;
                            file = fileManager.UpdateFile(file);
                        }

                        file = fileManager.MoveFile(file, attachmentFolder);
                    }
                    else
                    {
                        /* use direct SQL command rather than DAL2 entity so the column can be removed after data is migrated */
                        byte[] fileBytes = null;
                        var fileDataColumnObjectId = DotNetNuke.Data.DataContext.Instance().ExecuteQuery<int?>(System.Data.CommandType.Text, "SELECT object_id FROM sys.columns WHERE [name] = N'FileData' AND [object_id] = OBJECT_ID(N'{databaseOwner}[{objectQualifier}activeforums_Attachments]')").FirstOrDefault();
                        if (fileDataColumnObjectId != null)
                        {
                            var fileData = DotNetNuke.Data.DataContext.Instance().ExecuteQuery<byte[]>(System.Data.CommandType.Text, "SELECT FileData FROM {databaseOwner}[{objectQualifier}activeforums_Attachments] WHERE AttachmentId = @0", attachment.AttachmentId).FirstOrDefault();
                            if (fileData != null && fileData.Length > 0)
                            {
                                fileBytes = fileData;
                            }
                        }

                        if (fileDataColumnObjectId == null || fileBytes == null)
                        {
                            var filelocation = legacyAttachmentsPath + attachment.FileName;
                            if (!System.IO.File.Exists(filelocation))
                            {
                                const string fileNameTemplate = Globals.LegacyAttachmentFileNameFormatString;
                                filelocation = System.IO.Directory.EnumerateFiles(path: legacyAttachmentsPath, searchPattern: string.Format(fileNameTemplate, attachment.ContentId, "*", attachment.FileName), searchOption: System.IO.SearchOption.AllDirectories).FirstOrDefault();
                                if (filelocation == null || !System.IO.File.Exists(filelocation))
                                {
                                    // If the file doesn't exist in the legacy attachment location, attempt to find it in the user's folder (legacy location for user-uploaded attachments)
                                    var userInfo = DotNetNuke.Entities.Users.UserController.Instance.GetUserById(content.Post.PortalId, attachment.UserId);
                                    if (userInfo == null)
                                    {
                                        var log = new DotNetNuke.Services.Log.EventLog.LogInfo { LogTypeKey = DotNetNuke.Abstractions.Logging.EventLogType.HOST_ALERT.ToString() };
                                        log.LogProperties.Add(new LogDetailInfo("Module", Globals.ModuleFriendlyName));
                                        var message = $"Unable to locate user with id {attachment.UserId} while relocating attachment with id {attachment.AttachmentId} and content id {attachment.ContentId}";
                                        log.AddProperty("Message", message);
                                        DotNetNuke.Services.Log.EventLog.LogController.Instance.AddLog(log);
                                        return;
                                    }

                                    var userFolder = folderManager.GetUserFolder(userInfo);
                                    if (userFolder == null)
                                    {
                                        var log = new DotNetNuke.Services.Log.EventLog.LogInfo { LogTypeKey = DotNetNuke.Abstractions.Logging.EventLogType.HOST_ALERT.ToString() };
                                        log.LogProperties.Add(new LogDetailInfo("Module", Globals.ModuleFriendlyName));
                                        var message = $"Unable to locate user folder for user id {attachment.UserId} while relocating attachment with id {attachment.AttachmentId} and content id {attachment.ContentId}";
                                        log.AddProperty("Message", message);
                                        DotNetNuke.Services.Log.EventLog.LogController.Instance.AddLog(log);
                                        return;
                                    }

                                    filelocation = System.IO.Directory.EnumerateFiles(path: userFolder.PhysicalPath, searchPattern: string.Format(fileNameTemplate, attachment.ContentId, "*", attachment.FileName), searchOption: System.IO.SearchOption.AllDirectories).FirstOrDefault();
                                    if (filelocation == null || !System.IO.File.Exists(filelocation))
                                    {
                                        // look in orphan attachments folder
                                        filelocation = orphanedAttachmentsPath + attachment.FileName;
                                        if (!System.IO.File.Exists(filelocation))
                                        {
                                         filelocation = System.IO.Directory.EnumerateFiles(path: orphanedAttachmentsPath, searchPattern: string.Format(fileNameTemplate, attachment.ContentId, "*", attachment.FileName), searchOption: System.IO.SearchOption.AllDirectories).FirstOrDefault();
                                        }
                                    }
                                }
                            }

                            if (filelocation == null || !System.IO.File.Exists(filelocation))
                            {
                                var log = new DotNetNuke.Services.Log.EventLog.LogInfo { LogTypeKey = DotNetNuke.Abstractions.Logging.EventLogType.HOST_ALERT.ToString() };
                                log.LogProperties.Add(new LogDetailInfo("Module", Globals.ModuleFriendlyName));
                                var message = $"Unable to locate file for attachment filename {attachment.FileName} for user id {attachment.UserId} while relocating attachment with id {attachment.AttachmentId} and content id {attachment.ContentId}; removing attachment record.";
                                log.AddProperty("Message", message);
                                DotNetNuke.Services.Log.EventLog.LogController.Instance.AddLog(log);
                                new DotNetNuke.Modules.ActiveForums.Controllers.AttachmentController().Delete(attachment);
                                return;
                            }

                            fileBytes = System.IO.File.ReadAllBytes(filelocation);
                            System.IO.File.Delete(filelocation);
                        }

                        if (fileBytes != null)
                        {
                            using (var ms = new MemoryStream())
                            {
                                ms.Write(fileBytes, 0, fileBytes.Length);
                                ms.Position = 0;

                                attachment.FileName = DotNetNuke.Modules.ActiveForums.Controllers.AttachmentController.CreateUniqueFileName(attachmentFolder.PhysicalPath, attachment.FileName);
                                if (attachment.ContentType.ToLowerInvariant().StartsWith("image/"))
                                {
                                    System.Drawing.Imaging.ImageFormat saveFormat = null;
                                    switch (attachment.ContentType.ToLowerInvariant())
                                    {
                                        case "image/jpeg":
                                        case "image/jpg":
                                            saveFormat = System.Drawing.Imaging.ImageFormat.Jpeg;
                                            break;
                                        case "image/gif":
                                            saveFormat = System.Drawing.Imaging.ImageFormat.Gif;
                                            break;
                                        case "image/x-png":
                                        case "image/png":
                                            saveFormat = System.Drawing.Imaging.ImageFormat.Png;
                                            break;
                                        default:
                                            break;
                                    }

                                    if (saveFormat != null)
                                    {
                                        attachment.FileName = System.IO.Path.ChangeExtension(attachment.FileName, $".{saveFormat.ToString().ToLowerInvariant()}");
                                    }
                                }

                                if (string.IsNullOrEmpty(System.IO.Path.GetExtension(attachment.FileName)))
                                {
                                    var log = new DotNetNuke.Services.Log.EventLog.LogInfo { LogTypeKey = DotNetNuke.Abstractions.Logging.EventLogType.HOST_ALERT.ToString() };
                                    log.LogProperties.Add(new LogDetailInfo("Module", Globals.ModuleFriendlyName));
                                    var message = $"Attachment filename {attachment.FileName} has invalid file extension for user id {attachment.UserId} while relocating attachment with id {attachment.AttachmentId} and content id {attachment.ContentId}; skipping file move and leaving attachment record without file reference";
                                    log.AddProperty("Message", message);
                                    DotNetNuke.Services.Log.EventLog.LogController.Instance.AddLog(log);
                                    return;
                                }

                                try
                                {
                                    file = fileManager.AddFile(attachmentFolder, attachment.FileName, ms, true);
                                }
                                catch (Exception ex)
                                {
                                    var log = new DotNetNuke.Services.Log.EventLog.LogInfo { LogTypeKey = DotNetNuke.Abstractions.Logging.EventLogType.HOST_ALERT.ToString() };
                                    log.LogProperties.Add(new LogDetailInfo("Module", Globals.ModuleFriendlyName));
                                    var message = $"Unable to add file to DNN file manager for attachment filename {attachment.FileName} for user id {attachment.UserId} while relocating attachment with id {attachment.AttachmentId} and content id {attachment.ContentId}";
                                    log.AddProperty("Message", message);
                                    DotNetNuke.Services.Log.EventLog.LogController.Instance.AddLog(log);
                                    DotNetNuke.Modules.ActiveForums.Exceptions.LogException(ex);
                                }
                            }

                            if (file != null)
                            {
                                attachment.FileId = file.FileId;
                                attachment.FileSize = file.Size;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    var log = new DotNetNuke.Services.Log.EventLog.LogInfo { LogTypeKey = DotNetNuke.Abstractions.Logging.EventLogType.HOST_ALERT.ToString() };
                    log.LogProperties.Add(new LogDetailInfo("Module", Globals.ModuleFriendlyName));
                    var message = $"Exception while migrating attachment {attachment.FileName} for user id {attachment.UserId} while relocating attachment with id {attachment.AttachmentId} and content id {attachment.ContentId}";
                    log.AddProperty("Message", message);
                    DotNetNuke.Modules.ActiveForums.Exceptions.LogException(ex);
                }

                attachment.UserId = content.AuthorId;
                new DotNetNuke.Modules.ActiveForums.Controllers.AttachmentController().Update(attachment);
            }
        }

        private void RelocateInlineAttachment_090700(DotNetNuke.Modules.ActiveForums.Entities.AttachmentInfo attachment, DotNetNuke.Modules.ActiveForums.Entities.ContentInfo content)
        {
            if (attachment == null || attachment.DisplayInline == false)
            {
                return;
            }

            try
            {
                var legacyAttachmentsPath = Utilities.MapPath(content.Post.Forum.PortalSettings.HomeDirectory + $"{Globals.LegacyAttachmentsFolderName}/");
                var orphanedAttachmentsPath = string.Concat(legacyAttachmentsPath, "\\orphaned\\");
                var fileManager = DotNetNuke.Services.FileSystem.FileManager.Instance;
                var folderManager = DotNetNuke.Services.FileSystem.FolderManager.Instance;
                DotNetNuke.Services.FileSystem.IFileInfo file = null;

                var originalUrl = string.Empty;
                var originalAttachmentFileName = attachment.FileName;

                if (content != null)
                {
                    var embeddedImagesFolder =
                            folderManager.GetFolder(content.Post.PortalId, string.Format(Globals.EmbeddedImagesFolderNameFormatString, content.ModuleId, content.ContentId)) ??
                            folderManager.AddFolder(content.Post.PortalId, string.Format(Globals.EmbeddedImagesFolderNameFormatString, content.ModuleId, content.ContentId));

                    if (attachment.FileId.HasValue && attachment.FileId > 0)
                    {
                        file = fileManager.GetFile((int)attachment.FileId);
                        if (file == null)
                        {
                            var log = new DotNetNuke.Services.Log.EventLog.LogInfo { LogTypeKey = DotNetNuke.Abstractions.Logging.EventLogType.HOST_ALERT.ToString() };
                            log.LogProperties.Add(new LogDetailInfo("Module", Globals.ModuleFriendlyName));
                            var message = $"Unable to locate file with file id {attachment.FileId} and name {attachment.FileName} for user with id {attachment.UserId} while relocating attachment with id {attachment.AttachmentId} and content id {attachment.ContentId}; removing attachment record";
                            log.AddProperty("Message", message);
                            DotNetNuke.Services.Log.EventLog.LogController.Instance.AddLog(log);
                            new DotNetNuke.Modules.ActiveForums.Controllers.AttachmentController().Delete(attachment);
                            return;
                        }

                        /* file already in correct location; nothing to do */
                        if (file.FolderId == embeddedImagesFolder.FolderID)
                            {
                                return;
                        }

                        originalUrl = fileManager.GetUrl(file);

                        // if file uploaded by a host/superuser and portalid is not set, set the portalid to the content's portalid before moving the file
                        if (file.PortalId != content.Post.PortalId)
                        {
                            file.PortalId = content.Post.PortalId;
                            file = fileManager.UpdateFile(file);
                        }

                        file = fileManager.MoveFile(file, embeddedImagesFolder);
                    }
                    else
                    {
                        /* use direct SQL command rather than DAL2 entity so the column can be removed after data is migrated */
                        byte[] fileBytes = null;
                        var fileDataColumnObjectId = DotNetNuke.Data.DataContext.Instance().ExecuteQuery<int?>(System.Data.CommandType.Text, "SELECT object_id FROM sys.columns WHERE [name] = N'FileData' AND [object_id] = OBJECT_ID(N'{databaseOwner}[{objectQualifier}activeforums_Attachments]')").FirstOrDefault();
                        if (fileDataColumnObjectId != null)
                        {
                            var fileData = DotNetNuke.Data.DataContext.Instance().ExecuteQuery<byte[]>(System.Data.CommandType.Text, "SELECT FileData FROM {databaseOwner}[{objectQualifier}activeforums_Attachments] WHERE AttachmentId = @0", attachment.AttachmentId).FirstOrDefault();
                            if (fileData != null && fileData.Length > 0)
                            {
                                fileBytes = fileData;
                            }

                            if (fileBytes != null && fileBytes.Length > 0)
                            {
                                System.Drawing.Imaging.ImageFormat saveFormat = null;
                                switch (attachment.ContentType.ToLowerInvariant())
                                {
                                    case "image/jpeg":
                                    case "image/jpg":
                                        saveFormat = System.Drawing.Imaging.ImageFormat.Jpeg;
                                        break;
                                    case "image/gif":
                                        saveFormat = System.Drawing.Imaging.ImageFormat.Gif;
                                        break;
                                    case "image/png":
                                    case "image/x-png":
                                        saveFormat = System.Drawing.Imaging.ImageFormat.Png;
                                        break;
                                    default:
                                        // Unsupported image type
                                        var log = new DotNetNuke.Services.Log.EventLog.LogInfo { LogTypeKey = DotNetNuke.Abstractions.Logging.EventLogType.HOST_ALERT.ToString() };
                                        log.LogProperties.Add(new LogDetailInfo("Module", Globals.ModuleFriendlyName));
                                        var message = $"Unable to determine or invalid content type {attachment.ContentType} for file {attachment.FileName} with user id {attachment.UserId} while relocating attachment with id {attachment.AttachmentId} and content id {attachment.ContentId}";
                                        log.AddProperty("Message", message);
                                        DotNetNuke.Services.Log.EventLog.LogController.Instance.AddLog(log);
                                        return;
                                }

                                attachment.FileName = System.IO.Path.ChangeExtension(attachment.FileName, saveFormat.ToString().ToLowerInvariant());
                                using (var ms = new MemoryStream())
                                {
                                    ms.Write(fileBytes, 0, fileBytes.Length);
                                    ms.Position = 0;

                                    attachment.FileName = DotNetNuke.Modules.ActiveForums.Controllers.AttachmentController.CreateUniqueFileName(embeddedImagesFolder.PhysicalPath, attachment.FileName);
                                    file = fileManager.AddFile(embeddedImagesFolder, attachment.FileName, ms, true);
                                }

                                if (file != null)
                                {
                                    attachment.FileId = file.FileId;
                                }
                            }
                        }

                        if (fileDataColumnObjectId == null || fileBytes == null)
                        {
                            const string fileNameTemplate = Globals.LegacyAttachmentFileNameFormatString;
                            var filelocation = legacyAttachmentsPath + attachment.FileName;
                            if (!System.IO.File.Exists(filelocation))
                            {
                                filelocation = System.IO.Directory.EnumerateFiles(path: legacyAttachmentsPath, searchPattern: string.Format(fileNameTemplate, attachment.ContentId, "*", attachment.FileName), searchOption: System.IO.SearchOption.AllDirectories).FirstOrDefault();
                                if (filelocation == null || !System.IO.File.Exists(filelocation))
                                {
                                    // If the file doesn't exist in the legacy attachment location, attempt to find it in the user's folder (legacy location for user-uploaded attachments)
                                    var userInfo = DotNetNuke.Entities.Users.UserController.Instance.GetUserById(content.Post.PortalId, attachment.UserId);
                                    if (userInfo == null)
                                    {
                                        var log = new DotNetNuke.Services.Log.EventLog.LogInfo { LogTypeKey = DotNetNuke.Abstractions.Logging.EventLogType.HOST_ALERT.ToString() };
                                        log.LogProperties.Add(new LogDetailInfo("Module", Globals.ModuleFriendlyName));
                                        var message = $"Unable to locate user with id {attachment.UserId} while relocating attachment with id {attachment.AttachmentId} and content id {attachment.ContentId}";
                                        log.AddProperty("Message", message);
                                        DotNetNuke.Services.Log.EventLog.LogController.Instance.AddLog(log);
                                        return;
                                    }

                                    var userFolder = folderManager.GetUserFolder(userInfo);
                                    if (userFolder == null)
                                    {
                                        var log = new DotNetNuke.Services.Log.EventLog.LogInfo { LogTypeKey = DotNetNuke.Abstractions.Logging.EventLogType.HOST_ALERT.ToString() };
                                        log.LogProperties.Add(new LogDetailInfo("Module", Globals.ModuleFriendlyName));
                                        var message = $"Unable to locate user folder for user id {attachment.UserId} while relocating attachment with id {attachment.AttachmentId} and content id {attachment.ContentId}";
                                        log.AddProperty("Message", message);
                                        DotNetNuke.Services.Log.EventLog.LogController.Instance.AddLog(log);
                                        return;
                                    }

                                    filelocation = System.IO.Directory.EnumerateFiles(path: userFolder.PhysicalPath, searchPattern: string.Format(fileNameTemplate, attachment.ContentId, "*", attachment.FileName), searchOption: System.IO.SearchOption.AllDirectories).FirstOrDefault();
                                    if (filelocation == null || !System.IO.File.Exists(filelocation))
                                    {
                                        // look in orphan attachments folder
                                        filelocation = orphanedAttachmentsPath + attachment.FileName;
                                        if (!System.IO.File.Exists(filelocation))
                                        {
                                            filelocation = System.IO.Directory.EnumerateFiles(path: orphanedAttachmentsPath, searchPattern: string.Format(fileNameTemplate, attachment.ContentId, "*", attachment.FileName), searchOption: System.IO.SearchOption.AllDirectories).FirstOrDefault();
                                        }
                                    }
                                }
                            }

                            if (filelocation == null || !System.IO.File.Exists(filelocation))
                            {
                                var log = new DotNetNuke.Services.Log.EventLog.LogInfo { LogTypeKey = DotNetNuke.Abstractions.Logging.EventLogType.HOST_ALERT.ToString() };
                                log.LogProperties.Add(new LogDetailInfo("Module", Globals.ModuleFriendlyName));
                                var message = $"Unable to locate file for attachment filename {attachment.FileName} for user id {attachment.UserId} while relocating attachment with id {attachment.AttachmentId} and content id {attachment.ContentId}; removing attachment record";
                                log.AddProperty("Message", message);
                                DotNetNuke.Services.Log.EventLog.LogController.Instance.AddLog(log);
                                new DotNetNuke.Modules.ActiveForums.Controllers.AttachmentController().Delete(attachment);
                                return;
                            }

                            using (System.Drawing.Image imageFile = System.Drawing.Image.FromFile(filelocation))
                            {
                                if (imageFile != null)
                                {
                                    using (var ms = new MemoryStream())
                                    {
                                        // pick save format by comparing RawFormat GUIDs
                                        var raw = imageFile.RawFormat;
                                        var saveFormat = System.Drawing.Imaging.ImageFormat.Png;

                                        if (System.Drawing.Imaging.ImageFormat.Jpeg.Guid == raw.Guid)
                                        {
                                            saveFormat = System.Drawing.Imaging.ImageFormat.Jpeg;
                                        }
                                        else if (System.Drawing.Imaging.ImageFormat.Gif.Guid == raw.Guid)
                                        {
                                            saveFormat = System.Drawing.Imaging.ImageFormat.Gif;
                                        }
                                        else if (System.Drawing.Imaging.ImageFormat.Bmp.Guid == raw.Guid)
                                        {
                                            saveFormat = System.Drawing.Imaging.ImageFormat.Bmp;
                                        }

                                        attachment.FileName = System.IO.Path.ChangeExtension(attachment.FileName, $".{saveFormat.ToString().ToLowerInvariant()}");
                                        attachment.FileName = DotNetNuke.Modules.ActiveForums.Controllers.AttachmentController.CreateUniqueFileName(embeddedImagesFolder.PhysicalPath, attachment.FileName);

                                        try
                                        {
                                            imageFile.Save(ms, saveFormat);
                                            ms.Position = 0;
                                            file = fileManager.AddFile(embeddedImagesFolder, attachment.FileName, ms, true);
                                        }
                                        catch (Exception ex)
                                        {
                                            DotNetNuke.Modules.ActiveForums.Exceptions.LogException(ex);
                                            var log = new DotNetNuke.Services.Log.EventLog.LogInfo { LogTypeKey = DotNetNuke.Abstractions.Logging.EventLogType.HOST_ALERT.ToString() };
                                            log.LogProperties.Add(new LogDetailInfo("Module", Globals.ModuleFriendlyName));
                                            var message = $"Unable to add file to DNN file manager for attachment filename {attachment.FileName} for user id {attachment.UserId} while relocating attachment with id {attachment.AttachmentId} and content id {attachment.ContentId}";
                                            log.AddProperty("Message", message);
                                            DotNetNuke.Services.Log.EventLog.LogController.Instance.AddLog(log);
                                        }
                                    }
                                }
                            }

                            System.IO.File.Delete(filelocation);
                        }
                    }

                    if (file != null)
                    {
                        attachment.ContentType = file.ContentType;
                        attachment.FileId = file.FileId;
                        attachment.FileSize = file.Size;
                        attachment.UserId = content.AuthorId;
                    }

                    new DotNetNuke.Modules.ActiveForums.Controllers.AttachmentController().Update(attachment);

                    var width = file.Width;
                    var height = file.Height;
                    if (width > content.Post.Forum.FeatureSettings.MaxImageWidth ||
                        height > content.Post.Forum.FeatureSettings.MaxImageHeight)
                    {
                        if (width > 0 && height > 0)
                        {
                            var widthRatio = (double)content.Post.Forum.FeatureSettings.MaxImageWidth / width;
                            var heightRatio = (double)content.Post.Forum.FeatureSettings.MaxImageHeight / height;
                            var ratio = Math.Min(widthRatio, heightRatio);
                            width = (int)(width * ratio);
                            height = (int)(height * ratio);
                        }
                        else
                        {
                            width = content.Post.Forum.FeatureSettings.MaxImageWidth;
                            height = content.Post.Forum.FeatureSettings.MaxImageHeight;
                        }
                    }

                    const string imgSrcPattern = @"(?<tag><img.*?src=\""(?<src>[^>\""].*?)\"".*?>)";
                    var matches = RegexUtils.GetCachedRegex(imgSrcPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase).Matches(content.Body);
                    foreach (Match match in matches)
                    {
                        if (match.Groups["src"].Success && (match.Groups["src"].Value.EndsWith(originalAttachmentFileName, StringComparison.InvariantCultureIgnoreCase) || (!string.IsNullOrEmpty(originalUrl) && match.Groups["src"].Value.ToLowerInvariant().Contains(originalUrl.ToLowerInvariant()))))
                        {
                            var tag = Utilities.ResolveUrlInTag($"<img src=\"https://{content.Post.Forum.PortalSettings.DefaultPortalAlias}{fileManager.GetUrl(file)}\" width=\"{width}\" height=\"{height}\" loading=\"lazy\" />", content.Post.Forum.PortalSettings.DefaultPortalAlias, content.Post.Forum.PortalSettings.SSLEnabled);
                            content.Body = content.Body.Replace(match.Groups["tag"].Value, tag);
                            new DotNetNuke.Modules.ActiveForums.Controllers.ContentController().Save(content, content.ContentId);
                        }
                    }
                }
            }
                catch (Exception ex)
                {
                    var log = new DotNetNuke.Services.Log.EventLog.LogInfo { LogTypeKey = DotNetNuke.Abstractions.Logging.EventLogType.HOST_ALERT.ToString() };
                    log.LogProperties.Add(new LogDetailInfo("Module", Globals.ModuleFriendlyName));
                    var message = $"Exception while migrating attachment {attachment.FileName} for user id {attachment.UserId} while relocating attachment with id {attachment.AttachmentId} and content id {attachment.ContentId}";
                    log.AddProperty("Message", message);
                    DotNetNuke.Modules.ActiveForums.Exceptions.LogException(ex);
                }
        }
    }
}
