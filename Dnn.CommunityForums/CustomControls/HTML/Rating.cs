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

namespace DotNetNuke.Modules.ActiveForums.Controls
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Text;

    public class Ratings
    {
        public int ModuleId { get; set; } = -1;

        public int ForumId { get; set; } = -1;

        public int TopicId { get; set; } = -1;

        public bool Enabled { get; set; } = false;

        public int Rating { get; set; } = -1;

        public Ratings(int m, int f, int t, bool enable, int r)
        {
            this.ModuleId = m;
            this.ForumId = f;
            this.TopicId = t;
            this.Enabled = enable;
            this.Rating = r;
        }

        public string Render()
        {
            StringBuilder sb = new StringBuilder();
            if (this.Rating == -1)
            {
                this.Rating = new DotNetNuke.Modules.ActiveForums.Controllers.TopicRatingController().Average(topicId: this.TopicId);
            }

            if (this.Enabled)
            {
                sb.Append("<ul id=\"af-rater\" class=\"fa-rater ");
            }
            else
            {
                sb.Append("<ul class=\"fa-rater ");
            }

            if (this.Rating > 0)
            {
                sb.Append(" fa-rate" + this.Rating.ToString());
            }

            sb.Append("\">");
            if (this.Enabled)
            {
                sb.Append("<li onmouseover=\"amaf_hoverRate(this,1);\" onmouseout=\"amaf_hoverRate(this);\" onclick=\"amaf_ChangeTopicRating(" + this.ModuleId.ToString() + "," + this.ForumId.ToString() + "," + this.TopicId.ToString() + ",1);\"><i class=\"fa fa-star1\"></i></li>");
                sb.Append("<li onmouseover=\"amaf_hoverRate(this,2);\" onmouseout=\"amaf_hoverRate(this);\" onclick=\"amaf_ChangeTopicRating(" + this.ModuleId.ToString() + "," + this.ForumId.ToString() + "," + this.TopicId.ToString() + ",2);\"><i class=\"fa fa-star2\"></i></li>");
                sb.Append("<li onmouseover=\"amaf_hoverRate(this,3);\" onmouseout=\"amaf_hoverRate(this);\" onclick=\"amaf_ChangeTopicRating(" + this.ModuleId.ToString() + "," + this.ForumId.ToString() + "," + this.TopicId.ToString() + ",3);\"><i class=\"fa fa-star3\"></i></li>");
                sb.Append("<li onmouseover=\"amaf_hoverRate(this,4);\" onmouseout=\"amaf_hoverRate(this);\" onclick=\"amaf_ChangeTopicRating(" + this.ModuleId.ToString() + "," + this.ForumId.ToString() + "," + this.TopicId.ToString() + ",4);\"><i class=\"fa fa-star4\"></i></li>");
                sb.Append("<li onmouseover=\"amaf_hoverRate(this,5);\" onmouseout=\"amaf_hoverRate(this);\" onclick=\"amaf_ChangeTopicRating(" + this.ModuleId.ToString() + "," + this.ForumId.ToString() + "," + this.TopicId.ToString() + ",5);\"><i class=\"fa fa-star5\"></i></li>");
            }
            else
            {
                sb.Append("<li><i class=\"fa fa-star1\"></i></li>");
                sb.Append("<li><i class=\"fa fa-star2\"></i></li>");
                sb.Append("<li><i class=\"fa fa-star3\"></i></li>");
                sb.Append("<li><i class=\"fa fa-star4\"></i></li>");
                sb.Append("<li><i class=\"fa fa-star5\"></i></li>");
            }

            sb.Append("</ul>");
            if (this.Enabled)
            {
                sb.Append("<input type=\"hidden\" value=\"" + this.Rating.ToString() + "\" id=\"af-rate-value\" />");
            }

            return sb.ToString();
        }
    }
}
