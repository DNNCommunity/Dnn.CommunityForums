using DotNetNuke.Data;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNetNuke.Modules.ActiveForums.DAL2
{
    [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.ReplyController.")]
    class ReplyController : DotNetNuke.Modules.ActiveForums.Controllers.ReplyController
    {
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.ReplyController.")]
        public ReplyInfo Get(int replyId)
        {
            return (ReplyInfo)base.GetById(replyId);
        }
    }
}
