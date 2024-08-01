// Copyright (c) 2013-2024 by DNN Community
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
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;

    using DotNetNuke;

    public partial class af_usersonline : ForumBase
    {
        #region Public Members
        public string DisplayMode;
        public int pid = 0;

        #endregion
        #region Private Members

        #endregion
        #region Event Handlers
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                this.Page.ClientScript.RegisterStartupScript(this.Page.GetType(), "amaf_uo", "setInterval('amaf_uo(" + this.ModuleId.ToString() + ")',50000);", true);
                this.BindUsersOnline();
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.ProcessModuleLoadException(this, ex);
            }
        }
        #endregion

        #region Private Methods
        private void BindUsersOnline()
        {
            var user = new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController().GetByUserId(this.PortalId, this.UserInfo.UserID);
            string sOnlineList = new DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController().GetUsersOnline(this.PortalSettings, this.ForumModuleId, user);
            IDataReader dr = DataProvider.Instance().Profiles_GetStats(this.PortalId, 2);
            int anonCount = 0;
            int memCount = 0;
            int memTotal = 0;
            while (dr.Read())
            {
                anonCount = Convert.ToInt32(dr["Guests"]);
                memCount = Convert.ToInt32(dr["Members"]);
                memTotal = Convert.ToInt32(dr["MembersTotal"]);
            }

            dr.Close();
            string sGuestsOnline = null;
            string sUsersOnline = null;
            sGuestsOnline = Utilities.GetSharedResource("[RESX:GuestsOnline]");
            sUsersOnline = Utilities.GetSharedResource("[RESX:UsersOnline]");
            this.litGuestsOnline.Text = sGuestsOnline.Replace("[GUESTCOUNT]", anonCount.ToString());
            sUsersOnline = sUsersOnline.Replace("[USERCOUNT]", memCount.ToString());
            sUsersOnline = sUsersOnline.Replace("[TOTALMEMBERCOUNT]", memTotal.ToString());
            this.litUsersOnline.Text = sUsersOnline + " " + sOnlineList;
        }

        #endregion

    }
}
