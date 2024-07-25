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
    using System.Web;

    using DotNetNuke.Common.Utilities.Internal;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;

    public class ForumsReWriter : IHttpModule
    {
        private int forumgroupId = -1;
        private int forumId = -1;
        private int tabId = -1;
        private int moduleId = -1;
        private int topicId = -1;
        private int page = 0;
        private int contentId = -1;
        private int userId = -1;
        private int archived = 0;
        private SettingsInfo mainSettings = null;
        private int urlType = 0; // 0=default, 1= views, 2 = category, 3 = tag
        private int otherId = -1;
        private int categoryId = -1;
        private int tagId = -1;

        public void Dispose()
        {

        }

        public void Init(System.Web.HttpApplication context)
        {
            context.BeginRequest += this.OnBeginRequest;
        }

        public void OnBeginRequest(object s, EventArgs e)
        {
            this.forumId = -1;
            this.tabId = -1;
            this.moduleId = -1;
            this.topicId = -1;
            this.page = 0;
            this.contentId = -1;
            this.archived = 0;
            this.forumgroupId = -1;
            this.mainSettings = null;
            this.categoryId = -1;
            this.tagId = -1;

            HttpApplication app = (HttpApplication)s;
            HttpServerUtility Server = app.Server;
            HttpRequest Request = app.Request;
            HttpResponse Response = app.Response;
            string requestedPath = app.Request.Url.AbsoluteUri;
            HttpContext Context = ((HttpApplication)s).Context;

            if (Request.Url.LocalPath.ToLowerInvariant().Contains(".axd")
                || Request.Url.LocalPath.ToLowerInvariant().Contains(".js")
                || Request.Url.LocalPath.ToLowerInvariant().Contains(".aspx")
                || Request.Url.LocalPath.ToLowerInvariant().Contains(".gif")
                || Request.Url.LocalPath.ToLowerInvariant().Contains(".jpg")
                || Request.Url.LocalPath.ToLowerInvariant().Contains(".css")
                || Request.Url.LocalPath.ToLowerInvariant().Contains(".png")
                || Request.Url.LocalPath.ToLowerInvariant().Contains(".swf")
                || Request.Url.LocalPath.ToLowerInvariant().Contains(".htm")
                || Request.Url.LocalPath.ToLowerInvariant().Contains(".html")
                || Request.Url.LocalPath.ToLowerInvariant().Contains(".ashx")
                || Request.Url.LocalPath.ToLowerInvariant().Contains(".cur")
                || Request.Url.LocalPath.ToLowerInvariant().Contains(".ico")
                || Request.Url.LocalPath.ToLowerInvariant().Contains(".txt")
                || Request.Url.LocalPath.ToLowerInvariant().Contains(".pdf")
                || Request.Url.LocalPath.ToLowerInvariant().Contains(".xml")
                || Request.Url.LocalPath.ToLowerInvariant().Contains(".csv")
                || Request.Url.LocalPath.ToLowerInvariant().Contains(".xls")
                || Request.Url.LocalPath.ToLowerInvariant().Contains(".xlsx")
                || Request.Url.LocalPath.ToLowerInvariant().Contains(".doc")
                || Request.Url.LocalPath.ToLowerInvariant().Contains(".docx")
                || Request.Url.LocalPath.ToLowerInvariant().Contains(".ppt")
                || Request.Url.LocalPath.ToLowerInvariant().Contains(".pptx")
                || Request.Url.LocalPath.ToLowerInvariant().Contains(".zip")
                || Request.Url.LocalPath.ToLowerInvariant().Contains(".zipx")
                || Request.Url.LocalPath.ToLowerInvariant().Contains("/api/")
                || Request.Url.LocalPath.ToLowerInvariant().Contains("/portals/")
                || Request.Url.LocalPath.ToLowerInvariant().Contains("/desktopmodules/")
                || Request.Url.LocalPath.ToLowerInvariant().Contains(".eot")
                || Request.Url.LocalPath.ToLowerInvariant().Contains(".ttf")
                || Request.Url.LocalPath.ToLowerInvariant().Contains(".otf")
                || Request.Url.LocalPath.ToLowerInvariant().Contains(".svg")
                || Request.Url.LocalPath.ToLowerInvariant().Contains(".webp")
                || Request.Url.LocalPath.ToLowerInvariant().Contains(".woff")
                || Request.Url.LocalPath.ToLowerInvariant().Contains(".woff2"))
            {
                return;
            }

            DotNetNuke.Entities.Portals.PortalAliasInfo objPortalAliasInfo = DotNetNuke.Entities.Portals.PortalAliasController.Instance.GetPortalAlias(HttpContext.Current.Request.Url.Host);
            if (objPortalAliasInfo == null && !HttpContext.Current.Request.Url.IsDefaultPort)
            {
                objPortalAliasInfo = PortalAliasController.Instance.GetPortalAlias(HttpContext.Current.Request.Url.Host + ":" + HttpContext.Current.Request.Url.Port.ToString());
            }

            if (objPortalAliasInfo == null)
            {
                return;
            }

            int PortalId = objPortalAliasInfo.PortalID;

            string sUrl = HttpContext.Current.Request.RawUrl.Replace("http://", string.Empty).Replace("https://", string.Empty);
            // TODO: this is all probably now handled by moving the exclusion logic earlier and may be redundant?
            if (Request.RawUrl.ToLowerInvariant().Contains("404.aspx"))
            {
                string sEx = ".jpg,.gif,.png,.swf,.js,.css,.html,.htm,desktopmodules,portals,.ashx,.ico,.txt,.doc,.docx,.pdf,.xml,.xls,.xlsx,.ppt,.pptx,.csv,.zip,.asmx,.aspx";
                foreach (string sn in sEx.Split(','))
                {
                    if (sUrl.Contains(sn))
                    {
                        // IO.File.AppendAllText(sPath, Request.RawUrl & "165<br />")
                        return;
                    }
                }
            }

            string searchURL = sUrl;
            searchURL = searchURL.Replace(objPortalAliasInfo.HTTPAlias, string.Empty);
            if (searchURL.Length < 2)
            {
                return;
            }

            string query = string.Empty;
            if (searchURL.Contains("?"))
            {
                query = searchURL.Substring(searchURL.IndexOf("?"));
                searchURL = searchURL.Substring(0, searchURL.IndexOf("?") - 1);
            }

            string newSearchURL = string.Empty;
            foreach (string up in searchURL.Split('/'))
            {
                if (!string.IsNullOrEmpty(up))
                {
                    if (!SimulateIsNumeric.IsNumeric(up))
                    {
                        newSearchURL += up + "/";
                    }
                }
            }

            bool canContinue = false;
            string tagName = string.Empty;
            string catName = string.Empty;
            if (newSearchURL.Contains("/category/") || newSearchURL.Contains("/tag/"))
            {
                if (newSearchURL.Contains("/category/"))
                {
                    string cat = "/category/";
                    int iEnd = newSearchURL.IndexOf("/", newSearchURL.IndexOf(cat) + cat.Length + 1);
                    string catString = newSearchURL.Substring(newSearchURL.IndexOf(cat), iEnd - newSearchURL.IndexOf(cat));
                    catName = catString.Replace(cat, string.Empty);
                    catName = catName.Replace("/", string.Empty);
                    newSearchURL = newSearchURL.Replace(catString, string.Empty);
                }

                if (newSearchURL.Contains("/tag/"))
                {
                    string tag = "/tag/";
                    int iEnd = newSearchURL.IndexOf("/", newSearchURL.IndexOf(tag) + tag.Length + 1);
                    string tagString = newSearchURL.Substring(newSearchURL.IndexOf(tag), iEnd - newSearchURL.IndexOf(tag));
                    tagName = tagString.Replace(tag, string.Empty);
                    tagName = tagName.Replace("/", string.Empty);
                    newSearchURL = newSearchURL.Replace(tagString, string.Empty);
                }
            }

            if ((sUrl.Contains(ParamKeys.ViewType) && sUrl.Contains("post")) | (sUrl.Contains(ParamKeys.ViewType) && sUrl.Contains("confirmaction")) | (sUrl.Contains(ParamKeys.ViewType) && sUrl.Contains("sendto")) | (sUrl.Contains(ParamKeys.ViewType) && sUrl.Contains("modreport")) | (sUrl.Contains(ParamKeys.ViewType) && sUrl.Contains("search")) | sUrl.Contains("dnnprintmode") || (sUrl.Contains(ParamKeys.ViewType) && sUrl.Contains(Views.ModerateTopics)))
            {
                return;
            }

            Data.Common db = new Data.Common();
            try
            {
                using (IDataReader dr = db.URLSearch(PortalId, newSearchURL))
                {
                    while (dr.Read())
                    {
                        this.tabId = int.Parse(dr["TabID"].ToString());
                        this.moduleId = int.Parse(dr["ModuleId"].ToString());
                        this.forumgroupId = int.Parse(dr["ForumGroupId"].ToString());
                        this.forumId = int.Parse(dr["ForumId"].ToString());
                        this.topicId = int.Parse(dr["TopicId"].ToString());
                        this.archived = int.Parse(dr["Archived"].ToString());
                        this.otherId = int.Parse(dr["OtherId"].ToString());
                        this.urlType = int.Parse(dr["UrlType"].ToString());
                        canContinue = true;
                    }

                    dr.Close();
                }
            }
            catch
            {

            }

            if (!string.IsNullOrEmpty(catName))
            {
                this.categoryId = db.Tag_GetIdByName(PortalId, this.moduleId, catName, true);
                this.otherId = this.categoryId;
                this.urlType = 2;
            }

            if (!string.IsNullOrEmpty(tagName))
            {
                this.tagId = db.Tag_GetIdByName(PortalId, this.moduleId, tagName, false);
                this.otherId = this.tagId;
                this.urlType = 3;
            }

            if (this.archived == 1)
            {
                sUrl = db.GetUrl(this.moduleId, this.forumgroupId, this.forumId, this.topicId, this.userId, -1);
                if (!string.IsNullOrEmpty(sUrl))
                {
                    string sHost = objPortalAliasInfo.HTTPAlias;
                    if (sUrl.StartsWith("/"))
                    {
                        sUrl = sUrl.Substring(1);
                    }

                    if (!sHost.EndsWith("/"))
                    {
                        sHost += "/";
                    }

                    sUrl = sHost + sUrl;
                    if (!sUrl.EndsWith("/"))
                    {
                        sUrl += "/";
                    }

                    if (!sUrl.StartsWith("http"))
                    {
                        if (Request.IsSecureConnection)
                        {
                            sUrl = "https://" + sUrl;
                        }
                        else
                        {
                            sUrl = "http://" + sUrl;
                        }
                    }

                    Response.Clear();
                    Response.Status = "301 Moved Permanently";
                    Response.AddHeader("Location", sUrl);
                    Response.End();

                }
            }

            if (this.moduleId > 0)
            {
                this.mainSettings = new SettingsInfo { MainSettings = DotNetNuke.Entities.Modules.ModuleController.Instance.GetModule(moduleId: this.moduleId, tabId: this.tabId, ignoreCache: false).ModuleSettings };

            }

            if (this.mainSettings == null)
            {
                return;
            }

            if (!this.mainSettings.URLRewriteEnabled)
            {
                return;
            }

            if (!canContinue && (Request.RawUrl.Contains(ParamKeys.TopicId) || Request.RawUrl.Contains(ParamKeys.ForumId) || Request.RawUrl.Contains(ParamKeys.GroupId)))
            {
                sUrl = this.HandleOldUrls(Request.RawUrl, objPortalAliasInfo.HTTPAlias);
                if (!string.IsNullOrEmpty(sUrl))
                {
                    if (!sUrl.StartsWith("http"))
                    {
                        if (Request.IsSecureConnection)
                        {
                            sUrl = "https://" + sUrl;
                        }
                        else
                        {
                            sUrl = "http://" + sUrl;
                        }
                    }

                    Response.Clear();
                    Response.Status = "301 Moved Permanently";
                    Response.AddHeader("Location", sUrl);
                    Response.End();
                }
            }

            if (!canContinue)
            {
                string topicUrl = string.Empty;
                if (newSearchURL.EndsWith("/"))
                {
                    newSearchURL = newSearchURL.Substring(0, newSearchURL.Length - 1);
                }

                if (newSearchURL.Contains("/"))
                {
                    topicUrl = newSearchURL.Substring(newSearchURL.LastIndexOf("/"));
                }

                topicUrl = topicUrl.Replace("/", string.Empty);
                if (!string.IsNullOrEmpty(topicUrl))
                {
                    Data.Topics topicsDb = new Data.Topics();
                    this.topicId = topicsDb.TopicIdByUrl(PortalId, this.moduleId, topicUrl.ToLowerInvariant());
                    if (this.topicId > 0)
                    {
                        sUrl = db.GetUrl(this.moduleId, this.forumgroupId, this.forumId, this.topicId, this.userId, -1);
                    }
                    else
                    {
                        sUrl = string.Empty;
                    }

                    if (!string.IsNullOrEmpty(sUrl))
                    {
                        string sHost = objPortalAliasInfo.HTTPAlias;
                        if (sHost.EndsWith("/") && sUrl.StartsWith("/"))
                        {
                            sUrl = sHost.Substring(0, sHost.Length - 1) + sUrl;
                        }
                        else if (!sHost.EndsWith("/") && !sUrl.StartsWith("/"))
                        {
                            sUrl = sHost + "/" + sUrl;
                        }
                        else
                        {
                            sUrl = sHost + sUrl;
                        }

                        if (sUrl.StartsWith("/"))
                        {
                            sUrl = sUrl.Substring(1);
                        }

                        if (!sUrl.StartsWith("http"))
                        {
                            if (Request.IsSecureConnection)
                            {
                                sUrl = "https://" + sUrl;
                            }
                            else
                            {
                                sUrl = "http://" + sUrl;
                            }
                        }

                        if (!string.IsNullOrEmpty(sUrl))
                        {
                            Response.Clear();
                            Response.Status = "301 Moved Permanently";
                            Response.AddHeader("Location", sUrl);
                            Response.End();
                        }
                    }
                }

            }

            if (canContinue)
            {
                // avoid redirect, e.g. "/Forums" == "forums/"
                if ((searchURL.StartsWith("/") || searchURL.EndsWith("/")) && searchURL.IndexOf("/") == searchURL.LastIndexOf("/") &&
                    (newSearchURL.StartsWith("/") || newSearchURL.EndsWith("/")) && newSearchURL.IndexOf("/") == newSearchURL.LastIndexOf("/") &&
                     searchURL.Replace("/", string.Empty).ToLowerInvariant() == newSearchURL.Replace("/", string.Empty).ToLowerInvariant())
                {
                    return;
                }

                if (searchURL != newSearchURL)
                {
                    string urlTail = searchURL.Replace(newSearchURL, string.Empty);
                    if (urlTail.StartsWith("/"))
                    {
                        urlTail = urlTail.Substring(1);
                    }

                    if (urlTail.EndsWith("/"))
                    {
                        urlTail = urlTail.Substring(0, urlTail.Length - 1);
                    }

                    if (urlTail.Contains("/"))
                    {
                        urlTail = urlTail.Substring(0, urlTail.IndexOf("/") - 1);
                    }

                    if (SimulateIsNumeric.IsNumeric(urlTail))
                    {
                        this.page = Convert.ToInt32(urlTail);
                    }
                }

                string sPage = this.page != 0 ? $"&{ParamKeys.PageId}={this.page}" : string.Empty;
                string qs = string.Empty;
                if (sUrl.Contains("?"))
                {
                    qs = "&" + sUrl.Substring(sUrl.IndexOf("?") + 1);
                }

                string catQS = string.Empty;
                if (this.categoryId > 0)
                {
                    catQS = $"&{ParamKeys.Category}=" + this.categoryId.ToString();
                }

                string sendTo = string.Empty;
                if ((this.topicId > 0) || (this.forumId > 0) || (this.forumgroupId > 0))
                {
                    sendTo = ResolveUrl(app.Context.Request.ApplicationPath, $"~/default.aspx?tabid={this.tabId}" +
                                (this.forumgroupId > 0 ? $"&{ParamKeys.GroupId}={this.forumgroupId}" : string.Empty) +
                                (this.forumId > 0 ? $"&{ParamKeys.ForumId}={this.forumId}" : string.Empty) +
                                (this.topicId > 0 ? $"&{ParamKeys.TopicId}={this.topicId}" : string.Empty) +
                                sPage + qs +
                                ((this.forumgroupId > 0 || this.forumId > 0) ? catQS : string.Empty));
                }
                else if (this.urlType == 2 && this.otherId > 0)
                {
                    sendTo = ResolveUrl(app.Context.Request.ApplicationPath, "~/default.aspx?tabid=" + this.tabId + $"&{ParamKeys.Category}=" + this.otherId + sPage + qs);
                }
                else if (this.urlType == 3 && this.otherId > 0)
                {
                    sendTo = ResolveUrl(app.Context.Request.ApplicationPath, "~/default.aspx?tabid=" + this.tabId + $"&{ParamKeys.ViewType}={Views.Grid}&{ParamKeys.GridType}={Views.Tags}&{ParamKeys.Tags}=" + this.otherId + sPage + qs);
                }
                else if (this.urlType == 1)
                {
                    string v = string.Empty;
                    switch (this.otherId)
                    {
                        case 1:
                            v = GridTypes.Unanswered;
                            break;
                        case 2:
                            v = GridTypes.NotRead;
                            break;
                        case 3:
                            v = GridTypes.MyTopics;
                            break;
                        case 4:
                            v = GridTypes.ActiveTopics;
                            break;
                        case 5:
                            v = GridTypes.MySettings;
                            break;
                        case 6:
                            v = GridTypes.MostLiked;
                            break;
                        case 7:
                            v = GridTypes.MostReplies;
                            break;
                        case 8:
                            v = GridTypes.MySubscriptions;
                            break;
                        case 9:
                            v = GridTypes.Announcements;
                            break;
                        case 10:
                            v = GridTypes.Unresolved;
                            break;

                    }

                    sendTo = ResolveUrl(app.Context.Request.ApplicationPath, "~/default.aspx?tabid=" + this.tabId + $"&{ParamKeys.ViewType}={Views.Grid}&{ParamKeys.GridType}=" + v + sPage + qs);
                }
                else if (this.tabId > 0)
                {
                    sendTo = ResolveUrl(app.Context.Request.ApplicationPath, "~/default.aspx?tabid=" + this.tabId + sPage + qs);
                }

                RewriteUrl(app.Context, sendTo);
            }
        }

        internal static string ResolveUrl(string appPath, string url)
        {

            // String is Empty, just return Url
            if (url.Length == 0)
            {
                return url;
            }

            // String does not contain a ~, so just return Url
            if (url.StartsWith("~") == false)
            {
                return url;
            }

            // There is just the ~ in the Url, return the appPath
            if (url.Length == 1)
            {
                return appPath;
            }

            if (url.ToCharArray()[1] == '/' || url.ToCharArray()[1] == '\\')
            {
                // Url looks like ~/ or ~\
                if (appPath.Length > 1)
                {
                    return appPath + "/" + url.Substring(2);
                }
                else
                {
                    return "/" + url.Substring(2);
                }
            }
            else
            {
                // Url look like ~something
                if (appPath.Length > 1)
                {
                    return appPath + "/" + url.Substring(1);
                }
                else
                {
                    return appPath + url.Substring(1);
                }
            }

        }

        internal static void RewriteUrl(HttpContext context, string sendToUrl)
        {
            string x = "";
            string y = "";

            RewriteUrl(context, sendToUrl, ref x, ref y);
        }

        internal static void RewriteUrl(HttpContext context, string sendToUrl, ref string sendToUrlLessQString, ref string filePath)
        {

            // first strip the querystring, if any
            string queryString = string.Empty;
            sendToUrlLessQString = sendToUrl;

            if (sendToUrl.IndexOf("?") > 0)
            {
                sendToUrlLessQString = sendToUrl.Substring(0, sendToUrl.IndexOf("?"));
                queryString = sendToUrl.Substring(sendToUrl.IndexOf("?") + 1);
            }

            // grab the file's physical path
            filePath = string.Empty;
            filePath = context.Server.MapPath(sendToUrlLessQString);

            // rewrite the path..
            if (sendToUrlLessQString.Contains("/404.aspx?404;"))
            {
                sendToUrlLessQString = sendToUrlLessQString.Substring(sendToUrlLessQString.IndexOf("/404.aspx?404;") + 14);

            }

            context.RewritePath(sendToUrlLessQString, string.Empty, queryString);

            // NOTE!  The above RewritePath() overload is only supported in the .NET Framework 1.1
            // If you are using .NET Framework 1.0, use the below form instead:
            // context.RewritePath(sendToUrl);

        }

        private string HandleOldUrls(string rawUrl, string httpAlias)
        {
            string sUrl = string.Empty;
            string currURL = rawUrl;
            string splitter = "/";
            if (currURL.Contains("&"))
            {
                splitter = "&";
            }

            string[] parts = currURL.Split(Convert.ToChar(splitter));
            for (int i = 0; i < parts.Length; i++)
            {
                if (!string.IsNullOrEmpty(parts[i]))
                {
                    if (parts[i].ToLowerInvariant().Contains("="))
                    {
                        string[] pair = parts[i].Split('=');
                        if (pair[0].ToLowerInvariant() == ParamKeys.ForumId)
                        {
                            this.forumId = int.Parse(pair[1]);
                        }
                        else if (pair[0].ToLowerInvariant() == ParamKeys.TopicId)
                        {
                            this.topicId = int.Parse(pair[1]);
                        }
                        else if (pair[0].ToLowerInvariant() == ParamKeys.PageId)
                        {
                            this.page = int.Parse(pair[1]);
                        }
                        else if (pair[0].ToLowerInvariant() == ParamKeys.PageJumpId)
                        {
                            this.page = int.Parse(pair[1]);
                        }
                        else if (pair[0].ToLowerInvariant() == ParamKeys.ContentJumpId)
                        {
                            this.contentId = int.Parse(pair[1]);
                        }
                    }
                    else
                    {
                        if (parts[i].ToLowerInvariant() == ParamKeys.ForumId)
                        {
                            this.forumId = int.Parse(parts[i + 1]);
                        }
                        else if (parts[i].ToLowerInvariant() == ParamKeys.TopicId)
                        {
                            this.topicId = int.Parse(parts[i + 1]);
                        }
                        else if (parts[i].ToLowerInvariant() == ParamKeys.PageId)
                        {
                            this.page = int.Parse(parts[i + 1]);
                        }
                        else if (parts[i].ToLowerInvariant() == ParamKeys.PageJumpId)
                        {
                            this.page = int.Parse(parts[i + 1]);
                        }
                        else if (parts[i].ToLowerInvariant() == "tabid")
                        {
                            this.tabId = int.Parse(parts[i + 1]);
                        }
                        else if (parts[i].ToLowerInvariant() == ParamKeys.ContentJumpId)
                        {
                            this.contentId = int.Parse(parts[i + 1]);
                        }
                    }
                }
            }

            Data.Common db = new Data.Common();
            sUrl = db.GetUrl(this.moduleId, this.forumgroupId, this.forumId, this.topicId, this.userId, this.contentId);
            if (!string.IsNullOrEmpty(sUrl))
            {

                string sHost = httpAlias;
                if (sUrl.StartsWith("/"))
                {
                    sUrl = sUrl.Substring(1);
                }

                if (!sHost.EndsWith("/"))
                {
                    sHost += "/";
                }

                sUrl = sHost + sUrl;
                if (!sUrl.EndsWith("/"))
                {
                    sUrl += "/";
                }

                if (this.contentId > 0)
                {
                    if (this.page > 1)
                    {
                        sUrl += this.page + "/";
                    }
                }

            }

            return sUrl;
        }
    }
}
