// Copyright (c) by DNN Community
//
// DNN Community licenses this file to you under the MIT license.
//
// See the LICENSE file in the project root for more information.
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

namespace DotNetNuke.Modules.ActiveForums.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Web;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Modules.ActiveForums.Entities;
    using DotNetNuke.Modules.ActiveForums.Enums;
    using DotNetNuke.Modules.ActiveForums.Helpers;

    public class EmailController
    {
        private readonly DotNetNuke.Abstractions.Application.IHostSettingsService hostSettingsService;

        public EmailController()
            : this(hostSettingsService: new DotNetNuke.Entities.Controllers.HostController())
        {
        }

        public EmailController(IHostSettingsService hostSettingsService)
        {
            this.hostSettingsService = hostSettingsService;
        }

        [Obsolete("Deprecated in Community Forums. Scheduled removal in 10.00.00. Not Used.")]
        public static void SendEmail(int templateId, int portalId, int moduleId, int tabId, int forumId, int topicId, int replyId, string comments, DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo author) => throw new NotImplementedException();

        internal static void SendEmail(Enums.TemplateType templateType, int tabId, DotNetNuke.Modules.ActiveForums.Entities.ForumInfo fi, int topicId, int replyId, DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo author)
        {
            DotNetNuke.Abstractions.Portals.IPortalSettings portalSettings = new PortalSettingsHelper().GetPortalSettings(fi.PortalId);
            var sFrom = fi.FeatureSettings.EmailAddress != string.Empty ? fi.FeatureSettings.EmailAddress : portalSettings.Email;
            var subjectTemplate = !string.IsNullOrEmpty(fi.FeatureSettings.EmailNotificationSubjectTemplate) ?
                fi.FeatureSettings.EmailNotificationSubjectTemplate :
                (!string.IsNullOrEmpty(fi.ForumGroup.FeatureSettings.EmailNotificationSubjectTemplate) ?
                    fi.ForumGroup.FeatureSettings.EmailNotificationSubjectTemplate :
                    SettingsBase.GetModuleSettings(fi.ModuleId).DefaultFeatureSettings.EmailNotificationSubjectTemplate);
            var bodyTemplate = DotNetNuke.Modules.ActiveForums.Controllers.TemplateController.Template_Get(fi.ModuleId, templateType, fi.FeatureSettings.TemplateFileNameSuffix, null);
            var subject = TemplateUtils.ParseEmailTemplate(subjectTemplate, fi.PortalId, fi.ModuleId, tabId, fi.ForumID, topicId, replyId, author, accessingUser: author.ForumUser, topicSubscriber: false, new Services.URLNavigator().NavigationManager(), HttpContext.Current.Request.Url, HttpContext.Current.Request.RawUrl);
            var body = TemplateUtils.ParseEmailTemplate(bodyTemplate, fi.PortalId, fi.ModuleId, tabId, fi.ForumID, topicId, replyId, author, accessingUser: author.ForumUser, topicSubscriber: false, new Services.URLNavigator().NavigationManager(), HttpContext.Current.Request.Url, HttpContext.Current.Request.RawUrl);

            // Send now
            var recipients = new List<string>
            {
                author.Email,
            };
            DotNetNuke.Modules.ActiveForums.Controllers.EmailController.Send(new DotNetNuke.Modules.ActiveForums.Entities.EmailInfo()
            {
                Body = body,
                From = sFrom,
                ModuleId = fi.ModuleId,
                PortalId = fi.PortalId,
                Recipients = recipients,
                Subject = subject,
            });
        }

        internal static void SendTemplatedEmail(int topicId, int replyId, int moduleId, int tabId, DotNetNuke.Modules.ActiveForums.Entities.AuthorInfo author, DotNetNuke.Modules.ActiveForums.Entities.ForumInfo fi, List<DotNetNuke.Modules.ActiveForums.Entities.SubscriptionInfo> subs, Uri requestUrl, string rawUrl)
        {
            var navigationManager = (INavigationManager)new Services.URLNavigator().NavigationManager();
            DotNetNuke.Abstractions.Portals.IPortalSettings portalSettings = new PortalSettingsHelper().GetPortalSettings(fi.PortalId);
            var lstSubscriptionInfo = subs;
            var bodyTemplate = DotNetNuke.Modules.ActiveForums.Controllers.TemplateController.Template_Get(moduleId, TemplateType.SubscribedEmail, fi.FeatureSettings.TemplateFileNameSuffix, null);
            var subjectTemplate = !string.IsNullOrEmpty(fi.FeatureSettings.EmailNotificationSubjectTemplate) ?
                fi.FeatureSettings.EmailNotificationSubjectTemplate :
                (!string.IsNullOrEmpty(fi.ForumGroup.FeatureSettings.EmailNotificationSubjectTemplate) ?
                    fi.ForumGroup.FeatureSettings.EmailNotificationSubjectTemplate :
                    SettingsBase.GetModuleSettings(moduleId).DefaultFeatureSettings.EmailNotificationSubjectTemplate);
            IEnumerable<CultureInfo> userCultures = subs.Select(s => s.UserCulture).Distinct();
            foreach (CultureInfo userCulture in userCultures)
            {
                IEnumerable<TimeSpan> timeZoneOffsets = subs.Where(s => s.UserCulture.Equals(userCulture)).Select(s => s.TimeZoneOffSet).Distinct();
                foreach (TimeSpan timeZoneOffset in timeZoneOffsets)
                {
                    string sFrom = fi.FeatureSettings.EmailAddress != string.Empty ? fi.FeatureSettings.EmailAddress : portalSettings.Email;

                    /* subject and body, etc. can now be different based on topic subscriber vs forum subscriber so process first for topic subscribers and then for forum subscribers;
                       in addition, user-specific tokens are now supported in email templates, so need to process template and send email uniquely for each user */
                    var topicSubscribers = subs.Where(s => s.TimeZoneOffSet == timeZoneOffset && s.UserCulture.Equals(userCulture) && s.TopicSubscriber && !string.IsNullOrEmpty(s.Email)).ToList();
                    foreach (var topicSubscriber in topicSubscribers)
                    {
                        DotNetNuke.Modules.ActiveForums.Controllers.EmailController.Send(new DotNetNuke.Modules.ActiveForums.Entities.EmailInfo()
                        {
                            From = sFrom,
                            PortalId = fi.PortalId,
                            ModuleId = moduleId,
                            Recipients = new List<string>()
                            {
                                topicSubscriber.Email,
                            },
                            Subject = Utilities.StripHTMLTag(System.Net.WebUtility.HtmlDecode(TemplateUtils.ParseEmailTemplate(subjectTemplate, portalID: fi.PortalId, moduleID: moduleId, tabID: tabId, forumID: fi.ForumID, topicId: topicId, replyId: replyId, author: author, accessingUser: topicSubscriber.User, topicSubscriber: true, navigationManager: navigationManager, requestUrl: requestUrl, rawUrl: rawUrl))),
                            Body = TemplateUtils.ParseEmailTemplate(bodyTemplate, portalID: fi.PortalId, moduleID: moduleId, tabID: tabId, forumID: fi.ForumID, topicId: topicId, replyId: replyId, author: author, accessingUser: topicSubscriber.User, topicSubscriber: true, navigationManager: navigationManager, requestUrl: requestUrl, rawUrl: rawUrl),
                        });
                    }

                    var forumSubscribers = subs.Where(s => s.TimeZoneOffSet == timeZoneOffset && s.UserCulture.Equals(userCulture) && !s.TopicSubscriber && !string.IsNullOrEmpty(s.Email)).ToList();
                    foreach (var forumSubscriber in forumSubscribers)
                    {
                        DotNetNuke.Modules.ActiveForums.Controllers.EmailController.Send(new DotNetNuke.Modules.ActiveForums.Entities.EmailInfo()
                        {
                            From = sFrom,
                            ModuleId = moduleId,
                            PortalId = fi.PortalId,
                            Recipients = new List<string>()
                            {
                                forumSubscriber.Email,
                            },
                            Subject = Utilities.StripHTMLTag(System.Net.WebUtility.HtmlDecode(TemplateUtils.ParseEmailTemplate(subjectTemplate, portalID: fi.PortalId, moduleID: moduleId, tabID: tabId, forumID: fi.ForumID, topicId: topicId, replyId: replyId, author: author, accessingUser: forumSubscriber.User, topicSubscriber: false, navigationManager: navigationManager, requestUrl: requestUrl, rawUrl: rawUrl))),
                            Body = TemplateUtils.ParseEmailTemplate(bodyTemplate, portalID: fi.PortalId, moduleID: moduleId, tabID: tabId, forumID: fi.ForumID, topicId: topicId, replyId: replyId, author: author, accessingUser: forumSubscriber.User, topicSubscriber: false, navigationManager: navigationManager, requestUrl: requestUrl, rawUrl: rawUrl),
                        });
                    }
                }
            }
        }

        [Obsolete("Deprecated in Community Forums. Scheduled removal in 9.0.0. Use SendNotification(int portalId, int moduleId, string fromEmail, string toEmail, string subject, string body).")]
        public static void SendNotification(int portalId, int moduleId, string fromEmail, string toEmail, string subject, string bodyText, string bodyHTML) => throw new NotImplementedException();

        internal void SendNotification(int portalId, int moduleId, string fromEmail, string toEmail, string subject, string body)
        {
            // USE DNN API for this to ensure proper delivery & adherence to portal settings
            // Services.Mail.Mail.SendEmail(fromEmail, fromEmail, toEmail, subject, bodyHTML);

            // Since this code is triggered from the DNN scheduler, the default/simple API (now commented out above) uses Host rather than Portal-specific SMTP configuration
            // updated here to retrieve portal-specific SMTP configuration and use more elaborate DNN API that allows passing of the SMTP information rather than rely on DNN API DotNetNuke.Host.SMTP property accessors to determine portal vs. host SMTP values
            DotNetNuke.Services.Mail.Mail.SendMail(
                mailFrom: fromEmail,
                mailSender: this.SMTPPortalEnabled(portalId) ? PortalController.Instance.GetPortal(portalId).Email : Host.HostEmail,
                mailTo: toEmail,
                cc: string.Empty,
                bcc: string.Empty,
                replyTo: string.Empty,
                priority: DotNetNuke.Services.Mail.MailPriority.Normal,
                subject: subject,
                bodyFormat: DotNetNuke.Services.Mail.MailFormat.Html,
                bodyEncoding: System.Text.Encoding.UTF8,
                body: body,
                attachments: new System.Collections.Generic.List<DotNetNuke.Services.Mail.MailAttachment>(),
                smtpServer: this.SMTPServer(portalId),
                smtpAuthentication: this.SMTPAuthentication(portalId),
                smtpUsername: this.SMTPUsername(portalId),
                smtpPassword: this.SMTPPassword(portalId),
                smtpEnableSSL: this.EnableSMTPSSL(portalId));
        }

        internal static void Send(DotNetNuke.Modules.ActiveForums.Entities.EmailInfo message)
        {
            try
            {
                message.Subject = message.Subject.Replace("&#91;", "[");
                message.Subject = message.Subject.Replace("&#93;", "]");
                var mc = new DotNetNuke.Modules.ActiveForums.Controllers.EmailNotificationQueueController();
                foreach (var r in message.Recipients.Where(r => !string.IsNullOrEmpty(r)))
                {
                    mc.Add(portalId: message.PortalId, moduleId: message.ModuleId, message.From, emailTo: r, emailSubject: message.Subject, emailBody: message.Body);
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        #region "code modeled on DotNetNuke.Services.Mail/DotNetNuke.Entities.Host APIs to support portal-specific SMTP configuration"
        internal string SMTPServer(int portalId)
        {
            return this.GetSmtpSetting(portalId, "SMTPServer");
        }

        internal string SMTPAuthentication(int portalId)
        {
            return this.GetSmtpSetting(portalId, "SMTPAuthentication");
        }

        internal bool EnableSMTPSSL(int portalId)
        {
            return this.SMTPPortalEnabled(portalId)
                ? PortalController.GetPortalSettingAsBoolean("SMTPEnableSSL", portalId, false)
                : this.hostSettingsService.GetBoolean("SMTPEnableSSL", false);
        }

        internal string SMTPUsername(int portalId)
        {
            return this.GetSmtpSetting(portalId, "SMTPUsername");
        }

        internal string SMTPPassword(int portalId)
        {
            if (this.SMTPPortalEnabled(portalId))
            {
                return PortalController.GetEncryptedString("SMTPPassword", portalId, Config.GetDecryptionkey());
            }
            else
            {
                string decryptedText;
                try
                {
                    decryptedText = this.hostSettingsService.GetEncryptedString("SMTPPassword", Config.GetDecryptionkey());
                }
                catch (Exception)
                {
                    // fixes case where smtppassword failed to encrypt due to failing upgrade
                    var current = this.hostSettingsService.GetString("SMTPPassword");
                    if (!string.IsNullOrEmpty(current))
                    {
                        this.hostSettingsService.UpdateEncryptedString("SMTPPassword", current, Config.GetDecryptionkey());
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

        internal bool SMTPPortalEnabled(int portalId)
        {
            return !portalId.Equals(DotNetNuke.Common.Utilities.Null.NullInteger) && PortalController.GetPortalSetting("SMTPmode", portalId, Null.NullString).Equals("P", StringComparison.OrdinalIgnoreCase);
        }

        internal string GetSmtpSetting(int portalId, string settingName)
        {
            return this.SMTPPortalEnabled(portalId) ? PortalController.GetPortalSetting(settingName, portalId, Null.NullString) : this.hostSettingsService.GetString(settingName);
        }

        #endregion
    }
}
