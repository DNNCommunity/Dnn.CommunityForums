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
    using System.Data;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Web.UI.WebControls;
    using System.Xml;

    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Urls;
    using DotNetNuke.Framework;
    using DotNetNuke.Services.Log.EventLog;

    public partial class ForumSettings : ForumSettingsBase
    {
        private int? _fullTextStatus;

        private int FullTextStatus
        {
            get
            {
                if (!this._fullTextStatus.HasValue)
                {
                    this._fullTextStatus = DataProvider.Instance().Search_GetFullTextStatus();
                }

                return this._fullTextStatus.HasValue ? this._fullTextStatus.Value : -5;
            }
        }

        private bool IsFullTextAvailable
        {
            get
            {
                return this.FullTextStatus != -5 && this.FullTextStatus != -4 && this.FullTextStatus != 0;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();

            this.drpPageSize.Style.Add("float", "none");

            if (!Utilities.IsRewriteLoaded())
            {
                this.rdEnableURLRewriter.SelectedIndex = 1;
                this.rdEnableURLRewriter.Enabled = false;
            }
            else
            {
                this.rdEnableURLRewriter.Enabled = true;
            }

            var u = DotNetNuke.Entities.Users.UserController.Instance.GetCurrentUserInfo();
            if (u.IsSuperUser && (HttpRuntime.IISVersion.Major >= 7))
            {
                if (Utilities.IsRewriteLoaded())
                {
                    this.litToggleConfig.Text = "<a href=\"javascript:void(0);\" onclick=\"amaf_toggleConfig('configdisable',this); return false;\">Uninstall DNN Community Forums URL Handler</a>";
                }
                else
                {
                    this.litToggleConfig.Text = "<a href=\"javascript:void(0);\" onclick=\"amaf_toggleConfig('configenable',this); return false;\">Install DNN Community Forums URL Handler</a>";
                }

            }

            // Full Text
            this.rdFullTextSearch.Enabled = this.IsFullTextAvailable;
            switch (this.FullTextStatus)
            {
                case -4:
                    this.ltrFullTextMessage.Text = this.LocalizeString("FullTextAzure");
                    this.ltrFullTextMessage.Visible = true;
                    break;
                case 0:
                    this.ltrFullTextMessage.Text = this.LocalizeString("FullTextNotInstalled");
                    this.ltrFullTextMessage.Visible = true;
                    break;
                default:
                    this.ltrFullTextMessage.Visible = false;
                    break;
            }
        }

        #region Base Method Implementations

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// LoadSettings loads the settings from the Database and displays them
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// </history>
        /// -----------------------------------------------------------------------------
        public override void LoadSettings()
        {
            // Note, this is called before OnLoad

            try
            {
                // if (Page.IsPostBack == false)
                // {
                this.BindThemes();
                this.BindTemplates();
                this.BindPrivateMessaging();
                this.BindForumGroups();
                this.BindForumSecurity();

                Utilities.SelectListItemByValue(this.drpPageSize, this.PageSize);

                this.txtFloodInterval.Text = this.FloodInterval.ToString(); ;
                this.txtEditInterval.Text = this.EditInterval.ToString();

                Utilities.SelectListItemByValue(this.drpMode, this.Mode);
                Utilities.SelectListItemByValue(this.drpThemes, this.Theme);
                Utilities.SelectListItemByValue(this.drpTemplates, this.TemplateId);

                Utilities.SelectListItemByValue(this.rdAutoLinks, this.AutoLink);
                Utilities.SelectListItemByValue(this.drpDeleteBehavior, this.DeleteBehavior);
                Utilities.SelectListItemByValue(this.drpProfileVisibility, this.ProfileVisibility);
                Utilities.SelectListItemByValue(this.drpSignatures, this.Signatures);
                Utilities.SelectListItemByValue(this.drpUserDisplayMode, this.UserNameDisplay);
                Utilities.SelectListItemByValue(this.rdEnableURLRewriter, this.FriendlyURLs);

                Utilities.SelectListItemByValue(this.rdFullTextSearch, this.FullTextSearch && this.FullTextStatus == 1); // 1 = Enabled Status

                Utilities.SelectListItemByValue(this.rdCacheTemplates, this.CacheTemplates);
                Utilities.SelectListItemByValue(this.rdPoints, this.EnablePoints);
                Utilities.SelectListItemByValue(this.rdUsersOnline, this.EnableUsersOnline);
                Utilities.SelectListItemByValue(this.rdUseSkinBreadCrumb, this.UseSkinBreadCrumb);

                this.txtAnswerPointValue.Text = this.AnswerPointValue.ToString();
                this.txtTopicPointValue.Text = this.TopicPointValue.ToString();
                this.txtReplyPointValue.Text = this.ReplyPointValue.ToString();
                this.txtMarkAnswerPointValue.Text = this.MarkAsAnswerPointValue.ToString();
                this.txtModPointValue.Text = this.ModPointValue.ToString();

                this.txtURLPrefixBase.Text = this.PrefixURLBase;
                this.txtURLPrefixCategory.Text = this.PrefixURLCategory;
                this.txtURLPrefixOther.Text = this.PrefixURLOther;
                this.txtURLPrefixTags.Text = this.PrefixURLTag;

                this.txtAvatarHeight.Text = this.AvatarHeight.ToString();
                this.txtAvatarWidth.Text = this.AvatarWidth.ToString();

                this.txtTimeFormat.Text = this.TimeFormatString;
                this.txtDateFormat.Text = this.DateFormatString;

                Utilities.SelectListItemByValue(this.drpForumGroupTemplate, this.ForumGroupTemplate);
                // }
            }
            catch (Exception exc) // Module failed to load
            {
                DotNetNuke.Services.Exceptions.Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// UpdateSettings saves the modified settings to the Database
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// </history>
        /// -----------------------------------------------------------------------------
        public override void UpdateSettings()
        {

            try
            {
                this.Theme = this.drpThemes.SelectedValue;
                this.Mode = this.drpMode.SelectedValue;
                this.TemplateId = Utilities.SafeConvertInt(this.drpTemplates.SelectedValue);
                this.PageSize = Utilities.SafeConvertInt(this.drpPageSize.SelectedValue, 10);
                this.FloodInterval = Math.Max(0, Utilities.SafeConvertInt(this.txtFloodInterval.Text, 0));
                this.EditInterval = Math.Max(0, Utilities.SafeConvertInt(this.txtEditInterval.Text, 0));
                this.AutoLink = Utilities.SafeConvertBool(this.rdAutoLinks.SelectedValue);
                this.DeleteBehavior = Utilities.SafeConvertInt(this.drpDeleteBehavior.SelectedValue);
                this.ProfileVisibility = Utilities.SafeConvertInt(this.drpProfileVisibility.SelectedValue);
                this.Signatures = Utilities.SafeConvertInt(this.drpSignatures.SelectedValue);
                this.UserNameDisplay = this.drpUserDisplayMode.SelectedValue;
                this.FriendlyURLs = Utilities.SafeConvertBool(this.rdEnableURLRewriter.SelectedValue);

                var urlSettings = new FriendlyUrlSettings(this.PortalId);
                string DoNotRedirectRegex = urlSettings.DoNotRedirectRegex;
                const string ignoreForumsRegex = "(" + ParamKeys.ForumId + "=|" + ParamKeys.GroupId + "=|" + ParamKeys.TopicId + "=|" + ParamKeys.GridType + "=|" + ParamKeys.Tags + "=|" + ParamKeys.ViewType + "=|" + ParamKeys.Category + "=|" + ParamKeys.PageId + "=)|";

                if (Utilities.SafeConvertBool(this.rdEnableURLRewriter.SelectedValue))
                {
                    if (!DoNotRedirectRegex.Contains(ignoreForumsRegex))
                    {
                        DoNotRedirectRegex = string.Concat(ignoreForumsRegex, DoNotRedirectRegex);
                        DotNetNuke.Entities.Portals.PortalController.Instance.UpdatePortalSetting(portalID: this.PortalId, settingName: FriendlyUrlSettings.DoNotRedirectUrlRegexSetting, settingValue: DoNotRedirectRegex, clearCache: true, cultureCode: DotNetNuke.Common.Utilities.Null.NullString, isSecure: false);
                    }
                }
                else
                {
                    if (DoNotRedirectRegex.Contains(ignoreForumsRegex))
                    {
                        DoNotRedirectRegex.Replace(ignoreForumsRegex, string.Empty);
                        DotNetNuke.Entities.Portals.PortalController.Instance.UpdatePortalSetting(portalID: this.PortalId, settingName: FriendlyUrlSettings.DoNotRedirectUrlRegexSetting, settingValue: DoNotRedirectRegex, clearCache: true, cultureCode: DotNetNuke.Common.Utilities.Null.NullString, isSecure: false);
                    }
                }

                this.FullTextSearch = Utilities.SafeConvertBool(this.rdFullTextSearch.SelectedValue);
                this.CacheTemplates = Utilities.SafeConvertBool(this.rdCacheTemplates.SelectedValue);

                this.MessagingType = Utilities.SafeConvertInt(this.drpMessagingType.SelectedValue);

                this.EnableUsersOnline = Utilities.SafeConvertBool(this.rdUsersOnline.SelectedValue);
                this.UseSkinBreadCrumb = Utilities.SafeConvertBool(this.rdUseSkinBreadCrumb.SelectedValue);

                if (this.drpMessagingTab.SelectedItem != null)
                {
                    this.MessagingTabId = Utilities.SafeConvertInt(this.drpMessagingTab.SelectedValue);
                }

                this.PrefixURLBase = this.txtURLPrefixBase.Text;
                this.PrefixURLCategory = this.txtURLPrefixCategory.Text;
                this.PrefixURLOther = this.txtURLPrefixOther.Text;
                this.PrefixURLTag = this.txtURLPrefixTags.Text;

                this.EnablePoints = Utilities.SafeConvertBool(this.rdPoints.SelectedValue);
                this.AnswerPointValue = Utilities.SafeConvertInt(this.txtAnswerPointValue.Text, 1);
                this.ReplyPointValue = Utilities.SafeConvertInt(this.txtReplyPointValue.Text, 1);
                this.MarkAsAnswerPointValue = Utilities.SafeConvertInt(this.txtMarkAnswerPointValue.Text, 1);
                this.TopicPointValue = Utilities.SafeConvertInt(this.txtTopicPointValue.Text, 1);
                this.ModPointValue = Utilities.SafeConvertInt(this.txtModPointValue.Text, 1);

                this.AvatarHeight = Utilities.SafeConvertInt(this.txtAvatarHeight.Text, 48);
                this.AvatarWidth = Utilities.SafeConvertInt(this.txtAvatarWidth.Text, 48);

                this.TimeFormatString = !string.IsNullOrWhiteSpace(this.txtTimeFormat.Text) ? this.txtTimeFormat.Text : "h:mm tt";
                this.DateFormatString = !string.IsNullOrWhiteSpace(this.txtDateFormat.Text) ? this.txtDateFormat.Text : "M/d/yyyy";

                this.ForumGroupTemplate = Utilities.SafeConvertInt(this.drpForumGroupTemplate.SelectedValue);
                var adminSec = this.txtGroupModSec.Value.Split(',');
                this.SaveForumSecurity("groupadmin", adminSec);
                var memSec = this.txtGroupMemSec.Value.Split(',');
                this.SaveForumSecurity("groupmember", memSec);
                var regSec = this.txtGroupRegSec.Value.Split(',');
                this.SaveForumSecurity("registereduser", regSec);
                var anonSec = this.txtGroupAnonSec.Value.Split(',');
                this.SaveForumSecurity("anon", anonSec);

                try
                {
                    if (this.IsFullTextAvailable && this.FullTextSearch && this.FullTextStatus != 1) // Available, selected and not currently installed
                    {
                        // Note: We have to jump through some hoops here to maintain Azure compatibility and prevent a race condition in the procs.

                        // Create the full text manager proc
                        var fullTextInstallScript = Utilities.GetSqlString("DotNetNuke.Modules.ActiveForums.sql.FullTextInstallPart1.sql");
                        var result = DotNetNuke.Data.DataProvider.Instance().ExecuteScript(fullTextInstallScript);

                        // Exectute the full text manager proc to setup the search indexes
                        DataProvider.Instance().Search_ManageFullText(true);

                        // Create the full text search proc (can't be reliably created until the indexes are in place)
                        fullTextInstallScript = Utilities.GetSqlString("DotNetNuke.Modules.ActiveForums.sql.FullTextInstallPart2.sql");
                        DotNetNuke.Data.DataProvider.Instance().ExecuteScript(fullTextInstallScript);
                    }
                    else if (this.IsFullTextAvailable && !this.FullTextSearch) // Available, but not selected
                    {
                        try
                        {
                            // Remove the search indexes if they exist
                            DataProvider.Instance().Search_ManageFullText(false);
                        }
                        catch (InvalidOperationException)
                        {
                            // stored procedures have never been installed
                        }
                        catch
                        {
                            throw; // anything else
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.FullTextSearch = false;
                    DotNetNuke.Services.Exceptions.Exceptions.ProcessModuleLoadException(this, ex);
                }

                // Clear out the cache
                DataCache.ClearSettingsCache(this.ModuleId);

                var log = new DotNetNuke.Services.Log.EventLog.LogInfo { LogTypeKey = DotNetNuke.Abstractions.Logging.EventLogType.APPLICATION_SHUTTING_DOWN.ToString() };
                log.LogProperties.Add(new LogDetailInfo("ModuleId", this.ModuleId.ToString()));
                log.AddProperty("Message", this.LocalizeString("ApplicationRestart"));
                DotNetNuke.Services.Log.EventLog.LogController.Instance.AddLog(log);
                DotNetNuke.Common.Utilities.Config.Touch();

            }
            catch (Exception exc) // Module failed to load
            {
                DotNetNuke.Services.Exceptions.Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        #endregion

        #region Private Methods

        private void BindTemplates()
        {
            var tc = new TemplateController();
            var tl = tc.Template_List(this.PortalId, this.ModuleId, Templates.TemplateTypes.ForumView);
            this.drpTemplates.DataTextField = "Title";
            this.drpTemplates.DataValueField = "TemplateId";
            this.drpTemplates.DataSource = tl;
            this.drpTemplates.DataBind();
            this.drpTemplates.Items.Insert(0, new ListItem(this.LocalizeString("Default"), "0"));
        }

        private void BindThemes()
        {
            var di = new System.IO.DirectoryInfo(Utilities.MapPath(Globals.ModulePath + "themes"));
            this.drpThemes.DataSource = di.GetDirectories();
            this.drpThemes.DataBind();
        }

        private void BindPrivateMessaging()
        {
            var selectedMessagingType = this.drpMessagingType.Items.FindByValue(this.MessagingType.ToString());
            if (selectedMessagingType != null)
            {
                selectedMessagingType.Selected = true;
            }

            this.BindPrivateMessagingTab();
        }

        private void BindPrivateMessagingTab()
        {
            this.drpMessagingTab.Items.Clear();
            this.drpMessagingTab.ClearSelection();

            var mc = new DotNetNuke.Entities.Modules.ModuleController();
            var tc = new TabController();

            foreach (DotNetNuke.Entities.Modules.ModuleInfo mi in mc.GetModules(this.PortalId))
            {
                if (!mi.DesktopModule.ModuleName.Contains("DnnForge - PrivateMessages") || mi.IsDeleted)
                {
                    continue;
                }

                var ti = tc.GetTab(mi.TabID, this.PortalId, false);
                if (ti != null && !ti.IsDeleted)
                {
                    this.drpMessagingTab.Items.Add(new ListItem
                    {
                        Text = ti.TabName + " - Ventrian Messages",
                        Value = ti.TabID.ToString(),
                        Selected = ti.TabID == this.MessagingTabId
                    });
                }
            }

            if (this.drpMessagingTab.Items.Count == 0)
            {
                this.drpMessagingTab.Items.Add(new ListItem("No Messaging Tabs Found", "-1"));
                this.drpMessagingTab.Enabled = false;
            }

        }

        private void BindForumGroups()
        {
            using (IDataReader dr = DataProvider.Instance().Forums_List(this.PortalId, this.ModuleId, -1, -1, false))
            {
                var dt = new DataTable("Forums");
                dt.Load(dr);
                dr.Close();

                string tmpGroup = string.Empty;
                foreach (DataRow row in dt.Rows)
                {
                    if (tmpGroup != row["ForumGroupId"].ToString())
                    {
                        this.drpForumGroupTemplate.Items.Add(new ListItem(row["GroupName"].ToString(), row["ForumGroupId"].ToString()));
                        tmpGroup = row["ForumGroupId"].ToString();
                    }

                }

            }
        }

        private void BindForumSecurity()
        {
            var xDoc = new XmlDocument();
            if (string.IsNullOrEmpty(this.ForumConfig))
            {
                xDoc.Load(Utilities.MapPath(Globals.ModulePath + "config/defaultgroupforums.config"));
            }
            else
            {
                xDoc.LoadXml(this.ForumConfig);
            }

            if (xDoc != null)
            {
                XmlNode xRoot = xDoc.DocumentElement;
                var xNodeList = xRoot.SelectSingleNode("//defaultforums/forum/security[@type='groupadmin']").ChildNodes;
                var sb = new StringBuilder();
                sb.Append("<table cellpadding=\"0\" cellspacing=\"0\">");
                var rows = new string[17, 5];
                int i = 0;
                foreach (XmlNode x in xNodeList)
                {
                    rows[i, 0] = x.Name;
                    rows[i, 1] = x.Attributes["value"].Value;
                    i += 1;
                }

                i = 0;
                xNodeList = xRoot.SelectSingleNode("//defaultforums/forum/security[@type='groupmember']").ChildNodes;
                foreach (XmlNode x in xNodeList)
                {
                    rows[i, 2] = x.Attributes["value"].Value;
                    i += 1;
                }

                i = 0;
                xNodeList = xRoot.SelectSingleNode("//defaultforums/forum/security[@type='registereduser']").ChildNodes;
                foreach (XmlNode x in xNodeList)
                {
                    rows[i, 3] = x.Attributes["value"].Value;
                    i += 1;
                }

                i = 0;
                xNodeList = xRoot.SelectSingleNode("//defaultforums/forum/security[@type='anon']").ChildNodes;
                foreach (XmlNode x in xNodeList)
                {
                    rows[i, 4] = x.Attributes["value"].Value;
                    i += 1;
                }

                i = 0;
                sb.Append("<tr id=\"hd1\"><td></td><td colspan=\"10\" class=\"afgridhd sec1\">" + this.LocalizeString("UserPermissions") + "</td><td colspan=\"7\" class=\"afgridhd sec2\">" + this.LocalizeString("ModeratorPermissions") + "</td></tr>");
                sb.Append("<tr id=\"hd2\"><td></td>");
                string sClass;
                for (i = 0; i <= 16; i++)
                {
                    sClass = "afgridhdsub";
                    if (i == 0)
                    {
                        sClass += " colstart";
                    }
                    else if (i == 16)
                    {
                        sClass += " colend";
                    }
                    else if (i == 9)
                    {
                        sClass += " gridsep";

                    }

                    sb.Append("<td class=\"" + sClass + "\">");
                    sb.Append(this.LocalizeString("SecGrid:" + rows[i, 0]));
                    sb.Append("</td>");
                }

                sb.Append("</tr><tr id=\"row1\"><td class=\"rowhd\">" + this.LocalizeString("GroupAdmin") + "</td>");
                i = 0;

                for (i = 0; i <= 16; i++)
                {
                    sClass = "gridcheck";
                    if (i <= 9)
                    {
                        sClass += " sec1";
                    }
                    else
                    {
                        sClass += " sec2";
                    }

                    if (i == 16)
                    {
                        sClass += " colend";
                    }

                    if (i == 9)
                    {
                        // sClass &= " gridsep"
                    }

                    sb.Append("<td align=\"center\" class=\"" + sClass + "\">");

                    if (rows[i, 1] == "true")
                    {
                        sb.Append("<input type=\"checkbox\" id=\"ga" + rows[i, 0] + "\" checked=\"checked\" />");
                    }
                    else
                    {
                        sb.Append("<input type=\"checkbox\" id=\"ga" + rows[i, 0] + "\" />");
                    }

                    sb.Append("</td>");
                }

                sb.Append("</tr>");
                i = 0;
                sb.Append("<tr id=\"row2\"><td class=\"rowhd\">" + this.LocalizeString("GroupMember") + "</td>");
                for (i = 0; i <= 16; i++)
                {
                    sClass = "gridcheck";
                    if (i <= 9)
                    {
                        sClass += " sec1";
                    }
                    else
                    {
                        sClass += " sec2";
                    }

                    if (i == 16)
                    {
                        sClass += " colend";
                    }

                    if (i == 9)
                    {
                        // sClass &= " gridsep"
                    }

                    sb.Append("<td align=\"center\" class=\"" + sClass + "\">");
                    if (rows[i, 2] == "true")
                    {
                        sb.Append("<input type=\"checkbox\" id=\"gm" + rows[i, 0] + "\" checked=\"checked\" />");
                    }
                    else
                    {
                        sb.Append("<input type=\"checkbox\" id=\"gm" + rows[i, 0] + "\" />");
                    }

                    sb.Append("</td>");
                }

                sb.Append("</tr>");

                i = 0;
                sb.Append("<tr id=\"row3\"><td class=\"rowhd\">" + this.LocalizeString("RegisteredUser") + "</td>");
                for (i = 0; i <= 16; i++)
                {
                    sClass = "gridcheck";
                    if (i <= 9)
                    {
                        sClass += " sec1";
                    }
                    else
                    {
                        sClass += " sec2";
                    }

                    if (i == 16)
                    {
                        sClass += " colend";
                    }

                    if (i == 9)
                    {
                        // sClass &= " gridsep"
                    }

                    sb.Append("<td align=\"center\" class=\"" + sClass + "\">");
                    if (rows[i, 3] == "true")
                    {
                        sb.Append("<input type=\"checkbox\" id=\"gr" + rows[i, 0] + "\" checked=\"checked\" />");
                    }
                    else
                    {
                        sb.Append("<input type=\"checkbox\" id=\"gr" + rows[i, 0] + "\" />");
                    }

                    sb.Append("</td>");
                }

                sb.Append("</tr>");
                i = 0;
                sb.Append("<tr id=\"row4\"><td class=\"rowhd\">" + this.LocalizeString("Anon") + "</td>");
                for (i = 0; i <= 16; i++)
                {
                    sClass = "gridcheck";
                    if (i <= 9)
                    {
                        sClass += " sec1";
                    }
                    else
                    {
                        sClass += " sec2";
                    }

                    if (i == 16)
                    {
                        sClass += " colend";
                    }

                    if (i == 9)
                    {
                        // sClass &= " gridsep"
                    }

                    sb.Append("<td align=\"center\" class=\"" + sClass + "\">");
                    if (rows[i, 4] == "true")
                    {
                        sb.Append("<input type=\"checkbox\" id=\"gn" + rows[i, 0] + "\" checked=\"checked\" />");
                    }
                    else
                    {
                        sb.Append("<input type=\"checkbox\" id=\"gn" + rows[i, 0] + "\" />");
                    }

                    sb.Append("</td>");
                }

                sb.Append("</tr>");

                sb.Append("</table>");
                this.litForumSecurity.Text = sb.ToString();
            }
        }

        private void SaveForumSecurity(string sectype, string[] security)
        {
            var xDoc = new XmlDocument();
            if (string.IsNullOrEmpty(this.ForumConfig))
            {
                xDoc.Load(Utilities.MapPath(Globals.ModulePath + "config/defaultgroupforums.config"));
            }
            else
            {
                xDoc.LoadXml(this.ForumConfig);
            }

            XmlNode xRoot = xDoc.DocumentElement;
            var xNode = xRoot.SelectSingleNode("//defaultforums/forum/security[@type='" + sectype + "']");
            foreach (string s in security)
            {
                if (!string.IsNullOrEmpty(s))
                {
                    string nodeName = s.Split('=')[0];
                    string nodeValue = s.Split('=')[1];
                    xNode[nodeName].Attributes["value"].Value = nodeValue;
                }
            }

            this.ForumConfig = xDoc.OuterXml;

        }

        #endregion

    }
}
