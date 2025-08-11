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
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Collections;
    using DotNetNuke.Modules.ActiveForums.Entities;


    internal partial class TopicTagController : RepositoryControllerBase<DotNetNuke.Modules.ActiveForums.Entities.TopicTagInfo>
    {
        internal void AddTagToTopic(int tagId, int topicId)
        {
            this.Insert(new TopicTagInfo() { TagId = tagId, TopicId = topicId });
            new DotNetNuke.Modules.ActiveForums.Controllers.TagController().RecountItems(tagId);
        }

        internal IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.TopicTagInfo> GetForTopic(int topicId)
        {
            return this.Find("WHERE TopicId = @0", topicId);
        }

        internal IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.TopicTagInfo> GetForTag(int tagId)
        {
            return this.Find("WHERE TagId = @0", tagId);
        }

        internal void DeleteForTag(int tagId)
        {
            this.Delete("WHERE TagId = @0", tagId);
        }

        internal void DeleteForTopicTag(int topicId, int tagId)
        {
            this.Delete("WHERE TagId = @0 AND TopicId = @1", tagId, topicId);
            new DotNetNuke.Modules.ActiveForums.Controllers.TagController().RecountItems(tagId);
        }

        internal void DeleteForTopic(int topicId)
        {
            var tagsToRecount = this.GetForTopic(topicId).Select(x => x.TagId).Distinct();
            this.Delete("WHERE TopicId = @0", topicId);
            tagsToRecount.ForEach(tagId =>
            {
                new DotNetNuke.Modules.ActiveForums.Controllers.TagController().RecountItems(tagId);
            });
        }
    }
}
