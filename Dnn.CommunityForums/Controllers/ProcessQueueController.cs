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
namespace DotNetNuke.Modules.ActiveForums.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Data;
    using DotNetNuke.Modules.ActiveForums.Services.ProcessQueue;
    using DotNetNuke.Services.Scheduling;
    using DotNetNuke.UI.UserControls;

    internal class ProcessQueueController : DotNetNuke.Modules.ActiveForums.Controllers.RepositoryControllerBase<DotNetNuke.Modules.ActiveForums.Entities.ProcessQueueInfo>
    {
        public bool Add(ProcessType processType, int portalId, int tabId, int moduleId, int forumGroupId, int forumId, int topicId, int replyId, int authorId, string requestUrl)
        {
            try
            {
                this.Insert(new DotNetNuke.Modules.ActiveForums.Entities.ProcessQueueInfo
                {
                    PortalId = portalId,
                    ModuleId = moduleId,
                    ProcessType = processType,
                    ForumGroupId = forumGroupId,
                    ForumId = forumId,
                    TabId = tabId,
                    TopicId = topicId,
                    ReplyId = replyId,
                    AuthorId = authorId,
                    DateCreated = DateTime.UtcNow,
                    RequestUrl = requestUrl,
                });
                return true;
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                return false;
            }
        }

        public List<DotNetNuke.Modules.ActiveForums.Entities.ProcessQueueInfo> GetBatch()
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
