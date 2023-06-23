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
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Reflection;
using System.Web.UI.WebControls;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Framework;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Log.EventLog;

namespace DotNetNuke.Modules.ActiveForums.Helpers
{
	internal static class UpgradeModuleSettings
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(UpgradeModuleSettings));
        internal static void MoveSettings() {

			/* at some point around v6, general module settings were moved from the activeforums_settings table to the DNN platform Settings table;
			 * the code that did that migration would check every time during page load (in ForumBase.OnLoad()) to see if the settings conversion was required.
			 * So code has been moved here, and is now called once during module upgrade for one version to ensure that this is done.
			 */

			foreach (PortalInfo portal in DotNetNuke.Entities.Portals.PortalController.Instance.GetPortals())
			{
				foreach (ModuleInfo module in DotNetNuke.Entities.Modules.ModuleController.Instance.GetModules(portal.PortalID))
				{
					if (module.DesktopModule.ModuleName.Trim().ToLowerInvariant() == Globals.ModuleName.ToLowerInvariant())
                    {
                        if (!SettingsBase.GetModuleSettings(module.ModuleID).IsInstalled)
						{
                            MoveSettingsForModuleInstance(module.ModuleID, tabModuleId: module.TabModuleID);
                        }                            
                    }
				}
			}
		}
		internal static void MoveSettingsForModuleInstance(int forumModuleId, int tabModuleId)
        {
            var objModules = new DotNetNuke.Entities.Modules.ModuleController();
			var currSettings = new SettingsInfo {MainSettings = Settings.GeneralSettings(forumModuleId, "GEN")};

		    objModules.UpdateModuleSetting(tabModuleId, SettingKeys.PageSize, currSettings.PageSize.ToString());
			objModules.UpdateModuleSetting(tabModuleId, SettingKeys.UserNameDisplay, currSettings.UserNameDisplay);
			objModules.UpdateModuleSetting(tabModuleId, SettingKeys.ProfileVisibility, ((int)currSettings.ProfileVisibility).ToString());
			objModules.UpdateModuleSetting(tabModuleId, SettingKeys.EnablePoints, currSettings.EnablePoints.ToString());
			objModules.UpdateModuleSetting(tabModuleId, SettingKeys.TopicPointValue, currSettings.TopicPointValue.ToString());
			objModules.UpdateModuleSetting(tabModuleId, SettingKeys.ReplyPointValue, currSettings.ReplyPointValue.ToString());
			objModules.UpdateModuleSetting(tabModuleId, SettingKeys.AnswerPointValue, currSettings.AnswerPointValue.ToString());
			objModules.UpdateModuleSetting(tabModuleId, SettingKeys.MarkAnswerPointValue, currSettings.MarkAsAnswerPointValue.ToString());
			objModules.UpdateModuleSetting(tabModuleId, SettingKeys.ModPointValue, currSettings.ModPointValue.ToString());
			objModules.UpdateModuleSetting(tabModuleId, SettingKeys.AvatarHeight, currSettings.AvatarHeight.ToString());
			objModules.UpdateModuleSetting(tabModuleId, SettingKeys.AvatarWidth, currSettings.AvatarWidth.ToString());
			objModules.UpdateModuleSetting(tabModuleId, SettingKeys.AllowSignatures, currSettings.AllowSignatures.ToString());
			objModules.UpdateModuleSetting(tabModuleId, SettingKeys.ForumTemplateId, currSettings.ForumTemplateID.ToString());
			objModules.UpdateModuleSetting(tabModuleId, SettingKeys.InstallDate, currSettings.InstallDate.ToString());
			objModules.UpdateModuleSetting(tabModuleId, SettingKeys.IsInstalled, currSettings.IsInstalled.ToString());
			objModules.UpdateModuleSetting(tabModuleId, SettingKeys.Theme, currSettings.Theme);
			objModules.UpdateModuleSetting(tabModuleId, SettingKeys.FullText, currSettings.FullText.ToString());
			objModules.UpdateModuleSetting(tabModuleId, SettingKeys.MailQueue, currSettings.MailQueue.ToString());
			objModules.UpdateModuleSetting(tabModuleId, SettingKeys.FloodInterval, currSettings.FloodInterval.ToString());
			objModules.UpdateModuleSetting(tabModuleId, SettingKeys.EditInterval, currSettings.EditInterval.ToString());
			objModules.UpdateModuleSetting(tabModuleId, SettingKeys.DeleteBehavior, currSettings.DeleteBehavior.ToString());
			objModules.UpdateModuleSetting(tabModuleId, SettingKeys.EnableAutoLink, currSettings.AutoLinkEnabled.ToString());
			objModules.UpdateModuleSetting(tabModuleId, SettingKeys.EnableURLRewriter, currSettings.URLRewriteEnabled.ToString());
			if (string.IsNullOrEmpty(currSettings.PrefixURLBase))
			{
				objModules.UpdateModuleSetting(tabModuleId, SettingKeys.PrefixURLBase, "forums");
			}
			else
			{
				objModules.UpdateModuleSetting(tabModuleId, SettingKeys.PrefixURLBase, currSettings.PrefixURLBase);
			}
			if (string.IsNullOrEmpty(currSettings.PrefixURLOther))
			{
				objModules.UpdateModuleSetting(tabModuleId, SettingKeys.PrefixURLOther, "views");
			}
			else
			{
				objModules.UpdateModuleSetting(tabModuleId, SettingKeys.PrefixURLOther, currSettings.PrefixURLOther);
			}
			if (string.IsNullOrEmpty(currSettings.PrefixURLTag))
			{
				objModules.UpdateModuleSetting(tabModuleId, SettingKeys.PrefixURLTags, "tag");
			}
			else
			{
				objModules.UpdateModuleSetting(tabModuleId, SettingKeys.PrefixURLTags, currSettings.PrefixURLTag);
			}
			if (string.IsNullOrEmpty(currSettings.PrefixURLCategory))
			{
				objModules.UpdateModuleSetting(tabModuleId, SettingKeys.PrefixURLCategories, "category");
			}
			else
			{
				objModules.UpdateModuleSetting(tabModuleId, SettingKeys.PrefixURLCategories, currSettings.PrefixURLCategory);
			}
			Logger.InfoFormat("Settings converted for module Id {0} tab module Id {1}", forumModuleId, tabModuleId);

            objModules.DeleteModuleSetting(tabModuleId, "NeedsConvert");
			objModules.UpdateModuleSetting(tabModuleId, "AFINSTALLED", "True");
			DataCache.CacheClear(string.Format(CacheKeys.MainSettings, forumModuleId));

		}
	}
}

 