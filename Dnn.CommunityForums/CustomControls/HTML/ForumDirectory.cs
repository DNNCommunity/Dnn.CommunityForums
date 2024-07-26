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
namespace DotNetNuke.Modules.ActiveForums.Controls
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Text;
    using System.Web;

    public class ForumDirectory
    {
        private int portalId = -1;

        public int PortalId
        {
            get
            {
                return this.portalId;
            }

            set
            {
                this.portalId = value;
            }
        }

        private int moduleId = -1;

        public int ModuleId
        {
            get
            {
                return this.moduleId;
            }

            set
            {
                this.moduleId = value;
            }
        }

        private int tabId = -1;

        public int TabId
        {
            get
            {
                return this.tabId;
            }

            set
            {
                this.tabId = value;
            }
        }

        public User ForumUser { get; set; }

        private string forumIds = string.Empty;

        public string ForumIds
        {
            get
            {
                return this.forumIds;
            }

            set
            {
                this.forumIds = value;
            }
        }

        private string template = string.Empty;

        public string Template
        {
            get
            {
                return this.template;
            }

            set
            {
                this.template = value;
            }
        }

        public string Render()
        {
            if (string.IsNullOrEmpty(this.Template))
            {
                return "Please specify a template";
            }

            StringBuilder sb = new StringBuilder();
            string groupTemplate = TemplateUtils.GetTemplateSection(this.Template, "[AF:DIR:FORUMGROUP]", "[/AF:DIR:FORUMGROUP]");
            string forumTemplate = TemplateUtils.GetTemplateSection(this.Template, "[AF:DIR:FORUM]", "[/AF:DIR:FORUM]");
            string subForumTemplate = TemplateUtils.GetTemplateSection(this.Template, "[AF:DIR:SUBFORUM]", "[/AF:DIR:SUBFORUM]");
            int currGroup = -1;
            string gtmp = string.Empty;
            string ftmp = string.Empty;
            string subtmp = string.Empty;
            StringBuilder list = new StringBuilder();
            var filteredForums = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetForums(this.ModuleId).Where(f => f.ForumGroup.Active && f.Active && f.ParentForumId == 0 && DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(f.Security.View, this.ForumUser.UserRoles));
            foreach (DotNetNuke.Modules.ActiveForums.Entities.ForumInfo f in filteredForums)
            {
                if (currGroup != f.ForumGroupId)
                {
                    if (!string.IsNullOrEmpty(gtmp))
                    {
                        gtmp = gtmp.Replace("[FORUMHOLDER]", string.Empty);
                        list.Append(gtmp);
                    }

                    gtmp = groupTemplate;
                    gtmp = TemplateUtils.ReplaceSubSection(gtmp, "[FORUMHOLDER]", "[AF:DIR:FORUM]", "[/AF:DIR:FORUM]");
                    gtmp = this.ParseForumGroup(f.ForumGroup, gtmp);
                    ftmp = forumTemplate;
                    ftmp = TemplateUtils.ReplaceSubSection(ftmp, "[SUBFORUMHOLDER]", "[AF:DIR:SUBFORUM]", "[/AF:DIR:SUBFORUM]");
                    subtmp = subForumTemplate;
                    currGroup = f.ForumGroupId;
                }

                string forums = this.ParseForum(f, ftmp);
                if (f.SubForums != null)
                {
                    foreach (DotNetNuke.Modules.ActiveForums.Entities.ForumInfo s in f.SubForums)
                    {
                        forums = forums.Replace("[SUBFORUMHOLDER]", this.ParseForum(s, subtmp) + "[SUBFORUMHOLDER]");
                    }
                }

                forums = forums.Replace("[SUBFORUMHOLDER]", string.Empty);
                gtmp = gtmp.Replace("[FORUMHOLDER]", forums + "[FORUMHOLDER]");
            }

            gtmp = gtmp.Replace("[FORUMHOLDER]", string.Empty);
            list.Append(gtmp);
            this.Template = TemplateUtils.ReplaceSubSection(this.Template, list.ToString(), "[AF:DIR:FORUMGROUP]", "[/AF:DIR:FORUMGROUP]");
            return this.Template;
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
