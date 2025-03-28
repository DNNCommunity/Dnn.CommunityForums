﻿// Copyright (c) by DNN Community
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

namespace DotNetNuke.Modules.ActiveForums.Controls
{
    using System;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    [DefaultProperty("Text"), ToolboxData("<{0}:Modal runat=server></{0}:Modal>")]
    public class Modal : CompositeControl
    {
        protected Controls.Callback cb = new Controls.Callback();
        protected PlaceHolder plh = new PlaceHolder();
        private string callBackOnComplete = string.Empty;

        [Bindable(true), Category("Appearance"), DefaultValue(""), Localizable(true)]
        public string Text
        {
            get
            {
                string s = Convert.ToString(this.ViewState["Text"]);
                if (s == null)
                {
                    return string.Empty;
                }
                else
                {
                    return s;
                }
            }

            set
            {
                this.ViewState["Text"] = value;
            }
        }

        public string CallBackOnComplete
        {
            get
            {
                return this.callBackOnComplete;
            }

            set
            {
                this.callBackOnComplete = value;
            }
        }

        public PlaceHolder ModalContent
        {
            get
            {
                this.EnsureChildControls();
                if (this.plh == null)
                {
                    this.plh = new PlaceHolder();
                }

                return this.plh;
            }

            set
            {
                this.plh = value;
            }
        }

        protected override void CreateChildControls()
        {
            this.cb = new Callback();
            this.cb.ID = "CB_" + this.ClientID;
            if (HttpContext.Current.Request.Params["amtsdebug"] == "true" || HttpContext.Current.Request.Params["amdebug"] == "true")
            {
                this.cb.Debug = true;
            }
            else
            {
                this.cb.Debug = false;
            }

            if (!(this.CallBackOnComplete == string.Empty))
            {
                this.cb.OnCallbackComplete = this.CallBackOnComplete;
            }

            Controls.CallBackContent cnt = new Controls.CallBackContent();
            cnt.Controls.Add(this.ModalContent);
            this.cb.Content = cnt;
            this.Controls.Add(this.cb);
            this.ChildControlsCreated = true;
        }

        protected override void RenderChildren(System.Web.UI.HtmlTextWriter writer)
        {
            foreach (Control childControl in this.Controls)
            {
                childControl.RenderControl(writer);
            }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            // BEGIN MASK
            writer.AddAttribute("id", "amModalMask");
            writer.AddAttribute("style", "display:none;width:100%;background-color:Gray;top:0;left:0;position:absolute;opacity: 0.6;-moz-opacity: 0.6;filter: alpha(opacity=60);");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            writer.RenderEndTag();

            // END MASK
            // BEGIN MAIN DIV
            // <div id="amModal" class="amModal" style="display:none;position:absolute;">
            writer.AddAttribute("id", "amModal");
            writer.AddAttribute("class", "amModal");
            writer.AddAttribute("style", "display:none;position:absolute;");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            // BEGIN HEADER
            // <div class="amModalHeader">
            writer.AddAttribute("class", "amModalHeader");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            // BEGIN HEADER TEXT
            //    <div class="amModalHeaderText" id="amModalHeaderText"></div>
            writer.AddAttribute("class", "amModalHeaderText");
            writer.AddAttribute("id", "amModalHeaderText");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            writer.RenderEndTag();

            // END HEADER TEXT
            //       BEGIN CLOSE BUTTON
            //    <div class="amModalCloseImg" onclick="amaf_closeDialog();"><img src="<%=Page.ResolveUrl("~/DesktopModules/ActiveForums/images/close.gif") %>" alt="[RESX:Close]" /></div>
            writer.AddAttribute("class", "amModalCloseImg");
            writer.AddAttribute("onclick", "amaf_closeDialog();");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            writer.AddAttribute("src", this.Page.ResolveUrl("~/DesktopModules/ActiveForums/images/close.gif"));
            writer.AddAttribute("alt", "[RESX:Close]");
            writer.RenderBeginTag(HtmlTextWriterTag.Img);
            writer.RenderEndTag(); // Close image
            writer.RenderEndTag(); // Close div

            // END Close Button
            writer.RenderEndTag(); // Close Header

            // END HEADER
            // BEGIN Content AREA
            // <div class="amModalFrame" id="amModalFrameDiv">
            writer.AddAttribute("class", "amModalFrame");
            writer.AddAttribute("id", "amModalFrameDiv");
            writer.RenderBeginTag(HtmlTextWriterTag.Div);
            this.cb.RenderControl(writer);
            writer.RenderEndTag(); // Close Content Area

            // END Content AREA
            // END Main Div
            writer.RenderEndTag(); // Close Main Div

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("<script type=\"text/javascript\">function afam_showDialog(title,key,height,width,optional){");
            sb.Append("function getTop(obj,height){");
            sb.Append("var dlg=document.getElementById(obj);");
            sb.Append("var winH=document.body.clientHeight;");
            sb.Append("var top=((winH/2)-(height/2));");
            sb.Append("dlg.style.top=top+'px';");
            sb.Append("};");
            sb.Append("function getLeft(obj,width){");
            sb.Append("var dlg=document.getElementById(obj);");
            sb.Append("var winW=document.body.offsetWidth;");
            sb.Append("dlg.style.left=((winW/2)-(width/2))+'px';");
            sb.Append("};");
            sb.Append("dlgHeight=height;");
            sb.Append("dlgWidth=width;");
            sb.Append("        dlgTitle=title;");
            sb.Append("        getTop('amModal',height);");
            sb.Append("        getLeft('amModal',width);");
            sb.Append(" var mask = document.getElementById('amModalMask');");
            sb.Append("        mask.style.zIndex=200000;");
            sb.Append("        mask.style.height=document.body.offsetHeight;");
            sb.Append("        mask.style.width=document.body.offsetWidth-22;");
            sb.Append("        mask.style.display='';");
            sb.Append("    var modal = document.getElementById('amModal');");
            sb.Append("     var modalFrameDiv = document.getElementById('amModalFrameDiv');");
            sb.Append("        modal.style.zIndex=200001;");
            sb.Append("        modal.style.height=dlgHeight+'px';");
            sb.Append("        modal.style.width=dlgWidth+'px';  ");
            sb.Append("        modal.style.display='';");
            sb.Append("        modalFrameDiv.style.height=height-22+'px';");
            sb.Append("        var amModalHeader=document.getElementById('amModalHeaderText');");
            sb.Append("        amModalHeader.innerHTML=dlgTitle;");
            sb.Append("        if(optional=='undefined'){optional=''};");
            sb.Append("         var modFrame = document.getElementById('amModalFrame');");
            sb.Append("            if (modFrame != undefined){");
            sb.Append("                modFrame.height = '0';");
            sb.Append("            };");
            sb.Append("        " + this.cb.ClientID + ".Callback('load',key,optional);");
            sb.Append("};");
            sb.Append("function amaf_closeDialog(){");
            sb.Append("        var modFrame = document.getElementById('amModalFrame');");
            sb.Append("        if (modFrame != undefined){");
            sb.Append("            modFrame.height = '0';");
            sb.Append("           modFrame.parentNode.removeChild(modFrame);");
            sb.Append("        };");
            sb.Append("        var dlg=document.getElementById('amModal');");
            sb.Append("        dlg.style.display='none';");
            sb.Append(" var mask = document.getElementById('amModalMask');");
            sb.Append("        var cModal = document.getElementById('" + this.cb.ClientID + "');");
            sb.Append("        cModal.removeChild(cModal.firstChild);");
            sb.Append("        mask.style.display='none';");
            sb.Append("};</script>");
            writer.Write(sb.ToString());
        }

        public delegate void CallbackEventHandler(object sender, CallBackEventArgs e);

        public event CallbackEventHandler Callback;

        public void RaiseCallback(object sender, CallBackEventArgs e) // Implements ICallbackEventHandler.RaiseCallback
        {
            this.OnCallback(e);
        }

        protected virtual void OnCallback(CallBackEventArgs e)
        {
            if (this.Callback != null)
            {
                this.Callback(this.cb, e);
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.cb.CallbackEvent += this.RaiseCallback;
        }
    }
}
