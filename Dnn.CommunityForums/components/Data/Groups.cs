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
    using System.Data;

    using Microsoft.ApplicationBlocks.Data;

    [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
    public class Groups : DataConfig
    {
        // Public Function Forums_List(ByVal PortalId As Integer, ByVal ModuleId As Integer) As IDataReader
        //    Return SqlHelper.ExecuteReader(_connectionString, dbPrefix & "Forums_GetPermissions", PortalId, ModuleId)
        // End Function
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public IDataReader Groups_Get(int moduleId, int forumGroupId)
        {
            return SqlHelper.ExecuteReader(this.connectionString, this.dbPrefix + "Groups_Get", moduleId, forumGroupId);
        }
    }
}
