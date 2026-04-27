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
    using System.Linq;

    using DotNetNuke.Collections;

    internal partial class CategoryController : RepositoryControllerBase<DotNetNuke.Modules.ActiveForums.Entities.CategoryInfo>
    {
        public DotNetNuke.Modules.ActiveForums.Entities.CategoryInfo GetForCategoryName(string categoryName)
        {
            return this.Find("WHERE UPPER(RTRIM(LTRIM(categoryName))) = UPPER(RTRIM(LTRIM(@0)))", categoryName).FirstOrDefault();
        }

        internal void RecountItems(int categoryId)
        {
            var categoryController = new DotNetNuke.Modules.ActiveForums.Controllers.CategoryController();
            var category = new DotNetNuke.Modules.ActiveForums.Controllers.CategoryController().GetById(categoryId);
            category.Items = 0;
            var topicCategories = new DotNetNuke.Modules.ActiveForums.Controllers.TopicCategoryController().GetForCategory(categoryId);
            if (topicCategories != null)
            {
                category.Items = topicCategories.Count();
            }

            categoryController.Update(category);
        }

        internal void Delete(string sqlCondition, params object[] args)
        {
            this.Find(sqlCondition, args).ForEach(item =>
            {
                this.Delete(item);
            });
        }

        internal void DeleteById(int id)
        {
            this.Delete(this.GetById(id));
        }

        internal void Delete(DotNetNuke.Modules.ActiveForums.Entities.CategoryInfo item)
        {
            new DotNetNuke.Modules.ActiveForums.Controllers.TopicCategoryController().DeleteForCategory(item.CategoryId);
            base.Delete(item);
        }

        internal DotNetNuke.Modules.ActiveForums.Entities.CategoryInfo GetByName(int moduleId, string categoryName)
        {
            string cachekey = string.Format(CacheKeys.CategoryByName, moduleId, categoryName);
            DotNetNuke.Modules.ActiveForums.Entities.CategoryInfo categoryInfo = DataCache.ContentCacheRetrieve(moduleId, cachekey) as DotNetNuke.Modules.ActiveForums.Entities.CategoryInfo;
            if (categoryInfo == null)
            {
                // this accommodates duplicates which may exist since currently no uniqueness applied in database
                categoryInfo = this.Find("WHERE ModuleId = @0 AND CategoryName = @1", moduleId, categoryName.Trim()).OrderBy(t => t.CategoryId).FirstOrDefault();
                DataCache.ContentCacheStore(moduleId, cachekey, categoryInfo);
            }

            return categoryInfo;
        }
    }
}
