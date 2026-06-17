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
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Framework;
    using DotNetNuke.Modules.ActiveForums.Services.Cache;

    public class SettingsBase : PortalModuleBase
    {
        private int forumModuleId = -1;

        internal DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo ForumUser => DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.Instance.GetByUserId(this.PortalId, this.ModuleId, this.UserId);

        internal HashSet<int> UserForumsList => DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Instance.GetForumsForUser(this.ForumModuleId, this.ForumUser);

        public int ForumModuleId
        {
            get
            {
                return this.forumModuleId > 0
                    ? this.forumModuleId
                    : DotNetNuke.Modules.ActiveForums.Utilities.GetForumModuleId(this.ModuleContext.Configuration);
            }

            set
            {
                this.forumModuleId = value;
            }
        }

        public int ForumTabId { get; set; } = -1;

        public string Params { get; set; } = string.Empty;

        public int PageId
        {
            get
            {
                int tempPageId = 0;
                if (this.Request.QueryString[ParamKeys.PageId] != null)
                {
                    if (Utilities.IsNumeric(this.Request.QueryString[ParamKeys.PageId]))
                    {
                        tempPageId = Convert.ToInt32(this.Request.QueryString[ParamKeys.PageId]);
                    }
                }
                else if (this.Request.QueryString[Literals.Page] != null)
                {
                    if (Utilities.IsNumeric(this.Request.QueryString[Literals.Page]))
                    {
                        tempPageId = Convert.ToInt32(this.Request.QueryString[Literals.Page]);
                    }
                }
                else if (this.Params != string.Empty && this.Params.Contains(Literals.PageId))
                {
                    tempPageId = Convert.ToInt32(this.Params.Split('=')[1]);
                }
                else
                {
                    tempPageId = 1;
                }

                return tempPageId;
            }
        }

        public bool ShowToolbar { get; set; } = true;

        public int UserDefaultPageSize => this.UserId != -1 ? this.ForumUser.PrefPageSize : this.ModuleSettings.PageSize;

        public bool UserPrefTopicSubscribe => this.UserId != -1 ? this.ForumUser.PrefTopicSubscribe : false;

        public Framework.CDefault BasePage => (Framework.CDefault)this.Page;

        public static DotNetNuke.Modules.ActiveForums.ModuleSettings GetTabModuleSettings(int moduleId, int tabModuleId)
        {
            var cacheKey = string.Format(CacheKeys.TabModuleSettings, tabModuleId);
            var tabModuleSettings = DotNetNuke.Modules.ActiveForums.Services.Cache.SettingsCache.Retrieve(tabModuleId, cacheKey) as DotNetNuke.Modules.ActiveForums.ModuleSettings;
            if (tabModuleSettings == null)
            {
                var moduleSettings = GetModuleSettings(moduleId);

                // Overlay TabModuleSettings on top (tab/instance-specific settings take precedence)
                var tabModuleSettingsToOverlay = DotNetNuke.Entities.Modules.ModuleController.Instance.GetTabModule(tabModuleId).TabModuleSettings;
                tabModuleSettings = moduleSettings.CreateMergedTabModuleSettings(tabModuleSettingsToOverlay);
                DotNetNuke.Modules.ActiveForums.Services.Cache.SettingsCache.Store(tabModuleId, cacheKey, tabModuleSettings);
            }

            return tabModuleSettings;
        }

        public static DotNetNuke.Modules.ActiveForums.ModuleSettings GetModuleSettings(int moduleId)
        {
            var cacheKey = string.Format(CacheKeys.MainSettings, moduleId);
            var settings = DotNetNuke.Modules.ActiveForums.Services.Cache.SettingsCache.Retrieve(moduleId, cacheKey) as ModuleSettings;
            if (settings == null && moduleId > 0)
            {
                settings = new DotNetNuke.Modules.ActiveForums.ModuleSettings { ModuleId = moduleId, MainSettings = new DotNetNuke.Entities.Modules.ModuleController().GetModule(moduleId).ModuleSettings };
                DotNetNuke.Modules.ActiveForums.Services.Cache.SettingsCache.Store(moduleId, cacheKey, settings);
            }

            return settings;
        }

        public DotNetNuke.Modules.ActiveForums.ModuleSettings ModuleSettings
        {
            get
            {
                this.ForumModuleId = this.forumModuleId <= 0 ? this.ForumModuleId : this.forumModuleId;
                return GetTabModuleSettings(this.ForumModuleId, this.TabModuleId);
            }
        }

        public string ImagePath => this.Page.ResolveUrl(string.Concat(this.ModuleSettings.ThemeLocation, "/images"));

        public string GetViewType
        {
            get
            {
                if (this.Request.Params[ParamKeys.ViewType] != null)
                {
                    return this.Request.Params[ParamKeys.ViewType].ToUpperInvariant();
                }

                return this.Request.Params["view"] != null ? this.Request.Params["view"].ToUpperInvariant() : null;
            }
        }

        // Forums stores datetime in UTC, so this method returns timezoneoffset for current user if available or from portal settings as fallback
        public TimeSpan TimeZoneOffset => Utilities.GetTimeZoneOffsetForUser(this.UserInfo);

        public string NavigateUrl(int tabId) => Utilities.NavigateURL(tabId);

        public string NavigateUrl(int tabId, string controlKey, params string[] additionalParameters) => Utilities.NavigateURL(tabId, controlKey, additionalParameters);

        public void RenderMessage(string title, string message) => this.RenderMessage(Utilities.GetSharedResource(title), message, string.Empty, null);

        public void RenderMessage(string message, string errorMsg, Exception ex) => this.RenderMessage(Utilities.GetSharedResource("[RESX:Error]"), message, errorMsg, ex);

        public void RenderMessage(string title, string message, string errorMsg, Exception ex)
        {
            var im = new Controls.InfoMessage { Message = string.Concat(Utilities.GetSharedResource(message), "<br />") };
            if (ex != null)
            {
                im.Message = im.Message + ex.Message;
            }

            if (ex != null)
            {
                DotNetNuke.Services.Exceptions.Exceptions.ProcessModuleLoadException(this, ex);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (this.Request.Params[Literals.View] != null)
            {
                string sUrl;
                string sParams = string.Empty;

                if (this.Request.Params[Literals.ForumId] != null)
                {
                    if (Utilities.IsNumeric(this.Request.Params[Literals.ForumId]))
                    {
                        sParams = $"{ParamKeys.ForumId}={this.Request.Params[Literals.ForumId]}";
                    }
                }

                if (this.Request.Params[Literals.PostId] != null)
                {
                    if (Utilities.IsNumeric(this.Request.Params[Literals.PostId]))
                    {
                        sParams += $"|{ParamKeys.TopicId}={this.Request.Params[Literals.PostId]}";
                    }
                }

                sParams += $"|{ParamKeys.ViewType}={this.Request.Params[Literals.View]}";
                sUrl = this.NavigateUrl(this.TabId, string.Empty, sParams.Split('|'));

                this.Response.Status = "301 Moved Permanently";
                this.Response.AddHeader("Location", sUrl);
            }

            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
        }
    }
}
