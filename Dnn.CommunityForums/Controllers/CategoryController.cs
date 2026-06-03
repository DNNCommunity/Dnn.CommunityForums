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
    using System.Linq;

    using DotNetNuke.Collections;
    using DotNetNuke.Modules.ActiveForums.Entities;

    internal partial class CategoryController : RepositoryServiceLocatorBase<DotNetNuke.Modules.ActiveForums.Entities.CategoryInfo, ICategoryController, CategoryController>, ICategoryController
    {
        private readonly TopicCategoryController topicCategoryController;

        protected override Func<ICategoryController> GetFactory()
        {
            return () => new CategoryController();
        }

        public CategoryController()
            : this(new TopicCategoryController())
        {
        }

        internal CategoryController(TopicCategoryController topicCategoryController)
        {
            this.topicCategoryController = topicCategoryController ?? throw new ArgumentNullException(nameof(topicCategoryController));
        }

        public DotNetNuke.Modules.ActiveForums.Entities.CategoryInfo GetForCategoryName(string categoryName)
        {
            return this._repositoryControllerBase.Find("WHERE UPPER(RTRIM(LTRIM(categoryName))) = UPPER(RTRIM(LTRIM(@0)))", categoryName).FirstOrDefault();
        }

        public void RecountItems(int categoryId)
        {
            var category = this._repositoryControllerBase.GetById(categoryId);
            if (category == null)
            {
                return;
            }

            category.Items = 0;
            var topicCategories = this.topicCategoryController.GetForCategory(categoryId);
            if (topicCategories != null)
            {
                category.Items = topicCategories.Count();
            }

            this._repositoryControllerBase.Update(category);
        }

        public new void Delete(string sqlCondition, params object[] args)
        {
            this._repositoryControllerBase.Find(sqlCondition, args).ForEach(c =>
            {
                this.topicCategoryController.DeleteForCategory(c.CategoryId);
                this.DeleteById(c.CategoryId);
            });
        }

        public new void DeleteById(int id)
        {
            this.topicCategoryController.DeleteForCategory(id);
            this.Delete(this._repositoryControllerBase.GetById(id));
        }

        public new void Delete(DotNetNuke.Modules.ActiveForums.Entities.CategoryInfo item)
        {
            if (item == null)
            {
                return;
            }

            this.topicCategoryController.DeleteForCategory(item.CategoryId);
            this._repositoryControllerBase.Delete(item);
        }

        public virtual DotNetNuke.Modules.ActiveForums.Entities.CategoryInfo GetByName(int moduleId, string categoryName)
        {
            string cachekey = string.Format(CacheKeys.CategoryByName, moduleId, categoryName);
            DotNetNuke.Modules.ActiveForums.Entities.CategoryInfo categoryInfo = DataCache.ContentCacheRetrieve(moduleId, cachekey) as DotNetNuke.Modules.ActiveForums.Entities.CategoryInfo;
            if (categoryInfo == null)
            {
                // this accommodates duplicates which may exist since currently no uniqueness applied in database
                categoryInfo = this._repositoryControllerBase.Find("WHERE ModuleId = @0 AND CategoryName = @1", moduleId, categoryName.Trim()).OrderBy(t => t.CategoryId).FirstOrDefault();
                DataCache.ContentCacheStore(moduleId, cachekey, categoryInfo);
            }

            return categoryInfo;
        }
    }
}
