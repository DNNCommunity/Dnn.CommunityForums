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

namespace DotNetNuke.Modules.ActiveForums.Controllers
{
    internal class EmoticonController
    {
        public static string LoadEmoticons(int moduleId, string imagePath, EditorTypes insertType)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("<div id=\"emotions\" class=\"afemoticons\"><div id=\"emotions\" style=\"width:100%; height:100%;align:center;\">");

            int i = 0;
            foreach (var emoticon in new DotNetNuke.Modules.ActiveForums.Controllers.FilterController().GetEmoticons(moduleId))
            {
                string sEmotPath = imagePath + emoticon.Replace;
                string sInsert = insertType == EditorTypes.TEXTBOX ? emoticon.Find : $"<img src=\\'{sEmotPath}\\' />";
                sb.AppendFormat("<span class=\"afEmot\" style=\"width:20px;height:20px;cursor:hand;\" unselectable=\"on\" onclick=\"amaf_insertHTML('{0}')\"><img onmousedown=\"return false;\" src=\"{1}\" width=\"20\" height=\"20\" title=\"{2}\" /></span>", sInsert, sEmotPath, emoticon.Find);

                i += 1;
                if (i % 2 == 0)
                {
                    sb.Append("<br />");
                }
            }

            sb.Append("</div></div>");
            return sb.ToString();
        }
    }
}
