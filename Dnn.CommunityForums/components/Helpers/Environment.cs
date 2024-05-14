// Copyright (c) 2013-2024 by DNN Community
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

namespace DotNetNuke.Modules.ActiveForums
{
    using System;
    using System.Reflection;
    using System.Web.UI;

    using DotNetNuke.Instrumentation;

    public class Environment
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Environment));

        public static bool UpdateBreadCrumb(ControlCollection ctrls, string forumBread)
        {
            if (string.IsNullOrEmpty(forumBread))
            {
                return true;
            }

            string[] bcText = forumBread.Split('|');
            try
            {
                foreach (Control ctrl in ctrls)
                {
                    if (ctrl is DotNetNuke.UI.Skins.SkinObjectBase && ctrl.TemplateControl.AppRelativeVirtualPath != null)
                    {
                        if (ctrl.TemplateControl.AppRelativeVirtualPath.ToLowerInvariant().Contains("breadcrumb.ascx"))
                        {
                            object o = ctrl.GetType().GetProperty("Separator").GetValue(ctrl, BindingFlags.Public | BindingFlags.NonPublic, null, null, null);
                            object cssObject = ctrl.GetType().GetProperty("CssClass").GetValue(ctrl, BindingFlags.Public | BindingFlags.NonPublic, null, null, null);
                            string css = "SkinObject";
                            if (cssObject != null)
                            {
                                if (!string.IsNullOrEmpty(cssObject.ToString()))
                                {
                                    css = cssObject.ToString();
                                }
                            }

                            string sText = string.Empty;
                            if (o != null)
                            {
                                sText = o.ToString();
                            }

                            string sBread = string.Empty;
                            foreach (string s in bcText)
                            {
                                if (!string.IsNullOrEmpty(s))
                                {
                                    var newValue = s.Replace("<a ", "<a class=\"" + css + "\" ");
                                    sBread += sText + newValue;
                                }
                            }

                            ((System.Web.UI.WebControls.Label)ctrl.FindControl("lblBreadCrumb")).Text += sBread;
                            break;
                        }
                    }

                    if (ctrl.Controls.Count > 0)
                    {
                        UpdateBreadCrumb(ctrl.Controls, forumBread);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool UpdateMeta(ref DotNetNuke.Framework.CDefault bp, string title, string description, string keywords)
        {
            if (bp == null)
            {
                return false;
            }

            try
            {
                if (!string.IsNullOrEmpty(title))
                {
                    if (!string.IsNullOrEmpty(bp.Title))
                    {
                        bp.Title = title.Replace("[VALUE]", bp.Title);
                    }
                    else
                    {
                        bp.Title = title.Replace("[VALUE]", string.Empty);
                    }
                }

                if (!string.IsNullOrEmpty(description))
                {
                    description = description.Replace(System.Environment.NewLine, " ");
                    if (!string.IsNullOrEmpty(bp.Description))
                    {
                        bp.Description = description.Replace("[VALUE]", bp.Description);
                    }
                    else
                    {
                        bp.Description = description.Replace("[VALUE]", string.Empty);
                    }
                }

                if (!string.IsNullOrEmpty(keywords))
                {
                    if (!string.IsNullOrEmpty(bp.KeyWords))
                    {
                        string cKey = bp.KeyWords.Trim();
                        if (cKey.StartsWith(","))
                        {
                            cKey = cKey.Substring(1);
                        }
                        else if (cKey.EndsWith(","))
                        {
                            cKey = cKey.Substring(0, cKey.Length - 1);
                        }

                        if (keywords.StartsWith("[VALUE]"))
                        {
                            cKey += ",";
                        }
                        else if (keywords.EndsWith("[VALUE]"))
                        {
                            cKey = "," + cKey;
                        }

                        bp.KeyWords = keywords.Replace("[VALUE]", cKey);
                    }
                    else
                    {
                        bp.KeyWords = keywords.Replace("[VALUE]", string.Empty);
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex != null)
                {
                    Logger.Error(ex.Message, ex);
                    if (ex.InnerException != null)
                    {
                        Logger.Error(ex.InnerException.Message, ex.InnerException);
                    }
                }

                return false;
            }

            return false;
        }
    }
}
