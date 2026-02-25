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
    using System.Data;
    using System.Web.UI.WebControls;

    public partial class af_quickjump : ForumBase
    {
        protected DropDownList drpForums = new DropDownList();

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use Forums property.")]
        public DataTable dtForums { get; set; } = null;

        public List<DotNetNuke.Modules.ActiveForums.Entities.ForumInfo> Forums { get; set; }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.drpForums.SelectedIndexChanged += new System.EventHandler(this.drpForums_SelectedIndexChanged);

            try
            {
                if (this.drpForums == null)
                {
                    this.drpForums = new DropDownList();
                }

                this.drpForums.AutoPostBack = true;
                this.drpForums.CssClass = "afdropdown";
                this.BindForums();
                this.Controls.Add(this.drpForums);
            }
            catch (Exception exc)
            {
                DotNetNuke.Services.Exceptions.Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void BindForums()
        {
            #region "backward compatibilty - remove when removing dtForums property"
            /* this is for backward compatibility -- remove when removing dtForums property */
            if (this.dtForums != null)
            {
                this.Forums = new DotNetNuke.Modules.ActiveForums.Entities.ForumCollection();
                foreach (DataRow dr in this.dtForums.DefaultView.ToTable().Rows)
                {
                    this.Forums.Add(new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(Utilities.SafeConvertInt(dr["ForumId"]), this.ForumModuleId));
                }
            }
            #endregion

            if (this.Forums == null)
            {
                this.Forums = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetForums(this.ForumModuleId);
            }

            this.drpForums.Items.Clear();
            this.drpForums.Items.Insert(0, new ListItem(string.Empty, string.Empty));
            int index = 1;
            DotNetNuke.Modules.ActiveForums.Controllers.ForumController.IterateForumsList(
                forums: this.Forums,
                forumUserInfo: this.ForumUser,
                groupAction: fi =>
                {
                    this.drpForums.Items.Insert(index, new ListItem(fi.GroupName, $"GROUPJUMP:{fi.ForumGroupId}"));
                    index += 1;
                },
                forumAction: fi =>
                {
                    this.drpForums.Items.Insert(index, new ListItem($"--{fi.ForumName}", $"FORUMJUMP:{fi.ForumID}"));
                    index += 1;
                },
                subForumAction: fi =>
                {
                    this.drpForums.Items.Insert(index, new ListItem(
                        fi.ForumName.Length > 30 ? $"{fi.ForumName.Substring(0, 27)}..." : fi.ForumName,
                        $"FORUMJUMP:{fi.ForumID}"));
                    index += 1;
                },
                includeHiddenForums: false,
                includeInactiveForums: false);

            if (this.GetViewType != null)
            {
                if (this.GetViewType == "TOPICS" || this.GetViewType == "TOPIC")
                {
                    this.drpForums.SelectedIndex = this.drpForums.Items.IndexOf(this.drpForums.Items.FindByValue($"FORUMJUMP:{this.ForumId}"));
                }
            }
        }

        private void drpForums_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            string sJumpValue = this.drpForums.SelectedItem.Value;
            if (!string.IsNullOrEmpty(sJumpValue))
            {
                string sJumpType = sJumpValue.Substring(0, sJumpValue.IndexOf(":", 0) + 1 - 1);
                string sJumpID = sJumpValue.Substring(sJumpValue.IndexOf(":", 0) + 1);
                switch (sJumpType)
                {
                    case "GROUPJUMP":
                        this.Response.Redirect(this.NavigateUrl(this.PortalSettings.ActiveTab.TabID, string.Empty, ParamKeys.GroupId + "=" + sJumpID), false);
                        this.Context.ApplicationInstance.CompleteRequest();
                        break;
                    case "FORUMJUMP":
                        string[] @params = { ParamKeys.ViewType + "=" + Views.Topics, ParamKeys.ForumId + "=" + sJumpID };
                        this.Response.Redirect(this.NavigateUrl(this.PortalSettings.ActiveTab.TabID, string.Empty, @params), false);
                        this.Context.ApplicationInstance.CompleteRequest();
                        break;
                }
            }
        }
    }
}
