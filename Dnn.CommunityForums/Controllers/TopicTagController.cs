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

    using DotNetNuke.Collections;
    using DotNetNuke.Modules.ActiveForums.Entities;

    internal partial class TopicTagController : RepositoryServiceLocatorBase<DotNetNuke.Modules.ActiveForums.Entities.TopicTagInfo, ITopicTagController, TopicTagController>, ITopicTagController
    {
        protected override Func<ITopicTagController> GetFactory()
        {
            return () => new TopicTagController();
        }

        public void AddTagToTopic(int tagId, int topicId)
        {
            this._repositoryControllerBase.Insert(new TopicTagInfo { TagId = tagId, TopicId = topicId });
            TagController.Instance.RecountItems(tagId);
        }

        public IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.TopicTagInfo> GetForTopic(int topicId)
        {
            return this._repositoryControllerBase.Find("WHERE TopicId = @0", topicId);
        }

        public IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.TopicTagInfo> GetForTag(int tagId)
        {
            return this._repositoryControllerBase.Find("WHERE TagId = @0", tagId);
        }

        public void DeleteForTag(int tagId)
        {
            this._repositoryControllerBase.Delete("WHERE TagId = @0", tagId);
        }

        public void DeleteForTopicTag(int topicId, int tagId)
        {
            this._repositoryControllerBase.Delete("WHERE TagId = @0 AND TopicId = @1", tagId, topicId);
            TagController.Instance.RecountItems(tagId);
        }

        public void DeleteForTopic(int topicId)
        {
            var tagsToRecount = this.GetForTopic(topicId).Select(x => x.TagId).Distinct();
            this._repositoryControllerBase.Delete("WHERE TopicId = @0", topicId);
            tagsToRecount.ForEach(tagId =>
            {
                TagController.Instance.RecountItems(tagId);
            });
        }
    }
}
