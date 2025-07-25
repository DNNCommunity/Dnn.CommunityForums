﻿// Copyright (c) by DNN Community
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

namespace DotNetNuke.Modules.ActiveForums.Handlers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using System.Web;

    public class HandlerBase : System.Web.IHttpHandler
    {
        internal enum OutputCodes : int
        {
            Success,
            UnsupportedRequest,
            AuthenticationFailed,
            Exception,
            NoResults,
            AccessDenied,
        }

        private Hashtable _params;
        private bool _isValid = false;
        private int _gid = -1;
        private int _groupid = -1;
        private int _upid = -1;
        private DotNetNuke.Entities.Portals.PortalSettings _ps;
        private SettingsInfo _mainSettings;
        private bool _AdminRequired = false;

        public bool AdminRequired
        {
            get
            {
                return this._AdminRequired;
            }

            set
            {
                this._AdminRequired = value;
            }
        }

        private int _pid = -1;
        private int _mid = -1;
        private int _UserId = -1;

        public int UserId
        {
            get
            {
                return this._UserId;
            }

            set
            {
                this._UserId = value;
            }
        }

        public int PortalId
        {
            get
            {
                if (HttpContext.Current.Request.QueryString["PortalId"] != null && Utilities.IsNumeric(HttpContext.Current.Request.QueryString["PortalId"]))
                {
                    return int.Parse(HttpContext.Current.Request.QueryString["PortalId"]);
                }
                else
                {
                    return this._pid;
                }
            }
        }

        public int ModuleId
        {
            get
            {
                if (HttpContext.Current.Request.QueryString["ModuleId"] != null && Utilities.IsNumeric(HttpContext.Current.Request.QueryString["ModuleId"]))
                {
                    return int.Parse(HttpContext.Current.Request.QueryString["ModuleId"]);
                }
                else
                {
                    return this._mid;
                }
            }
        }

        public int TabId
        {
            get
            {
                if (HttpContext.Current.Request.QueryString["TabId"] != null && Utilities.IsNumeric(HttpContext.Current.Request.QueryString["TabId"]))
                {
                    return int.Parse(HttpContext.Current.Request.QueryString["TabId"]);
                }
                else
                {
                    return -1;
                }
            }
        }

        public bool IsDebug
        {
            get
            {
                if (HttpContext.Current.Request.QueryString["amdebug"] != null)
                {
                    return bool.Parse(HttpContext.Current.Request.QueryString["amdebug"]);
                }
                else
                {
                    return false;
                }
            }
        }

        public SettingsInfo MainSettings
        {
            get
            {
                return this._mainSettings;
            }
        }

        public bool IsValid
        {
            get
            {
                return this._isValid;
            }
        }

        public DotNetNuke.Entities.Portals.PortalSettings PS
        {
            get
            {
                return this._ps;
            }
        }

        public int RequestOption
        {
            get
            {
                if (HttpContext.Current.Request.QueryString["opt"] != null && Utilities.IsNumeric(HttpContext.Current.Request.QueryString["opt"]))
                {
                    return int.Parse(HttpContext.Current.Request.QueryString["opt"]);
                }
                else
                {
                    return -1;
                }
            }
        }

        public Hashtable Params
        {
            get
            {
                return this._params;
            }
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public int UPID => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public bool IsAuthenticated
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public string Username
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        internal DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo ForumUser
        {
            get
            {
                return new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController(this.ModuleId).GetUserFromHttpContext(this.PortalId, this.ModuleId);
            }
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public virtual bool IsReusable
        {
            get
            {
                return false;
            }
        }

        public virtual void ProcessRequest(System.Web.HttpContext context)
        {
            try
            {
                if (HttpContext.Current.Items["PortalSettings"] != null)
                {
                    this._ps = (DotNetNuke.Entities.Portals.PortalSettings)HttpContext.Current.Items["PortalSettings"];
                    this._pid = this._ps.PortalId;
                }
                else
                {
                    DotNetNuke.Entities.Portals.PortalAliasInfo objPortalAliasInfo = null;
                    string sUrl = HttpContext.Current.Request.RawUrl.Replace("http://", string.Empty)
                        .Replace("https://", string.Empty);
                    objPortalAliasInfo =
                        DotNetNuke.Entities.Portals.PortalAliasController.Instance.GetPortalAlias(HttpContext.Current
                            .Request.Url.Host);
                    this._pid = objPortalAliasInfo.PortalID;
                    this._ps = DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings();
                }

                this._mainSettings = SettingsBase.GetModuleSettings(this.ModuleId);

                this._isValid = true;
                if (this.AdminRequired & !context.Request.IsAuthenticated)
                {
                    this._isValid = false;
                    return;
                }

                if (this.AdminRequired && context.Request.IsAuthenticated)
                {
                    DotNetNuke.Entities.Modules.ModuleController objMC =
                        new DotNetNuke.Entities.Modules.ModuleController();
                    DotNetNuke.Entities.Modules.ModuleInfo objM = objMC.GetModule(this.ModuleId, this.TabId);
                    string roleIds = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetPortalRoleIds(this.PortalId, objM.ModulePermissions.ToString("EDIT").Split(';'));
                    this._isValid = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIdsFromRoleString(roleIds), this.ForumUser.UserRoleIds);
                }
                else if (this.AdminRequired & !context.Request.IsAuthenticated)
                {
                    this._isValid = false;
                    return;
                }

                string p = HttpContext.Current.Request.Params["p"];
                if (!string.IsNullOrEmpty(p))
                {
                    this._params = Utilities.JSON.ConvertFromJSONAssoicativeArrayToHashTable(p);
                }

                if (context.Request.Files.Count == 0)
                {
                    string jsonPost = string.Empty;
                    string prop = string.Empty;
                    string val = string.Empty;
                    string tmp = string.Empty;
                    bool bObj = false;

                    // Arrays
                    List<string> slist = null;

                    // Dim pairs As NameValueCollection = Nothing
                    Hashtable pairs = null;
                    Hashtable subPairs = null;

                    Hashtable ht = new Hashtable();
                    int idx = 0;
                    string parentProp = string.Empty;
                    string skip = "{}[]:," + ((char)34).ToString();
                    using (System.IO.StreamReader sr =
                           new System.IO.StreamReader(context.Request.InputStream, System.Text.Encoding.UTF8))
                    {
                        while (!sr.EndOfStream)
                        {
                            char c = (char)sr.Read();
                            if (idx > 0 && c == '[')
                            {
                                c = (char)sr.Read();
                                bObj = true;
                            }

                            if (idx > 0 && c == '{')
                            {
                                if (pairs == null)
                                {
                                    parentProp = prop;
                                    prop = string.Empty;
                                    tmp = string.Empty;

                                    // pairs = New NameValueCollection
                                    pairs = new Hashtable();
                                }
                                else if (subPairs == null)
                                {
                                    string subString = c.ToString();
                                    while (c != '}')
                                    {
                                        c = (char)sr.Read();
                                        subString += c;
                                        if (c == '}')
                                        {
                                            break;
                                        }
                                    }

                                    subPairs = Utilities.JSON.ConvertFromJSONAssoicativeArrayToHashTable(subString);
                                    pairs.Add(prop, subPairs);
                                    prop = string.Empty;
                                    tmp = string.Empty;
                                    subPairs = null;
                                    c = (char)sr.Read();
                                }
                            }

                            if (idx > 0 && bObj == true && !(c == '{'))
                            {
                                string subItem = string.Empty;
                                while (c != ']')
                                {
                                    if (slist == null)
                                    {
                                        slist = new List<string>();
                                    }

                                    if (skip.IndexOf(c) == -1)
                                    {
                                        subItem += c;
                                    }

                                    c = (char)sr.Read();
                                    if (c == ',' || c == ']')
                                    {
                                        slist.Add(subItem);
                                        subItem = string.Empty;
                                    }

                                    if (c == ']')
                                    {
                                        c = (char)sr.Read();
                                        bObj = false;
                                        break;
                                    }
                                }
                            }

                            if (c == ':')
                            {
                                prop = tmp;
                                tmp = string.Empty;
                            }

                            if (skip.IndexOf(c) == -1)
                            {
                                tmp += c;
                            }

                            if (c == ',' || c == '}')
                            {
                                if (!string.IsNullOrEmpty(tmp))
                                {
                                    tmp = System.Net.WebUtility.UrlDecode(tmp);
                                }

                                if (slist != null)
                                {
                                    ht.Add(prop, slist);
                                    slist = null;
                                }
                                else if (pairs != null && c == ',' && !string.IsNullOrEmpty(prop))
                                {
                                    pairs.Add(prop, tmp);
                                }
                                else if (pairs != null && c == '}')
                                {
                                    if (!string.IsNullOrEmpty(tmp))
                                    {
                                        pairs.Add(prop, tmp);
                                    }

                                    ht.Add(parentProp, pairs);
                                    parentProp = string.Empty;
                                    pairs = null;
                                }
                                else if (!string.IsNullOrEmpty(prop))
                                {
                                    ht.Add(prop, tmp);
                                }

                                prop = string.Empty;
                                tmp = string.Empty;
                            }

                            idx += 1;
                        }

                        if (pairs != null & !string.IsNullOrEmpty(parentProp))
                        {
                            ht.Add(parentProp, pairs);
                        }
                        else if (!string.IsNullOrEmpty(prop) && !string.IsNullOrEmpty(tmp))
                        {
                            ht.Add(prop, System.Net.WebUtility.UrlDecode(tmp));
                        }
                        else if (!string.IsNullOrEmpty(prop) && slist != null)
                        {
                            ht.Add(prop, slist);
                        }

                        // jsonPost = sr.ReadToEnd()
                        sr.Close();
                    }

                    this._params = ht;

                    // End If
                }
                else
                {
                    Hashtable ht = new Hashtable();
                    foreach (string s in context.Request.Params.AllKeys)
                    {
                        if (!ht.ContainsKey(s))
                        {
                            ht.Add(s, context.Request.Params[s]);
                        }
                    }

                    this._params = ht;
                }

                if (HttpContext.Current.Request.IsAuthenticated)
                {
                    this.UserId = DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.GetUserIdByUserName(
                        this.PortalId,
                        HttpContext.Current.User.Identity.Name);
                }
                else
                {
                    this.UserId = -1;
                }
            }
            catch (Exception ex)
            {
                this._isValid = false;
                Exceptions.LogException(ex);
            }
        }

        internal string BuildOutput(string text, OutputCodes code, bool success)
        {
            return this.BuildOutput(text, code, success, false);
        }

        internal string BuildOutput(string text, OutputCodes code, bool success, bool resultisobject)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            sb.Append("{");
            sb.Append("\"success\":");
            if (success)
            {
                sb.Append("true,");
            }
            else
            {
                sb.Append("false,");
            }

            if (!success)
            {
                switch (code)
                {
                    case OutputCodes.Exception:
                        sb.Append(Utilities.JSON.Pair("error", text));
                        break;
                    case OutputCodes.AuthenticationFailed:
                        sb.Append(Utilities.JSON.Pair("error", "Authentication Failed"));
                        break;
                    case OutputCodes.UnsupportedRequest:
                        sb.Append(Utilities.JSON.Pair("error", "Unsupported Request"));
                        break;
                    case OutputCodes.NoResults:
                        sb.Append(Utilities.JSON.Pair("error", "No Results"));
                        break;
                    case OutputCodes.AccessDenied:
                        sb.Append(Utilities.JSON.Pair("error", "Access Denied"));

                        break;
                }

                sb.Append(",");
            }

            if (string.IsNullOrEmpty(text))
            {
                resultisobject = true;
                text = "null";
            }

            sb.Append("\"result\":");
            if (resultisobject)
            {
                sb.Append(text);
            }
            else
            {
                sb.Append("\"" + Utilities.JSON.EscapeJsonString(text) + "\"");
            }

            sb.Append("}");
            if (this.IsDebug)
            {
                sb.Append(",{");
                foreach (string s in this.Params.Keys)
                {
                    sb.Append(Utilities.JSON.Pair(s, this.Params[s].ToString()));
                    sb.Append(",");
                }

                sb.Append(Utilities.JSON.Pair("userid", this.UserId.ToString()));
                sb.Append(",");
                sb.Append(Utilities.JSON.Pair("url", HttpContext.Current.Request.RawUrl.ToString()));
                sb.Append("}");
            }

            sb.Append("]");
            return sb.ToString();
        }
    }
}
