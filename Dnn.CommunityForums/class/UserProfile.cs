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
    using System.Collections;
    using System.Data;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.UI.UserControls;

    #region UserProfileInfo
    public class UserProfileInfo
    {
        #region Constructors
        public UserProfileInfo()
        {
            IsUserOnline = false;
            IsMod = false;

            PrefDefaultSort = "ASC";
            PrefPageSize = 20;
            PrefBlockSignatures = false;
            PrefBlockAvatars = false;
            PrefJumpLastPost = false;
            PrefDefaultShowReplies = false;
            PrefUseAjax = false;
            PrefTopicSubscribe = false;
        }

        public UserProfileInfo(int UserId, int PortalId)
        {
            IsUserOnline = false;
            IsMod = false;
            PrefBlockSignatures = false;
            PrefBlockAvatars = false;
            PrefTopicSubscribe = false;
            PrefJumpLastPost = false;
            PrefDefaultShowReplies = false;
        }
        #endregion

        #region Public Properties
        public int ProfileId { get; set; } = -1;

        public int UserID { get; set; } = -1;

        public int PortalId { get; set; }

        public int ModuleId { get; set; }

        public int TopicCount { get; set; }

        public int ReplyCount { get; set; }

        public int ViewCount { get; set; }

        public int AnswerCount { get; set; }

        public int RewardPoints { get; set; }

        public string UserCaption { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime DateUpdated { get; set; }

        public DateTime DateLastActivity { get; set; }

        public DateTime DateLastPost { get; set; }

        public DateTime DateLastReply { get; set; }

        public string Signature { get; set; }

        public bool SignatureDisabled { get; set; }

        public int TrustLevel { get; set; }

        public bool AdminWatch { get; set; }

        public bool AttachDisabled { get; set; }

        public string Avatar { get; set; }

        public AvatarTypes AvatarType { get; set; }

        public bool AvatarDisabled { get; set; }

        public string PrefDefaultSort { get; set; } = "ASC";

        public bool PrefDefaultShowReplies { get; set; }

        public bool PrefJumpLastPost { get; set; }

        public bool PrefTopicSubscribe { get; set; }

        public SubscriptionTypes PrefSubscriptionType { get; set; }

        public bool PrefUseAjax { get; set; } = true;

        public bool PrefBlockAvatars { get; set; }

        public bool PrefBlockSignatures { get; set; }

        public int PrefPageSize { get; set; } = 20;

        public string Yahoo { get; set; }

        public string MSN { get; set; }

        public string ICQ { get; set; }

        public string AOL { get; set; }

        public string Occupation { get; set; }

        public string Location { get; set; }

        public string Interests { get; set; }

        public string WebSite { get; set; }

        public string Badges { get; set; }

        public string Roles { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string DisplayName { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

        public bool IsMod { get; set; }

        public string Bio { get; set; } = string.Empty;

        public bool IsUserOnline { get; set; }

        public Hashtable ProfileProperties { get; set; }

        public CurrentUserTypes CurrentUserType { get; set; }

        public string ForumsAllowed { get; set; }

        public string UserForums { get; set; }
        #endregion

        #endregion
        #region Public ReadOnly Properties
        public int PostCount
        {
            get
            {
                return TopicCount + ReplyCount;
            }
        }
        #endregion
        #region UserProfileController
    }

    public class UserProfileController
    {
        public UserProfileInfo Profiles_Get(int PortalId, int ModuleId, int UserId)
        {

            UserProfileInfo upi = (UserProfileInfo)DataCache.SettingsCacheRetrieve(ModuleId, string.Format(CacheKeys.UserProfile, ModuleId, UserId));
            if (upi == null)
            {
                DataSet ds = DataProvider.Instance().Profiles_Get(PortalId, ModuleId, UserId);
                if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    IDataReader dr;
                    dr = ds.CreateDataReader();
                    upi = CBO.FillObject<UserProfileInfo>(dr);
                    DataCache.SettingsCacheStore(ModuleId, string.Format(CacheKeys.UserProfile, ModuleId, UserId), upi);
                }
            }

            return upi;
        }

        public void Profiles_Save(UserProfileInfo upi)
        {
            DataProvider.Instance().Profiles_Save(upi.PortalId, upi.ModuleId, upi.UserID, upi.TopicCount, upi.ReplyCount, upi.ViewCount, upi.AnswerCount, upi.RewardPoints, upi.UserCaption, upi.Signature, upi.SignatureDisabled, upi.TrustLevel, upi.AdminWatch, upi.AttachDisabled, upi.Avatar, (int)upi.AvatarType, upi.AvatarDisabled, upi.PrefDefaultSort, upi.PrefDefaultShowReplies, upi.PrefJumpLastPost, upi.PrefTopicSubscribe, (int)upi.PrefSubscriptionType, upi.PrefUseAjax, upi.PrefBlockAvatars, upi.PrefBlockSignatures, upi.PrefPageSize, upi.Yahoo, upi.MSN, upi.ICQ, upi.AOL, upi.Occupation, upi.Location, upi.Interests, upi.WebSite, upi.Badges);
            DataCache.SettingsCacheStore(upi.ModuleId, string.Format(CacheKeys.UserProfile, upi.ModuleId, upi.UserID), upi);
        }

        [Obsolete("Deprecated in Community Forums. Scheduled removal in 09.00.00. Use UserProfileController.Profiles_ClearCache(int ModuleId, int UserId)")]
        public static void Profiles_ClearCache(int UserID)
        {
            DataCache.CacheClearPrefix(-1, CacheKeys.CachePrefix);

        }

        public static void Profiles_ClearCache(int ModuleId, int UserId)
        {
            DataCache.SettingsCacheClear(ModuleId, string.Format(CacheKeys.UserProfile, ModuleId, UserId));
        }
    }
    #endregion
}
