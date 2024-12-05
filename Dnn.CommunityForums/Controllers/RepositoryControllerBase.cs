// Copyright (c) 2013-2024 by DNN Community
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
    using System.Reflection;
    using System.Web.UI;

    using DotNetNuke.Collections;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel.DataAnnotations;
    using DotNetNuke.Data;
    using DotNetNuke.Modules.ActiveForums.Entities;

    internal class RepositoryControllerBase<T> where T : class
    {
        private readonly IRepository<T> repo;

        internal virtual string cacheKeyTemplate => string.Empty;

        internal RepositoryControllerBase()
        {
            var ctx = DataContext.Instance();
            this.repo = ctx.GetRepository<T>();
        }

        internal IEnumerable<T> Get()
        {
            return this.repo.Get();
        }

        internal IEnumerable<T> Get<TScopeType>(TScopeType scopeValue)
        {
            return this.repo.Get(scopeValue);
        }

        internal T GetById<TProperty>(TProperty id)
        {
            return this.repo.GetById(id);
        }

        internal T GetById<TProperty, TScopeType>(TProperty id, TScopeType scopeValue)
        {
            return this.repo.GetById(id, scopeValue);
        }

        internal T Save<TProperty>(T item, TProperty id)
        {
            if (id == null || id.Equals(0) || id.Equals(-1) || this.GetById(id) == null)
            {
                this.Insert(item);
            }
            else
            {
                this.Update(item);
            }

            return item;
        }

        internal IEnumerable<T> Find(string sqlCondition, params object[] args)
        {
            return string.IsNullOrEmpty(sqlCondition) ? this.Get() : this.repo.Find(sqlCondition, args);
        }

        internal IPagedList<T> Find(int pageIndex, int pageSize, string sqlCondition, params object[] args)
        {
            return this.repo.Find(pageIndex, pageSize, sqlCondition, args);
        }

        internal void Update(T item)
        {
            this.repo.Update(item);
        }

        public void Update(string sqlCondition, params object[] args)
        {
            this.repo.Update(sqlCondition, args);
        }

        internal void Insert(T item)
        {
            this.repo.Insert(item);
        }

        internal void Delete(string sqlCondition, params object[] args)
        {
            this.repo.Delete(sqlCondition, args);
        }

        internal void DeleteById<TProperty>(TProperty id)
        {
            this.repo.Delete(this.repo.GetById(id));
        }

        internal void Delete(T item)
        {
            this.repo.Delete(item);
        }

        internal void DeleteByModuleId(int moduleId)
        {
            this.repo.Delete("WHERE (ModuleId = @0)", moduleId);
        }

        internal int Count(string sqlCondition, params object[] args)
        {
            return string.IsNullOrEmpty(sqlCondition) ? this.repo.Get().Count() : this.repo.Find(sqlCondition, args).Count();
        }

        internal IPagedList<T> GetPage(int pageIndex, int pageSize)
        {
            return this.repo.GetPage(pageIndex, pageSize);
        }

        internal IPagedList<T> GetPage<TScopeType>(TScopeType scopeValue, int pageIndex, int pageSize)
        {
            return this.repo.GetPage(scopeValue, pageIndex, pageSize);
        }

        internal static T LoadFromCache(int moduleId, string cachekey)
        {
            return (T)DotNetNuke.Modules.ActiveForums.DataCache.SettingsCacheRetrieve(moduleId, cachekey);
        }

        internal static void UpdateCache(int moduleId, string cachekey, T entity)
        {
            DotNetNuke.Modules.ActiveForums.DataCache.SettingsCacheStore(moduleId, cachekey, entity);
        }

        internal static void ClearSettingsCache(int moduleId)
        {
            DotNetNuke.Modules.ActiveForums.DataCache.ClearSettingsCache(moduleId);
        }

        internal string GetCacheKey<TProperty>(TProperty id, int moduleId)
        {
            return string.Format(this.cacheKeyTemplate, moduleId, id);
        }
    }
}
