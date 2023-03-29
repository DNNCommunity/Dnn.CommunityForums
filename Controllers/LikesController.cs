using DotNetNuke.Data;
using DotNetNuke.Modules.ActiveForums.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
namespace DotNetNuke.Modules.ActiveForums
{
    [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Replace with DotNetNuke.Modules.ActiveForums.Controllers.LikesController")]
    class LikesController : DotNetNuke.Modules.ActiveForums.Controllers.LikesController
    {
        [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Replace with DotNetNuke.Modules.ActiveForums.Controllers.LikesController.GetForPost()")]
        public new List<DotNetNuke.Modules.ActiveForums.Likes> GetForPost(int postId)
        {
            IDataContext ctx = DataContext.Instance();
            IRepository<DotNetNuke.Modules.ActiveForums.Entities.Likes> repo = ctx.GetRepository<DotNetNuke.Modules.ActiveForums.Entities.Likes>();
            List<DotNetNuke.Modules.ActiveForums.Likes> likes = new List<DotNetNuke.Modules.ActiveForums.Likes>();
            foreach (DotNetNuke.Modules.ActiveForums.Entities.Likes like in base.GetForPost(postId))
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
namespace DotNetNuke.Modules.ActiveForums.Controllers
{
    class LikesController
    {
        readonly IDataContext ctx;
        IRepository<DotNetNuke.Modules.ActiveForums.Entities.Likes> repo;
        public LikesController()
        {
            ctx = DataContext.Instance();
            repo = ctx.GetRepository<DotNetNuke.Modules.ActiveForums.Entities.Likes>();
        }
        public List<DotNetNuke.Modules.ActiveForums.Entities.Likes> GetForPost(int postId)
        { 
            return repo.Find("WHERE PostId = @0 AND Checked = 1", postId).ToList();
        }
        public void Like(int contentId, int userId)
        {
            DotNetNuke.Modules.ActiveForums.Entities.Likes like = repo.Find("Where PostId = @0 AND UserId = @1", contentId, userId).FirstOrDefault();
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
                like = new Entities.Likes();
                like.PostId = contentId;
                like.UserId = userId;
                like.Checked = true;
                repo.Insert(like);
            }            
        }
    }
}