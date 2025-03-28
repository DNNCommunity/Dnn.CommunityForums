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

    public partial class admin_filters_new : ActiveAdminBase
    {
        #region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.agFilters.Callback += this.agFilters_Callback;
            this.agFilters.ItemBound += this.agFilters_ItemBound;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.agFilters.ColDelimiter = "||";
        }

        private void agFilters_Callback(object sender, Modules.ActiveForums.Controls.CallBackEventArgs e)
        {
            try
            {
                if (!(e.Parameters[4] == string.Empty))
                {
                    string sAction = e.Parameters[4].Split(':')[0];
                    int filterId = Convert.ToInt32(e.Parameters[4].Split(':')[1]);
                    switch (sAction.ToUpper())
                    {
                        case "DELETE":
                            if (Utilities.IsNumeric(filterId))
                            {
                                new DotNetNuke.Modules.ActiveForums.Controllers.FilterController().DeleteById(filterId);
                            }

                            break;
                        case "DEFAULTS":
                            new DotNetNuke.Modules.ActiveForums.Controllers.FilterController().DeleteByModuleId(this.ModuleId);
                            Controllers.FilterController.ImportFilter(this.PortalId, this.ModuleId);
                            break;
                    }
                }

                int pageIndex = Convert.ToInt32(e.Parameters[0]);
                int pageSize = Convert.ToInt32(e.Parameters[1]);
                string sortColumn = e.Parameters[2].ToString();
                string sort = e.Parameters[3].ToString();
                this.agFilters.Datasource = DataProvider.Instance().Filters_List(this.PortalId, this.ModuleId, pageIndex, pageSize, sort, sortColumn);
                this.agFilters.Refresh(e.Output);
            }
            catch (Exception ex)
            {
            }
        }

        private void agFilters_ItemBound(object sender, Modules.ActiveForums.Controls.ItemBoundEventArgs e)
        {
            e.Item[1] = this.Server.HtmlEncode(e.Item[1].ToString());
            e.Item[2] = this.Server.HtmlEncode(e.Item[2].ToString());
            e.Item[4] = "<img src=\"" + this.Page.ResolveUrl(Globals.ModulePath + "images/delete16.png") + "\" alt=\"" + this.GetSharedResource("[RESX:Delete]") + "\" height=\"16\" width=\"16\" />";
        }
        #endregion
    }
}
