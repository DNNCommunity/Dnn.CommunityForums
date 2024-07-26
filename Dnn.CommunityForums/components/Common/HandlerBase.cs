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
namespace DotNetNuke.Modules.ActiveForums.Handlers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
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
            AccessDenied
        }

        private Hashtable @params;
        private bool isValid = false;
        private int upid = -1;
        private DotNetNuke.Entities.Portals.PortalSettings ps;
        private SettingsInfo mainSettings;
        private bool adminRequired = false;

        public bool AdminRequired
        {
            get
            {
                return this.adminRequired;
            }

            set
            {
                this.adminRequired = value;
            }
        }

        private int pid = -1;
        private int mid = -1;
        private int userId = -1;

        public int UserId
        {
            get
            {
                return this.userId;
            }

            set
            {
                this.userId = value;
            }
        }

        public int PortalId
        {
            get
            {
                if (HttpContext.Current.Request.QueryString["PortalId"] != null && SimulateIsNumeric.IsNumeric(HttpContext.Current.Request.QueryString["PortalId"]))
                {
                    return int.Parse(HttpContext.Current.Request.QueryString["PortalId"]);
                }
                else
                {
                    return this.pid;
                }
            }
        }

        public int ModuleId
        {
            get
            {
                if (HttpContext.Current.Request.QueryString["ModuleId"] != null && SimulateIsNumeric.IsNumeric(HttpContext.Current.Request.QueryString["ModuleId"]))
                {
                    return int.Parse(HttpContext.Current.Request.QueryString["ModuleId"]);
                }
                else
                {
                    return this.mid;
                }
            }
        }

        public int TabId
        {
            get
            {
                if (HttpContext.Current.Request.QueryString["TabId"] != null && SimulateIsNumeric.IsNumeric(HttpContext.Current.Request.QueryString["TabId"]))
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
                return this.mainSettings;
            }
        }

        public bool IsValid
        {
            get
            {
                return this.isValid;
            }
        }

        public DotNetNuke.Entities.Portals.PortalSettings PS
        {
            get
            {
                return this.ps;
            }
        }

        public int RequestOption
        {
            get
            {
                if (HttpContext.Current.Request.QueryString["opt"] != null && SimulateIsNumeric.IsNumeric(HttpContext.Current.Request.QueryString["opt"]))
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
                return this.@params;
            }
        }

        public int UPID
        {
            get
            {
                return this.upid;
            }
        }

        private bool isAuthenticated = false;

        public bool IsAuthenticated
        {
            get
            {
                return this.isAuthenticated;
            }

            set
            {
                this.isAuthenticated = value;
            }
        }

        private string username = string.Empty;

        public string Username
        {
            get
            {
                return this.username;
            }

            set
            {
                this.username = value;
            }
        }

        internal User ForumUser
        {
            get
            {
                UserController uc = new UserController();
                return uc.GetUser(this.PortalId, this.ModuleId);
            }
        }

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
                    this.ps = (DotNetNuke.Entities.Portals.PortalSettings)HttpContext.Current.Items["PortalSettings"];
                    this.pid = this.ps.PortalId;
                }
                else
                {
                    DotNetNuke.Entities.Portals.PortalAliasInfo objPortalAliasInfo = null;
                    string sUrl = HttpContext.Current.Request.RawUrl.Replace("http://", string.Empty).Replace("https://", string.Empty);
                    objPortalAliasInfo = DotNetNuke.Entities.Portals.PortalAliasController.Instance.GetPortalAlias(HttpContext.Current.Request.Url.Host);
                    this.pid = objPortalAliasInfo.PortalID;
                    this.ps = DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings();

                }

                // Dim sc As New Social.SocialSettings
                // _mainSettings = sc.LoadSettings[_ps.PortalId]
                this.mainSettings = SettingsBase.GetModuleSettings(this.ModuleId);
                // If context.Request.IsAuthenticated Then
                this.isValid = true;
                if (this.AdminRequired & !context.Request.IsAuthenticated)
                {
                    this.isValid = false;
                    return;
                }

                if (this.AdminRequired && context.Request.IsAuthenticated)
                {
                    // _isValid = DotNetNuke.Security.PortalSecurity.IsInRole(_ps.AdministratorRoleName)
                    DotNetNuke.Entities.Modules.ModuleController objMC = new DotNetNuke.Entities.Modules.ModuleController();
                    DotNetNuke.Entities.Modules.ModuleInfo objM = objMC.GetModule(this.ModuleId, this.TabId);
                    string roleIds = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIds(this.PortalId, objM.ModulePermissions.ToString("EDIT").Split(';'));
                    this.isValid = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasAccess(roleIds, this.ForumUser.UserRoles);
                }
                else if (this.AdminRequired & !context.Request.IsAuthenticated)
                {
                    this.isValid = false;
                    return;
                }

                string p = HttpContext.Current.Request.Params["p"];
                if (!string.IsNullOrEmpty(p))
                {
                    this.@params = Utilities.JSON.ConvertFromJSONAssoicativeArrayToHashTable(p);
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
                    using (System.IO.StreamReader sr = new System.IO.StreamReader(context.Request.InputStream, System.Text.Encoding.UTF8))
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
                                    tmp = HttpUtility.UrlDecode(tmp);
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
                            ht.Add(prop, HttpUtility.UrlDecode(tmp));
                        }
                        else if (!string.IsNullOrEmpty(prop) && slist != null)
                        {
                            ht.Add(prop, slist);
                        }

                        // jsonPost = sr.ReadToEnd()
                        sr.Close();
                    }

                    this.@params = ht;
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

                    this.@params = ht;
                }

                if (HttpContext.Current.Request.IsAuthenticated)
                {
                    this.UserId = UserController.GetUserIdByUserName(this.PortalId, HttpContext.Current.User.Identity.Name);
                }
                else
                {
                    this.UserId = -1;
                }

            }
            catch (Exception ex)
            {
                this.isValid = false;
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
