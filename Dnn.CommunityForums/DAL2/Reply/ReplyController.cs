using DotNetNuke.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNetNuke.Modules.ActiveForums.DAL2
{
    class ReplyController
    {
        IDataContext ctx;
        IRepository<DotNetNuke.Modules.ActiveForums.ReplyInfo> repo;

        public ReplyController()
        {
            ctx = DataContext.Instance();
            repo = ctx.GetRepository<DotNetNuke.Modules.ActiveForums.ReplyInfo>();
        }

        public DotNetNuke.Modules.ActiveForums.ReplyInfo Get(int replyId)
        {
            var reply = repo.GetById(replyId);
            return reply;
        }
    }
}
