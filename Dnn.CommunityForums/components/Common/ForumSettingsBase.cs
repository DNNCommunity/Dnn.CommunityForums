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

    using DotNetNuke.Entities.Modules;

    public class ForumSettingsBase : ModuleSettingsBase
    {
        /// <summary>
        /// This method is only needed because of an issue in DNN as of 8.0.4 where settings don't get updated if they are equal when compared case insensitively
        /// </summary>
        /// <param name="key"></param>
        /// <param name="newValue"></param>
        private void UpdateModuleSettingCaseSensitive(string key, string newValue)
        {
            var oldValue = this.Settings.GetString(key);
            if (oldValue != null && oldValue != newValue && oldValue.ToLowerInvariant() == newValue.ToLowerInvariant())
            {
                // changed but case-insensitive identical: empty the setting first
                DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(this.ModuleId, key, string.Empty);
            }

            DotNetNuke.Entities.Modules.ModuleController.Instance.UpdateModuleSetting(this.ModuleId, key, newValue);
        }

        public string Mode
        {
            get
            {
                return this.Settings.GetString(SettingKeys.Mode, "Standard");
            }

            set
            {
                this.UpdateModuleSettingCaseSensitive(SettingKeys.Mode, value);
            }
        }

        public string Theme
        {
            get
            {
                return this.Settings.GetString(SettingKeys.Theme, "_legacy");
            }

            set
            {
                this.UpdateModuleSettingCaseSensitive(SettingKeys.Theme, value);
            }
        }

        public string TimeFormatString
        {
            get
            {
                return this.Settings.GetString(SettingKeys.TimeFormatString, "h:mm tt");
            }

            set
            {
                this.UpdateModuleSettingCaseSensitive(SettingKeys.TimeFormatString, value);
            }
        }

        public string DateFormatString
        {
            get
            {
                return this.Settings.GetString(SettingKeys.DateFormatString, "MM/dd/yyyy");
            }

            set
            {
                this.UpdateModuleSettingCaseSensitive(SettingKeys.DateFormatString, value);
            }
        }

        public int TemplateId
        {
            get
            {
                return this.Settings.GetInt(SettingKeys.ForumTemplateId);
            }

            set
            {
                this.UpdateModuleSettingCaseSensitive(SettingKeys.ForumTemplateId, value.ToString());
            }
        }

        public int PageSize
        {
            get
            {
                return this.Settings.GetInt(SettingKeys.PageSize, 25);
            }

            set
            {
                this.UpdateModuleSettingCaseSensitive(SettingKeys.PageSize, value.ToString());
            }
        }

        public int FloodInterval
        {
            get
            {
                return this.Settings.GetInt(SettingKeys.FloodInterval);
            }

            set
            {
                this.UpdateModuleSettingCaseSensitive(SettingKeys.FloodInterval, value.ToString());
            }
        }

        public int EditInterval
        {
            get
            {
                return this.Settings.GetInt(SettingKeys.EditInterval);
            }

            set
            {
                this.UpdateModuleSettingCaseSensitive(SettingKeys.EditInterval, value.ToString());
            }
        }

        public bool AutoLink
        {
            get
            {
                return this.Settings.GetBoolean(SettingKeys.EnableAutoLink, true);
            }

            set
            {
                this.UpdateModuleSettingCaseSensitive(SettingKeys.EnableAutoLink, value.ToString());
            }
        }

        public int DeleteBehavior
        {
            get
            {
                return this.Settings.GetInt(SettingKeys.DeleteBehavior);
            }

            set
            {
                this.UpdateModuleSettingCaseSensitive(SettingKeys.DeleteBehavior, value.ToString());
            }
        }

        public int ProfileVisibility
        {
            get
            {
                return this.Settings.GetInt(SettingKeys.ProfileVisibility, (int)ProfileVisibilities.Everyone);
            }

            set
            {
                this.UpdateModuleSettingCaseSensitive(SettingKeys.ProfileVisibility, value.ToString());
            }
        }

        public int MessagingType
        {
            get
            {
                return this.Settings.GetInt(SettingKeys.PMType);
            }

            set
            {
                this.UpdateModuleSettingCaseSensitive(SettingKeys.PMType, value.ToString());
            }
        }

        public int MessagingTabId
        {
            get
            {
                return this.Settings.GetInt(SettingKeys.PMTabId);
            }

            set
            {
                this.UpdateModuleSettingCaseSensitive(SettingKeys.PMTabId, value.ToString());
            }
        }

        public int Signatures
        {
            get
            {
                return this.Settings.GetInt(SettingKeys.AllowSignatures, 1);
            }

            set
            {
                this.UpdateModuleSettingCaseSensitive(SettingKeys.AllowSignatures, value.ToString());
            }
        }

        public string UserNameDisplay
        {
            get
            {
                return this.Settings.GetString(SettingKeys.UserNameDisplay, "Username");
            }

            set
            {
                this.UpdateModuleSettingCaseSensitive(SettingKeys.UserNameDisplay, value);
            }
        }

        public bool FriendlyURLs
        {
            get
            {
                return this.Settings.GetBoolean(SettingKeys.EnableURLRewriter);
            }

            set
            {
                this.UpdateModuleSettingCaseSensitive(SettingKeys.EnableURLRewriter, value.ToString());
            }
        }

        public string PrefixURLBase
        {
            get
            {
                return this.Settings.GetString(SettingKeys.PrefixURLBase, "forums");
            }

            set
            {
                this.UpdateModuleSettingCaseSensitive(SettingKeys.PrefixURLBase, value);
            }
        }

        public string PrefixURLTag
        {
            get
            {
                return this.Settings.GetString(SettingKeys.PrefixURLTags, "tag");
            }

            set
            {
                this.UpdateModuleSettingCaseSensitive(SettingKeys.PrefixURLTags, value);
            }
        }

        public string PrefixURLCategory
        {
            get
            {
                return this.Settings.GetString(SettingKeys.PrefixURLCategories, "category");
            }

            set
            {
                this.UpdateModuleSettingCaseSensitive(SettingKeys.PrefixURLCategories, value);
            }
        }

        public string PrefixURLOther
        {
            get
            {
                return this.Settings.GetString(SettingKeys.PrefixURLOther, "views");
            }

            set
            {
                this.UpdateModuleSettingCaseSensitive(SettingKeys.PrefixURLOther, value);
            }
        }

        public string PrefixURLLikes
        {
            get
            {
                return this.Settings.GetString(SettingKeys.PrefixURLLikes, Views.Likes);
            }

            set
            {
                this.UpdateModuleSettingCaseSensitive(SettingKeys.PrefixURLLikes, value);
            }
        }

        public bool FullTextSearch
        {
            get
            {
                return this.Settings.GetBoolean(SettingKeys.FullText);
            }

            set
            {
                this.UpdateModuleSettingCaseSensitive(SettingKeys.FullText, value.ToString());
                if (this.Settings.ContainsKey(SettingKeys.FullText))
                {
                    this.Settings[SettingKeys.FullText] = value.ToString();
                }
                else
                {
                    this.Settings.Add(SettingKeys.FullText, value.ToString());
                }
            }
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public bool MailQueue => true;

        public bool CacheTemplates
        {
            get
            {
                return this.Settings.GetBoolean(SettingKeys.CacheTemplates, true);
            }

            set
            {
                this.UpdateModuleSettingCaseSensitive(SettingKeys.CacheTemplates, value.ToString());
            }
        }

        public bool EnablePoints
        {
            get
            {
                return this.Settings.GetBoolean(SettingKeys.EnablePoints);
            }

            set
            {
                this.UpdateModuleSettingCaseSensitive(SettingKeys.EnablePoints, value.ToString());
            }
        }

        public int TopicPointValue
        {
            get
            {
                return this.Settings.GetInt(SettingKeys.TopicPointValue, 1);
            }

            set
            {
                this.UpdateModuleSettingCaseSensitive(SettingKeys.TopicPointValue, value.ToString());
            }
        }

        public int ReplyPointValue
        {
            get
            {
                return this.Settings.GetInt(SettingKeys.ReplyPointValue, 1);
            }

            set
            {
                this.UpdateModuleSettingCaseSensitive(SettingKeys.ReplyPointValue, value.ToString());
            }
        }

        public int AnswerPointValue
        {
            get
            {
                return this.Settings.GetInt(SettingKeys.AnswerPointValue, 1);
            }

            set
            {
                this.UpdateModuleSettingCaseSensitive(SettingKeys.AnswerPointValue, value.ToString());
            }
        }

        public int MarkAsAnswerPointValue
        {
            get
            {
                return this.Settings.GetInt(SettingKeys.MarkAnswerPointValue, 1);
            }

            set
            {
                this.UpdateModuleSettingCaseSensitive(SettingKeys.MarkAnswerPointValue, value.ToString());
            }
        }

        public int ModPointValue
        {
            get
            {
                return this.Settings.GetInt(SettingKeys.ModPointValue, 1);
            }

            set
            {
                this.UpdateModuleSettingCaseSensitive(SettingKeys.ModPointValue, value.ToString());
            }
        }

        public int ForumGroupTemplate
        {
            get
            {
                return this.Settings.GetInt("ForumGroupTemplate", -1);
            }

            set
            {
                this.UpdateModuleSettingCaseSensitive("ForumGroupTemplate", value.ToString());
            }
        }

        public string ForumConfig
        {
            get
            {
                return this.Settings.GetString("ForumConfig", string.Empty);
            }

            set
            {
                this.UpdateModuleSettingCaseSensitive("ForumConfig", value);
            }
        }

        public int AvatarHeight
        {
            get
            {
                return this.Settings.GetInt(SettingKeys.AvatarHeight, 48);
            }

            set
            {
                this.UpdateModuleSettingCaseSensitive(SettingKeys.AvatarHeight, value.ToString());
            }
        }

        public int AvatarWidth
        {
            get
            {
                return this.Settings.GetInt(SettingKeys.AvatarWidth, 48);
            }

            set
            {
                this.UpdateModuleSettingCaseSensitive(SettingKeys.AvatarWidth, value.ToString());
            }
        }

        public bool EnableUsersOnline
        {
            get
            {
                return this.Settings.GetBoolean(SettingKeys.UsersOnlineEnabled);
            }

            set
            {
                this.UpdateModuleSettingCaseSensitive(SettingKeys.UsersOnlineEnabled, value.ToString());
            }
        }

        public bool UseSkinBreadCrumb
        {
            get
            {
                return this.Settings.GetBoolean(SettingKeys.UseSkinBreadCrumb);
            }

            set
            {
                this.UpdateModuleSettingCaseSensitive(SettingKeys.UseSkinBreadCrumb, value.ToString());
            }
        }
    }
}
