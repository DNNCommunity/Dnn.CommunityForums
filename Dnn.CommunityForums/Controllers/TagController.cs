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

namespace DotNetNuke.Modules.ActiveForums.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Collections;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Modules.ActiveForums.Entities;

    internal class TagController : RepositoryServiceLocatorBase<DotNetNuke.Modules.ActiveForums.Entities.TagInfo, ITagController, TagController>, ITagController
    {
        private readonly ITopicTagController topicTagController;

        protected override Func<ITagController> GetFactory()
        {
            return () => new TagController();
        }

        public TagController()
            : this(DotNetNuke.Modules.ActiveForums.Controllers.TopicTagController.Instance)
        {
        }

        internal TagController(ITopicTagController topicTagController)
        {
            this.topicTagController = topicTagController ?? throw new ArgumentNullException(nameof(topicTagController));
        }

        public void RecountItems(int tagId)
        {
            var tag = this._repositoryControllerBase.GetById(tagId);
            if (tag == null)
            {
                return;
            }

            tag.Items = this.topicTagController.GetForTag(tagId).Count();
            this._repositoryControllerBase.Update(tag);
        }

        public new void Delete(TagInfo item)
        {
            if (item == null)
            {
                return;
            }

            this.topicTagController.DeleteForTag(item.TagId);
            this._repositoryControllerBase.Delete(item);
        }

        public new void Delete(string sqlCondition, params object[] args)
        {
            this._repositoryControllerBase.Find(sqlCondition, args).ForEach(t =>
            {
                this.topicTagController.DeleteForTag(t.TagId);
                this.DeleteById(t.TagId);
            });
        }

        public new void DeleteById(int tagId)
        {
            this.topicTagController.DeleteForTag(tagId);
            this._repositoryControllerBase.DeleteById(tagId);
        }

        internal static string GetBodyWithTagsProcessed(DotNetNuke.Modules.ActiveForums.Entities.IPostInfo post, DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forum, INavigationManager navigationManager)
        {
            string tagReplacement;
            if (!DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(forum.Security.TagRoleIds, post.Author.ForumUser.UserRoleIds))
            {
                // if the user does not have permission to add tags, this removes any tags from the post content but leaves tag text
                tagReplacement = "#${tag}";
            }
            else
            {
                /* transform new tags directly entered in the post content to be links to the search view for tags */
                /* e.g. #tag1 becomes <a href="https://localhost/forums/afv/search?aftg=tag1">#tag1</a> */
                tagReplacement = "<a href=\"" + navigationManager.NavigateURL(forum.GetTabId(), forum.PortalSettings, string.Empty, new[] { $"{ParamKeys.ViewType}={Views.Search}", $"{ParamKeys.Tags}=" + "${tag}" }) + "\" class=\"dcf-tag-link\">" + "#${tag}</a>";
            }

            var body = post.Content.Body;
            const string TagsPattern = @"(?is)(<code\b[^>]*>.*?</code>)|(?<=^|\s|<p>)#(?<tag>\w*[A-Za-z_]+\w*)";
            try
            {
                var tagRegex = RegexUtils.GetCachedRegex(TagsPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase, 2);
                body = tagRegex.Replace(body, match => match.Groups["tag"].Success ? match.Result(tagReplacement) : match.Value);
            }
            catch (RegexMatchTimeoutException ex)
            {
                Exceptions.LogException(ex);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                throw;
            }

            try
            {
                /* tags inserted via the editor plugin have # in the URL (aftg=#), which needs to be removed */
                /* e.g. <a href="https://localhost/forums/afv/search?aftg=#tag1">#tag1</a> becomes <a href="https://localhost/forums/afv/search?aftg=tag1">#tag1</a> */
                body = body.Replace($"/{ParamKeys.ViewType}/{Views.Search}?{ParamKeys.Tags}=#", $"/{ParamKeys.ViewType}/{Views.Search}?{ParamKeys.Tags}=");
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }

            return body;
        }

        public void UpdateTopicTags(IPostInfo post)
        {
            if (post == null || topicTagController == null)
            {
                return;
            }

            if (!PermissionController.HasRequiredPerm(post.Topic.Forum.Security.TagRoleIds, post.Author.ForumUser.UserRoleIds))
            {
                return;
            }

            try
            {
                var tags = ParseTagsFromBody(post.Content.Body);
                if (tags.Count <= 0)
                {
                    return;
                }

                var existingTags = this.topicTagController.GetForTopic(post.TopicId);
                tags.Distinct().ToList().Where(t => !string.IsNullOrEmpty(t)).ForEach(t =>
                {
                    var tag = this._repositoryControllerBase.Find("WHERE TagName = @0", t).FirstOrDefault();
                    if (tag == null)
                    {
                        tag = new TagInfo
                        {
                            TagName = t,
                            ModuleId = post.ModuleId,
                            PortalId = post.PortalId,
                        };

                        this._repositoryControllerBase.Insert(tag);
                    }

                    if (!existingTags.Any(et => et.TagId == tag.TagId))
                    {
                        this.topicTagController.AddTagToTopic(tag.TagId, post.TopicId);
                    }
                });
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }
        }

        internal static List<string> ParseTagsFromBody(string body)
        {
            const string tagsPattern = @"href="".*?/afv/search\?aftg=(?<tag>.+?)""";
            var tags = new List<string>();
            var matches = RegexUtils.GetCachedRegex(tagsPattern, RegexOptions.Compiled & RegexOptions.IgnoreCase & RegexOptions.IgnorePatternWhitespace, 5).Matches(body);
            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    if (!string.IsNullOrEmpty(match.Groups["tag"]?.Value))
                    {
                        tags.Add(match.Groups["tag"].Value);
                    }
                }
            }

            return tags;
        }

        public virtual TagInfo GetByName(int moduleId, string tagName)
        {
            var cachekey = string.Format(CacheKeys.TagByName, moduleId, tagName);
            var tagInfo = DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheRetrieve(moduleId, cachekey) as TagInfo;
            if (tagInfo == null)
            {
                tagInfo = this._repositoryControllerBase.Find("WHERE ModuleId = @0 AND TagName = @1", moduleId, tagName.Trim()).OrderBy(t => t.TagId).FirstOrDefault();
                DotNetNuke.Modules.ActiveForums.DataCache.ContentCacheStore(moduleId, cachekey, tagInfo);
            }

            return tagInfo;
        }
    }
}
