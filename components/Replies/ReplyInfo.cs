using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetNuke.Modules.ActiveForums
{
        #region ReplyInfo
        public class ReplyInfo
        {
        #region Public Properties
        public int ReplyId { get; set; }
        public int ReplyToId { get; set; }
        public int TopicId { get; set; }
        public int ContentId { get; set; }
        public int StatusId { get; set; }
        public bool IsApproved { get; set; }
        public bool IsDeleted { get; set; }
        public Content Content { get; set; }
        public Author Author { get; set; }
        #endregion
        public ReplyInfo()
            {
                Content = new Content();
                Author = new Author();
            }
        }
        #endregion
    }
