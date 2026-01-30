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
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web.UI;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Modules.ActiveForums.Entities;

    public class TemplateController
    {
        public void Template_Save(int moduleId, DotNetNuke.Modules.ActiveForums.Entities.TemplateInfo templateInfo)
        {
            // now save to the template file
            try
            {
                string templatePathFileName = Globals.TemplatesPath + templateInfo.FileName;

                ModuleSettings moduleSettings = SettingsBase.GetModuleSettings(moduleId);
                templatePathFileName = moduleSettings.TemplatePath + templateInfo.FileName;
                if (!System.IO.Directory.Exists(Utilities.MapPath(moduleSettings.TemplatePath)))
                {
                    System.IO.Directory.CreateDirectory(Utilities.MapPath(moduleSettings.TemplatePath));
                }

                System.IO.File.WriteAllText(Utilities.MapPath(templatePathFileName), templateInfo.Template);
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
            }
        }

        public void Template_Delete(int moduleId, Enums.TemplateType templateType, string templateFileNameSuffix)
        {
            string fileNameBase = templateType.ToString().ToLowerInvariant();
            ModuleSettings moduleSettings = SettingsBase.GetModuleSettings(moduleId);
            try
            {
                if (!string.IsNullOrEmpty(templateFileNameSuffix))
                {
                    /* look first in templates folder within theme using - (dash) plus suffix */
                    string fileName = $"{fileNameBase}-{templateFileNameSuffix}.ascx";
                    string templateFilePathFileName = Utilities.MapPath(moduleSettings.TemplatePath + fileName);
                    if (System.IO.File.Exists(templateFilePathFileName))
                    {
                        System.IO.File.Delete(templateFilePathFileName);
                    }

                    /* look next in templates folder within theme without - (dash) but with suffix */
                    fileName = $"{fileNameBase}{templateFileNameSuffix}.ascx";
                    templateFilePathFileName = Utilities.MapPath(moduleSettings.TemplatePath + fileName);
                    if (System.IO.File.Exists(templateFilePathFileName))
                    {
                        System.IO.File.Delete(templateFilePathFileName);
                    }
                }
                else
                {
                     /* look in templates folder within theme without suffix at all */
                     string fileName = $"{fileNameBase}.ascx";
                     string templateFilePathFileName = Utilities.MapPath(moduleSettings.TemplatePath + fileName);
                     if (System.IO.File.Exists(templateFilePathFileName))
                     {
                         System.IO.File.Delete(templateFilePathFileName);
                     }
                }
            }
            catch (Exception ex)
            {
            }
        }

        public static string Template_Get(int moduleId, Enums.TemplateType templateType, string templateFileNameSuffix)
        {
            return Template_Get(moduleId, templateType.ToString(), templateFileNameSuffix, null);
        }

        internal static string Template_Get(int moduleId, Enums.TemplateType templateType, string templateFileNameSuffix, ForumUserInfo forumUser)
        {
            return Template_Get(moduleId, templateType.ToString(), templateFileNameSuffix, forumUser);
        }

        private static string Template_Get(int moduleId, string templateBaseFileName, string templateFileNameSuffix, ForumUserInfo forumUser)
        {
            bool fileFound = false;
            string sTemplate = string.Empty;
            string fileName = string.Empty;
            string templateFilePathFileName = string.Empty;
            ModuleSettings moduleSettings = SettingsBase.GetModuleSettings(moduleId);
            templateBaseFileName = templateBaseFileName.ToLowerInvariant();
            bool isAuthenticated = forumUser?.IsAuthenticated ?? false;
            string cacheKey = string.Format(CacheKeys.Template, moduleId, templateBaseFileName, templateFileNameSuffix, isAuthenticated);
            object obj = null;
            if (SettingsBase.GetModuleSettings(moduleId).CacheTemplates)
            {
                obj = DotNetNuke.Modules.ActiveForums.DataCache.SettingsCacheRetrieve(moduleId, cacheKey);
            }

            if (obj != null)
            {
                return Convert.ToString(obj);
            }

            try
            {
                if (!string.IsNullOrEmpty(templateFileNameSuffix))
                {
                    /* look first in templates folder within theme using - (dash) plus suffix */
                    fileName = $"{templateBaseFileName}-{templateFileNameSuffix}.ascx";
                    templateFilePathFileName = Utilities.MapPath(moduleSettings.TemplatePath + fileName);
                    fileFound = System.IO.File.Exists(templateFilePathFileName);

                    if (!fileFound)
                    {
                        /* look next in templates folder within theme without - (dash) but with suffix */
                        fileName = $"{templateBaseFileName}{templateFileNameSuffix}.ascx";
                        templateFilePathFileName = Utilities.MapPath(moduleSettings.TemplatePath + fileName);
                        fileFound = System.IO.File.Exists(templateFilePathFileName);
                    }
                }

                if (!fileFound)
                {
                    /* look in templates folder within theme without suffix at all */
                    fileName = $"{templateBaseFileName}.ascx";
                    templateFilePathFileName = Utilities.MapPath(moduleSettings.TemplatePath + fileName);
                    fileFound = System.IO.File.Exists(templateFilePathFileName);
                    if (!fileFound)
                    {
                        /* look in non-themes templates folder without suffix */
                        templateFilePathFileName = Utilities.MapPath(Globals.TemplatesPath + fileName);
                        fileFound = System.IO.File.Exists(templateFilePathFileName);
                        if (!fileFound)
                        {
                            /* last fallback is delivered templates folder without suffix */
                            templateFilePathFileName = Utilities.MapPath(Globals.DefaultTemplatePath + fileName);
                            fileFound = System.IO.File.Exists(templateFilePathFileName);
                        }
                    }
                }

                if (fileFound)
                {
                    using (var objStreamReader = System.IO.File.OpenText(templateFilePathFileName))
                    {
                        sTemplate = objStreamReader.ReadToEnd();
                    }

                    sTemplate = Utilities.ParseSpacer(sTemplate);
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                sTemplate = $"ERROR: Loading template {templateBaseFileName} failed";
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
                foreach (Match nestedTemplateToken in RegexUtils.GetCachedRegex(@"\[DCF:TEMPLATE-(?<templateName>.[^\]]+)\]", RegexOptions.Compiled & RegexOptions.IgnoreCase).Matches(sTemplate))
                {
                    var token = nestedTemplateToken.Value;
                    var nestedTemplateName = nestedTemplateToken.Groups["templateName"]?.Value;
                    if (!string.IsNullOrEmpty(nestedTemplateName))
                    {
                        sTemplate = sTemplate.Replace(token, Template_Get(moduleId, nestedTemplateName, templateFileNameSuffix, forumUser));
                    }
                }
            }

            sTemplate = HandleAuthenticatedBasedTemplateSection(sTemplate, isAuthenticated);

            if (SettingsBase.GetModuleSettings(moduleId).CacheTemplates)
            {
                DotNetNuke.Modules.ActiveForums.DataCache.SettingsCacheStore(moduleId, cacheKey, sTemplate);
            }

            return sTemplate;
        }

        internal static string HandleAuthenticatedBasedTemplateSection(string sTemplate, bool isAuthenticated)
        {
            if (sTemplate.ToUpperInvariant().Contains("DCF:USERISAUTHENTICATED"))
            {
                if (isAuthenticated)
                {
                    sTemplate = sTemplate.Replace("[DCF:USERISAUTHENTICATED]", string.Empty);
                    sTemplate = sTemplate.Replace("[/DCF:USERISAUTHENTICATED]", string.Empty);
                }
                else
                {
                    sTemplate = TemplateUtils.ReplaceSubSection(sTemplate, string.Empty, "[DCF:USERISAUTHENTICATED]", "[/DCF:USERISAUTHENTICATED]");
                }
            }

            if (sTemplate.ToUpperInvariant().Contains("DCF:USERISNOTAUTHENTICATED"))
            {
                if (!isAuthenticated)
                {
                    sTemplate = sTemplate.Replace("[DCF:USERISNOTAUTHENTICATED]", string.Empty);
                    sTemplate = sTemplate.Replace("[/DCF:USERISNOTAUTHENTICATED]", string.Empty);
                }
                else
                {
                    sTemplate = TemplateUtils.ReplaceSubSection(sTemplate, string.Empty, "[DCF:USERISNOTAUTHENTICATED]", "[/DCF:USERISNOTAUTHENTICATED]");
                }
            }

            return sTemplate;
        }
    }
}
