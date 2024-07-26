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
namespace DotNetNuke.Modules.ActiveForums.Controls
{
    using System;
    using System.ComponentModel;
    using System.Web.UI;

    [CLSCompliant(false), DefaultProperty("Text"), ToolboxData("<{0}:ImageButton runat=server></{0}:ImageButton>")]
    public class ImageButton : System.Web.UI.WebControls.WebControl, IPostBackEventHandler
    {
        public virtual string Text
        {
            get
            {
                object o = this.ViewState["Text"];
                if (o == null)
                {
                    return string.Empty;
                }

                return Convert.ToString(o);
            }

            set
            {
                this.ViewState["Text"] = value;
            }
        }

        private int hSpace;
        private int vSpace;
        private string imageAlign = "absmiddle";
        private bool postBack = true;
        private string confirmMessage = string.Empty;
        private string validationGroup = string.Empty;
        private string imageLocation = "LEFT";
        private string objectId = string.Empty;
        private string postBackScript;

        [Bindable(true), Category("Appearance"), DefaultValue("")]
        public string ImageUrl { get; set; }

        [Bindable(true), Category("Appearance"), DefaultValue("")]
        public string NavigateUrl { get; set; }

        [Bindable(true), Category("Appearance"), DefaultValue("absmiddle")]
        public string ImageAlign
        {
            get
            {
                return this.imageAlign;
            }

            set
            {
                this.imageAlign = value;
            }
        }

        [Bindable(true), Category("Appearance"), DefaultValue("")]
        public string HSpace
        {
            get
            {
                return this.hSpace.ToString();
            }

            set
            {
                this.hSpace = Convert.ToInt32(value);
            }
        }

        [Bindable(true), Category("Appearance"), DefaultValue("")]
        public string VSpace
        {
            get
            {
                return this.vSpace.ToString();
            }

            set
            {
                this.vSpace = Convert.ToInt32(value);
            }
        }

        [Bindable(true), Category("Appearance"), DefaultValue("")]
        public bool PostBack
        {
            get
            {
                return this.postBack;
            }

            set
            {
                this.postBack = value;
            }
        }

        [Bindable(true), Category("Appearance"), DefaultValue("")]
        public string ClientSideScript { get; set; }

        [Bindable(true), Category("Appearance"), DefaultValue("")]
        public string Params { get; set; }

        [Bindable(true), Category("Appearance"), DefaultValue("")]
        public bool Confirm { get; set; }

        [Bindable(true), Category("Appearance"), DefaultValue("")]
        public string ConfirmMessage
        {
            get
            {
                return this.confirmMessage;
            }

            set
            {
                this.confirmMessage = value;
            }
        }

        [Bindable(true), Category("Appearance"), DefaultValue("")]
        public bool EnableClientValidation { get; set; }

        [Bindable(true), Category("Appearance"), DefaultValue("")]
        public string ValidationGroup
        {
            get
            {
                return this.validationGroup;
            }

            set
            {
                this.validationGroup = value;
            }
        }

        [Bindable(true), Category("Appearance"), DefaultValue("")]
        public string ImageLocation
        {
            get
            {
                return this.imageLocation;
            }

            set
            {
                this.imageLocation = value;
            }
        }

        [Bindable(true), Category("Appearance"), DefaultValue("")]
        public string ObjectId
        {
            get
            {
                return this.objectId;
            }

            set
            {
                this.objectId = value;
            }
        }

        [Bindable(true), Category("Appearance"), DefaultValue("")]
        public string PostBackScript
        {
            get
            {
                return this.postBackScript;
            }
        }

        // Defines the Click event.
        public event EventHandler Click;

        // Invokes delegates registered with the Click event.
        protected virtual void OnClick(EventArgs e)
        {
            if (this.Click != null)
            {
                this.Click(this, e);
            }
        }

        public void RaisePostBackEvent(string eventArgument)
        {
            this.OnClick(new EventArgs());
        }

        protected override void Render(HtmlTextWriter writer)
        {
            string sConfirm = string.Empty;
            string sOnClick;
            if (this.CssClass == string.Empty)
            {
                this.CssClass = "amtoolbaritem";
            }

            var outerWriter = new HtmlTextWriter(writer);
            string sVoid = "javascript:void(0);";
            string sStatusOver = string.Empty;
            string sStatusOff = string.Empty;
            if (this.Confirm)
            {
                sConfirm = "if (confirm('" + this.ConfirmMessage + "')){ [FUNCTIONS] };";
            }

            sOnClick = this.ClientSideScript;
            if (this.Attributes["onclick"] != null)
            {
                sOnClick += this.Attributes["onclick"];
            }

            if (sConfirm != string.Empty)
            {
                sOnClick = sConfirm.Replace("[FUNCTIONS]", sOnClick);
            }

            if (this.EnableClientValidation)
            {
                sOnClick = "if (typeof(Page_ClientValidate) == 'function'){ if (Page_ClientValidate('" + this.ValidationGroup + "')){" + sOnClick + "};};";
            }

            string sPostBack = this.Page.ClientScript.GetPostBackEventReference(this, string.Empty);
            this.postBackScript = sPostBack;
            if (this.Enabled)
            {
                if (this.PostBack)
                {
                    if (!string.IsNullOrEmpty(sConfirm))
                    {
                        sPostBack = sConfirm.Replace("[FUNCTIONS]", sPostBack);
                    }

                    if (this.EnableClientValidation)
                    {
                        sPostBack = "if (typeof(Page_ClientValidate) == 'function'){ if (Page_ClientValidate('" + this.ValidationGroup + "')){" + sPostBack + "};};";
                    }

                    if (!string.IsNullOrEmpty(this.ClientSideScript))
                    {
                        sPostBack = this.ClientSideScript + sPostBack;
                    }

                    outerWriter.AddAttribute(HtmlTextWriterAttribute.Href, "javascript:" + sPostBack);
                }
                else if (!string.IsNullOrEmpty(this.NavigateUrl))
                {
                    outerWriter.AddAttribute(HtmlTextWriterAttribute.Href, this.Page.ResolveUrl(this.NavigateUrl));
                }

                if (!string.IsNullOrEmpty(sOnClick))
                {
                    if (!string.IsNullOrEmpty(this.ObjectId))
                    {
                        outerWriter.AddAttribute(HtmlTextWriterAttribute.Id, this.ObjectId);
                    }

                    outerWriter.AddAttribute(HtmlTextWriterAttribute.Href, sVoid);
                    outerWriter.AddAttribute(HtmlTextWriterAttribute.Onclick, sOnClick);
                }

                if (!string.IsNullOrEmpty(this.NavigateUrl))
                {
                    outerWriter.RenderBeginTag(HtmlTextWriterTag.A);
                }
            }

            if ((!string.IsNullOrEmpty(sOnClick) || !string.IsNullOrEmpty(sPostBack)) && string.IsNullOrEmpty(this.NavigateUrl))
            {
                if (!string.IsNullOrEmpty(this.ObjectId))
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ObjectId);
                }

                if (this.PostBack && this.Enabled)
                {
                    writer.AddAttribute("onclick", sPostBack);
                }
                else if (!string.IsNullOrEmpty(sOnClick) && this.Enabled)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Onclick, sOnClick);
                }
            }

            if (!this.Width.IsEmpty)
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Width, this.Width.ToString());
            }

            if (!this.Height.IsEmpty)
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Height, this.Height.ToString());
            }
            else if (string.IsNullOrEmpty(this.CssClass))
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.Height, "22px");
            }

            if (!string.IsNullOrEmpty(this.CssClass))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Class, this.CssClass);
            }

            if (this.Enabled)
            {
                writer.AddAttribute("onmouseover", "this.className='" + this.CssClass + "_over';");
                writer.AddAttribute("onmouseout", "this.className='" + this.CssClass + "';");
            }

            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            if (this.ImageLocation.ToUpper() == "LEFT")
            {
                var innerWriter = new HtmlTextWriter(writer);
                if (this.ImageUrl != string.Empty)
                {
                    var imageWriter = new HtmlTextWriter(innerWriter);
                    imageWriter.AddAttribute(HtmlTextWriterAttribute.Src, this.Page.ResolveUrl(this.ImageUrl));
                    imageWriter.AddAttribute("hspace", this.HSpace);
                    imageWriter.AddAttribute("vspace", this.VSpace);
                    imageWriter.AddAttribute("border", "0");

                    // If [PostBack] Then

                    // imageWriter.AddAttribute("onclick", sPostBack)
                    // End If
                    imageWriter.AddAttribute("align", this.ImageAlign);
                    imageWriter.RenderBeginTag(HtmlTextWriterTag.Img);
                    imageWriter.RenderEndTag();
                }

                innerWriter.Write("<span>" + this.Text + "</span>");
            }
            else if (this.ImageLocation.ToUpper() == "RIGHT")
            {
                var innerWriter = new HtmlTextWriter(writer);
                innerWriter.Write("<span>" + this.Text + "</span>");
                if (this.ImageUrl != string.Empty)
                {
                    var imageWriter = new HtmlTextWriter(innerWriter);
                    imageWriter.AddAttribute(HtmlTextWriterAttribute.Src, this.Page.ResolveUrl(this.ImageUrl));
                    imageWriter.AddAttribute("hspace", this.HSpace);
                    imageWriter.AddAttribute("vspace", this.VSpace);
                    imageWriter.AddAttribute("border", "0");

                    // If [PostBack] Then
                    //    imageWriter.AddAttribute("onclick", sPostBack)
                    // End If
                    imageWriter.AddAttribute("align", this.ImageAlign);
                    imageWriter.RenderBeginTag(HtmlTextWriterTag.Img);
                    imageWriter.RenderEndTag();
                }
            }
            else if (this.ImageLocation.ToUpper() == "TOP")
            {
                var innerWriter = new HtmlTextWriter(writer);
                if (this.ImageUrl != string.Empty)
                {
                    var imageWriter = new HtmlTextWriter(innerWriter);
                    imageWriter.AddAttribute(HtmlTextWriterAttribute.Src, this.Page.ResolveUrl(this.ImageUrl));
                    imageWriter.AddAttribute("hspace", this.HSpace);
                    imageWriter.AddAttribute("vspace", this.VSpace);
                    imageWriter.AddAttribute("border", "0");

                    // If [PostBack] Then
                    //    imageWriter.AddAttribute("onclick", sPostBack)
                    // End If
                    imageWriter.AddAttribute("align", this.ImageAlign);
                    imageWriter.RenderBeginTag(HtmlTextWriterTag.Img);
                    imageWriter.RenderEndTag();
                }

                innerWriter.Write("<br />");
                innerWriter.Write("<span>" + this.Text + "</span>");
            }

            // innerWriter.RenderEndTag()
            writer.RenderEndTag();
            if (!string.IsNullOrEmpty(this.NavigateUrl))
            {
                if (this.Enabled)
                {
                    outerWriter.RenderEndTag();
                }
            }
        }
    }
}
