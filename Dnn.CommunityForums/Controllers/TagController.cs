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

    internal partial class TagController : RepositoryControllerBase<DotNetNuke.Modules.ActiveForums.Entities.TagInfo>
    {
        internal void RecountItems(int tagId)
        {
            var tag = new DotNetNuke.Modules.ActiveForums.Controllers.TagController().GetById(tagId);
            tag.Items = new DotNetNuke.Modules.ActiveForums.Controllers.TopicTagController().GetForTag(tagId).Count();
            new DotNetNuke.Modules.ActiveForums.Controllers.TagController().Update(tag);
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

        internal void Delete(DotNetNuke.Modules.ActiveForums.Entities.TagInfo item)
        {
            new DotNetNuke.Modules.ActiveForums.Controllers.TopicTagController().DeleteForTag(item.TagId);
            base.Delete(item);
        } 
    }
}
