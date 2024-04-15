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
using System;
using System.Collections.Generic;
using DotNetNuke.Services.Scheduling;
namespace DotNetNuke.Modules.ActiveForums.Services.EmailNotificationQueue
{
    public class Scheduler : DotNetNuke.Services.Scheduling.SchedulerClient
    {
        public Scheduler(ScheduleHistoryItem scheduleHistoryItem)
        {
            ScheduleHistoryItem = scheduleHistoryItem;
        }


        public override void DoWork()
        {
            try
            {
                var intQueueCount = ProcessQueue();
                ScheduleHistoryItem.Succeeded = true;
                ScheduleHistoryItem.AddLogNote(string.Concat("Processed ", intQueueCount, " messages"));
            }
            catch (Exception ex)
            {
                ScheduleHistoryItem.Succeeded = false;
                ScheduleHistoryItem.AddLogNote(string.Concat("Process Queue Failed. ", ex));
                Errored(ref ex);
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        private static int ProcessQueue()
        {
            var intQueueCount = 0;
            try
            {
                new DotNetNuke.Modules.ActiveForums.Controllers.EmailNotificationQueueController().GetBatch().ForEach(m =>
                {
                    intQueueCount += 1;
                    try
                    {
                        DotNetNuke.Modules.ActiveForums.Controllers.EmailController.SendNotification(m.PortalId, m.ModuleId, m.EmailFrom, m.EmailTo, m.EmailSubject, m.EmailBody);
                        new DotNetNuke.Modules.ActiveForums.Controllers.EmailNotificationQueueController().DeleteById(m.Id);
                    }
                    catch (Exception ex)
                    {
                        DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                        intQueueCount = intQueueCount - 1;
                    }
                });
                return intQueueCount;
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                return -1;
            }
        }
    }

}
