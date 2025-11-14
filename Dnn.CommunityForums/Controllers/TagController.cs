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

        internal static void CleanUpTags(DotNetNuke.Modules.ActiveForums.Entities.IPostInfo post, DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forum)
        {
            const string tagsPattern = @"(?<=^|\s|<p>)#(?<tag>\w*[A-Za-z_]+\w*)";
            if (!DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(forum.Security.TagRoleIds, post.Author.ForumUser.UserRoleIds))
            {
                // if the user does not have permission to add tags, remove any tags from the post content but leave tag text
                post.Content.Body = RegexUtils.GetCachedRegex(tagsPattern, RegexOptions.Compiled & RegexOptions.IgnoreCase, 2).Replace(post.Content.Body, "${tag}");
            }
            else
            {
                /* transform new tags directly entered in the post content to be links to the search view for tags */
                /* e.g. #tag1 becomes <a href="https://localhost/forums/afv/search?aftg=tag1">#tag1</a> */
                var tagtag = "<a href=\"" + Utilities.NavigateURL(forum.GetTabId(), string.Empty, new[] { $"{ParamKeys.ViewType}={Views.Search}", $"{ParamKeys.Tags}=" + "${tag}" }) + "\" class=\"dcf-tag-link\">" + "#${tag}</a>";
                try
                {
                    post.Content.Body = RegexUtils.GetCachedRegex(tagsPattern, RegexOptions.Compiled & RegexOptions.IgnoreCase, 2).Replace(post.Content.Body, tagtag);
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

        public void DeleteById(int tagId)
        {
            /* delete all topic tags for this tag */
            new DotNetNuke.Modules.ActiveForums.Controllers.TopicTagController().Delete("WHERE TagId = @0", tagId);
            /* now delete the tag itself */
            base.DeleteById(tagId);
        }

        internal void Delete(DotNetNuke.Modules.ActiveForums.Entities.TagInfo item)
        {
            new DotNetNuke.Modules.ActiveForums.Controllers.TopicTagController().DeleteForTag(item.TagId);
            base.Delete(item);
        } 
    }
}
