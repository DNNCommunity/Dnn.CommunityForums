//
// Community Forums
// Copyright (c) 2013-2021
// by DNN Community
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
//
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.UI;
using DotNetNuke.Collections;
using DotNetNuke.Data;
namespace DotNetNuke.Modules.ActiveForums.Controllers
{
    internal partial class RepositoryControllerBase<T> where T : class
    {
        private readonly IRepository<T> Repo;
        internal RepositoryControllerBase()
        {
            var ctx = DataContext.Instance();
            Repo = ctx.GetRepository<T>();
        }
        internal RepositoryControllerBase(int moduleId)
        {
            var ctx = DataContext.Instance();
            Repo = ctx.GetRepository<T>();
            ModuleId = moduleId;
        }
        internal IEnumerable<T> Get()
        {
            return ModuleId > 0 ? Repo.Get(ModuleId) : Repo.Get();
        }
        internal IEnumerable<T> Get<TScopeType>(TScopeType scopeValue)
        {
            return Repo.Get(scopeValue);
        }
        internal T GetById<TProperty>(TProperty id)
        {
            return ModuleId > 0 ? Repo.GetById(id, ModuleId) : Repo.GetById(id);
        }
        internal T GetById<TProperty, TScopeType>(TProperty id, TScopeType scopeValue)
        {
            return Repo.GetById(id, scopeValue);
        }
        internal T GetById<TProperty, TScopeType>(TProperty id, TScopeType scopeValue)
        {
            return Repo.GetById(id, scopeValue);
        }
        internal T Save<TProperty>(T item, TProperty id)
        {
            if (id == null || id.Equals(0) || id.Equals(-1) || GetById(id) == null)
            {
                Insert(item);
            }
            else
            {
                Update(item);
            }
            return item;
        }
        internal IEnumerable<T> Find(string sqlCondition, params object[] args)
        {
            return string.IsNullOrEmpty(sqlCondition) ? ModuleId > 0 ? Get(ModuleId) : Get() : Repo.Find(sqlCondition, args);
        }
        internal IPagedList<T> Find(int pageIndex, int pageSize, string sqlCondition, params object[] args)
        {
            return Repo.Find(pageIndex, pageSize, sqlCondition, args);
        }
        internal IPagedList<T> Find(int pageIndex, int pageSize, string sqlCondition, params object[] args)
        {
            return Repo.Find(pageIndex, pageSize, sqlCondition, args);
        }
        internal void Update(T item)
        {
            Repo.Update(item);
        }
        public void Update(string sqlCondition, params object[] args)
        {
            Repo.Update(sqlCondition, args);
        }
        internal void Insert(T item)
        {
            Repo.Insert(item);
        }
        internal void Delete(string sqlCondition, params object[] args)
        {
            Repo.Delete(sqlCondition, args);
        }
        internal void DeleteById<TProperty>(TProperty id)
        {
            Repo.Delete(Repo.GetById(id));
        }
        internal void Delete(T item)
        {
            Repo.Delete(item);
        }
        internal void DeleteByModuleId(int ModuleId)
        {
            Repo.Delete("WHERE (ModuleId = @0)", ModuleId);
        }
        internal int Count(string sqlCondition, params object[] args)
        {
            return string.IsNullOrEmpty(sqlCondition) ? Repo.Get().Count() : Repo.Find(sqlCondition, args).Count(); 
        }
        internal IPagedList<T> GetPage(int pageIndex, int pageSize)
        {
            return Repo.GetPage(pageIndex, pageSize);
        }
        internal IPagedList<T> GetPage<TScopeType>(TScopeType scopeValue, int pageIndex, int pageSize)
        {
            return Repo.GetPage(scopeValue, pageIndex, pageSize);
        }
    }
}