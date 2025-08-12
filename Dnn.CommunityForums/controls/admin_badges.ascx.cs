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
    using System.Web.UI;
    using System.Web.UI.WebControls;

    public partial class admin_badges : ActiveAdminBase
    {
        public string ImagePath => this.Page.ResolveUrl(string.Concat(this.MainSettings.ThemeLocation, "/images"));

        protected global::DotNetNuke.Modules.ActiveForums.Controls.ActiveGrid agBadges;
        protected global::System.Web.UI.WebControls.DropDownList drpBadgeMetrics;
        protected global::System.Web.UI.WebControls.DropDownList drpBadgeImages;

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.agBadges.Callback += this.agBadges_Callback;
            this.agBadges.ItemBound += this.AgBadgesItemBound;

        }

        protected override void OnLoad(EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                base.OnLoad(e);

                this.BindBadgeMetrics();
                this.BindBadgeImages();
            }
        }

        private void agBadges_Callback(object sender, Modules.ActiveForums.Controls.CallBackEventArgs e)
        {
            this.agBadges.Datasource = DataProvider.Instance().Badges_List(this.ModuleId);
            this.agBadges.Refresh(e.Output);
        }

        private void AgBadgesItemBound(object sender, Modules.ActiveForums.Controls.ItemBoundEventArgs e)
        {
            e.Item[8] = this.GetImageTag(Utilities.SafeConvertInt(e.Item[7].ToString()), e.Item[2].ToString());
            e.Item[5] = this.GetEnumName((DotNetNuke.Modules.ActiveForums.Enums.BadgeMetric)Utilities.SafeConvertInt(e.Item[4].ToString()));
        }

        private string GetImageTag(int fileId, string badgeName)
        {
            if (fileId <= 0)
            {
                return string.Empty;
            }

            var imgUrl = Utilities.ResolveUrl($"https://{this.PortalSettings.DefaultPortalAlias}/DnnImageHandler.ashx?mode=securefile&fileId={fileId}&h=16&w=16");
            return $"<img src=\"{imgUrl}\" border=\"0\" alt=\"{badgeName}\" />";

        }

        private string GetEnumName(DotNetNuke.Modules.ActiveForums.Enums.BadgeMetric badgeMetric)
        {
            return Utilities.GetSharedResource($"[RESX:{Enum.GetName(typeof(DotNetNuke.Modules.ActiveForums.Enums.BadgeMetric), badgeMetric)}]", isAdmin: true);
        }

        private void BindBadgeMetrics()
        {
            Utilities.BindEnum(pDDL: this.drpBadgeMetrics, enumType: typeof(Enums.BadgeMetric), pColValue: string.Empty, addEmptyValue: false, localize: true, excludeIndex: -1);
        }

        private void BindBadgeImages()
        {
            var folderInfo = DotNetNuke.Services.FileSystem.FolderManager.Instance.GetFolder(this.PortalId, Globals.DefaultBadgesFolderName);
            foreach (var fileInfo in DotNetNuke.Services.FileSystem.FolderManager.Instance.GetFiles(folderInfo, recursive: true))
            {
                if (fileInfo.ContentType.ToLowerInvariant().Contains("image/"))
                {
                    this.drpBadgeImages.Items.Add(new ListItem(fileInfo.FileName, fileInfo.FileId.ToString()));
                }
            }
            this.drpBadgeImages.Items.Insert(0, new ListItem("[RESX:DropDownDefault]", "-1"));

        }
    }
}
