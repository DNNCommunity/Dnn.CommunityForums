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

namespace DotNetNuke.Modules.ActiveForums
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Text.RegularExpressions;
    using System.Web;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Security;

    public partial class Utilities
    {
        public class Text
        {
            public static string CheckSqlString(string input)
            {
                input = input.ToUpperInvariant();
                input = input.Replace("\\", string.Empty);
                input = input.Replace("[", string.Empty);
                input = input.Replace("]", string.Empty);
                input = input.Replace("(", string.Empty);
                input = input.Replace(")", string.Empty);
                input = input.Replace("{", string.Empty);
                input = input.Replace("}", string.Empty);
                input = input.Replace("'", "''");
                input = input.Replace("UNION", string.Empty);
                input = input.Replace("TABLE", string.Empty);
                input = input.Replace("WHERE", string.Empty);
                input = input.Replace("DROP", string.Empty);
                input = input.Replace("EXECUTE", string.Empty);
                input = input.Replace("EXEC ", string.Empty);
                input = input.Replace("FROM ", string.Empty);
                input = input.Replace("CMD ", string.Empty);
                input = input.Replace(";", string.Empty);
                input = input.Replace("--", string.Empty);

                // input = input.Replace("""", """""")
                return input;
            }

            public static string FilterScripts(string text)
            {
                if (string.IsNullOrEmpty(text))
                {
                    return string.Empty;
                }

                text = System.Net.WebUtility.HtmlEncode(text);
                string pattern = "<script.*/*>|</script>|<[a-zA-Z][^>]*=['\"]+javascript:\\w+.*['\"]+>|<\\w+[^>]*\\son\\w+=.*[ /]*>";
                text = RegexUtils.GetCachedRegex(pattern, RegexOptions.Compiled & RegexOptions.IgnoreCase).Replace(text, string.Empty);
                string strip = "/*,*/,alert,document.,window.,eval(,eval[,@import,vbscript,javascript,jscript,msgbox";
                foreach (string s in strip.Split(','))
                {
                    if (text.ToUpper().Contains(s.ToUpper()))
                    {
                        text = text.Replace(s.ToUpper(), string.Empty);
                        text = text.Replace(s, string.Empty);
                    }
                }

                return text;
            }

            public static string RemoveHTML(string sText)
            {
                if (string.IsNullOrEmpty(sText))
                {
                    return string.Empty;
                }

                sText = System.Net.WebUtility.HtmlDecode(sText);
                sText = System.Net.WebUtility.UrlDecode(sText);
                sText = sText.Trim();
                if (string.IsNullOrEmpty(sText))
                {
                    return string.Empty;
                }

                sText = System.Net.WebUtility.HtmlEncode(sText);
                sText = FilterScripts(sText);
                string strip = "/*,*/,alert,document.,window.,eval(,eval[,src=,rel=,href=,@import,vbscript,javascript,jscript,msgbox,<style";
                foreach (string s in strip.Split(','))
                {
                    if (sText.ToUpper().Contains(s.ToUpper()))
                    {
                        sText = sText.Replace(s.ToUpper(), string.Empty);
                        sText = sText.Replace(s, string.Empty);
                    }
                }

                string pattern = "<(.|\\n)*?>";
                sText = RegexUtils.GetCachedRegex(pattern, RegexOptions.IgnoreCase).Replace(sText, string.Empty);
                sText = System.Net.WebUtility.HtmlEncode(sText);

                return sText;
            }
        }
    }
}
