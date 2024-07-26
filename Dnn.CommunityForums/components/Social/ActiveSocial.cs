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
namespace DotNetNuke.Modules.ActiveForums
{
    using System;
    using System.Data;
    using System.Web;

    using DotNetNuke.Modules.ActiveForums.Data;
    using DotNetNuke.Services.Journal;
    using DotNetNuke.Web.Models;
    using Microsoft.ApplicationBlocks.Data;

    public class Social
    {
        [Obsolete("Deprecated in Community Forums 9.0.0. No interface with Active Social.")]
        public void AddTopicToJournal(int portalId, int moduleId, int forumId, int topicId, int userId, string uRL, string subject, string summary, string body, int securityOption, string readRoles, int socialGroupId)
        {
            this.AddTopicToJournal(portalId: portalId, moduleId: moduleId, tabId: -1, forumId: forumId, topicId: topicId, userId: userId, uRL: uRL, subject: subject, summary: summary, body: body, readRoles: readRoles, socialGroupId: socialGroupId);
        }

        internal void AddTopicToJournal(int portalId, int moduleId, int tabId, int forumId, int topicId, int userId, string uRL, string subject, string summary, string body, string readRoles, int socialGroupId)
        {
            var ji = new JournalItem
            {
                PortalId = portalId,
                ProfileId = userId,
                UserId = userId,
                Title = subject,
                ItemData = new ItemData { Url = uRL },
            };
            if (string.IsNullOrEmpty(summary))
            {
                summary = Utilities.StripHTMLTag(body);
                if (summary.Length > 150)
                {
                    summary = summary.Substring(0, 150) + "...";
                }
            }

            ji.Summary = summary;
            ji.Body = Utilities.StripHTMLTag(body);
            ji.JournalTypeId = 5;
            ji.ObjectKey = string.Format("{0}:{1}", forumId.ToString(), topicId.ToString());
            if (JournalController.Instance.GetJournalItemByKey(portalId, ji.ObjectKey) != null)
            {
                JournalController.Instance.DeleteJournalItemByKey(portalId, ji.ObjectKey);
            }

            string roles = string.Empty;
            if (!string.IsNullOrEmpty(readRoles))
            {
                if (readRoles.Contains("|"))
                {
                    roles = readRoles.Substring(0, readRoles.IndexOf("|", StringComparison.Ordinal) - 1);
                }
            }

            foreach (string s in roles.Split(';'))
            {
                if ((s == DotNetNuke.Common.Globals.glbRoleAllUsers) | (s == DotNetNuke.Common.Globals.glbRoleUnauthUser))
                {
                    /* cjh - securityset was null and throwing an error, thus journal items weren't added */
                    if ((ji.SecuritySet != null) && !ji.SecuritySet.Contains("E,"))
                    {
                        ji.SecuritySet += "E,";
                    }
                }
                else
                {
                    ji.SecuritySet += "R" + s + ",";
                }
            }

            if (socialGroupId > 0)
            {
                ji.SocialGroupId = socialGroupId;
            }

            JournalController.Instance.SaveJournalItem(journalItem: ji, module: DotNetNuke.Entities.Modules.ModuleController.Instance.GetModule(moduleId, tabId, true));
        }

        [Obsolete("Deprecated in Community Forums 9.0.0. No interface with Active Social.")]
        public void AddReplyToJournal(int portalId, int moduleId, int forumId, int topicId, int replyId, int userId, string uRL, string subject, string summary, string body, int securityOption, string readRoles, int socialGroupId)
        {
            this.AddReplyToJournal(portalId: portalId, moduleId: moduleId, tabId: -1, forumId: forumId, topicId: topicId, replyId: replyId, userId: userId, uRL: uRL, subject: subject, summary: summary, body: body, readRoles: readRoles, socialGroupId: socialGroupId);
        }

        internal void AddReplyToJournal(int portalId, int moduleId, int tabId, int forumId, int topicId, int replyId, int userId, string uRL, string subject, string summary, string body, string readRoles, int socialGroupId)
        {
            // make sure that this is a User before trying to create a journal item, you can't post a JI without
            if (userId > 0)
            {
                var ji = new JournalItem
                {
                    PortalId = portalId,
                    ProfileId = userId,
                    UserId = userId,
                    Title = subject,
                    ItemData = new ItemData { Url = uRL },
                };
                if (string.IsNullOrEmpty(summary))
                {
                    summary = Utilities.StripHTMLTag(body);
                    if (summary.Length > 150)
                    {
                        summary = summary.Substring(0, 150) + "...";
                    }
                }

                ji.Summary = summary;
                ji.Body = Utilities.StripHTMLTag(body);
                ji.JournalTypeId = 6;
                ji.ObjectKey = string.Format("{0}:{1}:{2}", forumId.ToString(), topicId.ToString(), replyId.ToString());
                if (JournalController.Instance.GetJournalItemByKey(portalId, ji.ObjectKey) != null)
                {
                    JournalController.Instance.DeleteJournalItemByKey(portalId, ji.ObjectKey);
                }

                string roles = string.Empty;
                if (!string.IsNullOrEmpty(readRoles))
                {
                    if (readRoles.Contains("|"))
                    {
                        roles = readRoles.Substring(0, readRoles.IndexOf("|", StringComparison.Ordinal) - 1);
                    }
                }

                foreach (string s in roles.Split(';'))
                {
                    if ((s == DotNetNuke.Common.Globals.glbRoleAllUsers) | (s == DotNetNuke.Common.Globals.glbRoleUnauthUser))
                    {
                        /* cjh - securityset was null and throwing an error, thus journal items weren't added */
                        if ((ji.SecuritySet != null) && (!ji.SecuritySet.Contains("E,")))
                        {
                            ji.SecuritySet += "E,";
                        }
                    }
                    else
                    {
                        ji.SecuritySet += "R" + s + ",";
                    }
                }

                if (socialGroupId > 0)
                {
                    ji.SocialGroupId = socialGroupId;
                }

                JournalController.Instance.SaveJournalItem(journalItem: ji, module: DotNetNuke.Entities.Modules.ModuleController.Instance.GetModule(moduleId: moduleId, tabId: tabId, ignoreCache: false));
            }
        }
    }
}
