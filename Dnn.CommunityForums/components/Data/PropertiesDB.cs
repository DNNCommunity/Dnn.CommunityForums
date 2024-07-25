//
// Community Forums
// Copyright (c) 2013-2024
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

namespace DotNetNuke.Modules.ActiveForums.Data
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;

    using Microsoft.ApplicationBlocks.Data;

    [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No longer used.")]
    public class PropertiesDB : Connection
    {
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No longer used.")]
        internal IDataReader ListProperties(int portalId, int objectType, int objectOwnerId)
        {
            return (IDataReader)SqlHelper.ExecuteReader(this.connectionString, this.dbPrefix + "Properties_List", portalId, objectType, objectOwnerId);
        }

        internal IDataReader GetProperties(int propertyId, int portalId)
        {
            return (IDataReader)SqlHelper.ExecuteReader(this.connectionString, this.dbPrefix + "Properties_Get", propertyId, portalId);
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No longer used.")]
        internal int SaveProperty(int propertyId, int portalId, int objectType, int objectOwnerId, string name, string dataType, int defaultAccessControl, bool isHidden, bool isRequired, bool isReadOnly, string validationExpression, string editTemplate, string viewTemplate, int sortOrder, string defaultValue)
        {
            return Convert.ToInt32(SqlHelper.ExecuteScalar(this.connectionString, this.dbPrefix + "Properties_Save", propertyId, portalId, objectType, objectOwnerId, name, dataType, defaultAccessControl, isHidden, isRequired, validationExpression, editTemplate, viewTemplate, isReadOnly, sortOrder, defaultValue));
        }

        internal void SortRebuild(int portalId, int objectType, int objectOwnerId)
        {
            SqlHelper.ExecuteNonQuery(this.connectionString, this.dbPrefix + "Properties_RebuildSort", portalId, objectType, objectOwnerId);
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No longer used.")]
        internal void DeleteProperty(int portalId, int propertyId)
        {
            SqlHelper.ExecuteNonQuery(this.connectionString, this.dbPrefix + "Properties_DeleteDefTopicProp", portalId, propertyId);
        }
    }
}
