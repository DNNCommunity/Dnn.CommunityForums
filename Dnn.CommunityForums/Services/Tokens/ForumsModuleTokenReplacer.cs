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

namespace DotNetNuke.Modules.ActiveForums.Services.Tokens
{
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Tokens;

    internal class ForumsModuleTokenReplacer : BaseCustomTokenReplace, DotNetNuke.Services.Tokens.IPropertyAccess
    {
        public int ForumTabId { get; set; }

        public int TabId  { get; set; }

        public int ForumModuleId { get; set; }

        public int ModuleId { get; set; }

        private const string PropertySource_resx = "resx";
        private const string PropertySource_dcf = "dcf";
        private const string PropertySource_tab = "tab";
        private const string PropertySource_portal = "portal";
        private const string PropertySource_host = "host";
        private PortalSettings portalSettings;

        public ForumsModuleTokenReplacer(PortalSettings portalSettings, int forumTabId, int forumModuleId, int tabId, int moduleId)
        {
            this.PropertySource[PropertySource_resx] = new ResourceStringTokenReplacer();
            this.PropertySource[PropertySource_dcf] = this;
            this.PropertySource[PropertySource_tab] = portalSettings.ActiveTab;
            this.PropertySource[PropertySource_portal] = portalSettings;
            this.PropertySource[PropertySource_host] = new HostPropertyAccess();
            this.CurrentAccessLevel = Scope.DefaultSettings;
            this.PortalSettings = portalSettings;
            this.ModuleId = moduleId;
            this.ForumModuleId = forumModuleId;
            this.TabId = tabId;
            this.ForumTabId = forumTabId;
            this.CurrentAccessLevel = Scope.DefaultSettings;
        }

        public PortalSettings PortalSettings
        {
            get => this.portalSettings;
            set => this.portalSettings = value;
        }

        public new string ReplaceTokens(string source)
        {
            return base.ReplaceTokens(source);
        }

        public CacheLevel Cacheability
        {
            get
            {
                return CacheLevel.fullyCacheable;
            }
        }

        public string GetProperty(string propertyName, string format, System.Globalization.CultureInfo formatProvider, DotNetNuke.Entities.Users.UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound)
        {
            // replace any embedded tokens in format string
            if (format.Contains("["))
            {
                var tokenReplacer = new DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer(this.PortalSettings, new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ModuleId).GetByUserId(accessingUser.PortalID, accessingUser.UserID))
                {
                    AccessingUser = accessingUser,
                };
                format = tokenReplacer.ReplaceEmbeddedTokens(format);
            }

            int length = -1;
            if (propertyName.Contains(":"))
            {
                var splitPropertyName = propertyName.Split(':');
                propertyName = splitPropertyName[0];
                length = Utilities.SafeConvertInt(splitPropertyName[1], -1);
            }

            propertyName = propertyName.ToLowerInvariant();
            switch (propertyName)
            {
                case "toolbar-forums-onclick":
                    return PropertyAccess.FormatString(Utilities.NavigateURL(this.TabId), format);
                case "toolbar-controlpanel-onclick":
                    return accessingUser.IsSuperUser || accessingUser.IsAdmin ?
                        PropertyAccess.FormatString(Utilities.NavigateURL(this.ForumTabId, "EDIT", $"mid={this.ForumModuleId}"), format) :
                        string.Empty;
                case "toolbar-moderate-onclick":
                    return accessingUser.IsSuperUser || accessingUser.IsAdmin || new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ModuleId).GetByUserId(portalId: accessingUser.PortalID, userId: accessingUser.UserID).GetIsMod(this.ModuleId) ?
                        PropertyAccess.FormatString(Utilities.NavigateURL(this.TabId, string.Empty, $"{ParamKeys.ViewType}={Views.ModerateTopics}"), format) :
                        string.Empty;
                case "toolbar-notread-onclick":
                    return accessingUser.UserID >= 0 ?
                        PropertyAccess.FormatString(
                        new ControlUtils().BuildUrl(this.PortalSettings.PortalId, this.TabId, this.ModuleId, string.Empty, string.Empty, -1, -1, -1, -1, GridTypes.NotRead, 1, -1, -1),
                        format) :
                        string.Empty;
                case "toolbar-mytopics-onclick":
                    return accessingUser.UserID >= 0 ?
                        PropertyAccess.FormatString(
                            new ControlUtils().BuildUrl(this.PortalSettings.PortalId, this.TabId, this.ModuleId, string.Empty, string.Empty, -1, -1, -1, -1, GridTypes.MyTopics, 1, -1, -1),
                            format) :
                        string.Empty;
                case "toolbar-mysettings-onclick":
                    return accessingUser.UserID >= 0 ?
                        PropertyAccess.FormatString(
                        new ControlUtils().BuildUrl(this.PortalSettings.PortalId, this.TabId, this.ModuleId, string.Empty, string.Empty, -1, -1, -1, -1, GridTypes.MySettings, 1, -1, -1),
                        format) :
                        string.Empty;
                case "toolbar-mysubscriptions-onclick":
                    return accessingUser.UserID >= 0 ?
                        PropertyAccess.FormatString(
                        new ControlUtils().BuildUrl(this.PortalSettings.PortalId, this.TabId, this.ModuleId, string.Empty, string.Empty, -1, -1, -1, -1, GridTypes.MySubscriptions, 1, -1, -1),
                        format) :
                        string.Empty;
                case "toolbar-unanswered-onclick":
                    return PropertyAccess.FormatString(new ControlUtils().BuildUrl(this.PortalSettings.PortalId, this.TabId, this.ModuleId, string.Empty, string.Empty, -1, -1, -1, -1, GridTypes.Unanswered, 1, -1, -1), format);
                case "toolbar-unresolved-onclick":
                    return PropertyAccess.FormatString(new ControlUtils().BuildUrl(this.PortalSettings.PortalId, this.TabId, this.ModuleId, string.Empty, string.Empty, -1, -1, -1, -1, GridTypes.Unresolved, 1, -1, -1), format);
                case "toolbar-announcements-onclick":
                    return PropertyAccess.FormatString(new ControlUtils().BuildUrl(this.PortalSettings.PortalId, this.TabId, this.ModuleId, string.Empty, string.Empty, -1, -1, -1, -1, GridTypes.Announcements, 1, -1, -1), format);
                case "toolbar-activetopics-onclick":
                    return PropertyAccess.FormatString(new ControlUtils().BuildUrl(this.PortalSettings.PortalId, this.TabId, this.ModuleId, string.Empty, string.Empty, -1, -1, -1, -1, GridTypes.ActiveTopics, 1, -1, -1), format);
                case "toolbar-mostliked-onclick":
                    return PropertyAccess.FormatString(new ControlUtils().BuildUrl(this.PortalSettings.PortalId, this.TabId, this.ModuleId, string.Empty, string.Empty, -1, -1, -1, -1, GridTypes.MostLiked, 1, -1, -1), format);
                case "toolbar-mostreplies-onclick":
                    return PropertyAccess.FormatString(new ControlUtils().BuildUrl(this.PortalSettings.PortalId, this.TabId, this.ModuleId, string.Empty, string.Empty, -1, -1, -1, -1, GridTypes.MostReplies, 1, -1, -1), format);
            }

            propertyNotFound = true;
            return string.Empty;
        }
    }
}
