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

namespace DotNetNuke.Modules.ActiveForums.Helpers
{
    using System;
    using System.Collections;
    using System.Linq;
    using System.Text;
    using System.Web.UI;
    using System.Xml;

    using DotNetNuke.Collections;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Modules.ActiveForums.Controllers;
    using DotNetNuke.Modules.ActiveForums.Entities;
    using DotNetNuke.Modules.ActiveForums.Extensions;
    using DotNetNuke.Modules.ActiveForums.Services.Cache;
    using DotNetNuke.Services.Log.EventLog;

    internal static class Upgrades
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Upgrades));

        internal static void MoveSettings_070011()
        {
            /* at some point around v6, general module settings were moved from the activeforums_settings table to the DNN platform Settings table;
             * the code that did that migration would check every time during page load (in ForumBase.OnLoad()) to see if the settings conversion was required.
             * So code has been moved here, and is now called once during module upgrade for one version to ensure that this is done.
             */

            foreach (DotNetNuke.Abstractions.Portals.IPortalInfo portal in DotNetNuke.Entities.Portals.PortalController.Instance.GetPortals())
            {
                foreach (ModuleInfo module in DotNetNuke.Entities.Modules.ModuleController.Instance.GetModules(portal.PortalId))
                {
                    if (module.DesktopModule.ModuleName.Trim().ToLowerInvariant() == Globals.ModuleName.ToLowerInvariant())
                    {
                        if (!SettingsBase.GetModuleSettings(module.ModuleID).IsInstalled)
                        {
                            MoveSettingsForModuleInstanceToTabModuleInstance_070011(module.ModuleID, tabModuleId: module.TabModuleID);
                        }
                    }
                }
            }
        }

        internal static void MoveSettingsForModuleInstanceToTabModuleInstance_070011(int forumModuleId, int tabModuleId)
        {
            var ht = new Hashtable();
            DotNetNuke.Modules.ActiveForums.Controllers.SettingsController.Instance.GetSettingsForModuleIdSettingsKey(forumModuleId, "GEN").ForEach(s => ht.Add(s.SettingName, s.SettingValue));
            var currSettings = new ModuleSettings { ModuleId = forumModuleId, MainSettings = ht };

            DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(tabModuleId, SettingKeys.PageSize, currSettings.PageSize.ToString());
            DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(tabModuleId, SettingKeys.UserNameDisplay, currSettings.UserNameDisplay);
            DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(tabModuleId, SettingKeys.ProfileVisibility, ((int)currSettings.ProfileVisibility).ToString());
            DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(tabModuleId, SettingKeys.EnablePoints, currSettings.EnablePoints.ToString());
            DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(tabModuleId, SettingKeys.TopicPointValue, currSettings.TopicPointValue.ToString());
            DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(tabModuleId, SettingKeys.ReplyPointValue, currSettings.ReplyPointValue.ToString());
            DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(tabModuleId, SettingKeys.AnswerPointValue, currSettings.AnswerPointValue.ToString());
            DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(tabModuleId, SettingKeys.MarkAnswerPointValue, currSettings.MarkAsAnswerPointValue.ToString());
            DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(tabModuleId, SettingKeys.ModPointValue, currSettings.ModPointValue.ToString());
            DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(tabModuleId, SettingKeys.AvatarHeight, currSettings.AvatarHeight.ToString());
            DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(tabModuleId, SettingKeys.AvatarWidth, currSettings.AvatarWidth.ToString());
            DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(tabModuleId, SettingKeys.AllowSignatures, currSettings.AllowSignatures.ToString());
            DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(tabModuleId, SettingKeys.InstallDate, currSettings.InstallDate.ToString());
            DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(tabModuleId, SettingKeys.IsInstalled, currSettings.IsInstalled.ToString());
            DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(tabModuleId, SettingKeys.Theme, currSettings.Theme);
            DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(tabModuleId, SettingKeys.FloodInterval, currSettings.FloodInterval.ToString());
            DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(tabModuleId, SettingKeys.EditInterval, currSettings.EditInterval.ToString());
            DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(tabModuleId, SettingKeys.DeleteBehavior, currSettings.DeleteBehavior.ToString());
            DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(tabModuleId, SettingKeys.EnableAutoLink, currSettings.AutoLinkEnabled.ToString());
            DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(tabModuleId, SettingKeys.EnableURLRewriter, currSettings.URLRewriteEnabled.ToString());
            if (string.IsNullOrEmpty(currSettings.PrefixURLOther))
            {
                DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(tabModuleId, SettingKeys.PrefixURLOther, Views.views);
            }
            else
            {
                DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(tabModuleId, SettingKeys.PrefixURLOther, currSettings.PrefixURLOther);
            }

            if (string.IsNullOrEmpty(currSettings.PrefixURLTag))
            {
                DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(tabModuleId, SettingKeys.PrefixURLTags, Views.tag);
            }
            else
            {
                DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(tabModuleId, SettingKeys.PrefixURLTags, currSettings.PrefixURLTag);
            }

            if (string.IsNullOrEmpty(currSettings.PrefixURLCategory))
            {
                DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(tabModuleId, SettingKeys.PrefixURLCategories, Views.category);
            }
            else
            {
                DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(tabModuleId, SettingKeys.PrefixURLCategories, currSettings.PrefixURLCategory);
            }

            Logger.InfoFormat("Settings converted for module Id {0} tab module Id {1}", forumModuleId, tabModuleId);

            DotNetNuke.Entities.Modules.ModuleController.Instance.DeleteModuleSetting(tabModuleId, "NeedsConvert");
            DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(tabModuleId, "AFINSTALLED", "True");
            DotNetNuke.Modules.ActiveForums.Services.Cache.SettingsCache.Clear(forumModuleId, string.Format(CacheKeys.MainSettings, forumModuleId));
        }

        private enum ConversionTemplateTypes : int
        {
            All, // 0
            System, // 1
            ForumView, // 2
            TopicView, // 3
            TopicsView, // 4
            TopicForm, // 5
            ReplyForm, // 6
            QuickReplyForm, // 7
            Email, // 8
            Profile, // 9
            ModEmail, // 10
            PostInfo, // 11
        }

        private class TemplateInfoForConversion
        {
            public int ModuleId { get; set; }

            public ConversionTemplateTypes TemplateType { get; set; }

            public string Template { get; set; }

            public string FileName { get; set; }
        }

        internal static void Upgrade_Templates_080000()
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

            var templates = DotNetNuke.Data.DataContext.Instance().ExecuteQuery<TemplateInfoForConversion>(
            System.Data.CommandType.Text,
            @"SELECT ModuleId, TemplateType, FileName, Template FROM {databaseOwner}[{objectQualifier}activeforums_Templates]");
            foreach (var templateInfo in templates)
            {
                ModuleSettings moduleSettings = SettingsBase.GetModuleSettings(templateInfo.ModuleId);
                string templatePathFileName = moduleSettings.TemplatePath + templateInfo.FileName;
                if (!System.IO.Directory.Exists(Utilities.MapPath(moduleSettings.TemplatePath)))
                {
                    System.IO.Directory.CreateDirectory(Utilities.MapPath(moduleSettings.TemplatePath));
                }

                /* only convert specific templates */
                if ((templateInfo.TemplateType == ConversionTemplateTypes.ForumView) ||
                    (templateInfo.TemplateType == ConversionTemplateTypes.TopicView) ||
                    (templateInfo.TemplateType == ConversionTemplateTypes.TopicsView) ||
                    (templateInfo.TemplateType == ConversionTemplateTypes.TopicForm) ||
                    (templateInfo.TemplateType == ConversionTemplateTypes.ReplyForm) ||
                    (templateInfo.TemplateType == ConversionTemplateTypes.Profile) ||
                    (templateInfo.TemplateType == ConversionTemplateTypes.PostInfo) ||
                    (templateInfo.TemplateType == ConversionTemplateTypes.QuickReplyForm))
                {
                    try
                    {
                        /* convert only legacy html portion of the template and save without encoding */
                        var template = Convert.ToString(templateInfo.Template).Replace("[TRESX:", "[RESX:");
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

                        System.IO.File.WriteAllText(Utilities.MapPath(templatePathFileName), templateInfo.Template);
                    }
                    catch (Exception ex)
                    {
                        DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                    }
                }
            }
        }

        internal static void DeleteObsoleteModuleSettings_080100()
        {
            /* remove TIMEZONEOFFSE, AMFORUMS, MAILQUEUE */

            foreach (DotNetNuke.Abstractions.Portals.IPortalInfo portal in DotNetNuke.Entities.Portals.PortalController.Instance.GetPortals())
            {
                foreach (ModuleInfo module in DotNetNuke.Entities.Modules.ModuleController.Instance.GetModules(portal.PortalId))
                {
                    if (module.DesktopModule.ModuleName.Trim().ToLowerInvariant() == Globals.ModuleName.ToLowerInvariant())
                    {
                        DotNetNuke.Entities.Modules.ModuleController.Instance.DeleteModuleSetting(module.ModuleID, "TIMEZONEOFFSET");
                        DotNetNuke.Entities.Modules.ModuleController.Instance.DeleteModuleSetting(module.ModuleID, "AMFORUMS");
                        DotNetNuke.Entities.Modules.ModuleController.Instance.DeleteModuleSetting(module.ModuleID, "MAILQUEUE");
                    }
                }
            }
        }

        internal static void UpgradeSocialGroupForumConfigModuleSettings_080100()
        {
            foreach (DotNetNuke.Abstractions.Portals.IPortalInfo portal in DotNetNuke.Entities.Portals.PortalController.Instance.GetPortals())
            {
                foreach (ModuleInfo module in DotNetNuke.Entities.Modules.ModuleController.Instance.GetModules(portal.PortalId))
                {
                    if (module.DesktopModule.ModuleName.Trim().ToLowerInvariant() == Globals.ModuleName.ToLowerInvariant())
                    {
                        /*remove four settings previously stored in both TabModuleSettings *and* ModuleSettings -- just store in ModuleSettings */
                        DotNetNuke.Entities.Modules.ModuleController.Instance.DeleteTabModuleSetting(module.TabModuleID, SettingKeys.SocialGroupModeForumConfig);
                        DotNetNuke.Entities.Modules.ModuleController.Instance.DeleteTabModuleSetting(module.TabModuleID, SettingKeys.SocialGroupModeForumGroupTemplate);
                        DotNetNuke.Entities.Modules.ModuleController.Instance.DeleteTabModuleSetting(module.TabModuleID, "MODE");
                        DotNetNuke.Entities.Modules.ModuleController.Instance.DeleteTabModuleSetting(module.TabModuleID, "AllowIndex");
                        DotNetNuke.Modules.ActiveForums.Services.Cache.CacheBase.ClearAllCacheForTabId(module.TabID);
                        DotNetNuke.Modules.ActiveForums.Services.Cache.CacheBase.ClearAllCache(module.ModuleID);
                        var forumConfig = module.ModuleSettings.GetString(SettingKeys.SocialGroupModeForumConfig, string.Empty);
                        if (!string.IsNullOrEmpty(forumConfig))
                        {
                            var xDoc = new XmlDocument();
                            xDoc.LoadXml(forumConfig);
                            if (xDoc != null)
                            {
                                string[] secTypes = { "groupadmin", "groupmember", "registereduser", "anon" };
                                foreach (string secType in secTypes)
                                {
                                    string xpath = $"//defaultforums/forum/security[@type='{secType}']";

                                    if (xDoc.DocumentElement.SelectSingleNode(xpath).ChildNodes.Count == 16)
                                    {
                                        xDoc.DocumentElement.SelectSingleNode(xpath).AddElement("moduser", string.Empty);
                                        xDoc.DocumentElement.SelectSingleNode(xpath).SelectSingleNode("moduser").AddAttribute("value", "false");
                                    }
                                }

                                forumConfig = xDoc.OuterXml;
                                DotNetNuke.Entities.Modules.ModuleController.Instance.DeleteModuleSetting(module.ModuleID, SettingKeys.SocialGroupModeForumConfig);
                                DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(module.ModuleID, SettingKeys.SocialGroupModeForumConfig, forumConfig);
                                DotNetNuke.Modules.ActiveForums.Services.Cache.SettingsCache.Clear(module.ModuleID, string.Format(DotNetNuke.Common.Utilities.DataCache.ModuleSettingsCacheKey, module.TabID));
                                DotNetNuke.Modules.ActiveForums.Services.Cache.SettingsCache.Clear(module.ModuleID, string.Format(DotNetNuke.Common.Utilities.DataCache.TabModuleSettingsCacheKey, module.TabID));
                                DotNetNuke.Modules.ActiveForums.Services.Cache.CacheBase.ClearAllCacheForTabId(module.TabID);
                                DotNetNuke.Modules.ActiveForums.Services.Cache.CacheBase.ClearAllCache(module.ModuleID);
                            }
                        }
                    }
                }
            }
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
                            var subject = DotNetNuke.Data.DataContext.Instance().ExecuteScalar<string>(
                            System.Data.CommandType.Text,
                            @"SELECT TOP 1 Subject FROM {databaseOwner}[{objectQualifier}activeforums_Templates] WHERE TemplateType = 8 & ModuleId = @0",
                            module.ModuleID);
                            try
                            {
                                var portalSettings = PortalSettings.Current;
                                if (portalSettings == null)
                                {
                                    portalSettings = new DotNetNuke.Modules.ActiveForums.Helpers.PortalSettingsHelper().GetPortalSettings(portal.PortalId);
                                }

                                subject = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.MapLegacyEmailNotificationTokenSynonyms(new StringBuilder(subject), portalSettings, portalSettings.DefaultLanguage).ToString();
                                DotNetNuke.Modules.ActiveForums.Controllers.SettingsController.Instance.SaveSetting(module.ModuleID, $"M{module.ModuleID}", ForumSettingKeys.EmailNotificationSubjectTemplate, subject);
                                DotNetNuke.Modules.ActiveForums.Services.Cache.CacheBase.ClearAllCache(module.ModuleID);
                            }
                            catch (Exception ex)
                            {
                                Logger.Error(ex.Message, ex);
                                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
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

        internal static void UpgradeSocialGroupForumConfigModuleSettings_080200()
        {
            foreach (DotNetNuke.Abstractions.Portals.IPortalInfo portal in DotNetNuke.Entities.Portals.PortalController.Instance.GetPortals())
            {
                foreach (ModuleInfo module in DotNetNuke.Entities.Modules.ModuleController.Instance.GetModules(portal.PortalId))
                {
                    if (module.DesktopModule.ModuleName.Trim().ToLowerInvariant() == Globals.ModuleName.ToLowerInvariant())
                    {
                        var ForumConfig = module.ModuleSettings.GetString(SettingKeys.SocialGroupModeForumConfig, string.Empty);
                        if (!string.IsNullOrEmpty(ForumConfig))
                        {
                            var xDoc = new XmlDocument();
                            xDoc.LoadXml(ForumConfig);
                            if (xDoc != null)
                            {
                                string[] secTypes = { "groupadmin", "groupmember", "registereduser", "anon" };
                                foreach (string secType in secTypes)
                                {
                                    string xpath = $"//defaultforums/forum/security[@type='{secType}']";
                                    foreach (var nodename in new string[] { "modlock", "modpin", "modmove", "moddelete", "modedit", "modapprove", "moduser" })
                                    {
                                        if (xDoc.DocumentElement.SelectSingleNode(xpath).InnerXml.Contains(nodename))
                                        {
                                            try
                                            {

                                                xDoc.DocumentElement.SelectSingleNode(xpath).RemoveChild(xDoc.DocumentElement.SelectSingleNode(xpath).SelectSingleNode(nodename));
                                            }
                                            catch
                                            {
                                            }
                                        }
                                    }

                                    foreach (var nodename in new string[] { "moderate", "ban" })
                                    {
                                        if (!xDoc.DocumentElement.SelectSingleNode(xpath).InnerXml.Contains(nodename))
                                        {
                                            try
                                            {

                                                xDoc.DocumentElement.SelectSingleNode(xpath).AddElement(nodename, string.Empty);
                                                xDoc.DocumentElement.SelectSingleNode(xpath).SelectSingleNode(nodename).AddAttribute("value", "false");
                                            }
                                            catch
                                            {
                                            }
                                        }
                                    }
                                }

                                ForumConfig = xDoc.OuterXml;
                                DotNetNuke.Entities.Modules.ModuleController.Instance.DeleteModuleSetting(module.ModuleID, SettingKeys.SocialGroupModeForumConfig);
                                DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(module.ModuleID, SettingKeys.SocialGroupModeForumConfig, ForumConfig);
                                DotNetNuke.Modules.ActiveForums.Services.Cache.SettingsCache.Clear(module.ModuleID, string.Format(DotNetNuke.Common.Utilities.DataCache.ModuleSettingsCacheKey, module.TabID));
                                DotNetNuke.Modules.ActiveForums.Services.Cache.SettingsCache.Clear(module.ModuleID, string.Format(DotNetNuke.Common.Utilities.DataCache.TabModuleSettingsCacheKey, module.TabID));
                                DotNetNuke.Modules.ActiveForums.Services.Cache.CacheBase.ClearAllCacheForTabId(module.TabID);
                                DotNetNuke.Modules.ActiveForums.Services.Cache.CacheBase.ClearAllCache(module.ModuleID);
                            }
                        }
                    }
                }
            }
        }

        internal static void AddUrlPrefixLikes_080200()
        {
            foreach (DotNetNuke.Abstractions.Portals.IPortalInfo portal in DotNetNuke.Entities.Portals.PortalController.Instance.GetPortals())
            {
                foreach (ModuleInfo module in DotNetNuke.Entities.Modules.ModuleController.Instance.GetModules(portal.PortalId))
                {
                    if (module.DesktopModule.ModuleName.Trim().Equals(Globals.ModuleName, System.StringComparison.InvariantCultureIgnoreCase))
                    {
                        DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(module.ModuleID, SettingKeys.PrefixURLLikes, module.ModuleSettings.GetString(SettingKeys.PrefixURLLikes, Views.likes));
                    }
                }
            }
        }

        internal static void DeleteObsoleteModuleSettings_090000()
        {
            foreach (DotNetNuke.Abstractions.Portals.IPortalInfo portal in DotNetNuke.Entities.Portals.PortalController.Instance.GetPortals())
            {
                foreach (ModuleInfo module in DotNetNuke.Entities.Modules.ModuleController.Instance.GetModules(portal.PortalId))
                {
                    if (module.DesktopModule.ModuleName.Trim().Equals(Globals.ModuleName.Trim(), StringComparison.InvariantCultureIgnoreCase))
                    {
                        var modTemplateIds = new string[] { "MODAPPROVETEMPLATEID", "MODREJECTTEMPLATEID", "MODMOVETEMPLATEID", "MODDELETETEMPLATEID", "MODNOTIFYTEMPLATEID", };
                        var modTemplateToggles = new string[] { "MODAPPROVENOTIFY", "MODREJECTNOTIFY", "MODMOVENOTIFY", "MODDELETENOTIFY", "MODNOTIFYNOTIFY", };
                        for (int i = 0; i < modTemplateIds.Length -1; i++)
                        {
                            DotNetNuke.Data.DataContext.Instance().Execute(System.Data.CommandType.Text, "INSERT INTO {databaseOwner}{objectQualifier}activeforums_Settings SELECT [ModuleId],[SettingsKey],'@2', CASE WHEN [SettingValue] <> '0' THEN 1 ELSE 0 END FROM {databaseOwner}{objectQualifier}activeforums_Settings WHERE ModuleId = @0 AND [SettingName] = '@1'", module.ModuleID, modTemplateIds[i], modTemplateToggles[i]);
                        }

                        foreach (var settingName in new string[]
                        {
                            "MODAPPROVETEMPLATEID",
                            "MODREJECTTEMPLATEID",
                            "MODMOVETEMPLATEID",
                            "MODDELETETEMPLATEID",
                            "MODNOTIFYTEMPLATEID",
                            "TOPICSTEMPLATEID",
                            "TOPICTEMPLATEID",
                            "TOPICFORMID",
                            "REPLYFORMID",
                            "QUICKREPLYFORMID",
                            "PROFILETEMPLATEID",
                        })
                        {
                            DotNetNuke.Data.DataContext.Instance().Execute(System.Data.CommandType.Text, "DELETE FROM {databaseOwner}{objectQualifier}activeforums_Settings WHERE ModuleId = @0 AND SettingName = @1", module.ModuleID, settingName);
                        }

                        DotNetNuke.Entities.Modules.ModuleController.Instance.DeleteModuleSetting(module.ModuleID, "FORUMTEMPLATEID");
                    }

                    if (module.DesktopModule.ModuleName.Trim().Equals(Globals.ModuleName + " Viewer".Trim(), StringComparison.InvariantCultureIgnoreCase))
                    {
                        foreach (var settingName in new string[] { "AFTopicsTemplate", "AFForumViewTemplate", "AFTopicTemplate", })
                        {
                            DotNetNuke.Entities.Modules.ModuleController.Instance.DeleteModuleSetting(module.ModuleID, settingName);
                        }
                    }
                }
            }
        }

        internal static void DeleteObsoleteModuleSettings_090100()
        {
            /* remove TIMEZONEOFFSE, AMFORUMS, MAILQUEUE */

            foreach (DotNetNuke.Abstractions.Portals.IPortalInfo portal in DotNetNuke.Entities.Portals.PortalController.Instance.GetPortals())
            {
                foreach (ModuleInfo module in DotNetNuke.Entities.Modules.ModuleController.Instance.GetModules(portal.PortalId))
                {
                    if (module.DesktopModule.ModuleName.Trim().ToLowerInvariant().Equals(Globals.ModuleName.ToLowerInvariant()))
                    {
                        DotNetNuke.Entities.Modules.ModuleController.Instance.DeleteModuleSetting(module.ModuleID, "ALLOWAVATARS");
                        DotNetNuke.Entities.Modules.ModuleController.Instance.DeleteModuleSetting(module.ModuleID, "ALLOWAVATARLINKS");
                        DotNetNuke.Entities.Modules.ModuleController.Instance.DeleteModuleSetting(module.ModuleID, "AVATARDEFAULT");
                    }
                }
            }
        }

        internal static void AddAvatarModuleSettings_090100()
        {
            foreach (DotNetNuke.Abstractions.Portals.IPortalInfo portal in DotNetNuke.Entities.Portals.PortalController.Instance.GetPortals())
            {
                foreach (ModuleInfo module in DotNetNuke.Entities.Modules.ModuleController.Instance.GetModules(portal.PortalId))
                {
                    if (module.DesktopModule.ModuleName.Trim().ToLowerInvariant().Equals(Globals.ModuleName.ToLowerInvariant()))
                    {
                        DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(module.ModuleID, SettingKeys.AvatarRefresh, Globals.AvatarRefreshGravatar);
                        DotNetNuke.Modules.ActiveForums.Services.Cache.SettingsCache.Clear(module.ModuleID, string.Format(DotNetNuke.Common.Utilities.DataCache.ModuleSettingsCacheKey, module.TabID));
                        DotNetNuke.Modules.ActiveForums.Services.Cache.SettingsCache.Clear(module.ModuleID, string.Format(DotNetNuke.Common.Utilities.DataCache.TabModuleSettingsCacheKey, module.TabID));
                        DotNetNuke.Modules.ActiveForums.Services.Cache.CacheBase.ClearAllCacheForTabId(module.TabID);
                        DotNetNuke.Modules.ActiveForums.Services.Cache.CacheBase.ClearAllCache(module.ModuleID);
                    }
                }
            }
        }

        internal static void UpgradeSocialGroupForumConfigModuleSettings_090201()
        {
            foreach (DotNetNuke.Abstractions.Portals.IPortalInfo portal in DotNetNuke.Entities.Portals.PortalController.Instance.GetPortals())
            {
                foreach (ModuleInfo module in DotNetNuke.Entities.Modules.ModuleController.Instance.GetModules(portal.PortalId))
                {
                    if (module.DesktopModule.ModuleName.Trim().ToLowerInvariant() == Globals.ModuleName.ToLowerInvariant())
                    {
                        if (SettingsBase.GetModuleSettings(module.ModuleID).ModeIsSocial)
                        {
                            var ForumConfig = module.ModuleSettings.GetString(SettingKeys.SocialGroupModeForumConfig, string.Empty);
                            if (!string.IsNullOrEmpty(ForumConfig))
                            {
                                var xDoc = new XmlDocument();
                                xDoc.LoadXml(ForumConfig);
                                if (xDoc != null)
                                {
                                    string[] secTypes = { "groupadmin", "groupmember", "registereduser", "anon" };
                                    foreach (string secType in secTypes)
                                    {
                                        string xpath = $"//defaultforums/forum/security[@type='{secType}']";
                                        foreach (var nodename in new string[] { "ban" })
                                        {
                                            if (xDoc.DocumentElement.SelectSingleNode(xpath).InnerXml.Contains(nodename))
                                            {
                                                try
                                                {
                                                    xDoc.DocumentElement.SelectSingleNode(xpath).RemoveChild(xDoc.DocumentElement.SelectSingleNode(xpath).SelectSingleNode(nodename));
                                                }
                                                catch
                                                {
                                                }
                                            }
                                        }

                                        foreach (var nodename in new string[] { "manageusers" })
                                        {
                                            if (!xDoc.DocumentElement.SelectSingleNode(xpath).InnerXml.Contains(nodename))
                                            {
                                                try
                                                {
                                                    xDoc.DocumentElement.SelectSingleNode(xpath).AddElement(nodename, string.Empty);
                                                    xDoc.DocumentElement.SelectSingleNode(xpath).SelectSingleNode(nodename).AddAttribute("value", "false");
                                                }
                                                catch
                                                {
                                                }
                                            }
                                        }
                                    }

                                    ForumConfig = xDoc.OuterXml;
                                    DotNetNuke.Entities.Modules.ModuleController.Instance.DeleteModuleSetting(module.ModuleID, SettingKeys.SocialGroupModeForumConfig);
                                    DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(module.ModuleID, SettingKeys.SocialGroupModeForumConfig, ForumConfig);
                                }
                            }
                        }
                        else
                        {
                            // fix any forums that have SocialGroupId set to 0
                            DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Instance.GetForums(module.ModuleID).Where(f => !f.SocialGroupId.Equals(0)).ForEach(forum =>
                            {
                                forum.SocialGroupId = 0;
                                DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Instance.Update(forum);
                            });
                            DotNetNuke.Entities.Modules.ModuleController.Instance.DeleteModuleSetting(module.ModuleID, SettingKeys.SocialGroupModeForumConfig);
                            DotNetNuke.Entities.Modules.ModuleController.Instance.DeleteModuleSetting(module.ModuleID, SettingKeys.SocialGroupModeForumGroupTemplate);
                        }

                        DotNetNuke.Modules.ActiveForums.Services.Cache.SettingsCache.Clear(module.ModuleID, string.Format(DotNetNuke.Common.Utilities.DataCache.ModuleSettingsCacheKey, module.TabID));
                        DotNetNuke.Modules.ActiveForums.Services.Cache.SettingsCache.Clear(module.ModuleID, string.Format(DotNetNuke.Common.Utilities.DataCache.TabModuleSettingsCacheKey, module.TabID));
                        DotNetNuke.Modules.ActiveForums.Services.Cache.CacheBase.ClearAllCacheForTabId(module.TabID);
                        DotNetNuke.Modules.ActiveForums.Services.Cache.CacheBase.ClearAllCache(module.ModuleID);
                    }
                }
            }
        }

        internal static void UpgradeSocialGroupForumConfigModuleSettings_090300()
        {
            foreach (DotNetNuke.Abstractions.Portals.IPortalInfo portal in DotNetNuke.Entities.Portals.PortalController.Instance.GetPortals())
            {
                foreach (ModuleInfo module in DotNetNuke.Entities.Modules.ModuleController.Instance.GetModules(portal.PortalId))
                {
                    if (module.DesktopModule.ModuleName.Trim().ToLowerInvariant() == Globals.ModuleName.ToLowerInvariant())
                    {
                        if (SettingsBase.GetModuleSettings(module.ModuleID).ModeIsSocial)
                        {
                            var forumConfig = module.ModuleSettings.GetString(SettingKeys.SocialGroupModeForumConfig, string.Empty);
                            if (!string.IsNullOrEmpty(forumConfig))
                            {
                                var xDoc = new XmlDocument();
                                xDoc.LoadXml(forumConfig);
                                if (xDoc != null)
                                {
                                    string[] secTypes = { "groupadmin", "groupmember", "registereduser", "anon" };
                                    foreach (string secType in secTypes)
                                    {
                                        string xpath = $"//defaultforums/forum/security[@type='{secType}']";
                                        foreach (var nodename in new string[] { "tag", "mention" })
                                        {
                                            if (!xDoc.DocumentElement.SelectSingleNode(xpath).InnerXml.Contains(nodename))
                                            {
                                                try
                                                {
                                                    xDoc.DocumentElement.SelectSingleNode(xpath).AddElement(nodename, string.Empty);
                                                    xDoc.DocumentElement.SelectSingleNode(xpath).SelectSingleNode(nodename).AddAttribute("value", secType.Equals("anon") || secType.Equals("registereduser") ? "false" : "true");
                                                }
                                                catch
                                                {
                                                }
                                            }
                                        }
                                    }

                                    forumConfig = xDoc.OuterXml;
                                    DotNetNuke.Entities.Modules.ModuleController.Instance.DeleteModuleSetting(module.ModuleID, SettingKeys.SocialGroupModeForumConfig);
                                    DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(module.ModuleID, SettingKeys.SocialGroupModeForumConfig, forumConfig);
                                }
                            }
                        }

                        DotNetNuke.Modules.ActiveForums.Services.Cache.SettingsCache.Clear(module.ModuleID, string.Format(DotNetNuke.Common.Utilities.DataCache.ModuleSettingsCacheKey, module.TabID));
                        DotNetNuke.Modules.ActiveForums.Services.Cache.SettingsCache.Clear(module.ModuleID, string.Format(DotNetNuke.Common.Utilities.DataCache.TabModuleSettingsCacheKey, module.TabID));
                        DotNetNuke.Modules.ActiveForums.Services.Cache.CacheBase.ClearAllCacheForTabId(module.TabID);
                        DotNetNuke.Modules.ActiveForums.Services.Cache.CacheBase.ClearAllCache(module.ModuleID);
                    }
                }
            }
        }

        internal static void DeleteObsoleteModuleSettings_090600()
        {
            /* remove FULLTEXT */

            foreach (DotNetNuke.Abstractions.Portals.IPortalInfo portal in DotNetNuke.Entities.Portals.PortalController.Instance.GetPortals())
            {
                foreach (ModuleInfo module in DotNetNuke.Entities.Modules.ModuleController.Instance.GetModules(portal.PortalId))
                {
                    if (module.DesktopModule.ModuleName.Trim().ToLowerInvariant().Equals(Globals.ModuleName.ToLowerInvariant()))
                    {
                        DotNetNuke.Entities.Modules.ModuleController.Instance.DeleteModuleSetting(module.ModuleID, "FULLTEXT");
                    }
                }
            }
        }

        internal static void DeleteObsoleteModuleSettings_100000()
        {
            /* remove URLBASE depending on how old the install is, it might be in ModuleSettings or it might be in activeforums_Settings, or both */

            foreach (DotNetNuke.Abstractions.Portals.IPortalInfo portal in DotNetNuke.Entities.Portals.PortalController.Instance.GetPortals())
            {
                foreach (ModuleInfo module in DotNetNuke.Entities.Modules.ModuleController.Instance.GetModules(portal.PortalId))
                {
                    if (module.DesktopModule.ModuleName.Trim().ToLowerInvariant().Equals(Globals.ModuleName.ToLowerInvariant()))
                    {
                        DotNetNuke.Entities.Modules.ModuleController.Instance.DeleteModuleSetting(module.ModuleID, "URLBASE");
                        foreach (var settingName in new string[]
                        {
                            "URLBASE",
                        })
                        {
                            DotNetNuke.Data.DataContext.Instance().Execute(System.Data.CommandType.Text, "DELETE FROM {databaseOwner}{objectQualifier}activeforums_Settings WHERE ModuleId = @0 AND SettingName = @1", module.ModuleID, settingName);
                        }
                    }
                }
            }

            /* remove PMTABID and update PMTYPE from 2 to 1 if needed */
            foreach (DotNetNuke.Abstractions.Portals.IPortalInfo portal in DotNetNuke.Entities.Portals.PortalController.Instance.GetPortals())
            {
                foreach (ModuleInfo module in DotNetNuke.Entities.Modules.ModuleController.Instance.GetModules(portal.PortalId))
                {
                    if (module.DesktopModule.ModuleName.Trim().ToLowerInvariant().Equals(Globals.ModuleName.ToLowerInvariant()))
                    {
                        DotNetNuke.Entities.Modules.ModuleController.Instance.DeleteModuleSetting(module.ModuleID, "PMTABID");
                        DotNetNuke.Data.DataContext.Instance().Execute(System.Data.CommandType.Text, "UPDATE {databaseOwner}{objectQualifier}ModuleSettings SET SettingValue = 1 WHERE ModuleId = @0 AND SettingName = 'PMTYPE' AND SettingValue = 2", module.ModuleID);
                    }
                }
            }
        }

        internal static void Remove_TemplatesTable_100000()
        {
            try
            {
                var log = new DotNetNuke.Services.Log.EventLog.LogInfo { LogTypeKey = DotNetNuke.Abstractions.Logging.EventLogType.HOST_ALERT.ToString() };
                log.LogProperties.Add(new LogDetailInfo("Module", Globals.ModuleFriendlyName));
                var message = $"dropping table activeforums_Templates table";
                log.AddProperty("Message", message);
                DotNetNuke.Services.Log.EventLog.LogController.Instance.AddLog(log);

                DotNetNuke.Data.DataContext.Instance().Execute(System.Data.CommandType.Text, "IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'{databaseOwner}[{objectQualifier}activeforums_Templates]') AND type in (N'U')) DROP TABLE {databaseOwner}[{objectQualifier}activeforums_Templates]");
            }
            catch (Exception ex)
            {
                DotNetNuke.Modules.ActiveForums.Exceptions.LogException(ex);
            }
        }

        internal static void Upgrade_PermissionSets_100000()
        {
            foreach (var perms in DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.Instance.Get())
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
                perms.Mention = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIds(DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIdsFromPermSet(string.IsNullOrEmpty(perms.Mention) ? string.Empty : perms.Mention.Replace(":", ";")));
                DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.Instance.Update(perms);
            }
        }
    }
}
