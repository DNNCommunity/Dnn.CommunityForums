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

namespace DotNetNuke.Modules.ActiveForums
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    internal static class MiniPager
    {
        public static string GetMiniPager(DataRow currentRow, int tabId, int socialGroupId, int pageSize)
        {
            if (currentRow == null)
            {
                return null;
            }

            var replyCount = Convert.ToInt32(currentRow["ReplyCount"]);
            var pageCount = Convert.ToInt32(Math.Ceiling((double)(replyCount + 1) / pageSize));
            var forumId = currentRow["ForumId"].ToString();
            var topicId = currentRow["TopicId"].ToString();

            // No pager if there is only one page.
            if (pageCount <= 1)
            {
                return null;
            }

            List<string> @params;

            var result = string.Empty;

            if (pageCount <= 5)
            {
                for (var i = 1; i <= pageCount; i++)
                {
                    @params = new List<string> { ParamKeys.ForumId + "=" + forumId, ParamKeys.TopicId + "=" + topicId, ParamKeys.ViewType + "=" + Views.Topic, ParamKeys.PageId + "=" + i };

                    if (socialGroupId > 0)
                    {
                        @params.Add("GroupId=" + socialGroupId.ToString());
                    }

                    result += "<a href=\"" + Utilities.NavigateURL(tabId, string.Empty, @params.ToArray()) + "\">" + i + "</a>";
                }

                return result;
            }

            // 1 2 3 ... N
            for (var i = 1; i <= 3; i++)
            {
                @params = new List<string> { ParamKeys.ForumId + "=" + forumId, ParamKeys.TopicId + "=" + topicId, ParamKeys.ViewType + "=" + Views.Topic, ParamKeys.PageId + "=" + i };
                if (socialGroupId > 0)
                {
                    @params.Add("GroupId=" + socialGroupId.ToString());
                }

                result += "<a href=\"" + Utilities.NavigateURL(tabId, string.Empty, @params.ToArray()) + "\">" + i + "</a>";
            }

            result += "<span>...</span>";

            @params = new List<string> { ParamKeys.ForumId + "=" + forumId, ParamKeys.TopicId + "=" + topicId, ParamKeys.ViewType + "=" + Views.Topic, ParamKeys.PageId + "=" + pageCount };
            if (socialGroupId > 0)
            {
                @params.Add("GroupId=" + socialGroupId.ToString());
            }

            result += "<a href=\"" + Utilities.NavigateURL(tabId, string.Empty, @params.ToArray()) + "\">" + pageCount + "</a>";

            return result;
        }
    }
}
