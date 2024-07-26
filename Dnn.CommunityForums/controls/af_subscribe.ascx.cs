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

    public partial class af_subscribe : ForumBase
    {
        #region Public Members
        public int mode = 1; // 0 = Forum 1 = Topic
        public bool IsSubscribed = false;
        #endregion
        #region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.cbSubscribe.CallbackEvent += this.cbSubscribe_Callback;
            this.chkSubscribe.CheckedChanged += new System.EventHandler(this.chkSubscribe_CheckedChanged);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            string subscribeText = this.GetSharedResource("[RESX:Subscribe]");
            this.chkSubscribe.Text = subscribeText;
            this.chkSubscribe.Checked = this.IsSubscribed;
            if (this.UseAjax)
            {
                this.chkSubscribe.Attributes.Add("onclick", "af_toggleSub();");
                this.AddToggleScript();
            }
            else
            {
                this.chkSubscribe.AutoPostBack = true;
            }
        }

        private void cbSubscribe_Callback(object sender, Modules.ActiveForums.Controls.CallBackEventArgs e)
        {
            this.ToggleSubscribe();
            this.chkSubscribe.RenderControl(e.Output);
        }

        private void chkSubscribe_CheckedChanged(object sender, System.EventArgs e)
        {
            this.ToggleSubscribe();
        }

        #endregion
        #region Private Methods
        private void AddToggleScript()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("function af_toggleSub(){" + this.cbSubscribe.ClientID + ".Callback();};");
            this.Page.ClientScript.RegisterClientScriptBlock(this.Page.GetType(), "afsubscribe", sb.ToString(), true);
        }

        private void ToggleSubscribe()
        {
            int iStatus = 0;
            SubscriptionController sc = new SubscriptionController();
            iStatus = sc.Subscription_Update(this.PortalId, this.ModuleId, this.ForumId, this.TopicId, 1, this.UserId, this.ForumUser.UserRoles);
            if (iStatus == 1)
            {
                this.IsSubscribed = true;
            }
            else
            {
                this.IsSubscribed = false;
            }

            this.chkSubscribe.Checked = this.IsSubscribed;
            this.chkSubscribe.Text = this.GetSharedResource("[RESX:Subscribe]");
        }
        #endregion
    }
}
