namespace DotNetNuke.Modules.ActiveForums.DAL2
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using DotNetNuke.Data;

    [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.ContentController.")]
    class ContentController : DotNetNuke.Modules.ActiveForums.Controllers.ContentController
    {
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.ContentController.")]
        public ContentController()
        {
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.ContentController.")]
        public Content Get(int contentId)
        {
            return (Content)this.GetById(contentId);
        }
    }
}
