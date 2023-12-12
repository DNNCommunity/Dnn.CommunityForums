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
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Xml.Linq;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;

namespace DotNetNuke.Modules.ActiveForums
{
    public class Email
    {
        public int PortalId;
        public string Subject;
        public string From;
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use Body property.")]
        public string BodyText;
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use Body property.")]
        public string BodyHTML;
        public string Body;

        public List<SubscriptionInfo> Recipients;

        public bool UseQueue = false;

        public static void SendEmail(int templateId, int portalId, int moduleId, int tabId, int forumId, int topicId, int replyId, string comments, Author author)
        {
            var portalSettings = (DotNetNuke.Entities.Portals.PortalSettings)(HttpContext.Current.Items["PortalSettings"]);
            var mainSettings = SettingsBase.GetModuleSettings(moduleId);
            var sTemplate = string.Empty;
            var tc = new TemplateController();
            var ti = tc.Template_Get(templateId);
            string subject = TemplateUtils.ParseEmailTemplate(ti.Subject, string.Empty, portalId, moduleId, tabId, forumId, topicId, replyId, string.Empty, author.AuthorId, Utilities.GetCultureInfoForUser(portalId, author.AuthorId), Utilities.GetTimeZoneOffsetForUser(portalId, author.AuthorId));
            string body = TemplateUtils.ParseEmailTemplate(ti.Template, string.Empty, portalId, moduleId, tabId, forumId, topicId, replyId, string.Empty, author.AuthorId, Utilities.GetCultureInfoForUser(portalId, author.AuthorId), Utilities.GetTimeZoneOffsetForUser(portalId, author.AuthorId));
            body = body.Replace("[REASON]", comments);
            var fc = new ForumController();
            var fi = fc.Forums_Get(portalId: portalId, moduleId: moduleId, forumId: forumId, useCache: true);
            var sFrom = fi.EmailAddress != string.Empty ? fi.EmailAddress : portalSettings.Email;

            //Send now

            var oEmail = new Email();
            var subs = new List<SubscriptionInfo>();
            var si = new SubscriptionInfo
            {
                UserId = author.AuthorId,
                Email = author.Email,
            };

            subs.Add(si);
            oEmail.PortalId = portalId;
            oEmail.UseQueue = mainSettings.MailQueue;
            oEmail.Recipients = subs;
            oEmail.Subject = subject;
            oEmail.From = sFrom;
            oEmail.Body = body;

            new Thread(oEmail.Send).Start();
        }

        public static void SendEmailToModerators(int templateId, int portalId, int forumId, int topicId, int replyId, int moduleID, int tabID, string comments)
        {
            SendEmailToModerators(templateId, portalId, forumId, topicId, replyId, moduleID, tabID, comments, null);
        }

        public static void SendEmailToModerators(int templateId, int portalId, int forumId, int topicId, int replyId, int moduleID, int tabID, string comments, UserInfo user)
        {
            var fc = new ForumController();
            var fi = fc.Forums_Get(portalId: portalId, moduleId: moduleID, forumId: forumId, useCache: true);
            if (fi == null)
                return;

            var subs = new List<SubscriptionInfo>();
            var rc = new Security.Roles.RoleController();
            var rp = RoleProvider.Instance();
            var uc = new DotNetNuke.Entities.Users.UserController();
            var modApprove = fi.Security.ModApprove;
            var modRoles = modApprove.Split('|')[0].Split(';');
            foreach (var r in modRoles)
            {
                if (string.IsNullOrEmpty(r)) continue;
                var rid = Convert.ToInt32(r);
                var rName = DotNetNuke.Security.Roles.RoleController.Instance.GetRoleById(portalId, rid).RoleName;
                foreach (UserRoleInfo usr in rp.GetUserRoles(portalId, null, rName))
                {
                    var ui = uc.GetUser(portalId, usr.UserID);
                    var si = new SubscriptionInfo
                    {
                        UserId = ui.UserID,
                        Email = ui.Email,
                        TimeZoneOffSet = Utilities.GetTimeZoneOffsetForUser(portalId, ui.UserID),
                        UserCulture = Utilities.GetCultureInfoForUser(portalId, ui.UserID),
                        TopicSubscriber = false
                    };
                    if (!(subs.Contains(si)))
                    {
                        subs.Add(si);
                    }
                }
            }
            if (subs.Count > 0)
            {
                SendTemplatedEmail(templateId, portalId, topicId, replyId, moduleID, tabID, comments, user.UserID, fi, subs);
            }
        }
        public static void SendTemplatedEmail(int templateId, int portalId, int topicId, int replyId, int moduleID, int tabID, string comments, int userId, ForumInfo fi, List<SubscriptionInfo> subs)
        {
            PortalSettings portalSettings = (DotNetNuke.Entities.Portals.PortalSettings)(HttpContext.Current.Items["PortalSettings"]);
            SettingsInfo mainSettings = SettingsBase.GetModuleSettings(moduleID);

            TemplateController tc = new TemplateController();
            TemplateUtils.lstSubscriptionInfo = subs;
            TemplateInfo ti = templateId > 0 ? tc.Template_Get(templateId) : tc.Template_Get("SubscribedEmail", portalId, moduleID);
            IEnumerable<CultureInfo> userCultures = subs.Select(s => s.UserCulture).Distinct();
            foreach (CultureInfo userCulture in userCultures)
            {
                IEnumerable<TimeSpan> timeZoneOffsets = subs.Where(s => s.UserCulture == userCulture).Select(s => s.TimeZoneOffSet).Distinct();
                foreach (TimeSpan timeZoneOffset in timeZoneOffsets)
                {
                    string sTemplate = string.Empty;
                    string sFrom = fi.EmailAddress != string.Empty ? fi.EmailAddress : portalSettings.Email;
                    /* subject/text/body, etc. can now be different based on topic subscriber vs forum subscriber so process first for topic subscribers and then for forum subscribers */
                    Email oEmail = new Email /* subject can be different based on topic subscriber vs forum subscriber so process first for topic subscribers and then for forum subscribers */
                    {
                        PortalId = portalId,
                        Recipients = subs.Where(s => s.TimeZoneOffSet == timeZoneOffset && s.UserCulture == userCulture && s.TopicSubscriber).ToList(),
                        Subject = TemplateUtils.ParseEmailTemplate(ti.Subject, templateName: string.Empty, portalID: portalId, moduleID: moduleID, tabID: tabID, forumID: fi.ForumID, topicId: topicId, replyId: replyId, comments: string.Empty, userId: userId, userCulture: userCulture, timeZoneOffset: timeZoneOffset, topicSubscriber: true),
                        Body = TemplateUtils.ParseEmailTemplate(ti.Template, templateName: string.Empty, portalID: portalId, moduleID: moduleID, tabID: tabID, forumID: fi.ForumID, topicId: topicId, replyId: replyId, comments: comments, userId: userId, userCulture: userCulture, timeZoneOffset: timeZoneOffset, topicSubscriber: true),
                        From = sFrom,
                        UseQueue = mainSettings.MailQueue
                    };
                    if (oEmail.Recipients.Count > 0)
                    {
                        new System.Threading.Thread(oEmail.Send).Start();
                    }
                    oEmail = new Email /* subject can be different based on topic subscriber vs forum subscriber so process first for topic subscribers and then for forum subscribers */
                    {
                        PortalId = portalId,
                        Recipients = subs.Where(s => s.TimeZoneOffSet == timeZoneOffset && s.UserCulture == userCulture && !s.TopicSubscriber).ToList(),
                        Subject = TemplateUtils.ParseEmailTemplate(ti.Subject, templateName: string.Empty, portalID: portalId, moduleID: moduleID, tabID: tabID, forumID: fi.ForumID, topicId: topicId, replyId: replyId, comments: string.Empty, userId: userId, userCulture: userCulture, timeZoneOffset: timeZoneOffset, topicSubscriber: false),
                        Body = TemplateUtils.ParseEmailTemplate(ti.Template, templateName: string.Empty, portalID: portalId, moduleID: moduleID, tabID: tabID, forumID: fi.ForumID, topicId: topicId, replyId: replyId, comments: comments, userId: userId, userCulture: userCulture, timeZoneOffset: timeZoneOffset, topicSubscriber: false),
                        From = sFrom,
                        UseQueue = mainSettings.MailQueue
                    };
                    if (oEmail.Recipients.Count > 0)
                    {
                        new System.Threading.Thread(oEmail.Send).Start();
                    }

                }
            }
        }
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use SendNotification(int portalId, string fromEmail, string toEmail, string subject, string body).")]
        public static void SendNotification(string fromEmail, string toEmail, string subject, string bodyText, string bodyHTML)
        {
            SendNotification(-1, fromEmail, toEmail, subject, bodyHTML);
        }
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use SendNotification(int portalId, string fromEmail, string toEmail, string subject, string body).")]
        public static void SendNotification(int portalId, string fromEmail, string toEmail, string subject, string bodyText, string bodyHTML)
        {
            SendNotification(portalId, fromEmail, toEmail, subject, bodyHTML);
        }
        public static void SendNotification(int portalId, string fromEmail, string toEmail, string subject, string body)
        {
                //USE DNN API for this to ensure proper delivery & adherence to portal settings
                //Services.Mail.Mail.SendEmail(fromEmail, fromEmail, toEmail, subject, bodyHTML);

                //Since this code is triggered from the DNN scheduler, the default/simple API (now commented out above) uses Host rather than Portal-specific SMTP configuration
                //updated here to retrieve portal-specific SMTP configuration and use more elaborate DNN API that allows passing of the SMTP information rather than rely on DNN API DotNetNuke.Host.SMTP property accessors to determine portal vs. host SMTP values 
                DotNetNuke.Services.Mail.Mail.SendMail(mailFrom: fromEmail,
                                        mailSender: (SMTPPortalEnabled(portalId) ? PortalController.Instance.GetPortal(portalId).Email : Host.HostEmail),
                                        mailTo: toEmail,
                                        cc: string.Empty,
                                        bcc: string.Empty,
                                        replyTo: string.Empty,
                                        priority: DotNetNuke.Services.Mail.MailPriority.Normal,
                                        subject: subject,
                                        bodyFormat: DotNetNuke.Services.Mail.MailFormat.Html,
                                        bodyEncoding: System.Text.Encoding.Default,
                                        body: body,
                                        attachments: new List<System.Net.Mail.Attachment>(),
                                        smtpServer: SMTPServer(portalId),
                                        smtpAuthentication: SMTPAuthentication(portalId),
                                        smtpUsername: SMTPUsername(portalId),
                                        smtpPassword: SMTPPassword(portalId),
                                        smtpEnableSSL: EnableSMTPSSL(portalId));
        }

        public void Send()
        {
            try
            {
                Subject = Subject.Replace("&#91;", "[");
                Subject = Subject.Replace("&#93;", "]");

                foreach (var si in Recipients.Where(si => si.Email != string.Empty))
                {
                    if (UseQueue)
                        Queue.Controller.Add(PortalId, From, si.Email, Subject, Body, string.Empty, string.Empty);
                    else
                        SendNotification(PortalId, From, si.Email, Subject, Body);
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        #region "code modeled on DotNetNuke.Services.Mail/DotNetNuke.Entities.Host APIs to support portal-specific SMTP configuration"
        internal static string SMTPServer(int portalId)
        {
            return GetSmtpSetting(portalId, "SMTPServer");
        }
        internal static string SMTPAuthentication(int portalId)
        {
            return GetSmtpSetting(portalId, "SMTPAuthentication");
        }
        internal static bool EnableSMTPSSL(int portalId)
        {
            if (SMTPPortalEnabled(portalId))
            {
                return PortalController.GetPortalSettingAsBoolean("SMTPEnableSSL", portalId, false);
            }
            else
            {
                return HostController.Instance.GetBoolean("SMTPEnableSSL", false);
            }
        }
        internal static string SMTPUsername(int portalId)
        {
            return GetSmtpSetting(portalId, "SMTPUsername");
        }
        internal static string SMTPPassword(int portalId)
        {
            if (SMTPPortalEnabled(portalId))
            {
                return PortalController.GetEncryptedString("SMTPPassword", portalId, Config.GetDecryptionkey());
            }
            else
            {
                string decryptedText;
                try
                {
                    decryptedText = HostController.Instance.GetEncryptedString("SMTPPassword", Config.GetDecryptionkey());
                }
                catch (Exception)
                {
                    //fixes case where smtppassword failed to encrypt due to failing upgrade
                    var current = HostController.Instance.GetString("SMTPPassword");
                    if (!string.IsNullOrEmpty(current))
                    {
                        HostController.Instance.UpdateEncryptedString("SMTPPassword", current, Config.GetDecryptionkey());
                        decryptedText = current;
                    }
                    else
                    {
                        decryptedText = string.Empty;
                    }
                }
                return decryptedText;
            }
        }

        internal static bool SMTPPortalEnabled(int portalId)
        {
            if (portalId != null && portalId != -1)
            {
                return PortalController.GetPortalSetting("SMTPmode", portalId, Null.NullString).Equals("P", StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                return false;
            }
        }

        internal static string GetSmtpSetting(int portalId, string settingName)
        {
            if (SMTPPortalEnabled(portalId))
            {
                return PortalController.GetPortalSetting(settingName, portalId, Null.NullString);
            }
            else
            {
                return HostController.Instance.GetString(settingName);
            }
        }
        #endregion
    }
}