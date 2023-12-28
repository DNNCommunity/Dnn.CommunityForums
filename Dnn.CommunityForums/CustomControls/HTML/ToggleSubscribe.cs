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

using System.Text;

namespace DotNetNuke.Modules.ActiveForums.Controls
{
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
		[Obsolete("Deprecated in Community Forums. Removing in 09.00.00. Use ToggleSubscribe(int ModuleId, int ForumId, int TopicId, int ToggleMode)")]
        public ToggleSubscribe(int m, int f, int t)
		{
            ModuleId = -1; 
			ToggleMode = m;
			ForumId = f;
            TopicId = t;    

        }

        public ToggleSubscribe(int ModuleId, int ForumId, int TopicId, int ToggleMode)
        {
			this.ToggleMode = ToggleMode;
            this.ModuleId = ModuleId;
            this.ForumId = ForumId;
            this.TopicId = TopicId; 
        }
		//amaf_topicSubscribe
		public string Render()
		{
			StringBuilder sb = new StringBuilder();
			if (DisplayMode == 0)
			{
				sb.Append("<span class=\"afnormal\">");
				sb.Append("<input id=\"amaf-chk-subs\" class=\"amaf-chk-subs\" type=\"checkbox\" ");
				if (Checked)
				{
					sb.Append("checked=\"checked\" ");
				}
				if (ToggleMode == 0)
				{
					sb.Append(" onclick=\"amaf_forumSubscribe(" + ModuleId + "," + ForumId + ");\" />");
				}
				else
				{
					sb.Append(" onclick=\"amaf_topicSubscribe(" + ModuleId + "," + ForumId + "," + TopicId + ");\" />");
				}

				sb.Append("<label for=\"amaf-chk-subs\">" + Text + "</label>");
				sb.Append("</span>");
			}
			else
			{
				sb.Append("<img src=\"" + ImageURL + "\" border=\"0\" alt=\"" + Text + "\" onclick=\"amaf_forumSubscribe(" + ModuleId + "," + ForumId + ", " + UserId + ");\" id=\"amaf-sub-" + ForumId + "\" />");
			}

			return sb.ToString();
		}

	}
}

