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

    public partial class af_polledit : ForumBase
    {
        private string pollQuestion = string.Empty;
        private string pollType = string.Empty;
        private string pollOptions = string.Empty;

        public string PollQuestion
        {
            get
            {
                return this.pollQuestion;
            }

            set
            {
                this.pollQuestion = value;
            }
        }

        public string PollType
        {
            get
            {
                return this.pollType;
            }

            set
            {
                this.pollType = value;
            }
        }

        public string PollOptions
        {
            get
            {
                return this.pollOptions;
            }

            set
            {
                this.pollOptions = value;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!this.Page.IsPostBack)
            {
                this.txtPollQuestion.Text = this.PollQuestion;
                this.rdPollType.SelectedIndex = this.rdPollType.Items.IndexOf(this.rdPollType.Items.FindByValue(this.PollType));
                this.txtPollOptions.Text = this.PollOptions;
            }
        }
    }
}
