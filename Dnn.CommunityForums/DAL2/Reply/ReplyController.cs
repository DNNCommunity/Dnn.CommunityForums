namespace DotNetNuke.Modules.ActiveForums.DAL2
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using DotNetNuke.Data;

    [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.ReplyController.")]
    class ReplyController : DotNetNuke.Modules.ActiveForums.Controllers.ReplyController
    {
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.ReplyController.")]
        public ReplyInfo Get(int replyId)
        {
            return (ReplyInfo)this.GetById(replyId);
        }
    }
}
