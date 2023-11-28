//
// Community Forums
// Copyright (c) 2013-2021
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

namespace DotNetNuke.Modules.ActiveForums
{
    public partial class profile_mysubscriptions : SettingsBase
    {
        public int UID { get; set; }
        protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
            try
            {
                if (Request.QueryString["UserId"] == null)
                {
                    UID = UserInfo.UserID;
                }
                else
                {
                    UID = Convert.ToInt32(Request.QueryString["UserId"]);
                }
                if (this.UserId == Convert.ToInt32(Request.Params["UID"]) | System.Convert.ToBoolean(Session["isMod"]) == true)
                {
                    if (Request.Params["UID"] != null)
                    {
                        BindSubs(UID, PortalId);
                        BindForumSubs(UID, PortalId);
                    }
                }
            }
            catch (Exception ex)
            {
            }
           
        }
        private void BindSubs(int UserID, int PortalID)
        {
            dgrdForumSubs.DataSource = new DotNetNuke.Modules.ActiveForums.Controllers.SubscriptionController().SubscribedTopics(PortalID, ForumModuleId, UID);
            dgrdSubs.DataBind();
        }
        private void BindForumSubs(int UserID, int PortalID)
        {
            dgrdForumSubs.DataSource = new DotNetNuke.Modules.ActiveForums.Controllers.SubscriptionController().SubscribedForums(PortalID, ForumModuleId, UID);
            dgrdForumSubs.DataBind();
        }
        private void dgrdSubs_DeleteCommand(object source, System.Web.UI.WebControls.DataGridCommandEventArgs e)
        {
            int Id = System.Convert.ToInt32(dgrdSubs.DataKeys[e.Item.ItemIndex]); 
            new DotNetNuke.Modules.ActiveForums.Controllers.SubscriptionController().Delete(Id);
            Response.Redirect(Request.RawUrl);
        }

        private void dgrdForumSubs_DeleteCommand(object source, System.Web.UI.WebControls.DataGridCommandEventArgs e)
        {
            int Id = System.Convert.ToInt32(dgrdForumSubs.DataKeys[e.Item.ItemIndex]);
            new DotNetNuke.Modules.ActiveForums.Controllers.SubscriptionController().Delete(Id);
            Response.Redirect(Request.RawUrl);
        }
        public string GetResourceString(string Key)
        {
            return Utilities.GetSharedResource(Key);
        }
    }
}
