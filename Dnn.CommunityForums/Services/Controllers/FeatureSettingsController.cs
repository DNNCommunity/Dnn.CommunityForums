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

namespace DotNetNuke.Modules.ActiveForums.Services.Controllers
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Web.Http;

    using DotNetNuke.Web.Api;

    /// <summary>
    /// Web API controller for managing feature settings.
    /// </summary>
    public class FeatureSettingsController : ControllerBase<DotNetNuke.Modules.ActiveForums.Services.Controllers.FeatureSettingsController>
    {
        /// <summary>
        /// Gets all settings for a given settings key.
        /// </summary>
        /// <param name="settingsKey">settingsKey</param>
        /// <returns>settings</returns>
        [HttpGet]
        [DnnAuthorize]
        public IHttpActionResult GetAllSettings(string settingsKey)
        {
            try
            {
                var settings = new DotNetNuke.Modules.ActiveForums.Entities.FeatureSettings(this.ForumModuleId, settingsKey);
                return this.Ok(settings.featureSettings);
            }
            catch (Exception ex)
            {
                return this.InternalServerError(ex);
            }
        }

        /// <summary>
        /// Gets a single setting value by key.
        /// </summary>
        /// <param name="settingsKey">settingsKey</param>
        /// <param name="key">key</param>
        /// <returns>setting</returns>
        [HttpGet]
        [DnnAuthorize]
        public IHttpActionResult GetSetting(string settingsKey, string key)
        {
            try
            {
                var settings = new DotNetNuke.Modules.ActiveForums.Entities.FeatureSettings(this.ForumModuleId, settingsKey);
                if (!settings.featureSettings.ContainsKey(key))
                {
                    return this.NotFound();
                }
                return Ok(settings.featureSettings[key]);
            }
            catch (Exception ex)
            {
                return this.InternalServerError(ex);
            }
        }

        /// <summary>
        /// Creates or updates a single setting.
        /// </summary>
        /// <param name="settingsKey">settingsKey</param>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        /// <returns>HTTP status</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(StaticRoles = "Administrators")]
        public IHttpActionResult SetSetting(string settingsKey, string key, [FromBody] string value)
        {
            try
            {
                var settings = new DotNetNuke.Modules.ActiveForums.Entities.FeatureSettings(this.ForumModuleId, settingsKey);
                settings.featureSettings[key] = value;
                DotNetNuke.Modules.ActiveForums.Entities.FeatureSettings.Save(this.ForumModuleId, settingsKey, settings);
                return this.Ok();
            }
            catch (Exception ex)
            {
                return this.InternalServerError(ex);
            }
        }

        /// <summary>
        /// Deletes a single setting.
        /// </summary>
        /// <param name="settingsKey">settingsKey</param>
        /// <param name="key">key</param>
        /// <returns>HTTP status</returns>
        [HttpDelete]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(StaticRoles = "Administrators")]
        public IHttpActionResult DeleteSetting(string settingsKey, string key)
        {
            try
            {
                var settings = new DotNetNuke.Modules.ActiveForums.Entities.FeatureSettings(this.ForumModuleId, settingsKey);
                if (!settings.featureSettings.ContainsKey(key))
                {
                    return this.NotFound();
                }
                settings.featureSettings.Remove(key);
                DotNetNuke.Modules.ActiveForums.Entities.FeatureSettings.Save(this.ForumModuleId, settingsKey, settings);
                return this.Ok();
            }
            catch (Exception ex)
            {
                return this.InternalServerError(ex);
            }
        }

        /// <summary>
        /// Updates all settings for a given module and settings key.
        /// </summary>
        /// <param name="settingsKey">settingsKey</param>
        /// <param name="newSettings">newSettings</param>
        /// <returns>HTTP status</returns>
        [HttpPut]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(StaticRoles = "Administrators")]
        public IHttpActionResult UpdateAllSettings(string settingsKey, [FromBody] Hashtable newSettings)
        {
            try
            {
                var settings = new DotNetNuke.Modules.ActiveForums.Entities.FeatureSettings(newSettings);
                DotNetNuke.Modules.ActiveForums.Entities.FeatureSettings.Save(this.ForumModuleId, settingsKey, settings);
                return this.Ok();
            }
            catch (Exception ex)
            {
                return this.InternalServerError(ex);
            }
        }

        /// <summary>
        /// Deletes all settings for a given settings key.
        /// </summary>
        /// <param name="settingsKey">settingsKey</param>
        /// <returns>HTTP status</returns>
        [HttpDelete]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(StaticRoles = "Administrators")]
        public IHttpActionResult DeleteAllSettings(string settingsKey)
        {
            try
            {
                var settings = new DotNetNuke.Modules.ActiveForums.Entities.FeatureSettings(this.ForumModuleId, settingsKey);
                foreach (DictionaryEntry entry in settings.featureSettings)
                {
                    new DotNetNuke.Modules.ActiveForums.Controllers.SettingsController().DeleteForModuleIdSettingsKeySettingName(this.ForumModuleId, settingsKey, (string)entry.Key);
                }

                return this.Ok();
            }
            catch (Exception ex)
            {
                return this.InternalServerError(ex);
            }
        }
    }
}
