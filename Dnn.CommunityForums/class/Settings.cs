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

namespace DotNetNuke.Modules.ActiveForums
{
    using System;
    using System.Collections;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Modules.ActiveForums.Entities;
    using Newtonsoft.Json.Linq;

    #region SettingsInfo

    public class SettingsInfo
    {
        public int ModuleId { get; set; }

        private DotNetNuke.Modules.ActiveForums.Entities.FeatureSettings featureSettings;

        public Hashtable MainSettings { get; set; }

        public SettingsInfo()
        {
            this.MainSettings = new Hashtable();
        }

        public SettingsInfo(int moduleId)
        {
            this.MainSettings = new Hashtable();
            this.ModuleId = moduleId;
        }

        public int PageSize
        {
            get { return this.MainSettings.GetInt(SettingKeys.PageSize, 20); }
        }

        public string UserNameDisplay
        {
            get { return this.MainSettings.GetString(SettingKeys.UserNameDisplay, "USERNAME"); }
        }

        public bool EnablePoints
        {
            get { return this.MainSettings.GetBoolean(SettingKeys.EnablePoints); }
        }

        public int TopicPointValue
        {
            get { return this.MainSettings.GetInt(SettingKeys.TopicPointValue, 1); }
        }

        public int ReplyPointValue
        {
            get { return this.MainSettings.GetInt(SettingKeys.ReplyPointValue, 1); }
        }

        public int AnswerPointValue
        {
            get { return this.MainSettings.GetInt(SettingKeys.AnswerPointValue, 1); }
        }

        public int MarkAsAnswerPointValue
        {
            get { return this.MainSettings.GetInt(SettingKeys.MarkAnswerPointValue, 1); }
        }

        public int ModPointValue
        {
            get { return this.MainSettings.GetInt(SettingKeys.ModPointValue, 1); }
        }

        public int AvatarHeight
        {
            get { return this.MainSettings.GetInt(SettingKeys.AvatarHeight, 80); }
        }

        public int AvatarWidth
        {
            get { return this.MainSettings.GetInt(SettingKeys.AvatarWidth, 80); }
        }

        public int AllowSignatures
        {
            get { return this.MainSettings.GetInt(SettingKeys.AllowSignatures); }
        }

        public string DateFormatString
        {
            get { return this.MainSettings.GetString(SettingKeys.DateFormatString, "M/d/yyyy"); }
        }

        public string TimeFormatString
        {
            get { return this.MainSettings.GetString(SettingKeys.TimeFormatString, "h:mm tt"); }
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public int TimeZoneOffset
        {
            get { return this.MainSettings.GetInt(SettingKeys.TimeZoneOffset); }
        }

        public bool UsersOnlineEnabled
        {
            get { return this.MainSettings.GetBoolean(SettingKeys.UsersOnlineEnabled); }
        }

        public string MemberListMode
        {
            get
            {
                return "Enabled";
            }
        }

        public int ForumTemplateID
        {
            get { return this.MainSettings.GetInt(SettingKeys.ForumTemplateId); }
        }

        public DateTime InstallDate
        {
            get { return Utilities.SafeConvertDateTime(this.MainSettings[SettingKeys.InstallDate], Utilities.NullDate()); }
        }

        public bool IsInstalled
        {
            get { return this.MainSettings.GetBoolean(SettingKeys.IsInstalled); }
        }

        public PMTypes PMType
        {
            get
            {
                PMTypes parsedValue;
                return Enum.TryParse(this.MainSettings.GetString(SettingKeys.PMType), true, out parsedValue)
                           ? parsedValue
                           : PMTypes.Disabled;
            }
        }

        public int PMTabId
        {
            get { return this.MainSettings.GetInt(SettingKeys.PMTabId, -1); }
        }

        public bool DisableAccountTab
        {
            get { return this.MainSettings.GetBoolean(SettingKeys.DisableAccountTab); }
        }

        public string Theme
        {
            get
            {
                var result = this.MainSettings.GetString(SettingKeys.Theme);
                return string.IsNullOrWhiteSpace(result) ? "_legacy" : result;
            }
        }

        public string ThemeLocation =>  string.Concat(Globals.ThemesPath, this.Theme, "/");

        public string TemplatePath => string.Concat(this.ThemeLocation, "templates/");

        public bool FullText
        {
            get { return this.MainSettings.GetBoolean(SettingKeys.FullText); }
        }

        public string AllowSubTypes
        {
            get { return this.MainSettings.GetString(SettingKeys.AllowSubTypes, string.Empty); }
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public bool MailQueue => true;

        public bool CacheTemplates
        {
            get { return this.MainSettings.GetBoolean(SettingKeys.CacheTemplates, defaultValue: true); }
        }

        public int FloodInterval
        {
            get { return this.MainSettings.GetInt(SettingKeys.FloodInterval, 5); }
        }

        public int EditInterval
        {
            get { return this.MainSettings.GetInt(SettingKeys.EditInterval); }
        }

        public int DeleteBehavior
        {
            get { return this.MainSettings.GetInt(SettingKeys.DeleteBehavior); }
        }

        public ProfileVisibilities ProfileVisibility
        {
            get
            {
                ProfileVisibilities parsedValue;

                return Enum.TryParse(this.MainSettings.GetString(SettingKeys.ProfileVisibility), true,
                                     out parsedValue)
                           ? parsedValue
                           : ProfileVisibilities.Disabled;
            }
        }

        public bool UseShortUrls
        {
            get { return this.MainSettings.GetBoolean(SettingKeys.UseShortUrls); }
        }

        public bool UseSkinBreadCrumb
        {
            get { return this.MainSettings.GetBoolean(SettingKeys.UseSkinBreadCrumb); }
        }

        public bool AutoLinkEnabled
        {
            get { return this.MainSettings.GetBoolean(SettingKeys.EnableAutoLink, true); }
        }

        public bool URLRewriteEnabled
        {
            get { return this.MainSettings.GetBoolean(SettingKeys.EnableURLRewriter); }
        }

        public string PrefixURLBase
        {
            get { return this.MainSettings.GetString(SettingKeys.PrefixURLBase, string.Empty); }
        }

        public string PrefixURLOther
        {
            get
            {
                return !this.URLRewriteEnabled
                           ? string.Empty
                           : this.MainSettings.GetString(SettingKeys.PrefixURLOther, "other");
            }
        }

        public string PrefixURLTag
        {
            get
            {
                return !this.URLRewriteEnabled
                           ? string.Empty
                           : this.MainSettings.GetString(SettingKeys.PrefixURLTags, "tag");
            }
        }

        public string PrefixURLCategory
        {
            get
            {
                return !this.URLRewriteEnabled
                           ? string.Empty
                           : this.MainSettings.GetString(SettingKeys.PrefixURLCategories, "category");
            }
        }

        public string PrefixURLLikes
        {
            get
            {
                return !this.URLRewriteEnabled
                    ? string.Empty
                    : this.MainSettings.GetString(SettingKeys.PrefixURLLikes, Views.Likes);
            }
        }

        public int DefaultPermissionId
        {
            get { return this.MainSettings.GetInt(SettingKeys.DefaultPermissionId); }
        }

        public string DefaultSettingsKey
        {
            get { return this.MainSettings.GetString(SettingKeys.DefaultSettingsKey); }
        }

        public FeatureSettings ForumFeatureSettings
        {
            get => this.featureSettings ?? (this.featureSettings = this.LoadFeatureSettings());
            set => this.featureSettings = value;
        }

        internal FeatureSettings LoadFeatureSettings()
        {
            return new DotNetNuke.Modules.ActiveForums.Entities.FeatureSettings(moduleId: this.ModuleId, settingsKey: this.DefaultSettingsKey);
        }
    }

    #endregion

    public class Settings
    {
        public static Hashtable GeneralSettings(int moduleId, string groupKey)
        {
            var ht = new Hashtable();
            var dr = DataProvider.Instance().Settings_List(moduleId, groupKey);
            while (dr.Read())
            {
                ht.Add(dr.GetString("SettingName"), dr.GetString("SettingValue"));
            }

            dr.Close();
            return ht;
        }

        public static string GetSetting(int moduleId, string groupKey, string settingName)
        {
            try
            {
                return DataProvider.Instance().Settings_Get(moduleId, groupKey, settingName);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static bool SaveSetting(int moduleId, string groupKey, string settingName, string settingValue)
        {
            try
            {
                DataProvider.Instance().Settings_Save(moduleId, groupKey, settingName, settingValue);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
