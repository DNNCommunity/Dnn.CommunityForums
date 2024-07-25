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
namespace DotNetNuke.Modules.ActiveForums.Controls
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Http.Results;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    [DefaultProperty("Text"), ToolboxData("<{0}:ForumView runat=server></{0}:ForumView>")]
    public class ForumView : ForumBase
    {
        private string ForumURL = string.Empty;
        private string ForumPageTitle = string.Empty;

        public bool SubsOnly { get; set; }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use Forums property.")]
        public DataTable ForumTable { get; set; }

        public List<DotNetNuke.Modules.ActiveForums.Entities.ForumInfo> Forums { get; set; }

        public string DisplayTemplate { get; set; } = "";

        public int CurrentUserId { get; set; } = -1;

        protected af_quickjump ctlForumJump = new af_quickjump();
        #region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.AppRelativeVirtualPath = "~/";

            try
            {

                if (this.CurrentUserId == -1)
                {
                    this.CurrentUserId = this.UserId;
                }

                string template = string.Empty;
                try
                {
                    int defaultTemplateId = this.MainSettings.ForumTemplateID;
                    if (this.DefaultForumViewTemplateId >= 0)
                    {
                        defaultTemplateId = this.DefaultForumViewTemplateId;
                    }

                    template = this.BuildForumView();
                }
                catch (Exception ex)
                {
                    DotNetNuke.Services.Exceptions.Exceptions.ProcessModuleLoadException(this, ex);
                }

                if (template != string.Empty)
                {
                    try
                    {
                        if (template.Contains("[TOOLBAR"))
                        {
                            template = template.Replace("[TOOLBAR]", Utilities.BuildToolbar(this.ForumModuleId, this.ForumTabId, this.ModuleId, this.TabId, this.CurrentUserType, HttpContext.Current?.Response?.Cookies["language"]?.Value));
                        }

                        Control tmpCtl = null;
                        try
                        {
                            tmpCtl = this.ParseControl(template);
                        }
                        catch (Exception ex)
                        {
                            DotNetNuke.Services.Exceptions.Exceptions.ProcessModuleLoadException(this, ex);
                        }

                        if (tmpCtl != null)
                        {
                            try
                            {
                                this.Controls.Add(tmpCtl);
                                this.LinkControls(this.Controls);
                                if (!this.SubsOnly)
                                {
                                    var plh = (PlaceHolder)tmpCtl.FindControl("plhQuickJump");
                                    if (plh != null)
                                    {
                                        this.ctlForumJump = new af_quickjump { ForumModuleId = this.ForumModuleId, Forums = this.Forums, ModuleId = this.ModuleId };
                                        plh.Controls.Add(this.ctlForumJump);
                                    }

                                    plh = (PlaceHolder)tmpCtl.FindControl("plhUsersOnline");
                                    if (plh != null)
                                    {
                                        ForumBase ctlWhosOnline;
                                        ctlWhosOnline = (ForumBase)this.LoadControl($"{Globals.ModulePath}controls/af_usersonline.ascx");
                                        ctlWhosOnline.ModuleConfiguration = this.ModuleConfiguration;
                                        plh.Controls.Add(ctlWhosOnline);
                                    }
                                }

                            }
                            catch (Exception ex)
                            {
                                DotNetNuke.Services.Exceptions.Exceptions.ProcessModuleLoadException(this, ex);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        DotNetNuke.Services.Exceptions.Exceptions.ProcessModuleLoadException(this, ex);
                    }
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.ProcessModuleLoadException(this, ex);
            }
        }

        #endregion
        #region Public Methods
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use BuildForumView()")]
        public string BuildForumView(int ForumTemplateId, int CurrentUserId, string ThemePath)
        {
            return this.BuildForumView();
        }

        public string BuildForumView()
        {
            try
            {
                string sTemplate = TemplateCache.GetCachedTemplate(this.ForumModuleId, "ForumView", 0);
                try
                {
                    sTemplate = this.ParseControls(sTemplate);
                }
                catch (Exception ex)
                {
                    DotNetNuke.Services.Exceptions.Exceptions.ProcessModuleLoadException(this, ex);
                    sTemplate = ex.Message;
                }

                if (sTemplate.Contains("[NOTOOLBAR]"))
                {
                    if (HttpContext.Current.Items.Contains("ShowToolbar"))
                    {
                        HttpContext.Current.Items["ShowToolbar"] = false;
                    }
                    else
                    {
                        HttpContext.Current.Items.Add("ShowToolbar", false);
                    }

                    sTemplate = sTemplate.Replace("[NOTOOLBAR]", string.Empty);
                }

                if (sTemplate.Contains("[FORUMS]"))
                {
                    string sGroupSection = string.Empty;
                    string sGroupSectionTemp = TemplateUtils.GetTemplateSection(sTemplate, "[GROUPSECTION]", "[/GROUPSECTION]");
                    string sGroup = TemplateUtils.GetTemplateSection(sTemplate, "[GROUP]", "[/GROUP]");
                    string sForums = string.Empty;
                    string sForumTemp = TemplateUtils.GetTemplateSection(sTemplate, "[FORUMS]", "[/FORUMS]");
                    string tmpGroup = string.Empty;

                    #region "backward compatibilty - remove when removing ForumTable property"
#pragma warning disable CS0618
                    /* this is for backward compatibility -- remove when removing ForumTable property in 10.00.00 */
                    if (this.ForumTable != null)
#pragma warning restore CS0618
                    {
                        this.Forums = new DotNetNuke.Modules.ActiveForums.Entities.ForumCollection();
#pragma warning disable CS0618
                        foreach (DataRow dr in this.ForumTable.DefaultView.ToTable().Rows)
#pragma warning restore CS0618
                        {
                            this.Forums.Add(new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(Utilities.SafeConvertInt(dr["ForumId"]), this.ForumModuleId));
                        }
                    }
                    #endregion

                    if (this.Forums == null)
                    {
                        string cachekey = string.Format(CacheKeys.ForumViewForUser, this.ForumModuleId, this.ForumUser.UserId, this.ForumIds, HttpContext.Current?.Response?.Cookies["language"]?.Value);
                        var obj = DataCache.ContentCacheRetrieve(this.ForumModuleId, cachekey);
                        if (obj == null)
                        {
                            this.Forums = new DotNetNuke.Modules.ActiveForums.Entities.ForumCollection();
                            foreach (string ForumId in this.ForumIds.Split(separator: ";".ToCharArray(), options: StringSplitOptions.RemoveEmptyEntries))
                            {
                                this.Forums.Add(new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(Utilities.SafeConvertInt(ForumId), this.ForumModuleId));
                            }

                            DataCache.ContentCacheStore(this.ForumModuleId, cachekey, this.Forums);
                        }
                        else
                        {
                            this.Forums = (List<DotNetNuke.Modules.ActiveForums.Entities.ForumInfo>)obj;
                        }
                    }

                    this.Forums = this.Forums.OrderBy(f => f.ForumGroup?.SortOrder).ThenBy(f => f.SortOrder).ToList();

                    string sGroupName = (this.ForumGroupId != -1 && this.Forums?.Count > 0) ? this.Forums?.FirstOrDefault().GroupName : string.Empty;
                    string sCrumb = (this.ForumGroupId != -1 && this.Forums?.Count > 0) ? "<div class=\"afcrumb\"><i class=\"fa fa-comments-o fa-grey\"></i>  <a href=\"" + Utilities.NavigateURL(this.TabId) + "\">[RESX:ForumMain]</a>  <i class=\"fa fa-long-arrow-right fa-grey\"></i>  " + sGroupName + "</div>" : string.Empty;

                    if (this.ParentForumId != -1)
                    {
                        sGroupName = this.Forums?.Where(f => f.ForumID == this.ParentForumId).FirstOrDefault().GroupName;
                    }

                    if (this.MainSettings.UseSkinBreadCrumb && this.Forums?.Count > 0 && this.SubsOnly == false && this.ForumGroupId != -1)
                    {
                        Environment.UpdateBreadCrumb(this.Page.Controls, "<a href=\"" + this.NavigateUrl(this.TabId, "", ParamKeys.GroupId + "=" + this.ForumGroupId) + "\">" + sGroupName + "</a>");
                        sTemplate = sTemplate.Replace("<div class=\"afcrumb\">[FORUMMAINLINK] > [FORUMGROUPLINK]</div>", string.Empty);
                        sTemplate = sTemplate.Replace("[BREADCRUMB]", string.Empty);
                    }
                    else
                    {
                        sTemplate = sTemplate.Replace("[BREADCRUMB]", sCrumb);
                    }

                    int iForum = 1;
                    int ForumCount = 0;
                    bool hasForums = false;
                    int tmpGroupCount = 0;
                    if (this.Forums != null)
                    {
                        foreach (var fi in this.Forums.Where(f => !this.SubsOnly || f.ParentForumId > 0).OrderBy(f => f.ForumGroup?.SortOrder).ThenBy(f => f.SortOrder).Take(Globals.ForumCount))
                        {
                            bool canView = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(fi.Security?.View, this.ForumUser.UserRoles);
                            if (this.UserInfo.IsSuperUser || canView || (!fi.ForumGroup.Hidden))
                            {
                                if (tmpGroup != fi.GroupName)
                                {
                                    if (tmpGroupCount < Globals.GroupCount)
                                    {
                                        ForumCount = this.Forums.Count(f => f.ForumGroupId == fi.ForumGroupId);
                                        if (sForums != string.Empty)
                                        {
                                            sGroupSection = TemplateUtils.ReplaceSubSection(sGroupSection, sForums, "[FORUMS]", "[/FORUMS]");
                                            sForums = string.Empty;
                                        }

                                        int GroupId = fi.ForumGroupId;
                                        sGroupSectionTemp = TemplateUtils.GetTemplateSection(sTemplate, "[GROUPSECTION]", "[/GROUPSECTION]");
                                        sGroupSectionTemp = sGroupSectionTemp.Replace("[GROUPNAME]", fi.GroupName);
                                        sGroupSectionTemp = sGroupSectionTemp.Replace("[FORUMGROUPID]", fi.ForumGroupId.ToString());
                                        sGroupSectionTemp = sGroupSectionTemp.Replace("[GROUPCOLLAPSE]", DotNetNuke.Modules.ActiveForums.Injector.InjectCollapsibleOpened(target: $"group{GroupId}", title: Utilities.GetSharedResource("[RESX:ToggleGroup]")));

                                        //any replacements on the group
                                        string sNewGroup = "<div id=\"group" + GroupId + "\" class=\"afgroup\">" + sGroup + "</div>";
                                        sNewGroup = sNewGroup.Replace("[FORUMGROUPID]", fi.ForumGroupId.ToString());
                                        sNewGroup = sNewGroup.Replace("[GROUPNAME]", fi.GroupName);
                                        sGroupSectionTemp = TemplateUtils.ReplaceSubSection(sGroupSectionTemp, sNewGroup, "[GROUP]", "[/GROUP]");
                                        sGroupSection += sGroupSectionTemp;
                                        tmpGroup = fi.GroupName;
                                        tmpGroupCount += 1;
                                        iForum = 1;
                                    }

                                }

                                if (iForum <= Globals.ForumCount)
                                {
                                    if (canView || (!fi.Hidden))
                                    {
                                        sForumTemp = TemplateUtils.GetTemplateSection(sTemplate, "[FORUMS]", "[/FORUMS]");
                                        hasForums = true;
                                        if (fi.ParentForumId == 0 || this.SubsOnly || (this.SubsOnly == false && fi.ParentForumId > 0 && this.Forums.Count == 1))
                                        {
                                            sForumTemp = this.ParseForumRow(sForumTemp, fi, iForum, ForumCount);
                                            iForum += 1;
                                            sForums += sForumTemp;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (hasForums == false && this.SubsOnly)
                    {
                        return string.Empty;
                    }

                    if (sForums != string.Empty)
                    {
                        sGroupSection = TemplateUtils.ReplaceSubSection(sGroupSection, sForums, "[FORUMS]", "[/FORUMS]");
                    }

                    sTemplate = sTemplate.Contains("[GROUPSECTION]") ? TemplateUtils.ReplaceSubSection(sTemplate, sGroupSection, "[GROUPSECTION]", "[/GROUPSECTION]") : sGroupSection;
                    sTemplate = TemplateUtils.ReplaceSubSection(sTemplate, string.Empty, "[FORUMS]", "[/FORUMS]");

                }

                return sTemplate;
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                throw;
            }
        }

        private void LinkControls(ControlCollection ctrls)
        {
            foreach (Control ctrl in ctrls)
            {
                if (ctrl is ForumBase)
                {
                    ((ForumBase)ctrl).ModuleConfiguration = this.ModuleConfiguration;

                }

                if (ctrl.Controls.Count > 0)
                {
                    this.LinkControls(ctrl.Controls);
                }
            }
        }

        private string ParseControls(string Template)
        {
            string sOutput = Template;
            sOutput = sOutput.Replace("[JUMPTO]", "<asp:placeholder id=\"plhQuickJump\" runat=\"server\" />");
            if (sOutput.Contains("[STATISTICS]"))
            {
                sOutput = sOutput.Replace("[STATISTICS]", "<am:Stats id=\"amStats\" MID=\"" + this.ModuleId + "\" PID=\"" + this.PortalId.ToString() + "\" runat=\"server\" />");
            }

            if (sOutput.Contains("[WHOSONLINE]"))
            {
                sOutput = sOutput.Replace("[WHOSONLINE]", this.MainSettings.UsersOnlineEnabled ? "<asp:placeholder id=\"plhUsersOnline\" runat=\"server\" />" : string.Empty);
            }

            sOutput = sOutput.Replace("[PORTALID]", this.PortalId.ToString());
            sOutput = sOutput.Replace("[MODULEID]", this.ModuleId.ToString());
            sOutput = sOutput.Replace("[TABID]", this.TabId.ToString());
            sOutput = sOutput.Replace("[USERID]", this.CurrentUserId.ToString());
            return sOutput;
        }

        private string ParseForumRow(string Template, DotNetNuke.Modules.ActiveForums.Entities.ForumInfo fi, int currForumIndex, int totalForums)
        {

            if (Template.Contains("[SUBFORUMS]") && Template.Contains("[/SUBFORUMS]"))
            {
                Template = this.GetSubForums(Template, fi.ForumID, this.TabId, this.ThemePath);
            }
            else
            {
                Template = Template.Replace("[SUBFORUMS]", this.GetSubForums(Template: string.Empty, ForumId: fi.ForumID, TabId: this.TabId, ThemePath: string.Empty));
            }

            string[] css = null;
            string cssmatch = string.Empty;
            if (Template.Contains("[CSS:"))
            {
                string pattern = "(\\[CSS:.+?\\])";
                if (Regex.IsMatch(Template, pattern))
                {
                    cssmatch = Regex.Match(Template, pattern).Value;
                    css = cssmatch.Split(':'); //0=CSS,1=TopRow, 2=mid rows, 3=lastRow
                }
            }

            if (cssmatch != string.Empty)
            {
                if (currForumIndex == 1)
                {
                    Template = Template.Replace(cssmatch, css[1]);
                }
                else if (currForumIndex > 1 & currForumIndex < totalForums)
                {
                    Template = Template.Replace(cssmatch, css[2]);
                }
                else
                {
                    Template = Template.Replace(cssmatch, css[3].Replace("]", string.Empty));
                }
            }

            bool canView = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(fi.Security.View, this.ForumUser.UserRoles);
            bool canSubscribe = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(fi.Security.Subscribe, this.ForumUser.UserRoles);
            bool canRead = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(fi.Security.Read, this.ForumUser.UserRoles);
            string sIcon = TemplateUtils.ShowIcon(canView, fi.ForumID, this.CurrentUserId, fi.LastPostDateTime, DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Forum_GetLastReadTopicByUser(fi.ForumID, this.CurrentUserId), fi.LastPostID);
            string sIconImage = "<img alt=\"" + fi.ForumName + "\" src=\"" + this.ThemePath + "images/" + sIcon + "\" />";

            if (Template.Contains("[FORUMICON]"))
            {
                Template = Template.Replace("[FORUMICON]", sIconImage);
            }
            else if (Template.Contains("[FORUMICONCSS]"))
            {
                string sFolderCSS = "fa-folder fa-blue";
                switch (sIcon.ToLower())
                {
                    case "folder.png":
                        sFolderCSS = "fa-folder fa-blue";
                        break;
                    case "folder_new.png":
                        sFolderCSS = "fa-folder fa-red";
                        break;
                    case "folder_forbidden.png":
                        sFolderCSS = "fa-folder fa-grey";
                        break;
                    case "folder_closed.png":
                        sFolderCSS = "fa-folder-o fa-grey";
                        break;
                }

                Template = Template.Replace("[FORUMICONCSS]", "<div style=\"height:30px;margin-right:10px;\"><i class=\"fa " + sFolderCSS + " fa-2x\"></i></div>");
            }

            var ctlUtils = new ControlUtils();
            this.ForumURL = ctlUtils.BuildUrl(this.TabId, this.ForumModuleId, fi.ForumGroup.PrefixURL, fi.PrefixURL, fi.ForumGroupId, fi.ForumID, -1, string.Empty, -1, -1, string.Empty, 1, -1, this.SocialGroupId);

            Template = Template.Replace("[FORUMNAME]", this.GetForumLink(fi.ForumName, fi.ForumID, this.TabId, canView, this.ForumURL));
            Template = Template.Replace("[FORUMNAMENOLINK]", fi.ForumName);
            Template = Template.Replace("[FORUMID]", fi.ForumID.ToString());
            if (Template.Contains("[RSSLINK]"))
            {
                if (fi.AllowRSS && canRead)
                {
                    string Url;
                    Url = Common.Globals.AddHTTP(Common.Globals.GetDomainName(this.Request)) + "/DesktopModules/ActiveForums/feeds.aspx?portalid=" + this.PortalId + "&forumid=" + fi.ForumID + "&tabid=" + this.TabId + "&moduleid=" + this.ModuleId;
                    Template = Template.Replace("[RSSLINK]", "<a href=\"" + Url + "\" target=\"_blank\"><img src=\"" + this.ThemePath + "images/rss.png\" border=\"0\" alt=\"[RESX:RSS]\" /></a>");
                }
                else
                {
                    Template = Template.Replace("[RSSLINK]", "<img src=\"" + this.ThemePath + "images/rss_disabled.png\" border=\"0\" alt=\"[RESX:RSSDisabled]\" />");
                }
            }

            if (Template.Contains("[AF:CONTROL:TOGGLESUBSCRIBE]"))
            {
                if (canSubscribe)
                {
                    bool IsSubscribed = new DotNetNuke.Modules.ActiveForums.Controllers.SubscriptionController().Subscribed(this.PortalId, this.ForumModuleId, this.UserId, this.ForumId);
                    string sAlt = "[RESX:Subscribe]";
                    string sImg = this.ThemePath + "images/email_unchecked.png";
                    if (IsSubscribed)
                    {
                        sImg = this.ThemePath + "images/email_checked.png";
                    }

                    var subControl = new ToggleSubscribe(this.ForumModuleId, fi.ForumID, -1, 0);
                    subControl.Checked = IsSubscribed;
                    subControl.DisplayMode = 1;
                    subControl.UserId = this.CurrentUserId;
                    subControl.ImageURL = sImg;
                    subControl.Text = "[RESX:Subscribe]";

                    Template = Template.Replace("[AF:CONTROL:TOGGLESUBSCRIBE]", subControl.Render());
                }
                else
                {
                    Template = Template.Replace("[AF:CONTROL:TOGGLESUBSCRIBE]", "<img src=\"" + this.ThemePath + "email_disabled.png\" border=\"0\" alt=\"[RESX:ForumSubscribe:Disabled]\" />");
                }
            }

            Template = canRead ? Template.Replace("[AF:CONTROL:ADDFAVORITE]", "<a href=\"javascript:afAddBookmark('" + fi.ForumName + "','" + this.ForumURL + "');\"><img src=\"" + this.ThemePath + "images/favorites16_add.png\" border=\"0\" alt=\"[RESX:AddToFavorites]\" /></a>") : Template.Replace("[AF:CONTROL:ADDFAVORITE]", string.Empty);
            if (Template.Contains("[AF:CONTROL:ADDTHIS"))
            {
                int inStart = Template.IndexOf("[AF:CONTROL:ADDTHIS", 0) + 1 + 19;
                int inEnd = Template.IndexOf("]", inStart - 1) + 1;
                Template.Remove(inStart, inEnd - inStart + 1);
            }

            if (fi.ForumDesc != "")
            {
                Template = Template.Replace("[FORUMDESCRIPTION]", "<i class=\"fa fa-file-o fa-grey\"></i>&nbsp;" + fi.ForumDesc);
            }
            else
            {
                Template = Template.Replace("[FORUMDESCRIPTION]", "");
            }

            Template = Template.Replace("[TOTALTOPICS]", fi.TotalTopics.ToString());
            Template = Template.Replace("[TOTALREPLIES]", fi.TotalReplies.ToString());
            Template = Template.Replace("[FORUMSUBSCRIBERCOUNT]", fi.SubscriberCount.ToString());

            //Last Post Section
            int intLength = 0;
            if ((Template.IndexOf("[LASTPOSTSUBJECT:", 0) + 1) > 0)
            {
                int inStart = Template.IndexOf("[LASTPOSTSUBJECT:", 0) + 1 + 17;
                int inEnd = Template.IndexOf("]", inStart - 1) + 1;
                string sLength = Template.Substring(inStart - 1, inEnd - inStart);
                intLength = Convert.ToInt32(sLength);
            }

            string ReplaceTag = "[LASTPOSTSUBJECT:" + intLength.ToString() + "]";
            if (fi.LastPostID == 0)
            {
                Template = Template.Replace("[RESX:BY]", string.Empty);
                Template = Template.Replace("[DISPLAYNAME]", string.Empty);
                Template = Template.Replace("[LASTPOSTDATE]", string.Empty);
                Template = Template.Replace(ReplaceTag, string.Empty);
            }
            else
            {
                if (canView)
                {
                    if (fi.LastPostUserID <= 0)
                    {
                        Template = Template.Replace("[DISPLAYNAME]", "<i class=\"fa fa-user fa-fw fa-blue\"></i>&nbsp;" + fi.LastPostDisplayName);
                    }
                    else
                    {
                        bool isMod = this.CurrentUserType == CurrentUserTypes.Admin || this.CurrentUserType == CurrentUserTypes.ForumMod || this.CurrentUserType == CurrentUserTypes.SuperUser;
                        bool isAdmin = this.CurrentUserType == CurrentUserTypes.Admin || this.CurrentUserType == CurrentUserTypes.SuperUser;
                        Template = Template.Replace("[DISPLAYNAME]", "<i class=\"fa fa-user fa-fw fa-blue\"></i>&nbsp;" + UserProfiles.GetDisplayName(this.PortalSettings, this.ForumModuleId, true, isMod, isAdmin, fi.LastPostUserID, fi.LastPostUserName, fi.LastPostFirstName, fi.LastPostLastName, fi.LastPostDisplayName));
                    }

                    DateTime dtLastPostDate = fi.LastPostDateTime;
                    Template = Template.Replace("[LASTPOSTDATE]", Utilities.GetUserFormattedDateTime(dtLastPostDate,this.PortalId, this.CurrentUserId));
                    string Subject = HttpUtility.HtmlDecode(fi.LastPostSubject);
                    if (Subject != string.Empty)
                    {
                        Template = Template.Replace(ReplaceTag, this.GetLastPostSubject(fi.LastPostID, fi.LastTopicId, fi.ForumID, this.TabId, Subject, intLength, this.MainSettings.PageSize, fi));
                    }
                    else
                    {
                        Template = Template.Replace("[RESX:BY]", string.Empty);
                        Template = Template.Replace(ReplaceTag, string.Empty);
                    }
                }
                else
                {
                    Template = Template.Replace("[DISPLAYNAME]", string.Empty);
                    Template = Template.Replace("[LASTPOSTDATE]", string.Empty);
                    Template = Template.Replace("[RESX:BY]", string.Empty);
                    Template = Template.Replace(ReplaceTag, string.Empty);
                }

            }

            return Template;

        }

        #endregion
        #region Private Methods - Helpers
        private string GetForumLink(string Name, int ForumId, int TabId, bool CanView, string Url)
        {
            string sOut;
            sOut = Name;

            if (CanView && Name != string.Empty)
            {
                sOut = "<a href=\"" + Url + "\">" + Name + "</a>";

            }
            else if (CanView && Name == string.Empty)
            {
                return Url;
            }
            else
            {
                sOut = Name;
            }

            return sOut;
        }

        private string GetLastPostSubject(int LastPostID, int ParentPostID, int ForumID, int TabID, string Subject, int Length, int PageSize, DotNetNuke.Modules.ActiveForums.Entities.ForumInfo fi)
        {
            var ti = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(ParentPostID);
            var sb = new StringBuilder();
            Subject = HttpUtility.HtmlDecode(Subject);
            Subject = Utilities.StripHTMLTag(Subject);
            Subject = Subject.Replace("[", "&#91");
            Subject = Subject.Replace("]", "&#93");
            if (Subject.Length > Length & Length > 0)
            {
                Subject = Subject.Substring(0, Length) + "...";
            }

            string sURL = new ControlUtils().TopicURL(this.TabId, this.ForumModuleId, ParentPostID, fi.ForumGroup.PrefixURL, fi.PrefixURL, ti.TopicUrl);
            if (sURL.Contains("~/"))
            {
                sURL = Utilities.NavigateURL(this.TabId, "", new[] { ParamKeys.TopicId + "=" + ParentPostID, ParamKeys.ContentJumpId + "=" + LastPostID });
            }

            if (sURL.EndsWith("/") && LastPostID != ParentPostID)
            {
                sURL += Utilities.UseFriendlyURLs(this.ForumModuleId) ? String.Concat("#", LastPostID) : String.Concat("?", ParamKeys.ContentJumpId, "=", LastPostID);
            }

            sb.Append("<a href=\"" + sURL + "\">" + HttpUtility.HtmlEncode(Subject) + "</a>");
            return sb.ToString();
        }

        private string GetSubForums(string Template, int ForumId, int TabId, string ThemePath)
        {
            int i = 0;
            var subforums = this.Forums.Where(f=>f.ParentForumId== ForumId).ToList();
            if (Template == string.Empty)
            {
                var sb = new StringBuilder();
                string SubForum;
                foreach (DotNetNuke.Modules.ActiveForums.Entities.ForumInfo fi in subforums)
                {
                    bool canView = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(fi.Security.View, this.ForumUser.UserRoles);
                    SubForum = this.GetForumName(canView, fi.Hidden, TabId, fi.ForumID, fi.ForumName, this.MainSettings.UseShortUrls);
                    if (SubForum != string.Empty)
                    {
                        sb.Append(SubForum);
                        if (i < subforums.Count() - 1)
                        {
                            sb.Append(", ");
                        }

                        i += 1;
                    }
                }

                string subs = string.Empty;
                if (sb.Length > 0)
                {
                    sb.Insert(0, "<br />[RESX:SubForums] ");
                    subs = sb.ToString();
                    subs = subs.Trim();
                    if (subs.IndexOf(",", subs.Length - 1) > 0)
                    {
                        subs = subs.Substring(0, subs.LastIndexOf(","));
                    }
                }

                return subs;
            }
            else
            {
                string subs = string.Empty;
                foreach (DotNetNuke.Modules.ActiveForums.Entities.ForumInfo fi in subforums)
                {
                    i += 1;
                    string tmpSubs = TemplateUtils.GetTemplateSection(Template, "[SUBFORUMS]", "[/SUBFORUMS]");
                    bool canView = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(fi.Security.View, this.ForumUser.UserRoles);
                    if (canView || (!fi.Hidden) | this.UserInfo.IsSuperUser)
                    {
                        string sIcon = TemplateUtils.ShowIcon(canView, fi.ForumID, this.CurrentUserId, fi.LastPostDateTime, fi.LastRead, fi.LastPostID);

                        string sFolderCSS = "fa-folder fa-blue";
                        switch (sIcon.ToLower())
                        {
                            case "folder.png":
                                sFolderCSS = "fa-folder fa-blue";
                                break;
                            case "folder_new.png":
                                sFolderCSS = "fa-folder fa-red";
                                break;
                            case "folder_forbidden.png":
                                sFolderCSS = "fa-folder fa-grey";
                                break;
                            case "folder_closed.png":
                                sFolderCSS = "fa-folder-o fa-grey";
                                break;
                        }

                        tmpSubs = tmpSubs.Replace("[FORUMICONSM]", "<i class=\"fa " + sFolderCSS + " fa-fw\"></i>&nbsp;&nbsp;");

                        //tmpSubs = tmpSubs.Replace("[FORUMICONSM]", "<i class=\"fa fa-folder fa-fw fa-blue\"></i>&nbsp;&nbsp;");
                        tmpSubs = this.ParseForumRow(tmpSubs, fi, i, subforums.Count());
                    }
                    else
                    {
                        tmpSubs = string.Empty;
                    }

                    subs += tmpSubs;
                }

                Template = TemplateUtils.ReplaceSubSection(Template, subs, "[SUBFORUMS]", "[/SUBFORUMS]");
                return Template;
            }

        }

        private string GetForumName(bool CanView, bool Hidden, int TabID, int ForumID, string Name, bool UseShortUrls)
        {
            string sOut;
            string[] Params = { ParamKeys.ViewType + "=" + Views.Topics, ParamKeys.ForumId + "=" + ForumID };
            if (UseShortUrls)
            {
                Params = new[] { ParamKeys.ForumId + "=" + ForumID };
            }

            if (CanView)
            {
                sOut = "<a href=\"" + Utilities.NavigateURL(TabID, "", Params) + "\">" + Name + "</a>";
            }
            else if (Hidden)
            {
                sOut = string.Empty;
            }
            else
            {
                sOut = Name;
            }

            return sOut;
        }
    #endregion
    }

}
