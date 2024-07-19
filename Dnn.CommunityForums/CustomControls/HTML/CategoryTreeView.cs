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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

using System.Text;

namespace DotNetNuke.Modules.ActiveForums.Controls
{
	public class CategoryTreeView
	{
		public enum GroupingOptions: int
		{
			None,
			Categories,
			Forums
		}

        public int PortalId { get; set; } = -1;
        public int ModuleId { get; set; } = -1;
        public int TabId { get; set; } = -1;
        public int ForumId { get; set; } = -1;
        public int ForumGroupId { get; set; } = -1;
        public int ParentForumId { get; set; } = -1;
        public GroupingOptions GroupBy { get; set; } = GroupingOptions.None;
        public int TopicId { get; set; } = -1;
        public string Topic { get; set; } = string.Empty;
        public string ItemTemplate { get; set; } = string.Empty;
        public string HeaderTemplate { get; set; } = string.Empty;
        public string FooterTemplate { get; set; } = string.Empty;
        public DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo ForumUser {get; set;}
        public bool IncludeClasses { get; set; } = true;

        public string Render()
		{
			StringBuilder sb = new StringBuilder();
			Data.CommonDB db = new Data.CommonDB();
			string sHost = Utilities.GetHost();
			if (sHost.EndsWith("/"))
			{
				sHost = sHost.Substring(0, sHost.Length - 1);
			}
			ControlUtils ctlUtils = new ControlUtils();
			string forumPrefix = string.Empty;
			string groupPrefix = string.Empty;
			//Dim _forumGroupId As Integer = -1
			if (ParentForumId == -1)
			{
				ParentForumId = ForumId;
			}
			using (IDataReader dr = db.ForumContent_List(PortalId, ModuleId, ForumGroupId, ForumId, ParentForumId))
			{
				//ParentForum Section
				dr.Read();
				//SubForums
				dr.NextResult();
				dr.Read();
				//Topics in ParentForum
				dr.NextResult();
				string catKey = string.Empty;
				int count = 0;
				int catCount = 0;
				sb.Append("<ul>");
				while (dr.Read())
				{
					if (catKey != dr["CategoryName"].ToString() + dr["CategoryId"].ToString())
					{
						if (count > 0)
						{
							sb.Replace("[CATCOUNT]", catCount.ToString());
							sb.Append("</ul></li>");
							count = 0;
							catCount = 0;
						}

						sb.Append("<li class=\"category\" id=\"afcat-" + dr["CategoryId"].ToString() + "\">");


						sb.Append("<em>[CATCOUNT]</em>");
						sb.Append("<span>" + dr["CategoryName"].ToString() + "</span>");
						sb.Append("<ul>");

						catKey = dr["CategoryName"].ToString() + dr["CategoryId"].ToString();
					}
					if (TopicId == Convert.ToInt32(dr["TopicId"].ToString()))
					{
						sb.Append("<li class=\"fcv-selected\">");
						sb.Replace("<li class=\"category\" id=\"afcat-" + dr["CategoryId"].ToString() + "\">", "<li class=\"category cat-selected\" id=\"afcat-" + dr["CategoryId"].ToString() + "\">");
					}
					else
					{
						sb.Append("<li>");
					}
					catCount += 1;
					//Dim Params As String() = {ParamKeys.ForumId & "=" & ForumId, ParamKeys.TopicId & "=" & TopicId, ParamKeys.ViewType & "=topic"}
					string[] Params = {ParamKeys.TopicId + "=" + dr["TopicId"].ToString()};
					//Dim sTopicURL As String = ctlUtils.BuildUrl(TabId, ModuleId, groupPrefix, forumPrefix, ForumGroupId, ForumId, Integer.Parse(dr("TopicId").ToString), dr("URL").ToString, -1, -1, String.Empty, 1)
					string sTopicURL = ctlUtils.TopicURL(dr, TabId, ModuleId);
					sb.Append("<a href=\"" + sTopicURL + "\"><span>" + dr["Subject"].ToString() + "</span></a></li>");
					if (TopicId > 0)
					{
						if (Convert.ToInt32(dr["TopicId"].ToString()) == TopicId)
						{
							//  RenderTopic(dr)
						}
					}

					count += 1;
				}
				sb.Replace("[CATCOUNT]", catCount.ToString());
				if (count > 0)
				{
					sb.Append("</ul></li>");
				}
				sb.Append("</ul>");
				dr.Close();
			}
			return sb.ToString();
		}
		//Private Sub RenderTopic(ByVal dr As IDataRecord)
		//    Dim topicsTemplate As String = TemplateUtils.GetTemplateSection(TopicTemplate, "[TOPIC]", "[/TOPIC]")
		//    Dim replyTemplate As String = TemplateUtils.GetTemplateSection(TopicTemplate, "[REPLIES]", "[/REPLIES]")
		//    TopicTemplate = TemplateUtils.ReplaceSubSection(TopicTemplate, String.Empty, "[REPLIES]", "[/REPLIES]")
		//    topicsTemplate = topicsTemplate.Replace("[BODY]", dr("Body").ToString)
		//    topicsTemplate = topicsTemplate.Replace("[ACTIONS:DELETE]", String.Empty)
		//    topicsTemplate = topicsTemplate.Replace("[ACTIONS:EDIT]", String.Empty)
		//    topicsTemplate = topicsTemplate.Replace("[ACTIONS:ALERT]", String.Empty)
		//    topicsTemplate = topicsTemplate.Replace("[ATTACHMENTS]", String.Empty)
		//    topicsTemplate = topicsTemplate.Replace("[SIGNATURE]", String.Empty)
		//    TopicTemplate = TemplateUtils.ReplaceSubSection(TopicTemplate, topicsTemplate, "[TOPIC]", "[/TOPIC]")


		//    TopicTemplate = TopicTemplate.Replace("[ADDREPLY]", String.Empty)
		//    TopicTemplate = TopicTemplate.Replace("[POSTRATINGBUTTON]", String.Empty)
		//    TopicTemplate = TopicTemplate.Replace("[TOPICSUBSCRIBE]", String.Empty)
		//    Topic = TopicTemplate

		//End Sub
	}
}

