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
	public class Email
	{
		public int PortalId;
		public string Subject;
		public string From;
		public string BodyText;
		public string BodyHTML;

		public List<SubscriptionInfo> Recipients;
	
        public bool UseQueue = false;

		public static void SendEmail(int templateId, int portalId, int moduleId, int tabId, int forumId, int topicId, int replyId, string comments, Author author)
		{
			var portalSettings = (Entities.Portals.PortalSettings)(HttpContext.Current.Items["PortalSettings"]);
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
			
            var oEmail = new Email();
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
			oEmail.PortalId = portalId;
			oEmail.UseQueue = mainSettings.MailQueue;
			oEmail.Recipients = subs;
			oEmail.Subject = subject;
			oEmail.From = sFrom;
			oEmail.BodyText = bodyText;
			oEmail.BodyHTML = bodyHTML;

			new Thread(oEmail.Send).Start();
		}

		public static void SendEmailToModerators(int templateId, int portalId, int forumId, int topicId, int replyId, int moduleID, int tabID, string comments)
		{
			SendEmailToModerators(templateId, portalId, forumId, topicId, replyId, moduleID, tabID, comments, null);
		}

		public static void SendEmailToModerators(int templateId, int portalId, int forumId, int topicId, int replyId, int moduleID, int tabID, string comments, UserInfo user)
        {
            var fc = new ForumController();
            var fi = fc.Forums_Get(forumId, -1, false, true);
            if (fi == null)
                return;

            var subs = new List<SubscriptionInfo>();
            var rc = new Security.Roles.RoleController();
            var rp = RoleProvider.Instance();
            var uc = new Entities.Users.UserController();
            var modApprove = fi.Security.ModApprove;
            var modRoles = modApprove.Split('|')[0].Split(';');
            foreach (var r in modRoles)
            {
                if (string.IsNullOrEmpty(r)) continue;
                var rid = Convert.ToInt32(r);
                var rName = rc.GetRole(rid, portalId).RoleName;
                foreach (UserRoleInfo usr in rp.GetUserRoles(portalId, null, rName))
                {
                    var ui = uc.GetUser(portalId, usr.UserID);
                    var si = new SubscriptionInfo
                    {
                        UserId = ui.UserID,
                        DisplayName = ui.DisplayName,
                        Email = ui.Email,
                        FirstName = ui.FirstName,
                        LastName = ui.LastName,
						TimeZoneOffSet = Utilities.GetTimeZoneOffsetForUser(portalId, ui.UserID),
						UserCulture = Utilities.GetCultureInfoForUser(portalId, ui.UserID)
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
		public static void SendTemplatedEmail(int templateId, int portalId, int topicId, int replyId, int moduleID, int tabID, string comments, int userId, Forum fi, List<SubscriptionInfo> subs)
		{
			PortalSettings portalSettings = (Entities.Portals.PortalSettings)(HttpContext.Current.Items["PortalSettings"]);
			SettingsInfo mainSettings = DataCache.MainSettings(moduleID);

			TemplateController tc = new TemplateController();
			TemplateUtils.lstSubscriptionInfo = subs;
			TemplateInfo ti = templateId > 0 ? tc.Template_Get(templateId) : tc.Template_Get("SubscribedEmail", portalId, moduleID);
			IEnumerable<CultureInfo> userCultures = subs.Select(s => s.UserCulture).Distinct();
			foreach (CultureInfo userCulture in userCultures)
			{
				IEnumerable<TimeSpan> timeZoneOffsets = subs.Where(s=>s.UserCulture == userCulture).Select(s => s.TimeZoneOffSet).Distinct();
				foreach (TimeSpan timeZoneOffset in timeZoneOffsets)
				{
					string sTemplate = string.Empty;
					string subject = TemplateUtils.ParseEmailTemplate(ti.Subject, string.Empty, portalId, moduleID, tabID, fi.ForumID, topicId, replyId, string.Empty, userId, userCulture, timeZoneOffset);
					string bodyText = TemplateUtils.ParseEmailTemplate(ti.TemplateText, string.Empty, portalId, moduleID, tabID, fi.ForumID, topicId, replyId, comments, userId, userCulture, timeZoneOffset);
					string bodyHTML = TemplateUtils.ParseEmailTemplate(ti.TemplateHTML, string.Empty, portalId, moduleID, tabID, fi.ForumID, topicId, replyId, comments, userId, userCulture, timeZoneOffset);
					string sFrom = fi.EmailAddress != string.Empty ? fi.EmailAddress : portalSettings.Email;
					Email oEmail = new Email
					{
						PortalId = portalId,
						Recipients = subs.Where(s => s.TimeZoneOffSet == timeZoneOffset && s.UserCulture == userCulture).ToList(),
						Subject = subject,
						From = sFrom,
						BodyText = bodyText,
						BodyHTML = bodyHTML,
						UseQueue = mainSettings.MailQueue
					};
					new System.Threading.Thread(oEmail.Send).Start();
				}
			}
		}		
        public static void SendAdminWatchEmail(int postID, int userID)
		{
			//TODO: Come back to fix and mod list
			// Try
			//    Dim _portalSettings As Entities.Portals.PortalSettings = CType(Current.Items("PortalSettings"), Entities.Portals.PortalSettings)
			//    Dim objsubs As New SubscriptionController
			//    Dim intPost As Integer
			//    Dim objPosts As New PostController
			//    Dim objPost As PostInfo = objPosts.NTForums_GetPostByID(PostID)
			//    Dim strSubject As String = _portalSettings.PortalName & " Forums:" & objPost.Subject
			//    If objPost.ParentPostID = 0 Then
			//        intPost = objPost.PostID
			//    Else
			//        intPost = objPost.ParentPostID
			//    End If
			//    Dim ForumID As Integer = objPost.ForumID
			//    Dim objForums As New ForumController
			//    Dim objForum As ForumInfo = objForums.Forums_Get(objPost.ForumID)

			//    Dim myTemplate As String
			//    myTemplate = CType(Current.Cache("AdminWatchEmail" & objForum.ModuleId), String)
			//    If myTemplate Is Nothing Then
			//        TemplateUtils.LoadTemplateCache(objForum.ModuleId)
			//        myTemplate = CType(Current.Cache("AdminWatchEmail" & objForum.ModuleId), String)
			//    End If
			//    Dim arrMods As String()
			//    'TODO: Come back and properly get list of moderators
			//    'arrMods = Split(objForum.CanModerate, ";")
			//    Dim i As Integer = 0
			//    Dim sLink As String
			//    sLink = Common.Globals.GetPortalDomainName(_portalSettings.PortalAlias.HTTPAlias, Current.Request) & "/default.aspx?tabid=" & _portalSettings.ActiveTab.TabID & "&view=topic&forumid=" & objPost.ForumID & "&postid=" & intPost
			//    Dim PortalId As Integer = _portalSettings.PortalId
			//    Dim FromEmail As String = _portalSettings.Email
			//    Try
			//        If Not String.IsNullOrEmpty(objForum.EmailAddress) Then
			//            FromEmail = objForum.EmailAddress
			//        End If
			//    Catch ex As Exception

			//    End Try
			//    For i = 0 To UBound(arrMods) - 1
			//        Dim objUserController As New Entities.Users.UserController
			//        Dim objRoleController As New Security.Roles.RoleController
			//        Dim RoleName As String = objRoleController.GetRole(CInt(arrMods(i)), PortalId).RoleName
			//        Dim Arr As ArrayList = Roles.GetUsersByRoleName(PortalId, RoleName)
			//        Dim objUser As Entities.Users.UserRoleInfo
			//        For Each objUser In Arr
			//            Dim sBody As String = myTemplate
			//            sBody = Replace(sBody, "[FULLNAME]", objUser.FullName)
			//            sBody = Replace(sBody, "[PORTALNAME]", _portalSettings.PortalName)
			//            sBody = Replace(sBody, "[USERNAME]", objPost.UserName)
			//            sBody = Replace(sBody, "[POSTDATE]", objPost.DateAdded.ToString)
			//            sBody = Replace(sBody, "[SUBJECT]", objPost.Subject)
			//            sBody = Replace(sBody, "[BODY]", objPost.Body)
			//            sBody = Replace(sBody, "[LINK]", "<a href=""" & sLink & """>" & sLink & "</a>")
			//            Dim objUserInfo As Entities.Users.UserInfo = objUserController.GetUser(PortalId, objUser.UserID)
			//            SendNotification(FromEmail, objUserInfo.Membership.Email, strSubject, sBody, ForumID, intPost)
			//        Next
			//    Next
			//Catch ex As Exception
			//    DotNetNuke.Services.Exceptions.Exceptions.LogException(ex)
			//End Try

		}

		/* 
         * Note: This is the method that actual sends the email.  The mail queue  
         */
		public static void SendNotification(string fromEmail, string toEmail, string subject, string bodyText, string bodyHTML)
		{
			SendNotification(-1, fromEmail, toEmail, subject, bodyText, bodyHTML);
		}
		public static void SendNotification(int portalId, string fromEmail, string toEmail, string subject, string bodyText, string bodyHTML)
		{
				//USE DNN API for this to ensure proper delivery & adherence to portal settings
				//Services.Mail.Mail.SendEmail(fromEmail, fromEmail, toEmail, subject, bodyHTML);

				//Since this code is triggered from the DNN scheduler, the default/simple API (now commented out above) uses Host rather than Portal-specific SMTP configuration
				//updated here to retrieve portal-specific SMTP configuration and use more elaborate DNN API that allows passing of the SMTP information rather than rely on DNN API DotNetNuke.Host.SMTP property accessors to determine portal vs. host SMTP values 
				Services.Mail.Mail.SendMail(mailFrom: fromEmail,
										mailSender: (SMTPPortalEnabled(portalId) ? PortalController.Instance.GetPortal(portalId).Email : Host.HostEmail),
										mailTo: toEmail,
										cc: string.Empty,
										bcc: string.Empty,
										replyTo: string.Empty,
										priority: DotNetNuke.Services.Mail.MailPriority.Normal,
										subject: subject,
										bodyFormat: DotNetNuke.Services.Mail.MailFormat.Html,
										bodyEncoding: System.Text.Encoding.Default,
										body: bodyHTML,
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
				var intRecipients = 0;
				var intMessages = 0;
				var strDistributionList = string.Empty;
				Subject = Subject.Replace("&#91;", "[");
				Subject = Subject.Replace("&#93;", "]");


				foreach (var si in Recipients.Where(si => si.Email != string.Empty))
				{
					intRecipients += 1;

                    if(UseQueue)
					    Queue.Controller.Add(PortalId, From, si.Email, Subject, BodyHTML, BodyText, string.Empty, string.Empty);
                    else
                        SendNotification(PortalId, From, si.Email, Subject, BodyText, BodyHTML);  

					intMessages += 1;
				}

			}
			catch (Exception ex)
			{
				Services.Exceptions.Exceptions.LogException(ex);
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