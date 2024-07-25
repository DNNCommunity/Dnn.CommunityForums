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
    using System.Collections;
    using System.ComponentModel;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.UI;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;

    [DefaultProperty("Text"), ToolboxData("<{0}:WhatsNewRSS runat=server></{0}:WhatsNewRSS>")]
    public class WhatsNewRSS : Control
    {

        #region Constants

        private const string ModuleIDRequestKey = "moduleid";
        private const string PortalIDRequestKey = "portalid";
        private const string TabIDRequestKey = "tabid";

        private const int DefaultModuleID = -1;
        private const int DefaultPortalID = 0;
        private const int DefaultTabID = -1;

        private const string XmlHeader = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
        private const string RSSHeader = "<rss version=\"2.0\" xmlns:atom=\"http://www.w3.org/2005/Atom\" xmlns:cf=\"http://www.microsoft.com/schemas/rss/core/2005\" xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:slash=\"http://purl.org/rss/1.0/modules/slash/\">";

        #endregion

        #region Private Variables

        private int? _tabId;
        private int? _moduleId;
        private int? _portalId;
        private WhatsNewModuleSettings _settings;
        private string _authorizedForums;
        private User _currentUser;
        private string _cacheKey;

        #endregion

        #region Properties

        private int TabId
        {
            get
            {
                if (!this._tabId.HasValue)
                {
                    int parsedTabId;
                    this._tabId = int.TryParse(HttpContext.Current.Request.QueryString[TabIDRequestKey], out parsedTabId) ? parsedTabId : DefaultTabID;
                }

                return this._tabId.Value;
            }
        }

        private int ModuleId
        {
            get
            {
                if (!this._moduleId.HasValue)
                {
                    int parsedModuleID;
                    this._moduleId = int.TryParse(HttpContext.Current.Request.QueryString[ModuleIDRequestKey], out parsedModuleID) ? parsedModuleID : DefaultModuleID;
                }

                return this._moduleId.Value;
            }
        }

        private int PortalId
        {
            get
            {
                if (!this._portalId.HasValue)
                {
                    int parsedPortalID;
                    this._portalId = int.TryParse(HttpContext.Current.Request.QueryString[PortalIDRequestKey], out parsedPortalID) ? parsedPortalID : DefaultPortalID;
                }

                return this._portalId.Value;
            }
        }

        private WhatsNewModuleSettings Settings
        {
            get
            {
                if (this._settings == null)
                {
                    var settingsCacheKey = string.Format(CacheKeys.WhatsNew, this.ModuleId);

                    var moduleSettings = DataCache.SettingsCacheRetrieve(this.ModuleId, settingsCacheKey) as Hashtable;
                    if (moduleSettings == null)
                    {
                        moduleSettings = DotNetNuke.Entities.Modules.ModuleController.Instance.GetModule(moduleId: this.ModuleId, tabId: this.TabId, ignoreCache: false).ModuleSettings;
                        DataCache.SettingsCacheStore(this.ModuleId, settingsCacheKey, moduleSettings);
                    }

                    this._settings = WhatsNewModuleSettings.CreateFromModuleSettings(moduleSettings);
                }

                return this._settings;
            }
        }

        private User CurrentUser
        {
            get { return this._currentUser ?? (this._currentUser = new UserController().GetUser(this.PortalId, -1)); }
        }

        private string AuthorizedForums
        {
            get
            {
                return this._authorizedForums ??
                       (this._authorizedForums =
                        DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.CheckForumIdsForViewForRSS(-1, this.Settings.Forums, this.CurrentUser.UserRoles));
            }
        }

        private string CacheKey
        {
            get
            {
                return this._cacheKey ??
                       (this._cacheKey =
                        string.Format(CacheKeys.RssTemplate, this.ModuleId,
                                      this.Settings.RSSIgnoreSecurity ? this.Settings.Forums : this.AuthorizedForums));
            }
        }

        #endregion

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            HttpContext.Current.Response.ContentType = "text/xml";
            HttpContext.Current.Response.ContentEncoding = Encoding.UTF8;
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (this.TabId == DefaultTabID || this.ModuleId == DefaultModuleID || !this.Settings.RSSEnabled)
            {
                return;
            }

            // Attempt to load from cache if it's enabled
            var rss = (this.Settings.RSSCacheTimeout > 0) ? DataCache.SettingsCacheRetrieve(this.ModuleId, this.CacheKey) as string : null;

            // Build the RSS if needed
            rss = rss ?? this.BuildRSS();

            // Save the rss to cache if it's enabled
            if (this.Settings.RSSCacheTimeout > 0)
            {
                DataCache.SettingsCacheStore(this.ModuleId, this.CacheKey, rss);
            }

            // Render the output
            writer.Write(rss);
            base.Render(writer);
        }

        #region XML Helpers

        private static string WriteElement(string element, int indent)
        {
            var inputLength = element.Trim().Length + 20;
            var sb = new StringBuilder(inputLength);
            sb.Append(System.Environment.NewLine.PadRight(indent + 2, '\t'));
            sb.Append("<").Append(element).Append(">");
            return sb.ToString();
        }

        private static string WriteElement(string element, string elementValue, int indent)
        {
            var inputLength = element.Trim().Length + elementValue.Trim().Length + 20;
            var sb = new StringBuilder(inputLength);
            sb.Append(System.Environment.NewLine.PadRight(indent + 2, '\t'));
            sb.Append("<").Append(element).Append(">");
            sb.Append(CleanXmlString(elementValue));
            sb.Append("</").Append(element).Append(">");
            return sb.ToString();
        }

        private static string CleanXmlString(string xmlString)
        {
            xmlString = HttpUtility.HtmlEncode(xmlString);
            return xmlString;
        }

        #endregion

        private string BuildRSS()
        {
            const int indent = 2;

            var sb = new StringBuilder(1024);

            // build header
            sb.Append(XmlHeader + System.Environment.NewLine);
            sb.Append(RSSHeader + System.Environment.NewLine);

            // build channel
            var ps = DotNetNuke.Modules.ActiveForums.Utilities.GetPortalSettings();

            var offSet = Convert.ToInt32(PortalSettings.Current.TimeZone.GetUtcOffset(DateTime.UtcNow).TotalMinutes);

            sb.Append(WriteElement("channel", indent));
            sb.Append(WriteElement("title", ps.PortalName, indent));
            sb.Append(WriteElement("link", "http://" + HttpContext.Current.Request.Url.Host, indent));
            sb.Append(value: WriteElement("description", ps.PortalName, indent));
            sb.Append(WriteElement("generator", "ActiveForums  5.0", indent));
            sb.Append(WriteElement("language", ps.DefaultLanguage, indent));

            if (ps.LogoFile != string.Empty)
            {
                var sLogo = "<image><url>http://" + HttpContext.Current.Request.Url.Host + ps.HomeDirectory + ps.LogoFile + "</url>";
                sLogo += "<title>" + ps.PortalName + "</title>";
                sLogo += "<link>http://" + HttpContext.Current.Request.Url.Host + "</link></image>";
                sb.Append(sLogo);
            }

            sb.Append(WriteElement("copyright", ps.FooterText.Replace("[year]", DateTime.UtcNow.Year.ToString()), 2));
            sb.Append(WriteElement("lastBuildDate", "[LASTBUILDDATE]", 2));

            var lastBuildDate = DateTime.MinValue;

            // build items

            var forumids = this.Settings.RSSIgnoreSecurity ? this.Settings.Forums : this.AuthorizedForums;

            var dr = DataProvider.Instance().GetPosts(this.PortalId, forumids, true, false, this.Settings.Rows, this.Settings.Tags);
            var sHost = Utilities.GetHost();

            try
            {
                while (dr.Read())
                {
                    var groupName = Convert.ToString(dr["GroupName"]);
                    var groupId = Convert.ToInt32(dr["ForumGroupId"]);
                    var topicTabId = Convert.ToInt32(dr["TabId"]);
                    var topicModuleId = Convert.ToInt32(dr["ModuleId"]);
                    var forumName = Convert.ToString(dr["ForumName"]);
                    var forumId = Convert.ToInt32(dr["ForumId"]);
                    var subject = Convert.ToString(dr["Subject"]);
                    var userName = Convert.ToString(dr["AuthorUserName"]);
                    var postDate = Convert.ToString(dr["DateCreated"]);
                    var body = Utilities.StripHTMLTag(Convert.ToString(dr["Body"]));
                    var bodyHtml = Convert.ToString(dr["Body"]);
                    var displayName = Convert.ToString(dr["AuthorDisplayName"]);
                    var replyCount = Convert.ToString(dr["ReplyCount"]);
                    var lastPostDate = string.Empty;
                    var topicId = Convert.ToInt32(dr["TopicId"]);
                    var replyId = Convert.ToInt32(dr["ReplyId"]);
                    var firstName = Convert.ToString(dr["AuthorFirstName"]);
                    var lastName = Convert.ToString(dr["AuthorLastName"]);
                    var authorId = Convert.ToInt32(dr["AuthorId"]);
                    var sForumUrl = Convert.ToString(dr["PrefixURL"]);
                    var sTopicUrl = Convert.ToString(dr["TopicURL"]);
                    var sGroupPrefixUrl = Convert.ToString(dr["GroupPrefixURL"]);

                    var dateCreated = Convert.ToDateTime(dr["DateCreated"]).AddMinutes(offSet);

                    if (lastBuildDate == DateTime.MinValue || dateCreated > lastBuildDate)
                    {
                        lastBuildDate = dateCreated;
                    }

                    var ts = SettingsBase.GetModuleSettings(topicModuleId);

                    string url;
                    if (string.IsNullOrEmpty(sTopicUrl) || !Utilities.UseFriendlyURLs(topicModuleId))
                    {
                        string[] Params = { ParamKeys.ViewType + "=" + Views.Topic, ParamKeys.ForumId + "=" + forumId, ParamKeys.TopicId + "=" + topicId };
                        url = Utilities.NavigateURL(topicTabId, "", Params);
                        if (url.IndexOf(HttpContext.Current.Request.Url.Host, StringComparison.CurrentCulture) == -1)
                        {
                            url = Common.Globals.AddHTTP(HttpContext.Current.Request.Url.Host) + url;
                        }
                    }
                    else
                    {
                        var ctlUtils = new ControlUtils();
                        sTopicUrl = ctlUtils.BuildUrl(topicTabId, topicModuleId, sGroupPrefixUrl, sForumUrl, groupId, forumId, topicId, sTopicUrl, -1, -1, string.Empty, 1, replyId, -1);
                        if (sHost.EndsWith("/") && sTopicUrl.StartsWith("/"))
                        {
                            sHost = sHost.Substring(0, sHost.Length - 1);
                        }

                        url = sHost + sTopicUrl;
                    }

                    sb.Append(WriteElement("item", indent));

                    sb.Append(WriteElement("title", subject, indent + 1));
                    if (this.Settings.RSSIncludeBody)
                    {
                        if (bodyHtml.IndexOf("<body>", StringComparison.CurrentCulture) > 0)
                        {
                            bodyHtml = TemplateUtils.GetTemplateSection(bodyHtml, "<body>", "</body>");
                        }

                        // Legacy Attachment functionality uses "attachid"
                        if (bodyHtml.Contains("&#91;IMAGE:"))
                        {
                            var strHost = Common.Globals.AddHTTP(Common.Globals.GetDomainName(HttpContext.Current.Request)) + "/";
                            const string pattern = "(&#91;IMAGE:(.+?)&#93;)";
                            var regExp = new Regex(pattern);
                            var matches = regExp.Matches(bodyHtml);
                            foreach (Match match in matches)
                            {
                                var sImage = "<img src=\"" + strHost + "DesktopModules/ActiveForums/viewer.aspx?portalid=" + this.PortalId + "&moduleid=" + this.ModuleId + "&attachid=" + match.Groups[2].Value + "\" border=\"0\" />";
                                bodyHtml = bodyHtml.Replace(match.Value, sImage);
                            }
                        }

                        // Legacy Attachment functionality uses "attachid"
                        if (bodyHtml.Contains("&#91;THUMBNAIL:"))
                        {
                            var strHost = Common.Globals.AddHTTP(Common.Globals.GetDomainName(HttpContext.Current.Request)) + "/";
                            const string pattern = "(&#91;THUMBNAIL:(.+?)&#93;)";
                            var regExp = new Regex(pattern);
                            var matches = regExp.Matches(bodyHtml);
                            foreach (Match match in matches)
                            {
                                var thumbId = match.Groups[2].Value.Split(':')[0];
                                var parentId = match.Groups[2].Value.Split(':')[1];
                                var sImage = "<a href=\"" + strHost + "DesktopModules/ActiveForums/viewer.aspx?portalid=" + this.PortalId + "&moduleid=" + this.ModuleId + "&attachid=" + parentId + "\" target=\"_blank\"><img src=\"" + strHost + "DesktopModules/ActiveForums/viewer.aspx?portalid=" + this.PortalId + "&moduleid=" + this.ModuleId + "&attachid=" + thumbId + "\" border=\"0\" /></a>";
                                bodyHtml = bodyHtml.Replace(match.Value, sImage);
                            }
                        }

                        bodyHtml = bodyHtml.Replace("src=\"/Portals", "src=\"" + Common.Globals.AddHTTP(HttpContext.Current.Request.Url.Host) + "/Portals");
                        bodyHtml = Utilities.ManageImagePath(bodyHtml, new Uri(Common.Globals.AddHTTP(HttpContext.Current.Request.Url.Host)));

                        sb.Append(WriteElement("description", bodyHtml, indent + 1));
                    }
                    else
                    {
                        sb.Append(WriteElement("description", string.Empty, indent + 1));
                    }

                    sb.Append(WriteElement("link", url, indent + 1));
                    sb.Append(WriteElement("comments", url, indent + 1));
                    sb.Append(WriteElement("pubDate", Convert.ToDateTime(postDate).AddMinutes(offSet).ToString("r"), indent + 1));
                    sb.Append(WriteElement("dc:creator", displayName, indent + 1));
                    sb.Append(WriteElement("guid", url, indent + 1));
                    sb.Append(WriteElement("slash:comments", replyCount, indent + 1));
                    sb.Append(WriteElement("/item", indent));

                }

                dr.Close();
                sb.Append("<atom:link href=\"http://" + HttpContext.Current.Request.Url.Host + HttpUtility.HtmlEncode(HttpContext.Current.Request.RawUrl) + "\" rel=\"self\" type=\"application/rss+xml\" />");
                sb.Append(WriteElement("/channel", 1));
                sb.Append("</rss>");
                sb.Replace("[LASTBUILDDATE]", lastBuildDate.ToString("r"));
                return sb.ToString();
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }
    }

}
