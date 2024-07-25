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
namespace DotNetNuke.Modules.ActiveForums
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;

    public class UserRolesDictionary
    {
        internal static string GetRoles(int PortalId, int UserId)
        {
            try
            {
                Dictionary<string, string> dict = (Dictionary<string, string>)DataCache.SettingsCacheRetrieve(-1, string.Format(CacheKeys.UserRoles, PortalId));
                return (dict !=null && dict.ContainsKey(UserId.ToString())) ? dict[UserId.ToString()] : string.Empty;
                
            }
            catch
            {
                return string.Empty;
            }
        }

        internal static bool AddRoles(int PortalId, int UserId, string Roles)
        {
            try
            {
                Dictionary<string, string> dict = (Dictionary<string, string>)DataCache.SettingsCacheRetrieve(-1, string.Format(CacheKeys.UserRoles, PortalId));
                if (dict == null)
                {
                    dict = new Dictionary<string, string>();
                }

                if (dict.ContainsKey(UserId.ToString()))
                {
                    dict[UserId.ToString()] = Roles;
                }
                else
                {
                    dict.Add(UserId.ToString(), Roles);
                }
                DataCache.SettingsCacheStore(-1, string.Format(CacheKeys.UserRoles, PortalId), dict, DateTime.UtcNow.AddMinutes(3));
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
