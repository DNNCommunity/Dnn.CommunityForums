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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Web.UI.WebControls;

namespace DotNetNuke.Modules.ActiveForums
{
    public partial class af_quickjump : ForumBase
    {
        protected DropDownList drpForums = new DropDownList();
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use Forums property.")]
        public DataTable dtForums { get; set; } = null;
        public List<DotNetNuke.Modules.ActiveForums.Entities.ForumInfo> Forums { get; set; }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            drpForums.SelectedIndexChanged += new System.EventHandler(drpForums_SelectedIndexChanged);

            try
            {
                if (drpForums == null)
                {
                    drpForums = new DropDownList();
                }
                drpForums.AutoPostBack = true;
                drpForums.CssClass = "afdropdown";
                BindForums();
                Controls.Add(drpForums);
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
            if (dtForums != null)
            {
                Forums = new DotNetNuke.Modules.ActiveForums.Entities.ForumCollection();
                foreach (DataRow dr in dtForums.DefaultView.ToTable().Rows)
                {
                    Forums.Add(new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(Utilities.SafeConvertInt(dr["ForumId"]), ForumModuleId));
                }
            }
            #endregion

            if (Forums == null)
            {
                Forums = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetForums(ForumModuleId);
            }
            drpForums.Items.Clear();
            drpForums.Items.Insert(0, new ListItem(string.Empty, string.Empty));
            int index = 1;
            DotNetNuke.Modules.ActiveForums.Controllers.ForumController.IterateForumsList(Forums, ForumUser, fi =>
            {
                drpForums.Items.Insert(index, new ListItem(fi.GroupName, $"GROUPJUMP:{fi.ForumGroupId}"));
                index += 1;
            }, fi =>
            {
                drpForums.Items.Insert(index, new ListItem($"--{fi.ForumName}", $"FORUMJUMP{fi.ForumID}"));
                index += 1;
            },
            fi =>
            {
                drpForums.Items.Insert(index, new ListItem(
                    fi.ForumName.Length > 30 ? $"{fi.ForumName.Substring(0, 27)}..." : fi.ForumName,
                    $"FORUMJUMP{fi.ForumID}"));
                index += 1;
            });

            if (GetViewType != null)
            {
                if (GetViewType == "TOPICS" || GetViewType == "TOPIC")
                {
                    drpForums.SelectedIndex = drpForums.Items.IndexOf(drpForums.Items.FindByValue($"FORUMJUMP{ForumId}"));
                }
            }
        }

        private void drpForums_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            string sJumpValue = drpForums.SelectedItem.Value;
            if (!(sJumpValue == string.Empty) && !(sJumpValue == ""))
            {
                string sJumpType = sJumpValue.Substring(0, (sJumpValue.IndexOf(":", 0) + 1) - 1);
                string sJumpID = sJumpValue.Substring((sJumpValue.IndexOf(":", 0) + 1));
                switch (sJumpType)
                {
                    case "GROUPJUMP":
                        Response.Redirect(NavigateUrl(TabId, "", ParamKeys.GroupId + "=" + sJumpID));
                        break;
                    case "FORUMJUMP":
                        string[] Params = { ParamKeys.ViewType + "=" + Views.Topics, ParamKeys.ForumId + "=" + sJumpID };
                        Response.Redirect(NavigateUrl(TabId, "", Params));
                        break;
                }
            }
        }
    }
}
