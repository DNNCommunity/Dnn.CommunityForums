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
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Reflection;
    using System.Web;

#region Templates
    public class Templates
    {
        public enum TemplateTypes: int
        {
            All, //0
            System, //1
            ForumView, //2
            TopicView, //3
            TopicsView, //4
            TopicForm, //5
            ReplyForm, //6
            QuickReplyForm, //7
            Email, //8
            Profile, //9
            ModEmail, //10
            PostInfo //11
        }

    }

#endregion
#region TemplateInfo
    public class TemplateInfo
    {
#region Private Members

        #endregion
#region Public Properties

        public int TemplateId { get; set; }

        public int PortalId { get; set; }

        public int ModuleId { get; set; }

        public Templates.TemplateTypes TemplateType { get; set; }

        public bool IsSystem { get; set; }

        public string Subject { get; set; }

        public string Title { get; set; }

        public string Template { get; set; }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use Template property.")]
        public string TemplateHTML { get; set; }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use Template property.")]
        public string TemplateText { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime DateUpdated { get; set; }

        public string FileName { get; set; }

        #endregion

    }

#endregion
#region Template Controller
    public class TemplateController
    {
#region Public Methods
        //'<summary>
        //'Function to save template.</summary>
        //'<param name="info">TemplateInfo object</param>
        public int Template_Save(TemplateInfo templateInfo)
        {
            // save updated template to database; will return TemplateId which is critical if new template
            int TemplateId = Convert.ToInt32(DataProvider.Instance().Templates_Save(templateInfo.TemplateId, templateInfo.PortalId, templateInfo.ModuleId, (int)templateInfo.TemplateType, templateInfo.IsSystem, templateInfo.Title, templateInfo.Subject, templateInfo.Template));
            // retrieve the template from the database, which will return the filename but will also get the template text from the file which has not been updated yet
            TemplateInfo TemplateInfo = Template_Get(TemplateId);
            // override the template text with what is being saved
            TemplateInfo.Template = templateInfo.Template;
            // now save to the template file
            try
            {
                string templatePathFileName = Globals.TemplatesPath + TemplateInfo.FileName;
                if (templateInfo.ModuleId > 0)
                {
                    SettingsInfo moduleSettings = SettingsBase.GetModuleSettings(templateInfo.ModuleId);
                    templatePathFileName = moduleSettings.TemplatePath + TemplateInfo.FileName;
                    if (!System.IO.Directory.Exists(Utilities.MapPath(moduleSettings.TemplatePath)))
                    {
                        System.IO.Directory.CreateDirectory(Utilities.MapPath(moduleSettings.TemplatePath));
                    }
                }
                else
                {
                    if (!System.IO.Directory.Exists(Utilities.MapPath(Globals.TemplatesPath)))
                    {
                        System.IO.Directory.CreateDirectory(Utilities.MapPath(Globals.TemplatesPath));
                    }
                }
                System.IO.File.WriteAllText(Utilities.MapPath(templatePathFileName), TemplateInfo.Template);
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
            }

            return TemplateId;
        }

        public List<TemplateInfo> Template_List(int PortalId, int ModuleId)
        {
            return GetTemplateList(PortalId, ModuleId, Templates.TemplateTypes.All);
        }

        public List<TemplateInfo> Template_List(int PortalId, int ModuleId, Templates.TemplateTypes TemplateType)
        {
            return GetTemplateList(PortalId, ModuleId, TemplateType);
        }

        public void Template_Delete(int TemplateId, int PortalId, int ModuleId)
        {

            TemplateInfo templateInfo = Template_Get(TemplateId);
            SettingsInfo moduleSettings = SettingsBase.GetModuleSettings(templateInfo.ModuleId);
            string templateFile = Utilities.MapPath(moduleSettings.TemplatePath + templateInfo.FileName);
            try
            {
                if (System.IO.File.Exists(templateFile))
                {
                    System.IO.File.Delete(templateFile);
                }
            }
            catch (Exception ex)
            {
            }
            DataProvider.Instance().Templates_Delete(TemplateId, PortalId, ModuleId);
        }

        public TemplateInfo Template_Get(string TemplateName, int PortalId, int ModuleId)
        {
            string templateFileName = string.Empty;
            string templateFilePathFileName = string.Empty;
            TemplateInfo ti = Template_List(PortalId, ModuleId).Where(t => t.Title.ToUpperInvariant() == TemplateName.ToUpperInvariant() && t.ModuleId == ModuleId).FirstOrDefault();

            if (ti != null && !string.IsNullOrEmpty(ti.FileName))
            {
                templateFileName = ti.FileName;
            }
            else
            {
                templateFileName = TemplateName;
                ti = new TemplateInfo { PortalId = PortalId, ModuleId = ModuleId, FileName = TemplateName, Template = string.Empty };
            }
            templateFilePathFileName = Utilities.MapPath(SettingsBase.GetModuleSettings(ModuleId).TemplatePath + templateFileName);
            if (!System.IO.File.Exists(templateFilePathFileName))
            {
                templateFilePathFileName = Utilities.MapPath(Globals.TemplatesPath + templateFileName);
                if (!System.IO.File.Exists(templateFilePathFileName))
                {
                    templateFilePathFileName = Utilities.MapPath(Globals.DefaultTemplatePath + templateFileName);
                }
            }
            ti.Template = Utilities.GetFileContent(templateFilePathFileName).Replace("[TRESX:", "[RESX:");
            return ti;
        }

        public TemplateInfo Template_Get(int TemplateId)
        {
            var ti = new TemplateInfo();
            try
            {
                using (IDataReader dr = DataProvider.Instance().Templates_Get(TemplateId, -1, -1))
                {
                    while (dr.Read())
                    {
                        ti = FillTemplateInfo(dr);
                    }
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
            return ti;
        }
        #endregion

        #region Private Methods
        private List<TemplateInfo> GetTemplateList(int PortalId, int ModuleId, Templates.TemplateTypes TemplateType)
        {
            var tl = new List<TemplateInfo>();
            try
            {
                using (IDataReader dr = TemplateType == Templates.TemplateTypes.All ? DataProvider.Instance().Templates_List(PortalId, ModuleId, -1) : DataProvider.Instance().Templates_List(PortalId, ModuleId, (int)TemplateType))
                {
                    dr.Read();
                    dr.NextResult();
                    while (dr.Read())
                    {
                        TemplateInfo ti = FillTemplateInfo(dr);
                        tl.Add(ti);
                    }
                    dr.Close();
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
            return tl;
        }

        private static TemplateInfo FillTemplateInfo(IDataReader dr)
        {
            try
            {
                var ti = new TemplateInfo
                {
                    TemplateId = Convert.ToInt32(dr["TemplateId"]),
                    DateCreated = Utilities.SafeConvertDateTime(dr["DateCreated"]),
                    DateUpdated = Utilities.SafeConvertDateTime(dr["DateUpdated"]),
                    IsSystem = Convert.ToBoolean(dr["IsSystem"]),
                    ModuleId = Convert.ToInt32(dr["ModuleID"]),
                    PortalId = Convert.ToInt32(dr["PortalId"]),
                    Subject = Convert.ToString(dr["Subject"]),
                    Title = Convert.ToString(dr["Title"]),
                    FileName = Convert.ToString(dr["FileName"]),
                    TemplateType = (Templates.TemplateTypes)dr["TemplateType"],
                };
                SettingsInfo moduleSettings = SettingsBase.GetModuleSettings(ti.ModuleId);
                string templateFilePathFileName = Utilities.MapPath(moduleSettings.TemplatePath + ti.FileName);
                if (!System.IO.File.Exists(templateFilePathFileName))
                {
                    templateFilePathFileName = Utilities.MapPath(Globals.TemplatesPath + ti.FileName);
                    if (!System.IO.File.Exists(templateFilePathFileName))
                    {
                        templateFilePathFileName = Utilities.MapPath(Globals.DefaultTemplatePath + ti.FileName);
                    }
                }
                ti.Template = Utilities.GetFileContent(templateFilePathFileName);
                if (string.IsNullOrEmpty(ti.Template))
                {
                    ti.Template = Convert.ToString(dr["Template"]);
                }
                ti.Template = ti.Template.Replace("[TRESX:", "[RESX:");
                return ti;
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                return null;
            }
        }
        #endregion
    }
    #endregion
}
