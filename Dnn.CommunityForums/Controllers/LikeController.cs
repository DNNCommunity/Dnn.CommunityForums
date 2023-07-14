using DotNetNuke.Data;
using DotNetNuke.Modules.ActiveForums.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

#region Deprecated
namespace DotNetNuke.Modules.ActiveForums
{
    [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Replace with DotNetNuke.Modules.ActiveForums.Controllers.LikesController")]
    class LikesController : DotNetNuke.Modules.ActiveForums.Controllers.LikeController
    {
        [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Replace with DotNetNuke.Modules.ActiveForums.Controllers.LikesController.GetForPost()")]
        public new List<DotNetNuke.Modules.ActiveForums.Likes> GetForPost(int postId)
        {
            IDataContext ctx = DataContext.Instance();
            IRepository<DotNetNuke.Modules.ActiveForums.Entities.Like> repo = ctx.GetRepository<DotNetNuke.Modules.ActiveForums.Entities.Like>();
            List<DotNetNuke.Modules.ActiveForums.Likes> likes = new List<DotNetNuke.Modules.ActiveForums.Likes>();
            foreach (DotNetNuke.Modules.ActiveForums.Entities.Like like in base.GetForPost(postId))
            {
                likes.Add((DotNetNuke.Modules.ActiveForums.Likes)like);
            }
            return likes; 
        }
        [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Replace with DotNetNuke.Modules.ActiveForums.Controllers.LikesController.Like()")]
        public new void Like(int contentId, int userId)
        {
            base.Like(contentId, userId);
        }
    }
}
#endregion

namespace DotNetNuke.Modules.ActiveForums.Controllers
{
    class LikeController
    {
        readonly IDataContext ctx;
        IRepository<DotNetNuke.Modules.ActiveForums.Entities.Like> repo;
        public LikeController()
        {
            ctx = DataContext.Instance();
            repo = ctx.GetRepository<DotNetNuke.Modules.ActiveForums.Entities.Like>();
        }
        public bool GetForUser(int userId, int postId)
        {
            return repo.Get().Where(l => l.UserId == userId && l.PostId == postId && l.Checked).Select(l => l.Checked).FirstOrDefault();
        }
        public (int count,bool liked) Get(int userId, int postId)
        {
            return (Count(postId), GetForUser(userId, postId));
        }
        public List<DotNetNuke.Modules.ActiveForums.Entities.Like> GetForPost(int postId)
        {
            return repo.Find("WHERE PostId = @0 AND Checked = 1", postId).ToList();
        }
        public int Count(int postId)
        {
            return repo.Find("WHERE PostId = @0 AND Checked = 1", postId).Count();
        }
        public int Like(int contentId, int userId)
        {
            DotNetNuke.Modules.ActiveForums.Entities.Like like = repo.Find("Where PostId = @0 AND UserId = @1", contentId, userId).FirstOrDefault();
            if (like != null)
            {
                if (like.Checked)
                    like.Checked = false;
                else
                    like.Checked = true;
                repo.Update(like);
            }
            else
            {
                like = new DotNetNuke.Modules.ActiveForums.Entities.Like();
                like.PostId = contentId;
                like.UserId = userId;
                like.Checked = true;
                repo.Insert(like);
            }
            return Count(contentId);
        }
    }
}