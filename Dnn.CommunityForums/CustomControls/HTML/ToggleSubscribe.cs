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

namespace DotNetNuke.Modules.ActiveForums.Controls
{
    using System;
    using System.Text;

    public class ToggleSubscribe
    {
        public int ToggleMode { get; set; } = 0;

        public int DisplayMode { get; set; } = 0;

        public int ModuleId { get; set; } = -1;

        public int ForumId { get; set; } = -1;

        public int TopicId { get; set; } = -1;

        public bool Checked { get; set; } = false;

        public string Text { get; set; } = string.Empty;

        public int UserId { get; set; } = -1;

        public string ImageURL { get; set; } = string.Empty;

        // amaf_topicSubscribe
        public string Render()
        {
            StringBuilder sb = new StringBuilder();
            if (this.DisplayMode == 0)
            {
                sb.Append("<span class=\"afnormal\">");
                sb.Append("<input id=\"amaf-chk-subs\" class=\"amaf-chk-subs\" type=\"checkbox\" ");
                if (this.Checked)
                {
                    sb.Append("checked=\"checked\" ");
                }

                if (this.ToggleMode == 0)
                {
                    sb.Append(" onclick=\"amaf_forumSubscribe(" + this.ModuleId + "," + this.ForumId + ");\" />");
                }
                else
                {
                    sb.Append(" onclick=\"amaf_topicSubscribe(" + this.ModuleId + "," + this.ForumId + "," + this.TopicId + ");\" />");
                }

                sb.Append("<label for=\"amaf-chk-subs\">" + this.Text + "</label>");
                sb.Append("</span>");
            }
            else
            {
                sb.Append("<img src=\"" + this.ImageURL + "\" border=\"0\" alt=\"" + this.Text + "\" onclick=\"amaf_forumSubscribe(" + this.ModuleId + "," + this.ForumId + ", " + this.UserId + ");\" id=\"amaf-sub-" + this.ForumId + "\" />");
            }

            return sb.ToString();
        }
    }
}
