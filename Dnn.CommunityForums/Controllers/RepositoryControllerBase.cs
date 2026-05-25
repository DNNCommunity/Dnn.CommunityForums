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
    using DotNetNuke.Data;

    internal class RepositoryControllerBase<T> where T : class
    {
        private IRepository<T> _repo;

        private IRepository<T> Repo
        {
            get
            {
                if (this._repo == null)
                {
                    try
                    {
                        var ctx = DataContext.Instance();
                        this._repo = ctx?.GetRepository<T>();
                    }
                    catch (Exception)
                    {
                        // DataContext may not be available in test environments.
                    }
                }

                return this._repo;
            }
        }

        internal virtual string cacheKeyTemplate => string.Empty;

        internal RepositoryControllerBase()
        {
        }

        internal virtual IEnumerable<T> Get()
        {
            return this.Repo.Get();
        }

        internal virtual IEnumerable<T> Get<TScopeType>(TScopeType scopeValue)
        {
            return this.Repo.Get(scopeValue);
        }

        internal virtual T GetById<TProperty>(TProperty id)
        {
            return this.Repo.GetById(id);
        }

        internal virtual T GetById<TProperty, TScopeType>(TProperty id, TScopeType scopeValue)
        {
            return this.Repo.GetById(id, scopeValue);
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
            return string.IsNullOrEmpty(sqlCondition) ? this.Get() : this.Repo.Find(sqlCondition, args);
        }

        internal IPagedList<T> Find(int pageIndex, int pageSize, string sqlCondition, params object[] args)
        {
            return this.Repo.Find(pageIndex, pageSize, sqlCondition, args);
        }

        internal void Update(T item)
        {
            var property = typeof(T).GetProperty("DateUpdated");
            if (property != null && property.CanWrite && property.PropertyType == typeof(System.DateTime))
            {
                property.SetValue(item, System.DateTime.UtcNow, null);
            }
            this.Repo.Update(item);
        }

        internal void Insert(T item)
        {
            this.Repo.Insert(item);
        }

        internal void Delete(string sqlCondition, params object[] args)
        {
            this.Repo.Delete(sqlCondition, args);
        }

        internal void DeleteById<TProperty>(TProperty id)
        {
            this.Repo.Delete(this.Repo.GetById(id));
        }

        internal void Delete(T item)
        {
            this.Repo.Delete(item);
        }

        internal void DeleteByModuleId(int moduleId)
        {
            this.Repo.Delete("WHERE (ModuleId = @0)", moduleId);
        }

        internal int Count(string sqlCondition, params object[] args)
        {
            return string.IsNullOrEmpty(sqlCondition) ? this.Repo.Get().Count() : this.Repo.Find(sqlCondition, args).Count();
        }

        internal IPagedList<T> GetPage(int pageIndex, int pageSize)
        {
            return this.Repo.GetPage(pageIndex, pageSize);
        }

        internal IPagedList<T> GetPage<TScopeType>(TScopeType scopeValue, int pageIndex, int pageSize)
        {
            return this.Repo.GetPage(scopeValue, pageIndex, pageSize);
        }

        internal static T LoadFromSettingsCache(int moduleId, string cachekey)
        {
            return (T)DotNetNuke.Modules.ActiveForums.DataCache.SettingsCacheRetrieve(moduleId, cachekey);
        }

        internal static void UpdateSettingsCache(int moduleId, string cachekey, T entity)
        {
            DotNetNuke.Modules.ActiveForums.DataCache.SettingsCacheStore(moduleId, cachekey, entity);
        }

        internal static void ClearSettingsCache(int moduleId)
        {
            DotNetNuke.Modules.ActiveForums.DataCache.ClearSettingsCache(moduleId);
        }

        internal string GetCacheKey<TProperty>(int moduleId, TProperty id)
        {
            return string.Format(this.cacheKeyTemplate, moduleId, id);
        }
    }
}
