//
// Community Forums
// Copyright (c) 2013-2024
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
namespace DotNetNuke.Modules.ActiveForums
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Web.UI.WebControls;

    public partial class af_topicrating : ForumBase
    {
        public string RatingClass = "rating0";
        #region Private Members
        private int _Rating = -1;
        private bool _Enabled = false;
        #endregion
        #region Controls
        protected ImageButton Rate1 = new ImageButton();
        protected ImageButton Rate2 = new ImageButton();
        protected ImageButton Rate3 = new ImageButton();
        protected ImageButton Rate4 = new ImageButton();
        protected ImageButton Rate5 = new ImageButton();

        #endregion
        #region Public Properties
        public int Rating
        {
            get
            {
                return this._Rating;
            }

            set
            {
                this._Rating = value;
            }
        }

        public bool Enabled
        {
            get
            {
                return this._Enabled;
            }

            set
            {
                this._Enabled = value;
            }
        }
        #endregion
        #region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.Rate1.Click += this.Rate1_Click;
            this.Rate2.Click += this.Rate2_Click;
            this.Rate3.Click += this.Rate3_Click;
            this.Rate4.Click += this.Rate4_Click;
            this.Rate5.Click += this.Rate5_Click;
            this.cbRating.CallbackEvent += this.cbRating_Callback;

        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.RenderRating();
            string sRating = "function afchangerate(rate){var rd = document.getElementById('ratingdiv');rd.className=rate;};";
            this.Page.ClientScript.RegisterClientScriptBlock(this.Page.GetType(), "afratescript", sRating, true);
        }

        private void Rate1_Click(object sender, System.Web.UI.ImageClickEventArgs e)
        {
            new DotNetNuke.Modules.ActiveForums.Controllers.TopicRatingController().Rate(userId: this.UserId, topicId: this.TopicId, rating: 1, IpAddress: this.Request.UserHostAddress.ToString());
        }

        private void Rate2_Click(object sender, System.Web.UI.ImageClickEventArgs e)
        {
            new DotNetNuke.Modules.ActiveForums.Controllers.TopicRatingController().Rate(userId: this.UserId, topicId: this.TopicId, rating: 2, IpAddress: this.Request.UserHostAddress.ToString());
        }

        private void Rate3_Click(object sender, System.Web.UI.ImageClickEventArgs e)
        {
            new DotNetNuke.Modules.ActiveForums.Controllers.TopicRatingController().Rate(userId: this.UserId, topicId: this.TopicId, rating: 3, IpAddress: this.Request.UserHostAddress.ToString());
        }

        private void Rate4_Click(object sender, System.Web.UI.ImageClickEventArgs e)
        {
            new DotNetNuke.Modules.ActiveForums.Controllers.TopicRatingController().Rate(userId: this.UserId, topicId: this.TopicId, rating: 4, IpAddress: this.Request.UserHostAddress.ToString());
        }

        private void Rate5_Click(object sender, System.Web.UI.ImageClickEventArgs e)
        {
            new DotNetNuke.Modules.ActiveForums.Controllers.TopicRatingController().Rate(userId: this.UserId, topicId: this.TopicId, rating: 5, IpAddress: this.Request.UserHostAddress.ToString());
        }

        private void cbRating_Callback(object sender, Modules.ActiveForums.Controls.CallBackEventArgs e)
        {
            if (e.Parameters.Length > 0)
            {
                int rate = Convert.ToInt32(e.Parameter);
                if (rate >= 1 && rate <= 5)
                {
                    new DotNetNuke.Modules.ActiveForums.Controllers.TopicRatingController().Rate(userId: this.UserId, topicId: this.TopicId, rating: rate, IpAddress: this.Request.UserHostAddress.ToString());
                }
            }

            this.Rating = -1;
            this.RenderRating();
            this.plhRating.RenderControl(e.Output);
        }

        #endregion
        #region Private Methods
        private void RenderRating()
        {
            if (this.Rating == -1)
            {
                this.Rating = new DotNetNuke.Modules.ActiveForums.Controllers.TopicRatingController().Average(topicId: this.TopicId);
            }

            this.RatingClass = "rating" + this.Rating.ToString();
            this.plhRating.Controls.Clear();
            this.Rate1.Attributes.Add("onmouseover", "afchangerate('rating1');");
            this.Rate1.Enabled = this.Enabled;
            this.Rate2.Attributes.Add("onmouseover", "afchangerate('rating2');");
            this.Rate2.Enabled = this.Enabled;
            this.Rate3.Attributes.Add("onmouseover", "afchangerate('rating3');");
            this.Rate3.Enabled = this.Enabled;
            this.Rate4.Attributes.Add("onmouseover", "afchangerate('rating4');");
            this.Rate4.Enabled = this.Enabled;
            this.Rate5.Attributes.Add("onmouseover", "afchangerate('rating5');");
            this.Rate5.Enabled = this.Enabled;

            Literal lit = new Literal();
            lit.Text = "<div class=\"" + this.RatingClass + "\" id=\"ratingdiv\" onmouseout=\"this.className='" + this.RatingClass + "'\">";
            this.plhRating.Controls.Add(lit);
            this.Rate1.ID = "Rate1";
            this.Rate1.CausesValidation = false;
            this.Rate1.Width = 13;
            this.Rate1.Height = 14;
            this.Rate1.ImageUrl = "<% (DotNetNuke.Modules.ActiveForums.Globals.ModuleImagesPath) %>spacer.gif";
            this.plhRating.Controls.Add(this.Rate1);
            this.Rate2.ID = "Rate2";
            this.Rate2.CausesValidation = false;
            this.Rate2.Width = 14;
            this.Rate2.Height = 14;
            this.Rate2.ImageUrl = "<% (DotNetNuke.Modules.ActiveForums.Globals.ModuleImagesPath) %>spacer.gif";
            this.plhRating.Controls.Add(this.Rate2);
            this.Rate3.ID = "Rate3";
            this.Rate3.CausesValidation = false;
            this.Rate3.Width = 14;
            this.Rate3.Height = 14;
            this.Rate3.ImageUrl = "<% (DotNetNuke.Modules.ActiveForums.Globals.ModuleImagesPath) %>spacer.gif";
            this.plhRating.Controls.Add(this.Rate3);
            this.Rate4.ID = "Rate4";
            this.Rate4.CausesValidation = false;
            this.Rate4.Width = 14;
            this.Rate4.Height = 14;
            this.Rate4.ImageUrl = "<% (DotNetNuke.Modules.ActiveForums.Globals.ModuleImagesPath) %>spacer.gif";
            this.plhRating.Controls.Add(this.Rate4);
            this.Rate5.ID = "Rate5";
            this.Rate5.CausesValidation = false;
            this.Rate5.Width = 14;
            this.Rate5.Height = 14;
            this.Rate5.ImageUrl = "<% (DotNetNuke.Modules.ActiveForums.Globals.ModuleImagesPath) %>spacer.gif";

            this.plhRating.Controls.Add(this.Rate5);
            lit = new Literal();
            lit.Text = "</div>";
            this.plhRating.Controls.Add(lit);
            if (this.UseAjax)
            {
                this.Rate1.OnClientClick = "af_rateTopic(1);return false;";
                this.Rate2.OnClientClick = "af_rateTopic(2);return false;";
                this.Rate3.OnClientClick = "af_rateTopic(3);return false;";
                this.Rate4.OnClientClick = "af_rateTopic(4);return false;";
                this.Rate5.OnClientClick = "af_rateTopic(5);return false;";
                this.AddRatingScript();
            }
        }

        private void AddRatingScript()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("function af_rateTopic(rate){" + this.cbRating.ClientID + ".Callback(rate);};");
            this.Page.ClientScript.RegisterClientScriptBlock(this.Page.GetType(), "afrate", sb.ToString(), true);
        }
        #endregion

    }
}
