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
    using System.Linq;
    using System.Text;
    using System.Web.UI;

    [ToolboxData("<{0}:forumdisplay runat=server></{0}:forumdisplay>")]
    public class ForumDisplay : ControlsBase
    {
        #region Private Members

        #endregion
        #region Public Properties

        public int ToggleBehavior { get; set; }

        #endregion
        #region Event Handlers
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            string sTemp;

            // pt = New Forums.Utils.TimeCalcItem("ForumDisplay")
            object obj = DataCache.SettingsCacheRetrieve(this.ModuleId, string.Format(CacheKeys.ForumViewTemplate, this.ModuleId, this.ForumGroupId));
            if (obj == null)
            {
                sTemp = this.ParseTemplate();
                DataCache.SettingsCacheStore(this.ModuleId, string.Format(CacheKeys.ForumViewTemplate, this.ModuleId, this.ForumGroupId), sTemp);
            }
            else
            {
                sTemp = Convert.ToString(obj);
            }

            sTemp = Utilities.LocalizeControl(sTemp);
            if (!sTemp.Contains(Globals.ForumsControlsRegisterAFTag))
            {
                sTemp = Globals.ForumsControlsRegisterAFTag + sTemp;
            }

            Control ctl = this.Page.ParseControl(sTemp);
            this.LinkControls(ctl.Controls);
            this.Controls.Add(ctl);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            // pt.ProcEnd("ForumDisplay")
        }

        #endregion
        #region Private Methods
        private void LinkControls(ControlCollection ctrls)
        {
            foreach (Control ctrl in ctrls)
            {
                if (ctrl is ForumRow)
                {
                    ((ForumRow)ctrl).UserRoles = this.ForumUser.UserRoles;
                }

                if (ctrl is ControlsBase)
                {
                    ((ControlsBase)ctrl).ControlConfig = this.ControlConfig;
                    ((ControlsBase)ctrl).ModuleConfiguration = this.ModuleConfiguration;
                }

                if (ctrl.Controls.Count > 0)
                {
                    this.LinkControls(ctrl.Controls);
                }
            }
        }

        private string ParseTemplate()
        {
            var sb = new StringBuilder();
            string groupTemplate = this.DisplayTemplate;
            if (groupTemplate.Contains("[GROUPSECTION]"))
            {
                groupTemplate = TemplateUtils.GetTemplateSection(this.DisplayTemplate, "[GROUPSECTION]", "[/GROUPSECTION]");
            }

            var forumGroups = new DotNetNuke.Modules.ActiveForums.Controllers.ForumGroupController().Get(this.ControlConfig.ForumModuleId);
            foreach (var forumGroup in forumGroups)
            { 
                if (forumGroup.Active && (this.ForumGroupId == -1 || forumGroup.ForumGroupId == this.ForumGroupId))
                {
                    string sGroup = groupTemplate;
                    int groupId = forumGroup.ForumGroupId;
                    string groupName = forumGroup.GroupName;
                    sGroup = sGroup.Replace("[GROUPNAME]", forumGroup.GroupName);
                    bool isVisible = this.ToggleBehavior != 1;
                    sGroup = sGroup.Replace("[GROUPCOLLAPSE]", "<af:toggle IsVisible=\"" + isVisible + "\"  id=\"tgGroup" + groupId + "\" key=\"" + groupId + "\" cssclass=\"afarrow\" CssClassOn=\"aficon afarrowdown\" CssClassOff=\"aficon afarrowleft\" runat=\"server\" ImagePath=\"" + this.ThemePath + "\" />");

                    var forums = new StringBuilder();
                    int i = 0;
                    var forumsForGroup = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().Get(this.ControlConfig.ForumModuleId).Where(f => f.ForumGroupId == forumGroup.ForumGroupId).ToList();
                    foreach (var forum in forumsForGroup)
                    {
                        i += 1;
                        string sCSSClass = "afforumrowmid";
                        if (i == 1)
                        {
                            sCSSClass = "afforumrowtop";
                        }
                        else if (i == forumsForGroup.Count)
                        {
                            sCSSClass = "afforumrowbottom";
                        }

                        int fid = forum.ForumID;
                        string sForum = TemplateUtils.GetTemplateSection(sGroup, "[FORUMS]", "[/FORUMS]");
                        sForum = sForum.Replace("[CSS:ROWCLASS]", sCSSClass);
                        sForum = this.ParseForumRow(forum, sForum, groupName);

                        forums.Append(sForum);
                    }

                    sGroup = TemplateUtils.ReplaceSubSection(sGroup, forums.ToString(), "[FORUMS]", "[/FORUMS]");
                    sGroup = sGroup.Replace("[GROUP]", "<af:toggledisplay IsVisible=\"" + isVisible + "\" id=\"tgdGroup" + groupId + "\" key=\"" + groupId + "\" runat=\"server\"><content>");
                    sGroup = sGroup.Replace("[/GROUP]", "</content></af:toggledisplay>");
                    sb.Append(sGroup);
                }
            }

            string sOut = sb.ToString();
            if (sOut.Contains("[GROUPSECTION]"))
            {
                sOut = TemplateUtils.ReplaceSubSection(this.DisplayTemplate, sb.ToString(), "[GROUPSECTION]", "[/GROUPSECTION]");
            }

            return sOut;
        }

        private string ParseForumRow(DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forum, string template, string groupName)
        {
            string sForum = template;

            int lasttopicid;
            int lastreplyid;
            int fid;
            string lastpostdate = forum.LastPostDateTime.ToString();
            DateTime lastReadDate = forum.LastRead;
            string viewRoles = forum.Security.View;
            string readRoles = forum.Security.Read;
            string forumname = forum.ForumName;
            string hidden = forum.Hidden.ToString();
            lasttopicid = forum.LastTopicId;
            lastreplyid = forum.LastReplyId;

            fid = forum.ForumID;

            // TODO: Validate can view
            // sForum = sForum.Replace("[FORUMNAME]", "<af:link id=""hypForumName" & fid & """ navigateurl=""" & Utilities.NavigateUrl(PageId, "", New String() {ParamKeys.ViewType & "=" & Views.Topics, ParamKeys.ForumId & "=" & fid}) & """ text=""" & forumname & """ runat=""server"" />") 'GetForumLink(forumname, PageId, True))
            sForum = sForum.Replace("[FORUMNAME]", "<af:link id=\"hypForumName" + fid + "\" navigateurl=\"" + URL.ForForum(this.PageId, fid, string.Empty, forumname) + "\" text=\"" + forumname + "\" runat=\"server\" />"); // GetForumLink(forumname, PageId, True))
            sForum = sForum.Replace("[FORUMNAMENOLINK]", forumname);
            sForum = sForum.Replace("[FORUMDESCRIPTION]", forum.ForumDesc);
            sForum = sForum.Replace("[TOTALTOPICS]", forum.TotalTopics.ToString());
            sForum = sForum.Replace("[TOTALREPLIES]", forum.TotalReplies.ToString());
            sForum = sForum.Replace("[DISPLAYNAME]", "<i class=\"fa fa-user fa-fw fa-blue\"></i>&nbsp;" + forum.LastPost.Author.DisplayName);
            sForum = sForum.Replace("[LASTPOST]", "<asp:placeholder id=\"plhLastPost" + fid + "\" runat=\"server\">");
            sForum = sForum.Replace("[/LASTPOST]", "</asp:placeholder>");

            int intLength = 0;
            if ((sForum.IndexOf("[LASTPOSTSUBJECT:", 0) + 1) > 0)
            {
                int inStart = sForum.IndexOf("[LASTPOSTSUBJECT:", 0) + 1 + 17;
                int inEnd = sForum.IndexOf("]", inStart - 1) + 1;
                string sLength = sForum.Substring(inStart - 1, inEnd - inStart);
                intLength = Convert.ToInt32(sLength);
            }

            string replaceTag = "[LASTPOSTSUBJECT:" + intLength.ToString() + "]";
            string sSubject = forum.LastPostSubject;
            if (lastreplyid == 0)
            {
                lastreplyid = lasttopicid;
            }

            sSubject = this.GetLastPostSubject(lastreplyid, lasttopicid, fid, sSubject, intLength);

            sForum = sForum.Replace(replaceTag, sSubject);
            if (sSubject == string.Empty)
            {
                sForum = sForum.Replace("[RESX:BY]", string.Empty);
                sForum = sForum.Replace("[LASTPOSTDATE]", string.Empty);
            }
            else
            {
                sForum = sForum.Replace("[LASTPOSTDATE]", lastpostdate);
            }

            sForum = sForum.Replace("[FORUMICON]", "<div style=\"height:30px;margin=right:10px;\"><i class=\"fa fa-folder fa-2x fa-blue\"></i></div>");
            sForum = sForum.Replace("[CSS:FORUMICON]", "affoldernorm");
            sForum = sForum.Replace("[CSS:FORUMICONSM]", "affoldersmnorm");

            // sForum = sForum.Replace("[FORUMICONSM]", sIconImage.Replace("folder", "folder16"));
            sForum = sForum.Replace("[FORUMICONSM]", string.Empty);
            string sSubs = string.Empty;
            if (forum.SubForums.Count > 0)
            {
                string sTemplate = TemplateUtils.GetTemplateSection(template, "[SUBFORUMS]", "[/SUBFORUMS]");
                foreach (var subforum in forum.SubForums)
                {
                    sSubs += this.ParseForumRow(subforum, sTemplate, groupName);
                }
            }

            sForum = TemplateUtils.ReplaceSubSection(sForum, sSubs, "[SUBFORUMS]", "[/SUBFORUMS]");
            string fc = "<af:forumrow id=\"ctlFR" + fid + "\" Hidden=\"" + hidden + "\" ForumId=\"" + fid + "\" ReadRoles=\"" + readRoles + "\" ViewRoles=\"" + viewRoles + "\" runat=\"server\">";
            fc += "<content>" + sForum + "</content>";
            fc += "</af:forumrow>";
            return fc;
        }


        #endregion
        #region Public Methods
        private string GetLastPostSubject(int lastPostID, int parentPostID, int fid, string subject, int length)
        {
            string sOut = string.Empty;
            int postId = lastPostID;
            subject = Utilities.StripHTMLTag(subject);
            subject = subject.Replace("[", "&#91");
            subject = subject.Replace("]", "&#93");
            if (lastPostID != 0)
            {
                if (subject.Length > length)
                {
                    subject = subject.Substring(0, length) + "...";
                }

                string sURL;
                if (parentPostID == 0 || lastPostID == parentPostID)
                {
                    sURL = Utilities.NavigateURL(this.PageId, string.Empty, new[] { ParamKeys.ForumId + "=" + fid, ParamKeys.ViewType + "=" + Views.Topic, ParamKeys.TopicId + "=" + postId });
                }
                else
                {
                    sURL = Utilities.NavigateURL(this.PageId, string.Empty, new[] { ParamKeys.ForumId + "=" + fid, ParamKeys.ViewType + "=" + Views.Topic, ParamKeys.TopicId + "=" + parentPostID, ParamKeys.ContentJumpId + "=" + postId });
                }

                sOut = "<af:link id=\"hypLastPostSubject" + fid + "\" NavigateUrl=\"" + sURL + "\" Text=\"" + System.Net.WebUtility.HtmlEncode(subject) + "\" runat=\"server\" />";
            }

            return sOut;
        }
        #endregion
    }
}
