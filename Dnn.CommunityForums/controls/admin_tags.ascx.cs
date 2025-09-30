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

    public partial class admin_tags : ActiveAdminBase
    {
        #region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.agTags.Callback += this.agTags_Callback;
            this.agTags.ItemBound += this.agTags_ItemBound;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        private void agTags_Callback(object sender, Modules.ActiveForums.Controls.CallBackEventArgs e)
        {
            try
            {
                if (!(e.Parameters[4] == string.Empty))
                {
                    string sAction = e.Parameters[4].Split(':')[0];

                    switch (sAction.ToUpper())
                    {
                        case "DELETE":
                            {
                                int tagId = Convert.ToInt32(e.Parameters[4].Split(':')[1]);
                                if (Utilities.IsNumeric(tagId))
                                {
                                    new DotNetNuke.Modules.ActiveForums.Controllers.TagController().DeleteById(tagId);
                                }

                                break;
                            }

                        case "SAVE":
                            {
                                string[] sParams = e.Parameters[4].Split(':');
                                string tagName = sParams[1].Trim();
                                int tagId = 0;
                                if (sParams.Length > 2)
                                {
                                    tagId = Convert.ToInt32(sParams[2]);
                                }

                                if (!(tagName == string.Empty))
                                {
                                    var tagController = new DotNetNuke.Modules.ActiveForums.Controllers.TagController();
                                    DotNetNuke.Modules.ActiveForums.Entities.TagInfo tag = null;
                                    if (tagId > 0)
                                    {
                                        tag = tagController.GetById(tagId);
                                    }

                                    if (tag != null)
                                    {
                                        tag.TagName = tagName;
                                        tag.PortalId = this.PortalId;
                                        tag.ModuleId = this.ModuleId;
                                        tagController.Update(tag);
                                        tagController.RecountItems(tag.TagId);
                                    }
                                    else
                                    {
                                        tag = new DotNetNuke.Modules.ActiveForums.Entities.TagInfo()
                                        {

                                            TagName = tagName,
                                            PortalId = this.PortalId,
                                            ModuleId = this.ModuleId,
                                            Items = 0,
                                        };
                                        tagController.Insert(tag);
                                    }
                                }

                                break;
                            }
                    }
                }

                this.agTags.DefaultParams = string.Empty;
                int pageIndex = Convert.ToInt32(e.Parameters[0]);
                int pageSize = Convert.ToInt32(e.Parameters[1]);
                string sortColumn = e.Parameters[2].ToString();
                string sort = e.Parameters[3].ToString();
                this.agTags.Datasource = DataProvider.Instance().Tags_List(this.PortalId, this.ModuleId, pageIndex, pageSize, sort, sortColumn);
                this.agTags.Refresh(e.Output);
            }
            catch (Exception ex)
            {
            }
        }

        private void agTags_ItemBound(object sender, Modules.ActiveForums.Controls.ItemBoundEventArgs e)
        {
            e.Item[3] = "<img src=\"" + this.Page.ResolveUrl(Globals.ModulePath + "images/delete16.png") + "\" alt=\"" + this.GetSharedResource("[RESX:Delete]") + "\" height=\"16\" width=\"16\" />";
        }
        #endregion

    }
}
