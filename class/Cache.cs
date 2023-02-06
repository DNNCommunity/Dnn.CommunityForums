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

		public static SettingsInfo MainSettings(int ModuleId)
		{
			object obj = CacheRetrieve(string.Format(CacheKeys.MainSettings, ModuleId));
			if (obj == null || disableCache)
			{
				var objSettings = new SettingsInfo();
				var sb = new SettingsBase {ForumModuleId = ModuleId};
			    obj = sb.MainSettings;
				if (disableCache == false)
				{
					CacheStore(string.Format(CacheKeys.MainSettings, ModuleId), obj);
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
				CacheClear(ModuleId + "fv");
				CacheClear(ModuleId + "ForumStatTable");
				CacheClear(ModuleId + "ForumStatsOutput");
				CacheClear(ModuleId + TabId + "ForumTemplate");
			}
			catch (Exception ex)
			{
				Services.Exceptions.Exceptions.LogException(ex);
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
			object obj = CacheRetrieve(ModuleID + GroupID + "ForumsByGroup");
			if (obj != null)
			{
				CacheClear(ModuleID + GroupID + "ForumsByGroup");
			}
		}
		public static void ClearForumGroupsCache(int ModuleID)
		{
			CacheClear(ModuleID + "ForumGroups");
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
			CacheClear(ForumID + "ForumSettings");
			CacheClear(string.Format(CacheKeys.ForumInfo, ForumID));
			CacheClear(string.Format(CacheKeys.ForumInfo, ForumID) + "st");

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
					CacheClear(intForumID + "ForumSettings");
					CacheClear(ModuleID + TopicsTemplateId + "TopicsTemplate");
					CacheClear(ModuleID + TopicTemplateId + "TopicTemplate");
					CacheClear(string.Format(CacheKeys.ForumInfo, intForumID));
					CacheClear(string.Format(CacheKeys.ForumInfo, intForumID) + "st");
				}
				rd.Close();
			}
			catch (Exception ex)
			{

			}

		}
		public static void ClearFilterCache(int ModuleID)
		{
			object obj = CacheRetrieve(ModuleID + "FilterList");
			if (obj != null)
			{
				//Current.Cache.Remove(ModuleID & "FilterList")
				CacheClear(ModuleID + "FilterList");
			}
		}
		public static Hashtable GetSettings(int ModuleId, string SettingsKey, string CacheKey, bool UseCache)
		{
			var ht = new Hashtable();
			if (UseCache)
			{
				object obj = CacheRetrieve(CacheKey + "st");
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
					CacheStore(CacheKey + "st", ht);
					//Current.Cache.Insert(ModuleId & SettingsKey & "Settings", ht, Nothing, DateTime.UtcNow.AddMinutes(10), Web.Caching.Cache.NoSlidingExpiration)
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
	}
}
