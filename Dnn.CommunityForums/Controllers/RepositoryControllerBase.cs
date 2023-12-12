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
using System.Linq;
using DotNetNuke.Data;
namespace DotNetNuke.Modules.ActiveForums.Controllers
{
    internal partial class RepositoryControllerBase<T> where T : class
    {
        internal readonly IRepository<T> Repo;
        internal RepositoryControllerBase()
        {
            var ctx = DataContext.Instance();
            Repo = ctx.GetRepository<T>();
        }
        internal T GetById<TProperty>(TProperty id)
        {
            var content = Repo.GetById(id);
            return content;
        }
        internal T Get(int id)
        {
            var content = Repo.GetById(id);
            return content;
        }
        internal void Update(T item)
        {
            Repo.Update(item);
        }
        internal void Insert(T item)
        {
            Repo.Insert(item);
        }
        internal void Delete(string sqlCondition, params object[] args)
        {
            Repo.Delete(sqlCondition, args);
        }
        internal void Delete(T item)
        {
            Repo.Delete(item);
        }
        internal int Count(string sqlCondition, params object[] args)
        {
            return string.IsNullOrEmpty(sqlCondition) ? Repo.Get().Count() : Repo.Find(sqlCondition, args).Count(); 
        }
    }
}