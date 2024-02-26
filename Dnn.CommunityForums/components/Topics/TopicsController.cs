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
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Linq;
using System.Text.RegularExpressions;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Search.Entities;

namespace DotNetNuke.Modules.ActiveForums
{
    #region Topics Controller
    public class TopicsController : DotNetNuke.Entities.Modules.ModuleSearchBase, DotNetNuke.Entities.Modules.IUpgradeable
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(TopicsController));
        public int Topic_QuickCreate(int PortalId, int ModuleId, int ForumId, string Subject, string Body, int UserId, string DisplayName, bool IsApproved, string IPAddress)
        {
            int topicId = -1;
            DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti = new DotNetNuke.Modules.ActiveForums.Entities.TopicInfo();
            ti.AnnounceEnd = Utilities.NullDate();
            ti.AnnounceStart = Utilities.NullDate();
            ti.Content.AuthorId = UserId;
            ti.Content.AuthorName = DisplayName;
            ti.Content.Subject = Subject;
            ti.Content.Body = Body;
            ti.Content.Summary = string.Empty;

            ti.Content.IPAddress = IPAddress;

            DateTime dt = DateTime.Now;
            ti.Content.DateCreated = dt;
            ti.Content.DateUpdated = dt;
            ti.IsAnnounce = false;
            ti.IsApproved = IsApproved;
            ti.IsArchived = false;
            ti.IsDeleted = false;
            ti.IsLocked = false;
            ti.IsPinned = false;
            ti.ReplyCount = 0;
            ti.StatusId = -1;
            ti.TopicIcon = string.Empty;
            ti.TopicType = TopicTypes.Topic;
            ti.ViewCount = 0;
            topicId = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().Save<int>(ti, ti.TopicId).TopicId;

            Utilities.UpdateModuleLastContentModifiedOnDate(ModuleId);

            if (topicId > 0)
            {
                Topics_SaveToForum(ForumId, topicId, PortalId, ModuleId);
                if (UserId > 0)
                {
                    Data.Profiles uc = new Data.Profiles();
                    uc.Profile_UpdateTopicCount(PortalId, UserId);
                }
            }
            return topicId;
        }

        public void Replies_Split(int OldTopicId, int NewTopicId, string listreplies, bool isNew)
        {
            Regex rgx = new Regex(@"^\d+(\|\d+)*$");
            if (OldTopicId > 0 && NewTopicId > 0 && rgx.IsMatch(listreplies))
            {
                if (isNew)
                {
                    string[] slistreplies = listreplies.Split("|".ToCharArray(), 2);
                    string str = "";
                    if (slistreplies.Length > 1) str = slistreplies[1];
                    DataProvider.Instance().Replies_Split(OldTopicId, NewTopicId, str, DateTime.Now, Convert.ToInt32(slistreplies[0]));
                }
                else
                {
                    DataProvider.Instance().Replies_Split(OldTopicId, NewTopicId, listreplies, DateTime.Now, 0);
                }
            }
        }
        [Obsolete("Deprecated in Community Forums. Scheduled removal in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Save(TopicInfo ti)")]
        public int TopicSave(int PortalId, TopicInfo ti) => new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().Save<int>(ti, ti.TopicId).TopicId;
        [Obsolete("Deprecated in Community Forums. Scheduled removal in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Save(TopicInfo ti)")]
        public int TopicSave(int PortalId, int ModuleId, DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti) => new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().Save<int>(ti, ti.TopicId).TopicId;
        public int Topics_SaveToForum(int ForumId, int TopicId, int PortalId, int ModuleId)
        {
            int id = Topics_SaveToForum(ForumId, TopicId, PortalId, ModuleId, -1);
            return id;
        }
        public int Topics_SaveToForum(int ForumId, int TopicId, int PortalId, int ModuleId, int LastReplyId)
        {
            Utilities.UpdateModuleLastContentModifiedOnDate(ModuleId);
            int id = Convert.ToInt32(DataProvider.Instance().Topics_SaveToForum(ForumId, TopicId, LastReplyId));

            return id;
        }
        [Obsolete("Deprecated in Community Forums. Scheduled removal in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.TopicController.GetById(int TopicId)")]
        public DotNetNuke.Modules.ActiveForums.Entities.TopicInfo Topics_Get(int PortalId, int ModuleId, int TopicId) => new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(TopicId);
        [Obsolete("Deprecated in Community Forums. Scheduled removal in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.TopicController.GetById(int TopicId)")]
        public DotNetNuke.Modules.ActiveForums.Entities.TopicInfo Topics_Get(int PortalId, int ModuleId, int TopicId, int ForumId, int UserId, bool WithSecurity) => new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(TopicId);
        [Obsolete("Deprecated in Community Forums. Scheduled removal in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.TopicController.DeleteById(int TopicId)")]
        public void Topics_Delete(int PortalId, int ModuleId, int ForumId, int TopicId, int DelBehavior) => new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().DeleteById(TopicId);
        public void Topics_Move(int PortalId, int ModuleId, int ForumId, int TopicId)
        {
            SettingsInfo settings = SettingsBase.GetModuleSettings(ModuleId);
            if (settings.URLRewriteEnabled)
            {
                try
                {
                    Data.ForumsDB db = new Data.ForumsDB();
                    int oldForumId = -1;
                    oldForumId = db.Forum_GetByTopicId(TopicId);
                    DotNetNuke.Modules.ActiveForums.Entities.ForumInfo fi = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Forums_Get(portalId: PortalId, moduleId: ModuleId, forumId: oldForumId, useCache: true);

                    if (!(string.IsNullOrEmpty(fi.PrefixURL)))
                    {
                        Data.Common dbC = new Data.Common();
                        string sURL = dbC.GetUrl(ModuleId, fi.ForumGroupId, oldForumId, TopicId, -1, -1);
                        if (!(string.IsNullOrEmpty(sURL)))
                        {
                            dbC.ArchiveURL(PortalId, fi.ForumGroupId, ForumId, TopicId, sURL);
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }
            DataProvider.Instance().Topics_Move(PortalId, ModuleId, ForumId, TopicId);
            Utilities.UpdateModuleLastContentModifiedOnDate(ModuleId);
        }

        #region "Obsolete ISearchable replaced by DotNetNuke.Entities.Modules.ModuleSearchBase.GetModifiedSearchDocuments "
        /*
		public DotNetNuke.Services.Search.SearchItemInfoCollection GetSearchItems(DotNetNuke.Entities.Modules.ModuleInfo ModInfo)
		{
			DotNetNuke.Services.Search.SearchItemInfoCollection SearchItemCollection = new DotNetNuke.Services.Search.SearchItemInfoCollection();
			IDataReader dr = null;
			try
			{
				dr = DataProvider.Instance().Search_DotNetNuke(ModInfo.ModuleID);
				DotNetNuke.Services.Search.SearchItemInfo SearchItem = null;
				while (dr.Read())
				{
					string subject = dr["Subject"].ToString();
					string description = string.Empty;
					string body = dr["Body"].ToString();
					int authorid = Convert.ToInt32(dr["AuthorId"]);
					DateTime datecreated = Convert.ToDateTime(dr["DateCreated"]);
					DateTime dateupdated = Convert.ToDateTime(dr["DateUpdated"]);
					int contentid = Convert.ToInt32(dr["ContentId"]);
					int forumid = Convert.ToInt32(dr["ForumId"]);
					int topicid = Convert.ToInt32(dr["TopicId"]);
					int replyId = Convert.ToInt32(dr["ReplyId"]);
					int jumpid = topicid;
					if (replyId > 0)
					{
						jumpid = replyId;
					}
					body = System.Web.HttpUtility.HtmlDecode(body);
					body = Utilities.StripHTMLTag(body);
					if (! (string.IsNullOrEmpty(body)))
					{
						if (body.Length > 100)
						{
							description = body.Substring(0, 100) + "...";
						}
						else
						{
							description = body;
						}
					}
					SearchItem = new DotNetNuke.Services.Search.SearchItemInfo(subject, description, authorid, datecreated, ModInfo.ModuleID, contentid.ToString() + "-" + forumid.ToString(), body, ParamKeys.ForumId + "=" + forumid + "&" + ParamKeys.ViewType + "=" + Views.Topic + "&" + ParamKeys.TopicId + "=" + topicid + "&" + ParamKeys.ContentJumpId + "=" + jumpid);
					SearchItemCollection.Add(SearchItem);
				}
				dr.Close();
				return SearchItemCollection;
			}
			catch (Exception ex)
			{
				return null;
			}
			finally
			{
				if (dr != null)
				{
					if (! dr.IsClosed)
					{
						dr.Close();
					}
				}
			}
		}
		*/
        #endregion "Obsolete ISearchable replaced by DotNetNuke.Entities.Modules.ModuleSearchBase.GetModifiedSearchDocuments "

        public DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ApproveTopic(int PortalId, int TabId, int ModuleId, int ForumId, int TopicId)
        {
            DotNetNuke.Modules.ActiveForums.Entities.ForumInfo fi = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Forums_Get(portalId: PortalId, moduleId: ModuleId, forumId: ForumId, useCache: true);
            TopicsController tc = new TopicsController();
            DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(TopicId);
            if (ti == null)
            {
                return null;
            }
            ti.IsApproved = true;
            new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().Save<int>(ti, ti.TopicId);
            tc.Topics_SaveToForum(ForumId, TopicId, PortalId, ModuleId);

            if (fi.ModApproveTemplateId > 0 & ti.Author.AuthorId > 0)
            {
                Email.SendEmail(fi.ModApproveTemplateId, PortalId, ModuleId, TabId, ForumId, TopicId, 0, string.Empty, ti.Author);
            }

            Subscriptions.SendSubscriptions(PortalId, ModuleId, TabId, ForumId, TopicId, 0, ti.Content.AuthorId);

            try
            {
                ControlUtils ctlUtils = new ControlUtils();
                string sUrl = ctlUtils.BuildUrl(TabId, ModuleId, fi.ForumGroup.PrefixURL, fi.PrefixURL, fi.ForumGroupId, fi.ForumID, TopicId, ti.TopicUrl, -1, -1, string.Empty, 1, -1, fi.SocialGroupId);
                Social amas = new Social();
                amas.AddTopicToJournal(PortalId, ModuleId, TabId, ForumId, TopicId, ti.Author.AuthorId, sUrl, ti.Content.Subject, string.Empty, ti.Content.Body, fi.Security.Read, fi.SocialGroupId);
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
            return ti;
        }
        [Obsolete("Deprecated in Community Forums. Moved to Utilities and changed to internal in 10.00.00.")]
        public void UpdateModuleLastContentModifiedOnDate(int ModuleId)
        {
            Utilities.UpdateModuleLastContentModifiedOnDate(ModuleId);
        }

        #region ModuleSearchBase

        public override IList<SearchDocument> GetModifiedSearchDocuments(ModuleInfo moduleInfo, DateTime beginDateUtc)
        {
            var ms = new SettingsInfo { MainSettings = moduleInfo.ModuleSettings };
            /* if not using soft deletes, remove and rebuild entire index; 
			   note that this "internals" method is suggested by blog post (https://www.dnnsoftware.com/community-blog/cid/154913/integrating-with-search-introducing-modulesearchbase#Comment106)
			   and also is used by the Community Links module (https://github.com/DNNCommunity/DNN.Links/blob/development/Components/FeatureController.cs)
			*/
            if (ms.DeleteBehavior != 1)
            {
                DotNetNuke.Services.Search.Internals.InternalSearchController.Instance.DeleteSearchDocumentsByModule(moduleInfo.PortalID, moduleInfo.ModuleID, moduleInfo.ModuleDefID);
                beginDateUtc = SqlDateTime.MinValue.Value.AddDays(1);
            }

            /* since this code runs without HttpContext, get https:// by looking at page settings */
            bool isHttps = DotNetNuke.Entities.Tabs.TabController.Instance.GetTab(moduleInfo.TabID, moduleInfo.PortalID).IsSecure;
            bool useFriendlyURLs = Utilities.UseFriendlyURLs(moduleInfo.ModuleID);
            string primaryPortalAlias = DotNetNuke.Entities.Portals.PortalAliasController.Instance.GetPortalAliasesByPortalId(moduleInfo.PortalID).FirstOrDefault(x => x.IsPrimary).HTTPAlias;
            PortalSettings portalSettings = DotNetNuke.Modules.ActiveForums.Utilities.GetPortalSettings();

            Dictionary<int, string> AuthorizedRolesForForum = new Dictionary<int, string>();

            List<string> roles = new List<string>();
            foreach (DotNetNuke.Security.Roles.RoleInfo r in DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoles(moduleInfo.PortalID))
            {
                roles.Add(r.RoleName);
            }
            string roleIds = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIds(moduleInfo.PortalID, roles.ToArray());

            string queryString = string.Empty;
            System.Text.StringBuilder qsb = new System.Text.StringBuilder();
            List<SearchDocument> searchDocuments = new List<SearchDocument>();
            IDataReader dr = null;
            try
            {
                dr = DataProvider.Instance().Search_DotNetNuke(moduleInfo.ModuleID, beginDateUtc);
                while (dr.Read())
                {
                    string subject = dr["Subject"].ToString();
                    string description = string.Empty;
                    string body = dr["Body"].ToString();
                    List<string> tags = dr["Tags"].ToString().Split(",".ToCharArray()).ToList();
                    DateTime dateupdated = Convert.ToDateTime(dr["DateUpdated"]);
                    int authorid = Convert.ToInt32(dr["AuthorId"]);
                    bool isDeleted = Convert.ToBoolean(dr["isDeleted"]);
                    bool isApproved = Convert.ToBoolean(dr["isApproved"]);
                    int contentid = Convert.ToInt32(dr["ContentId"]);
                    int forumid = Convert.ToInt32(dr["ForumId"]); 
                    int forumGroupId = Convert.ToInt32(dr["ForumGroupId"]); 
                    int topicid = Convert.ToInt32(dr["TopicId"]);
                    int replyId = Convert.ToInt32(dr["ReplyId"]);
                    string topicURL = dr["TopicUrl"].ToString();
                    string forumGroupUrlPrefix = dr["ForumGroupUrlPrefix"].ToString();
                    string forumUrlPrefix = dr["ForumUrlPrefix"].ToString();
                    int jumpid = (replyId > 0) ? replyId : topicid;
                    body = Common.Utilities.HtmlUtils.Clean(body, false);
                    if (!(string.IsNullOrEmpty(body)))
                    {
                        description = body.Length > 100 ? body.Substring(0, 100) + "..." : body;
                    };
                    DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forumInfo = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForum(moduleInfo.PortalID, moduleInfo.ModuleID, forumid);

                    // NOTE: indexer is called from scheduler and has no httpcontext 
                    // so any code that relies on HttpContext cannot be used...

                    string link = new ControlUtils().BuildUrl(moduleInfo.TabID, moduleInfo.ModuleID, forumGroupUrlPrefix, forumUrlPrefix, forumGroupId, forumid, topicid, topicURL, -1, -1, string.Empty, 1, contentid, forumInfo.SocialGroupId);
                    if (!(string.IsNullOrEmpty(link)) && !(link.StartsWith("http")))
                    {
                        link = (isHttps ? "https://" : "http://") + primaryPortalAlias + link;
                    }
                    queryString = qsb.Clear().Append(ParamKeys.ForumId).Append("=").Append(forumid).Append("&").Append(ParamKeys.TopicId).Append("=").Append(topicid).Append("&").Append(ParamKeys.ViewType).Append("=").Append(Views.Topic).Append("&").Append(ParamKeys.ContentJumpId).Append("=").Append(jumpid).ToString();
                    string permittedRolesCanView = string.Empty;
                    if (!AuthorizedRolesForForum.TryGetValue(forumid, out permittedRolesCanView))
                    {
                        var canView = new Data.Common().WhichRolesCanViewForum(moduleInfo.ModuleID, forumid, roleIds);
                        permittedRolesCanView = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetNamesForRoles(moduleInfo.PortalID, string.Join(";", canView.Split(":".ToCharArray())));
                        AuthorizedRolesForForum.Add(forumid, permittedRolesCanView);
                    }
                    var searchDoc = new SearchDocument
                    {
                        UniqueKey = moduleInfo.ModuleID.ToString() + "-" + contentid.ToString(),
                        AuthorUserId = authorid,
                        PortalId = moduleInfo.PortalID,
                        Title = subject,
                        Description = description,
                        Body = body,
                        Url = link,
                        QueryString = queryString,
                        ModifiedTimeUtc = dateupdated,
                        Tags = (tags.Count > 0 ? tags : null),
                        Permissions = permittedRolesCanView,
                        IsActive = (isApproved && !isDeleted)
                    };
                    searchDocuments.Add(searchDoc);
                };
                dr.Close();
                return searchDocuments;
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                if (dr != null)
                {
                    if (!dr.IsClosed)
                    {
                        dr.Close();
                    }
                }
            }

        }
        #endregion

        //Public Function ActiveForums_GetPostsForSearch(ByVal ModuleID As Integer) As ArrayList
        //    Return CBO.FillCollection(DataProvider.Instance().ActiveForums_GetPostsForSearch(ModuleID), GetType(PostInfo))
        //End Function
        #region "IUpgradeable"
        public string UpgradeModule(string Version)
        {
            switch (Version)
            {
                case "07.00.07":
                    try
                    {
                        var fc = new ForumsConfig();
                        fc.ArchiveOrphanedAttachments();
                    }
                    catch (Exception ex)
                    {
                        LogError(ex.Message, ex);
                        Exceptions.LogException(ex);
                        return "Failed";
                    }

                    break;
                case "07.00.11":
                    try
                    {
                        DotNetNuke.Modules.ActiveForums.Helpers.UpgradeModuleSettings.MoveSettings();
                    }
                    catch (Exception ex)
                    {
                        LogError(ex.Message, ex);
                        Exceptions.LogException(ex);
                        return "Failed";
                    }

                    break;
                case "07.00.12":
                    try
                    {
                        ForumsConfig.FillMissingTopicUrls();
                    }
                    catch (Exception ex)
                    {
                        LogError(ex.Message, ex);
                        Exceptions.LogException(ex);
                        return "Failed";
                    }

                    break;
                case "08.00.00":
                    try
                    {
                        var fc = new ForumsConfig();
                        fc.Install_Or_Upgrade_MoveTemplates();
                        fc.Install_Or_Upgrade_RenameThemeCssFiles();
                        fc.Install_Or_Upgrade_RelocateDefaultThemeToLegacy();
                        ForumsConfig.FillMissingTopicUrls(); /* for anyone upgrading from 07.00.12-> 08.00.00 */
                    }
                    catch (Exception ex)
                    {
                        LogError(ex.Message, ex);
                        Exceptions.LogException(ex);
                        return "Failed";
                    }

                    break;
                default:
                    break;
            }
            return Version;
        }
        private void LogError(string message, Exception ex)
        {
            if (ex != null)
            {
                Logger.Error(message, ex);
                if (ex.InnerException != null)
                {
                    Logger.Error(ex.InnerException.Message, ex.InnerException);
                }
            }
            else
            {
                Logger.Error(message);
            }
        }
        #endregion

    }

    #endregion
}

