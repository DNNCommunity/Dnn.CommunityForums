// Copyright (c) 2013-2024 by DNN Community
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
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Web.UI.WebControls;

    public partial class af_topicstatus : ForumBase
    {
        private int status = -1;
        private bool autoPostBack = true;

        public int Status
        {
            get
            {
                return this.status;
            }

            set
            {
                this.status = value;
            }
        }

        public bool AutoPostBack
        {
            get
            {
                return this.autoPostBack;
            }

            set
            {
                this.autoPostBack = value;
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.drpStatus.SelectedIndexChanged += new System.EventHandler(this.drpStatus_SelectedIndexChanged);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.drpStatus.AutoPostBack = this.AutoPostBack;
            foreach (ListItem li in this.drpStatus.Items)
            {
                li.Text = Utilities.GetSharedResource(li.Text);
            }

            if (!this.Page.IsPostBack)
            {
                this.drpStatus.SelectedIndex = this.drpStatus.Items.IndexOf(this.drpStatus.Items.FindByValue(this.Status.ToString()));
            }
        }

        private void drpStatus_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (this.AutoPostBack == true)
            {
                int intStatus = 0;
                intStatus = Convert.ToInt32(this.drpStatus.SelectedItem.Value);
                if (intStatus >= -1 && intStatus <= 3)
                {
                    DataProvider.Instance().Topics_UpdateStatus(this.PortalId, this.ModuleId, this.TopicId, -1, intStatus, -1, this.UserId);
                }

                this.Response.Redirect(this.Request.RawUrl);
            }
        }
    }
}
