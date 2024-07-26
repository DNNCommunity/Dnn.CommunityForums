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
    using System.Collections.Generic;
    using System.Web;

    public partial class af_searchquick : ForumBase
    {
        public int MID;
        public int FID;
        public int TID;

        #region Event Handlers

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                this.ForumModuleId = this.MID;
                if (this.ForumId < 1)
                {
                    this.ForumId = this.FID;
                }

                if (this.ForumTabId < 1)
                {
                    this.ForumTabId = this.TID;
                }

                if (this.Request.QueryString["GroupId"] != null && SimulateIsNumeric.IsNumeric(this.Request.QueryString["GroupId"]))
                {
                    this.SocialGroupId = Convert.ToInt32(this.Request.QueryString["GroupId"]);
                }

                // Put user code to initialize the page here
                this.txtSearch.Attributes.Add("onkeydown", "if(event.keyCode == 13){document.getElementById('" + this.lnkSearch.ClientID + "').click();}");
            }
            catch (Exception exc)
            {
                DotNetNuke.Services.Exceptions.Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        #endregion

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.LocalResourceFile = Globals.SharedResourceFile;

            this.lnkSearch.Click += this.lnkSearch_Click;
        }

        private void lnkSearch_Click(object sender, EventArgs e)
        {
            if (this.txtSearch.Text.Trim() != string.Empty)
            {
                var @params = new List<string>
                {
                    $"{ParamKeys.ViewType}={Views.Search}",
                    $"{ParamKeys.ForumId}={this.ForumId}",
                    $"{SearchParamKeys.Query}={HttpUtility.UrlEncode(this.txtSearch.Text.Trim())}",
                };

                if (this.SocialGroupId > 0)
                {
                    @params.Add($"{Literals.GroupId}={this.SocialGroupId}");
                }

                this.Response.Redirect(this.NavigateUrl(this.ForumTabId, string.Empty, @params.ToArray()));
            }
        }
    }
}
