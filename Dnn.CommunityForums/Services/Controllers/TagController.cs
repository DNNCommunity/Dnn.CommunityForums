// Copyright (c) by DNN Community
//
// DNN Community licenses this file to you under the MIT license.
//
// See the LICENSE file in the project root for more information.
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

namespace DotNetNuke.Modules.ActiveForums.Services.Controllers
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using DotNetNuke.Web.Api;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public class TagController : ControllerBase<TagController>
    {
#pragma warning disable CS1570
        /// <summary>
        /// Gets Tags matching a string anywhere in string
        /// </summary>
        /// <param name="forumId" type="int"></param>
        /// <param name="query" type="string"></param>
        /// <returns></returns>
        /// <remarks>https://dnndev.me/API/ActiveForums/Tag/Matches?ForumId=xxx&query=xxx</remarks>
#pragma warning restore CS1570
        [HttpGet]
        [DnnAuthorize]
        [ForumsAuthorize(SecureActions.Edit)]
        public HttpResponseMessage Matches(int forumId, string query)
        {
            return this.Match($"%{DotNetNuke.Modules.ActiveForums.Services.ServicesHelper.CleanAndChopString(query, 20)}%");
        }

#pragma warning disable CS1570
        /// <summary>
        /// Gets Tags with names matching string from beginning
        /// </summary>
        /// <param name="forumId" type="int"></param>
        /// <param name="query" type="string"></param>
        /// <returns></returns>
        /// <remarks>https://dnndev.me/API/ActiveForums/Tag/BeginsWith?ForumId=xxxquery=xxx</remarks>
#pragma warning restore CS1570
        [HttpGet]
        [DnnAuthorize]
        [ForumsAuthorize(SecureActions.Edit)]
        public HttpResponseMessage BeginsWith(int forumId, string query)
        {
            return this.Match($"{DotNetNuke.Modules.ActiveForums.Services.ServicesHelper.CleanAndChopString(query, 20)}%");
        }

        private HttpResponseMessage Match(string matchString)
        {
            try
            {
                if (!string.IsNullOrEmpty(matchString))
                {
                    var matchingTags = new DotNetNuke.Modules.ActiveForums.Controllers.TagController().Find("WHERE PortalId = @0 AND ModuleId = @1 AND TagName LIKE @2 ORDER By TagName", this.ActiveModule.PortalID, this.ForumModuleId, $"{DotNetNuke.Modules.ActiveForums.Services.ServicesHelper.CleanAndChopString(matchString, 20)}%").Select(t => new { id = t.TagId, name = t.TagName }).ToList();
                    if (matchingTags.Count > 0)
                    {
                        return this.Request.CreateResponse(HttpStatusCode.OK, matchingTags);
                    }

                    return this.Request.CreateResponse(HttpStatusCode.NoContent);
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }

            return this.Request.CreateResponse(HttpStatusCode.BadRequest);
        }

    }
}
