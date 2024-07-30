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

    public class UserRolesDictionary
    {
        internal static string GetRoles(int portalId, int userId)
        {
            try
            {
                Dictionary<string, string> dict = (Dictionary<string, string>)DataCache.SettingsCacheRetrieve(-1, string.Format(CacheKeys.UserRoles, portalId));
                return (dict != null && dict.ContainsKey(userId.ToString())) ? dict[userId.ToString()] : string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        internal static bool AddRoles(int portalId, int userId, string roles)
        {
            try
            {
                Dictionary<string, string> dict = (Dictionary<string, string>)DataCache.SettingsCacheRetrieve(-1, string.Format(CacheKeys.UserRoles, portalId));
                if (dict == null)
                {
                    dict = new Dictionary<string, string>();
                }

                if (dict.ContainsKey(userId.ToString()))
                {
                    dict[userId.ToString()] = roles;
                }
                else
                {
                    dict.Add(userId.ToString(), roles);
                }

                DataCache.SettingsCacheStore(-1, string.Format(CacheKeys.UserRoles, portalId), dict, DateTime.UtcNow.AddMinutes(3));
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
