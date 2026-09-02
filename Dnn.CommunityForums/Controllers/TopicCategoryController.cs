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

    internal partial class TopicCategoryController : RepositoryServiceLocatorBase<DotNetNuke.Modules.ActiveForums.Entities.TopicCategoryInfo, ITopicCategoryController, TopicCategoryController>, ITopicCategoryController
    {
        protected override Func<ITopicCategoryController> GetFactory()
        {
            return () => new TopicCategoryController();
        }

        public void AddCategoryToTopic(int categoryId, int topicId)
        {
            this._repositoryControllerBase.Insert(new TopicCategoryInfo { CategoryId = categoryId, TopicId = topicId });
            CategoryController.Instance.RecountItems(categoryId);
        }

        public IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.TopicCategoryInfo> GetForTopic(int topicId)
        {
            return this._repositoryControllerBase.Find("WHERE TopicId = @0", topicId).ToList();
        }

        public IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.TopicCategoryInfo> GetForCategory(int categoryId)
        {
            return this._repositoryControllerBase.Find("WHERE CategoryId = @0", categoryId).ToList();
        }

        public void DeleteForCategory(int categoryId)
        {
            this._repositoryControllerBase.Delete("WHERE CategoryId = @0", categoryId);
        }

        public void DeleteForTopicCategory(int topicId, int categoryId)
        {
            this._repositoryControllerBase.Delete("WHERE CategoryId = @0 AND TopicId = @1", categoryId, topicId);
            CategoryController.Instance.RecountItems(categoryId);
        }

        public void DeleteForTopic(int topicId)
        {
            var categoriesToRecount = this.GetForTopic(topicId).Select(x => x.CategoryId).Distinct().ToList();
            this._repositoryControllerBase.Delete("WHERE TopicId = @0", topicId);
            categoriesToRecount.ForEach(categoryId =>
            {
                CategoryController.Instance.RecountItems(categoryId);
            });
        }
    }
}
