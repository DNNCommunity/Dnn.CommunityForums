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

    public partial class af_posticonlist : System.Web.UI.UserControl
    {
        public string Theme { get; set; }

        public string PostIcon { get; set; }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                // If Not Page.IsPostBack Then
                this.LoadPostIcons();
                if (!string.IsNullOrEmpty(this.PostIcon))
                {
                    this.rblMessageIcons1.SelectedIndex = this.rblMessageIcons1.Items.IndexOf(this.rblMessageIcons1.Items.FindByValue(this.PostIcon));
                }

                // End If

            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        private void LoadPostIcons()
        {
            this.rblMessageIcons1.Items.Clear();
            string myTheme = this.Theme;
            string strHost = DotNetNuke.Common.Globals.AddHTTP(DotNetNuke.Common.Globals.GetDomainName(this.Request)) + "/";
            this.rblMessageIcons1.Items.Insert(0, new ListItem("<img src=\"" + strHost + "DesktopModules/ActiveForums/themes/" + myTheme + "/emoticons/biggrin.gif\" width=\"20\" height=\"20\" align=\"absmiddle\">&nbsp;&nbsp;&nbsp;&nbsp;", "biggrin.gif"));
            this.rblMessageIcons1.Items.Insert(1, new ListItem("<img src=\"" + strHost + "DesktopModules/ActiveForums/themes/" + myTheme + "/emoticons/crazy.gif\" width=\"20\" height=\"20\" align=\"absmiddle\">&nbsp;&nbsp;&nbsp;&nbsp;", "crazy.gif"));
            this.rblMessageIcons1.Items.Insert(2, new ListItem("<img src=\"" + strHost + "DesktopModules/ActiveForums/themes/" + myTheme + "/emoticons/cry.gif\" width=\"20\" height=\"20\" align=\"absmiddle\">&nbsp;&nbsp;&nbsp;&nbsp;", "cry.gif"));
            this.rblMessageIcons1.Items.Insert(3, new ListItem("<img src=\"" + strHost + "DesktopModules/ActiveForums/themes/" + myTheme + "/emoticons/arrow.gif\" width=\"20\" height=\"20\" align=\"absmiddle\">&nbsp;&nbsp;&nbsp;&nbsp;", "arrow.gif"));
            this.rblMessageIcons1.Items.Insert(4, new ListItem("<img src=\"" + strHost + "DesktopModules/ActiveForums/themes/" + myTheme + "/emoticons/hazard.gif\" width=\"20\" height=\"20\" align=\"absmiddle\">&nbsp;&nbsp;&nbsp;&nbsp;", "hazard.gif"));
            this.rblMessageIcons1.Items.Insert(5, new ListItem("<img src=\"" + strHost + "DesktopModules/ActiveForums/themes/" + myTheme + "/emoticons/explanationmark.gif\" width=\"20\" height=\"20\" align=\"absmiddle\">&nbsp;&nbsp;&nbsp;&nbsp;", "explanationmark.gif"));
            this.rblMessageIcons1.Items.Insert(6, new ListItem("<img src=\"" + strHost + "DesktopModules/ActiveForums/themes/" + myTheme + "/emoticons/w00t.gif\" width=\"20\" height=\"20\" align=\"absmiddle\">&nbsp;&nbsp;&nbsp;&nbsp;", "w00t.gif"));
            this.rblMessageIcons1.Items.Insert(7, new ListItem("<img src=\"" + strHost + "DesktopModules/ActiveForums/themes/" + myTheme + "/emoticons/pinch.gif\" width=\"20\" height=\"20\" align=\"absmiddle\">&nbsp;&nbsp;&nbsp;&nbsp;", "pinch.gif"));
            this.rblMessageIcons1.Items.Insert(8, new ListItem("<img src=\"" + strHost + "DesktopModules/ActiveForums/themes/" + myTheme + "/emoticons/whistling.gif\" width=\"20\" height=\"20\" align=\"absmiddle\">&nbsp;&nbsp;&nbsp;&nbsp;", "whistling.gif"));
            this.rblMessageIcons1.Items.Insert(9, new ListItem("<img src=\"" + strHost + "DesktopModules/ActiveForums/themes/" + myTheme + "/emoticons/sad.gif\" width=\"20\" height=\"20\" align=\"absmiddle\">&nbsp;&nbsp;&nbsp;&nbsp;", "sad.gif"));
            this.rblMessageIcons1.Items.Insert(10, new ListItem("<img src=\"" + strHost + "DesktopModules/ActiveForums/themes/" + myTheme + "/emoticons/questionmark.gif\" width=\"20\" height=\"20\" align=\"absmiddle\">&nbsp;&nbsp;&nbsp;&nbsp;", "questionmark.gif"));

            // rblMessageIcons1.Items.Insert(0, new ListItem("<img src=\"" + Page.ResolveUrl(ThemeLocation) + "/emoticons/biggrin.gif\") width=\"20\" height=\"20\" align=\"absmiddle\">&nbsp;&nbsp;&nbsp;&nbsp;", "biggrin.gif"));
            // rblMessageIcons1.Items.Insert(1, new ListItem("<img src=\"" + Page.ResolveUrl(MainSettings.ThemeLocation) + "/emoticons/crazy.gif\" width=\"20\" height=\"20\" align=\"absmiddle\">&nbsp;&nbsp;&nbsp;&nbsp;", "crazy.gif"));
            // rblMessageIcons1.Items.Insert(2, new ListItem("<img src=\"" + Page.ResolveUrl(MainSettings.ThemeLocation) + "/emoticons/cry.gif\" width=\"20\" height=\"20\" align=\"absmiddle\">&nbsp;&nbsp;&nbsp;&nbsp;", "cry.gif"));
            // rblMessageIcons1.Items.Insert(3, new ListItem("<img src=\"" + Page.ResolveUrl(MainSettings.ThemeLocation) + "/emoticons/arrow.gif\" width=\"20\" height=\"20\" align=\"absmiddle\">&nbsp;&nbsp;&nbsp;&nbsp;", "arrow.gif"));
            // rblMessageIcons1.Items.Insert(4, new ListItem("<img src=\"" + Page.ResolveUrl(MainSettings.ThemeLocation) + "/emoticons/hazard.gif\" width=\"20\" height=\"20\" align=\"absmiddle\">&nbsp;&nbsp;&nbsp;&nbsp;", "hazard.gif"));
            // rblMessageIcons1.Items.Insert(5, new ListItem("<img src=\"" + Page.ResolveUrl(MainSettings.ThemeLocation) + "/emoticons/explanationmark.gif\" width=\"20\" height=\"20\" align=\"absmiddle\">&nbsp;&nbsp;&nbsp;&nbsp;", "explanationmark.gif"));
            // rblMessageIcons1.Items.Insert(6, new ListItem("<img src=\"" + Page.ResolveUrl(ThemeLocation) + "/emoticons/w00t.gif\" width=\"20\" height=\"20\" align=\"absmiddle\">&nbsp;&nbsp;&nbsp;&nbsp;", "w00t.gif"));
            // rblMessageIcons1.Items.Insert(7, new ListItem("<img src=\"" + Page.ResolveUrl(DotNetNuke.Modules.ActiveForums.MainSettings.ThemeLocation) + "/emoticons/pinch.gif\" width=\"20\" height=\"20\" align=\"absmiddle\">&nbsp;&nbsp;&nbsp;&nbsp;", "pinch.gif"));
            // rblMessageIcons1.Items.Insert(8, new ListItem("<img src=\"" + Page.ResolveUrl(DotNetNuke.Modules.ActiveForums.MainSettings.ThemeLocation) + "/emoticons/whistling.gif\" width=\"20\" height=\"20\" align=\"absmiddle\">&nbsp;&nbsp;&nbsp;&nbsp;", "whistling.gif"));
            // rblMessageIcons1.Items.Insert(9, new ListItem("<img src=\"" + Page.ResolveUrl(DotNetNuke.Modules.ActiveForums.MainSettings.ThemeLocation) + "/emoticons/sad.gif\" width=\"20\" height=\"20\" align=\"absmiddle\">&nbsp;&nbsp;&nbsp;&nbsp;", "sad.gif"));
            // rblMessageIcons1.Items.Insert(10, new ListItem("<img src=\"" + Page.ResolveUrl(DotNetNuke.Modules.ActiveForums.MainSettings.ThemeLocation) + "/emoticons/questionmark.gif\" width=\"20\" height=\"20\" align=\"absmiddle\">&nbsp;&nbsp;&nbsp;&nbsp;", "questionmark.gif"));
            this.rblMessageIcons1.Items.Insert(11, new ListItem(Utilities.GetSharedResource("[RESX:PostIconNone]"), string.Empty));
            this.rblMessageIcons1.SelectedIndex = 11;
        }
    }
}
