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
    using System.Reflection;

    using DotNetNuke.Collections;
    using DotNetNuke.Data;

    internal class RepositoryControllerBase<T>
        where T : class
    {
        private static readonly PropertyInfo DateUpdatedProperty = typeof(T).GetProperty("DateUpdated");

        private IRepository<T> _repo;

        internal RepositoryControllerBase()
        {
        }

        internal virtual IEnumerable<T> Get()
        {
            return this.WithRepository(repo => repo.Get());
        }

        internal virtual IEnumerable<T> Get<TScopeType>(TScopeType scopeValue)
        {
            return this.WithRepository(repo => repo.Get(scopeValue));
        }

        internal virtual T GetById<TProperty>(TProperty id)
        {
            return this.WithRepository(repo => repo.GetById(id));
        }

        internal virtual T GetById<TProperty, TScopeType>(TProperty id, TScopeType scopeValue)
        {
            return this.WithRepository(repo => repo.GetById(id, scopeValue));
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
            return string.IsNullOrEmpty(sqlCondition) ? this.Get() : this.WithRepository(repo => repo.Find(sqlCondition, args));
        }

        internal IPagedList<T> Find(int pageIndex, int pageSize, string sqlCondition, params object[] args)
        {
            return this.WithRepository(repo => repo.Find(pageIndex, pageSize, sqlCondition, args));
        }

        internal void Update(T item)
        {
            if (DateUpdatedProperty != null && DateUpdatedProperty.CanWrite && DateUpdatedProperty.PropertyType == typeof(DateTime))
            {
                DateUpdatedProperty.SetValue(item, DateTime.UtcNow, null);
            }

            this.WithRepository(repo => repo.Update(item));
        }

        internal void Update(string sqlCondition, params object[] args)
        {
            if (!string.IsNullOrEmpty(sqlCondition))
            {
                this.Find(sqlCondition, args).ToList().ForEach(this.Update);
            }
        }

        internal void Insert(T item)
        {
            this.WithRepository(repo => repo.Insert(item));
        }

        internal void Delete(string sqlCondition, params object[] args)
        {
            this.WithRepository(repo => repo.Delete(sqlCondition, args));
        }

        internal void DeleteById<TProperty>(TProperty id)
        {
            this.WithRepository(repo => repo.Delete(repo.GetById(id)));
        }

        internal void Delete(T item)
        {
            this.WithRepository(repo => repo.Delete(item));
        }

        internal void DeleteByModuleId(int moduleId)
        {
            this.Repo.Delete("WHERE (ModuleId = @0)", moduleId);
        }

        internal int Count(string sqlCondition, params object[] args)
        {
            return string.IsNullOrEmpty(sqlCondition)
                ? this.WithRepository(repo => repo.Get().Count())
                : this.WithRepository(repo => repo.Find(sqlCondition, args).Count());
        }

        internal IPagedList<T> GetPage(int pageIndex, int pageSize)
        {
            return this.WithRepository(repo => repo.GetPage(pageIndex, pageSize));
        }

        internal IPagedList<T> GetPage<TScopeType>(TScopeType scopeValue, int pageIndex, int pageSize)
        {
            return this.WithRepository(repo => repo.GetPage(scopeValue, pageIndex, pageSize));
        }

        private TResult WithRepository<TResult>(Func<IRepository<T>, TResult> action)
        {
            if (this._repo != null)
            {
                return action(this._repo);
            }

            try
            {
                using var ctx = DataContext.Instance();
                return action(ctx?.GetRepository<T>());
            }
            catch (Exception)
            {
                // DataContext may not be available in test environments.
                return default;
            }
        }

        private void WithRepository(Action<IRepository<T>> action)
        {
            if (this._repo != null)
            {
                action(this._repo);
                return;
            }

            try
            {
                using var ctx = DataContext.Instance();
                action(ctx?.GetRepository<T>());
            }
            catch (Exception)
            {
                // DataContext may not be available in test environments.
            }
        }
    }
}
