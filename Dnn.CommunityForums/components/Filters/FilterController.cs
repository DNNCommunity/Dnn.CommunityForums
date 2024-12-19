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

namespace DotNetNuke.Modules.ActiveForums
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Web;

    [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.FilterController")]
    public class FilterController
    {
        private DotNetNuke.Modules.ActiveForums.Controllers.FilterController filterController = new DotNetNuke.Modules.ActiveForums.Controllers.FilterController();

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.FilterController.Update()")]
        public DotNetNuke.Modules.ActiveForums.FilterInfo Filter_Save(DotNetNuke.Modules.ActiveForums.FilterInfo filter)
        {
            if (filter.FilterId > 0)
            {
                this.filterController.Update(filter);
            }
            else
            {
                this.filterController.Insert(filter);
            }

            return (DotNetNuke.Modules.ActiveForums.FilterInfo)this.filterController.GetById(filter.FilterId);
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.FilterController.Delete()")]
        public void Filter_Delete(int portalId, int moduleId, int filterId)
        {
            this.filterController.DeleteById(filterId);
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.FilterController.GetById()")]
        public DotNetNuke.Modules.ActiveForums.FilterInfo Filter_Get(int portalId, int moduleID, int filterId)
        {
            return (DotNetNuke.Modules.ActiveForums.FilterInfo)this.filterController.GetById(filterId, moduleID);
        }
    }
}
