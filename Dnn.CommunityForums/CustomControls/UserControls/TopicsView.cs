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

using System;
using System.Collections.Generic;
using System.Data;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text.RegularExpressions;
using System.Xml;
using DotNetNuke.Instrumentation;
using DotNetNuke.Modules.ActiveForums.Constants;
using DotNetNuke.Entities.Portals;
using System.Linq;
using System.Net;
using System.Text;
using DotNetNuke.Modules.ActiveForums.Entities;

namespace DotNetNuke.Modules.ActiveForums.Controls
{
    [DefaultProperty("Text"), ToolboxData("<{0}:TopicsView runat=server></{0}:TopicsView>")]
    public class TopicsView : ForumBase
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Environment));

        #region Private Members
        private DataRow drForum;
        private DataRow drSecurity;
        private DataTable dtTopics;
        private DataTable dtAnnounce;
        private DataTable dtSubForums;
        private bool bView = false;
        private bool bRead = false;
        private bool bPoll = false;
        private bool bDelete = false;
        private bool bEdit = false;
        private bool bSubscribe = false;
        private bool bModApprove = false;
        private bool bModMove = false;
        private bool bModSplit = false;
        private bool bModDelete = false;
        private bool bModEdit = false;
        private bool bModLock = false;
        private bool bModPin = false;
        private bool bAllowRSS = false;
        private int RowIndex = 1;
        private int PageSize = 20;
        private int TopicRowCount = 0;
        private bool IsSubscribedForum = false;
        private string sGroupURL = string.Empty;
        private string sForumURL = string.Empty;
        #endregion
        #region Public Properties
        public string MetaTemplate { get; set; } = "[META][TITLE][PORTALNAME] - [PAGENAME] - [GROUPNAME] - [FORUMNAME][/TITLE][DESCRIPTION][BODY][/DESCRIPTION][KEYWORDS][VALUE][/KEYWORDS][/META]";
        public string MetaTitle { get; set; } = string.Empty;
        public string MetaDescription { get; set; } = string.Empty;
        public string MetaKeywords { get; set; } = string.Empty;

        public string ForumUrl { get; set; } = string.Empty;
        #endregion

        #region Controls
        protected af_quickjump ctlForumJump = new af_quickjump();

        protected ForumView ctlForumSubs = new ForumView();
        #endregion

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                if (ForumId < 1)
                {
                    Response.Redirect(NavigateUrl(TabId));
                }
                if (ForumInfo == null)
                {
                    Response.Redirect(NavigateUrl(TabId));
                }
                if (ForumInfo.Active == false)
                {
                    Response.Redirect(NavigateUrl(TabId));
                }
                this.AppRelativeVirtualPath = "~/";
                int defaultTemplateId = ForumInfo.TopicsTemplateId;
                if (DefaultTopicsViewTemplateId >= 0)
                {
                    defaultTemplateId = DefaultTopicsViewTemplateId;
                }
                string TopicsTemplate = string.Empty;
                TopicsTemplate = TemplateCache.GetCachedTemplate( ForumModuleId, "TopicsView", defaultTemplateId);
                if (TopicsTemplate.Contains("[NOTOOLBAR]"))
                {
                    if (HttpContext.Current.Items.Contains("ShowToolbar"))
                    {
                        HttpContext.Current.Items["ShowToolbar"] = false;
                    }
                    else
                    {
                        HttpContext.Current.Items.Add("ShowToolbar", false);
                    }
                    TopicsTemplate = TopicsTemplate.Replace("[NOTOOLBAR]", string.Empty);
                }
                StringBuilder stringBuilder = new StringBuilder(TopicsTemplate);

                stringBuilder = DotNetNuke.Modules.ActiveForums.Controllers.TokenController.ReplaceForumTokens(stringBuilder, ForumInfo, PortalSettings, MainSettings, new Services.URLNavigator().NavigationManager(), UserInfo, TabId, ForumModuleId, CurrentUserType);
                stringBuilder = DotNetNuke.Modules.ActiveForums.Controllers.TokenController.ReplaceModuleTokens(stringBuilder, PortalSettings, MainSettings, UserInfo, TabId, ForumModuleId);


                TopicsTemplate = stringBuilder.ToString();

                PageSize = MainSettings.PageSize;
                if (UserId > 0)
                {
                    PageSize = UserDefaultPageSize;
                }
                if (PageSize < 5)
                {
                    PageSize = 10;
                }

                if (PageId == 1)
                {
                    RowIndex = 0;
                }
                else
                {
                    RowIndex = ((PageId * PageSize) - PageSize);
                }
                string sort = SortColumns.ReplyCreated;
                if (TopicsTemplate.Contains("[AF:SORT:TOPICCREATED]"))
                {
                    sort = SortColumns.TopicCreated;
                    TopicsTemplate = TopicsTemplate.Replace("[AF:SORT:TOPICCREATED]", string.Empty);
                }
                TopicsTemplate = CheckControls(TopicsTemplate);

                TopicsTemplate = TopicsTemplate.Replace("[AF:SORT:REPLYCREATED]", string.Empty);
                if (TopicsTemplate.Contains("[TOPICS]"))
                {
                    string cacheKey = string.Format(CacheKeys.TopicsViewForUser, ModuleId, ForumId, UserId, HttpContext.Current?.Response?.Cookies["language"]?.Value, RowIndex, PageSize);
                    DataSet ds = (DataSet)DataCache.ContentCacheRetrieve(ForumModuleId, cacheKey);
                    if (ds == null)
                    {
                        ds = DataProvider.Instance().UI_TopicsView(PortalId, ForumModuleId, ForumId, UserId, RowIndex, PageSize, UserInfo.IsSuperUser, sort);
                        DataCache.ContentCacheStore(ModuleId, cacheKey, ds); 
                    }
                    if (ds.Tables.Count > 0)
                    {
                        drForum = ds.Tables[0].Rows[0];
                        drSecurity = ds.Tables[1].Rows[0];
                        dtSubForums = ds.Tables[2];
                        dtTopics = ds.Tables[3];
                        if (PageId == 1)
                        {
                            dtAnnounce = ds.Tables[4];
                        }

                        bView = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(drSecurity["CanView"].ToString(), ForumUser.UserRoles);
                        bRead = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(drSecurity["CanRead"].ToString(), ForumUser.UserRoles);
                        //bCreate = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(drSecurity["CanCreate"].ToString(), ForumUser.UserRoles);
                        bEdit = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(drSecurity["CanEdit"].ToString(), ForumUser.UserRoles);
                        bDelete = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(drSecurity["CanDelete"].ToString(), ForumUser.UserRoles);
                        //bReply = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(drSecurity["CanReply"].ToString(), ForumUser.UserRoles);
                        bPoll = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(drSecurity["CanPoll"].ToString(), ForumUser.UserRoles);

                        bSubscribe = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(drSecurity["CanSubscribe"].ToString(), ForumUser.UserRoles);
                        bModMove = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(drSecurity["CanModMove"].ToString(), ForumUser.UserRoles);
                        bModSplit = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(drSecurity["CanModSplit"].ToString(), ForumUser.UserRoles);
                        bModDelete = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(drSecurity["CanModDelete"].ToString(), ForumUser.UserRoles);
                        bModApprove = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(drSecurity["CanModApprove"].ToString(), ForumUser.UserRoles);
                        bModEdit = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(drSecurity["CanModEdit"].ToString(), ForumUser.UserRoles);
                        bModPin = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(drSecurity["CanModPin"].ToString(), ForumUser.UserRoles);
                        bModLock = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(drSecurity["CanModLock"].ToString(), ForumUser.UserRoles);
                        bModApprove = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(drSecurity["CanModApprove"].ToString(), ForumUser.UserRoles);

                        ControlUtils ctlUtils = new ControlUtils();
                        sGroupURL = ctlUtils.BuildUrl(TabId, ModuleId, ForumInfo.ForumGroup.PrefixURL, string.Empty, ForumInfo.ForumGroupId, -1, -1, -1, string.Empty, 1, -1, SocialGroupId);
                        sForumURL = ctlUtils.BuildUrl(TabId, ModuleId, ForumInfo.ForumGroup.PrefixURL, ForumInfo.PrefixURL, ForumInfo.ForumGroupId, ForumInfo.ForumID, -1, -1, string.Empty, 1, -1, SocialGroupId);
                        if (bView)
                        {
                            try
                            {
                                bAllowRSS = ForumInfo.AllowRSS;
                            }
                            catch
                            {
                                bAllowRSS = false;
                            }

                            if (bRead == false)
                            {
                                bAllowRSS = false;
                            }
                            TopicRowCount = Convert.ToInt32(drForum["TopicRowCount"]);
                            if (UserId > 0)
                            {
                                IsSubscribedForum = new DotNetNuke.Modules.ActiveForums.Controllers.SubscriptionController().Subscribed(PortalId, ForumModuleId, UserId, ForumId);
                            }
                            if (MainSettings.UseSkinBreadCrumb)
                            {
                                Environment.UpdateBreadCrumb(Page.Controls, "<a href=\"" + sGroupURL + "\">" + ForumInfo.ForumGroup.GroupName + "</a>");
                                TopicsTemplate = TopicsTemplate.Replace("<div class=\"afcrumb\">[FORUMMAINLINK] > [FORUMGROUPLINK]</div>", string.Empty);
                            }
                            if (TopicsTemplate.Contains("[META]"))
                            {
                                MetaTemplate = TemplateUtils.GetTemplateSection(TopicsTemplate, "[META]", "[/META]");
                                TopicsTemplate = TemplateUtils.ReplaceSubSection(TopicsTemplate, string.Empty, "[META]", "[/META]");
                            }
                            //Parse Meta Template
                            if (!(string.IsNullOrEmpty(MetaTemplate)))
                            {
                                MetaTemplate = DotNetNuke.Modules.ActiveForums.Controllers.TokenController.ReplaceForumTokens(new StringBuilder(MetaTemplate), ForumInfo, PortalSettings, MainSettings, new Services.URLNavigator().NavigationManager(), UserInfo, TabId, ForumModuleId, CurrentUserType).ToString();
                                MetaTemplate = DotNetNuke.Modules.ActiveForums.Controllers.TokenController.ReplaceModuleTokens(new StringBuilder(MetaTemplate), PortalSettings, MainSettings, UserInfo, TabId, ForumModuleId).ToString();

                                MetaTemplate = MetaTemplate.Replace("[TAGS]", string.Empty);
                                if (MetaTemplate.Contains("[TOPICSUBJECT:"))
                                {
                                    string pattern = "(\\[TOPICSUBJECT:(.+?)\\])";
                                    Regex regExp = new Regex(pattern);
                                    MatchCollection matches = null;
                                    matches = regExp.Matches(MetaTemplate);
                                    foreach (Match m in matches)
                                    {
                                        MetaTemplate = MetaTemplate.Replace(m.Value, string.Empty);
                                    }
                                }
                                MetaTemplate = MetaTemplate.Replace("[TOPICSUBJECT]", string.Empty);
                                if (MetaTemplate.Contains("[BODY:"))
                                {
                                    string pattern = "(\\[BODY:(.+?)\\])";
                                    Regex regExp = new Regex(pattern);
                                    MatchCollection matches = null;
                                    matches = regExp.Matches(MetaTemplate);
                                    foreach (Match m in matches)
                                    {
                                        int iLen = Convert.ToInt32(m.Groups[2].Value);
                                        if (ForumInfo.ForumDesc.Length > iLen)
                                        {
                                            MetaTemplate = MetaTemplate.Replace(m.Value, ForumInfo.ForumDesc.Substring(0, iLen) + "...");
                                        }
                                        else
                                        {
                                            MetaTemplate = MetaTemplate.Replace(m.Value, ForumInfo.ForumDesc);
                                        }
                                    }
                                }
                                MetaTemplate = MetaTemplate.Replace("[BODY]", Utilities.StripHTMLTag(ForumInfo.ForumDesc));

                                MetaTitle = TemplateUtils.GetTemplateSection(MetaTemplate, "[TITLE]", "[/TITLE]").Replace("[TITLE]", string.Empty).Replace("[/TITLE]", string.Empty);
                                MetaTitle = MetaTitle.TruncateAtWord(SEOConstants.MaxMetaTitleLength);
                                MetaDescription = TemplateUtils.GetTemplateSection(MetaTemplate, "[DESCRIPTION]", "[/DESCRIPTION]").Replace("[DESCRIPTION]", string.Empty).Replace("[/DESCRIPTION]", string.Empty);
                                MetaDescription = MetaDescription.TruncateAtWord(SEOConstants.MaxMetaDescriptionLength);
                                MetaKeywords = TemplateUtils.GetTemplateSection(MetaTemplate, "[KEYWORDS]", "[/KEYWORDS]").Replace("[KEYWORDS]", string.Empty).Replace("[/KEYWORDS]", string.Empty);
                            }
                            BindTopics(TopicsTemplate);
                        }
                        else
                        {
                            Response.Redirect(NavigateUrl(TabId), true);
                        }

                        try
                        {
                            DotNetNuke.Framework.CDefault tempVar = this.BasePage;
                            Environment.UpdateMeta(ref tempVar, MetaTitle, MetaDescription, MetaKeywords);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex.Message, ex);
                            if (ex.InnerException != null)
                            {
                                Logger.Error(ex.InnerException.Message, ex.InnerException);
                            }
                        }
                    }
                    else
                    {
                        Response.Redirect(NavigateUrl(TabId), true);
                    }
                }
                else
                {
                    string fs = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.GetForumsForUser(ForumUser.UserRoles, PortalId, ForumModuleId, "CanEdit");
                    if (!(string.IsNullOrEmpty(fs)))
                    {
                        bModEdit = true;
                    }
                    TopicsTemplate = ParseControls(TopicsTemplate);
                    TopicsTemplate = Utilities.LocalizeControl(TopicsTemplate);
                    this.Controls.Add(this.ParseControl(TopicsTemplate));
                    LinkControls(this.Controls);
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc.Message, exc);
                if (exc.InnerException != null)
                {
                    Logger.Error(exc.InnerException.Message, exc.InnerException);
                }
                RenderMessage("[RESX:Error:LoadingTopics]", exc.Message, exc);
            }
        }

        private void BindTopics(string TopicsTemplate)
        {
            string sOutput = TopicsTemplate;
            string subTemplate = string.Empty;

            //Subforum Template

            if (sOutput.Contains("[SUBFORUMS]"))
            {
                if (dtSubForums.Rows.Count > 0)
                {
                    subTemplate = TemplateUtils.GetTemplateSection(sOutput, "[SUBFORUMS]", "[/SUBFORUMS]");
                }
                sOutput = TemplateUtils.ReplaceSubSection(sOutput, "<asp:placeholder id=\"plhSubForums\" runat=\"server\" />", "[SUBFORUMS]", "[/SUBFORUMS]");
            }

            //Parse Common Controls
            sOutput = ParseControls(sOutput);
            //Parse Topics
            sOutput = ParseTopics(sOutput, dtTopics, "TOPICS");
            //Parse Announce
            string sAnnounce = TemplateUtils.GetTemplateSection(sOutput, "[ANNOUNCEMENTS]", "[/ANNOUNCEMENTS]");
            if (dtAnnounce != null)
            {
                if (dtAnnounce.Rows.Count > 0)
                {
                    sAnnounce = ParseTopics(sAnnounce, dtAnnounce, "ANNOUNCEMENT");
                }
                else
                {
                    sAnnounce = string.Empty;
                }
            }
            else
            {
                sAnnounce = string.Empty;
            }
            sOutput = TemplateUtils.ReplaceSubSection(sOutput, sAnnounce, "[ANNOUNCEMENTS]", "[/ANNOUNCEMENTS]");

            sOutput = Utilities.LocalizeControl(sOutput);
            this.Controls.Add(this.ParseControl(sOutput));
            BuildPager();

            PlaceHolder plh = (PlaceHolder)(this.FindControl("plhQuickJump"));
            if (plh != null)
            {
                ctlForumJump = new af_quickjump();
                ctlForumJump.ForumModuleId = ForumModuleId;
                ctlForumJump.ModuleConfiguration = this.ModuleConfiguration;
                ctlForumJump.ForumId = ForumId;
                ctlForumJump.ModuleId = ModuleId;
                if (ForumId > 0)
                {
                    ctlForumJump.ForumInfo = ForumInfo;
                }
                plh.Controls.Add(ctlForumJump);
            }

            plh = (PlaceHolder)(this.FindControl("plhSubForums"));
            if (plh != null)
            {
                ctlForumSubs = (ForumView)(LoadControl(typeof(ForumView), null));
                ctlForumSubs.ModuleConfiguration = this.ModuleConfiguration;
                ctlForumSubs.ForumId = ForumId;
                ctlForumSubs.Forums = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(ForumId, ForumModuleId).SubForums;
                ctlForumSubs.ForumTabId = ForumTabId;
                ctlForumSubs.ForumModuleId = ForumModuleId;
                ctlForumSubs.SubsOnly = true;
                ctlForumSubs.DisplayTemplate = subTemplate;
                if (ForumId > 0)
                {
                    ctlForumSubs.ForumInfo = ForumInfo;
                }
                plh.Controls.Add(ctlForumSubs);
            }
        }
        private void LinkControls(ControlCollection ctrls)
        {
            foreach (Control ctrl in ctrls)
            {
                if ((ctrl) is ForumBase)
                {
                    ((ForumBase)ctrl).ModuleConfiguration = this.ModuleConfiguration;

                }
                if (ctrl.Controls.Count > 0)
                {
                    LinkControls(ctrl.Controls);
                }
            }
        }
        private string ParseControls(string Template)
        {
            string MyTheme = MainSettings.Theme;
            string sOutput = Template;
            sOutput = "<%@ Register TagPrefix=\"ac\" Namespace=\"DotNetNuke.Modules.ActiveForums.Controls\" Assembly=\"DotNetNuke.Modules.ActiveForums\" %>" + sOutput;

            //Forum Drop Downlist
            sOutput = sOutput.Replace("[JUMPTO]", "<asp:placeholder id=\"plhQuickJump\" runat=\"server\" />");
            //Tag Cloud
            sOutput = sOutput.Replace("[AF:CONTROLS:TAGCLOUD]", "<ac:tagcloud ModuleId=\"" + ModuleId + "\" PortalId=\"" + PortalId + "\" tabid=\"" + TabId + "\" runat=\"server\" />");

           
            //Forum Subscription Control
            if (bSubscribe)
            {
                Controls.ToggleSubscribe subControl = new Controls.ToggleSubscribe(ForumModuleId, ForumId, -1, 0);
                subControl.Checked = IsSubscribedForum;
                subControl.Text = "[RESX:Subscribe]";
                sOutput = sOutput.Replace("[FORUMSUBSCRIBE]", subControl.Render());
            }
            else
            {
                sOutput = sOutput.Replace("[FORUMSUBSCRIBE]", string.Empty);
            }
            if (Request.IsAuthenticated)
            {
                sOutput = sOutput.Replace("[MARKFORUMREAD]", "<am:MarkForumRead EnableViewState=\"False\" id=\"amMarkForumRead\" MID=\"" + ForumModuleId + "\" runat=\"server\" />");
            }
            else
            {
                sOutput = sOutput.Replace("[MARKFORUMREAD]", string.Empty);
            }

            if (CanCreate)
            {
                string[] Params = { };
                if (SocialGroupId <= 0)
                {
                    Params = new string[] { ParamKeys.ViewType + "=post", ParamKeys.ForumId + "=" + ForumId };
                }
                else
                {
                    Params = new string[] { ParamKeys.ViewType + "=post", ParamKeys.ForumId + "=" + ForumId, "GroupId=" + SocialGroupId, };
                }
                sOutput = sOutput.Replace("[ADDTOPIC]", "<a href=\"" + NavigateUrl(TabId, "", Params) + "\" class=\"dnnPrimaryAction\">[RESX:AddTopic]</a>");
            }
            else
            {
                sOutput = sOutput.Replace("[ADDTOPIC]", "<div class=\"amnormal\">[RESX:NotAuthorizedTopic]</div>");
            }
            sOutput = sOutput.Replace("[ADDPOLL]", string.Empty);
            string Url = null;
            if (bAllowRSS)
            {

                Url = DotNetNuke.Common.Globals.AddHTTP(DotNetNuke.Common.Globals.GetDomainName(Request)) + "/DesktopModules/ActiveForums/feeds.aspx?portalid=" + PortalId + "&forumid=" + ForumId + "&tabid=" + TabId + "&moduleid=" + ForumModuleId;
                if (SocialGroupId > 0)
                {
                    Url += "&GroupId=" + SocialGroupId;
                }
                sOutput = sOutput.Replace("[RSSLINK]", "<a href=\"" + Url + "\"><img src=\"" + MainSettings.ThemeLocation + "/images/rss.png\" border=\"0\" alt=\"[RESX:RSS]\" /></a>");
            }
            else
            {
                sOutput = sOutput.Replace("[RSSLINK]", string.Empty);
            }
            if (Request.IsAuthenticated)
            {
                Url = NavigateUrl(TabId, "", new string[] { ParamKeys.ViewType + "=sendto", ParamKeys.ForumId + "=" + ForumId, ParamKeys.TopicId + "=" + TopicId });
                sOutput = sOutput.Replace("[AF:CONTROL:EMAIL]", "<a href=\"" + Url + "\" rel=\"nofollow\"><img src=\"" + MainSettings.ThemeLocation + "/images/email16.png\" border=\"0\" alt=\"[RESX:EmailThis]\" /></a>");
            }
            else
            {
                sOutput = sOutput.Replace("[AF:CONTROL:EMAIL]", string.Empty);
            }
            sOutput = sOutput.Replace("[MINISEARCH]", "<am:MiniSearch  EnableViewState=\"False\" id=\"amMiniSearch\" MID=\"" + ModuleId + "\" TID=\"" + TabId + "\" FID=\"" + ForumId + "\" runat=\"server\" />");
            sOutput = sOutput.Replace("[PAGER1]", "<am:pagernav id=\"Pager1\"  EnableViewState=\"False\" runat=\"server\" />");
            sOutput = sOutput.Replace("[PAGER2]", "<am:pagernav id=\"Pager2\" runat=\"server\" EnableViewState=\"False\" />");


            StringBuilder stringBuilder = new StringBuilder(sOutput);
            stringBuilder = DotNetNuke.Modules.ActiveForums.Controllers.TokenController.ReplaceForumTokens(stringBuilder, ForumInfo, PortalSettings, MainSettings, new Services.URLNavigator().NavigationManager(), UserInfo, TabId, ForumModuleId, CurrentUserType);
            stringBuilder = DotNetNuke.Modules.ActiveForums.Controllers.TokenController.ReplaceModuleTokens(stringBuilder, PortalSettings, MainSettings, UserInfo, TabId, ForumModuleId);

            if (bModDelete)
            {
                sOutput = sOutput.Replace("[ACTIONS:DELETE]", "<a href=\"javascript:void(0)\" onclick=\"amaf_modDel(" + ModuleId + "," + ForumId + ",[TOPICID]);\" style=\"vertical-align:middle;\" title=\"[RESX:DeleteTopic]\" /><i class=\"fa fa-trash-o fa-fw fa-blue\"></i></a>");
            }
            else
            {
                sOutput = sOutput.Replace("[ACTIONS:DELETE]", string.Empty);
            }
            if (bModEdit)
            {
                string[] EditParams = { ParamKeys.ViewType + "=post", "action=te", ParamKeys.ForumId + "=" + ForumId, ParamKeys.TopicId + "=0-0" };
                sOutput = sOutput.Replace("[ACTIONS:EDIT]", "<a title=\"[RESX:EditTopic]\" href=\"" + NavigateUrl(TabId, "", EditParams) + "\"><i class=\"fa fa-pencil-square-o fa-fw fa-blue\"></i></a>");
                sOutput = sOutput.Replace("0-0", "[TOPICID]");
                sOutput = sOutput.Replace("[AF:QUICKEDITLINK]", "<a href=\"javascript:void(0)\" title=\"[RESX:TopicQuickEdit]\" onclick=\"amaf_quickEdit(" + ModuleId + "," + ForumId + ",[TOPICID]);\"><i class=\"fa fa-cog fa-fw fa-blue\"></i></a>");
            }
            else
            {
                sOutput = sOutput.Replace("[AF:QUICKEDITLINK]", string.Empty);
                sOutput = sOutput.Replace("[ACTIONS:EDIT]", string.Empty);
            }
            if (bModMove)
            {
                sOutput = sOutput.Replace("[ACTIONS:MOVE]", "<a href=\"javascript:void(0)\" onclick=\"javascript:amaf_openMove(" + ModuleId + "," + ForumId + ",[TOPICID]);\" title=\"[RESX:MoveTopic]\" style=\"vertical-align:middle;\" /><i class=\"fa fa-exchange fa-rotate-90 fa-blue\"></i></a>");
            }
            else
            {
                sOutput = sOutput.Replace("[ACTIONS:MOVE]", string.Empty);
            }
            if (bModLock)
            {
                sOutput = sOutput.Replace("[ACTIONS:LOCK]", "<a href=\"javascript:void(0)\" class=\"dcf-topic-lock-outer\" onclick=\"javascript:if(confirm('[RESX:Confirm:Lock]')){amaf_Lock(" + ModuleId + "," +ForumId + ",[TOPICID]);};\" title=\"[RESX:LockTopic]\" style=\"vertical-align:middle;\"><i class=\"fa fa-lock fa-fw fa-blue dcf-topic-lock-inner\"></i></a>");
            }
            else
            {
                sOutput = sOutput.Replace("[ACTIONS:LOCK]", string.Empty);
            }
            if (bModPin)
            {
                sOutput = sOutput.Replace("[ACTIONS:PIN]", "<a href=\"javascript:void(0)\" class=\"dcf-topic-pin-outer\" onclick=\"javascript:if(confirm('[RESX:Confirm:Pin]')){amaf_Pin(" + ModuleId + "," + ForumId + ",[TOPICID]);};\" title=\"[RESX:PinTopic]\" style=\"vertical-align:middle;\"><i class=\"fa fa-thumb-tack fa-fw fa-blue dcf-topic-pin-pin dcf-topic-pin-inner\"></i></a>");
            }
            else
            {
                sOutput = sOutput.Replace("[ACTIONS:PIN]", string.Empty);
            }


            return sOutput;
        }
        private string ParseTopics(string Template, DataTable Topics, string Section)
        {
            if (Topics?.Rows?.Count < 1)
            {
                return TemplateUtils.ReplaceSubSection(Template, string.Empty, "[" + Section + "]", "[/" + Section + "]");
            }
            string template = TemplateUtils.GetTemplateSection(Template, "[" + Section + "]", "[/" + Section + "]");
            string sTopics = string.Empty;

            int rowcount = 0;
            foreach (DataRow drTopic in Topics.Rows)
            {
                string topicTemplate = template;

                /* pull in as much data as needed from stored procedure to populate object model; eventually want to move this part to DAL2 */
                var topicInfo = new DotNetNuke.Modules.ActiveForums.Entities.TopicInfo
                {
                    Forum = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(Convert.ToInt32(drTopic["ForumId"]), ForumModuleId),
                    TopicId = Convert.ToInt32(drTopic["TopicId"]),
                    TopicType = (TopicTypes)Enum.Parse(typeof(TopicTypes), Convert.ToInt32(drTopic["TopicType"]).ToString()),
                    Content = new DotNetNuke.Modules.ActiveForums.Entities.ContentInfo
                    {
                        Subject = HttpUtility.HtmlDecode(Convert.ToString(drTopic["Subject"])),
                        Summary = HttpUtility.HtmlDecode(Convert.ToString(drTopic["Summary"])),
                        DateCreated = Convert.ToDateTime(drTopic["DateCreated"]),
                        Body = HttpUtility.HtmlDecode(Convert.ToString(drTopic["Body"])),
                        AuthorId = Convert.ToInt32(drTopic["AuthorId"]),
                        AuthorName = Convert.ToString(drTopic["AuthorName"]).ToString().Replace("&amp;#", "&#"),
                    },
                    LastReplyId = Convert.ToInt32(drTopic["LastReplyId"]),
                    ReplyCount = Convert.ToInt32(drTopic["ReplyCount"]),
                    ViewCount = Convert.ToInt32(drTopic["ViewCount"]),
                    StatusId = Convert.ToInt32(drTopic["StatusId"]),
                    IsLocked = Convert.ToBoolean(drTopic["IsLocked"]),
                    IsPinned = Convert.ToBoolean(drTopic["IsPinned"]),
                    Rating = Convert.ToInt32(drTopic["TopicRating"]),
                    TopicUrl = drTopic["TopicURL"].ToString(),
                    TopicData = drTopic["TopicData"].ToString(),
                    LastReply = new DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo
                    {
                        ReplyId = Convert.ToInt32(drTopic["LastReplyId"]),
                        Content = new DotNetNuke.Modules.ActiveForums.Entities.ContentInfo
                        {
                            Subject = HttpUtility.HtmlDecode(Convert.ToString(drTopic["LastReplySubject"])),
                            Summary = HttpUtility.HtmlDecode(Convert.ToString(drTopic["LastReplySummary"])),
                            DateCreated = Convert.ToDateTime(drTopic["LastReplyDate"]),
                            AuthorId = Convert.ToInt32(drTopic["LastReplyAuthorId"]),
                            AuthorName = Convert.ToString(drTopic["LastReplyAuthorName"]).ToString().Replace("&amp;#", "&#"),
                        }
                    }
                };
                if (!(string.IsNullOrEmpty(topicInfo.Content.Summary)) && (!(Utilities.HasHTML(topicInfo.Content.Summary))))
                {
                    topicInfo.Content.Summary = topicInfo.Content.Summary.Replace(System.Environment.NewLine, "<br />");
                }
                topicInfo.Author.FirstName = topicInfo.Author.FirstName.Replace("&amp;#", "&#");
                topicInfo.Author.LastName = topicInfo.Author.LastName.Replace("&amp;#", "&#");
                topicInfo.Author.DisplayName = topicInfo.Author.DisplayName.Replace("&amp;#", "&#");
                topicInfo.LastReplyAuthor.FirstName = topicInfo.Author.FirstName.Replace("&amp;#", "&#");
                topicInfo.LastReplyAuthor.LastName = topicInfo.Author.LastName.Replace("&amp;#", "&#");
                topicInfo.LastReplyAuthor.DisplayName = topicInfo.Author.DisplayName.Replace("&amp;#", "&#");

                int UserLastTopicRead = Convert.ToInt32(drTopic["UserLastTopicRead"]);
                int UserLastReplyRead = Convert.ToInt32(drTopic["UserLastReplyRead"]);

                if (string.IsNullOrEmpty(topicInfo.LastReply.Content.Subject))
                {
                    topicInfo.LastReply.Content.Subject = "RE: " + topicInfo.Content.Subject;
                }
                if (topicInfo.LastReply.Content.Summary == "")
                {
                    topicInfo.LastReply.Content.Summary = topicInfo.Content.Summary;
                }
                if (string.IsNullOrEmpty(topicInfo.TopicData))
                {
                    topicTemplate = TemplateUtils.ReplaceSubSection(topicTemplate, string.Empty, "[AF:PROPERTIES]", "[/AF:PROPERTIES]");
                }
                else
                {
                    string sPropTemplate = TemplateUtils.GetTemplateSection(topicTemplate, "[AF:PROPERTIES]", "[/AF:PROPERTIES]");
                    string sProps = string.Empty;
                    var pl = DotNetNuke.Modules.ActiveForums.Controllers.TopicPropertyController.Deserialize(topicInfo.TopicData);
                    foreach (var p in pl)
                    {
                        string tmp = sPropTemplate;
                        var pName = HttpUtility.HtmlDecode(p.Name);
                        var pValue = HttpUtility.HtmlDecode(p.Value); tmp = tmp.Replace("[AF:PROPERTY:LABEL]", Utilities.GetSharedResource("[RESX:" + pName + "]"));
                        tmp = tmp.Replace("[AF:PROPERTY:VALUE]", pValue);
                        topicTemplate = topicTemplate.Replace("[AF:PROPERTY:" + pName + ":LABEL]", Utilities.GetSharedResource("[RESX:" + pName + "]"));
                        topicTemplate = topicTemplate.Replace("[AF:PROPERTY:" + pName + ":VALUE]", pValue);
                        string pValueKey = string.Empty;
                        if (!(string.IsNullOrEmpty(pValue)))
                        {
                            pValueKey = Utilities.CleanName(pValue).ToLowerInvariant();
                        }
                        topicTemplate = topicTemplate.Replace("[AF:PROPERTY:" + pName + ":VALUEKEY]", pValueKey);
                        sProps += tmp;
                    }
                    topicTemplate = TemplateUtils.ReplaceSubSection(topicTemplate, sProps, "[AF:PROPERTIES]", "[/AF:PROPERTIES]");
                }
                StringBuilder stringBuilder = new StringBuilder(topicTemplate);
                if (!bRead)
                {
                    stringBuilder.Replace("[LASTPOSTDATE]", string.Empty);
                    stringBuilder = DotNetNuke.Modules.ActiveForums.Controllers.TokenController.RemovePrefixedToken(stringBuilder, "[LASTPOSTSUBJECT");
                    stringBuilder.Replace("[BODYTITLE]", string.Empty);
                    stringBuilder.Replace("[BODY]", string.Empty);
                    stringBuilder.Replace("[AF:ICONLINK:LASTREPLY]", string.Empty);
                    stringBuilder.Replace("[AF:URL:LASTREPLY]", string.Empty);
                    if (topicInfo.LastReplyId < 1)
                    {
                        stringBuilder.Replace("[AF:ICONLINK:LASTREPLY]", string.Empty);
                        stringBuilder.Replace("[AF:URL:LASTREPLY]", string.Empty);
                        stringBuilder.Replace("[AF:UI:MINIPAGER]", string.Empty);
                    }
                }
                if (UserId == -1 || topicInfo.LastReplyId == 0)
                {
                    stringBuilder.Replace("[AF:ICONLINK:LASTREAD]", string.Empty);
                    stringBuilder.Replace("[AF:URL:LASTREAD]", string.Empty);
                }
                if ((UserLastTopicRead >= TopicId || UserLastTopicRead == 0) & (UserLastReplyRead >= topicInfo.LastReplyId || UserLastReplyRead == 0))
                {
                    stringBuilder.Replace("[AF:ICONLINK:LASTREAD]", string.Empty);
                    stringBuilder.Replace("[AF:URL:LASTREAD]", string.Empty);
                }
                stringBuilder = DotNetNuke.Modules.ActiveForums.Controllers.TokenController.ReplaceTopicTokens(stringBuilder, topicInfo, PortalSettings, MainSettings, new Services.URLNavigator().NavigationManager(),  UserInfo, ForumUser, UserLastTopicRead, UserLastReplyRead, TabId, ForumModuleId);
                    
                stringBuilder.Replace("[ROWCSS]", GetRowCSS(UserLastTopicRead, UserLastReplyRead, TopicId, topicInfo.LastReplyId, rowcount));
                stringBuilder.Replace("[AF:UI:MINIPAGER]", GetSubPages(TabId, topicInfo.ReplyCount, ForumId, TopicId));
                    
                sTopics += stringBuilder.ToString(); ;
                rowcount += 1;
            }
            return TemplateUtils.ReplaceSubSection(Template, sTopics, "[" + Section + "]", "[/" + Section + "]");
        }
        private void BuildPager()
        {
            if (TopicRowCount > 0)
            {
                DotNetNuke.Modules.ActiveForums.Controls.PagerNav Pager1 = null;
                Pager1 = (DotNetNuke.Modules.ActiveForums.Controls.PagerNav)(this.FindControl("Pager1"));

                DotNetNuke.Modules.ActiveForums.Controls.PagerNav Pager2 = null;
                object obj = this.FindControl("Pager2");
                if (obj != null)
                {
                    Pager2 = (DotNetNuke.Modules.ActiveForums.Controls.PagerNav)obj;
                }

                int intPages = 0;
                if (Pager1 != null)
                {
                    intPages = Convert.ToInt32(System.Math.Ceiling(TopicRowCount / (double)PageSize));
                    Pager1.PageCount = intPages;
                    Pager1.PageMode = PagerNav.Mode.Links;
                    Pager1.BaseURL = URL.ForumLink(TabId, ForumInfo);
                    Pager1.CurrentPage = PageId;
                    Pager1.TabID = Convert.ToInt32(Request.Params["TabId"]);
                    Pager1.ForumID = ForumId;
                    Pager1.UseShortUrls = MainSettings.UseShortUrls;
                    Pager1.PageText = Utilities.GetSharedResource("[RESX:Page]");
                    Pager1.OfText = Utilities.GetSharedResource("[RESX:PageOf]");
                    Pager1.View = Views.Topics;
                    if (Request.Params[ParamKeys.Sort] != null)
                    {
                        string[] Params = { $"{ParamKeys.Sort}={Request.Params[ParamKeys.Sort]}", "afcol=" + Request.Params["afcol"] };
                        Pager1.Params = Params;
                        if (Pager2 != null)
                        {
                            Pager2.Params = Params;
                        }

                    }
                }


                if (Pager2 != null)
                {
                    Pager2.PageMode = Modules.ActiveForums.Controls.PagerNav.Mode.Links;
                    Pager2.BaseURL = URL.ForumLink(TabId, ForumInfo);
                    Pager2.UseShortUrls = MainSettings.UseShortUrls;
                    Pager2.PageCount = intPages;
                    Pager2.CurrentPage = PageId;
                    Pager2.TabID = Convert.ToInt32(Request.Params["TabId"]);
                    Pager2.ForumID = ForumId;
                    Pager2.PageText = Utilities.GetSharedResource("[RESX:Page]");
                    Pager2.OfText = Utilities.GetSharedResource("[RESX:PageOf]");
                    Pager2.View = Views.Topics;
                }
            }


        }
        private string GetSubPages(int TabID, int Replies, int ForumID, int PostID)
        {
            int i = 0;
            string sOut = "";

            if (Replies + 1 > PageSize)
            {
                sOut = "<div class=\"afpagermini\">(<img src=\"" + MainSettings.ThemeLocation + "/images/icon_multipage.png\" alt=\"[RESX:MultiPageTopic]\" style=\"vertical-align:middle;\" />";
                //Jump to pages
                int intPostPages = 0;
                intPostPages = Convert.ToInt32(System.Math.Ceiling((double)(Replies + 1) / PageSize));
                if (intPostPages > 3)
                {
                    for (i = 1; i <= 3; i++)
                    {
                        if (UseAjax)
                        {
                            var @params = new List<string> { ParamKeys.ForumId + "=" + ForumID, ParamKeys.TopicId + "=" + PostID, ParamKeys.ViewType + "=" + Views.Topic };
                            if (MainSettings.UseShortUrls)
                            {
                                @params = new List<string> { ParamKeys.TopicId + "=" + PostID };
                            }
                            if (i > 1)
                            {
                                @params.Add(ParamKeys.PageJumpId + "=" + i);
                            }
                            sOut += "<a href=\"" + NavigateUrl(TabID, "", @params.ToArray()) + "\">" + i + "</a>&nbsp;";
                        }
                        else
                        {
                            var @params = new List<string> { ParamKeys.ForumId + "=" + ForumID, ParamKeys.TopicId + "=" + PostID, ParamKeys.ViewType + "=" + Views.Topic };
                            if (MainSettings.UseShortUrls)
                            {
                                @params = new List<string> { ParamKeys.TopicId + "=" + PostID };
                            }
                            if (i > 1)
                            {
                                @params.Add(ParamKeys.PageId + "=" + i);
                            }
                            sOut += "<a href=\"" + NavigateUrl(TabID, "", @params.ToArray()) + "\">" + i + "</a>&nbsp;";
                        }

                    }
                    if (intPostPages > 4)
                    {
                        sOut += "...&nbsp;";
                    }
                    if (UseAjax)
                    {
                        var @params = new List<string> { ParamKeys.ForumId + "=" + ForumID, ParamKeys.TopicId + "=" + PostID, ParamKeys.ViewType + "=" + Views.Topic };
                        if (MainSettings.UseShortUrls)
                        {
                            @params = new List<string> { ParamKeys.TopicId + "=" + PostID };
                        }
                        if (i > 1)
                        {
                            @params.Add(ParamKeys.PageJumpId + "=" + i);
                        }
                        sOut += "<a href=\"" + NavigateUrl(TabID, "", @params.ToArray()) + "\">" + i + "</a>&nbsp;";

                    }
                    else
                    {
                        var @params = new List<string> { ParamKeys.ForumId + "=" + ForumID, ParamKeys.TopicId + "=" + PostID, ParamKeys.ViewType + "=" + Views.Topic };
                        if (MainSettings.UseShortUrls)
                        {
                            @params = new List<string> { ParamKeys.TopicId + "=" + PostID };
                        }
                        if (i > 1)
                        {
                            @params.Add(ParamKeys.PageId + "=" + i);
                        }
                        sOut += "<a href=\"" + NavigateUrl(TabID, "", @params.ToArray()) + "\">" + i + "</a>&nbsp;";
                    }

                }
                else
                {
                    for (i = 1; i <= intPostPages; i++)
                    {
                        if (UseAjax)
                        {
                            var @params = new List<string> { ParamKeys.ForumId + "=" + ForumID, ParamKeys.TopicId + "=" + PostID, ParamKeys.ViewType + "=" + Views.Topic };
                            if (MainSettings.UseShortUrls)
                            {
                                @params = new List<string> { ParamKeys.TopicId + "=" + PostID };
                            }
                            if (i > 1)
                            {
                                @params.Add(ParamKeys.PageJumpId + "=" + i);
                            }
                            sOut += "<a href=\"" + NavigateUrl(TabID, "", @params.ToArray()) + "\">" + i + "</a>&nbsp;";
                        }
                        else
                        {
                            var @params = new List<string> { ParamKeys.ForumId + "=" + ForumID, ParamKeys.TopicId + "=" + PostID, ParamKeys.ViewType + "=" + Views.Topic };
                            if (MainSettings.UseShortUrls)
                            {
                                @params = new List<string> { ParamKeys.TopicId + "=" + PostID };
                            }
                            if (i > 1)
                            {
                                @params.Add(ParamKeys.PageId + "=" + i);
                            }
                            sOut += "<a href=\"" + NavigateUrl(TabID, "", @params.ToArray()) + "\">" + i + "</a>&nbsp;";
                        }

                    }
                }

                sOut += ")</div>";
            }
            return sOut;
        }
        private string GetRowCSS(int LastTopicRead, int LastReplyRead, int TopicId, int ReplyId, int RowCount)
        {
            bool isRead = false;
            if (LastTopicRead >= TopicId && LastReplyRead >= ReplyId)
            {
                isRead = true;
            }
            if (!Request.IsAuthenticated)
            {
                isRead = true;
            }
            if (isRead == true)
            {
                if (RowCount % 2 == 0)
                {
                    return "aftopicrow";
                }
                else
                {
                    return "aftopicrowalt";
                }
            }
            else
            {
                if (RowCount % 2 == 0)
                {
                    return "aftopicrownew";
                }
                else
                {
                    return "aftopicrownewalt";
                }
            }
        }

        private static string CheckControls(string template)
        {
            const string tagRegistration = "<%@ Register TagPrefix=\"ac\" Namespace=\"DotNetNuke.Modules.ActiveForums.Controls\" Assembly=\"DotNetNuke.Modules.ActiveForums\" %>";

            if (string.IsNullOrWhiteSpace(template))
                return string.Empty;

            if (!(template.Contains(tagRegistration)))
                template = tagRegistration + template;

            return template;
        }
    }
}

