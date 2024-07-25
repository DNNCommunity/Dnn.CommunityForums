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
