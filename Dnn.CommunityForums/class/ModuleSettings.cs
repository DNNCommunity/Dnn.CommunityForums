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
    using System.Collections;
    using System.Reflection;

    using DotNetNuke.Modules.ActiveForums.Entities;

    public class ModuleSettings
    {
        private DotNetNuke.Modules.ActiveForums.Entities.FeatureSettings defaultFeatureSettings;

        public int ModuleId { get; set; }

        public Hashtable MainSettings { get; set; }

        public ModuleSettings()
        {
            this.MainSettings = new Hashtable();
        }

        public ModuleSettings(int moduleId)
        {
            this.MainSettings = new Hashtable();
            this.ModuleId = moduleId;
        }

        public int PageSize => this.MainSettings.GetInt(SettingKeys.PageSize, 20);

        public string UserNameDisplay => this.MainSettings.GetString(SettingKeys.UserNameDisplay, "USERNAME");

        public bool EnablePoints => this.MainSettings.GetBoolean(SettingKeys.EnablePoints);

        public int TopicPointValue => this.MainSettings.GetInt(SettingKeys.TopicPointValue, 1);

        public int ReplyPointValue => this.MainSettings.GetInt(SettingKeys.ReplyPointValue, 1);

        public int AnswerPointValue => this.MainSettings.GetInt(SettingKeys.AnswerPointValue, 1);

        public int MarkAsAnswerPointValue => this.MainSettings.GetInt(SettingKeys.MarkAnswerPointValue, 1);

        public int ModPointValue => this.MainSettings.GetInt(SettingKeys.ModPointValue, 1);

        public int AvatarHeight => this.MainSettings.GetInt(SettingKeys.AvatarHeight, 80);

        public int AvatarWidth => this.MainSettings.GetInt(SettingKeys.AvatarWidth, 80);

        public bool AvatarRefreshEnabled => this.MainSettings.GetString(SettingKeys.AvatarRefresh) == Globals.AvatarRefreshGravatar;

        public string AvatarRefreshType => this.MainSettings.GetString(SettingKeys.AvatarRefresh);

        public int AllowSignatures => this.MainSettings.GetInt(SettingKeys.AllowSignatures);

        public string DateFormatString => this.MainSettings.GetString(SettingKeys.DateFormatString, "M/d/yyyy");

        public string TimeFormatString => this.MainSettings.GetString(SettingKeys.TimeFormatString, "h:mm tt");

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public int TimeZoneOffset => this.MainSettings.GetInt(SettingKeys.TimeZoneOffset);

        public bool UsersOnlineEnabled => this.MainSettings.GetBoolean(SettingKeys.UsersOnlineEnabled);

        public string MemberListMode => "Enabled";

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public int ForumTemplateID => throw new NotImplementedException();

        public DateTime InstallDate => Utilities.SafeConvertDateTime(this.MainSettings[SettingKeys.InstallDate], Utilities.NullDate());

        public bool IsInstalled => this.MainSettings.GetBoolean(SettingKeys.IsInstalled);

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

        public int PMTabId => this.MainSettings.GetInt(SettingKeys.PMTabId, -1);

        public bool DisableAccountTab => this.MainSettings.GetBoolean(SettingKeys.DisableAccountTab);

        public string Theme
        {
            get
            {
                var result = this.MainSettings.GetString(SettingKeys.Theme);
                return string.IsNullOrWhiteSpace(result) ? "_legacy" : result;
            }
        }

        public string ThemeLocation => string.Concat(Globals.ThemesPath, this.Theme, "/");

        public string TemplatePath => string.Concat(this.ThemeLocation, "templates/");

        public bool FullText => this.MainSettings.GetBoolean(SettingKeys.FullText);

        public string AllowSubTypes => this.MainSettings.GetString(SettingKeys.AllowSubTypes, string.Empty);

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public bool MailQueue => true;

        public bool CacheTemplates => this.MainSettings.GetBoolean(SettingKeys.CacheTemplates, defaultValue: true);

        public int FloodInterval => this.MainSettings.GetInt(SettingKeys.FloodInterval, 5);

        public int EditInterval => this.MainSettings.GetInt(SettingKeys.EditInterval);

        public DotNetNuke.Modules.ActiveForums.Enums.DeleteBehavior DeleteBehavior
        {
            get
            {
                DotNetNuke.Modules.ActiveForums.Enums.DeleteBehavior parsedValue;

                return Enum.TryParse(this.MainSettings.GetString(SettingKeys.DeleteBehavior), true, out parsedValue)
                    ? parsedValue
                    : DotNetNuke.Modules.ActiveForums.Enums.DeleteBehavior.Recycle;
            }
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

        public bool UseShortUrls => this.MainSettings.GetBoolean(SettingKeys.UseShortUrls);

        public bool UseSkinBreadCrumb => this.MainSettings.GetBoolean(SettingKeys.UseSkinBreadCrumb);

        public bool AutoLinkEnabled => this.MainSettings.GetBoolean(SettingKeys.EnableAutoLink, true);

        public bool URLRewriteEnabled => this.MainSettings.GetBoolean(SettingKeys.EnableURLRewriter);

        public string PrefixURLBase => this.MainSettings.GetString(SettingKeys.PrefixURLBase, string.Empty);

        public string PrefixURLOther => !this.URLRewriteEnabled
                           ? string.Empty
                           : this.MainSettings.GetString(SettingKeys.PrefixURLOther, "other");

        public string PrefixURLTag => !this.URLRewriteEnabled
                           ? string.Empty
                           : this.MainSettings.GetString(SettingKeys.PrefixURLTags, "tag");

        public string PrefixURLCategory => !this.URLRewriteEnabled
                           ? string.Empty
                           : this.MainSettings.GetString(SettingKeys.PrefixURLCategories, "category");

        public string PrefixURLLikes => !this.URLRewriteEnabled
                    ? string.Empty
                    : this.MainSettings.GetString(SettingKeys.PrefixURLLikes, Views.Likes);

        public int DefaultPermissionId => this.MainSettings.GetInt(SettingKeys.DefaultPermissionId);

        public string DefaultSettingsKey => this.MainSettings.GetString(SettingKeys.DefaultSettingsKey) ?? $"M{this.ModuleId}";

        public FeatureSettings DefaultFeatureSettings
        {
            get => this.defaultFeatureSettings ?? (this.defaultFeatureSettings = new DotNetNuke.Modules.ActiveForums.Entities.FeatureSettings(moduleId: this.ModuleId, settingsKey: this.DefaultSettingsKey));
            set => this.defaultFeatureSettings = value;
        }

        public bool ModeIsStandard => this.MainSettings.GetString(SettingKeys.Mode, ModuleModes.Standard).Equals(ModuleModes.Standard, StringComparison.InvariantCultureIgnoreCase);

        public bool ModeIsSocial => this.MainSettings.GetString(SettingKeys.Mode, ModuleModes.Standard).Equals(ModuleModes.SocialGroup, StringComparison.InvariantCultureIgnoreCase);
    }

    public class SettingsInfo
    {
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public static Hashtable GeneralSettings(int moduleId, string groupKey) => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public static string GetSetting(int moduleId, string groupKey, string settingName) => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public static bool SaveSetting(int moduleId, string settingKey, string settingName, string settingValue) => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public static bool DeleteSetting(int moduleId, string groupKey, string settingName) => throw new NotImplementedException();
    }
}
