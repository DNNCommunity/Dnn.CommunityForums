// Copyright (c) 2013-2024 by DNN Community
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
    using System.Linq;

    using DotNetNuke.Data;
    using DotNetNuke.Services.Scheduling;
    using DotNetNuke.UI.UserControls;

    internal class EmailNotificationQueueController : DotNetNuke.Modules.ActiveForums.Controllers.RepositoryControllerBase<DotNetNuke.Modules.ActiveForums.Entities.EmailNotificationQueueInfo>
    {
        public void Add(int portalId, int moduleId, string emailFrom, string emailTo, string emailSubject, string emailBody)
        {
            try
            {
                this.Insert(new DotNetNuke.Modules.ActiveForums.Entities.EmailNotificationQueueInfo
                {
                    PortalId = portalId,
                    ModuleId = moduleId,
                    EmailFrom = emailFrom,
                    EmailTo = emailTo,
                    EmailBody = emailBody,
                    EmailSubject = emailSubject,
                    DateCreated = DateTime.UtcNow,
                });
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        public List<DotNetNuke.Modules.ActiveForums.Entities.EmailNotificationQueueInfo> GetBatch()
        {
            try
            {
                return this.Get().OrderBy(m => m.DateCreated).Take(200).ToList();
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                return null;
            }
        }
    }
}
