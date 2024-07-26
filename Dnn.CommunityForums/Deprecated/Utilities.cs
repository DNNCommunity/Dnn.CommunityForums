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

namespace DotNetNuke.Modules.ActiveForums
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.UI.WebControls;

    public abstract partial class Utilities
    {
        [Obsolete("Deprecated in Community Forums. To be removed in 09.00.00. Use ManageImagePath(string sHTML, Uri hostUri)")]
        public static string ManageImagePath(string sHTML)
        {
            return ManageImagePath(sHTML, HttpContext.Current.Request.Url);
        }

        [Obsolete("Deprecated in Community Forums. To be removed in 09.00.00. Use ManageImagePath(string sHTML, Uri hostUri)")]
        public static string ManageImagePath(string sHTML, string hostWithScheme)
        {
            return ManageImagePath(sHTML, new Uri(hostWithScheme));
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.ModerationController.GetListOfModerators(int portalId, int ModuleId, int forumId).")]
        public static List<DotNetNuke.Entities.Users.UserInfo> GetListOfModerators(int portalId, int forumId)
        {
            return DotNetNuke.Modules.ActiveForums.Controllers.ModerationController.GetListOfModerators(portalId, -1, forumId);
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use BindEnum(DropDownList pDDL, Type enumType, string pColValue, bool addEmptyValue, bool localize, int excludeIndex)")]
        public static void BindEnum(System.Web.UI.WebControls.DropDownList pDDL, Type enumType, string pColValue, bool addEmptyValue)
        {
            BindEnum(pDDL, enumType, pColValue, addEmptyValue, false, -1);
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public static string ParsePre(string strMessage)
        {
            var objRegEx = new System.Text.RegularExpressions.Regex("<pre>(.*?)</pre>");
            strMessage = "<code>" + HttpUtility.HtmlDecode(objRegEx.Replace(strMessage, "$1")) + "</code>";
            return strMessage;
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        internal static string ParseSecurityTokens(string template, string userRoles)
        {
            const string pattern = @"(\[AF:SECURITY:(.+?):(.+?)\])(.|\n)*?(\[/AF:SECURITY:(.+?):(.+?)\])";

            var sKey = string.Empty;
            var sReplace = string.Empty;

            var regExp = new System.Text.RegularExpressions.Regex(pattern);
            var matches = regExp.Matches(template);
            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                var sRoles = match.Groups[3].Value;
                if (DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasAccess(sRoles, userRoles))
                {
                    template = template.Replace(match.Groups[1].Value, string.Empty);
                    template = template.Replace(match.Groups[5].Value, string.Empty);
                }
                else
                {
                    template = template.Replace(match.Value, string.Empty);
                }
            }

            return template;
        }

        /// <summary>
        /// Calculates a friendly display string based on an input timespan
        /// </summary>
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public static string HumanFriendlyDate(DateTime displayDate, int moduleId, int timeZoneOffset)
        {
            var newDate = displayDate.AddMinutes(timeZoneOffset);
            var ts = new TimeSpan(DateTime.Now.Ticks - newDate.Ticks);
            var delta = ts.TotalSeconds;
            if (delta <= 1)
            {
                return GetSharedResource("[RESX:TimeSpan:SecondAgo]");
            }

            if (delta < 60)
            {
                return string.Format(GetSharedResource("[RESX:TimeSpan:SecondsAgo]"), ts.Seconds);
            }

            if (delta < 120)
            {
                return GetSharedResource("[RESX:TimeSpan:MinuteAgo]");
            }

            if (delta < (45 * 60))
            {
                return string.Format(GetSharedResource("[RESX:TimeSpan:MinutesAgo]"), ts.Minutes);
            }

            if (delta < (90 * 60))
            {
                return GetSharedResource("[RESX:TimeSpan:HourAgo]");
            }

            if (delta < (24 * 60 * 60))
            {
                return string.Format(GetSharedResource("[RESX:TimeSpan:HoursAgo]"), ts.Hours);
            }

            if (delta < (48 * 60 * 60))
            {
                return GetSharedResource("[RESX:TimeSpan:DayAgo]");
            }

            if (delta < (72 * 60 * 60))
            {
                return string.Format(GetSharedResource("[RESX:TimeSpan:DaysAgo]"), ts.Days);
            }

            if (delta < Convert.ToDouble(new TimeSpan(24 * 32, 0, 0).TotalSeconds))
            {
                return GetSharedResource("[RESX:TimeSpan:MonthAgo]");
            }

            if (delta < Convert.ToDouble(new TimeSpan(24 * 30 * 11, 0, 0).TotalSeconds))
            {
                return string.Format(GetSharedResource("[RESX:TimeSpan:MonthsAgo]"), Math.Ceiling(ts.Days / 30.0));
            }

            if (delta < Convert.ToDouble(new TimeSpan(24 * 30 * 18, 0, 0).TotalSeconds))
            {
                return GetSharedResource("[RESX:TimeSpan:YearAgo]");
            }

            return string.Format(GetSharedResource("[RESX:TimeSpan:YearsAgo]"), Math.Ceiling(ts.Days / 365.0));
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public static DateTime GetUserDate(DateTime displayDate, int mid, int offset)
        {
            var mainSettings = SettingsBase.GetModuleSettings(mid);
            var mServerOffSet = mainSettings.TimeZoneOffset;
            var newDate = displayDate.AddMinutes(-mServerOffSet);

            return newDate.AddMinutes(offset);
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public string GetUserFormattedDate(DateTime date, DotNetNuke.Entities.Portals.PortalInfo portalInfo, DotNetNuke.Entities.Users.UserInfo userInfo)
        {
            return GetUserFormattedDateTime(date, portalInfo.PortalID, userInfo.UserID);
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public static string GetDate(DateTime displayDate, int mid, int offset)
        {
            string dateStr;

            try
            {
                var mUserOffSet = 0;
                var mainSettings = SettingsBase.GetModuleSettings(mid);
                var mServerOffSet = mainSettings.TimeZoneOffset;
                var newDate = displayDate.AddMinutes(-mServerOffSet);

                newDate = newDate.AddMinutes(offset);

                var dateFormat = mainSettings.DateFormatString;
                var timeFormat = mainSettings.TimeFormatString;
                var formatString = string.Concat(dateFormat, " ", timeFormat);

                try
                {
                    dateStr = newDate.ToString(formatString);
                }
                catch
                {
                    dateStr = displayDate.ToString();
                }

                return dateStr;
            }
            catch (Exception ex)
            {
                dateStr = displayDate.ToString();
                return dateStr;
            }
        }

        [Obsolete("Deprecated in Community Forums. Removed in 09.00.00. Use HttpUtility.HtmlEncode.")]
        public static string HtmlEncode(string strMessage = "") => HttpUtility.HtmlEncode(strMessage);

        [Obsolete("Deprecated in Community Forums. Removed in 09.00.00. Use HttpUtility.HtmlDecode.")]
        public static string HtmlDecode(string strMessage) => HttpUtility.HtmlDecode(strMessage);

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used. Use Utilities.NavigateURL(int tabId) [Note URL in NavigateURL() is uppercase]")]
        public static string NavigateUrl(int tabId)
        {
            return NavigateURL(tabId);
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used. Use Utilities.NavigateURL(int tabId, string controlKey, params string[] additionalParameters) [Note URL in NavigateURL() is uppercase]")]
        public static string NavigateUrl(int tabId, int portalId, string controlKey, params string[] additionalParameters)
        {
            return new DotNetNuke.Modules.ActiveForums.Services.URLNavigator().NavigateURL(tabId, Utilities.GetPortalSettings(portalId), controlKey, additionalParameters);
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used. Use Utilities.NavigateURL(int tabId, string controlKey, params string[] additionalParameters) [Note URL in NavigateURL() is uppercase]")]
        public static string NavigateUrl(int tabId, string controlKey, params string[] additionalParameters)
        {
            return new DotNetNuke.Modules.ActiveForums.Services.URLNavigator().NavigateURL(tabId, controlKey, additionalParameters);
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used. Use Utilities.NavigateURL(int tabId, string controlKey, params string[] additionalParameters) [Note URL in NavigateURL() is uppercase]")]
        public static string NavigateUrl(int tabId, string controlKey, List<string> additionalParameters)
        {
            return DotNetNuke.Modules.ActiveForums.Utilities.NavigateURL(tabId, controlKey, additionalParameters.ToArray());
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used. Use Utilities.NavigateURL(int tabId, string controlKey, params string[] additionalParameters) [Note URL in NavigateURL() is uppercase]")]
        public static string NavigateUrl(int tabId, string controlKey, string pageName, int portalId, params string[] additionalParameters)
        {
            return NavigateURL(tabId, controlKey, pageName, portalId, additionalParameters);
        }
    }
}
