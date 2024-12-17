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

namespace DotNetNuke.Modules.ActiveForums.Entities
{
    using System;

    using DotNetNuke.ComponentModel.DataAnnotations;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Services.Tokens;

    [TableName("activeforums_Groups")]
    [PrimaryKey("ForumGroupId", AutoIncrement = true)]
    [Scope("ModuleId")]

    // TODO [Cacheable("activeforums_Groups", CacheItemPriority.Low)] /* TODO: DAL2 caching cannot be used until all CRUD methods use DAL2; must update Save method to use DAL2 rather than stored procedure */
    public partial class ForumGroupInfo : DotNetNuke.Services.Tokens.IPropertyAccess
    {
        private DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo security;
        private FeatureSettings featureSettings;
        private DotNetNuke.Modules.ActiveForums.SettingsInfo mainSettings;
        private PortalSettings portalSettings;
        private ModuleInfo moduleInfo;

        public int ForumGroupId { get; set; }

        public int ModuleId { get; set; }

        public string GroupName { get; set; }

        public int SortOrder { get; set; }

        public bool Active { get; set; }

        public bool Hidden { get; set; }

        public string GroupSettingsKey { get; set; } = string.Empty;

        public int PermissionsId { get; set; } = -1;

        public string PrefixURL { get; set; } = string.Empty;

        [IgnoreColumn]
        public Uri RequestUri { get; set; }

        [IgnoreColumn]
        public string RawUrl { get; set; }

        [IgnoreColumn]
        public int TabId => this.ModuleInfo.TabID;

        [IgnoreColumn]
        public string ThemeLocation => Utilities.ResolveUrl(SettingsBase.GetModuleSettings(this.ModuleId).ThemeLocation);

        [IgnoreColumn]
        public bool InheritSecurity => this.PermissionsId == this.MainSettings.DefaultPermissionId;

        [IgnoreColumn]
        public bool InheritSettings => this.GroupSettingsKey == this.MainSettings.DefaultSettingsKey;

        [IgnoreColumn]
        public DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo Security
        {
            get
            {
                if (this.security == null)
                {
                    this.security = this.LoadSecurity();
                    this.UpdateCache();
                }

                return this.security;
            }

            set
            {
                this.security = value;
                this.UpdateCache();
            }
        }

        internal DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo LoadSecurity()
        {
            var security = new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController().GetById(this.PermissionsId, this.ModuleId);
            if (security == null)
            {
                security = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetEmptyPermissions(this.ModuleId);
                var log = new DotNetNuke.Services.Log.EventLog.LogInfo { LogTypeKey = DotNetNuke.Abstractions.Logging.EventLogType.ADMIN_ALERT.ToString() };
                log.LogProperties.Add(new LogDetailInfo("Module", Globals.ModuleFriendlyName));
                string message = string.Format(Utilities.GetSharedResource("[RESX:PermissionsMissingForForumGroup]"), this.PermissionsId, this.ForumGroupId);
                log.AddProperty("Message", message);
                DotNetNuke.Services.Log.EventLog.LogController.Instance.AddLog(log);
            }

            return this.security = security;
        }

        [IgnoreColumn]
        public FeatureSettings FeatureSettings
        {
            get
            {
                if (this.featureSettings == null)
                {
                    this.featureSettings = this.LoadFeatureSettings();
                    this.UpdateCache();
                }

                return this.featureSettings;
            }

            set
            {
                this.featureSettings = value;
                this.UpdateCache();
            }
        }

        internal FeatureSettings LoadFeatureSettings()
        {
            return this.featureSettings = new DotNetNuke.Modules.ActiveForums.Entities.FeatureSettings(moduleId: this.ModuleId, settingsKey: this.GroupSettingsKey);
        }

        [IgnoreColumn]
        public SettingsInfo MainSettings
        {
            get
            {
                if (this.mainSettings == null)
                {
                    this.mainSettings = this.LoadMainSettings();
                    this.UpdateCache();
                }

                return this.mainSettings;
            }

            set
            {
                this.mainSettings = value;
                this.UpdateCache();
            }
        }

        internal SettingsInfo LoadMainSettings()
        {
            return this.mainSettings = SettingsBase.GetModuleSettings(this.ModuleId);
        }

        [IgnoreColumn]
        public PortalSettings PortalSettings
        {
            get
            {
                if (this.portalSettings == null)
                {
                    this.portalSettings = this.LoadPortalSettings();
                    this.UpdateCache();
                }

                return this.portalSettings;
            }

            set
            {
                this.portalSettings = value;
                this.UpdateCache();
            }
        }

        internal PortalSettings LoadPortalSettings()
        {
            return this.portalSettings = Utilities.GetPortalSettings(this.ModuleInfo.PortalID);
        }

        [IgnoreColumn]
        public ModuleInfo ModuleInfo
        {
            get
            {
                if (this.moduleInfo == null)
                {
                    this.moduleInfo = this.LoadModuleInfo();
                    this.UpdateCache();
                }

                return this.moduleInfo;
            }

            set
            {
                this.moduleInfo = value;
                this.UpdateCache();
            }
        }

        internal ModuleInfo LoadModuleInfo()
        {
            return this.moduleInfo = DotNetNuke.Entities.Modules.ModuleController.Instance.GetModule(this.ModuleId, DotNetNuke.Common.Utilities.Null.NullInteger, false);
        }

        [IgnoreColumn]
        public DotNetNuke.Services.Tokens.CacheLevel Cacheability
        {
            get
            {
                return DotNetNuke.Services.Tokens.CacheLevel.notCacheable;
            }
        }

        /// <inheritdoc/>
        public string GetProperty(string propertyName, string format, System.Globalization.CultureInfo formatProvider, DotNetNuke.Entities.Users.UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound)
        {
            // replace any embedded tokens in format string
            if (format.Contains("["))
            {
                var tokenReplacer = new DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer(this.PortalSettings, new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ModuleId).GetByUserId(accessingUser.PortalID, accessingUser.UserID), this, this.RequestUri, this.RawUrl)
                {
                    AccessingUser = accessingUser,
                };
                format = tokenReplacer.ReplaceEmbeddedTokens(format);
            }

            propertyName = propertyName.ToLowerInvariant();

            switch (propertyName)
            {
                case "themelocation":
                    return PropertyAccess.FormatString(this.ThemeLocation.ToString(), format);
                case "groupid":
                case "forumgroupid":
                    return PropertyAccess.FormatString(this.ForumGroupId.ToString(), format);
                case "grouplink":
                case "forumgrouplink":
                    return PropertyAccess.FormatString(new ControlUtils().BuildUrl(
                            this.PortalSettings.PortalId,
                            this.GetTabId(),
                            this.ModuleId,
                            this.PrefixURL,
                            string.Empty,
                            this.ForumGroupId,
                            -1,
                            -1,
                            -1,
                            string.Empty,
                            1,
                            -1,
                            -1),
                        format);
                case "forumgroupname":
                case "groupname":
                case "name":
                    return PropertyAccess.FormatString(this.GroupName, format);
                case "groupcollapse":
                    return PropertyAccess.FormatString(DotNetNuke.Modules.ActiveForums.Injector.InjectCollapsibleOpened(target: $"group{this.ForumGroupId}", title: Utilities.GetSharedResource("[RESX:ToggleGroup]")), format);

            }

            propertyNotFound = true;
            return string.Empty;
        }

        private int GetTabId()
        {
            return this.PortalSettings.ActiveTab.TabID == -1 || this.PortalSettings.ActiveTab.TabID == this.PortalSettings.HomeTabId ? this.TabId : this.PortalSettings.ActiveTab.TabID;
        }

        internal string GetCacheKey() => new DotNetNuke.Modules.ActiveForums.Controllers.ForumGroupController().GetCacheKey(this.ModuleId, this.ForumGroupId);

        internal void UpdateCache() => DotNetNuke.Modules.ActiveForums.DataCache.SettingsCacheStore(this.ModuleId, this.GetCacheKey(), this);
    }
}
