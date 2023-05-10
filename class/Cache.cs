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
	public class DataCache
	{
		public static bool _disableCache = false;
		public static bool disableCache
		{
			get
			{
				return _disableCache;
			}
			set
			{
				_disableCache = value;
			}
		}
		public static bool CacheStore(string cacheKey, object cacheObj)
		{
			return CacheStore(cacheKey, cacheObj, DateTime.UtcNow.AddMinutes(10));
		}
		public static bool CacheStore(string cacheKey, object cacheObj, DateTime Expiration)
		{
			try
			{
				Common.Utilities.DataCache.SetCache(cacheKey, cacheObj, Expiration);
				return true;
			}
			catch (Exception ex)
			{
				return false;
			}
		}
		public static object CacheRetrieve(string cacheKey)
		{
			object obj = Common.Utilities.DataCache.GetCache(cacheKey);
			return obj;
		}
		public static bool CacheClear(string cacheKey)
		{
			try
			{
				Common.Utilities.DataCache.RemoveCache(cacheKey);
				return true;
			}
			catch (Exception ex)
			{
				return false;
			}
		}
		// KR - remove all cache starting with the given string
		public static bool CacheClearPrefix(string cacheKeyPrefix)
		{
			try
			{
				Common.Utilities.DataCache.ClearCache(cacheKeyPrefix);
				return true;
			}
			catch (Exception ex)
			{
				return false;
			}
		}

		public static SettingsInfo MainSettings(int MID)
		{
			object obj = CacheRetrieve(string.Format(CacheKeys.MainSettings, MID));
			if (obj == null || disableCache)
			{
				var objSettings = new SettingsInfo();
				var sb = new SettingsBase {ForumModuleId = MID};
			    obj = sb.MainSettings;
				if (disableCache == false)
				{
					CacheStore(string.Format(CacheKeys.MainSettings, MID), obj);
				}
			}

			return (SettingsInfo)obj;
		}
		public static void ClearAllCache(int ModuleId, int TabId)
		{
			try
			{
				ClearAllForumSettingsCache(ModuleId);
				ClearSettingsCache(ModuleId);
				ClearTemplateCache(ModuleId);
				CacheClear(string.Concat(ModuleId, "fv"));
				CacheClear(string.Concat(ModuleId, "ForumStatTable"));
				CacheClear(string.Concat(ModuleId, "ForumStatsOutput"));
				CacheClear(string.Concat(ModuleId, TabId, "ForumTemplate"));
			}
			catch (Exception ex)
			{
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
			}

		}
		public static void ClearSettingsCache(int ModuleID)
		{
			try
			{
				object obj = CacheRetrieve(string.Format(CacheKeys.MainSettings, ModuleID));
				if (obj != null)
				{
					CacheClear(string.Format(CacheKeys.MainSettings, ModuleID));
				}
			}
			catch (Exception ex)
			{

			}

		}
		public static void ClearForumsByGroupCache(int ModuleID, int GroupID)
		{
			object obj = CacheRetrieve(string.Concat(ModuleID, GroupID, "ForumsByGroup"));
			if (obj != null)
			{
				CacheClear(string.Concat(ModuleID, GroupID, "ForumsByGroup"));
			}
		}
		public static void ClearForumGroupsCache(int ModuleID)
		{
			CacheClear(string.Concat(ModuleID, "ForumGroups"));
			IDataReader rd;
			rd = DataProvider.Instance().Groups_List(ModuleID);
			while (rd.Read())
			{
				ClearForumsByGroupCache(ModuleID, Convert.ToInt32(rd["ForumGroupID"]));
			}
			rd.Close();
		}
		public static void ClearForumSettingsCache(int ForumID)
		{
			CacheClear(string.Concat(ForumID, "ForumSettings"));
			CacheClear(string.Format(CacheKeys.ForumInfo, ForumID));
			CacheClear(string.Concat(string.Format(CacheKeys.ForumInfo, ForumID), "st"));
        }
		public static void ClearAllForumSettingsCache(int ModuleID)
		{
			try
			{
				IDataReader rd;
				rd = DataProvider.Instance().Forums_List(-1, ModuleID, -1, -1, false);
				while (rd.Read())
				{
					int intForumID;
					intForumID = Convert.ToInt32(rd["ForumID"]);
					int TopicsTemplateId;
					int TopicTemplateId;
					TopicsTemplateId = Convert.ToInt32(rd["TopicsTemplateId"]);
					TopicTemplateId = Convert.ToInt32(rd["TopicTemplateId"]);
					CacheClear(string.Concat(intForumID, "ForumSettings"));
					CacheClear(string.Concat(ModuleID + TopicsTemplateId, "TopicsTemplate"));
					CacheClear(string.Concat(ModuleID + TopicTemplateId, "TopicTemplate"));
					CacheClear(string.Format(CacheKeys.ForumInfo, intForumID));
					CacheClear(string.Concat(string.Format(CacheKeys.ForumInfo, intForumID), "st"));
				}
				rd.Close();
			}
			catch
			{
                // do nothing? 
			}

		}
		public static void ClearFilterCache(int ModuleID)
		{
			object obj = CacheRetrieve(ModuleID + "FilterList");
			if (obj != null)
			{
				CacheClear(string.Concat(ModuleID, "FilterList"));
			}
		}
		public static void ClearTemplateCache(int ModuleId)
		{
			try
			{
				if (System.IO.Directory.Exists(DotNetNuke.Modules.ActiveForums.Utilities.MapPath(string.Concat(Globals.ModulePath, "cache"))))
				{
					var di = new System.IO.DirectoryInfo(DotNetNuke.Modules.ActiveForums.Utilities.MapPath(string.Concat(Globals.ModulePath, "cache")));
					foreach (System.IO.FileInfo fi in di.GetFiles())
					{
						if ((fi.FullName.IndexOf(string.Concat(ModuleId, "_"), 0) + 1) > 0)
						{
							fi.Delete();
						}

					}
				}
			}
			catch (Exception ex)
			{
				// do nothing?
			}
		}
		public static Hashtable GetSettings(int ModuleId, string SettingsKey, string CacheKey, bool UseCache)
		{
			var ht = new Hashtable();
			if (UseCache)
			{
				object obj = CacheRetrieve(string.Concat(CacheKey, "st"));
				if (obj == null)
				{
					IDataReader dr = DataProvider.Instance().Settings_List(ModuleId, SettingsKey);
					while (dr.Read())
					{
						if (! (ht.ContainsKey(dr["SettingName"].ToString())))
						{
							ht.Add(dr["SettingName"].ToString(), string.Empty);
						}
						ht[dr["SettingName"].ToString()] = dr["SettingValue"].ToString();
					}
					dr.Close();
					CacheStore(string.Concat(CacheKey, "st"), ht);
                }
				else
				{
					ht = (Hashtable)obj;
				}
			}
			else
			{
				IDataReader dr = DataProvider.Instance().Settings_List(ModuleId, SettingsKey);
				while (dr.Read())
				{
					if (! (ht.ContainsKey(dr["SettingName"].ToString())))
					{
						ht.Add(dr["SettingName"].ToString(), string.Empty);
					}
					ht[dr["SettingName"].ToString()] = dr["SettingValue"].ToString();
				}
				dr.Close();
			}

			return ht;
		}

#region Cache Storage
		private static void CacheTemplateToDisk(int ModuleId, int TemplateId, string TemplateType, string Template)
		{
			string myFile;
			string FileName = string.Concat(ModuleId, "_", TemplateId, TemplateType, ".resources");
			string strPath;
			strPath = HttpContext.Current.Request.MapPath(string.Concat(Globals.ModulePath, "cache\\"));
			if (! (System.IO.Directory.Exists(strPath)))
			{
				try
				{
					System.IO.Directory.CreateDirectory(strPath);
				}
				catch (Exception ex)
				{
					return;
				}

			}
			try
			{
				myFile = HttpContext.Current.Request.MapPath(string.Concat(Globals.ModulePath, "cache\\")) + FileName;
				if (System.IO.File.Exists(myFile))
				{
					try
					{
						System.IO.File.Delete(myFile);
					}
					catch (Exception ex)
					{
                        DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
					}
				}
				try
				{
					System.IO.File.AppendAllText(myFile, Template);
				}
				catch (Exception ex)
				{
					// do nothing??
				}
			}
			catch (Exception ex)
			{
				// do nothing??
			}
		}
#endregion

#region Cache Retrieval
		public static string GetCachedTemplate(int TemplateStore, int ModuleId, string TemplateType, int TemplateId)
		{
			string sTemplate;
			switch (TemplateStore)
			{
				case 0:
					//Get From Memory
					sTemplate = GetTemplateFromMemory(ModuleId, TemplateType, TemplateId);
					break;
				case 1:
					//Get From Disk
					sTemplate = GetTemplateFromDisk(ModuleId, TemplateType, TemplateId);
					break;
				default:
					//Get From DB
					sTemplate = GetTemplate(TemplateId, TemplateType);
					break;
			}
			sTemplate = sTemplate.Replace("[TOOLBAR]", string.Empty);
			sTemplate = sTemplate.Replace("[TEMPLATE:TOOLBAR]", string.Empty);

			return sTemplate;
		}
		private static string GetTemplateFromMemory(int ModuleId, string TemplateType, int TemplateId)
		{
			string sTemplate = string.Empty;

			string myFile;
			object obj = CacheRetrieve(ModuleId + TemplateId + TemplateType);
			if (disableCache)
			{
				obj = null;
			}
			if (obj == null)
			{
				if (TemplateId == 0)
				{
					try
					{
						myFile = DotNetNuke.Modules.ActiveForums.Utilities.MapPath(string.Concat("~/DesktopModules/ActiveForums/config/templates/" , TemplateType, ".txt"));
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
							CacheStore(string.Concat(ModuleId, TemplateId, TemplateType), sTemplate);
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
					CacheStore(ModuleId + TemplateId + TemplateType, sTemplate);
				}
			}
			else
			{
				sTemplate = Convert.ToString(obj);
			}
			return sTemplate;
		}
		private static string GetTemplateFromDisk(int ModuleId, string TemplateType, int TemplateId)
		{
			string sTemplate;
			string myFile;
			string FileName = string.Concat(ModuleId, "_", TemplateId, TemplateType, ".resources");
			System.IO.StreamReader objStreamReader;
			if (_disableCache)
			{
				sTemplate = GetTemplate(TemplateId, TemplateType);
			}
			else
			{
				try
				{
					myFile = HttpContext.Current.Request.MapPath(string.Concat(Globals.ModulePath, "cache\\")) + FileName;
					if (System.IO.File.Exists(myFile))
					{
						try
						{
							objStreamReader = System.IO.File.OpenText(myFile);

							sTemplate = objStreamReader.ReadToEnd();
							objStreamReader.Close();
						}
						catch (Exception exc)
						{
							sTemplate = GetTemplate(TemplateId, TemplateType);
						}
					}
					else
					{
						sTemplate = GetTemplate(TemplateId, TemplateType);
						CacheTemplateToDisk(ModuleId, TemplateId, TemplateType, sTemplate);
					}
				}
				catch (Exception ex)
				{
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
					sTemplate = "ERROR: Loading template failed";
				}
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
					    string myFile = DotNetNuke.Modules.ActiveForums.Utilities.MapPath(string.Concat("~/DesktopModules/ActiveForums/config/templates/", TemplateType, ".txt"));
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
					myFile = DotNetNuke.Modules.ActiveForums.Utilities.MapPath("~/DesktopModules/ActiveForums/config/templates/" + TemplateFileName);
					if (System.IO.File.Exists(myFile))
					{
						System.IO.StreamReader objStreamReader = null;
						try
						{
							objStreamReader = System.IO.File.OpenText(myFile);
						}
						catch (Exception ex)
						{
							Exceptions.LogException(ex);
						}
						sOut = objStreamReader.ReadToEnd();
						objStreamReader.Close();
						sOut = Utilities.ParseSpacer(sOut);
					}
				}
				catch (Exception ex)
				{
					Exceptions.LogException(ex);
				}

			}
			catch (Exception ex)
			{
				Exceptions.LogException(ex);
				sOut = "ERROR: Loading template failed";
			}
			return sOut;
		}
#endregion

    }
}