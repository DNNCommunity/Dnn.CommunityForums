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

namespace DotNetNuke.Modules.ActiveForums.Controllers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;

    using DotNetNuke.Collections;

    internal class SettingsController : DotNetNuke.Modules.ActiveForums.Controllers.RepositoryControllerBase<DotNetNuke.Modules.ActiveForums.Entities.SettingsInfo>
    {
        public static void SaveSetting(int moduleId, string settingsKey, string settingName, string settingValue)
        {
            try
            {
                var sc = new DotNetNuke.Modules.ActiveForums.Controllers.SettingsController();
                var setting = sc.GetSettingForModuleIdSettingsKeySettingName(moduleId, settingsKey, settingName);
                if (setting == null)
                {
                    setting = new DotNetNuke.Modules.ActiveForums.Entities.SettingsInfo
                                  {
                                      ModuleId = moduleId,
                                      SettingsKey = settingsKey,
                                      SettingName = settingName,
                                      SettingValue = settingValue,
                                  };
                    sc.Insert(setting);
                }
                else
                {
                    setting.SettingValue = settingValue;
                    sc.Update(setting);
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Modules.ActiveForums.Exceptions.LogException(ex);
            }
        }

        public IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.SettingsInfo> GetSettingsForModuleIdSettingsKey(int moduleID, string settingsKey)
        {
            return this.Find("WHERE ModuleId = @0 AND SettingsKey = @1", moduleID, settingsKey);
        }

        public Hashtable GetSettingsHashTableForModuleIdSettingsKey(int moduleId, string settingsKey)
        {
            var ht = new Hashtable();
            this.GetSettingsForModuleIdSettingsKey(moduleId, settingsKey).ForEach(s => ht.Add(s.SettingName, s.SettingValue));
            return ht;
        }

        public DotNetNuke.Modules.ActiveForums.Entities.SettingsInfo GetSettingForModuleIdSettingsKeySettingName(int moduleID, string settingsKey, string settingName)
        {
            return this.Find("WHERE ModuleId = @0 AND SettingsKey = @1 AND SettingName = @2", moduleID, settingsKey, settingName).FirstOrDefault();
        }

        public void DeleteForModuleIdSettingsKey(int moduleId, string settingsKey)
        {
            this.Delete("WHERE ModuleId = @0 AND SettingsKey = @1", moduleId, settingsKey);
        }

        public void DeleteForModuleIdSettingsKeySettingName(int moduleId, string settingsKey, string settingName)
        {
            this.Delete("WHERE ModuleId = @0 AND SettingsKey = @1 AND SettingName = @2", moduleId, settingsKey, settingName);
        }
    }
}
