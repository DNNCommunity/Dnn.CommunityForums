﻿//
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

using System.Collections.Generic;
using System.Linq;

namespace DotNetNuke.Modules.ActiveForums.Controllers
{
    internal partial class LikeController : RepositoryControllerBase<DotNetNuke.Modules.ActiveForums.Entities.LikeInfo>
    {
        public LikeController() : base()
        {
        }
        public bool GetForUser(int userId, int postId)
        {
            return Repo.Get().Where(l => l.UserId == userId && l.PostId == postId && l.Checked).Select(l => l.Checked).FirstOrDefault();
        }
        public (int count,bool liked) Get(int userId, int postId)
        {
            return (Count(postId), GetForUser(userId, postId));
        }
        public List<DotNetNuke.Modules.ActiveForums.Entities.LikeInfo> GetForPost(int postId)
        {
            return Repo.Find("WHERE PostId = @0 AND Checked = 1", postId).ToList();
        }
        public int Count(int postId)
        {
            return Count("WHERE PostId = @0 AND Checked = 1", postId);
        }
        public int Like(int contentId, int userId)
        {
            DotNetNuke.Modules.ActiveForums.Entities.LikeInfo like = Repo.Find("Where PostId = @0 AND UserId = @1", contentId, userId).FirstOrDefault();
            if (like != null)
            {
                if (like.Checked)
                    like.Checked = false;
                else
                    like.Checked = true;
                Repo.Update(like);
            }
            else
            {
                like = new DotNetNuke.Modules.ActiveForums.Entities.LikeInfo();
                like.PostId = contentId;
                like.UserId = userId;
                like.Checked = true;
                Repo.Insert(like);
            }
            return Count(contentId);
        }
    }
}