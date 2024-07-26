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
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.Reflection;

    using DotNetNuke.Services.Scheduling;

    [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Entities.SubscriptionInfo.")]
    public class SubscriptionInfo : DotNetNuke.Modules.ActiveForums.Entities.SubscriptionInfo
    {
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Entities.SubscriptionInfo.")]
        public int UserId { get; set; }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Entities.SubscriptionInfo.")]
        public string Email { get; set; }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Entities.SubscriptionInfo.")]
        public TimeSpan TimeZoneOffSet { get; set; }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Entities.SubscriptionInfo.")]
        public CultureInfo UserCulture { get; set; }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Entities.SubscriptionInfo.")]
        public bool TopicSubscriber { get; set; }
    }

    public class SubscriptionController
    {
        // TODO: move to new DAL2 subscription controller
        public int Subscription_Update(int portalId, int moduleId, int forumId, int topicId, int mode, int userId, string userRoles = "")
        {
            if (userId == -1)
            {
                return -1;
            }

            if (string.IsNullOrEmpty(userRoles))
            {
                UserController uc = new UserController();
                User uu = uc.GetUser(portalId, moduleId, userId);
                userRoles = uu.UserRoles;
            }

            DotNetNuke.Modules.ActiveForums.Entities.ForumInfo fi = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Forums_Get(portalId, moduleId, forumId, false, -1);

            if (DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(fi.Security.Subscribe, userRoles))
            {
                return Convert.ToInt32(DataProvider.Instance().Subscription_Update(portalId, moduleId, forumId, topicId, mode, userId));
            }

            return -1;
        }

        // TODO: move to new DAL2 subscription controller
        public List<DotNetNuke.Modules.ActiveForums.Entities.SubscriptionInfo> Subscription_GetSubscribers(int portalId, int forumId, int topicId, SubscriptionTypes mode, int authorId, string canSubscribe)
        {
            DotNetNuke.Modules.ActiveForums.Entities.SubscriptionInfo si;
            var sl = new List<DotNetNuke.Modules.ActiveForums.Entities.SubscriptionInfo>();
            IDataReader dr = DataProvider.Instance().Subscriptions_GetSubscribers(portalId, forumId, topicId, (int)mode);
            while (dr.Read())
            {
                if (authorId != Convert.ToInt32(dr["UserId"]))
                {
                    si = new DotNetNuke.Modules.ActiveForums.Entities.SubscriptionInfo
                    {
                        UserId = Convert.ToInt32(dr["UserId"]),
                    };

                    if (!sl.Contains(si))
                    {
                        if (DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(canSubscribe, si.UserId, portalId))
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
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.SubscriptionController().Subscribed(PortalId, ModuleId, UserId, ForumId, TopicId).")]
        public static bool IsSubscribed(int portalId, int moduleId, int forumId, int topicId, SubscriptionTypes subscriptionType, int userId)
        {
            return new DotNetNuke.Modules.ActiveForums.Controllers.SubscriptionController().Subscribed(portalId, moduleId, userId, forumId, topicId);
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use DotNetNuke.Modules.ActiveForums.Controllers.SubscriptionController().Subscribed(PortalId, ModuleId, UserId, ForumId).")]
        public static bool IsSubscribed(int portalId, int moduleId, int forumId, SubscriptionTypes subscriptionType, int userId)
        {
            return new DotNetNuke.Modules.ActiveForums.Controllers.SubscriptionController().Subscribed(portalId, moduleId, userId, forumId);
        }

        // TODO: move to new DAL2 subscription controller
        public static void SendSubscriptions(int portalId, int moduleId, int tabId, int forumId, int topicId, int replyId, int authorId)
        {
            DotNetNuke.Modules.ActiveForums.Entities.ForumInfo fi = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(forumId: forumId, moduleId: moduleId);
            SendSubscriptions(portalId, moduleId, tabId, fi, topicId, replyId, authorId);
        }

        // TODO: move to new DAL2 subscription controller
        public static void SendSubscriptions(int portalId, int moduleId, int tabId, DotNetNuke.Modules.ActiveForums.Entities.ForumInfo fi, int topicId, int replyId, int authorId)
        {
            SendSubscriptions(-1, portalId, moduleId, tabId, fi, topicId, replyId, authorId, null);
        }

        // TODO: move to new DAL2 subscription controller
        public static void SendSubscriptions(int templateId, int portalId, int moduleId, int tabId, DotNetNuke.Modules.ActiveForums.Entities.ForumInfo fi, int topicId, int replyId, int authorId, Uri requestUrl)
        {
            var sc = new SubscriptionController();
            List<DotNetNuke.Modules.ActiveForums.Entities.SubscriptionInfo> subs = sc.Subscription_GetSubscribers(portalId, fi.ForumID, topicId, SubscriptionTypes.Instant, authorId, fi.Security.Subscribe);

            if (subs.Count > 0)
            {
                DotNetNuke.Modules.ActiveForums.Controllers.EmailController.SendTemplatedEmail(templateId, portalId, topicId, replyId, moduleId, tabId, string.Empty, authorId, fi, subs, requestUrl);
            }
        }

        // TODO: move to new DAL2 subscription controller
        public static void SendSubscriptions(SubscriptionTypes subscriptionType, DateTime startDate)
        {
            string sysTemplateName = "DailyDigest";
            if (subscriptionType == SubscriptionTypes.WeeklyDigest)
            {
                sysTemplateName = "WeeklyDigest";
            }

            var objRecipients = new ArrayList();
            IDataReader dr = DataProvider.Instance().Subscriptions_GetDigest(Convert.ToString(subscriptionType), startDate);

            string tmpEmail = string.Empty;
            string tmpFG = string.Empty;
            string sBody = string.Empty;
            var sb = new System.Text.StringBuilder();
            string template = string.Empty;
            string templateSubject = string.Empty;
            string templateHeader = string.Empty;
            string templateBody = string.Empty;
            string templateFooter = string.Empty;
            string templateItems = string.Empty;
            string templateGroupSection = string.Empty;
            string itemsFooter = string.Empty;
            string items = string.Empty;
            string sMessageBody;
            string fromEmail = string.Empty;
            string subscriberDisplayName = string.Empty;
            string subscriberUserName = string.Empty;
            string subscriberFirstName = string.Empty;
            string subscriberLastName = string.Empty;
            string subscriberEmail = string.Empty;
            int portalId = -1;
            int i = 0;
            int groupCount = 0;

            while (dr.Read())
            {
                portalId = Convert.ToInt32(dr["PortalId"].ToString());

                subscriberDisplayName = dr["SubscriberDisplayName"].ToString();
                subscriberUserName = dr["SubscriberUserName"].ToString();
                subscriberFirstName = dr["SubscriberFirstName"].ToString();
                subscriberLastName = dr["SubscriberLastName"].ToString();
                subscriberEmail = dr["Email"].ToString();

                string newEmail = dr["Email"].ToString();
                if (i > 0)
                {
                    if (newEmail != tmpEmail)
                    {
                        sMessageBody = templateHeader;
                        sMessageBody += items + itemsFooter;
                        sMessageBody += templateFooter;
                        items = string.Empty;
                        sMessageBody = sMessageBody.Replace("[SUBSCRIBERDISPLAYNAME]", subscriberDisplayName);
                        sMessageBody = sMessageBody.Replace("[SUBSCRIBERUSERNAME]", subscriberUserName);
                        sMessageBody = sMessageBody.Replace("[SUBSCRIBERFIRSTNAME]", subscriberFirstName);
                        sMessageBody = sMessageBody.Replace("[SUBSCRIBERLASTNAME]", subscriberLastName);
                        sMessageBody = sMessageBody.Replace("[SUBSCRIBEREMAIL]", subscriberEmail);
                        sMessageBody = sMessageBody.Replace("[DATE]", DateTime.UtcNow.ToString());

                        if ((sMessageBody.IndexOf("[DATE:", 0) + 1) > 0)
                        {
                            string sFormat;
                            int inStart = sMessageBody.IndexOf("[DATE:", 0) + 1 + 5;
                            int inEnd = sMessageBody.IndexOf("]", inStart - 1) + 1 - 1;
                            string sValue = sMessageBody.Substring(inStart, inEnd - inStart);
                            sFormat = sValue;
                            sMessageBody = sMessageBody.Replace(string.Concat("[DATE:", sFormat, "]"), DateTime.UtcNow.ToString(sFormat));
                        }

                        groupCount = 0;
                        tmpEmail = newEmail;
                        new DotNetNuke.Modules.ActiveForums.Controllers.EmailNotificationQueueController().Add(portalId, -1, fromEmail, tmpEmail, templateSubject, sMessageBody);
                        i = 0;
                    }
                }
                else
                {
                    tmpEmail = dr["Email"].ToString();
                }

                if (tmpFG != string.Concat(dr["GroupName"], dr["ForumName"].ToString()))
                {
                    if (groupCount > 0)
                    {
                        items += itemsFooter;
                    }

                    string sTmpBody = templateGroupSection;

                    sTmpBody = sTmpBody.Replace("[TABID]", dr["TabId"].ToString());
                    sTmpBody = sTmpBody.Replace("[PORTALID]", dr["PortalId"].ToString());
                    sTmpBody = sTmpBody.Replace("[GROUPNAME]", dr["GroupName"].ToString());
                    sTmpBody = sTmpBody.Replace("[FORUMNAME]", dr["ForumName"].ToString());
                    sTmpBody = sTmpBody.Replace("[FORUMID]", dr["ForumId"].ToString());
                    sTmpBody = sTmpBody.Replace("[GROUPID]", dr["ForumGroupId"].ToString());
                    items += sTmpBody;

                    groupCount += 1;
                }

                string sTemp = templateItems;
                sTemp = sTemp.Replace("[SUBJECT]", dr["Subject"].ToString());
                int intLength = 0;

                if ((sTemp.IndexOf("[BODY:", 0) + 1) > 0)
                {
                    int inStart = sTemp.IndexOf("[BODY:", 0) + 1 + 5;
                    int inEnd = sTemp.IndexOf("]", inStart - 1) + 1 - 1;
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

                items += sTemp;

                i += 1;
            }

            dr.Close();
            dr = null;

            if (items != string.Empty)
            {
                sMessageBody = templateHeader;
                sMessageBody += items + itemsFooter;
                sMessageBody += templateFooter;
                sMessageBody = sMessageBody.Replace("[SUBSCRIBERDISPLAYNAME]", subscriberDisplayName);
                sMessageBody = sMessageBody.Replace("[SUBSCRIBERUSERNAME]", subscriberUserName);
                sMessageBody = sMessageBody.Replace("[SUBSCRIBERFIRSTNAME]", subscriberFirstName);
                sMessageBody = sMessageBody.Replace("[SUBSCRIBERLASTNAME]", subscriberLastName);
                sMessageBody = sMessageBody.Replace("[SUBSCRIBEREMAIL]", subscriberEmail);
                sMessageBody = sMessageBody.Replace("[DATE]", DateTime.UtcNow.ToString());

                if ((sMessageBody.IndexOf("[DATE:", 0) + 1) > 0)
                {
                    string sFormat;
                    int inStart = sMessageBody.IndexOf("[DATE:", 0) + 1 + 5;
                    int inEnd = sMessageBody.IndexOf("]", inStart - 1) + 1 - 1;
                    string sValue = sMessageBody.Substring(inStart, inEnd - inStart);
                    sFormat = sValue;
                    sMessageBody = sMessageBody.Replace(string.Concat("[DATE:", sFormat, "]"), DateTime.UtcNow.ToString(sFormat));
                }

                new DotNetNuke.Modules.ActiveForums.Controllers.EmailNotificationQueueController().Add(portalId, -1, fromEmail, tmpEmail, templateSubject, sMessageBody);
            }
        }
    }

    public class WeeklyDigest : DotNetNuke.Services.Scheduling.SchedulerClient
    {
        public WeeklyDigest(DotNetNuke.Services.Scheduling.ScheduleHistoryItem objScheduleHistoryItem) : base()
        {
            this.ScheduleHistoryItem = objScheduleHistoryItem;
        }

        public override void DoWork()
        {
            try
            {
                Subscriptions.SendSubscriptions(SubscriptionTypes.WeeklyDigest, GetStartOfWeek(DateTime.UtcNow.AddDays(-1)));
                this.ScheduleHistoryItem.Succeeded = true;
                this.ScheduleHistoryItem.TimeLapse = GetElapsedTimeTillNextStart();
                this.ScheduleHistoryItem.AddLogNote("Weekly Digest Complete");
            }
            catch (Exception ex)
            {
                this.ScheduleHistoryItem.Succeeded = false;
                this.ScheduleHistoryItem.AddLogNote(string.Concat("Weekly Digest Failed:", ex));
                this.Errored(ref ex);
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        private static DateTime GetStartOfWeek(DateTime startDate)
        {
            switch (startDate.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    return startDate.AddDays(0);
                case DayOfWeek.Tuesday:
                    return startDate.AddDays(-1);
                case DayOfWeek.Wednesday:
                    return startDate.AddDays(-2);
                case DayOfWeek.Thursday:
                    return startDate.AddDays(-3);
                case DayOfWeek.Friday:
                    return startDate.AddDays(-4);
                case DayOfWeek.Saturday:
                    return startDate.AddDays(-5);
                case DayOfWeek.Sunday:
                    return startDate.AddDays(-6);
            }

            return DateTime.MinValue;
        }

        private static int GetElapsedTimeTillNextStart()
        {
            DateTime nextRun = DateTime.UtcNow.AddDays(7);
            var nextStart = new DateTime(nextRun.Year, nextRun.Month, nextRun.Day, 22, 0, 0);
            int elapseMinutes = Convert.ToInt32((nextStart.Ticks - DateTime.UtcNow.Ticks) / TimeSpan.TicksPerDay);
            return elapseMinutes;
        }
    }
}
