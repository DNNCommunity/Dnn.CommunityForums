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
    using System.Linq;
    using System.Web.UI.WebControls;

    using DotNetNuke.Modules.ActiveForums.Services.ProcessQueue;

    internal class ProcessQueueController : RepositoryServiceLocatorBase<DotNetNuke.Modules.ActiveForums.Entities.ProcessQueueInfo, IProcessQueueController, ProcessQueueController>, IProcessQueueController
    {
        protected override Func<IProcessQueueController> GetFactory()
        {
            return () => new ProcessQueueController();
        }
        
        public bool Add(ProcessType processType, int portalId, int tabId, int moduleId, int forumGroupId, int forumId, int topicId, int replyId, int contentId, int authorId, int userId, int badgeId, DateTime dateCreated, string requestUrl)
        {
            try
            {
                this._repositoryControllerBase.Insert(new DotNetNuke.Modules.ActiveForums.Entities.ProcessQueueInfo
                {
                    PortalId = portalId,
                    ModuleId = moduleId,
                    ProcessType = processType,
                    ForumGroupId = forumGroupId,
                    ForumId = forumId,
                    TabId = tabId,
                    TopicId = topicId,
                    ReplyId = replyId,
                    ContentId = contentId,
                    AuthorId = authorId,
                    UserId = userId,
                    BadgeId = badgeId,
                    DateCreated = dateCreated,
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

        public void DeleteById<TProperty>(TProperty id)
        {
            try
            {
                this._repositoryControllerBase.DeleteById(id);
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        public List<DotNetNuke.Modules.ActiveForums.Entities.ProcessQueueInfo> GetBatch()
        {
            try
            {
                return this._repositoryControllerBase.Get().OrderBy(m => m.DateCreated).Take(200).ToList();
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                return null;
            }
        }
    }
}
