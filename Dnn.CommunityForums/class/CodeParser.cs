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

namespace DotNetNuke.Modules.ActiveForums
{
    using System.Text.RegularExpressions;
    using System.Web;

    public class CodeParser
    {
        public static string ParseCode(string sCode)
        {
            Regex objRegEx;
            string sOut = null;
            sCode = sCode.Replace("[", "&#91;");
            sCode = sCode.Replace("]", "&#93;");
            sCode = Regex.Replace(sCode, "(&#91;CODE&#93;)", "[CODE]", RegexOptions.IgnoreCase);
            sCode = Regex.Replace(sCode, "(&#91;\\/CODE&#93;)", "[/CODE]", RegexOptions.IgnoreCase);
            objRegEx = new Regex("\\[CODE([^>]*)\\]((.|\\n)*?)\\[/CODE\\]", RegexOptions.IgnoreCase);

            MatchCollection matches;

            matches = objRegEx.Matches(sCode);
            sOut = sCode;
            string tmp;
            string codeSnip;
            foreach (Match myMatch in matches)
            {
                codeSnip = myMatch.Result("$2");
                tmp = string.Concat("<pre><code>", codeSnip, "</code></pre>");
                if ((myMatch.Result("$1").IndexOf("vb", 0) + 1) > 0)
                {
                    tmp = HandleBrackets(tmp);
                    sCode = sCode.Replace(myMatch.Value, string.Concat("<div class=\"afcodeblock\">", tmp, "</div>"));
                }
                else if ((myMatch.Result("$1").IndexOf("html", 0) + 1) > 0)
                {
                    tmp = HandleBrackets(tmp);
                    sCode = sCode.Replace(myMatch.Value, string.Concat("<div class=\"afcodeblock\">", tmp, "</div>"));
                }
                else if ((myMatch.Result("$1").IndexOf("csharp", 0) + 1) > 0)
                {
                    tmp = HandleBrackets(tmp);
                    sCode = sCode.Replace(myMatch.Value, string.Concat("<div class=\"afcodeblock\">", tmp, "</div>"));
                }
                else if ((myMatch.Result("$1").IndexOf("script", 0) + 1) > 0)
                {
                    tmp = HandleBrackets(tmp);
                    sCode = sCode.Replace(myMatch.Value, string.Concat("<div class=\"afcodeblock\">", tmp, "</div>"));
                }
                else
                {
                    tmp = HandleBrackets(tmp);
                    sCode = sCode.Replace(myMatch.Value, string.Concat("<div class=\"afcodeblock\">", tmp, "</div>"));
                }
            }

            return HandleBrackets(sCode);
        }

        private static string HandleBrackets(string sCode)
        {
            sCode = sCode.Replace("&#91;", "[");
            sCode = sCode.Replace("&amp;#91;", "[");
            sCode = sCode.Replace("&#93;", "]");
            sCode = sCode.Replace("&amp;#93;", "]");

            return sCode;
        }
    }
}
