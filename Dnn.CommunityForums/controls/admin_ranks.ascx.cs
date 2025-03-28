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

namespace DotNetNuke.Modules.ActiveForums
{
    using System;
    using System.Web.UI.WebControls;

    public partial class admin_ranks_new : ActiveAdminBase
    {
        public string ImagePath => this.Page.ResolveUrl(string.Concat(this.MainSettings.ThemeLocation, "/images"));

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.agRanks.Callback += this.agRanks_Callback;
            this.agRanks.ItemBound += this.agRanks_ItemBound;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.BindRankImages();
        }

        private void agRanks_Callback(object sender, Modules.ActiveForums.Controls.CallBackEventArgs e)
        {
            this.agRanks.Datasource = DataProvider.Instance().Ranks_List(this.PortalId, this.ModuleId);
            this.agRanks.Refresh(e.Output);
        }

        private void agRanks_ItemBound(object sender, Modules.ActiveForums.Controls.ItemBoundEventArgs e)
        {
            e.Item[4] = this.GetDisplay(e.Item[4].ToString(), e.Item[1].ToString());
        }

        public string GetDisplay(string display, string rankName)
        {
            return "<img src=\"" + this.HostURL + display.Replace("activeforums/Ranks", "ActiveForums/images/ranks") + "\" border=\"0\" alt=\"" + rankName + "\" />";
        }

        private void BindRankImages()
        {
            string[] fileCollection = null;
            System.IO.FileInfo myFileInfo = null;
            int i = 0;

            fileCollection = System.IO.Directory.GetFiles(Utilities.MapPath(Globals.ModulePath + "images/ranks"));
            for (i = 0; i < fileCollection.Length; i++)
            {
                string path = null;
                myFileInfo = new System.IO.FileInfo(fileCollection[i]);
                path = "DesktopModules/ActiveForums/images/ranks/" + myFileInfo.Name;
                this.drpRankImages.Items.Insert(i, new ListItem(myFileInfo.Name, path.ToLowerInvariant()));
            }

            this.drpRankImages.Items.Insert(0, new ListItem("[RESX:DropDownDefault]", "-1"));

            // drpRankImages.Items.Insert(1, New ListItem(Utilities.GetSharedResource("RankCustom.Text"), "0"))
        }
    }
}
