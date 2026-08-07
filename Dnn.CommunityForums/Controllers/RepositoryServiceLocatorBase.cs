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

    using DotNetNuke.Collections;
    using DotNetNuke.Data;
    using DotNetNuke.Framework;

    /// <summary>
    /// Base class that combines ServiceLocator and IRepository implementations to reduce boilerplate in controller classes.
    /// </summary>
    /// <typeparam name="TEntity">The entity type managed by the repository.</typeparam>
    /// <typeparam name="TServiceInterface">The service interface type.</typeparam>
    /// <typeparam name="TService">The concrete service type.</typeparam>
    internal abstract class RepositoryServiceLocatorBase<TEntity, TServiceInterface, TService> :
        ServiceLocator<TServiceInterface, TService>,
        IRepository<TEntity>
        where TEntity : class
        where TServiceInterface : class
        where TService : ServiceLocator<TServiceInterface, TService>, TServiceInterface, new()
    {
        /// <summary>
        /// Protected repository controller that can be used by derived classes.
        /// </summary>
        protected readonly RepositoryControllerBase<TEntity> _repositoryControllerBase = new RepositoryControllerBase<TEntity>();

        void IRepository<TEntity>.Delete(TEntity item) => this._repositoryControllerBase.Delete(item);

        void IRepository<TEntity>.Delete(string sqlCondition, params object[] args) => this._repositoryControllerBase.Delete(sqlCondition, args);

        IEnumerable<TEntity> IRepository<TEntity>.Find(string sqlCondition, params object[] args) => this._repositoryControllerBase.Find(sqlCondition, args);

        IPagedList<TEntity> IRepository<TEntity>.Find(int pageIndex, int pageSize, string sqlCondition, params object[] args) => this._repositoryControllerBase.Find(pageIndex, pageSize, sqlCondition, args);

        IEnumerable<TEntity> IRepository<TEntity>.Get() => this._repositoryControllerBase.Get();

        IEnumerable<TEntity> IRepository<TEntity>.Get<TScopeType>(TScopeType scopeValue) => this._repositoryControllerBase.Get(scopeValue);

        TEntity IRepository<TEntity>.GetById<TProperty>(TProperty id) => this._repositoryControllerBase.GetById(id);

        TEntity IRepository<TEntity>.GetById<TProperty, TScopeType>(TProperty id, TScopeType scopeValue) => this._repositoryControllerBase.GetById(id: id, scopeValue: scopeValue);

        IPagedList<TEntity> IRepository<TEntity>.GetPage(int pageIndex, int pageSize) => this._repositoryControllerBase.GetPage(pageIndex, pageSize);

        IPagedList<TEntity> IRepository<TEntity>.GetPage<TScopeType>(TScopeType scopeValue, int pageIndex, int pageSize) => this._repositoryControllerBase.GetPage(scopeValue, pageIndex, pageSize);

        void IRepository<TEntity>.Insert(TEntity item) => this._repositoryControllerBase.Insert(item);

        void IRepository<TEntity>.Update(TEntity item) => this._repositoryControllerBase.Update(item);

        void IRepository<TEntity>.Update(string sqlCondition, params object[] args) => this._repositoryControllerBase.Update(sqlCondition, args);

        public int Count(string sqlCondition, params object[] args) => this._repositoryControllerBase.Count(sqlCondition, args);
    }
}
