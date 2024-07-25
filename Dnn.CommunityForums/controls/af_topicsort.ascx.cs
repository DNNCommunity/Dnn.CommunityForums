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
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke;

    public partial class af_topicsorter : ForumBase
    {
        private string _defaultSort = "ASC";

        public string DefaultSort
        {
            get
            {
                return this._defaultSort;
            }

            set
            {
                this._defaultSort = value;
            }
        }
        #region Controls
        #endregion

        #region Event Handlers
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                // Put user code to initialize the page here
                if (!this.Page.IsPostBack)
                {
                    string Sort = this.DefaultSort;
                    if (this.Request.Params[ParamKeys.Sort] != null)
                    {
                        Sort = this.Request.Params[ParamKeys.Sort];
                    }

                    this.drpSort.SelectedIndex = this.drpSort.Items.IndexOf(this.drpSort.Items.FindByValue(Sort));
                    this.drpSort.Items[0].Text = this.GetSharedResource(this.drpSort.Items[0].Text);
                    this.drpSort.Items[1].Text = this.GetSharedResource(this.drpSort.Items[1].Text);
                }

            }
            catch (Exception exc)
            {
                DotNetNuke.Services.Exceptions.Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
        #endregion

        #region  Web Form Designer Generated Code

        // This call is required by the Web Form Designer.
        [System.Diagnostics.DebuggerStepThrough()]
        private void InitializeComponent()
        {
        }

        // NOTE: The following placeholder declaration is required by the Web Form Designer.
        // Do not delete or move it.
        private object designerPlaceholderDeclaration;

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // CODEGEN: This method call is required by the Web Form Designer
            // Do not modify it using the code editor.
            this.LocalResourceFile = Globals.SharedResourceFile;
            this.InitializeComponent();

            this.drpSort.SelectedIndexChanged += new System.EventHandler(this.drpSort_SelectedIndexChanged);

        }

        #endregion

        private void drpSort_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            var sort = this.drpSort.SelectedItem.Value;
            var dest = DotNetNuke.Modules.ActiveForums.Utilities.NavigateURL(this.TabId, "",
                                                             new[]
                                                                 {
                                                                     ParamKeys.ViewType + "=" + Views.Topic,
                                                                     ParamKeys.ForumId + "=" + this.ForumId,
                                                                     ParamKeys.TopicId + "=" + this.TopicId,
                                                                     ParamKeys.Sort + "=" + sort
                                                                 });
            this.Response.Redirect(dest);
        }
    }

}
