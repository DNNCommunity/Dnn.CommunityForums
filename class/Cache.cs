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
        private static int settingsCacheTime = 10;
        private static int contentCacheTime = 2;
        [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Use SettingsCacheStore(int ModuleId, string cacheKey, object cacheObj) or ContentCacheStore(int ModuleId, string cacheKey, object cacheObj)")]
        public static bool CacheStore(string cacheKey, object cacheObj)
        {
            SettingsCacheStore(-1, cacheKey, cacheObj, DateTime.UtcNow.AddMinutes(settingsCacheTime));
			return true;
        }
        public static void SettingsCacheStore(int ModuleId, string cacheKey, object cacheObj)
        {
            SettingsCacheStore(ModuleId, cacheKey, cacheObj, DateTime.UtcNow.AddMinutes(settingsCacheTime));
        }
        public static void ContentCacheStore(int ModuleId, string cacheKey, object cacheObj)
        {
			Common.Utilities.DataCache.SetCache(cacheKey, cacheObj,DateTime.UtcNow.AddMinutes(contentCacheTime));
		}
		[Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Use SettingsCacheStore(int ModuleId, string cacheKey, object cacheObj, DateTime Expiration) or ContentCacheStore(int ModuleId, string cacheKey, object cacheObj, DateTime Expiration)")]
        public static bool CacheStore(string cacheKey, object cacheObj, DateTime Expiration)
        {
				SettingsCacheStore(-1,  cacheKey, cacheObj, Expiration);
                return true;
        }
        public static void SettingsCacheStore(int ModuleId, string cacheKey, object cacheObj, DateTime Expiration)
        {
            try
            {
                Common.Utilities.DataCache.SetCache(cacheKey, cacheObj, Expiration);
            }
            catch
            {
            }
        }
        [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Use SettingsCacheRetrieve(int ModuleId, string cacheKey) or ContentCacheRetrieve(int ModuleId, string cacheKey)")]
        public static object CacheRetrieve(string cacheKey)
        {
            return SettingsCacheRetrieve(ModuleId: -1, cacheKey: cacheKey);
        }
        public static object SettingsCacheRetrieve(int ModuleId, string cacheKey)
        {
            return Common.Utilities.DataCache.GetCache(CacheKey: cacheKey);
        }
        public static object ContentCacheRetrieve(int ModuleId, string cacheKey)
        {
            return Common.Utilities.DataCache.GetCache(CacheKey: cacheKey);
        }
        [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Use CacheClear(int ModuleId, string cacheKey)")]
        public static bool CacheClear(string cacheKey)
        {
			SettingsCacheClear(ModuleId: -1, cacheKey: cacheKey);
			return true;
        }
        public static void SettingsCacheClear(int ModuleId, string cacheKey)
        {
            try
            {
                Common.Utilities.DataCache.RemoveCache(CacheKey: cacheKey);
            }
            catch
            {
            }
        }
        public static void ContentCacheClear(int ModuleId, string cacheKey)
        {
			Common.Utilities.DataCache.RemoveCache(CacheKey: cacheKey);
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
				Common.Utilities.DataCache.ClearCache(cachePrefix: cacheKeyPrefix);
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
                CacheClearPrefix(ModuleId, string.Format(CacheKeys.CacheModulePrefix, ModuleId));
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
				object obj = SettingsCacheRetrieve(ModuleId, cacheKey: string.Format(CacheKeys.MainSettings, ModuleId));
				if (obj != null)
				{
					SettingsCacheClear(ModuleId, cacheKey: string.Format(CacheKeys.MainSettings, ModuleId));
				}
			}
			catch 
			{

			}
		}
        [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0.")]
        public static void ClearForumsByGroupCache(int ModuleID, int GroupID)
		{
			object obj = SettingsCacheRetrieve(ModuleID,ModuleID + GroupID + "ForumsByGroup");
			if (obj != null)
			{
				SettingsCacheClear(ModuleID, ModuleID + GroupID + "ForumsByGroup");
			}
		}
        [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Only Cleared but never Set so not needed.")]
        public static void ClearForumGroupsCache(int ModuleID)
		{
			SettingsCacheClear(ModuleID,ModuleID + "ForumGroups");
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
			SettingsCacheClear(-1,string.Format(CacheKeys.ForumSettings,-1, ForumID));
			SettingsCacheClear(-1,string.Format(CacheKeys.ForumInfo, -1, ForumID));
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
                    SettingsCacheClear(ModuleID, string.Format(CacheKeys.ForumSettings,ModuleID, ForumId));
					SettingsCacheClear(ModuleID, string.Format(CacheKeys.ForumInfo, ModuleID, ForumId));
				}
				rd.Close();
			}
			catch
			{
                // do nothing? 
			}
		}
        [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Not Used.")]
        public static void ClearFilterCache(int ModuleID)
		{
			object obj = SettingsCacheRetrieve(ModuleID,ModuleID + "FilterList");
			if (obj != null)
			{
				//Current.Cache.Remove(ModuleID & "FilterList")
				SettingsCacheClear(ModuleID,ModuleID + "FilterList");
			}
		}
		public static Hashtable GetSettings(int ModuleId, string SettingsKey, string CacheKey, bool UseCache)
		{
			var ht = new Hashtable();
			if (UseCache)
			{
				object obj = SettingsCacheRetrieve(ModuleId, CacheKey);
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
					SettingsCacheStore(ModuleId, CacheKey, ht);
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
        #region "code will be removed after templates storage changed from database to files in 08.00.00"

        #region Cache Storage
        [Obsolete("Deprecated in Community Forums. Scheduled removal in v8.0.0.0. Template Storage/Retrieval/Caching Rewritten and Template cache moved to TemplateCache.cs")]
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
        [Obsolete("Deprecated in Community Forums. Scheduled removal in v8.0.0.0. Template Storage/Retrieval/Caching Rewritten and Template cache moved to TemplateCache.cs")]
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
			if (!SettingsBase.GetModuleSettings(ModuleId).CacheTemplates)
			{
				obj = null;
			}
			if (obj == null)
			{
				if (TemplateId == 0)
				{
					try
					{
						myFile = HttpContext.Current.Server.MapPath(string.Concat(Globals.DefaultTemplatePath, TemplateType, ".txt"));
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
            if (!SettingsBase.GetModuleSettings(ModuleId).CacheTemplates)
            {
                sTemplate = GetTemplate(TemplateId, TemplateType);
            }
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
			//}


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
					    string myFile = HttpContext.Current.Server.MapPath(string.Concat(Globals.DefaultTemplatePath, TemplateType, ".txt"));
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
					myFile = HttpContext.Current.Server.MapPath(Globals.DefaultTemplatePath + TemplateFileName);
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

    #endregion
    }
}
