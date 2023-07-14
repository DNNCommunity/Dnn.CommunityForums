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
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;

using System.Web;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Host;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Scheduling;
using DotNetNuke.UI.UserControls;

namespace DotNetNuke.Modules.ActiveForums
{
    #region SubscriberInfo
    public class SubscriptionInfo
    {
        #region Public Properties
        public int UserId { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public TimeSpan TimeZoneOffSet { get; set; }
        public CultureInfo UserCulture { get; set; }
        public bool TopicSubscriber { get; set; }
        #endregion
    }
    #endregion

    public class SubscriptionController
    {
        public int Subscription_Update(int PortalId, int ModuleId, int ForumId, int TopicId, int Mode, int UserId, string UserRoles = "")
        {
            if (UserId == -1)
            {
                return -1;
            }

            if (string.IsNullOrEmpty(UserRoles))
            {
                UserController uc = new UserController();
                User uu = uc.GetUser(PortalId, ModuleId, UserId);
                UserRoles = uu.UserRoles;
            }
            
            var fc = new ForumController();
            Forum fi = fc.Forums_Get(PortalId, ModuleId, ForumId, UserId, true, false, -1);
            
            if (Permissions.HasPerm(fi.Security.Subscribe, UserRoles))
            {
                return Convert.ToInt32(DataProvider.Instance().Subscription_Update(PortalId, ModuleId, ForumId, TopicId, Mode, UserId));
            }
            
            return -1;
        }
        
        public List<SubscriptionInfo> Subscription_GetSubscribers(int PortalId, int ForumId, int TopicId, SubscriptionTypes Mode, int AuthorId, string CanSubscribe)
        {
            SubscriptionInfo si;
            var sl = new List<SubscriptionInfo>();
            IDataReader dr = DataProvider.Instance().Subscriptions_GetSubscribers(PortalId, ForumId, TopicId, (int)Mode);
            while (dr.Read())
            {
                if (AuthorId != Convert.ToInt32(dr["UserId"]))
                {
                    si = new SubscriptionInfo
                    {
                        DisplayName = dr["DisplayName"].ToString(),
                        Email = dr["Email"].ToString(),
                        FirstName = dr["FirstName"].ToString(),
                        LastName = dr["LastName"].ToString(),
                        UserId = Convert.ToInt32(dr["UserId"]),
                        Username = dr["Username"].ToString(),
                        TimeZoneOffSet = Utilities.GetTimeZoneOffsetForUser(PortalId, Convert.ToInt32(dr["UserId"])),
                        UserCulture = Utilities.GetCultureInfoForUser(PortalId, Convert.ToInt32(dr["UserId"])),
                        TopicSubscriber = Utilities.SafeConvertBool(dr["TopicSubscriber"])
                    };

                    if (!(sl.Contains(si)))
                    {
                        if (Permissions.HasPerm(CanSubscribe, si.UserId, PortalId))
                        {
                            sl.Add(si);
                        }
                    }
                }
            }
            dr.Close();

            return sl;
        }
    }

    public abstract class Subscriptions
    {
        public static bool IsSubscribed(int PortalId, int ModuleId, int ForumId, int TopicId, SubscriptionTypes SubscriptionType, int AuthorId)
        {
            return new DotNetNuke.Modules.ActiveForums.Controllers.SubscriptionController().Subscribed(PortalId, ModuleId, AuthorId, ForumId, TopicId);
        }

        public static void SendSubscriptions(int PortalId, int ModuleId, int TabId, int ForumId, int TopicId, int ReplyId, int AuthorId)
        {
            var fc = new ForumController();
            Forum fi = fc.Forums_Get(ForumId, -1, false);
            SendSubscriptions(PortalId, ModuleId, TabId, fi, TopicId, ReplyId, AuthorId);
        }

        public static void SendSubscriptions(int PortalId, int ModuleId, int TabId, Forum fi, int TopicId, int ReplyId, int AuthorId)
        {
            SendSubscriptions(-1, PortalId, ModuleId, TabId, fi, TopicId, ReplyId, AuthorId);
        }
        
        public static void SendSubscriptions(int TemplateId, int PortalId, int ModuleId, int TabId, Forum fi, int TopicId, int ReplyId, int AuthorId)
        {
            var sc = new SubscriptionController();
            List<SubscriptionInfo> subs = sc.Subscription_GetSubscribers(PortalId, fi.ForumID, TopicId, SubscriptionTypes.Instant, AuthorId, fi.Security.Subscribe);
            
            if (subs.Count > 0)
            {
                Email.SendTemplatedEmail(TemplateId, PortalId, TopicId, ReplyId, ModuleId, TabId, string.Empty, AuthorId, fi, subs);
            }
        }

        public static void SendSubscriptions(SubscriptionTypes SubscriptionType, DateTime StartDate)
        {
            string sysTemplateName = "DailyDigest";
            if (SubscriptionType == SubscriptionTypes.WeeklyDigest)
            {
                sysTemplateName = "WeeklyDigest";
            }
            
            var objRecipients = new ArrayList();
            IDataReader dr = DataProvider.Instance().Subscriptions_GetDigest(Convert.ToString(SubscriptionType), StartDate);
            
            string tmpEmail = string.Empty;
            string tmpFG = string.Empty;
            string sBody = string.Empty;
            var sb = new System.Text.StringBuilder();
            string Template = string.Empty;
            string TemplatePlainText = string.Empty;
            string TemplateSubject = string.Empty;
            int tmpModuleId = 0;
            string TemplateHeader = string.Empty;
            string TemplateBody = string.Empty;
            string TemplateFooter = string.Empty;
            string TemplateItems = string.Empty;
            string TemplateGroupSection = string.Empty;
            string ItemsFooter = string.Empty;
            string Items = string.Empty;
            string sMessageBody;
            string FromEmail = string.Empty;
            //Dim newBody As String
            string SubscriberDisplayName = string.Empty;
            string SubscriberUserName = string.Empty;
            string SubscriberFirstName = string.Empty;
            string SubscriberLastName = string.Empty;
            string SubscriberEmail = string.Empty;
            int portalId = -1;
            int i = 0;
            int GroupCount = 0;

            while (dr.Read())
            {
                portalId = Convert.ToInt32(dr["PortalId"].ToString());

                SubscriberDisplayName = dr["SubscriberDisplayName"].ToString();
                SubscriberUserName = dr["SubscriberUserName"].ToString();
                SubscriberFirstName = dr["SubscriberFirstName"].ToString();
                SubscriberLastName = dr["SubscriberLastName"].ToString();
                SubscriberEmail = dr["Email"].ToString();
                
                string newEmail = dr["Email"].ToString();
                if (i > 0)
                {
                    if (newEmail != tmpEmail)
                    {
                        sMessageBody = TemplateHeader;
                        sMessageBody += Items + ItemsFooter;
                        sMessageBody += TemplateFooter;
                        Items = string.Empty;
                        sMessageBody = sMessageBody.Replace("[SUBSCRIBERDISPLAYNAME]", SubscriberDisplayName);
                        sMessageBody = sMessageBody.Replace("[SUBSCRIBERUSERNAME]", SubscriberUserName);
                        sMessageBody = sMessageBody.Replace("[SUBSCRIBERFIRSTNAME]", SubscriberFirstName);
                        sMessageBody = sMessageBody.Replace("[SUBSCRIBERLASTNAME]", SubscriberLastName);
                        sMessageBody = sMessageBody.Replace("[SUBSCRIBEREMAIL]", SubscriberEmail);
                        sMessageBody = sMessageBody.Replace("[DATE]", DateTime.UtcNow.ToString());

                        if ((sMessageBody.IndexOf("[DATE:", 0) + 1) > 0)
                        {
                            string sFormat;
                            int inStart = (sMessageBody.IndexOf("[DATE:", 0) + 1) + 5;
                            int inEnd = (sMessageBody.IndexOf("]", inStart - 1) + 1) - 1;
                            string sValue = sMessageBody.Substring(inStart, inEnd - inStart);
                            sFormat = sValue;
                            sMessageBody = sMessageBody.Replace(string.Concat("[DATE:", sFormat, "]"), DateTime.UtcNow.ToString(sFormat));
                        }

                        GroupCount = 0;
                        tmpEmail = newEmail;
                        Queue.Controller.Add(portalId, FromEmail, tmpEmail, TemplateSubject, sMessageBody, "TestPlainText", string.Empty, string.Empty);
                        i = 0;
                    }
                }
                else
                {
                    tmpEmail = dr["Email"].ToString();

                }

                if (tmpFG != string.Concat(dr["GroupName"], dr["ForumName"].ToString()))
                {
                    if (GroupCount > 0)
                    {
                        Items += ItemsFooter;
                    }
                    
                    string sTmpBody = TemplateGroupSection;

                    sTmpBody = sTmpBody.Replace("[TABID]", dr["TabId"].ToString());
                    sTmpBody = sTmpBody.Replace("[PORTALID]", dr["PortalId"].ToString());
                    sTmpBody = sTmpBody.Replace("[GROUPNAME]", dr["GroupName"].ToString());
                    sTmpBody = sTmpBody.Replace("[FORUMNAME]", dr["ForumName"].ToString());
                    sTmpBody = sTmpBody.Replace("[FORUMID]", dr["ForumId"].ToString());
                    sTmpBody = sTmpBody.Replace("[GROUPID]", dr["ForumGroupId"].ToString());
                    Items += sTmpBody;

                    GroupCount += 1;
                }
                string sTemp = TemplateItems;
                sTemp = sTemp.Replace("[SUBJECT]", dr["Subject"].ToString());
                int intLength = 0;

                if ((sTemp.IndexOf("[BODY:", 0) + 1) > 0)
                {
                    int inStart = (sTemp.IndexOf("[BODY:", 0) + 1) + 5;
                    int inEnd = (sTemp.IndexOf("]", inStart - 1) + 1) - 1;
                    string sLength = sTemp.Substring(inStart, inEnd - inStart);
                    intLength = Convert.ToInt32(sLength);
                }
                
                string body = dr["Body"].ToString();
                
                if (intLength > 0)
                {
                    if (body.Length > intLength)
                    {
                        body = body.Substring(0, intLength);
                        body = string.Concat(body, "...");
                    }
                    sTemp = sTemp.Replace(string.Concat("[BODY:", intLength, "]"), body);
                }
                else
                {
                    sTemp = sTemp.Replace("[BODY]", body);
                }

                sTemp = sTemp.Replace("[FORUMID]", dr["ForumId"].ToString());
                sTemp = sTemp.Replace("[POSTID]", dr["PostId"].ToString());
                sTemp = sTemp.Replace("[DISPLAYNAME]", dr["DisplayName"].ToString());
                sTemp = sTemp.Replace("[USERNAME]", dr["UserName"].ToString());
                sTemp = sTemp.Replace("[FIRSTNAME]", dr["FirstName"].ToString());
                sTemp = sTemp.Replace("[LASTNAME]", dr["LASTNAME"].ToString());
                sTemp = sTemp.Replace("[REPLIES]", dr["Replies"].ToString());
                sTemp = sTemp.Replace("[VIEWS]", dr["Views"].ToString());
                sTemp = sTemp.Replace("[TABID]", dr["TabId"].ToString());
                sTemp = sTemp.Replace("[PORTALID]", dr["PortalId"].ToString());

                Items += sTemp;

                i += 1;
            }

            dr.Close();
            dr = null;
            
            if (Items != string.Empty)
            {
                sMessageBody = TemplateHeader;
                sMessageBody += Items + ItemsFooter;
                sMessageBody += TemplateFooter;
                sMessageBody = sMessageBody.Replace("[SUBSCRIBERDISPLAYNAME]", SubscriberDisplayName);
                sMessageBody = sMessageBody.Replace("[SUBSCRIBERUSERNAME]", SubscriberUserName);
                sMessageBody = sMessageBody.Replace("[SUBSCRIBERFIRSTNAME]", SubscriberFirstName);
                sMessageBody = sMessageBody.Replace("[SUBSCRIBERLASTNAME]", SubscriberLastName);
                sMessageBody = sMessageBody.Replace("[SUBSCRIBEREMAIL]", SubscriberEmail);
                sMessageBody = sMessageBody.Replace("[DATE]", DateTime.UtcNow.ToString());

                if ((sMessageBody.IndexOf("[DATE:", 0) + 1) > 0)
                {
                    string sFormat;
                    int inStart = (sMessageBody.IndexOf("[DATE:", 0) + 1) + 5;
                    int inEnd = (sMessageBody.IndexOf("]", inStart - 1) + 1) - 1;
                    string sValue = sMessageBody.Substring(inStart, inEnd - inStart);
                    sFormat = sValue;
                    sMessageBody = sMessageBody.Replace(string.Concat("[DATE:", sFormat, "]"), DateTime.UtcNow.ToString(sFormat));
                }
                Queue.Controller.Add(portalId, FromEmail, tmpEmail, TemplateSubject, sMessageBody, "TestPlainText", string.Empty, string.Empty);
            }
        }
    }

    public class WeeklyDigest : DotNetNuke.Services.Scheduling.SchedulerClient
    {
        public WeeklyDigest(DotNetNuke.Services.Scheduling.ScheduleHistoryItem objScheduleHistoryItem) : base()
        {
            ScheduleHistoryItem = objScheduleHistoryItem;
        }

        public override void DoWork()
        {
            try
            {
                Subscriptions.SendSubscriptions(SubscriptionTypes.WeeklyDigest, GetStartOfWeek(DateTime.UtcNow.AddDays(-1)));
                ScheduleHistoryItem.Succeeded = true;
                ScheduleHistoryItem.TimeLapse = GetElapsedTimeTillNextStart();
                ScheduleHistoryItem.AddLogNote("Weekly Digest Complete");

            }
            catch (Exception ex)
            {
                ScheduleHistoryItem.Succeeded = false;
                ScheduleHistoryItem.AddLogNote(string.Concat("Weekly Digest Failed:", ex));
                Errored(ref ex);
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        private static DateTime GetStartOfWeek(DateTime StartDate)
        {
            switch (StartDate.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    return StartDate.AddDays(0);
                case DayOfWeek.Tuesday:
                    return StartDate.AddDays(-1);
                case DayOfWeek.Wednesday:
                    return StartDate.AddDays(-2);
                case DayOfWeek.Thursday:
                    return StartDate.AddDays(-3);
                case DayOfWeek.Friday:
                    return StartDate.AddDays(-4);
                case DayOfWeek.Saturday:
                    return StartDate.AddDays(-5);
                case DayOfWeek.Sunday:
                    return StartDate.AddDays(-6);
            }

            return DateTime.MinValue;
        }

        private static int GetElapsedTimeTillNextStart()
        {
            DateTime NextRun = DateTime.UtcNow.AddDays(7);
            var nextStart = new DateTime(NextRun.Year, NextRun.Month, NextRun.Day, 22, 0, 0);
            int elapseMinutes = Convert.ToInt32((nextStart.Ticks - DateTime.UtcNow.Ticks) / TimeSpan.TicksPerDay);
            return elapseMinutes;
        }
    }
}