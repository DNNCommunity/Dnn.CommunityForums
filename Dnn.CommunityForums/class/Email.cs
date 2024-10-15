// Copyright (c) 2013-2024 by DNN Community
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

namespace DotNetNuke.Modules.ActiveForums
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net.Mail;
    using System.Reflection;
    using System.Threading;
    using System.Web;
    using System.Xml.Linq;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Roles;

    [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Use DotNetNuke.Modules.ActiveForums.Entities.Email.")]
    public class Email : DotNetNuke.Modules.ActiveForums.Entities.EmailInfo
    {
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use Body property.")]
        public string BodyText { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Use Body property.")]
        public string BodyHTML { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Use DotNetNuke.Modules.ActiveForums.Controllers.EmailController.SendEMail().")]
        public static void SendEmail(int templateId, int portalId, int moduleId, int tabId, int forumId, int topicId, int replyId, string comments, Author author) => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Use DotNetNuke.Modules.ActiveForums.Controllers.EmailController.SendEmailToModerators().")]
        public static void SendEmailToModerators(int templateId, int portalId, int forumId, int topicId, int replyId, int moduleID, int tabID, string comments) => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Use DotNetNuke.Modules.ActiveForums.Controllers.EmailController.SendEmailToModerators().")]
        public static void SendEmailToModerators(int templateId, int portalId, int forumId, int topicId, int replyId, int moduleID, int tabID, string comments, UserInfo user) => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Use DotNetNuke.Modules.ActiveForums.Controller.EmailController.SendTemplatedEmail()")]
        public static void SendTemplatedEmail(int templateId, int portalId, int topicId, int replyId, int moduleID, int tabID, string comments, int userId, Forum fi, List<SubscriptionInfo> subs) => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Use DotNetNuke.Modules.ActiveForums.Controller.EmailController.SendNotification(int portalId, int moduleId, string fromEmail, string toEmail, string subject, string body).")]
        public static void SendNotification(int portalId, int moduleId, string fromEmail, string toEmail, string subject, string bodyText, string bodyHTML) => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Use DotNetNuke.Modules.ActiveForums.Controller.EmailController.SendNotification(int portalId, int moduleId, string fromEmail, string toEmail, string subject, string body).")]
        public static void SendNotification(string fromEmail, string toEmail, string subject, string bodyText, string bodyHTML) => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Use  DotNetNuke.Modules.ActiveForums.Controller.EmailController.SendNotification(int portalId, int moduleId, string fromEmail, string toEmail, string subject, string body).")]
        public static void SendNotification(int portalId, string fromEmail, string toEmail, string subject, string bodyText, string bodyHTML) => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Scheduled removal in v9.0.0.0. Use  DotNetNuke.Modules.ActiveForums.Controller.EmailController.Send().")]
        public void Send() => throw new NotImplementedException();
    }
}
