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
using System;
using System.Globalization;
using System.Web.Caching;
namespace DotNetNuke.Modules.ActiveForums.Entities
{
    [TableName("activeforums_Subscriptions")]
    [PrimaryKey("Id", AutoIncrement = true)]
    [Scope("ModuleId")]
    [Cacheable("activeforums_Subscriptions", CacheItemPriority.Normal)]
    public class SubscriptionInfo
    {
        private string _email = string.Empty;
        private DotNetNuke.Modules.ActiveForums.User _user;
        public int Id { get; set; }
        public int PortalId { get; set; }
        public int ModuleId { get; set; }
        public int ForumId { get; set; }
        public int TopicId { get; set; }
        public int Mode { get; set; }
        public int UserId { get; set; }
        public bool TopicSubscriber { get => TopicId > 0; }
        public DotNetNuke.Modules.ActiveForums.User User
        {
            get
            {
                if (_user == null)
                {
                    _user = new DotNetNuke.Modules.ActiveForums.UserController().GetUser(PortalId, ModuleId, UserId);
                }
                return _user;
            }
        }
        public string Email { get => (_email = User?.Email); set => _email = value; } 
        public TimeSpan TimeZoneOffSet { get => Utilities.GetTimeZoneOffsetForUser(PortalId, UserId); }
        public CultureInfo UserCulture { get => Utilities.GetCultureInfoForUser(PortalId, UserId); }
    }
}
