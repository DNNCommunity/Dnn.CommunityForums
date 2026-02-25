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

    internal class PropertyController : DotNetNuke.Modules.ActiveForums.Controllers.RepositoryControllerBase<DotNetNuke.Modules.ActiveForums.Entities.PropertyInfo>
    {
        private IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.PropertyInfo> Properties { get; set; }

        public PropertyController() : base()
        {
        }

        public PropertyController(IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.PropertyInfo> properties)
        {
            this.Properties = properties;
        }

        internal string ListPropertiesJSON(int portalId, int objectType, int objectOwnerId)
        {
            var list = new DotNetNuke.Modules.ActiveForums.Controllers.PropertyController().Find("WHERE PortalId = @0 AND ObjectType = @1 AND ObjectOwnerId = @2", portalId, objectType, objectOwnerId);
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
    }
}
