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
using System.Linq;
using System.Text;
using System.Web;

namespace DotNetNuke.Modules.ActiveForums.Controls
{
	public class ForumDirectory
	{
		private int _PortalId = -1;
		public int PortalId
		{
			get
			{
				return _PortalId;
			}
			set
			{
				_PortalId = value;
			}
		}
		private int _ModuleId = -1;
		public int ModuleId
		{
			get
			{
				return _ModuleId;
			}
			set
			{
				_ModuleId = value;
			}
		}
		private int _TabId = -1;
		public int TabId
		{
			get
			{
				return _TabId;
			}
			set
			{
				_TabId = value;
			}
		}
		public User ForumUser {get; set;}
		private string _ForumIds = string.Empty;
		public string ForumIds
		{
			get
			{
				return _ForumIds;
			}
			set
			{
				_ForumIds = value;
			}
		}
		private string _Template = string.Empty;
		public string Template
		{
			get
			{
				return _Template;
			}
			set
			{
				_Template = value;
			}
		}

		public string Render()
		{
			if (string.IsNullOrEmpty(Template))
			{
				return "Please specify a template";
			}
			StringBuilder sb = new StringBuilder();
            string groupTemplate = TemplateUtils.GetTemplateSection(Template, "[AF:DIR:FORUMGROUP]", "[/AF:DIR:FORUMGROUP]");
			string forumTemplate = TemplateUtils.GetTemplateSection(Template, "[AF:DIR:FORUM]", "[/AF:DIR:FORUM]");
			string subForumTemplate = TemplateUtils.GetTemplateSection(Template, "[AF:DIR:SUBFORUM]", "[/AF:DIR:SUBFORUM]");
			int currGroup = -1;
			string gtmp = string.Empty;
			string ftmp = string.Empty;
			string subtmp = string.Empty;
			StringBuilder list = new StringBuilder();
			var filteredForums = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetForums(ModuleId).Where(f =>
            {
                return f.ForumGroup.Active && f.Active && f.ParentForumId == 0 && DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(f.Security.View, ForumUser.UserRoles);
            });
			foreach (DotNetNuke.Modules.ActiveForums.Entities.ForumInfo f in filteredForums)
			{
				if (currGroup != f.ForumGroupId)
				{
					if (! (string.IsNullOrEmpty(gtmp)))
					{
						gtmp = gtmp.Replace("[FORUMHOLDER]", string.Empty);
						list.Append(gtmp);
					}
					gtmp = groupTemplate;
					gtmp = TemplateUtils.ReplaceSubSection(gtmp, "[FORUMHOLDER]", "[AF:DIR:FORUM]", "[/AF:DIR:FORUM]");
					gtmp = ParseForumGroup(f.ForumGroup, gtmp);
					ftmp = forumTemplate;
					ftmp = TemplateUtils.ReplaceSubSection(ftmp, "[SUBFORUMHOLDER]", "[AF:DIR:SUBFORUM]", "[/AF:DIR:SUBFORUM]");
					subtmp = subForumTemplate;
					currGroup = f.ForumGroupId;
				}
				string forums = ParseForum(f, ftmp);
				if (f.SubForums != null)
				{
					foreach (DotNetNuke.Modules.ActiveForums.Entities.ForumInfo s in f.SubForums)
					{
						forums = forums.Replace("[SUBFORUMHOLDER]", ParseForum(s, subtmp) + "[SUBFORUMHOLDER]");
					}
				}
				forums = forums.Replace("[SUBFORUMHOLDER]", string.Empty);
				gtmp = gtmp.Replace("[FORUMHOLDER]", forums + "[FORUMHOLDER]");

			}
			gtmp = gtmp.Replace("[FORUMHOLDER]", string.Empty);
			list.Append(gtmp);
			Template = TemplateUtils.ReplaceSubSection(Template, list.ToString(), "[AF:DIR:FORUMGROUP]", "[/AF:DIR:FORUMGROUP]");
			return Template;
		}
		private string ParseForumGroup(DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo f, string template)
		{
			template = template.Replace("[AF:DIR:FORUMGROUPID]", f.ForumGroupId.ToString());
			template = template.Replace("[AF:DIR:FORUMGROUPNAME]", f.GroupName);
			return template;
		}
		private string ParseForum(DotNetNuke.Modules.ActiveForums.Entities.ForumInfo f, string template)
		{
			template = template.Replace("[AF:DIR:FORUMID]", f.ForumID.ToString());
			template = template.Replace("[AF:DIR:FORUMNAME]", f.ForumName);
			template = template.Replace("[AF:DIR:FORUMDESC]", f.ForumDesc);
			template = template.Replace("[AF:DIR:FORUMURL]", f.ForumURL);
			template = template.Replace("[AF:DIR:TOTALTOPICS]", f.TotalTopics.ToString());
			template = template.Replace("[AF:DIR:TOTALREPLIES]", f.TotalReplies.ToString());
			template = template.Replace("[AF:DIR:FORUMGROUPID]", f.ForumGroupId.ToString());
			template = template.Replace("[AF:DIR:PARENTFORUMID]", f.ParentForumId.ToString());
			string selected = string.Empty;
			if (HttpContext.Current.Request.QueryString[ParamKeys.ForumId] != null)
			{
				selected = int.Parse(HttpContext.Current.Request.QueryString[ParamKeys.ForumId]) == f.ForumID ? "afn-currentforum" : string.Empty;
            }
			template = f.ParentForumId > 0
                ? template.Replace("[AF:DIR:SELECTEDSUBFORUM]", selected)
                : template.Replace("[AF:DIR:SELECTEDFORUM]", selected);

            return template;
		}
	}
}
