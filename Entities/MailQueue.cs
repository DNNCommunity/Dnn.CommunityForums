//
// Community Forums
// Copyright (c) 2013-2021
// by DNN Community
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
//
using DotNetNuke.ComponentModel.DataAnnotations;
using DotNetNuke.Modules.ActiveForums;
using System;
using System.Web.Caching;
namespace DotNetNuke.Modules.ActiveForums.Entities
{
    [TableName("activeforums_MailQueue")]
    [PrimaryKey("Id", AutoIncrement = true)] 
    [Cacheable("activeforums_MailQueue", CacheItemPriority.Normal)]
    public class MailQueue
    {
        public int Id { get; set; }
        public int PortalId { get; set; }
        public int ModuleId { get; set; }
        public string EmailFrom { get; set; }
        public string EmailTo { get; set; }
        public string EmailSubject { get; set; }
        public string EmailBody { get; set; }
        public string EmailBodyPlainText { get; set; }
        public string EmailCC { get; set; }
        public string EmailBCC { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
