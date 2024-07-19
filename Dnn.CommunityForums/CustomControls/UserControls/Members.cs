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

using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Security.Cryptography;

namespace DotNetNuke.Modules.ActiveForums.Controls
{
    [DefaultProperty("Text"), ToolboxData("<{0}:Members runat=server></{0}:Members>")]
    public class Members : SettingsBase
    {
        #region Private Members
        private int _memberCount = 0;

        private int PageSize = 20;
        private int RowIndex = 0;
        private string Filter = "";
        #endregion
        #region Public Properties
        #endregion
        #region Protected Controls
        protected PlaceHolder plhContent = new PlaceHolder();
        protected global::DotNetNuke.Modules.ActiveForums.Controls.PagerNav Pager1;
        #endregion
        #region Event Handlers

        protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

            this.AppRelativeVirtualPath = "~/";
            if (Request.Params["affilter"] != null)
            {
                Filter = Convert.ToString(Request.Params["affilter"]).Substring(0, 1);
            }
            else
            {
                Filter = string.Empty;
            }
        }
        protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

            try
            {
                BuildControl();
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
            try
            {
                LinkControls(Controls);
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
            try
            {
                BuildPager();
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }

        }
        #endregion
        #region Private Methods
        private void BuildControl()
        {

            System.Text.StringBuilder sb = new System.Text.StringBuilder(1024);
            SettingsInfo moduleSettings = SettingsBase.GetModuleSettings(ForumModuleId);
            string sTemplate = TemplateCache.GetCachedTemplate(ForumModuleId, "_memberlist", 0);
            if (!(sTemplate == string.Empty))
            {
                string sGrid = TemplateUtils.GetTemplateSection(sTemplate, "[AF:CONTROL:LIST]", "[/AF:CONTROL:LIST]");
                string sHeader = TemplateUtils.GetTemplateSection(sTemplate, "[AF:CONTROL:LIST:HEADER]", "[/AF:CONTROL:LIST:HEADER]");
                string sNormRow = TemplateUtils.GetTemplateSection(sTemplate, "[AF:CONTROL:LIST:ITEM]", "[/AF:CONTROL:LIST:ITEM]");
                string sAltRow = TemplateUtils.GetTemplateSection(sTemplate, "[AF:CONTROL:LIST:ALTITEM]", "[/AF:CONTROL:LIST:ALTITEM]");
                string sFooter = TemplateUtils.GetTemplateSection(sTemplate, "[AF:CONTROL:LIST:FOOTER]", "[/AF:CONTROL:LIST:FOOTER]");
                sGrid = TemplateUtils.ReplaceSubSection(sGrid, sHeader, "[AF:CONTROL:LIST:HEADER]", "[/AF:CONTROL:LIST:HEADER]");
                sGrid = TemplateUtils.ReplaceSubSection(sGrid, sFooter, "[AF:CONTROL:LIST:FOOTER]", "[/AF:CONTROL:LIST:FOOTER]");

                List<DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo> upl = GetMemberList();
                if (upl != null)
                {
                    int i = 0;
                    foreach (DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo up in upl)
                    {
                        string sRow = string.Empty;
                        if (i % 2 == 0)
                        {
                            sRow = sNormRow;
                        }
                        else
                        {
                            sRow = sAltRow;
                        }
                        var user = new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController().GetById(up.UserID);
                        sRow = TemplateUtils.ParseProfileTemplate(ForumModuleId,sRow, user, ImagePath, CurrentUserType, false, false, false, string.Empty, -1, TimeZoneOffset);
                        sb.Append(sRow);
                        i += 1;
                    }
                    sGrid = TemplateUtils.ReplaceSubSection(sGrid, sb.ToString(), "[AF:CONTROL:LIST:ITEM]", "[/AF:CONTROL:LIST:ALTITEM]");
                }
                sTemplate = TemplateUtils.ReplaceSubSection(sTemplate, sGrid, "[AF:CONTROL:LIST]", "[/AF:CONTROL:LIST]");
            }
            sTemplate = Globals.ForumsControlsRegisterAMTag + sTemplate;
            sTemplate = sTemplate.Replace("[AF:CONTROL:PAGER]", "<am:pagernav id=\"Pager1\" runat=\"server\" />");
            sTemplate = sTemplate.Replace("[AF:CONTROL:ALPHABAR]", BuildAlphaList());
            Control ctl = this.ParseControl(sTemplate);

            this.Controls.Add(ctl);
        }
        private void LinkControls(ControlCollection ctrls)
        {
            foreach (Control ctrl in ctrls)
            {
                switch (ctrl.ID)
                {
                    case "Pager1":
                        Pager1 = (DotNetNuke.Modules.ActiveForums.Controls.PagerNav)ctrl;
                        break;
                }
                if (ctrl.Controls.Count > 0)
                {
                    LinkControls(ctrl.Controls);
                }
            }
        }
        private void BuildPager()
        {
            int intPages = 0;
            intPages = Convert.ToInt32(System.Math.Ceiling(_memberCount / (double)PageSize));
            Pager1.PageCount = intPages;
            Pager1.CurrentPage = PageId;
            Pager1.TabID = TabId;
            Pager1.ForumID = -1;
            Pager1.PageText = Utilities.GetSharedResource("[RESX:Page]");
            Pager1.OfText = Utilities.GetSharedResource("[RESX:PageOf]");
            Pager1.PageMode = PagerNav.Mode.CallBack;
            Pager1.View = "members";

            if (Request.Params["affilter"] != null)
            {
                string[] Params = { "affilter=" + Request.Params["affilter"] };
                Pager1.Params = Params;
            }
        }
        private string BuildAlphaList()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            LiteralControl litString = new LiteralControl();
            sb.Append("<div align=\"center\"><table width=\"90%\" cellpadding=\"0\" cellspacing=\"5\"><tr>");
            int i = 65;
            HyperLink hypAlpha = new HyperLink();
            for (i = 65; i <= 90; i++)
            {
                litString = new LiteralControl();
                sb.Append("<td align=\"center\">");
                char strChar = (char)(i);
                string[] Params = { ParamKeys.ViewType + "=members", "affilter=" + strChar };
                sb.Append("<a href=\"" + NavigateUrl(TabId, "", Params) + "\" class=\"CommandButton\">");
                sb.Append(strChar);
                sb.Append("</a></td>");
            }
            sb.Append("<td align=center>");
            sb.Append("<a href=\"" + NavigateUrl(TabId, "", ParamKeys.ViewType + "=members") + "\" class=\"CommandButton\">");
            sb.Append(Utilities.GetSharedResource("[RESX:All]"));
            sb.Append("</a></td></tr></table></div>");
            return sb.ToString();
        }
        private List<DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo> GetMemberList()
        {
            List<DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo> upl = new List<DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo>();
            DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo upi = null;
            PageSize = MainSettings.PageSize;
            if (PageId == 1)
            {
                RowIndex = 0;
            }
            else
            {
                RowIndex = ((PageId * PageSize) - PageSize);
            }
            IDataReader dr = DataProvider.Instance().Profiles_MemberList(PortalId, ModuleId, PageSize, RowIndex, Filter);
            try
            {
                dr.Read();
                _memberCount = Convert.ToInt32(dr[0]);
                dr.NextResult();
                while (dr.Read())
                {

                    upl.Add(new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController().GetById(Convert.ToInt32(dr["UserId"].ToString())));
                }
            }
            catch (Exception ex)
            {
                dr.Close();
                dr = null;
            }
            finally
            {
                if (!dr.IsClosed)
                {
                    dr.Close();
                    dr = null;
                }
            }
            return upl;
        }

        #endregion
    }
}