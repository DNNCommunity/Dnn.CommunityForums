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
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Web;
using DotNetNuke.Modules.ActiveForums.Entities;

namespace DotNetNuke.Modules.ActiveForums
{
    [Obsolete("Deprecated in Community Forums. Not Used. Use DotNetNuke.Modules.ActiveForums.Controllers.TokenController()")]
    public class TokensController { TokensController() { throw new NotImplementedException(); } }    
}
    namespace DotNetNuke.Modules.ActiveForums.Controllers
{
    internal static class TokenController
    {
        internal static List<Token> List()
        {
            return List(string.Empty);
        }
        internal static List<Token> List(string group)
        {
            try
            {
                List<Token> li = (List<Token>)DataCache.CacheRetrieve(string.Format(CacheKeys.Tokens, group));
                if (li == null)
                {
                    li = new List<Token>();
                    Token tk = null;
                    System.Xml.XmlDocument xDoc = new System.Xml.XmlDocument();
                    string sPath = HttpContext.Current.Server.MapPath(Globals.ModulePath + "config/tokens.config");
                    xDoc.Load(sPath);
                    if (xDoc != null)
                    {
                        System.Xml.XmlNode xRoot = xDoc.DocumentElement;
                        string sQuery = "//tokens/token";
                        if (!(group == string.Empty))
                        {
                            sQuery = string.Concat(sQuery, "[@group='", group, "' or @group='*']");
                        }
                        System.Xml.XmlNodeList xNodeList = xRoot.SelectNodes(sQuery);
                        if (xNodeList.Count > 0)
                        {
                            int i = 0;
                            for (i = 0; i < xNodeList.Count; i++)
                            {
                                tk = new Token();
                                tk.Group = xNodeList[i].Attributes["group"].Value;
                                tk.TokenTag = xNodeList[i].Attributes["name"].Value;
                                if (xNodeList[i].Attributes["value"] != null)
                                {
                                    tk.TokenReplace = Utilities.HTMLDecode(xNodeList[i].Attributes["value"].Value);
                                }
                                else
                                {
                                    tk.TokenReplace = Utilities.HTMLDecode(xNodeList[i].ChildNodes[0].InnerText);
                                }

                                li.Add(tk);
                            }
                        }
                    }
                    DataCache.CacheStore(string.Format(CacheKeys.Tokens, group), li);
                }
                return li;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        internal static string Get(string group, string TokenName)
        {
            string sOut = string.Empty;
            List<Token> tl = List(group);
            foreach (Token t in tl)
            {
                if (t.TokenTag == TokenName)
                {
                    sOut = t.TokenReplace;
                    break;
                }
            }
            return sOut;
        }
    }
}

