using DotNetNuke.ComponentModel.DataAnnotations;
using System;
using System.Web.Caching;
namespace DotNetNuke.Modules.ActiveForums
{
    [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Replace with DotNetNuke.Modules.ActiveForums.Entities.Likes")]
    class Likes : DotNetNuke.Modules.ActiveForums.Entities.Likes { }
}
namespace DotNetNuke.Modules.ActiveForums.Entities
{
    [TableName("activeforums_Likes")]
    [PrimaryKey("Id", AutoIncrement = true)]
    [Scope("PostId")]
    [Cacheable("activeforums_Likes", CacheItemPriority.Normal)]
    class Likes
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public int UserId { get; set; }
        public bool Checked { get; set; }
    }
}
