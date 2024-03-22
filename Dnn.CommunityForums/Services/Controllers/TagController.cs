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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DotNetNuke.Web.Api;
using System.Web.Http;
using System.Data;
using System.Reflection;
using System.Web;

namespace DotNetNuke.Modules.ActiveForums.Services.Controllers
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public class TagController : ControllerBase<TagController>
    {
        public struct TopicDto1
        {
            public int ForumId { get; set; }
            public int TopicId { get; set; }
        }
        public struct TopicDto2
        {
            public int ForumId { get; set; }
            public DotNetNuke.Modules.ActiveForums.Entities.TopicInfo Topic { get; set; }
        }

        /// <summary>
        /// Subscribes to a Topic
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        /// <remarks>https://dnndev.me/API/ActiveForums/Topic/Subscribe</remarks>
        [HttpPost]
        [DnnAuthorize]
        [ForumsAuthorize(SecureActions.Subscribe)]
        public HttpResponseMessage Subscribe(TopicDto1 dto)
        {
            if (dto.TopicId > 0 && dto.ForumId > 0)
            { 
                {
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    string q = string.Empty;
                    if (!(string.IsNullOrEmpty(HttpContext.Current.Request.QueryString["q"])))
                    {
                        q = HttpContext.Current.Request.QueryString["q"].Trim();
                        q = Utilities.Text.RemoveHTML(q);
                        q = Utilities.Text.CheckSqlString(q);
                        if (!(string.IsNullOrEmpty(q)))
                        {
                            if (q.Length > 20)
                            {
                                q = q.Substring(0, 20);
                            }
                        }
                    }
                    int i = 0;
                    if (!(string.IsNullOrEmpty(q)))
                    {
                        using (IDataReader dr = DataProvider.Instance().Tags_Search(PortalId, ModuleId, q))
                        {
                            while (dr.Read())
                            {
                                sb.AppendLine("{\"id\":\"" + dr["TagId"].ToString() + "\",\"name\":\"" + dr["TagName"].ToString() + "\",\"type\":\"0\"},");
                                i += 1;
                            }
                            dr.Close();
                        }
                    }
                    string @out = "[";
                    if (i > 0)
                    {
                        @out += sb.ToString().Trim();
                        @out = @out.Substring(0, @out.Length - 1);
                    }
                    @out += "]";
                    return @out;
                }

                string userRoles = new DotNetNuke.Modules.ActiveForums.UserProfileController()
                    .Profiles_Get(ActiveModule.PortalID, ForumModuleId, UserInfo.UserID).Roles;
                int subscribed = new SubscriptionController().Subscription_Update(ActiveModule.PortalID,
                    ForumModuleId, dto.ForumId, dto.TopicId, 1, UserInfo.UserID, userRoles);
                return Request.CreateResponse(HttpStatusCode.OK, subscribed == 1);
            }

            return Request.CreateResponse(HttpStatusCode.BadRequest);
        }
    }
}
