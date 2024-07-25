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
    using System.Web.UI;
    using System.Web.UI.WebControls;

    public partial class af_pollvote : ForumBase
    {
        private int PollId = -1;
        private string PollType = "S";

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.btnVote.Click += new System.EventHandler(this.btnVote_Click);

            if (this.TopicId > 0)
            {
                this.BindPoll();
            }
        }

        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            System.IO.StringWriter stringWriter = new System.IO.StringWriter();
            HtmlTextWriter htmlWriter = new HtmlTextWriter(stringWriter);
            base.Render(htmlWriter);
            string html = stringWriter.ToString();
            html = Utilities.LocalizeControl(html);
            writer.Write(html);
        }

        private void BindPoll()
        {

            try
            {
                DataSet ds = DataProvider.Instance().Poll_Get(this.TopicId);
                if (ds.Tables.Count > 0)
                {
                    DataTable dtPoll = ds.Tables[0];
                    DataTable dtOptions = ds.Tables[1];
                    if (dtPoll.Rows.Count > 0)
                    {
                        this.lblQuestion.Text = dtPoll.Rows[0]["Question"].ToString();
                        this.PollType = dtPoll.Rows[0]["PollType"].ToString();
                        this.PollId = Convert.ToInt32(dtPoll.Rows[0]["PollId"]);
                        if (this.PollType == "S")
                        {
                            this.rdbtnOptions.DataTextField = "OptionName";
                            this.rdbtnOptions.DataValueField = "PollOptionsID";
                            this.rdbtnOptions.DataSource = dtOptions;
                            this.rdbtnOptions.DataBind();
                            this.rdbtnOptions.Visible = true;
                            this.cblstOptions.Visible = false;
                        }
                        else
                        {
                            this.cblstOptions.DataTextField = "OptionName";
                            this.cblstOptions.DataValueField = "PollOptionsID";
                            this.cblstOptions.DataSource = dtOptions;
                            this.cblstOptions.DataBind();
                            this.rdbtnOptions.Visible = false;
                            this.cblstOptions.Visible = true;
                        }

                    }

                }
            }
            catch (Exception ex)
            {
            }
        }

        private void btnVote_Click(object sender, System.EventArgs e)
        {
            try
            {
                int optionId = -1;
                if (this.rdbtnOptions.Visible == true)
                {
                    if (this.rdbtnOptions.SelectedIndex > -1)
                    {
                        optionId = Convert.ToInt32(this.rdbtnOptions.SelectedItem.Value);
                    }

                    if (this.PollId > 0 & optionId > 0)
                    {
                        DataProvider.Instance().Poll_Vote(this.PollId, optionId, string.Empty, this.Request.UserHostAddress, this.UserId);
                    }
                }
                else if (this.cblstOptions.Visible == true)
                {
                    if (this.cblstOptions.SelectedIndex > -1)
                    {
                        foreach (ListItem item in this.cblstOptions.Items)
                        {
                            if (item.Selected)
                            {
                                optionId = Convert.ToInt32(item.Value);
                                DataProvider.Instance().Poll_Vote(this.PollId, optionId, string.Empty, this.Request.UserHostAddress, this.UserId);
                            }
                        }
                    }
                }

                this.Response.Redirect(this.Request.RawUrl);

            }
            catch (Exception ex)
            {

            }
        }
    }
}
