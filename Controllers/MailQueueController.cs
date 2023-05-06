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
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN doworkafaffdaafdfdfffdfdffd WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
//
using System;
using System.Collections.Generic;
using System.Linq;
using DotNetNuke.Data;
using DotNetNuke.Services.Scheduling;
using DotNetNuke.UI.UserControls;

namespace DotNetNuke.Modules.ActiveForums.Controllers
{
    public class MailQueueController
    {
        public static void Add(int portalId, int moduleId, string emailFrom, string emailTo, string emailSubject, string emailBody, string emailBodyPlainText, string emailCC, string emailBcc)
        {
            try
            {
                using (IDataContext ctx = DataContext.Instance())
                {
                    var repo = ctx.GetRepository<DotNetNuke.Modules.ActiveForums.Entities.MailQueue>();
                    repo.Insert(new DotNetNuke.Modules.ActiveForums.Entities.MailQueue
                    {
                        PortalId = portalId,
                        ModuleId = moduleId,
                        EmailFrom = emailFrom,
                        EmailTo = emailTo,
                        EmailCC = emailCC,
                        EmailBCC = emailBcc,
                        EmailBody = emailBody,
                        EmailBodyPlainText = emailBodyPlainText,
                        EmailSubject = emailSubject,
                        DateCreated = DateTime.UtcNow
                    });
                }
                DataProvider.Instance().MailQueue_Add(portalId, moduleId, emailFrom, emailTo, emailSubject, emailBody, emailBodyPlainText, emailCC, emailBcc);
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }
        public static void Delete(int Id)
        {
            try
            {
                using (IDataContext ctx = DataContext.Instance())
                {
                    var repo = ctx.GetRepository<DotNetNuke.Modules.ActiveForums.Entities.MailQueue>();
                    repo.Delete(repo.GetById(Id));
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }
        public static List<DotNetNuke.Modules.ActiveForums.Entities.MailQueue> GetBatch()
        {
            try
            {
                using (IDataContext ctx = DataContext.Instance())
                {
                    var repo = ctx.GetRepository<DotNetNuke.Modules.ActiveForums.Entities.MailQueue>();
                    return repo.Get().OrderBy(m => m.DateCreated).Take(200).ToList();
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                return null;
            }
        }
    }
} 