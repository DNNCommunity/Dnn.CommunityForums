using DotNetNuke.Data;
using DotNetNuke.Modules.ActiveForums.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
namespace DotNetNuke.Modules.ActiveForums
{
    [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Replace with DotNetNuke.Modules.ActiveForums.Controllers.LikesController")]
    class LikesController : DotNetNuke.Modules.ActiveForums.Controllers.LikeController
    {
        public List<Like> GetForPost(int postId)
        {
            return base.GetForPost(postId);
        }
        public void Like(int contentId, int userId)
        {
            base.Like(contentId, userId);
        }
    }
}
namespace DotNetNuke.Modules.ActiveForums.Controllers
{
    class LikeController
    {
        public List<Like> GetForPost(int postId)
        {
            List<Like> likes = new List<Like>();
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<Like>();
                likes = rep.Find("WHERE PostId = @0 AND Checked = 1", postId).ToList();
            }
            return likes;
        }
        public void Like(int contentId, int userId)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<Like>();
                var like = rep.Find("Where PostId = @0 AND UserId = @1", contentId, userId).FirstOrDefault();

                if (like != null)
                {
                    if (like.Checked)
                        like.Checked = false;
                    else
                        like.Checked = true;
                    rep.Update(like);
                }
                else
                {
                    like = new Like();
                    like.PostId = contentId;
                    like.UserId = userId;
                    like.Checked = true;
                    rep.Insert(like);
                }
            }
        }
    }
}