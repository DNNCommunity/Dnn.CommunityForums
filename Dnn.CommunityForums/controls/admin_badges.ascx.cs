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
    using System.Web.UI.WebControls;

    public partial class admin_badges : ActiveAdminBase
    {
        public string ImagePath => this.Page.ResolveUrl(string.Concat(this.MainSettings.ThemeLocation, "/images"));

        protected global::DotNetNuke.Modules.ActiveForums.Controls.ActiveGrid agBadges;
        protected global::System.Web.UI.WebControls.DropDownList drpBadgeImages;
        protected global::System.Web.UI.WebControls.DropDownList drpBadgeMetrics;

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.agBadges.Callback += this.agBadges_Callback;
            this.agBadges.ItemBound += this.AgBadgesItemBound;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.BindBadgeImages();
        }

        private void agBadges_Callback(object sender, Modules.ActiveForums.Controls.CallBackEventArgs e)
        {
            this.agBadges.Datasource = DataProvider.Instance().Ranks_List(this.PortalId, this.ModuleId);
            this.agBadges.Refresh(e.Output);
        }

        private void AgBadgesItemBound(object sender, Modules.ActiveForums.Controls.ItemBoundEventArgs e)
        {
            e.Item[4] = this.GetDisplay(e.Item[4].ToString(), e.Item[1].ToString());
        }

        public string GetDisplay(string display, string badgeName)
        {
            return "<img src=\"" + this.HostURL + display.Replace("activeforums/Badges", "ActiveForums/images/badges") + "\" border=\"0\" alt=\"" + badgeName + "\" />";
        }

        private void BindBadgeImages()
        {
            string[] fileCollection = null;
            System.IO.FileInfo myFileInfo = null;
            int i = 0;

            fileCollection = System.IO.Directory.GetFiles(Utilities.MapPath(Globals.ModulePath + "images/badges"));
            for (i = 0; i < fileCollection.Length; i++)
            {
                string path = null;
                myFileInfo = new System.IO.FileInfo(fileCollection[i]);
                path = "DesktopModules/ActiveForums/images/badges/" + myFileInfo.Name;
                this.drpBadgeImages.Items.Insert(i, new ListItem(myFileInfo.Name, path.ToLowerInvariant()));
            }

            this.drpBadgeImages.Items.Insert(0, new ListItem("[RESX:DropDownDefault]", "-1"));
        }

        private void BindBadgeMetrics()
        {
            Utilities.BindEnum(pDDL: this.drpBadgeMetrics, enumType: typeof(Enums.BadgeMetric), pColValue: string.Empty, addEmptyValue: false, localize: true, excludeIndex: 0);
        }
    }
}
