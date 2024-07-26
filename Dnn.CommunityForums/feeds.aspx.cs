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
    using System.Collections.Generic;
    using System.Data;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;

    public class af_rss : DotNetNuke.Framework.PageBase
    {
        protected System.Web.UI.WebControls.Label xML;
        private int moduleID = -1;

        private DataRow drForum;
        private DataRow drSecurity;
        private DataTable dtTopics;
        private bool bView = false;
        private bool bRead = false;
        private string forumName;
        private string forumDescription;
        private string groupName;
        private bool bAllowRSS = false;
        private DateTime lastBuildDate = DateTime.MinValue;
        private int offSet = 0;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Put user code to initialize the page here
            this.Response.ContentType = "text/xml";
            this.Response.ContentEncoding = Encoding.UTF8;
            int intPortalId = -1;
            if (this.Request.QueryString["portalid"] != null)
            {
                if (SimulateIsNumeric.IsNumeric(this.Request.QueryString["portalid"]))
                {
                    intPortalId = Convert.ToInt32(this.Request.QueryString["portalid"]);
                }
            }

            // PortalSettings.PortalId
            int intTabId = -1;

            if (this.Request.QueryString["tabid"] != null)
            {
                if (SimulateIsNumeric.IsNumeric(this.Request.QueryString["tabid"]))
                {
                    intTabId = Convert.ToInt32(this.Request.QueryString["tabid"]);
                }
            }

            if (this.Request.QueryString["moduleid"] != null)
            {
                if (SimulateIsNumeric.IsNumeric(this.Request.QueryString["moduleid"]))
                {
                    this.moduleID = Convert.ToInt32(this.Request.QueryString["moduleid"]);
                }
            }

            int intPosts = 10;
            bool bolSecurity = false;
            bool bolBody = true;
            int forumID = -1;
            if (this.Request.QueryString["ForumID"] != null)
            {
                if (SimulateIsNumeric.IsNumeric(this.Request.QueryString["ForumId"]))
                {
                    forumID = Int32.Parse(this.Request.QueryString["ForumID"]);
                }
            }

            if (intPortalId >= 0 && intTabId > 0 & this.moduleID > 0 & forumID > 0)
            {
                this.Response.Write(this.BuildRSS(intPortalId, intTabId, this.moduleID, intPosts, forumID, bolSecurity, bolBody));
            }
        }
        #region Private Methods

        private string BuildRSS(int portalId, int tabId, int moduleId, int intPosts, int forumID, bool ingnoreSecurity, bool includeBody)
        {
            PortalSettings ps = DotNetNuke.Modules.ActiveForums.Utilities.GetPortalSettings();
            DotNetNuke.Entities.Users.UserInfo ou = DotNetNuke.Entities.Users.UserController.Instance.GetCurrentUserInfo();
            UserController uc = new UserController();
            User u = uc.GetUser(portalId, moduleId);

            DataSet ds = DataProvider.Instance().UI_TopicsView(portalId, moduleId, forumID, ou.UserID, 0, 20, ou.IsSuperUser, SortColumns.ReplyCreated);
            if (ds.Tables.Count > 0)
            {
                this.offSet = Convert.ToInt32(ps.TimeZone.GetUtcOffset(DateTime.UtcNow).TotalMinutes);
                if (ds.Tables[0].Rows.Count == 0)
                {
                    return string.Empty;
                }

                this.drForum = ds.Tables[0].Rows[0];

                this.drSecurity = ds.Tables[1].Rows[0];
                this.dtTopics = ds.Tables[3];
                if (this.dtTopics.Rows.Count == 0)
                {
                    return string.Empty;
                }

                this.bView = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(this.drSecurity["CanView"].ToString(), u.UserRoles);
                this.bRead = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(this.drSecurity["CanRead"].ToString(), u.UserRoles);
                StringBuilder sb = new StringBuilder(1024);
                if (this.bRead)
                {
                    this.forumName = this.drForum["ForumName"].ToString();
                    this.groupName = this.drForum["GroupName"].ToString();
                    this.forumDescription = this.drForum["ForumDesc"].ToString();

                    // TopicsTemplateId = CInt(drForum("TopicsTemplateId"))
                    this.bAllowRSS = Convert.ToBoolean(this.drForum["AllowRSS"]);
                    if (this.bAllowRSS)
                    {
                        sb.Append("<?xml version=\"1.0\" ?>" + System.Environment.NewLine);
                        sb.Append("<rss version=\"2.0\" xmlns:atom=\"http://www.w3.org/2005/Atom\" xmlns:cf=\"http://www.microsoft.com/schemas/rss/core/2005\" xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:slash=\"http://purl.org/rss/1.0/modules/slash/\">" + System.Environment.NewLine);
                        string[] @params = { ParamKeys.ForumId + "=" + forumID, ParamKeys.ViewType + "=" + Views.Topics };
                        string uRL = string.Empty;
                        uRL = DotNetNuke.Modules.ActiveForums.Utilities.NavigateURL(tabId, string.Empty, @params);
                        if (uRL.IndexOf(this.Request.Url.Host) == -1)
                        {
                            uRL = DotNetNuke.Common.Globals.AddHTTP(this.Request.Url.Host) + uRL;
                        }

                        // build channel
                        sb.Append(this.WriteElement("channel", 1));
                        sb.Append(this.WriteElement("title", HttpUtility.HtmlEncode(ps.PortalName) + " " + this.forumName, 2));
                        sb.Append(this.WriteElement("link", uRL, 2));
                        sb.Append(this.WriteElement("description", this.forumDescription, 2));
                        sb.Append(this.WriteElement("language", this.PortalSettings.DefaultLanguage, 2));
                        sb.Append(this.WriteElement("generator", "ActiveForums  5.0", 2));
                        sb.Append(this.WriteElement("copyright", this.PortalSettings.FooterText, 2));
                        sb.Append(this.WriteElement("lastBuildDate", "[LASTBUILDDATE]", 2));
                        if (!(ps.LogoFile == string.Empty))
                        {
                            string sLogo = "<image><url>http://" + this.Request.Url.Host + ps.HomeDirectory + ps.LogoFile + "</url>";
                            sLogo += "<title>" + ps.PortalName + " " + this.forumName + "</title>";
                            sLogo += "<link>" + uRL + "</link></image>";
                            sb.Append(sLogo);
                        }

                        foreach (DataRow dr in this.dtTopics.Rows)
                        {
                            if (DotNetNuke.Security.PortalSecurity.IsInRoles(this.PortalSettings.ActiveTab.TabPermissions.ToString("VIEW")))
                            {
                                // objModule = objModules.GetModule(ModuleId, TabId)
                                // If DotNetNuke.Security.PortalSecurity.IsInRoles(objModule.AuthorizedViewRoles) = True Then
                                //    sb.Append(BuildItem(dr, TabId, 2, IncludeBody, PortalId))
                                // End If
                                sb.Append(this.BuildItem(dr, tabId, 2, includeBody, portalId));
                            }
                        }

                        sb.Append("<atom:link href=\"http://" + this.Request.Url.Host + HttpUtility.HtmlEncode(this.Request.RawUrl) + "\" rel=\"self\" type=\"application/rss+xml\" />");
                        sb.Append(this.WriteElement("/channel", 1));
                        sb.Replace("[LASTBUILDDATE]", this.lastBuildDate.ToString("r"));
                        sb.Append("</rss>");

                        // Cache.Insert("RSS" & ModuleId & ForumID, sb.ToString, Nothing, DateTime.UtcNow.AddMinutes(dblCacheTimeOut), TimeSpan.Zero)
                        return sb.ToString();
                    }
                }
            }

            return string.Empty;
        }

        private string BuildItem(DataRow dr, int postTabID, int indent, bool includeBody, int portalId)
        {
            SettingsInfo mainSettings = SettingsBase.GetModuleSettings(this.moduleID);
            StringBuilder sb = new StringBuilder(1024);
            string[] @params = { ParamKeys.ForumId + "=" + dr["ForumID"].ToString(), ParamKeys.TopicId + "=" + dr["TopicId"].ToString(), ParamKeys.ViewType + "=" + Views.Topic };
            string uRL = DotNetNuke.Modules.ActiveForums.Utilities.NavigateURL(postTabID, string.Empty, @params);
            if (mainSettings.URLRewriteEnabled && !string.IsNullOrEmpty(dr["FullUrl"].ToString()))
            {
                string sTopicURL = string.Empty;
                if (!string.IsNullOrEmpty(mainSettings.PrefixURLBase))
                {
                    sTopicURL = "/" + mainSettings.PrefixURLBase;
                }

                sTopicURL += dr["FullUrl"].ToString();

                uRL = sTopicURL;
            }

            if (uRL.IndexOf(this.Request.Url.Host) == -1)
            {
                uRL = DotNetNuke.Common.Globals.AddHTTP(this.Request.Url.Host) + uRL;
            }

            if (this.lastBuildDate == new DateTime())
            {
                this.lastBuildDate = Convert.ToDateTime(dr["DateCreated"]).AddMinutes(this.offSet);
            }
            else
            {
                if (Convert.ToDateTime(dr["DateCreated"]).AddMinutes(this.offSet) > this.lastBuildDate)
                {
                    this.lastBuildDate = Convert.ToDateTime(dr["DateCreated"]).AddMinutes(this.offSet);
                }
            }

            sb.Append(this.WriteElement("item", indent));
            string body = dr["Body"].ToString();
            if (body.IndexOf("<body>") > 0)
            {
                body = TemplateUtils.GetTemplateSection(body, "<body>", "</body>");
            }

            /*
            if (body.Contains("&#91;IMAGE:"))
            {
                string strHost = DotNetNuke.Common.Globals.AddHTTP(DotNetNuke.Common.Globals.GetDomainName(Request)) + "/";
                string pattern = "(&#91;IMAGE:(.+?)&#93;)";
                Regex regExp = new Regex(pattern);
                MatchCollection matches = null;
                matches = regExp.Matches(body);
                foreach (Match match in matches)
                {
                    string sImage = "";
                    sImage = "<img src=\"" + strHost + "DesktopModules/ActiveForums/viewer.aspx?portalid=" + PortalId + "&moduleid=" + ModuleID + "&attachid=" + match.Groups[2].Value + "\" border=\"0\" />";
                    body = body.Replace(match.Value, sImage);
                }
            }
            if (body.Contains("&#91;THUMBNAIL:"))
            {
                string strHost = DotNetNuke.Common.Globals.AddHTTP(DotNetNuke.Common.Globals.GetDomainName(Request)) + "/";
                string pattern = "(&#91;THUMBNAIL:(.+?)&#93;)";
                Regex regExp = new Regex(pattern);
                MatchCollection matches = null;
                matches = regExp.Matches(body);
                foreach (Match match in matches)
                {
                    string sImage = "";
                    string thumbId = match.Groups[2].Value.Split(':')[0];
                    string parentId = match.Groups[2].Value.Split(':')[1];
                    sImage = "<a href=\"" + strHost + "DesktopModules/ActiveForums/viewer.aspx?portalid=" + PortalId + "&moduleid=" + ModuleID + "&attachid=" + parentId + "\" target=\"_blank\"><img src=\"" + strHost + "DesktopModules/ActiveForums/viewer.aspx?portalid=" + PortalId + "&moduleid=" + ModuleID + "&attachid=" + thumbId + "\" border=\"0\" /></a>";
                    body = body.Replace(match.Value, sImage);
                }
            }
             */
            body = body.Replace("src=\"/Portals", "src=\"" + DotNetNuke.Common.Globals.AddHTTP(this.Request.Url.Host) + "/Portals");
            body = Utilities.ManageImagePath(body, new Uri(Common.Globals.AddHTTP(this.Request.Url.Host)));

            sb.Append(this.WriteElement("title", dr["Subject"].ToString(), indent + 1));
            sb.Append(this.WriteElement("description", body, indent + 1));
            sb.Append(this.WriteElement("link", uRL, indent + 1));
            sb.Append(this.WriteElement("dc:creator", UserProfiles.GetDisplayName(this.PortalSettings, this.moduleID, false, false, false, -1, dr["AuthorUserName"].ToString(), dr["AuthorFirstName"].ToString(), dr["AuthorLastName"].ToString(), dr["AuthorDisplayName"].ToString(), null), indent + 1));
            sb.Append(this.WriteElement("pubDate", Convert.ToDateTime(dr["DateCreated"]).AddMinutes(this.offSet).ToString("r"), indent + 1));
            sb.Append(this.WriteElement("guid", uRL, indent + 1));
            sb.Append(this.WriteElement("slash:comments", dr["ReplyCount"].ToString(), indent + 1));
            sb.Append(this.WriteElement("/item", indent));

            return sb.ToString();
        }

        private string WriteElement(string element, int indent)
        {
            int inputLength = element.Trim().Length + 20;
            StringBuilder sb = new StringBuilder(inputLength);
            sb.Append(System.Environment.NewLine.PadRight(indent + 2, '\t'));
            sb.Append("<").Append(element).Append(">");
            return sb.ToString();
        }

        private string WriteElement(string element, string elementValue, int indent)
        {
            int inputLength = element.Trim().Length + elementValue.Trim().Length + 20;
            StringBuilder sb = new StringBuilder(inputLength);
            sb.Append(System.Environment.NewLine.PadRight(indent + 2, '\t'));
            sb.Append("<").Append(element).Append(">");
            sb.Append(this.CleanXmlString(elementValue));
            sb.Append("</").Append(element).Append(">");
            return sb.ToString();
        }

        private string CleanXmlString(string xmlString)
        {
            xmlString = this.Server.HtmlEncode(xmlString);

            // XmlString = StripHTMLTag(XmlString)
            // XmlString = Replace(XmlString, "&", "&amp;")
            // XmlString = Replace(XmlString, "<", "&lt;")
            // XmlString = Replace(XmlString, ">", "&gt;")
            return xmlString;
        }

        #endregion
        #region  Web Form Designer Generated Code

        // This call is required by the Web Form Designer.
        [System.Diagnostics.DebuggerStepThrough()]
        private void InitializeComponent()
        {
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // CODEGEN: This method call is required by the Web Form Designer
            // Do not modify it using the code editor.
            this.InitializeComponent();
        }

        #endregion
    }
}
