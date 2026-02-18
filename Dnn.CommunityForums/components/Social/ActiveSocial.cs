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

namespace DotNetNuke.Modules.ActiveForums
{
    using System;
    using System.Linq;
    using System.Reflection;

    using DotNetNuke.Modules.ActiveForums.Data;
    using DotNetNuke.Modules.ActiveForums.Entities;
    using DotNetNuke.Modules.ActiveForums.ViewModels;
    using DotNetNuke.Services.Journal;
    using DotNetNuke.Services.Sitemap;

    internal static class Social
    {
        internal static void AddPostToJournal(DotNetNuke.Modules.ActiveForums.Entities.IPostInfo post)
        {
            try
            {
                if (post.Author?.AuthorId <= 0)
                {
                    return;
                }

                var sUrl = post.GetLink();
                var summary = post.Content.Summary;
                if (string.IsNullOrEmpty(summary))
                {
                    summary = Utilities.StripQuoteTag(post.Content.Body);
                    summary = Utilities.StripHTMLTag(summary);
                    if (summary.Length > 150)
                    {
                        summary = summary.Substring(0, 150) + "...";
                    }
                }

                DeleteJournalItemForPost(post);
                var ji = new JournalItem
                {
                    PortalId = post.PortalId,
                    ProfileId = post.Author.AuthorId,
                    UserId = post.Author.AuthorId,
                    Title = post.Content.Subject,
                    Summary = summary,
                    Body = Utilities.StripHTMLTag(Utilities.StripQuoteTag(post.Content.Summary)),
                    ItemData = new ItemData { Url = sUrl },
                    JournalTypeId = post.IsTopic ? 5 : 6,
                    ObjectKey = post.IsTopic ? $"{post.ForumId}:{post.TopicId}" : $"{post.ForumId}:{post.TopicId}:{post.ReplyId}",
                    SecuritySet = string.Empty,
                };

                if (post.Forum.Security.ReadRoleIds.Contains(int.Parse(DotNetNuke.Common.Globals.glbRoleAllUsers)) ||
                    post.Forum.Security.ReadRoleIds.Contains(int.Parse(DotNetNuke.Common.Globals.glbRoleUnauthUser)))
                {
                    ji.SecuritySet += "E,";
                }

                post.Forum.Security.ReadRoleIds.Where(r => r != int.Parse(DotNetNuke.Common.Globals.glbRoleAllUsers) && r != int.Parse(DotNetNuke.Common.Globals.glbRoleUnauthUser)).ToList().ForEach(r => ji.SecuritySet += "R" + r + ",");

                if (post.Forum.SocialGroupId > 0)
                {
                    ji.SocialGroupId = post.Forum.SocialGroupId;
                }

                JournalController.Instance.SaveJournalItem(journalItem: ji, module: DotNetNuke.Entities.Modules.ModuleController.Instance.GetModule(post.PortalId, post.Forum.GetTabId(), true));
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        internal static void UpdateJournalItemForPost(DotNetNuke.Modules.ActiveForums.Entities.IPostInfo post)
        {
            try
            {
                var sUrl = post.GetLink();
                var objectKey = post.IsTopic ? $"{post.ForumId}:{post.TopicId}" : $"{post.ForumId}:{post.TopicId}:{post.ReplyId}";
                var ji = JournalController.Instance.GetJournalItemByKey(post.PortalId, objectKey);
                if (ji != null)
                {
                    var summary = post.Content.Summary;
                    if (string.IsNullOrEmpty(summary))
                    {
                        summary = Utilities.StripQuoteTag(post.Content.Body);
                        summary = Utilities.StripHTMLTag(summary);
                        if (summary.Length > 150)
                        {
                            summary = summary.Substring(0, 150) + "...";
                        }
                    }

                    ji.Title = post.Content.Subject;
                    ji.Summary = summary;
                    ji.ItemData = new ItemData { Url = sUrl };
                    ji.Body = Utilities.StripHTMLTag(Utilities.StripQuoteTag(post.Content.Summary));
                    ji.DateUpdated = DateTime.UtcNow;
                    JournalController.Instance.UpdateJournalItem(journalItem: ji, module: DotNetNuke.Entities.Modules.ModuleController.Instance.GetModule(post.ModuleId, post.Forum.GetTabId(), true));
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        internal static void DeleteJournalItemForPost(DotNetNuke.Modules.ActiveForums.Entities.IPostInfo post)
        {
            try
            {
                var objectKey = post.IsTopic ? $"{post.ForumId}:{post.TopicId}" : $"{post.ForumId}:{post.TopicId}:{post.ReplyId}";
                var ji = JournalController.Instance.GetJournalItemByKey(post.PortalId, objectKey);
                if (ji != null)
                {
                    JournalController.Instance.DeleteJournalItemByKey(post.PortalId, objectKey);
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }
    }
}
