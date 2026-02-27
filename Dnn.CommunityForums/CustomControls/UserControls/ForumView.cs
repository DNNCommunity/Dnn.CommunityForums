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

namespace DotNetNuke.Modules.ActiveForums.Controls
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Modules.ActiveForums.Extensions;

    [DefaultProperty("Text"), ToolboxData("<{0}:ForumView runat=server></{0}:ForumView>")]
    public class ForumView : ForumBase
    {
        public bool SubsOnly { get; set; }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use Forums property.")]
        public DataTable ForumTable { get; set; }

        public List<DotNetNuke.Modules.ActiveForums.Entities.ForumInfo> Forums { get; set; }

        public string DisplayTemplate { get; set; } = string.Empty;

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
        public string BuildForumView(int forumTemplateId, int currentUserId, string themePath)
        {
            return this.BuildForumView();
        }

        public string BuildForumView()
        {
            try
            {
                string sTemplate = DotNetNuke.Modules.ActiveForums.Controllers.TemplateController.Template_Get(this.ForumModuleId, Enums.TemplateType.ForumView, SettingsBase.GetModuleSettings(this.ForumModuleId).DefaultFeatureSettings.TemplateFileNameSuffix, this.ForumUser);

                StringBuilder stringBuilder = new StringBuilder(sTemplate);
                #region "Backward compatilbility -- remove in v10.00.00"
                stringBuilder = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.RemoveObsoleteTokens(stringBuilder);
                stringBuilder = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.MapLegacyUserTokenSynonyms(stringBuilder, this.PortalSettings, this.ModuleSettings, this.ForumUser.UserInfo?.Profile?.PreferredLocale);
                stringBuilder = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.MapLegacyForumTokenSynonyms(stringBuilder, this.PortalSettings, this.ForumUser.UserInfo?.Profile?.PreferredLocale);
                #endregion "Backward compatilbility -- remove in v10.00.00"

                stringBuilder.Replace("[JUMPTO]", "<asp:placeholder id=\"plhQuickJump\" runat=\"server\" />");
                stringBuilder.Replace("[STATISTICS]", "<am:Stats id=\"amStats\" MID=\"" + this.ModuleId + "\" PID=\"" + this.PortalId.ToString() + "\" runat=\"server\" />");
                stringBuilder.Replace("[WHOSONLINE]", this.ModuleSettings.UsersOnlineEnabled ? "<asp:placeholder id=\"plhUsersOnline\" runat=\"server\" />" : string.Empty);

                if (stringBuilder.ToString().Contains("[NOTOOLBAR]"))
                {
                    if (HttpContext.Current.Items.Contains("ShowToolbar"))
                    {
                        HttpContext.Current.Items["ShowToolbar"] = false;
                    }
                    else
                    {
                        HttpContext.Current.Items.Add("ShowToolbar", false);
                    }

                    stringBuilder.Replace("[NOTOOLBAR]", string.Empty);
                }

                sTemplate = stringBuilder.ToString();

                if (sTemplate.Contains("[FORUMS]"))
                {
                    string sGroupSection = string.Empty;
                    string sGroupSectionTemp = TemplateUtils.GetTemplateSection(sTemplate, "[GROUPSECTION]", "[/GROUPSECTION]");
                    string sGroupTemplate = TemplateUtils.GetTemplateSection(sTemplate, "[GROUP]", "[/GROUP]");
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
                    #endregion "backward compatibilty - remove when removing ForumTable property"

                    if (this.Forums == null)
                    {
                        string cachekey = string.Format(CacheKeys.ForumViewForUser, this.ForumModuleId, this.ForumUser.UserId, this.ForumIds.FromHashSetToDelimitedString(";"), HttpContext.Current?.Response?.Cookies["language"]?.Value, this.ForumUser.RunningInViewer);
                        var obj = DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheRetrieve(this.ForumModuleId, cachekey);
                        if (obj == null)
                        {
                            this.Forums = new DotNetNuke.Modules.ActiveForums.Entities.ForumCollection();
                            foreach (var forumId in this.ForumIds)
                            {
                                this.Forums.Add(new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(Utilities.SafeConvertInt(forumId), this.ForumModuleId));
                            }

                            DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheStore(this.ForumModuleId, cachekey, this.Forums);
                        }
                        else
                        {
                            this.Forums = (List<DotNetNuke.Modules.ActiveForums.Entities.ForumInfo>)obj;
                        }
                    }

                    if (this.Request.QueryString[ParamKeys.GroupId] != null)
                    {
                        this.Forums = this.Forums.Where(f => f.ForumGroupId == Convert.ToInt32(this.Request.QueryString[ParamKeys.GroupId]) && f.Active && !f.Hidden && f.ForumGroup != null && f.ForumGroup.Active && !f.ForumGroup.Hidden).OrderBy(f => f.ForumGroup?.SortOrder).ThenBy(f => f.ForumGroupId).ThenBy(f => f.SortOrder).ToList();
                    }
                    else
                    {
                        this.Forums = this.Forums.Where(f => f.Active && !f.Hidden && f.ForumGroup != null && f.ForumGroup.Active && !f.ForumGroup.Hidden).OrderBy(f => f.ForumGroup?.SortOrder).ThenBy(f => f.ForumGroupId).ThenBy(f => f.SortOrder).ToList();
                    }

                    string sGroupName = (this.ForumGroupId != -1 && this.Forums?.Count > 0) ? this.Forums?.FirstOrDefault().GroupName : string.Empty;
                    string sCrumb = (this.ForumGroupId != -1 && this.Forums?.Count > 0) ? "<div class=\"afcrumb\"><i class=\"fa fa-comments-o fa-grey\"></i>  <a href=\"" + Utilities.NavigateURL(this.TabId) + "\">[RESX:ForumMain]</a>  <i class=\"fa fa-long-arrow-right fa-grey\"></i>  " + sGroupName + "</div>" : string.Empty;

                    if (this.ParentForumId != -1)
                    {
                        sGroupName = this.Forums?.Where(f => f.ForumID == this.ParentForumId).FirstOrDefault().GroupName;
                    }

                    if (this.ModuleSettings.UseSkinBreadCrumb && this.Forums?.Count > 0 && this.SubsOnly == false && this.ForumGroupId != -1)
                    {
                        DotNetNuke.Modules.ActiveForums.Environment.UpdateBreadCrumb(this.Page.Controls, "<a href=\"" + this.NavigateUrl(this.TabId, string.Empty, ParamKeys.GroupId + "=" + this.ForumGroupId) + "\">" + sGroupName + "</a>");
                        sTemplate = sTemplate.Replace("<div class=\"afcrumb\">[FORUMMAINLINK] > [FORUMGROUPLINK]</div>", string.Empty);
                        sTemplate = sTemplate.Replace("[BREADCRUMB]", string.Empty);
                    }
                    else
                    {
                        sTemplate = sTemplate.Replace("[BREADCRUMB]", sCrumb);
                    }

                    int iForum = 1;
                    int forumCount = 0;
                    bool hasForums = false;
                    int tmpGroupCount = 0;
                    if (this.Forums != null)
                    {
                        foreach (var fi in this.Forums.Where(f => !this.SubsOnly || f.ParentForumId > 0).OrderBy(f => f.ForumGroup?.SortOrder).ThenBy(f => f.ForumGroupId).ThenBy(f => f.SortOrder).Take(Globals.ForumCount))
                        {
                            fi.PortalSettings = this.PortalSettings;
                            fi.MainSettings = this.ModuleSettings;
                            bool canView = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(fi.Security?.ViewRoleIds, this.ForumUser.UserRoleIds);
                            if (this.UserInfo.IsSuperUser || (canView && !fi.ForumGroup.Hidden))
                            {
                                if (tmpGroup != fi.GroupName)
                                {
                                    if (tmpGroupCount < Globals.GroupCount)
                                    {
                                        forumCount = this.Forums.Count(f => f.ForumGroupId == fi.ForumGroupId);
                                        if (sForums != string.Empty)
                                        {
                                            sGroupSection = TemplateUtils.ReplaceSubSection(sGroupSection, sForums, "[FORUMS]", "[/FORUMS]");
                                            sForums = string.Empty;
                                        }

                                        int groupId = fi.ForumGroupId;
                                        sGroupSectionTemp = TemplateUtils.GetTemplateSection(sTemplate, "[GROUPSECTION]", "[/GROUPSECTION]");

                                        StringBuilder sGroupSectionTempStringBuilder = new StringBuilder(sGroupSectionTemp);
                                        sGroupSectionTempStringBuilder = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.ReplaceForumGroupTokens(sGroupSectionTempStringBuilder, fi.ForumGroup, this.PortalSettings, this.ModuleSettings, new Services.URLNavigator().NavigationManager(), this.ForumUser, this.TabId, this.ForumUser.CurrentUserType, this.Request.Url, this.Request.RawUrl);
                                        sGroupSectionTemp = sGroupSectionTempStringBuilder.ToString();

                                        // any replacements on the group
                                        StringBuilder sNewGroupStringBuilder = new StringBuilder("<div id=\"group" + fi.ForumGroupId + "\" class=\"afgroup\">" + sGroupTemplate + "</div>");
                                        sNewGroupStringBuilder = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.ReplaceForumGroupTokens(sNewGroupStringBuilder, fi.ForumGroup, this.PortalSettings, this.ModuleSettings, new Services.URLNavigator().NavigationManager(), this.ForumUser, this.TabId, this.ForumUser.CurrentUserType, this.Request.Url, this.Request.RawUrl);
                                        string sNewGroup = sNewGroupStringBuilder.ToString();

                                        sGroupSectionTemp = TemplateUtils.ReplaceSubSection(sGroupSectionTemp, sNewGroup, "[GROUP]", "[/GROUP]");
                                        sGroupSection += sGroupSectionTemp;
                                        tmpGroup = fi.GroupName;
                                        tmpGroupCount += 1;
                                        iForum = 1;
                                    }
                                }

                                if (iForum <= Globals.ForumCount)
                                {
                                    if (this.UserInfo.IsSuperUser || (canView && !fi.Hidden))
                                    {
                                        sForumTemp = TemplateUtils.GetTemplateSection(sTemplate, "[FORUMS]", "[/FORUMS]");
                                        hasForums = true;
                                        if (fi.ParentForumId == 0 || this.SubsOnly || (this.SubsOnly == false && fi.ParentForumId > 0 && this.Forums.Count == 1))
                                        {
                                            sForumTemp = this.ParseForumRow(sForumTemp, fi, iForum, forumCount);
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

        private string ParseForumRow(string template, DotNetNuke.Modules.ActiveForums.Entities.ForumInfo fi, int currForumIndex, int totalForums)
        {
            if (template.Contains("[SUBFORUMS]") && template.Contains("[/SUBFORUMS]"))
            {
                template = this.GetSubForums(template, fi.ForumID, fi.GetTabId());
            }
            else
            {
                template = template.Replace("[SUBFORUMS]", this.GetSubForums(template: string.Empty, forumId: fi.ForumID, tabId: fi.GetTabId()));
            }

            string[] css = null;
            string cssmatch = string.Empty;
            if (template.Contains("[CSS:"))
            {
                string pattern = "(\\[CSS:.+?\\])";
                if (RegexUtils.GetCachedRegex(pattern, RegexOptions.Compiled & RegexOptions.IgnoreCase, 2).IsMatch(template))
                {
                    cssmatch = RegexUtils.GetCachedRegex(pattern, RegexOptions.Compiled & RegexOptions.IgnoreCase, 2).Match(template).Value;
                    css = cssmatch.Split(':'); // 0=CSS,1=TopRow, 2=mid rows, 3=lastRow
                }
            }

            if (cssmatch != string.Empty)
            {
                if (currForumIndex == 1)
                {
                    template = template.Replace(cssmatch, css[1]);
                }
                else if (currForumIndex > 1 & currForumIndex < totalForums)
                {
                    template = template.Replace(cssmatch, css[2]);
                }
                else
                {
                    template = template.Replace(cssmatch, css[3].Replace("]", string.Empty));
                }
            }

            StringBuilder templateStringBuilder = new StringBuilder(template);
            templateStringBuilder = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.ReplaceForumTokens(templateStringBuilder, fi, this.PortalSettings, this.ModuleSettings, new Services.URLNavigator().NavigationManager(), this.ForumUser, this.TabId, this.ForumUser.CurrentUserType, this.Request.Url, this.Request.RawUrl);

            if (templateStringBuilder.ToString().Contains("[AF:CONTROL:TOGGLESUBSCRIBE]"))
            {
                if (DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(fi.Security.SubscribeRoleIds, this.ForumUser.UserRoleIds))
                {
                    bool isSubscribed = new DotNetNuke.Modules.ActiveForums.Controllers.SubscriptionController().Subscribed(this.PortalId, this.ForumModuleId, this.UserId, this.ForumId);
                    var subControl = new ToggleSubscribe(this.ModuleId, fi.ForumID, -1, 0);
                    subControl.Checked = isSubscribed;
                    subControl.DisplayMode = 1;
                    subControl.UserId = this.CurrentUserId;
                    subControl.ImageURL = this.ThemePath + (isSubscribed ? "images/email_checked.png" : "images/email_unchecked.png"); ;
                    subControl.Text = "[RESX:Subscribe]";

                    templateStringBuilder.Replace("[AF:CONTROL:TOGGLESUBSCRIBE]", subControl.Render());
                }
                else
                {
                    templateStringBuilder.Replace("[AF:CONTROL:TOGGLESUBSCRIBE]", "<img src=\"" + this.ThemePath + "email_disabled.png\" border=\"0\" alt=\"[RESX:ForumSubscribe:Disabled]\" />");
                }
            }

            template = templateStringBuilder.ToString();

            return template;
        }

        #endregion
        #region Private Methods - Helpers

        private string GetSubForums(string template, int forumId, int tabId)

        {
            int i = 0;
            var subforums = this.Forums.Where(f => f.ParentForumId == forumId).ToList();
            if (template == string.Empty)
            {
                var sb = new StringBuilder();
                string subForum;
                foreach (DotNetNuke.Modules.ActiveForums.Entities.ForumInfo fi in subforums)
                {
                    subForum = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.ReplaceForumTokens(new StringBuilder("[FORUMLINK]"), fi, this.PortalSettings, this.ModuleSettings, new Services.URLNavigator().NavigationManager(), this.ForumUser, this.TabId, this.ForumUser.CurrentUserType, this.Request.Url, this.Request.RawUrl).ToString();
                    if (subForum != string.Empty)
                    {
                        sb.Append(subForum);
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
                    string tmpSubs = TemplateUtils.GetTemplateSection(template, "[SUBFORUMS]", "[/SUBFORUMS]");

                    bool canView = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(fi.Security.ViewRoleIds, this.ForumUser.UserRoleIds);
                    if (canView || (!fi.Hidden) | this.UserInfo.IsSuperUser)
                    {
                        tmpSubs = this.ParseForumRow(tmpSubs, fi, i, subforums.Count());
                    }
                    else
                    {
                        tmpSubs = string.Empty;
                    }

                    subs += tmpSubs;
                }

                template = TemplateUtils.ReplaceSubSection(template, subs, "[SUBFORUMS]", "[/SUBFORUMS]");
                return template;
            }
        }

        #endregion
    }
}
