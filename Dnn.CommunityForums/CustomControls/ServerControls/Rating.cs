﻿// Copyright (c) by DNN Community
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
    using System.Text;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    [ToolboxData("<{0}:Rating runat=server></{0}:Rating>")]
    public class Rating : WebControl
    {
        private double rating = 0;
        private string ratingCSS = "afrate";
        private int topicId = -1;
        private int userId = -1;
        protected Callback cb = new Callback();

        public double RatingValue
        {
            get
            {
                return this.rating;
            }

            set
            {
                this.rating = value;
            }
        }

        public string RatingCSS
        {
            get
            {
                return this.ratingCSS;
            }

            set
            {
                this.ratingCSS = value;
            }
        }

        public int TopicId
        {
            get
            {
                return this.topicId;
            }

            set
            {
                this.topicId = value;
            }
        }

        public int UserId
        {
            get
            {
                return this.userId;
            }

            set
            {
                this.userId = value;
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.cb.CallbackEvent += new Callback.CallbackEventHandler(this.cb_Callback);

            if (this.Enabled)
            {
                this.cb.ID = "cb";
                this.Controls.Add(this.cb);
            }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            Literal lit = new Literal();
            lit.Text = this.RenderRating();
            if (this.Enabled)
            {
                CallBackContent cbContent = new CallBackContent();
                cbContent.Controls.Add(lit);
                this.cb.Content = cbContent;

                this.cb.RenderControl(writer);
            }
            else
            {
                lit.RenderControl(writer);
            }
        }

        private string RenderRating()
        {
            if (this.RatingValue > 0)
            {
                if (Math.Round(this.RatingValue, 0) == 1)
                {
                    this.RatingCSS += " onepos";
                }
                else if (Math.Round(this.RatingValue, 0) == 2)
                {
                    this.RatingCSS += " twopos";
                }
                else if (Math.Round(this.RatingValue, 0) == 3)
                {
                    this.RatingCSS += " threepos";
                }
                else if (Math.Round(this.RatingValue, 0) == 4)
                {
                    this.RatingCSS += " fourpos";
                }
                else if (Math.Round(this.RatingValue, 0) == 5)
                {
                    this.RatingCSS += " fivepos";
                }
            }

            if (this.RatingValue == 0 && this.Enabled == false)
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("<div class=\"" + this.CssClass + "\">");
            sb.Append("<ul class=\"" + this.RatingCSS + "\">");
            if (this.Enabled)
            {
                sb.Append("<li class=\"one\"><a title=\"[RESX:Rate:1Star]\" href=\"#\" onclick=\"" + this.cb.ClientID + ".Callback(1); return false;\">1</a></li>");
                sb.Append("<li class=\"two\"><a title=\"[RESX:Rate:2Star]\" href=\"#\" onclick=\"" + this.cb.ClientID + ".Callback(2); return false;\">2</a></li>");
                sb.Append("<li class=\"three\"><a title=\"[RESX:Rate:3Star]\" href=\"#\" onclick=\"" + this.cb.ClientID + ".Callback(3); return false;\">3</a></li>");
                sb.Append("<li class=\"four\"><a title=\"[RESX:Rate:4Star]\" href=\"#\" onclick=\"" + this.cb.ClientID + ".Callback(4); return false;\">4</a></li>");
                sb.Append("<li class=\"five\"><a title=\"[RESX:Rate:5Star]\" href=\"#\" onclick=\"" + this.cb.ClientID + ".Callback(5); return false;\">5</a></li>");
            }
            else
            {
                sb.Append("<li class=\"one\">1</li>");
                sb.Append("<li class=\"two\">2</li>");
                sb.Append("<li class=\"three\">3</li>");
                sb.Append("<li class=\"four\">4</li>");
                sb.Append("<li class=\"five\">5</li>");
            }

            sb.Append("</ul>");
            sb.Append("</div>");
            return sb.ToString();
        }

        private void cb_Callback(object sender, CallBackEventArgs e)
        {
            Data.Topics db = new Data.Topics();
            if (e.Parameters.Length > 0)
            {
                int rate = Convert.ToInt32(e.Parameter);
                if (rate >= 1 && rate <= 5)
                {
                    this.RatingValue = new DotNetNuke.Modules.ActiveForums.Controllers.TopicRatingController().Rate(userId: this.UserId, topicId: this.TopicId, rating: rate, ipAddress: HttpContext.Current.Request.UserHostAddress.ToString());
                }
            }

            CallBackContent cbContent = new CallBackContent();
            cbContent.Controls.Add(new LiteralControl(this.RenderRating()));
            this.cb.Content = cbContent;
            this.cb.Content.RenderControl(e.Output);
        }
    }
}
