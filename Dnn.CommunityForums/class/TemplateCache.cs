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

using System.Text;
using System.Text.RegularExpressions;
using DotNetNuke.Common.Utilities;

namespace DotNetNuke.Modules.ActiveForums
{
    using System;
    using System.Web;

    internal static class TemplateCache
    {
        internal static string GetCachedTemplate(int moduleId, string templateType)
        {
            return GetCachedTemplate(moduleId, templateType, -1);
        }

        public static string GetCachedTemplate(int moduleId, string TemplateType, int TemplateId)
        {
            string sTemplate = string.Empty;
            string cacheKey = string.Format(CacheKeys.Template, moduleId, TemplateId, TemplateType);
            object obj = null;
            if (SettingsBase.GetModuleSettings(moduleId).CacheTemplates)
            {
                obj = DataCache.SettingsCacheRetrieve(moduleId, cacheKey);
            }

            if (obj != null)
            {
                return Convert.ToString(obj);
            }

            if (TemplateId < 1)
            {
                try
                {
                    string fileName = $"{TemplateType}.ascx";
                    SettingsInfo moduleSettings = SettingsBase.GetModuleSettings(moduleId);
                    string templateFilePathFileName = Utilities.MapPath(moduleSettings.TemplatePath + fileName);
                    if (!System.IO.File.Exists(templateFilePathFileName))
                    {
                        templateFilePathFileName = Utilities.MapPath(Globals.TemplatesPath + fileName);
                        if (!System.IO.File.Exists(templateFilePathFileName))
                        {
                            templateFilePathFileName = Utilities.MapPath(Globals.DefaultTemplatePath + fileName);
                        }
                    }

                    if (System.IO.File.Exists(templateFilePathFileName))
                    {
                        using (System.IO.StreamReader objStreamReader = System.IO.File.OpenText(templateFilePathFileName))
                        {
                            sTemplate = objStreamReader.ReadToEnd();
                        }

                        sTemplate = Utilities.ParseSpacer(sTemplate);
                    }
                }
                catch (Exception ex)
                {
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                    sTemplate = $"ERROR: Loading template {TemplateType} failed";
                }
            }
            else
            {
                TemplateInfo templateInfo = new TemplateController().Template_Get(TemplateId);
                if (templateInfo != null)
                {
                    sTemplate = templateInfo.Template;
                    sTemplate = Utilities.ParseSpacer(sTemplate);
                }
            }

            sTemplate = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.MapLegacyTemplateTokenSynonyms(new StringBuilder(sTemplate)).ToString();

            sTemplate = sTemplate.Replace("[TRESX:", "[RESX:");
            if (sTemplate.ToLowerInvariant().Contains("<dnn:"))
            {
                sTemplate = Globals.DnnControlsRegisterTag + sTemplate;
            }

            if (sTemplate.ToLowerInvariant().Contains("<am:"))
            {
                sTemplate = Globals.ForumsControlsRegisterAMTag + sTemplate;
            }

            if (sTemplate.ToLowerInvariant().Contains("<af:"))
            {
                sTemplate = Globals.ForumsControlsRegisterAFTag + sTemplate;
            }

            if (sTemplate.ToUpperInvariant().Contains("DCF:TEMPLATE-"))
            {
                foreach (Match nestedTemplateToken in RegexUtils.GetCachedRegex(@"\[DCF:TEMPLATE-(?<templateName>.+)\]", RegexOptions.Compiled & RegexOptions.IgnoreCase).Matches(sTemplate))
                {
                    var token = nestedTemplateToken.Value;
                    var nestedTemplateName = nestedTemplateToken.Groups["templateName"]?.Value;
                    if (!string.IsNullOrEmpty(nestedTemplateName))
                    {
                        sTemplate = sTemplate.Replace(token, GetCachedTemplate(moduleId, nestedTemplateName, -1));
                    }
                }
            }

            if (SettingsBase.GetModuleSettings(moduleId).CacheTemplates)
            {
                DataCache.SettingsCacheStore(moduleId, cacheKey, sTemplate);
            }

            return sTemplate;
        }
    }
}
