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
namespace DotNetNuke.Modules.ActiveForums.Controls
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Text;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    [ToolboxData("<{0}:Toolbar runat=server></{0}:Toolbar>")]
    public class Toolbar : ControlsBase
    {

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            string sTemp = string.Empty;
            //pt = New Forums.Utils.TimeCalcItem("ForumDisplay")
            if (this.ControlConfig != null)
            {
                object obj = DataCache.SettingsCacheRetrieve(this.ModuleId,string.Format(CacheKeys.Toolbar, this.ForumModuleId, this.UserInfo?.Profile?.PreferredLocale ));
                if (obj == null)
                {
                    sTemp = this.ParseTemplate();
                    DataCache.SettingsCacheStore(this.ModuleId, string.Format(CacheKeys.Toolbar, this.ModuleId), sTemp);
                }
                else
                {
                    sTemp = Convert.ToString(obj);
                }

                sTemp = Utilities.LocalizeControl(sTemp);
                if (!sTemp.Contains(Globals.ForumsControlsRegisterAFTag))
                {
                    sTemp = Globals.ForumsControlsRegisterAFTag + sTemp;
                }

                Control ctl = this.Page.ParseControl(sTemp);
                this.LinkControls(ctl.Controls);
                this.Controls.Add(ctl);
            }
        }

        private void LinkControls(ControlCollection ctrls)
        {
            foreach (Control ctrl in ctrls)
            {
                if (ctrl is Controls.ControlsBase)
                {
                    ((Controls.ControlsBase)ctrl).ControlConfig = this.ControlConfig;
                }

                if (ctrl is Controls.Link)
                {
                    ((Controls.Link)ctrl).UserRoles = this.ForumUser.UserRoles;
                }

                if (ctrl.Controls.Count > 0)
                {
                    this.LinkControls(ctrl.Controls);
                }
            }
        }

        private string ParseTemplate()
        {
            string tb = this.DisplayTemplate; //Utilities.ParseToolBar(DisplayTemplate, PageId, ModuleId, UserId, CurrentUserTypes.Admin)
            //tb = tb.Replace
            tb = tb.Replace("[AF:TB:Announcements]", "<af:link id=\"lnkAnnouncements\" NavigateUrl=\"" + Utilities.NavigateURL(this.PageId, "", new string[] { $"{ParamKeys.ViewType}={Views.Grid}", $"{ParamKeys.GridType}={GridTypes.Announcements}" }) + "\" text=\"[RESX:Announcements]\" runat=\"server\" />");
            tb = tb.Replace("[AF:TB:Unresolved]", "<af:link id=\"lnkUnresolved\" NavigateUrl=\"" + Utilities.NavigateURL(this.PageId, "", new string[] { $"{ParamKeys.ViewType}={Views.Grid}", $"{ParamKeys.GridType}={GridTypes.Unresolved}" }) + "\" text=\"[RESX:Unresolved]\" runat=\"server\" />");
            tb = tb.Replace("[AF:TB:Unanswered]", "<af:link id=\"lnkUnanswered\" NavigateUrl=\"" + Utilities.NavigateURL(this.PageId, "", new string[] { $"{ParamKeys.ViewType}={Views.Grid}", $"{ParamKeys.GridType}={GridTypes.Unanswered}" }) + "\" text=\"[RESX:Unanswered]\" runat=\"server\" />");
            tb = tb.Replace("[AF:TB:ActiveTopics]", "<af:link id=\"lnkActive\" NavigateURL=\"" + Utilities.NavigateURL(this.PageId, "", new string[] { $"{ParamKeys.ViewType}={Views.Grid}", $"{ParamKeys.GridType}={GridTypes.ActiveTopics}" }) + "\" text=\"[RESX:ActiveTopics]\" runat=\"server\" />");
            tb = tb.Replace("[AF:TB:Search]", "<af:link id=\"lnkSearch\" NavigateUrl=\"" + Utilities.NavigateURL(this.PageId, "", $"{ParamKeys.ViewType}={Views.Search}") + "\" text=\"[RESX:Search]\" runat=\"server\" />");
            tb = tb.Replace("[AF:TB:Forums]", "<af:link id=\"lnkForums\" navigateUrl=\"" + Utilities.NavigateURL(this.PageId) + "\" text=\"[RESX:FORUMS]\" runat=\"server\" />");
            tb = tb.Replace("[AF:TB:NotRead]", "<af:link id=\"lnkNotRead\" NavigateUrl=\"" + Utilities.NavigateURL(this.PageId, "", new string[] { $"{ParamKeys.ViewType}={Views.Grid}", $"{ParamKeys.GridType}={GridTypes.NotRead}" }) + "\" text=\"[RESX:NotRead]\" AuthRequired=\"True\" runat=\"server\" />");
            tb = tb.Replace("[AF:TB:MyTopics]", "<af:link id=\"lnkMyTopics\" NavigateUrl=\"" + Utilities.NavigateURL(this.PageId, "", new string[] { $"{ParamKeys.ViewType}={Views.Grid}", $"{ParamKeys.GridType}={GridTypes.MyTopics}" }) + "\" text=\"[RESX:MyTopics]\" AuthRequired=\"True\" runat=\"server\" />");
            tb = tb.Replace("[AF:TB:MostLiked]", "<af:link id=\"lnkMostLiked\" NavigateUrl=\"" + Utilities.NavigateURL(this.PageId, "", new string[] { $"{ParamKeys.ViewType}={Views.Grid}", $"{ParamKeys.GridType}={GridTypes.MostLiked}" }) + "\" text=\"[RESX:MostLiked]\" runat=\"server\" />");
            tb = tb.Replace("[AF:TB:MostReplies]", "<af:link id=\"lnkMostReplies\" NavigateUrl=\"" + Utilities.NavigateURL(this.PageId, "", new string[] { $"{ParamKeys.ViewType}={Views.Grid}", $"{ParamKeys.GridType}={GridTypes.MostReplies}" }) + "\" text=\"[RESX:MostReplies]\" runat=\"server\" />");
            tb = tb.Replace("[AF:TB:MyProfile]", string.Empty);
            tb = tb.Replace("[AF:TB:MemberList]", string.Empty);
            tb = tb.Replace("[AF:TB:MySettings]", string.Empty);
            tb = tb.Replace("[AF:TB:MySubscriptions]", string.Empty);
            tb = tb.Replace("[AF:TB:ControlPanel]", "<af:link id=\"lnkControlPanel\" NavigateUrl=\"" + Utilities.NavigateURL(this.PageId, "EDIT", "mid=" + this.ControlConfig.ModuleId) + "\" EnabledRoles=\"" + this.ControlConfig.AdminRoles + "\" Text=\"[RESX:ControlPanel]\" runat=\"server\" />");
            //TODO: Check for moderator
            tb = tb.Replace("[AF:TB:ModList]", string.Empty);
            return tb;
        }
    }

}
