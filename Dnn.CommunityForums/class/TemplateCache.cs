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
using System.Data;

using System.Web;
namespace DotNetNuke.Modules.ActiveForums 
{
    internal class TemplateCache
{
    #region Cache Retrieval
    public static string GetCachedTemplate(int ModuleId, string TemplateType, int TemplateId)
    {
        string sTemplate = GetTemplateFromMemory(ModuleId, TemplateType, TemplateId);
        sTemplate = sTemplate.Replace("[TOOLBAR]", string.Empty);
        sTemplate = sTemplate.Replace("[TEMPLATE:TOOLBAR]", string.Empty);
        sTemplate = sTemplate.Replace("[TRESX:", "[RESX:");

        return sTemplate;
    }
    private static string GetTemplateFromMemory(int ModuleId, string TemplateType, int TemplateId)
    {
        string sTemplate = string.Empty;
        string cacheKey = string.Format(CacheKeys.Template, ModuleId, TemplateId, TemplateType);
        object obj = null;
        if (!SettingsBase.GetModuleSettings(ModuleId).CacheTemplates)
        {
            obj = DataCache.SettingsCacheRetrieve(ModuleId, cacheKey);
        }
        if (obj == null)
        {
            if (TemplateId == 0)
            {
                try
                {
                    string myFile = HttpContext.Current.Server.MapPath(Globals.DefaultTemplatePath + TemplateType + ".txt");
                    if (System.IO.File.Exists(myFile))
                    {
                        System.IO.StreamReader objStreamReader = null;
                        try
                        {
                            objStreamReader = System.IO.File.OpenText(myFile);
                        }
                        catch (Exception ex)
                        {
                            DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                        }
                        sTemplate = objStreamReader.ReadToEnd();
                        objStreamReader.Close();
                        sTemplate = Utilities.ParseSpacer(sTemplate);
                        DataCache.SettingsCacheStore(ModuleId, cacheKey: cacheKey, cacheObj: sTemplate);
                    }
                }
                catch (Exception ex)
                {
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                }
            }
            else
            {
                sTemplate = GetTemplate(TemplateId, TemplateType);
                DataCache.ContentCacheStore(ModuleId, cacheKey, sTemplate);
            }
        }
        else
        {
            sTemplate = Convert.ToString(obj);
        }
        return sTemplate;
    }

        private static string GetTemplate(int TemplateId, string TemplateType)
    {
        string sOut = string.Empty;
        try
        {
            if (TemplateId == 0)
            {
                try
                {
                        string myFile = HttpContext.Current.Server.MapPath(Globals.TemplatesPath + TemplateType + ".txt");
                    if (System.IO.File.Exists(myFile))
                    {
                        System.IO.StreamReader objStreamReader = null;
                        try
                        {
                            objStreamReader = System.IO.File.OpenText(myFile);
                        }
                        catch (Exception ex)
                        {
                            DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                        }
                        sOut = objStreamReader.ReadToEnd();
                        objStreamReader.Close();
                        sOut = Utilities.ParseSpacer(sOut);
                    }
                }
                catch (Exception ex)
                {
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                }
            }
            else
            {
                var objTemplates = new TemplateController();
                TemplateInfo objTempInfo = objTemplates.Template_Get(TemplateId);
                if (objTempInfo != null)
                {
                    sOut = objTempInfo.TemplateHTML;
                    sOut = Utilities.ParseSpacer(sOut);
                }
            }

        }
        catch (Exception ex)
        {
            DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            sOut = "ERROR: Loading template failed";
        }
        return sOut;
    }
    internal static string GetTemplate(string TemplateFileName)
    {
        string sOut = string.Empty;
        string myFile;
        try
        {
            try
            {
                    myFile = HttpContext.Current.Server.MapPath(Globals.TemplatesPath + TemplateFileName);
                if (System.IO.File.Exists(myFile))
                {
                    System.IO.StreamReader objStreamReader = null;
                    try
                    {
                        objStreamReader = System.IO.File.OpenText(myFile);
                    }
                    catch (Exception ex)
                    {
                        DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                    }
                    sOut = objStreamReader.ReadToEnd();
                    objStreamReader.Close();
                    sOut = Utilities.ParseSpacer(sOut);
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }

        }
        catch (Exception ex)
        {
            DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            sOut = "ERROR: Loading template failed";
        }
        return sOut;
    }
    #endregion

}
}
