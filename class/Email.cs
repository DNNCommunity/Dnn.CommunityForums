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
using System.Net.Mail;
using System.Reflection;
using System.Threading;
using System.Web;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;

namespace DotNetNuke.Modules.ActiveForums
{
    public class Email : DotNetNuke.Modules.ActiveForums.Entities.Email
    {


        public static void SendEmail(int templateId, int portalId, int moduleId, int tabId, int forumId, int topicId, int replyId, string comments, Author author)
        {

            var portalSettings = (DotNetNuke.Entities.Portals.PortalSettings)(HttpContext.Current.Items["PortalSettings"]);
            var mainSettings = DataCache.MainSettings(moduleId);
            var sTemplate = string.Empty;
            var tc = new TemplateController();
            var ti = tc.Template_Get(templateId, portalId, moduleId);
            var subject = TemplateUtils.ParseEmailTemplate(ti.Subject, string.Empty, portalId, moduleId, tabId, forumId, topicId, replyId, string.Empty, author.AuthorId, Utilities.GetCultureInfoForUser(portalId, author.AuthorId), Utilities.GetTimeZoneOffsetForUser(portalId, author.AuthorId));
            var bodyText = TemplateUtils.ParseEmailTemplate(ti.TemplateText, string.Empty, portalId, moduleId, tabId, forumId, topicId, replyId, string.Empty, author.AuthorId, Utilities.GetCultureInfoForUser(portalId, author.AuthorId), Utilities.GetTimeZoneOffsetForUser(portalId, author.AuthorId));
            var bodyHTML = TemplateUtils.ParseEmailTemplate(ti.TemplateHTML, string.Empty, portalId, moduleId, tabId, forumId, topicId, replyId, string.Empty, author.AuthorId, Utilities.GetCultureInfoForUser(portalId, author.AuthorId), Utilities.GetTimeZoneOffsetForUser(portalId, author.AuthorId));
            bodyText = bodyText.Replace("[REASON]", comments);
            bodyHTML = bodyHTML.Replace("[REASON]", comments);
            var fc = new ForumController();
            var fi = fc.Forums_Get(forumId, -1, false, true);
            var sFrom = fi.EmailAddress != string.Empty ? fi.EmailAddress : portalSettings.Email;

            //Send now

            var subs = new List<SubscriptionInfo>();
            var si = new SubscriptionInfo
            {
                DisplayName = author.DisplayName,
                Email = author.Email,
                FirstName = author.FirstName,
                LastName = author.LastName,
                UserId = author.AuthorId,
                Username = author.Username
            };

            subs.Add(si);
            //new Thread(
            DotNetNuke.Modules.ActiveForums.Controllers.EmailController.Send(new DotNetNuke.Modules.ActiveForums.Entities.Email()
            {
                PortalId = portalId,
                ModuleId = moduleId,
                BodyHTML = bodyHTML,
                BodyText = bodyText,
                Recipients = subs,
                Subject = subject,
                From = sFrom
            });
            //).Start();
        }
        #region Deprecated
        public static void SendEmailToModerators(int templateId, int portalId, int forumId, int topicId, int replyId, int moduleID, int tabID, string comments)
        {
            DotNetNuke.Modules.ActiveForums.Controllers.EmailController.SendEmailToModerators(templateId: templateId, portalId: portalId, moduleID: moduleID, forumId: forumId, topicId: topicId, replyId: replyId, tabID: tabID, comments: comments, user: null);
        }

        public static void SendEmailToModerators(int templateId, int portalId, int forumId, int topicId, int replyId, int moduleID, int tabID, string comments, UserInfo user)
        {
            DotNetNuke.Modules.ActiveForums.Controllers.EmailController.SendEmailToModerators(templateId, portalId, forumId, topicId, replyId, moduleID, tabID, comments, user);
        }
        [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Use DotNetNuke.Modules.ActiveForums.Controller.EmailController.SendTemplatedEmail()")]
        public static void SendTemplatedEmail(int templateId, int portalId, int topicId, int replyId, int moduleID, int tabID, string comments, int userId, Forum fi, List<SubscriptionInfo> subs)
        {
            DotNetNuke.Modules.ActiveForums.Controllers.EmailController.SendTemplatedEmail(templateId, portalId, topicId, replyId, moduleID, tabID, comments, userId, fi, subs);
        }
        [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Use DotNetNuke.Modules.ActiveForums.Controller.EmailController.SendNotification(int portalId, int moduleId, string fromEmail, string toEmail, string subject, string bodyText, string bodyHTML).")]
        public static void SendNotification(int portalId, int moduleId, string fromEmail, string toEmail, string subject, string bodyText, string bodyHTML)
        {
            DotNetNuke.Modules.ActiveForums.Controllers.EmailController.SendNotification(portalId, moduleId, fromEmail, toEmail, subject, bodyText, bodyHTML);
        }
        [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Use DotNetNuke.Modules.ActiveForums.Controller.EmailController.SendNotification(int portalId, int moduleId, string fromEmail, string toEmail, string subject, string bodyText, string bodyHTML).")]
        public static void SendNotification(string fromEmail, string toEmail, string subject, string bodyText, string bodyHTML)
        {
            DotNetNuke.Modules.ActiveForums.Controllers.EmailController.SendNotification(-1, -1, fromEmail, toEmail, subject, bodyText, bodyHTML);
        }
        [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Use  DotNetNuke.Modules.ActiveForums.Controller.EmailController.SendNotification(int portalId, int moduleId, string fromEmail, string toEmail, string subject, string bodyText, string bodyHTML).")]
        public static void SendNotification(int portalId, string fromEmail, string toEmail, string subject, string bodyText, string bodyHTML)
        {
            DotNetNuke.Modules.ActiveForums.Controllers.EmailController.SendNotification(portalId, -1, fromEmail, toEmail, subject, bodyText, bodyHTML);
        }
        [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Use  DotNetNuke.Modules.ActiveForums.Controller.EmailController.Send().")]
        public void Send()
        {
            DotNetNuke.Modules.ActiveForums.Controllers.EmailController.Send(new DotNetNuke.Modules.ActiveForums.Entities.Email()
            {
                BodyText = this.BodyText,
                BodyHTML = this.BodyHTML,
                From = this.From,
                Subject = this.Subject,
                ModuleId = this.ModuleId,
                PortalId = this.PortalId,
                Recipients = this.Recipients
            });
        }

        #endregion  
    }
}