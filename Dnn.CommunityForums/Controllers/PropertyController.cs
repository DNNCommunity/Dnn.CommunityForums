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
    using System.Text;

    using DotNetNuke.Modules.ActiveForums.Entities;

    internal class PropertyController : RepositoryServiceLocatorBase<DotNetNuke.Modules.ActiveForums.Entities.PropertyInfo, IPropertyController, PropertyController>, IPropertyController
    {
        private readonly ITopicPropertyController topicPropertyController;

        protected override Func<IPropertyController> GetFactory()
        {
            return () => new PropertyController();
        }

        public PropertyController()
            : this(DotNetNuke.Modules.ActiveForums.Controllers.TopicPropertyController.Instance)
        {
        }

        internal PropertyController(ITopicPropertyController topicPropertyController)
        {
            this.topicPropertyController = topicPropertyController ?? throw new ArgumentNullException(nameof(topicPropertyController));
        }

        public void Delete(DotNetNuke.Modules.ActiveForums.Entities.PropertyInfo item)
        {
            if (item == null)
            {
                return;
            }

            this.topicPropertyController.DeleteForProperty(item.PropertyId);
            this._repositoryControllerBase.Delete(item);
        }

        public string ListPropertiesJSON(int portalId, int objectType, int objectOwnerId)
        {
            var list = DotNetNuke.Modules.ActiveForums.Controllers.PropertyController.Instance.Find("WHERE PortalId = @0 AND ObjectType = @1 AND ObjectOwnerId = @2", portalId, objectType, objectOwnerId);
            StringBuilder sb = new StringBuilder();
            foreach (var p in list)
            {
                sb.Append("{");
                sb.Append(Utilities.JSON.Pair("PropertyId", p.PropertyId.ToString()));
                sb.Append(",");
                sb.Append(Utilities.JSON.Pair("PortalId", p.PortalId.ToString()));
                sb.Append(",");
                sb.Append(Utilities.JSON.Pair("ObjectType", p.ObjectType.ToString()));
                sb.Append(",");
                sb.Append(Utilities.JSON.Pair("ObjectOwnerId", p.ObjectOwnerId.ToString()));
                sb.Append(",");
                sb.Append(Utilities.JSON.Pair("Name", p.Name?.ToString()));
                sb.Append(",");
                sb.Append(Utilities.JSON.Pair("DataType", p.DataType?.ToString()));
                sb.Append(",");
                sb.Append(Utilities.JSON.Pair("DefaultAccessControl", Convert.ToInt32(p.DefaultAccessControl).ToString()));
                sb.Append(",");
                sb.Append(Utilities.JSON.Pair("IsHidden", p.IsHidden.ToString().ToLowerInvariant()));
                sb.Append(",");
                sb.Append(Utilities.JSON.Pair("IsReadOnly", p.IsReadOnly.ToString().ToLowerInvariant()));
                sb.Append(",");
                sb.Append(Utilities.JSON.Pair("IsRequired", p.IsRequired.ToString().ToLowerInvariant()));
                sb.Append(",");
                sb.Append(Utilities.JSON.Pair("ValidationExpression", System.Net.WebUtility.UrlEncode(System.Net.WebUtility.HtmlEncode(p.ValidationExpression.ToString()))));
                sb.Append(",");
                sb.Append(Utilities.JSON.Pair("ViewTemplate", p.ViewTemplate?.ToString()));
                sb.Append(",");
                sb.Append(Utilities.JSON.Pair("EditTemplate", p.EditTemplate?.ToString()));
                sb.Append(",");
                sb.Append(Utilities.JSON.Pair("SortOrder", p.SortOrder.ToString()));
                sb.Append(",");
                sb.Append(Utilities.JSON.Pair("DefaultValue", p.DefaultValue?.ToString()));
                sb.Append(",");
                sb.Append(Utilities.JSON.Pair("Label", System.Net.WebUtility.HtmlEncode("[RESX:" + p.Name + "]")));
                sb.Append("},");
            }

            if (sb.Length > 2)
            {
                sb.Remove(sb.Length - 1, 1);
            }

            return sb.ToString();
        }

        public void DeleteById(int id)
        {
            this.topicPropertyController.DeleteForProperty(id);
            this.Delete(this._repositoryControllerBase.GetById(id));
        }

        public DotNetNuke.Modules.ActiveForums.Entities.PropertyInfo Save<TProperty>(DotNetNuke.Modules.ActiveForums.Entities.PropertyInfo item, TProperty id)
        {
            if (id == null || id.Equals(0) || id.Equals(-1) || this._repositoryControllerBase.GetById(id) == null)
            {
                this._repositoryControllerBase.Insert(item);
            }
            else
            {
                this._repositoryControllerBase.Update(item);
            }

            return item;
        }

        public PropertyInfo Save(PropertyInfo item, int id)
        {
            throw new NotImplementedException();
        }
    }
}
