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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using DotNetNuke.Common.Utilities;

namespace DotNetNuke.Modules.ActiveForums.Controllers
{
    using System.Linq;

    using DotNetNuke.Collections;

    internal partial class TagController : RepositoryControllerBase<DotNetNuke.Modules.ActiveForums.Entities.TagInfo>
    {
        internal void RecountItems(int tagId)
        {
            var tag = new DotNetNuke.Modules.ActiveForums.Controllers.TagController().GetById(tagId);
            tag.Items = new DotNetNuke.Modules.ActiveForums.Controllers.TopicTagController().GetForTag(tagId).Count();
            new DotNetNuke.Modules.ActiveForums.Controllers.TagController().Update(tag);
        }

        internal void Delete(string sqlCondition, params object[] args)
        {
            this.Find(sqlCondition, args).ForEach(item =>
            {
                this.Delete(item);
            });
        }

        public void DeleteById(int tagId)
        {
            /* delete all topic tags for this tag */
            new DotNetNuke.Modules.ActiveForums.Controllers.TopicTagController().Delete("WHERE TagId = @0", tagId);
            /* now delete the tag itself */
            base.DeleteById(tagId);
        }

        internal static void ProcessTags(DotNetNuke.Modules.ActiveForums.Entities.IPostInfo post)
        {
            /* transform new tags directly entered into links to the search view for tags */
            /* e.g. #tag1 #tag2 becomes <a href="https://localhost/forums/afv/search?aftg=tag1">#tag1</a> */
            const string newTagsPattern = @"(?<=\s|^)#(?<tag>\w*[A-Za-z_]+\w*)";
            var tagtag = "<a href=\"" + Utilities.NavigateURL(post.Forum.GetTabId(), string.Empty, new[] { $"{ParamKeys.ViewType}={Views.Search}", $"{ParamKeys.Tags}=" + "${tag}" }) + "\" class=\"dcf-tag-link\">" + "#${tag}</a>";
            try
            {
                post.Content.Body = RegexUtils.GetCachedRegex(newTagsPattern, RegexOptions.Compiled & RegexOptions.IgnoreCase, 2).Replace(post.Content.Body, tagtag);
                new DotNetNuke.Modules.ActiveForums.Controllers.ContentController().Update(post.Content);
                post.Content.UpdateCache();
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
            ///* tags inserted via the editor plugin have # in the URL (aftg=#), which needs to be removed */
            ///* e.g. <a href="https://localhost/forums/afv/search?aftg=#tag1">#tag1</a> becomes <a href="https://localhost/forums/afv/search?aftg=tag1">#tag1</a> */
            post.Content.Body = post.Content.Body.Replace("/afv/search?aftg=#", "/afv/search?aftg=");
            new DotNetNuke.Modules.ActiveForums.Controllers.ContentController().Update(post.Content);
            post.Content.UpdateCache();
            //const string insertedTagsPattern = @"/afv/search\?aftg=#";
            //try
            //{
            //    post.Content.Body = RegexUtils.GetCachedRegex(insertedTagsPattern, RegexOptions.Compiled & RegexOptions.IgnoreCase, 2).Replace(post.Content.Body, @"/afv/search\?aftg=");
            //    new DotNetNuke.Modules.ActiveForums.Controllers.ContentController().Update(post.Content);
            //    post.Content.UpdateCache();
            //}
            //catch (RegexMatchTimeoutException ex)
            //{
            //    Exceptions.LogException(ex);
            //}
            //catch (Exception ex)
            //{
            //    Exceptions.LogException(ex);
            //    throw;
            //}
            /* now operate on all tags, both existing and new */
            const string existingTagsPattern = @"href="".*?/afv/search\?aftg=(?<tag>.+?)""";
            try
            {
                var tags = new List<string>();
                var matches = RegexUtils.GetCachedRegex(existingTagsPattern, RegexOptions.Compiled & RegexOptions.IgnoreCase & RegexOptions.IgnorePatternWhitespace, 5).Matches(post.Content.Body);
                if (matches.Count > 0)
                {
                    foreach (Match match in matches)
                    {
                        if (!string.IsNullOrEmpty(match.Groups["tag"]?.Value))
                        {
                            tags.Add(match.Groups["tag"].Value);
                        }
                    }
                    ;

                    tags.Distinct().ToList().ForEach(t =>
                    {
                        var tag = new DotNetNuke.Modules.ActiveForums.Controllers.TagController().Find("WHERE TagName = @0", t).FirstOrDefault();
                        if (tag == null)
                        {
                            tag = new DotNetNuke.Modules.ActiveForums.Entities.TagInfo()
                            {
                                TagName = t,
                                ModuleId = post.ModuleId,
                                PortalId = post.PortalId,
                            };
                            new DotNetNuke.Modules.ActiveForums.Controllers.TagController().Insert(tag);
                        }
                        ;

        internal void Delete(DotNetNuke.Modules.ActiveForums.Entities.TagInfo item)
        {
            new DotNetNuke.Modules.ActiveForums.Controllers.TopicTagController().DeleteForTag(item.TagId);
            base.Delete(item);
        } 
    }
}
