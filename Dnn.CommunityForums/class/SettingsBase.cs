//
// Community Forums
// Copyright (c) 2013-2024
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

namespace DotNetNuke.Modules.ActiveForums
{
    using System;
    using System.Reflection;
    using System.Web;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework;
    using DotNetNuke.Modules.ActiveForums.Data;
    using DotNetNuke.Security.Permissions;

    public class SettingsBase : PortalModuleBase
    {
        #region Private Members
        private int forumModuleId = -1;
        private string loadView = string.Empty;
        private int loadGroupForumID = 0;
        private int loadPostID = 0;
        private string imagePath = string.Empty;
        private string @params = string.Empty;
        private int forumTabId = -1;
        #endregion

        #region Public Properties
        internal User ForumUser
        {
            get
            {
                var uc = new UserController();
                return uc.GetUser(this.PortalId, this.ForumModuleId);
            }
        }

        internal string UserForumsList
        {
            get
            {
                string forums;
                if (string.IsNullOrEmpty(this.ForumUser.UserForums))
                {
                    forums = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForumsForUser(this.ForumUser.UserRoles, this.PortalId, this.ForumModuleId);
                    this.ForumUser.UserForums = forums;
                }
                else
                {
                    forums = this.ForumUser.UserForums;
                }

                return forums;
            }
        }

        public int ForumModuleId
        {
            get
            {
                if (this.forumModuleId > 0)
                {
                    return this.forumModuleId;
                }

                return DotNetNuke.Modules.ActiveForums.Utilities.GetForumModuleId(this.ModuleId, this.TabId);
            }

            set
            {
                this.forumModuleId = value;
            }
        }

        public int ForumTabId
        {
            get
            {
                return this.forumTabId;
            }

            set
            {
                this.forumTabId = value;
            }
        }

        public string Params
        {
            get
            {
                return this.@params;
            }

            set
            {
                this.@params = value;
            }
        }

        public bool UseAjax
        {
            get
            {
                bool tempUseAjax = this.Request.IsAuthenticated && this.UserPrefUseAjax;

                return tempUseAjax;
            }
        }

        public int PageId
        {
            get
            {
                int tempPageId = 0;
                if (this.Request.QueryString[ParamKeys.PageId] != null)
                {
                    if (SimulateIsNumeric.IsNumeric(this.Request.QueryString[ParamKeys.PageId]))
                    {
                        tempPageId = Convert.ToInt32(this.Request.QueryString[ParamKeys.PageId]);
                    }
                }
                else if (this.Request.QueryString[Literals.page] != null)
                {
                    if (SimulateIsNumeric.IsNumeric(this.Request.QueryString[Literals.page]))
                    {
                        tempPageId = Convert.ToInt32(this.Request.QueryString[Literals.page]);
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
        #endregion

        public UserController UserController
        {
            get
            {
                const string userControllerContextKey = "AF|UserController";
                var userController = HttpContext.Current.Items[userControllerContextKey] as UserController;
                if (userController == null)
                {
                    userController = new UserController();
                    HttpContext.Current.Items[userControllerContextKey] = userController;
                }

                return userController;
            }
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No longer used.")]
        public ForumsDB ForumsDB
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        #region Public Properties - User Preferences
        public CurrentUserTypes CurrentUserType
        {
            get
            {
                if (this.Request.IsAuthenticated)
                {
                    if (this.UserInfo.IsSuperUser)
                    {
                        return CurrentUserTypes.SuperUser;
                    }

                    if (ModulePermissionController.HasModulePermission(this.ModuleConfiguration.ModulePermissions, "EDIT"))
                    {
                        return CurrentUserTypes.Admin;
                    }

                    if (this.ForumUser.Profile.IsMod)
                    {
                        return CurrentUserTypes.ForumMod;
                    }

                    return CurrentUserTypes.Auth;
                }

                return CurrentUserTypes.Anon;
            }
        }

        public bool UserIsMod
        {
            get
            {
                if (this.UserId == -1)
                {
                    return false;
                }

                if (this.ForumUser != null)
                {
                    return this.ForumUser.Profile.IsMod;
                }

                return false;
            }
        }

        public string UserDefaultSort
        {
            get
            {
                if (this.UserId != -1)
                {
                    return this.ForumUser.Profile.PrefDefaultSort;
                }

                return "ASC";
            }
        }

        public int UserDefaultPageSize
        {
            get
            {
                if (this.UserId != -1)
                {
                    return this.ForumUser.Profile.PrefPageSize;
                }

                return this.MainSettings.PageSize;
            }
        }

        public bool UserPrefHideSigs
        {
            get
            {
                if (this.UserId != -1)
                {
                    try
                    {
                        return this.ForumUser.Profile.PrefBlockSignatures;
                    }
                    catch (Exception ex)
                    {
                        return false;
                    }
                }

                return false;
            }
        }

        public bool UserPrefHideAvatars
        {
            get
            {
                if (this.UserId != -1)
                {
                    return this.ForumUser.Profile.PrefBlockAvatars;
                }

                return false;
            }
        }

        public bool UserPrefJumpLastPost
        {
            get
            {
                if (this.UserId != -1)
                {
                    return this.ForumUser.Profile.PrefJumpLastPost;
                }

                return false;
            }
        }

        public bool UserPrefUseAjax
        {
            get
            {
                if (this.UserId != -1)
                {
                    return this.ForumUser.Profile.PrefUseAjax;
                }

                return false;
            }
        }

        public bool UserPrefShowReplies
        {
            get
            {
                if (this.UserId != -1)
                {
                    return this.ForumUser.Profile.PrefDefaultShowReplies;
                }

                return false;
            }
        }

        public bool UserPrefTopicSubscribe
        {
            get
            {
                if (this.UserId != -1)
                {
                    return this.ForumUser.Profile.PrefTopicSubscribe;
                }

                return false;
            }
        }
        #endregion

        #region Public ReadOnly Properties
        public Framework.CDefault BasePage
        {
            get
            {
                return (Framework.CDefault)this.Page;
            }
        }

        public static SettingsInfo GetModuleSettings(int moduleId)
        {
            SettingsInfo objSettings = (SettingsInfo)DataCache.SettingsCacheRetrieve(moduleId, string.Format(CacheKeys.MainSettings, moduleId));
            if (objSettings == null && moduleId > 0)
            {
                objSettings = new SettingsInfo { MainSettings = new DotNetNuke.Entities.Modules.ModuleController().GetModule(moduleId).ModuleSettings };
                DataCache.SettingsCacheStore(moduleId, string.Format(CacheKeys.MainSettings, moduleId), objSettings);
            }

            return objSettings;

        }

        public SettingsInfo MainSettings
        {
            get
            {
                this.ForumModuleId = this.forumModuleId <= 0 ? this.ForumModuleId : this.forumModuleId;
                return GetModuleSettings(this.ForumModuleId);
            }
        }

        public string ImagePath
        {
            get
            {
                return this.Page.ResolveUrl(string.Concat(this.MainSettings.ThemeLocation, "/images"));
            }
        }

        public string GetViewType
        {
            get
            {
                if (this.Request.Params[ParamKeys.ViewType] != null)
                {
                    return this.Request.Params[ParamKeys.ViewType].ToUpperInvariant();
                }

                if (this.Request.Params["view"] != null)
                {
                    return this.Request.Params["view"].ToUpperInvariant();
                }

                return null;
            }
        }

        public TimeSpan TimeZoneOffset
        {
            /* AF now stores datetime in UTC, so this method returns timezoneoffset for current user if available or from portal settings as fallback */
            get
            {
                return Utilities.GetTimeZoneOffsetForUser(this.UserInfo);
            }
        }

        #endregion

        #region Protected Methods
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        protected DateTime GetUserDate(DateTime displayDate)
        {
            return displayDate.AddMinutes(this.TimeZoneOffset.TotalMinutes);
        }

        #endregion

        #region Public Methods
        public string NavigateUrl(int tabId)
        {
            return Utilities.NavigateURL(tabId);
        }

        public string NavigateUrl(int tabId, string controlKey, params string[] additionalParameters)
        {
            return Utilities.NavigateURL(tabId, controlKey, additionalParameters);
        }

        private string[] AddParams(string param, string[] currParams)
        {
            var tmpParams = new[] { param };
            int intLength = tmpParams.Length;
            Array.Resize(ref tmpParams, intLength + currParams.Length);
            currParams.CopyTo(tmpParams, intLength);
            return tmpParams;
        }

        public void RenderMessage(string title, string message)
        {
            this.RenderMessage(Utilities.GetSharedResource(title), message, string.Empty, null);
        }

        public void RenderMessage(string message, string errorMsg, Exception ex)
        {
            this.RenderMessage(Utilities.GetSharedResource("[RESX:Error]"), message, errorMsg, ex);
        }

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

            if (this.Request.Params[Literals.view] != null)
            {
                string sUrl;
                string sParams = string.Empty;

                if (this.Request.Params[Literals.ForumId] != null)
                {
                    if (SimulateIsNumeric.IsNumeric(this.Request.Params[Literals.ForumId]))
                    {
                        sParams = $"{ParamKeys.ForumId}={this.Request.Params[Literals.ForumId]}";
                    }
                }

                if (this.Request.Params[Literals.PostId] != null)
                {
                    if (SimulateIsNumeric.IsNumeric(this.Request.Params[Literals.PostId]))
                    {
                        sParams += $"|{ParamKeys.TopicId}={this.Request.Params[Literals.PostId]}";
                    }
                }

                sParams += $"|{ParamKeys.ViewType}={this.Request.Params[Literals.view]}";
                sUrl = this.NavigateUrl(this.TabId, "", sParams.Split('|'));

                this.Response.Status = "301 Moved Permanently";
                this.Response.AddHeader("Location", sUrl);
            }

            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();

        }
        #endregion
    }
}
