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

namespace DotNetNuke.Modules.ActiveForums.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    internal class SearchResults
    {
        public int SearchId { get; }

        public int RowCount => this.Results?.Count ?? 0;

        public int SearchDuration { get; set; }

        public int SearchAge { get; }

        public List<DotNetNuke.Modules.ActiveForums.Entities.IPostInfo> Results { get; set; }

        public SearchResults()
        {
        }

        public SearchResults(DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0)
            {
                return;
            }

            var row = dt.Rows[0];
            this.SearchId = GetIntFromRow(row, 0);
            this.SearchDuration = GetIntFromRow(row, 2);
            this.SearchAge = GetIntFromRow(row, 3);
        }


        private static int GetIntFromRow(DataRow row, int index)
        {
            if (row == null || index < 0 || index >= row.Table.Columns.Count || row.IsNull(index))
            {
                return 0;
            }

            var val = row[index];
            if (val == null)
            {
                return 0;
            }

            int parsed;
            return int.TryParse(val.ToString(), out parsed) ? parsed : 0;
        }

    }
}
