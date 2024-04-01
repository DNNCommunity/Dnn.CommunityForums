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

using DotNetNuke.Collections;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Modules.ActiveForums.Data;
using Microsoft.ApplicationBlocks.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Http.Results;
using System.Web.Profile;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Data;
using System.Reflection;
using DotNetNuke.Instrumentation;
using System.Web.UI;

namespace DotNetNuke.Modules.ActiveForums
{
	public class ForumsConfig
	{
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ForumsConfig));

        public string sPath = DotNetNuke.Modules.ActiveForums.Utilities.MapPath(string.Concat(Globals.ModulePath, "config/defaultsetup.config"));
		public bool ForumsInit(int PortalId, int ModuleId)
		{
			try
			{
				//Initial Settings
				LoadSettings(PortalId, ModuleId);
				//Add Default Templates
				LoadTemplates(PortalId, ModuleId);
				//Add Default Status
				LoadFilters(PortalId, ModuleId);
				//Add Default Steps
				LoadRanks(PortalId, ModuleId);
				//Add Default Forums
				LoadDefaultForums(PortalId, ModuleId);

				Install_Or_Upgrade_MoveTemplates();

                // templates are loaded; map new forumview template id
                UpdateForumViewTemplateId(PortalId, ModuleId);

                return true;
			}
			catch (Exception ex)
			{
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
				return false;
			}
		}
		private void LoadSettings(int PortalId, int ModuleId)
		{
			try
			{
				var objModules = new DotNetNuke.Entities.Modules.ModuleController();
				var xDoc = new System.Xml.XmlDocument();
				xDoc.Load(sPath);
				if (xDoc != null)
				{

					System.Xml.XmlNode xRoot = xDoc.DocumentElement;
					System.Xml.XmlNodeList xNodeList = xRoot.SelectNodes("//mainsettings/setting");
					if (xNodeList.Count > 0)
					{
						int i;
						for (i = 0; i < xNodeList.Count; i++)
						{
							objModules.UpdateModuleSetting(ModuleId, xNodeList[i].Attributes["name"].Value, xNodeList[i].Attributes["value"].Value);

						}
					}
				}
				objModules.UpdateModuleSetting(ModuleId, SettingKeys.IsInstalled, "True");
				objModules.UpdateModuleSetting(ModuleId, "NeedsConvert", "False");
				try
				{
					System.Globalization.DateTimeFormatInfo nfi = new System.Globalization.CultureInfo("en-US", true).DateTimeFormat;


					objModules.UpdateModuleSetting(ModuleId, SettingKeys.InstallDate, DateTime.UtcNow.ToString(new System.Globalization.CultureInfo("en-US")));
				}
				catch (Exception ex)
				{
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
				}
			}
			catch (Exception ex)
			{

			}
		}
		private void LoadTemplates(int PortalId, int ModuleId)
		{
			try
			{
				var xDoc = new System.Xml.XmlDocument();
				xDoc.Load(sPath);
				if (xDoc != null)
				{
					System.Xml.XmlNode xRoot = xDoc.DocumentElement;
					System.Xml.XmlNodeList xNodeList = xRoot.SelectNodes("//templates/template");
					if (xNodeList.Count > 0)
					{
						var tc = new TemplateController();
						int i;
						for (i = 0; i < xNodeList.Count; i++)
						{
							var ti = new TemplateInfo
							{
								TemplateId = -1,
								TemplateType =
												 (Templates.TemplateTypes)
												 Enum.Parse(typeof(Templates.TemplateTypes), xNodeList[i].Attributes["templatetype"].Value),
								IsSystem = true,
								PortalId = PortalId,
								ModuleId = ModuleId,
								Title = xNodeList[i].Attributes["templatetitle"].Value,
								Subject = xNodeList[i].Attributes["templatesubject"].Value,
								Template = Utilities.GetFileContent(xNodeList[i].Attributes["templatefile"].Value)
	                        };
							tc.Template_Save(ti);
						}
					}
				}
			}
			catch (Exception ex)
			{
			}
		}

		private void LoadFilters(int PortalId, int ModuleId)
        {
            DotNetNuke.Modules.ActiveForums.Controllers.FilterController.ImportFilter(PortalId, ModuleId);
        }

		private void LoadRanks(int PortalId, int ModuleId)
		{
			try
			{
				var xDoc = new System.Xml.XmlDocument();
				xDoc.Load(sPath);
				if (xDoc != null)
				{
					System.Xml.XmlNode xRoot = xDoc.DocumentElement;
					System.Xml.XmlNodeList xNodeList = xRoot.SelectNodes("//ranks/rank");
					if (xNodeList.Count > 0)
					{
						int i;
						for (i = 0; i < xNodeList.Count; i++)
						{
							DataProvider.Instance().Ranks_Save(PortalId, ModuleId, -1, xNodeList[i].Attributes["rankname"].Value, Convert.ToInt32(xNodeList[i].Attributes["rankmin"].Value), Convert.ToInt32(xNodeList[i].Attributes["rankmax"].Value), xNodeList[i].Attributes["rankimage"].Value);
						}
					}
				}
			}
			catch 
			{
				// do nothing? 
			}
		}

		private void LoadDefaultForums(int PortalId, int ModuleId)
		{
			var xDoc = new System.Xml.XmlDocument();
			xDoc.Load(sPath);
			if (xDoc != null)
			{
				System.Xml.XmlNode xRoot = xDoc.DocumentElement;
				System.Xml.XmlNodeList xNodeList = xRoot.SelectNodes("//defaultforums/groups/group");
				if (xNodeList.Count > 0)
				{

                    // since templates are loaded, get template ids and attach to forum settings
                    var tc = new TemplateController();
                    int ProfileInfoTemplateId = tc.Template_Get(TemplateName: "ProfileInfo", PortalId: PortalId, ModuleId: ModuleId).TemplateId;
                    int ReplyEditorTemplateId = tc.Template_Get(TemplateName: "ReplyEditor", PortalId: PortalId, ModuleId: ModuleId).TemplateId;
                    int QuickReplyTemplateId = tc.Template_Get(TemplateName: "QuickReply", PortalId: PortalId, ModuleId: ModuleId).TemplateId;
                    int TopicEditorTemplateId = tc.Template_Get(TemplateName: "TopicEditor", PortalId: PortalId, ModuleId: ModuleId).TemplateId;
                    int TopicsViewTemplateId = tc.Template_Get(TemplateName: "TopicsView", PortalId: PortalId, ModuleId: ModuleId).TemplateId;
                    int TopicViewTemplateId = tc.Template_Get(TemplateName: "TopicView", PortalId: PortalId, ModuleId: ModuleId).TemplateId;

                    for (int i = 0; i < xNodeList.Count; i++)
					{
						var gi = new DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo
						             {
						                 ModuleId = ModuleId,
						                 ForumGroupId = -1,
						                 GroupName = xNodeList[i].Attributes["groupname"].Value,
						                 PrefixURL = xNodeList[i].Attributes["prefixurl"].Value,
						                 Active = xNodeList[i].Attributes["active"].Value == "1",
						                 Hidden = xNodeList[i].Attributes["hidden"].Value == "1",
						                 SortOrder = i,
						                 GroupSettingsKey = string.Empty,
						                 PermissionsId = -1
						             };
					    var gc = new DotNetNuke.Modules.ActiveForums.Controllers.ForumGroupController();
						int groupId = gc.Groups_Save(PortalId, gi, true);
						gi = gc.GetById(groupId, ModuleId);
						string sKey = string.Concat("G:", groupId.ToString());
						string sAllowHTML = "false";
						if (xNodeList[i].Attributes["allowhtml"] != null)
						{
							sAllowHTML = xNodeList[i].Attributes["allowhtml"].Value;
                        }
                        Settings.SaveSetting(ModuleId, sKey, ForumSettingKeys.TopicsTemplateId, Convert.ToString(TopicsViewTemplateId));
                        Settings.SaveSetting(ModuleId, sKey, ForumSettingKeys.TopicTemplateId, Convert.ToString(TopicViewTemplateId));
						Settings.SaveSetting(ModuleId, sKey, ForumSettingKeys.EmailAddress, string.Empty);
						Settings.SaveSetting(ModuleId, sKey, ForumSettingKeys.UseFilter, "true");
						Settings.SaveSetting(ModuleId, sKey, ForumSettingKeys.AllowPostIcon, "true");
                        Settings.SaveSetting(ModuleId, sKey, ForumSettingKeys.AllowLikes, "true");
                        Settings.SaveSetting(ModuleId, sKey, ForumSettingKeys.AllowEmoticons, "true");
                        Settings.SaveSetting(ModuleId, sKey, ForumSettingKeys.AllowScript, "false");
						Settings.SaveSetting(ModuleId, sKey, ForumSettingKeys.IndexContent, "true");
						Settings.SaveSetting(ModuleId, sKey, ForumSettingKeys.AllowRSS, "true");
						Settings.SaveSetting(ModuleId, sKey, ForumSettingKeys.AllowAttach, "true");
						Settings.SaveSetting(ModuleId, sKey, ForumSettingKeys.AttachCount, "3");
						Settings.SaveSetting(ModuleId, sKey, ForumSettingKeys.AttachMaxSize, "1000");
						Settings.SaveSetting(ModuleId, sKey, ForumSettingKeys.AttachTypeAllowed, "txt,tiff,pdf,xls,xlsx,doc,docx,ppt,pptx");
						//Settings.SaveSetting(ModuleId, sKey, ForumSettingKeys.AttachStore, "FILESYSTEM");
						Settings.SaveSetting(ModuleId, sKey, ForumSettingKeys.AttachMaxHeight, "450");
						Settings.SaveSetting(ModuleId, sKey, ForumSettingKeys.AttachMaxWidth, "450");
						Settings.SaveSetting(ModuleId, sKey, ForumSettingKeys.AttachAllowBrowseSite, "true");
						Settings.SaveSetting(ModuleId, sKey, ForumSettingKeys.MaxAttachHeight, "800");
						Settings.SaveSetting(ModuleId, sKey, ForumSettingKeys.MaxAttachWidth, "800");
						Settings.SaveSetting(ModuleId, sKey, ForumSettingKeys.AttachInsertAllowed, "false");
						Settings.SaveSetting(ModuleId, sKey, ForumSettingKeys.ConvertingToJpegAllowed, "false");
						Settings.SaveSetting(ModuleId, sKey, ForumSettingKeys.AllowHTML, sAllowHTML);
						if (sAllowHTML.ToLowerInvariant() == "true")
						{
							Settings.SaveSetting(ModuleId, sKey, ForumSettingKeys.EditorType, ((int)EditorTypes.HTMLEDITORPROVIDER).ToString());
							Settings.SaveSetting(ModuleId, sKey, ForumSettingKeys.EditorMobile, ((int)EditorTypes.HTMLEDITORPROVIDER).ToString());
						}
						else
						{
							Settings.SaveSetting(ModuleId, sKey, ForumSettingKeys.EditorType, ((int)EditorTypes.TEXTBOX).ToString());
							Settings.SaveSetting(ModuleId, sKey, ForumSettingKeys.EditorMobile, ((int)EditorTypes.TEXTBOX).ToString());
						}

						Settings.SaveSetting(ModuleId, sKey, ForumSettingKeys.EditorHeight, "350");
						Settings.SaveSetting(ModuleId, sKey, ForumSettingKeys.EditorWidth, "99%");
						Settings.SaveSetting(ModuleId, sKey, ForumSettingKeys.EditorToolbar, "bold,italic,underline,quote");
						Settings.SaveSetting(ModuleId, sKey, ForumSettingKeys.EditorStyle, "2");
						Settings.SaveSetting(ModuleId, sKey, ForumSettingKeys.TopicFormId, Convert.ToString(TopicEditorTemplateId));
						Settings.SaveSetting(ModuleId, sKey, ForumSettingKeys.ReplyFormId, Convert.ToString(ReplyEditorTemplateId));
                        Settings.SaveSetting(ModuleId, sKey, ForumSettingKeys.QuickReplyFormId, Convert.ToString(QuickReplyTemplateId));
                        Settings.SaveSetting(ModuleId, sKey, ForumSettingKeys.ProfileTemplateId, Convert.ToString(ProfileInfoTemplateId));
						Settings.SaveSetting(ModuleId, sKey, ForumSettingKeys.IsModerated, "false");
						Settings.SaveSetting(ModuleId, sKey, ForumSettingKeys.DefaultTrustLevel, "0");
						Settings.SaveSetting(ModuleId, sKey, ForumSettingKeys.AutoTrustLevel, "0");
						Settings.SaveSetting(ModuleId, sKey, ForumSettingKeys.ModApproveTemplateId, "0");
						Settings.SaveSetting(ModuleId, sKey, ForumSettingKeys.ModRejectTemplateId, "0");
						Settings.SaveSetting(ModuleId, sKey, ForumSettingKeys.ModMoveTemplateId, "0");
						Settings.SaveSetting(ModuleId, sKey, ForumSettingKeys.ModDeleteTemplateId, "0");
						Settings.SaveSetting(ModuleId, sKey, ForumSettingKeys.ModNotifyTemplateId, "0");
						if (groupId != -1)
						{
							if (xNodeList[i].HasChildNodes)
							{
								System.Xml.XmlNodeList cNodes = xNodeList[i].ChildNodes;
								for (int c = 0; c < cNodes.Count; c++)
								{
									var fi = new DotNetNuke.Modules.ActiveForums.Entities.ForumInfo();
									fi.ForumID = -1;
									fi.ModuleId = ModuleId;
									fi.ForumGroupId = groupId;
									fi.ParentForumId = 0;
									fi.ForumName = cNodes[c].Attributes["forumname"].Value;
									fi.ForumDesc = cNodes[c].Attributes["forumdesc"].Value;
									fi.PrefixURL = cNodes[c].Attributes["prefixurl"].Value;
									fi.ForumSettingsKey = string.Concat("G:", groupId.ToString());
									fi.Active = cNodes[c].Attributes["active"].Value == "1";
									fi.Hidden = cNodes[c].Attributes["hidden"].Value == "1";
									fi.SortOrder = c;
									fi.PermissionsId = gi.PermissionsId;
                                    new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().Forums_Save(PortalId, fi, true, true, true);
								}
							}
						}
					}
				}
			}
		}
		private void UpdateForumViewTemplateId(int PortalId, int ModuleId)
		{
			try
			{
				var tc = new TemplateController();
				int ForumViewTemplateId = tc.Template_Get(TemplateName: "ForumView", PortalId: PortalId, ModuleId: ModuleId).TemplateId;
				var objModules = new DotNetNuke.Entities.Modules.ModuleController();
				objModules.UpdateModuleSetting(ModuleId, SettingKeys.ForumTemplateId, Convert.ToString(ForumViewTemplateId));
			}
			catch (Exception ex)
			{
				DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
			}
		}
        internal void Install_Or_Upgrade_RelocateDefaultThemeToLegacy()
        {
            try
			{
				DotNetNuke.Modules.ActiveForums.Utilities.CopyFolder(new System.IO.DirectoryInfo(Utilities.MapPath(Globals.ThemesPath + "_default")), new System.IO.DirectoryInfo(Utilities.MapPath(Globals.ThemesPath + "_legacy")));
				DotNetNuke.Modules.ActiveForums.Utilities.DeleteFolder(new System.IO.DirectoryInfo(Utilities.MapPath(Globals.ThemesPath + "_default")));
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        internal void Install_Or_Upgrade_RenameThemeCssFiles()
        {
            try
            {
                var di = new System.IO.DirectoryInfo(Utilities.MapPath(Globals.ThemesPath));
                System.IO.DirectoryInfo[] themeFolders = di.GetDirectories();
                foreach (System.IO.DirectoryInfo themeFolder in themeFolders)
                {
                    foreach (var fullFilePathName in System.IO.Directory.EnumerateFiles(path: themeFolder.FullName, searchPattern: "module.css", searchOption: System.IO.SearchOption.TopDirectoryOnly))
                    {
                        System.IO.File.Copy(fullFilePathName, fullFilePathName.Replace("module.css", "theme.css"), true);
                        System.IO.File.Delete(fullFilePathName);
                    }
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }
        internal void Install_Or_Upgrade_MoveTemplates()
		{
			if (!System.IO.Directory.Exists(Utilities.MapPath(Globals.TemplatesPath)))
			{
				System.IO.Directory.CreateDirectory(Utilities.MapPath(Globals.TemplatesPath));
			}
			if (!System.IO.Directory.Exists(Utilities.MapPath(Globals.DefaultTemplatePath)))
			{
				System.IO.Directory.CreateDirectory(Utilities.MapPath(Globals.DefaultTemplatePath));
			}

			var di = new System.IO.DirectoryInfo(Utilities.MapPath(Globals.ThemesPath));
			System.IO.DirectoryInfo[] themeFolders = di.GetDirectories();
			foreach (System.IO.DirectoryInfo themeFolder in themeFolders)
			{
				if (!System.IO.Directory.Exists(themeFolder.FullName + "/templates"))
				{
					System.IO.Directory.CreateDirectory(themeFolder.FullName + "/templates");
				}
			}
			TemplateController tc = new TemplateController();
			foreach (TemplateInfo templateInfo in tc.Template_List(-1, -1))
            {
                /* during upgrade, explicitly (re-)load template text from database rather than Template_List  API since API loads template using fallback/default logic and doesn't yet have the upgraded template text */
                IDataReader dr = DataProvider.Instance().Templates_Get(templateInfo.TemplateId, templateInfo.PortalId, templateInfo.ModuleId);
                while (dr.Read())
                {
                    try
                    {
						/* convert only legacy html portion of the template and save without encoding */
						string template = Convert.ToString(dr["Template"]).Replace("[TRESX:", "[RESX:");
                        if (template.Contains("<html>"))
                        {
                            string sHTML;
                            var xDoc = new System.Xml.XmlDocument();
                            xDoc.LoadXml(template);
                            System.Xml.XmlNode xNode;
                            System.Xml.XmlNode xRoot = xDoc.DocumentElement;
                            xNode = xRoot.SelectSingleNode("/template/html");
                            sHTML = xNode.InnerText;
                            template = sHTML;
                        }
                        templateInfo.Template = System.Web.HttpUtility.HtmlDecode(template);
                        tc.Template_Save(templateInfo);
                    }
                    catch (Exception ex)
                    {
                        DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                    }
                }
			}
		}
        internal void ArchiveOrphanedAttachments()
        {
            var di = new System.IO.DirectoryInfo(DotNetNuke.Modules.ActiveForums.Utilities.MapPath("~/portals"));
            System.IO.DirectoryInfo[] attachmentFolders = di.GetDirectories("activeforums_Attach",System.IO.SearchOption.AllDirectories);

            foreach (System.IO.DirectoryInfo attachmentFolder in attachmentFolders)
            {
                if (!System.IO.Directory.Exists(string.Concat(attachmentFolder.FullName, "\\orphaned")))
                {
                    System.IO.Directory.CreateDirectory(string.Concat(attachmentFolder.FullName, "\\orphaned"));
                }

                List<string> attachmentFullFileNames = System.IO.Directory.EnumerateFiles(path: attachmentFolder.FullName, searchPattern: "*", searchOption: System.IO.SearchOption.AllDirectories).ToList<string>();
                List<string> attachmentFileNames = new List<string>();

                foreach (string attachmentFileName in attachmentFullFileNames)
                {
                    attachmentFileNames.Add(new System.IO.FileInfo(attachmentFileName).Name);
                }

                List<string> databaseFileNames = new List<string>();
                string connectionString = new Connection().connectionString;
                string dbPrefix = new Connection().dbPrefix;

                using (IDataReader dr = SqlHelper.ExecuteReader(connectionString, CommandType.Text, $"SELECT FileName FROM {dbPrefix}Attachments ORDER BY FileName"))
                {
                    while (dr.Read())
                    {
                        databaseFileNames.Add(Utilities.SafeConvertString(dr["FileName"]));
                    }
                    dr.Close();
                }

                foreach (string attachmentFileName in attachmentFileNames)
                {
                    if (!databaseFileNames.Contains(attachmentFileName))
                    {
                        System.IO.File.Copy(string.Concat(attachmentFolder.FullName, "\\", attachmentFileName), string.Concat(attachmentFolder.FullName, "\\orphaned\\", attachmentFileName), true);
                        System.IO.File.Delete(string.Concat(attachmentFolder.FullName, "\\", attachmentFileName));
                    }
                }
            }
        }
        internal static void FillMissingTopicUrls()
        {
            string connectionString = new Connection().connectionString;
            string dbPrefix = new Connection().dbPrefix;
            var tc = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController();

            using (IDataReader dr = SqlHelper.ExecuteReader(connectionString, CommandType.Text, $"SELECT f.PortalId,f.ModuleId,ft.ForumId,t.topicId,c.Subject FROM {dbPrefix}Topics t INNER JOIN {dbPrefix}ForumTopics ft ON ft.TopicId = t.TopicId INNER JOIN {dbPrefix}Content c ON c.ContentId = t.ContentId INNER JOIN {dbPrefix}Forums f ON f.ForumId = ft.ForumId WHERE t.URL = ''"))
            {
                while (dr.Read())
                {
                    int portalId = (Utilities.SafeConvertInt(dr["PortalId"]));
                    int moduleId = (Utilities.SafeConvertInt(dr["ModuleId"]));
                    int forumId = (Utilities.SafeConvertInt(dr["ForumId"]));
                    int topicId = (Utilities.SafeConvertInt(dr["TopicId"]));
                    string subject = (Utilities.SafeConvertString(dr["Subject"]));
                    DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forumInfo = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(forumId, moduleId);
					DotNetNuke.Modules.ActiveForums.Entities.TopicInfo topicInfo = tc.Get(topicId);
					topicInfo.TopicUrl = DotNetNuke.Modules.ActiveForums.Controllers.UrlController.BuildTopicUrl(PortalId: portalId, ModuleId: moduleId, TopicId: topicId, subject: subject, forumInfo: forumInfo);
					tc.Update(topicInfo); 
                }
                dr.Close();
            } 
		}
	}
}