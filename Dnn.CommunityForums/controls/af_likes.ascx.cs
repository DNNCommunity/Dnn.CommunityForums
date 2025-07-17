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
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Modules.ActiveForums.Controls;

    public partial class af_likes : ForumBase
    {
        protected System.Web.UI.WebControls.Repeater rptLikes = new Repeater();
        protected System.Web.UI.WebControls.Panel pnlMessage = new Panel();
        protected System.Web.UI.WebControls.Literal litMessage = new Literal();
        protected DotNetNuke.Modules.ActiveForums.Controls.PagerNav PagerTop = new PagerNav();
        protected DotNetNuke.Modules.ActiveForums.Controls.PagerNav PagerBottom = new PagerNav();

        private Control ctl;
        private int? contentId;

        private List<string> parameters;

        private int pageSize = 20;
        private DotNetNuke.Modules.ActiveForums.Entities.IPostInfo post;
        private int rowCount;
        private int rowIndex;

        private int ContentId
        {
            get
            {
                if (!this.contentId.HasValue)
                {
                    int parsedContentId;
                    this.contentId = int.TryParse(this.Request.Params[ParamKeys.ContentId], out parsedContentId) ? parsedContentId : 0;
                }

                return this.contentId.Value;
            }
        }

        private List<string> Parameters
        {
            get
            {
                if (this.parameters == null)
                {
                    this.parameters = new List<string>();

                    if (this.ContentId > 0)
                    {
                        this.parameters.Add($"{ParamKeys.ContentId}={this.ContentId}");
                    }
                }

                return this.parameters;
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.post = new DotNetNuke.Modules.ActiveForums.Controllers.ContentController().GetById(this.ContentId, this.ForumModuleId).Post;
            string template = DotNetNuke.Modules.ActiveForums.Controllers.TemplateController.Template_Get(this.ForumModuleId, Enums.TemplateType.Likes, this.post.Forum.FeatureSettings.TemplateFileNameSuffix ?? SettingsBase.GetModuleSettings(this.ForumModuleId).ForumFeatureSettings.TemplateFileNameSuffix);

            try
            {
                template = Globals.ForumsControlsRegisterAMTag + template;
                template = Utilities.LocalizeControl(template);
                this.ctl = this.ParseControl(template);
                this.LinkControls(this.ctl.Controls);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                throw;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.rptLikes.ItemDataBound += this.LikeRepeaterOnItemDataBound;

            try
            {
                if (this.Request.QueryString[Literals.GroupId] != null && Utilities.IsNumeric(this.Request.QueryString[Literals.GroupId]))
                {
                    this.SocialGroupId = Convert.ToInt32(this.Request.QueryString[Literals.GroupId]);
                }

                this.BindLikes();
                foreach (Control control in this.ctl.Controls)
                {
                    string template = string.Empty;
                    try
                    {
                        if (control.GetType().FullName == "System.Web.UI.LiteralControl")
                        {
                            template = ((System.Web.UI.LiteralControl)control).Text;
                        }
                        else if (control.GetType().FullName == "System.Web.UI.HtmlControls.HtmlGenericControl")
                        {
                            template = ((System.Web.UI.HtmlControls.HtmlGenericControl)control).InnerText;
                        }
                    }
                    catch (Exception ex)
                    {
                        Exceptions.LogException(ex);
                    }

                    if (!string.IsNullOrEmpty(template) && template.Contains("["))
                    {
                        if (this.post != null)
                        {
                            template = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.ReplacePostTokens(new StringBuilder(template), this.post, this.PortalSettings, this.MainSettings, new Services.URLNavigator().NavigationManager(), this.ForumUser, this.Request.Url, this.Request.RawUrl).ToString();
                            if (control.GetType().FullName == "System.Web.UI.LiteralControl")
                            {
                                ((System.Web.UI.LiteralControl)control).Text = template;
                            }
                            else if (control.GetType().FullName == "System.Web.UI.HtmlControls.HtmlGenericControl")
                            {
                                ((System.Web.UI.HtmlControls.HtmlGenericControl)control).InnerText = template;
                            }
                        }
                    }
                }

                this.Likes.Controls.Clear();
                this.Likes.Controls.Add(this.ctl);

                // Update Meta Data
                var tempVar = this.BasePage;
                Environment.UpdateMeta(ref tempVar, "[VALUE] - " + this.GetSharedResource("[RESX:LIKES]") + " - " + this.ContentId, "[VALUE]", "[VALUE]");
            }
            catch (Exception ex)
            {
                this.Controls.Clear();
                this.RenderMessage("[RESX:ERROR]", "[RESX:ERROR]", ex.Message, ex);
            }
        }

        private void LikeRepeaterOnItemDataBound(object sender, RepeaterItemEventArgs repeaterItemEventArgs)
        {
            if (repeaterItemEventArgs.Item.ItemType == ListItemType.Header || repeaterItemEventArgs.Item.ItemType == ListItemType.Footer)
            {
                this.post = new DotNetNuke.Modules.ActiveForums.Controllers.ContentController().GetById(this.ContentId, this.ForumModuleId).Post;

                foreach (Control control in repeaterItemEventArgs.Item.Controls)
                {
                    string itemTemplate = string.Empty;
                    try
                    {
                        if (control.GetType().FullName == "System.Web.UI.LiteralControl")
                        {
                            itemTemplate = ((System.Web.UI.LiteralControl)control).Text;
                        }
                        else if (control.GetType().FullName == "System.Web.UI.HtmlControls.HtmlGenericControl")
                        {
                            itemTemplate = ((System.Web.UI.HtmlControls.HtmlGenericControl)control).InnerText;
                        }
                    }
                    catch (Exception ex)
                    {
                        Exceptions.LogException(ex);
                    }

                    if (!string.IsNullOrEmpty(itemTemplate) && itemTemplate.Contains("["))
                    {
                        if (this.post != null)
                        {

                            itemTemplate = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.ReplacePostTokens(new StringBuilder(itemTemplate), this.post, this.PortalSettings, this.MainSettings, new Services.URLNavigator().NavigationManager(), this.ForumUser, this.Request.Url, this.Request.RawUrl).ToString();

                            if (control.GetType().FullName == "System.Web.UI.LiteralControl")
                            {
                                ((System.Web.UI.LiteralControl)control).Text = itemTemplate;
                            }
                            else if (control.GetType().FullName == "System.Web.UI.HtmlControls.HtmlGenericControl")
                            {
                                ((System.Web.UI.HtmlControls.HtmlGenericControl)control).InnerText = itemTemplate;
                            }
                        }
                    }
                }
            }
            else if (repeaterItemEventArgs.Item.ItemType == ListItemType.Item || repeaterItemEventArgs.Item.ItemType == ListItemType.AlternatingItem)
            {
                try
                {
                    var like = (DotNetNuke.Modules.ActiveForums.Entities.LikeInfo)repeaterItemEventArgs.Item.DataItem;

                    foreach (Control control in repeaterItemEventArgs.Item.Controls)
                    {
                        string itemTemplate = string.Empty;
                        try
                        {
                            if (control.GetType().FullName == "System.Web.UI.LiteralControl")
                            {
                                itemTemplate = ((System.Web.UI.LiteralControl)control).Text;
                            }
                            else if (control.GetType().FullName == "System.Web.UI.HtmlControls.HtmlGenericControl")
                            {
                                itemTemplate = ((System.Web.UI.HtmlControls.HtmlGenericControl)control).InnerText;
                            }
                        }
                        catch (Exception ex)
                        {
                            Exceptions.LogException(ex);
                        }

                        if (!string.IsNullOrEmpty(itemTemplate) && itemTemplate.Contains("["))
                        {
                            if (like != null)
                            {
                                itemTemplate = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.ReplaceLikeTokens(new StringBuilder(itemTemplate), like, this.PortalSettings, this.MainSettings, new Services.URLNavigator().NavigationManager(), this.ForumUser, this.Request.Url, this.Request.RawUrl).ToString();
                                if (control.GetType().FullName == "System.Web.UI.LiteralControl")
                                {
                                    ((System.Web.UI.LiteralControl)control).Text = itemTemplate;
                                }
                                else if (control.GetType().FullName == "System.Web.UI.HtmlControls.HtmlGenericControl")
                                {
                                    ((System.Web.UI.HtmlControls.HtmlGenericControl)control).InnerText = itemTemplate;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Exceptions.LogException(ex);
                }
            }
        }

        private void BindLikes()
        {
            this.pageSize = (this.UserId > 0) ? this.UserDefaultPageSize : this.MainSettings.PageSize;

            if (this.pageSize < 5)
            {
                this.pageSize = 10;
            }

            this.rowIndex = (this.PageId - 1) * this.pageSize;

            // If we don't have a contentId
            if (this.ContentId <= 0)
            {
                return;
            }

            this.rowCount = new DotNetNuke.Modules.ActiveForums.Controllers.LikeController(this.PortalId, this.ForumModuleId).Count(this.ContentId);
            var likes = new DotNetNuke.Modules.ActiveForums.Controllers.LikeController(this.PortalId, this.ForumModuleId).GetForPost(this.ContentId).Skip(this.rowIndex).Take(this.pageSize).ToList();

            this.parameters = null; // We reset this so we make sure to get an updated version

            if (likes.Count > 0)
            {
                var rptResults = this.rptLikes;

                this.pnlMessage.Visible = false;

                try
                {
                    rptResults.Visible = true;
                    rptResults.DataSource = likes;
                    rptResults.DataBind();
                    this.BuildPager(this.PagerTop);
                    this.BuildPager(this.PagerBottom);
                }
                catch (Exception ex)
                {
                    this.litMessage.Text = ex.Message;
                    this.pnlMessage.Visible = true;
                    rptResults.Visible = false;
                }
            }
            else
            {
                this.litMessage.Text = this.GetSharedResource("[RESX:SearchNoResults]");
                this.pnlMessage.Visible = true;
            }
        }

        private void LinkControls(ControlCollection ctrls)
        {
            foreach (Control ctrl in ctrls)
            {
                switch (ctrl.ID)
                {
                    case "litMessage":
                        this.litMessage = (Literal)ctrl;
                        break;
                    case "rptLikes":
                        this.rptLikes = (Repeater)ctrl;
                        break;
                    case "PagerTop":
                        this.PagerTop = (PagerNav)ctrl;
                        break;
                    case "PagerBottom":
                        this.PagerBottom = (PagerNav)ctrl;
                        break;
                }

                if (ctrl is Controls.ControlsBase)
                {
                    ((Controls.ControlsBase)ctrl).ControlConfig = this.ControlConfig;
                }

                if (ctrl.Controls.Count > 0)
                {
                    this.LinkControls(ctrl.Controls);
                }
            }
        }

        private void BuildPager(PagerNav pager)
        {
            var intPages = Convert.ToInt32(Math.Ceiling(this.rowCount / (double)this.pageSize));

            pager.PageCount = intPages;
            pager.CurrentPage = this.PageId;
            pager.TabID = this.TabId;
            pager.ForumID = this.ForumId;
            pager.UseShortUrls = this.MainSettings.UseShortUrls;
            pager.ContentId = this.ContentId;
            pager.PageText = Utilities.GetSharedResource("[RESX:Page]");
            pager.OfText = Utilities.GetSharedResource("[RESX:PageOf]");
            pager.View = Views.Likes;
            pager.PageMode = PagerNav.Mode.Links;
            pager.BaseURL = URL.ForumLink(this.TabId, this.post.Forum) + this.post.Topic.TopicUrl + "/" + this.MainSettings.PrefixURLLikes + "/" + this.ContentId;

            pager.Params = this.Parameters.ToArray();
        }
    }
}
