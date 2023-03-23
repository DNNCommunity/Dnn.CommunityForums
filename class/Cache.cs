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
using System.Reflection;
using System.Web;

namespace DotNetNuke.Modules.ActiveForums
{
	public class DataCache
    {
		public static bool IsCachingEnabledForModule(int ModuleId)
		{
            /* Track whether caching is being used at all in this module; this setting itself is cached to avoid repeated module lookups; 
			   so it is stored/retrieved directly using DNN API rather than local APIs since if caching is disabled would never return the correct value for this setting
			*/
            if (ModuleId < 0)
			{
				return true;
			}
			else
			{
				object IsCachingEnabledForModule = DotNetNuke.Common.Utilities.DataCache.GetCache(string.Format(CacheKeys.CacheEnabled, ModuleId));
				if (IsCachingEnabledForModule == null)
				{
					DotNetNuke.Entities.Modules.ModuleInfo objModule = new Entities.Modules.ModuleController().GetModule(ModuleId);
					IsCachingEnabledForModule = ((!String.IsNullOrEmpty(objModule.CacheMethod)) && (objModule.CacheTime > 0)); 
					DotNetNuke.Common.Utilities.DataCache.SetCache(string.Format(CacheKeys.CacheEnabled, ModuleId), IsCachingEnabledForModule, DateTime.UtcNow.AddMinutes(10));
				}
				return (bool)IsCachingEnabledForModule;
			}
        }
        [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Use CacheStore(int ModuleId, string cacheKey, object cacheObj)")]
        public static bool CacheStore(string cacheKey, object cacheObj)
        {
            CacheStore(-1, cacheKey, cacheObj, DateTime.UtcNow.AddMinutes(10));
			return true;
        }
        public static void CacheStore(int ModuleId, string cacheKey, object cacheObj)
        {
			//if (IsCachingEnabledForModule(ModuleId))
			//{
			CacheStore(ModuleId, cacheKey, cacheObj, DateTime.UtcNow.AddMinutes(10));
			//}
        }
        [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Use CacheStore(int ModuleId, string cacheKey, object cacheObj, DateTime Expiration)")]
        public static bool CacheStore(string cacheKey, object cacheObj, DateTime Expiration)
        {
				CacheStore(-1,  cacheKey, cacheObj, Expiration);
                return true;
        }
        public static void CacheStore(int ModuleId, string cacheKey, object cacheObj, DateTime Expiration)
        {
			//if (IsCachingEnabledForModule(ModuleId))
			//{
			try
			{
				Common.Utilities.DataCache.SetCache(cacheKey, cacheObj, Expiration);
			}
			catch
			{
			}
			//}
		}
        [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Use CacheRetrieve(int ModuleId, string cacheKey)")]
        public static object CacheRetrieve(string cacheKey)
        {
            return CacheRetrieve(ModuleId: -1, cacheKey: cacheKey);
        }
        public static object CacheRetrieve(int ModuleId, string cacheKey)
        {
			//if (IsCachingEnabledForModule(ModuleId))
			//{
				return Common.Utilities.DataCache.GetCache(CacheKey: cacheKey);
			//}
			//else
			//{ 
			//	return null; 
			//}
        }
        [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Use CacheClear(int ModuleId, string cacheKey)")]
        public static bool CacheClear(string cacheKey)
        {
			CacheClear(ModuleId: -1, cacheKey: cacheKey);
			return true;
        }
        public static void CacheClear(int ModuleId, string cacheKey)
        {
			//if (IsCachingEnabledForModule(ModuleId))
			//{
			try
			{
				Common.Utilities.DataCache.RemoveCache(CacheKey: cacheKey);
			}
			catch
			{
			}
			//}
		}
        [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Use CacheClearPrefix(int ModuleId, string cacheKeyPrefix)")]
        public static bool CacheClearPrefix(string cacheKeyPrefix)
        {
			CacheClearPrefix(ModuleId: -1, cacheKeyPrefix);
            return true;
        }
        public static void CacheClearPrefix(int ModuleId, string cacheKeyPrefix)
        {
            try
            {
				//if (IsCachingEnabledForModule(ModuleId))
				//{
					Common.Utilities.DataCache.ClearCache(cachePrefix: cacheKeyPrefix);
				//}
            }
            catch
            {
            }
        }
        [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Use ClearAllCache(int ModuleId).")]
        public static void ClearAllCache(int ModuleId, int TabId)
        {
			ClearAllCache(ModuleId);
        }
        public static void ClearAllCache(int ModuleId)
        {
            try
            {
                //if (!IsCachingEnabledForModule(ModuleId))
                //{
                CacheClearPrefix(ModuleId, string.Format(CacheKeys.CacheModulePrefix, ModuleId));
                //}
            }
            catch (Exception ex)
            {
                Services.Exceptions.Exceptions.LogException(ex);
            }

        }
        public static void ClearAllCacheForTabId(int TabId)
        {
			Common.Utilities.DataCache.ClearModuleCache(TabId);

        }
        public static void ClearSettingsCache(int ModuleId)
		{
			try
			{
				object obj = CacheRetrieve(ModuleId, cacheKey: string.Format(CacheKeys.MainSettings, ModuleId));
				if (obj != null)
				{
					CacheClear(ModuleId, cacheKey: string.Format(CacheKeys.MainSettings, ModuleId));
				}
			}
			catch 
			{

			}
		}
        [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0.")]
        public static void ClearForumsByGroupCache(int ModuleID, int GroupID)
		{
			object obj = CacheRetrieve(ModuleID,ModuleID + GroupID + "ForumsByGroup");
			if (obj != null)
			{
				CacheClear(ModuleID, ModuleID + GroupID + "ForumsByGroup");
			}
		}
        [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Only Cleared but never Set so not needed.")]
        public static void ClearForumGroupsCache(int ModuleID)
		{
			CacheClear(ModuleID,ModuleID + "ForumGroups");
			IDataReader rd;
			rd = DataProvider.Instance().Groups_List(ModuleID);
			while (rd.Read())
			{
				ClearForumsByGroupCache(ModuleID, Convert.ToInt32(rd["ForumGroupID"]));
			}
			rd.Close();
		}
        [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Not Used.")]
        public static void ClearForumSettingsCache(int ForumID)
		{
			CacheClear(-1,string.Format(CacheKeys.ForumSettings,-1, ForumID));
			CacheClear(-1,string.Format(CacheKeys.ForumInfo, -1, ForumID));
		}
		public static void ClearAllForumSettingsCache(int ModuleID)
		{
			try
			{
				IDataReader rd;
				rd = DataProvider.Instance().Forums_List(-1, ModuleID, -1, -1, false);
				while (rd.Read())
				{
					int ForumId = Convert.ToInt32(rd["ForumID"]);
                    CacheClear(ModuleID, string.Format(CacheKeys.ForumSettings,ModuleID, ForumId));
					CacheClear(ModuleID, string.Format(CacheKeys.ForumInfo, ModuleID, ForumId));
				}
				rd.Close();
			}
			catch
			{
			}
		}
        [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Not Used.")]
        public static void ClearFilterCache(int ModuleID)
		{
			object obj = CacheRetrieve(ModuleID,ModuleID + "FilterList");
			if (obj != null)
			{
				//Current.Cache.Remove(ModuleID & "FilterList")
				CacheClear(ModuleID,ModuleID + "FilterList");
			}
		}
		public static void ClearTemplateCache(int ModuleId)
		{
			try
			{
				if (System.IO.Directory.Exists(HttpContext.Current.Server.MapPath("~/DesktopModules/ActiveForums/cache")))
				{
					var di = new System.IO.DirectoryInfo(HttpContext.Current.Server.MapPath("~/DesktopModules/ActiveForums/cache"));
					foreach (System.IO.FileInfo fi in di.GetFiles())
					{
						if ((fi.FullName.IndexOf(ModuleId + "_", 0) + 1) > 0)
						{
							fi.Delete();
						}

					}
				}
			}
			catch (Exception ex)
			{
				//DotNetNuke.Services.Exceptions.Exceptions.LogException(ex)
			}
		}
		public static Hashtable GetSettings(int ModuleId, string SettingsKey, string CacheKey, bool UseCache)
		{
			var ht = new Hashtable();
			if (UseCache)
			{
				object obj = CacheRetrieve(ModuleId, CacheKey);
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
					CacheStore(ModuleId, CacheKey, ht);
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
        [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Replace with DotNetNuke.Modules.ActiveForums.SettingsBase.GetModuleSettings")]
        public static SettingsInfo MainSettings(int ModuleId)
        {
            return SettingsBase.GetModuleSettings(ModuleId);
        }

        #region Cache Storage
        private static void CacheTemplateToDisk(int ModuleId, int TemplateId, string TemplateType, string Template)
		{
			string myFile;
			string FileName = ModuleId + "_" + TemplateId + TemplateType + ".resources";
			string strPath;
			strPath = HttpContext.Current.Request.MapPath(Common.Globals.ApplicationPath) + "\\DesktopModules\\ActiveForums\\cache\\";
			if (! (System.IO.Directory.Exists(strPath)))
			{
				try
				{
					System.IO.Directory.CreateDirectory(strPath);
				}
				catch (Exception ex)
				{
					//   DotNetNuke.Services.Exceptions.Exceptions.LogException(ex)
					return;
				}

			}
			try
			{
				myFile = HttpContext.Current.Request.MapPath(Common.Globals.ApplicationPath) + "\\DesktopModules\\ActiveForums\\cache\\" + FileName;
				if (System.IO.File.Exists(myFile))
				{
					try
					{
						System.IO.File.Delete(myFile);
					}
					catch (Exception ex)
					{
						Services.Exceptions.Exceptions.LogException(ex);
					}
				}
				try
				{
					System.IO.File.AppendAllText(myFile, Template);
				}
				catch (Exception ex)
				{
					// DotNetNuke.Services.Exceptions.Exceptions.LogException(ex)
				}
			}
			catch (Exception ex)
			{
				//DotNetNuke.Services.Exceptions.Exceptions.LogException(ex)
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
						myFile = HttpContext.Current.Server.MapPath("~/DesktopModules/ActiveForums/config/templates/" + TemplateType + ".txt");
						if (System.IO.File.Exists(myFile))
						{
							System.IO.StreamReader objStreamReader = null;
							try
							{
								objStreamReader = System.IO.File.OpenText(myFile);
							}
							catch (Exception ex)
							{
								Services.Exceptions.Exceptions.LogException(ex);
							}
							sTemplate = objStreamReader.ReadToEnd();
							objStreamReader.Close();
							sTemplate = Utilities.ParseSpacer(sTemplate);
							CacheStore(ModuleId + TemplateId + TemplateType, sTemplate);
							//Current.Cache.Insert(ModuleId & TemplateId & TemplateType, sTemplate, New System.Web.Caching.CacheDependency(Current.Server.MapPath("~/DesktopModules/ActiveForums/config/Templates/" & TemplateType & ".txt")))
						}
					}
					catch (Exception ex)
					{
						Services.Exceptions.Exceptions.LogException(ex);
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
			string FileName = ModuleId + "_" + TemplateId + TemplateType + ".resources";
			System.IO.StreamReader objStreamReader;
			if (_disableCache)
			{
				sTemplate = GetTemplate(TemplateId, TemplateType);
			}
			else
			{
				try
				{
					myFile = HttpContext.Current.Request.MapPath(Common.Globals.ApplicationPath) + "\\DesktopModules\\ActiveForums\\cache\\" + FileName;
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
					Services.Exceptions.Exceptions.LogException(ex);
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
					    string myFile = HttpContext.Current.Server.MapPath("~/DesktopModules/ActiveForums/config/templates/" + TemplateType + ".txt");
					    if (System.IO.File.Exists(myFile))
						{
							System.IO.StreamReader objStreamReader = null;
							try
							{
								objStreamReader = System.IO.File.OpenText(myFile);
							}
							catch (Exception ex)
							{
								Services.Exceptions.Exceptions.LogException(ex);
							}
							sOut = objStreamReader.ReadToEnd();
							objStreamReader.Close();
							sOut = Utilities.ParseSpacer(sOut);
						}
					}
					catch (Exception ex)
					{
						Services.Exceptions.Exceptions.LogException(ex);
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
				Services.Exceptions.Exceptions.LogException(ex);
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
					myFile = HttpContext.Current.Server.MapPath("~/DesktopModules/ActiveForums/config/templates/" + TemplateFileName);
					if (System.IO.File.Exists(myFile))
					{
						System.IO.StreamReader objStreamReader = null;
						try
						{
							objStreamReader = System.IO.File.OpenText(myFile);
						}
						catch (Exception ex)
						{
							Services.Exceptions.Exceptions.LogException(ex);
						}
						sOut = objStreamReader.ReadToEnd();
						objStreamReader.Close();
						sOut = Utilities.ParseSpacer(sOut);
					}
				}
				catch (Exception ex)
				{
					Services.Exceptions.Exceptions.LogException(ex);
				}

			}
			catch (Exception ex)
			{
				Services.Exceptions.Exceptions.LogException(ex);
				sOut = "ERROR: Loading template failed";
			}
			return sOut;
		}
#endregion


	}
}
