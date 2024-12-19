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
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Http.Results;
    using System.Web.Profile;
    using System.Web.UI;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Collections;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Definitions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Modules.ActiveForums.Data;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Localization;
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

                // Add Default Templates
                this.LoadTemplates(portalId, moduleId);

                // Add Default Status
                this.LoadFilters(portalId, moduleId);

                // Add Default Steps
                this.LoadRanks(portalId, moduleId);

                // Add Default Forums
                this.LoadDefaultForums(portalId, moduleId);

                this.Install_Or_Upgrade_MoveTemplates();

                // templates are loaded; map new forumview template id
                this.UpdateForumViewTemplateId(portalId, moduleId);

                // Create "User Banned" core messaging notification type new in 08.01.00
                ForumsConfig.Install_BanUser_NotificationType_080100();

                // Create "like notification" core messaging notification type new in 08.02.00
                ForumsConfig.Install_LikeNotificationType_080200();

                // Create "Pin notification" core messaging notification type new in 08.02.00
                ForumsConfig.Install_PinNotificationType_080200();

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

        private void LoadTemplates(int portalId, int moduleId)
        {
            try
            {
                var xDoc = new System.Xml.XmlDocument();
                xDoc.Load(this.sPath);
                if (xDoc != null)
                {
                    System.Xml.XmlNode xRoot = xDoc.DocumentElement;
                    System.Xml.XmlNodeList xNodeList = xRoot.SelectNodes("//templates/template");
                    if (xNodeList.Count > 0)
                    {
                        var tc = new TemplateController();
                        int i;
                        for (i = 0; i < xNodeList.Count; i++)
                        {
                            var ti = new TemplateInfo
                            {
                                TemplateId = -1,
                                TemplateType =
                                                 (Templates.TemplateTypes)
                                                 Enum.Parse(typeof(Templates.TemplateTypes), xNodeList[i].Attributes["templatetype"].Value),
                                IsSystem = true,
                                PortalId = portalId,
                                ModuleId = moduleId,
                                Title = xNodeList[i].Attributes["templatetitle"].Value,
                                Subject = xNodeList[i].Attributes["templatesubject"].Value,
                                Template = Utilities.GetFileContent(xNodeList[i].Attributes["templatefile"].Value),
                            };
                            tc.Template_Save(ti);
                        }
                    }
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

                    Install_Upgrade_ForumDefaultSettingsAndSecurity_080200(portalId: portalId, moduleId: moduleId);
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
                            GroupSettingsKey = $"M:{moduleId}",
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
                                    var fi = new DotNetNuke.Modules.ActiveForums.Entities.ForumInfo
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
                                        ForumSettingsKey = $"G:{groupId}",
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

        private void UpdateForumViewTemplateId(int portalId, int moduleId)
        {
            try
            {
                var tc = new TemplateController();
                int forumViewTemplateId = tc.Template_Get(templateName: "ForumView", portalId: portalId, moduleId: moduleId).TemplateId;
                var objModules = new DotNetNuke.Entities.Modules.ModuleController();
                objModules.UpdateModuleSetting(moduleId, SettingKeys.ForumTemplateId, Convert.ToString(forumViewTemplateId));
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        internal void Install_Or_Upgrade_RelocateDefaultThemeToLegacy()
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

        internal void Install_Or_Upgrade_RenameThemeCssFiles()
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

        internal void Install_Or_Upgrade_MoveTemplates()
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
                }
            }

            TemplateController tc = new TemplateController();
            foreach (TemplateInfo templateInfo in tc.Template_List(-1, -1))
            {
                /* during upgrade, explicitly (re-)load template text from database rather than Template_List  API since API loads template using fallback/default logic and doesn't yet have the upgraded template text */
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

        internal static void FillMissingTopicUrls()
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
                    string newUsers = string.Join(";", newAuthUsers);
                    if (!string.IsNullOrEmpty(newUsers))
                    {
                        newUsers += ";";
                    }
                    string newGroups = string.Join(";", newAuthGroups);
                    if (!string.IsNullOrEmpty(newGroups))
                    {
                        newGroups += ";";
                    }

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
            try
            {
                TemplateController tc = new TemplateController();
                foreach (TemplateInfo templateInfo in tc.Template_List(-1, -1))
                {
                    if (templateInfo.TemplateType == Templates.TemplateTypes.Email)
                    {
                        try
                        {
                            templateInfo.Subject = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.MapLegacyEmailNotificationTokenSynonyms(new StringBuilder(templateInfo.Subject), PortalSettings.Current, PortalSettings.Current.DefaultLanguage).ToString();
                            tc.Template_Save(templateInfo);
                            Settings.SaveSetting(templateInfo.ModuleId, $"M:{templateInfo.ModuleId}", ForumSettingKeys.EmailNotificationSubjectTemplate, templateInfo.Subject);
                            DotNetNuke.Modules.ActiveForums.DataCache.ClearAllCache(templateInfo.ModuleId);
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

        internal static void Upgrade_RelocateSqlFiles_080200()
        {
            try
            {
                DotNetNuke.Services.Log.EventLog.LogInfo log;
                string message;

                var di = new System.IO.DirectoryInfo(Utilities.MapPath($"{Globals.ModulePath}sql/sql/"));
                System.IO.DirectoryInfo[] themeFolders = di.GetDirectories();
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
                            Install_Upgrade_ForumDefaultSettingsAndSecurity_080200(portalId: module.PortalID, moduleId: module.ModuleID);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        internal static void Install_Upgrade_ForumDefaultSettingsAndSecurity_080200(int portalId, int moduleId)
        {
            try
            {
                var permissions = new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController().CreateAdminPermissions(DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetAdministratorsRoleId(portalId).ToString(), moduleId);
                DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.CreateDefaultSets(portalId, moduleId, permissions.PermissionsId);
                DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(moduleId, SettingKeys.DefaultPermissionId, permissions.PermissionsId.ToString());

                string sKey = $"M:{moduleId}";
                DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(moduleId, SettingKeys.DefaultSettingsKey, sKey);
                if (string.IsNullOrEmpty(SettingsBase.GetModuleSettings(moduleId).ForumFeatureSettings.EmailNotificationSubjectTemplate))
                {
                    Settings.SaveSetting(moduleId, sKey, ForumSettingKeys.EmailNotificationSubjectTemplate, "[FORUMAUTHOR:DISPLAYNAME] [POSTEDORREPLIEDTO] [SUBSCRIBEDFORUMORTOPICSUBJECTFORUMNAME] on [PORTAL:PORTALNAME]");
                }

                Settings.SaveSetting(moduleId, sKey, ForumSettingKeys.EmailAddress, string.Empty);
                Settings.SaveSetting(moduleId, sKey, ForumSettingKeys.UseFilter, "true");
                Settings.SaveSetting(moduleId, sKey, ForumSettingKeys.AllowPostIcon, "false");
                Settings.SaveSetting(moduleId, sKey, ForumSettingKeys.AllowLikes, "true");
                Settings.SaveSetting(moduleId, sKey, ForumSettingKeys.AllowEmoticons, "false");
                Settings.SaveSetting(moduleId, sKey, ForumSettingKeys.AllowScript, "false");
                Settings.SaveSetting(moduleId, sKey, ForumSettingKeys.IndexContent, "true");
                Settings.SaveSetting(moduleId, sKey, ForumSettingKeys.AllowRSS, "true");
                Settings.SaveSetting(moduleId, sKey, ForumSettingKeys.AllowAttach, "true");
                Settings.SaveSetting(moduleId, sKey, ForumSettingKeys.AttachCount, "3");
                Settings.SaveSetting(moduleId, sKey, ForumSettingKeys.AttachMaxSize, "1000");
                Settings.SaveSetting(moduleId, sKey, ForumSettingKeys.AttachTypeAllowed, "txt,tiff,pdf,xls,xlsx,doc,docx,ppt,pptx");

                Settings.SaveSetting(moduleId, sKey, ForumSettingKeys.AttachMaxHeight, "450");
                Settings.SaveSetting(moduleId, sKey, ForumSettingKeys.AttachMaxWidth, "450");
                Settings.SaveSetting(moduleId, sKey, ForumSettingKeys.AttachAllowBrowseSite, "true");
                Settings.SaveSetting(moduleId, sKey, ForumSettingKeys.MaxAttachHeight, "800");
                Settings.SaveSetting(moduleId, sKey, ForumSettingKeys.MaxAttachWidth, "800");
                Settings.SaveSetting(moduleId, sKey, ForumSettingKeys.AttachInsertAllowed, "false");
                Settings.SaveSetting(moduleId, sKey, ForumSettingKeys.ConvertingToJpegAllowed, "false");
                Settings.SaveSetting(moduleId, sKey, ForumSettingKeys.AllowHTML, "true");
                Settings.SaveSetting(moduleId, sKey, ForumSettingKeys.EditorType, ((int)EditorTypes.HTMLEDITORPROVIDER).ToString());
                Settings.SaveSetting(moduleId, sKey, ForumSettingKeys.EditorMobile, ((int)EditorTypes.HTMLEDITORPROVIDER).ToString());

                Settings.SaveSetting(moduleId, sKey, ForumSettingKeys.EditorHeight, "350");
                Settings.SaveSetting(moduleId, sKey, ForumSettingKeys.EditorWidth, "99%");
                Settings.SaveSetting(moduleId, sKey, ForumSettingKeys.EditorToolbar, "bold,italic,underline,quote");
                Settings.SaveSetting(moduleId, sKey, ForumSettingKeys.EditorStyle, "2");
                Settings.SaveSetting(moduleId, sKey, ForumSettingKeys.IsModerated, "false");
                Settings.SaveSetting(moduleId, sKey, ForumSettingKeys.DefaultTrustLevel, "0");
                Settings.SaveSetting(moduleId, sKey, ForumSettingKeys.AutoTrustLevel, "0");
                Settings.SaveSetting(moduleId, sKey, ForumSettingKeys.ModApproveTemplateId, "0");
                Settings.SaveSetting(moduleId, sKey, ForumSettingKeys.ModRejectTemplateId, "0");
                Settings.SaveSetting(moduleId, sKey, ForumSettingKeys.ModMoveTemplateId, "0");
                Settings.SaveSetting(moduleId, sKey, ForumSettingKeys.ModDeleteTemplateId, "0");
                Settings.SaveSetting(moduleId, sKey, ForumSettingKeys.ModNotifyTemplateId, "0");

                var tc = new TemplateController();
                int profileInfoTemplateId = tc.Template_Get(templateName: "ProfileInfo", portalId: portalId, moduleId: moduleId).TemplateId;
                int replyEditorTemplateId = tc.Template_Get(templateName: "ReplyEditor", portalId: portalId, moduleId: moduleId).TemplateId;
                int quickReplyTemplateId = tc.Template_Get(templateName: "QuickReply", portalId: portalId, moduleId: moduleId).TemplateId;
                int topicEditorTemplateId = tc.Template_Get(templateName: "TopicEditor", portalId: portalId, moduleId: moduleId).TemplateId;
                int topicsViewTemplateId = tc.Template_Get(templateName: "TopicsView", portalId: portalId, moduleId: moduleId).TemplateId;
                int topicViewTemplateId = tc.Template_Get(templateName: "TopicView", portalId: portalId, moduleId: moduleId).TemplateId;
                Settings.SaveSetting(moduleId, sKey, ForumSettingKeys.TopicsTemplateId, Convert.ToString(topicsViewTemplateId));
                Settings.SaveSetting(moduleId, sKey, ForumSettingKeys.TopicsTemplateId, Convert.ToString(topicsViewTemplateId));
                Settings.SaveSetting(moduleId, sKey, ForumSettingKeys.TopicTemplateId, Convert.ToString(topicViewTemplateId));
                Settings.SaveSetting(moduleId, sKey, ForumSettingKeys.TopicFormId, Convert.ToString(topicEditorTemplateId));
                Settings.SaveSetting(moduleId, sKey, ForumSettingKeys.ReplyFormId, Convert.ToString(replyEditorTemplateId));
                Settings.SaveSetting(moduleId, sKey, ForumSettingKeys.QuickReplyFormId, Convert.ToString(quickReplyTemplateId));
                Settings.SaveSetting(moduleId, sKey, ForumSettingKeys.ProfileTemplateId, Convert.ToString(profileInfoTemplateId));
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
    }
}
