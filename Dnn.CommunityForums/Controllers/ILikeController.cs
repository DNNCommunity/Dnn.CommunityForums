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
    using System.Collections.Generic;

    using DotNetNuke.Data;

    internal interface ILikeController : IRepository<DotNetNuke.Modules.ActiveForums.Entities.LikeInfo>
    {
        IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.LikeInfo> GetForPost(int portalId, int moduleId, int postId);

        bool GetLikedByUser(int portalId, int moduleId, int userId, int postId);

        DotNetNuke.Modules.ActiveForums.Entities.LikeInfo GetForUser(int portalId, int moduleId, int userId, int postId);

        int Like(int portalId, int moduleId, int contentId, int userId, int authorId, int tabId, int forumGroupId, int forumId, int replyId, int topicId, string requestUrl);

        DotNetNuke.Modules.ActiveForums.Entities.LikeInfo GetById(int portalId, int moduleId, int id);

        int Count(int moduleId, int postId);

        (int Count, bool Liked) Get(int portalId, int moduleId, int userId, int postId);

        int Count(string sqlCondition, params object[] args);
    }

}
