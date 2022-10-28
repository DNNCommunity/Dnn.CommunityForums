using DotNetNuke.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNetNuke.Modules.ActiveForums.Entities
{
    [TableName("activeforums_Replies")]
    [PrimaryKey("ReplyId")]
    public partial class ReplyInfo
    {
        public int ReplyId { get; set; }
        public int TopicId { get; set; }
        public int ReplyToId { get; set; }
        public int ContentId { get; set; }
        public bool IsApproved { get; set; }
        public bool IsRejected { get; set; }
        public int StatusId { get; set; }
        public bool IsDeleted { get; set; }
    }
}
