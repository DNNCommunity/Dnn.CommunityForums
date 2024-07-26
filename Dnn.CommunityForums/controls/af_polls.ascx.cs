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

    public partial class af_polls : ForumBase
    {
        private int pollId = -1;

        public int PollId
        {
            get
            {
                return this.pollId;
            }

            set
            {
                this.pollId = value;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (this.TopicId > 0)
            {
                try
                {
                    Polls polls = new Polls();
                    bool showResults = false;
                    if (this.UserId > 0)
                    {
                        if (polls.HasVoted(this.TopicId, this.UserId))
                        {
                            showResults = true;
                        }
                    }
                    else
                    {
                        showResults = true;
                    }

                    if (showResults)
                    {
                        Literal lit = new Literal();
                        lit.Text = polls.PollResults(this.TopicId, this.ImagePath);
                        this.Controls.Add(lit);
                    }
                    else
                    {
                        // Show Questions
                        ForumBase ctl = (ForumBase)this.LoadControl(this.Page.ResolveUrl(Globals.ModulePath + "controls/af_pollvote.ascx"));
                        ctl.ModuleConfiguration = this.ModuleConfiguration;
                        ctl.ForumId = this.ForumId;
                        ctl.TopicId = this.TopicId;
                        this.Controls.Add(ctl);
                    }
                }
                catch (Exception ex)
                {
                }
            }
        }
    }
}
