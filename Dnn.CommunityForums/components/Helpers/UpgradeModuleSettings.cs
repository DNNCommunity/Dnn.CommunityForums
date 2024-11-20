// Copyright (c) 2013-2024 by DNN Community
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
    using System.Reflection;
    using System.Web.UI;
    using System.Xml;
    using System.Xml.Linq;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Web.Models;

    internal static class UpgradeModuleSettings
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(UpgradeModuleSettings));

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
                            MoveSettingsForModuleInstanceToTabModuleInstance(module.ModuleID, tabModuleId: module.TabModuleID);
                        }
                    }
                }
            }
        }

        internal static void MoveSettingsForModuleInstanceToTabModuleInstance(int forumModuleId, int tabModuleId)
        {
            var currSettings = new SettingsInfo { ModuleId = forumModuleId, MainSettings = Settings.GeneralSettings(forumModuleId, "GEN") };

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
            DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(tabModuleId, SettingKeys.ForumTemplateId, currSettings.ForumTemplateID.ToString());
            DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(tabModuleId, SettingKeys.InstallDate, currSettings.InstallDate.ToString());
            DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(tabModuleId, SettingKeys.IsInstalled, currSettings.IsInstalled.ToString());
            DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(tabModuleId, SettingKeys.Theme, currSettings.Theme);
            DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(tabModuleId, SettingKeys.FullText, currSettings.FullText.ToString());
            DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(tabModuleId, SettingKeys.MailQueue, currSettings.MailQueue.ToString());
            DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(tabModuleId, SettingKeys.FloodInterval, currSettings.FloodInterval.ToString());
            DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(tabModuleId, SettingKeys.EditInterval, currSettings.EditInterval.ToString());
            DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(tabModuleId, SettingKeys.DeleteBehavior, currSettings.DeleteBehavior.ToString());
            DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(tabModuleId, SettingKeys.EnableAutoLink, currSettings.AutoLinkEnabled.ToString());
            DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(tabModuleId, SettingKeys.EnableURLRewriter, currSettings.URLRewriteEnabled.ToString());
            if (string.IsNullOrEmpty(currSettings.PrefixURLBase))
            {
                DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(tabModuleId, SettingKeys.PrefixURLBase, "forums");
            }
            else
            {
                DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(tabModuleId, SettingKeys.PrefixURLBase, currSettings.PrefixURLBase);
            }

            if (string.IsNullOrEmpty(currSettings.PrefixURLOther))
            {
                DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(tabModuleId, SettingKeys.PrefixURLOther, "views");
            }
            else
            {
                DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(tabModuleId, SettingKeys.PrefixURLOther, currSettings.PrefixURLOther);
            }

            if (string.IsNullOrEmpty(currSettings.PrefixURLTag))
            {
                DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(tabModuleId, SettingKeys.PrefixURLTags, "tag");
            }
            else
            {
                DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(tabModuleId, SettingKeys.PrefixURLTags, currSettings.PrefixURLTag);
            }

            if (string.IsNullOrEmpty(currSettings.PrefixURLCategory))
            {
                DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(tabModuleId, SettingKeys.PrefixURLCategories, "category");
            }
            else
            {
                DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(tabModuleId, SettingKeys.PrefixURLCategories, currSettings.PrefixURLCategory);
            }

            Logger.InfoFormat("Settings converted for module Id {0} tab module Id {1}", forumModuleId, tabModuleId);

            DotNetNuke.Entities.Modules.ModuleController.Instance.DeleteModuleSetting(tabModuleId, "NeedsConvert");
            DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(tabModuleId, "AFINSTALLED", "True");
            DotNetNuke.Modules.ActiveForums.DataCache.SettingsCacheClear(forumModuleId, string.Format(CacheKeys.MainSettings, forumModuleId));
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
                        DotNetNuke.Entities.Modules.ModuleController.Instance.DeleteTabModuleSetting(module.TabModuleID, "ForumConfig");
                        DotNetNuke.Entities.Modules.ModuleController.Instance.DeleteTabModuleSetting(module.TabModuleID, "ForumGroupTemplate");
                        DotNetNuke.Entities.Modules.ModuleController.Instance.DeleteTabModuleSetting(module.TabModuleID, "MODE");
                        DotNetNuke.Entities.Modules.ModuleController.Instance.DeleteTabModuleSetting(module.TabModuleID, "AllowIndex");
                        DotNetNuke.Modules.ActiveForums.DataCache.ClearAllCacheForTabId(module.TabID);
                        DotNetNuke.Modules.ActiveForums.DataCache.ClearAllCache(module.ModuleID);
                        var forumConfig = module.ModuleSettings.GetString("ForumConfig", string.Empty);
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
                                DotNetNuke.Entities.Modules.ModuleController.Instance.DeleteModuleSetting(module.ModuleID, "ForumConfig");
                                DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(module.ModuleID, "ForumConfig", forumConfig);
                                DotNetNuke.Modules.ActiveForums.DataCache.SettingsCacheClear(module.ModuleID, string.Format(DataCache.ModuleSettingsCacheKey, module.TabID));
                                DotNetNuke.Modules.ActiveForums.DataCache.SettingsCacheClear(module.ModuleID, string.Format(DataCache.TabModuleSettingsCacheKey, module.TabID));
                                DotNetNuke.Modules.ActiveForums.DataCache.ClearAllCacheForTabId(module.TabID);
                                DotNetNuke.Modules.ActiveForums.DataCache.ClearAllCache(module.ModuleID);
                            }
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
                        var ForumConfig = module.ModuleSettings.GetString("ForumConfig", string.Empty);
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
                                DotNetNuke.Entities.Modules.ModuleController.Instance.DeleteModuleSetting(module.ModuleID, "ForumConfig");
                                DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(module.ModuleID, "ForumConfig", ForumConfig);
                                DotNetNuke.Modules.ActiveForums.DataCache.SettingsCacheClear(module.ModuleID, string.Format(DataCache.ModuleSettingsCacheKey, module.TabID));
                                DotNetNuke.Modules.ActiveForums.DataCache.SettingsCacheClear(module.ModuleID, string.Format(DataCache.TabModuleSettingsCacheKey, module.TabID));
                                DotNetNuke.Modules.ActiveForums.DataCache.ClearAllCacheForTabId(module.TabID);
                                DotNetNuke.Modules.ActiveForums.DataCache.ClearAllCache(module.ModuleID);
                            }
                        }
                    }
                }
            }
        }
    }
}
