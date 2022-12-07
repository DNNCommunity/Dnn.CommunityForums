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
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Reflection;
using System.Web;
using System.Web.UI;

namespace DotNetNuke.Modules.ActiveForums
{
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
	    public string TemplateHTML { get; set; }
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
		public int Template_Save(TemplateInfo info)
		{
            // save updated template to database; will return TemplateId which is critical if new template
            int TemplateId = Convert.ToInt32(DataProvider.Instance().Templates_Save(info.TemplateId, info.PortalId, info.ModuleId, (int)info.TemplateType, info.IsSystem, info.Title, info.Subject, info.Template));
            // retrieve the template from the database, which will return the filename but will also get the template text from the file which has not been updated yet
            TemplateInfo TemplateInfo = Template_Get(TemplateId);
            // override the template text with what is being saved
            TemplateInfo.Template = info.Template;
            // now save to the template file
            try
            {
                SettingsInfo MainSettings = DataCache.MainSettings(info.ModuleId);
                System.IO.File.WriteAllText(HttpContext.Current.Server.MapPath (MainSettings.TemplatesLocation + "/" + TemplateInfo.FileName), TemplateInfo.Template);
            }
            catch (Exception exc)
            {

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
			SettingsInfo MainSettings = DataCache.MainSettings(ModuleId);

            string templateFile = HttpContext.Current.Server.MapPath(MainSettings.TemplatesLocation + "/" + templateInfo.FileName);
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
            SettingsInfo MainSettings = DataCache.MainSettings(ModuleId);
            TemplateInfo ti = null;
			foreach (TemplateInfo tiWithinLoop in Template_List(PortalId, ModuleId))
			{
				ti = tiWithinLoop;
				if (TemplateName.ToUpperInvariant() == tiWithinLoop.Title.ToUpperInvariant())
				{
                    ti.Template = Utilities.GetFileContent(HttpContext.Current.Server.MapPath(MainSettings.TemplatesLocation + "/" + ti.FileName));
                    ti.Template = ti.Template.Replace("[TRESX:", "[RESX:");
                    tiWithinLoop.TemplateHTML = GetHTML(tiWithinLoop.Template);
					tiWithinLoop.TemplateText = GetText(tiWithinLoop.Template);
					return tiWithinLoop;
				}
			}
			return ti;
		}
		public TemplateInfo Template_Get(int TemplateId)
		{
			return Template_Get(TemplateId, -1, -1);
		}
		public TemplateInfo Template_Get(int TemplateId, int PortalId, int ModuleId)
		{
            SettingsInfo MainSettings = DataCache.MainSettings(ModuleId);
            var ti = new TemplateInfo();
			IDataReader dr = DataProvider.Instance().Templates_Get(TemplateId, PortalId, ModuleId);
			while (dr.Read())
			{
				try
				{
					ti.TemplateId = Convert.ToInt32(dr["TemplateId"]);
					ti.PortalId = Convert.ToInt32(dr["PortalId"]);
					ti.ModuleId = Convert.ToInt32(dr["ModuleId"]);
					ti.TemplateType = (Templates.TemplateTypes)(Convert.ToInt32(dr["TemplateType"]));
					ti.IsSystem = Convert.ToBoolean(dr["IsSystem"]);
                    ti.Title = Convert.ToString(dr["Title"]);
                    ti.FileName = Convert.ToString(dr["FileName"]);
                    ti.Subject = Convert.ToString(dr["Subject"]);
					ti.Template = Utilities.GetFileContent(HttpContext.Current.Server.MapPath(MainSettings.TemplatesLocation + "/" + ti.FileName));
					if (string.IsNullOrEmpty(ti.Template))
					{
                        ti.Template = Convert.ToString(dr["Template"]);
                    }
                    ti.Template = ti.Template.Replace("[TRESX:", "[RESX:");
                    ti.TemplateHTML = GetHTML(ti.Template);
					ti.TemplateText = GetText(ti.Template);
					ti.DateCreated = Utilities.SafeConvertDateTime(dr["DateCreated"]);
					ti.DateUpdated = Utilities.SafeConvertDateTime(dr["DateUpdated"]);

                }
				catch (Exception ex)
				{
					return null;
				}


			}

            return ti;
		}
#endregion
#region Private Methods
		private List<TemplateInfo> GetTemplateList(int PortalId, int ModuleId, Templates.TemplateTypes TemplateType)
		{
            SettingsInfo MainSettings = DataCache.MainSettings(ModuleId);
            try
            {
				var tl = new List<TemplateInfo>();
			    IDataReader dr = TemplateType == Templates.TemplateTypes.All ? DataProvider.Instance().Templates_List(PortalId, ModuleId, -1) : DataProvider.Instance().Templates_List(PortalId, ModuleId, (int)TemplateType);
				dr.Read();
				dr.NextResult();
                while (dr.Read())
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
                        TemplateType = (Templates.TemplateTypes)(dr["TemplateType"]),
                    };
                    ti.Template = Utilities.GetFileContent(HttpContext.Current.Server.MapPath(MainSettings.TemplatesLocation + "/" + ti.FileName));
                    if (string.IsNullOrEmpty(ti.Template))
                    {
                        ti.Template = Convert.ToString(dr["Template"]);
                    }

                    ti.Template = ti.Template.Replace("[TRESX:", "[RESX:");
                    ti.TemplateHTML = GetHTML(ti.Template);
                    ti.TemplateText = GetText(ti.Template);
                    tl.Add(ti);
                }
                dr.Close();
                return tl;
            }
            catch (Exception ex)
			{
				return null;
			}
		}
        private string GetHTML(string Template)
        {
            try
            {
                if (Template.Contains("<html>"))
                {
                    string sHTML;
                    var xDoc = new System.Xml.XmlDocument();
                    xDoc.LoadXml(Template);
                    System.Xml.XmlNode xNode;
                    System.Xml.XmlNode xRoot = xDoc.DocumentElement;
                    xNode = xRoot.SelectSingleNode("/template/html");
                    sHTML = xNode.InnerText;
                    return sHTML;
                }
                return Template;
            }
            catch (Exception ex)
            {
                return Template;
            }

        }
        private string GetText(string Template)
        {
            try
            {
                if (Template.Contains("<plaintext>"))
                {
                    string sText;
                    var xDoc = new System.Xml.XmlDocument();
                    xDoc.LoadXml(Template);
                    System.Xml.XmlNode xNode;
                    System.Xml.XmlNode xRoot = xDoc.DocumentElement;
                    xNode = xRoot.SelectSingleNode("/template/plaintext");
                    sText = xNode.InnerText;
                    return sText;
                }
                return Template;
            }
            catch (Exception ex)
            {
                return Template;
            }

        }

        #endregion
    }
    #endregion
}
