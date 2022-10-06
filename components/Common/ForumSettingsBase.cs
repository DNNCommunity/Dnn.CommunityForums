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
using DotNetNuke.Entities.Modules;

namespace DotNetNuke.Modules.ActiveForums
{
	public class ForumSettingsBase : ModuleSettingsBase
	{
		private readonly ModuleController _objModules = new ModuleController();

        /// <summary>
        /// This method is only needed because of an issue in DNN as of 8.0.4 where settings don't get updated if they are equal when compared case insensitively
        /// </summary>
        /// <param name="key"></param>
        /// <param name="newValue"></param>
        private void UpdateTabModuleSettingCaseSensitive(string key, string newValue)
        {
            var oldValue = Settings.GetString(key);
            if (oldValue != null && oldValue != newValue && oldValue.ToLowerInvariant() == newValue.ToLowerInvariant())
            {
                // changed but case-insensitive identical: empty the setting first
                UpdateTabModuleSettingCaseSensitive(key, "");
            }
            _objModules.UpdateTabModuleSetting(TabModuleId, key, newValue);
        }

        /// <summary>
        /// This method is only needed because of an issue in DNN as of 8.0.4 where settings don't get updated if they are equal when compared case insensitively
        /// </summary>
        /// <param name="key"></param>
        /// <param name="newValue"></param>
        private void UpdateModuleSettingCaseSensitive(string key, string newValue)
        {
            var oldValue = Settings.GetString(key);
            if (oldValue != null && oldValue != newValue && oldValue.ToLowerInvariant() == newValue.ToLowerInvariant())
            {
                // changed but case-insensitive identical: empty the setting first
                _objModules.UpdateModuleSetting(ModuleId, key, "");
            }
            _objModules.UpdateModuleSetting(ModuleId, key, newValue);
        }

		public string Mode
		{
			get
			{
                return Settings.GetString(SettingKeys.Mode, "Standard");
			}
			set
			{
				UpdateTabModuleSettingCaseSensitive(SettingKeys.Mode, value);
			}
		}

		public string Theme
		{
			get
			{
                return Settings.GetString(SettingKeys.Theme, "_default");
			}
			set
			{
				UpdateModuleSettingCaseSensitive(SettingKeys.Theme, value);
			}
		}

        public string TimeFormatString
        {
            get
            {
                return Settings.GetString(SettingKeys.TimeFormatString, "h:mm tt");
            }
            set
            {
                UpdateModuleSettingCaseSensitive(SettingKeys.TimeFormatString, value);
            }
        }

        public string DateFormatString
        {
            get
            {
                return Settings.GetString(SettingKeys.DateFormatString, "MM/dd/yyyy");
            }
            set
            {
                UpdateModuleSettingCaseSensitive(SettingKeys.DateFormatString, value);
            }
        }

		public int TemplateId
		{
			get
			{
				return Settings.GetInt(SettingKeys.ForumTemplateId);
			}
			set
			{
				UpdateModuleSettingCaseSensitive(SettingKeys.ForumTemplateId, value.ToString());
			}
		}

		public int PageSize
		{
			get
			{
				return Settings.GetInt(SettingKeys.PageSize, 25);
			}
			set
			{
				UpdateModuleSettingCaseSensitive(SettingKeys.PageSize, value.ToString());
			}
		}

		public int FloodInterval
		{
			get
			{
                return Settings.GetInt(SettingKeys.FloodInterval);
			}
			set
			{
				UpdateModuleSettingCaseSensitive(SettingKeys.FloodInterval, value.ToString());
			}
		}

		public int EditInterval
		{
			get
			{
                return Settings.GetInt(SettingKeys.EditInterval);
			}
			set
			{
				UpdateModuleSettingCaseSensitive(SettingKeys.EditInterval, value.ToString());
			}
		}

		public bool AutoLink
		{
			get
			{
                return Settings.GetBoolean(SettingKeys.EnableAutoLink, true);
			}
			set
			{
				UpdateModuleSettingCaseSensitive(SettingKeys.EnableAutoLink, value.ToString());
			}
		}

		public int DeleteBehavior
		{
			get
			{
                return Settings.GetInt(SettingKeys.DeleteBehavior);
			}
			set
			{
				UpdateModuleSettingCaseSensitive(SettingKeys.DeleteBehavior, value.ToString());
			}
		}

		public string AddThis
		{
			get
			{
				return Settings.GetString(SettingKeys.AddThisAccount, string.Empty);
			}
			set
			{
				UpdateModuleSettingCaseSensitive(SettingKeys.AddThisAccount, value);
			}
		}

		public int ProfileVisibility
		{
			get
			{
				return Settings.GetInt(SettingKeys.ProfileVisibility, (int)ProfileVisibilities.Everyone);
			}
			set
			{
				UpdateModuleSettingCaseSensitive(SettingKeys.ProfileVisibility, value.ToString());
			}
		}

        public int MessagingType
        {
            get
            {
                return Settings.GetInt(SettingKeys.PMType);
            }
            set
            {
                UpdateModuleSettingCaseSensitive(SettingKeys.PMType, value.ToString());
            }
        }

        public int MessagingTabId
        {
            get
            {
                return Settings.GetInt(SettingKeys.PMTabId);
            }
            set
            {
                UpdateModuleSettingCaseSensitive(SettingKeys.PMTabId, value.ToString());
            }
        }

		public int Signatures
		{
			get
			{
                return Settings.GetInt(SettingKeys.AllowSignatures, 1);
			}
			set
			{
				UpdateModuleSettingCaseSensitive(SettingKeys.AllowSignatures, value.ToString());
			}
		}

		public string UserNameDisplay
		{
			get
			{
				return Settings.GetString(SettingKeys.UserNameDisplay, "Username");
			}
			set
			{
				UpdateModuleSettingCaseSensitive(SettingKeys.UserNameDisplay, value);
			}
		}

		public bool FriendlyURLs
		{
			get
			{
				return Settings.GetBoolean(SettingKeys.EnableURLRewriter);
			}
			set
			{
				UpdateModuleSettingCaseSensitive(SettingKeys.EnableURLRewriter, value.ToString());
			}
		}

		public string PrefixURLBase
		{
			get
			{
				return Settings.GetString(SettingKeys.PrefixURLBase, "forums");
			}
			set
			{
				UpdateModuleSettingCaseSensitive(SettingKeys.PrefixURLBase, value);
			}
		}

		public string PrefixURLTag
		{
			get
			{
				return Settings.GetString(SettingKeys.PrefixURLTags, "tag");
			}
			set
			{
				UpdateModuleSettingCaseSensitive(SettingKeys.PrefixURLTags, value);
			}
		}

		public string PrefixURLCategory
		{
			get
			{
                return Settings.GetString(SettingKeys.PrefixURLCategories, "category");
			}
			set
			{
				UpdateModuleSettingCaseSensitive(SettingKeys.PrefixURLCategories, value);
			}
		}

		public string PrefixURLOther
		{
			get
			{
                return Settings.GetString(SettingKeys.PrefixURLOther, "views");
			}
			set
			{
				UpdateModuleSettingCaseSensitive(SettingKeys.PrefixURLOther, value);
			}
		}

		public bool FullTextSearch
		{
			get
			{
                return Settings.GetBoolean(SettingKeys.FullText);
			}
			set
			{
				UpdateModuleSettingCaseSensitive(SettingKeys.FullText, value.ToString());
                if (Settings.ContainsKey(SettingKeys.FullText))
                    Settings[SettingKeys.FullText] = value.ToString();
                else
                    Settings.Add(SettingKeys.FullText, value.ToString());
            }
		}

		public bool MailQueue
		{
			get
			{
				return Settings.GetBoolean(SettingKeys.MailQueue);
			}
			set
			{
				UpdateModuleSettingCaseSensitive(SettingKeys.MailQueue, value.ToString());
			}
		}

		public bool EnablePoints
		{
			get
			{
				return Settings.GetBoolean(SettingKeys.EnablePoints);
			}
			set
			{
				UpdateModuleSettingCaseSensitive(SettingKeys.EnablePoints, value.ToString());
			}
		}

		public int TopicPointValue
		{
			get
			{
				return Settings.GetInt(SettingKeys.TopicPointValue, 1);
			}
			set
			{
				UpdateModuleSettingCaseSensitive(SettingKeys.TopicPointValue, value.ToString());
			}
		}

		public int ReplyPointValue
		{
			get
			{
				return Settings.GetInt(SettingKeys.ReplyPointValue, 1);
			}
			set
			{
				UpdateModuleSettingCaseSensitive(SettingKeys.ReplyPointValue, value.ToString());
			}
		}

		public int AnswerPointValue
		{
			get
			{
                return Settings.GetInt(SettingKeys.AnswerPointValue, 1);
			}
			set
			{
				UpdateModuleSettingCaseSensitive(SettingKeys.AnswerPointValue, value.ToString());
			}
		}

		public int MarkAsAnswerPointValue
		{
			get
			{
                return Settings.GetInt(SettingKeys.MarkAnswerPointValue, 1);
			}
			set
			{
				UpdateModuleSettingCaseSensitive(SettingKeys.MarkAnswerPointValue, value.ToString());
			}
		}

		public int ModPointValue
		{
			get
			{
                return Settings.GetInt(SettingKeys.ModPointValue, 1);
			}
			set
			{
				UpdateModuleSettingCaseSensitive(SettingKeys.ModPointValue, value.ToString());
			}
		}

		public int ForumGroupTemplate
		{
			get
			{
                return Settings.GetInt("ForumGroupTemplate", -1);
			}
			set
			{
				//Use Tab Module Setting
				UpdateTabModuleSettingCaseSensitive("ForumGroupTemplate", value.ToString());
			}
		}

		public string ForumConfig
		{
			get
			{
				return Settings.GetString("ForumConfig", string.Empty);
			}
			set
			{
				UpdateTabModuleSettingCaseSensitive("ForumConfig", value);
			}
		}

		public int AvatarHeight
		{
			get
			{
                return Settings.GetInt(SettingKeys.AvatarHeight, 48);
			}
			set
			{
				UpdateModuleSettingCaseSensitive(SettingKeys.AvatarHeight, value.ToString());
			}
		}

		public int AvatarWidth
		{
			get
			{
                return Settings.GetInt(SettingKeys.AvatarWidth, 48);
			}
			set
			{
				UpdateModuleSettingCaseSensitive(SettingKeys.AvatarWidth, value.ToString());
			}
		}

        public bool EnableUsersOnline
        {
            get
            {
                return Settings.GetBoolean(SettingKeys.UsersOnlineEnabled);
            }
            set
            {
                UpdateModuleSettingCaseSensitive(SettingKeys.UsersOnlineEnabled, value.ToString());
            }
        }

        public bool UseSkinBreadCrumb
        {
            get
            {
                return Settings.GetBoolean(SettingKeys.UseSkinBreadCrumb);
            }
            set
            {
                UpdateModuleSettingCaseSensitive(SettingKeys.UseSkinBreadCrumb, value.ToString());
            }
        }
	}
}

