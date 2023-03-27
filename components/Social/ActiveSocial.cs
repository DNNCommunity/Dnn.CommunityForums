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
using System.Data;

using System.Web;
using DotNetNuke.Modules.ActiveForums.Data;
using Microsoft.ApplicationBlocks.Data;
using DotNetNuke.Services.Journal;
namespace DotNetNuke.Modules.ActiveForums
{
    public class Social : DataConfig
    {
        public void AddTopicToJournal(int PortalId, int ModuleId, int ForumId, int TopicId, int UserId, string URL, string Subject, string Summary, string Body, int SecurityOption, string ReadRoles, int SocialGroupId)
        {
            AddTopicToJournal(PortalId, ModuleId, ForumId, TopicId, UserId, URL, Subject, Summary, Body, ReadRoles, SocialGroupId);
        }
        internal void AddTopicToJournal(int PortalId, int ModuleId, int ForumId, int TopicId, int UserId, string URL, string Subject, string Summary, string Body, string ReadRoles, int SocialGroupId)
            {
                var ji = new JournalItem
                         {
                             PortalId = PortalId,
                             ProfileId = UserId,
                             UserId = UserId,
                             Title = Subject,
                             ItemData = new ItemData { Url = URL }
                         };
            if (string.IsNullOrEmpty(Summary))
            {
                Summary = Utilities.StripHTMLTag(Body);
                if (Summary.Length > 150)
                {
                    Summary = Summary.Substring(0, 150) + "...";
                }
            }
            ji.Summary = Summary;
            ji.Body = Utilities.StripHTMLTag(Body);
            ji.JournalTypeId = 5;
            ji.ObjectKey = string.Format("{0}:{1}", ForumId.ToString(), TopicId.ToString());
            if (JournalController.Instance.GetJournalItemByKey(PortalId, ji.ObjectKey) != null)
            {
                JournalController.Instance.DeleteJournalItemByKey(PortalId, ji.ObjectKey);
            }
            string roles = string.Empty;
            if (!(string.IsNullOrEmpty(ReadRoles)))
            {
                if (ReadRoles.Contains("|"))
                {
                    roles = ReadRoles.Substring(0, ReadRoles.IndexOf("|", StringComparison.Ordinal) - 1);
                }
            }

            foreach (string s in roles.Split(';'))
            {
                if ((s == "-1") | (s == "-3"))
                {
                    /* cjh - securityset was null and throwing an error, thus journal items weren't added */
                    if ((ji.SecuritySet != null) && !(ji.SecuritySet.Contains("E,")))
                    {
                        ji.SecuritySet += "E,";
                    }
                }
                else
                {
                    ji.SecuritySet += "R" + s + ",";
                }

            }
            if (SocialGroupId > 0)
            {

                ji.SocialGroupId = SocialGroupId;

            }
            JournalController.Instance.SaveJournalItem(ji, -1);
        }
        [Obsolete("Deprecated in Community Forums 9.0.0. No interface with Active Social.")]
        public void AddReplyToJournal(int PortalId, int ModuleId, int ForumId, int TopicId, int ReplyId, int UserId, string URL, string Subject, string Summary, string Body, int SecurityOption, string ReadRoles, int SocialGroupId)
        {
            AddReplyToJournal(PortalId, ModuleId, ForumId, TopicId, ReplyId, UserId, URL, Subject, Summary, Body, ReadRoles, SocialGroupId);
        }
        internal void AddReplyToJournal(int PortalId, int ModuleId, int ForumId, int TopicId, int ReplyId, int UserId, string URL, string Subject, string Summary, string Body, string ReadRoles, int SocialGroupId)
        {
            //make sure that this is a User before trying to create a journal item, you can't post a JI without
            if (UserId > 0)
            {
                var ji = new JournalItem
                             {
                                 PortalId = PortalId,
                                 ProfileId = UserId,
                                 UserId = UserId,
                                 Title = Subject,
                                 ItemData = new ItemData { Url = URL }
                             };
                if (string.IsNullOrEmpty(Summary))
                {
                    Summary = Utilities.StripHTMLTag(Body);
                    if (Summary.Length > 150)
                    {
                        Summary = Summary.Substring(0, 150) + "...";
                    }
                }
                ji.Summary = Summary;
                ji.Body = Utilities.StripHTMLTag(Body);
                ji.JournalTypeId = 6;
                ji.ObjectKey = string.Format("{0}:{1}:{2}", ForumId.ToString(), TopicId.ToString(), ReplyId.ToString());
                if (JournalController.Instance.GetJournalItemByKey(PortalId, ji.ObjectKey) != null)
                {
                    JournalController.Instance.DeleteJournalItemByKey(PortalId, ji.ObjectKey);
                }
                string roles = string.Empty;
                if (!(string.IsNullOrEmpty(ReadRoles)))
                {
                    if (ReadRoles.Contains("|"))
                    {
                        roles = ReadRoles.Substring(0, ReadRoles.IndexOf("|", StringComparison.Ordinal) - 1);
                    }
                }

                foreach (string s in roles.Split(';'))
                {
                    if ((s == "-1") | (s == "-3"))
                    {
                        /* cjh - securityset was null and throwing an error, thus journal items weren't added */
                        if ((ji.SecuritySet != null) && (!(ji.SecuritySet.Contains("E,"))))
                        {
                            ji.SecuritySet += "E,";
                        }
                    }
                    else
                    {
                        ji.SecuritySet += "R" + s + ",";
                    }
                }
                if (SocialGroupId > 0)
                {
                    ji.SocialGroupId = SocialGroupId;
                }
                JournalController.Instance.SaveJournalItem(ji, -1);
            }
        }
    }
}
