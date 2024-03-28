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

using System.Web;
using System.Web.Services;
using System.Text;
using System.Xml;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Journal;
using DotNetNuke.Modules.ActiveForums.Data;
using DotNetNuke.Modules.ActiveForums.Entities;
using System.Linq;
namespace DotNetNuke.Modules.ActiveForums.Handlers
{
    public class forumhelper : HandlerBase
    {
        public enum Actions : int
        {
            None,
            UserPing, /* no longer used */
            GetUsersOnline,/* no longer used */
            TopicSubscribe,/* no longer used */
            ForumSubscribe,/* no longer used */
            RateTopic,/* no longer used */
            DeleteTopic,
			MoveTopic,
            PinTopic,/* no longer used */
            LockTopic,/* no longer used */
            MarkAnswer,
            TagsAutoComplete,
			DeletePost,
			LoadTopic,
			SaveTopic,
			ForumList,
            LikePost /*no longer used*/

        }
        public override void ProcessRequest(HttpContext context)
        {
            AdminRequired = false;
            base.AdminRequired = false;
            base.ProcessRequest(context);
            string sOut = "{\"result\":\"success\"}";
            Actions action = Actions.None;
            if (Params != null && Params.Count > 0)
            {
                if (Params[ParamKeys.action] != null && SimulateIsNumeric.IsNumeric(Params[ParamKeys.action]))
                {
                    action = (Actions)(Convert.ToInt32(Params[ParamKeys.action].ToString()));
                }
            }
            else if (HttpContext.Current.Request.QueryString[ParamKeys.action] != null && SimulateIsNumeric.IsNumeric(HttpContext.Current.Request.QueryString[ParamKeys.action]))
            {
                if (int.Parse(HttpContext.Current.Request.QueryString[ParamKeys.action]) == 11)
                {
                    action = Actions.TagsAutoComplete;
                }
            }
            switch (action)
            {
                case Actions.UserPing:
                    throw new NotImplementedException();
                ////sOut = UserOnline();
                ////break;
                case Actions.GetUsersOnline:
                    throw new NotImplementedException();
                ////sOut = GetUserOnlineList();
                ////break;
                case Actions.TopicSubscribe:
                    throw new NotImplementedException();
     //               sOut = SubscribeTopic();
					//break;
				case Actions.ForumSubscribe:
                    throw new NotImplementedException();
                //	sOut = SubscribeForum();
                //	break;
                case Actions.RateTopic:
                    throw new NotImplementedException();
                //sOut = RateTopic();
                //break;
                case Actions.DeleteTopic:
					sOut = DeleteTopic();
					break;
				case Actions.MoveTopic:
					sOut = MoveTopic();
					break;
				case Actions.PinTopic:
                    throw new NotImplementedException();
                //               sOut = PinTopic();
                //break;
                case Actions.LockTopic:
                    throw new NotImplementedException();
                //               sOut = LockTopic();
                //break;
                case Actions.MarkAnswer:
                    sOut = MarkAnswer();
                    break;
                case Actions.TagsAutoComplete:
                    sOut = TagsAutoComplete();
                    break;
                case Actions.DeletePost:
                    sOut = DeletePost();
                    break;
                case Actions.LoadTopic:
                    sOut = LoadTopic();
                    break;
                case Actions.SaveTopic:
                    sOut = SaveTopic();
                    break;
                case Actions.ForumList:
                    sOut = ForumList();
                    break;
                case Actions.LikePost:
                    throw new NotImplementedException();
                    //sOut = LikePost();
                    //break;
            }
            context.Response.ContentType = "text/plain";
            context.Response.Write(sOut);
        }
        private string ForumList()
        {
            var sb = new StringBuilder();
            int index = 1;
            var forums = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetForums(ModuleId).Where(f => !f.Hidden && !f.ForumGroup.Hidden && (ForumUser.IsSuperUser || DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(f.Security.View, ForumUser.UserRoles)));
            DotNetNuke.Modules.ActiveForums.Controllers.ForumController.IterateForumsList(forums.ToList(), ForumUser,
                fi =>
                {
                    sb.AppendFormat("<option value=\"{0}\">{1}</option>", "-1", fi.GroupName);
                    index += 1;
                },
                fi =>
                {
                    sb.AppendFormat("<option value=\"{0}\">{1}</option>", fi.ForumID.ToString(), "--" + fi.ForumName);
                    index += 1;
                },
                fi =>
                {
                    sb.AppendFormat("<option value=\"{0}\">----{1}</option>", fi.ForumID.ToString(), fi.ForumName);
                    index += 1;
                }
                );
            return sb.ToString();
        }
        private string SubscribeForum()
        {
            if (UserId <= 0)
            {
                return BuildOutput(string.Empty, OutputCodes.AuthenticationFailed, true);
            }
            int rUserId = -1;

            if (Params.ContainsKey(Literals.UserId) && SimulateIsNumeric.IsNumeric(Params[Literals.UserId]))
            {
                rUserId = int.Parse(Params[Literals.UserId].ToString());

            }
            if (rUserId > 0 & rUserId != ForumUser.UserId & !ForumUser.IsAdmin)
            {
                return BuildOutput(string.Empty, OutputCodes.AuthenticationFailed, true);
            }
            rUserId = UserId;
            int iStatus = 0;
            SubscriptionController sc = new SubscriptionController();
            int forumId = -1;
            if (Params.ContainsKey(Literals.ForumId) && SimulateIsNumeric.IsNumeric(Params[Literals.ForumId]))
            {
                forumId = int.Parse(Params[Literals.ForumId].ToString());
            }
            iStatus = sc.Subscription_Update(PortalId, ModuleId, forumId, -1, 1, rUserId, ForumUser.UserRoles);

            if (iStatus == 1)
            {
                return BuildOutput("{\"subscribed\":true,\"text\":\"" + Utilities.JSON.EscapeJsonString(Utilities.GetSharedResource("[RESX:ForumSubscribe:TRUE]")) + "\",\"forumid\":" + forumId.ToString() + "}", OutputCodes.Success, true, true);
            }
            else
            {
                return BuildOutput("{\"subscribed\":false,\"text\":\"" + Utilities.JSON.EscapeJsonString(Utilities.GetSharedResource("[RESX:ForumSubscribe:FALSE]")) + "\",\"forumid\":" + forumId.ToString() + "}", OutputCodes.Success, true, true);
            }


		}
		private string DeleteTopic()
		{
			int topicId = -1;

			if (Params.ContainsKey(Literals.TopicId) && SimulateIsNumeric.IsNumeric(Params[Literals.TopicId]))
			{
				topicId = int.Parse(Params[Literals.TopicId].ToString());
			}
			if (topicId > 0)
			{
                DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(topicId);
                if (ti?.Forum != null)
                {
                    if (DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(ti.Forum.Security.ModDelete, ForumUser.UserRoles) || (ti.Author.AuthorId == this.UserId && DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasAccess(ti.Forum.Security.Delete, ForumUser.UserRoles)))
                    {
                        new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().DeleteById(topicId);
                        return BuildOutput(string.Empty, OutputCodes.Success, true);
                    }
                }
            }
            return BuildOutput(string.Empty, OutputCodes.UnsupportedRequest, false);
        }
        private string MoveTopic()
        {
            int topicId = -1;
            int forumId = -1;
            int targetForumId = -1;
            if (Params.ContainsKey(Literals.TopicId) && SimulateIsNumeric.IsNumeric(Params[Literals.TopicId]))
            {
                topicId = int.Parse(Params[Literals.TopicId].ToString());
            }
            if (Params.ContainsKey(Literals.ForumId) && SimulateIsNumeric.IsNumeric(Params[Literals.ForumId]))
            {
                targetForumId = int.Parse(Params[Literals.ForumId].ToString());
            }
            if (topicId > 0)
            {
                DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(topicId);
                if (ti?.Forum != null)
                {
                    if (DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(ti.Forum.Security.ModMove, ForumUser.UserRoles))
                    {
                        DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Move(topicId, targetForumId);
                        DataCache.CacheClearPrefix(ModuleId, string.Format(CacheKeys.ForumViewPrefix, ModuleId));
                        return BuildOutput(string.Empty, OutputCodes.Success, true);
                    }
                }

            }
            return BuildOutput(string.Empty, OutputCodes.UnsupportedRequest, false);
        }
        private string MarkAnswer()
        {
            int topicId = -1;
            int replyId = -1;
            if (Params.ContainsKey(Literals.TopicId) && SimulateIsNumeric.IsNumeric(Params[Literals.TopicId]))
            {
                topicId = int.Parse(Params[Literals.TopicId].ToString());
            }
            if (Params.ContainsKey(Literals.ReplyId) && SimulateIsNumeric.IsNumeric(Params[Literals.ReplyId]))
            {
                replyId = int.Parse(Params[Literals.ReplyId].ToString());
            }
            if (topicId > 0 & UserId > 0)
            { 
                DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(topicId);
                if ((this.UserId == ti.Author.AuthorId && !ti.IsLocked) || DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasAccess(ti.Forum.Security.ModEdit, ForumUser.UserRoles))
                {
                    DataProvider.Instance().Reply_UpdateStatus(PortalId, ModuleId, topicId, replyId, UserId, 1, DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasAccess(ti.Forum.Security.ModEdit, ForumUser.UserRoles));
                }
                return BuildOutput(string.Empty, OutputCodes.Success, true);
            }
            else
            {
                return BuildOutput(string.Empty, OutputCodes.UnsupportedRequest, false);
            }
        }
        private string TagsAutoComplete()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            string q = string.Empty;
            if (!(string.IsNullOrEmpty(HttpContext.Current.Request.QueryString[SearchParamKeys.Query])))
            {
                q = HttpContext.Current.Request.QueryString[SearchParamKeys.Query].Trim();
                q = Utilities.Text.RemoveHTML(q);
                q = Utilities.Text.CheckSqlString(q);
                if (!(string.IsNullOrEmpty(q)))
                {
                    if (q.Length > 20)
                    {
                        q = q.Substring(0, 20);
                    }
                }
            }
            int i = 0;
            if (!(string.IsNullOrEmpty(q)))
            {
                using (IDataReader dr = DataProvider.Instance().Tags_Search(PortalId, ModuleId, q))
                {
                    while (dr.Read())
                    {
                        sb.AppendLine("{\"id\":\"" + dr["TagId"].ToString() + "\",\"name\":\"" + dr["TagName"].ToString() + "\",\"type\":\"0\"},");
                        i += 1;
                    }
                    dr.Close();
                }
            }
            string @out = "[";
            if (i > 0)
            {
                @out += sb.ToString().Trim();
                @out = @out.Substring(0, @out.Length - 1);
            }
            @out += "]";
            return @out;
        }
        private string DeletePost()
        {
            int replyId = -1;
            int TopicId = -1;
            if (Params.ContainsKey(Literals.TopicId) && SimulateIsNumeric.IsNumeric(Params[Literals.TopicId]))
            {
                TopicId = int.Parse(Params[Literals.TopicId].ToString());
            }
            if (Params.ContainsKey(Literals.ReplyId) && SimulateIsNumeric.IsNumeric(Params[Literals.ReplyId]))
            {
                replyId = int.Parse(Params[Literals.ReplyId].ToString());
            }
            int forumId = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Forum_GetByTopicId(TopicId);
            DotNetNuke.Modules.ActiveForums.Entities.ForumInfo f = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Forums_Get(portalId: PortalId, moduleId: ModuleId, forumId: forumId, useCache: true);

            // Need to get the list of attachments BEFORE we remove the post recods
            var attachmentController = new Data.AttachController();
            var attachmentList = (MainSettings.DeleteBehavior == 0)
                                     ? attachmentController.ListForPost(TopicId, replyId)
                                     : null;


            if (TopicId > 0 & replyId < 1)
            {
                DotNetNuke.Modules.ActiveForums.Entities.TopicInfo ti = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(TopicId);
                if (DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasAccess(ti.Forum.Security.ModDelete, ForumUser.UserRoles) || (DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasAccess(ti.Forum.Security.Delete, ForumUser.UserRoles) && ti.Content.AuthorId == UserId && ti.IsLocked == false))
                {
                    DataProvider.Instance().Topics_Delete(forumId, TopicId, MainSettings.DeleteBehavior);
                    string journalKey = string.Format("{0}:{1}", forumId.ToString(), TopicId.ToString());
                    JournalController.Instance.DeleteJournalItemByKey(PortalId, journalKey);
                }
                else
                {
                    return BuildOutput(string.Empty, OutputCodes.UnsupportedRequest, false);
                }
            }
            else
            {
                ReplyController rc = new ReplyController();
                DotNetNuke.Modules.ActiveForums.ReplyInfo ri = rc.Reply_Get(PortalId, ModuleId, TopicId, replyId);
                if (DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasAccess(f.Security.ModDelete, ForumUser.UserRoles) || (DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasAccess(f.Security.Delete, ForumUser.UserRoles) && ri.Content.AuthorId == UserId))
                {
                    DataProvider.Instance().Reply_Delete(forumId, TopicId, replyId, MainSettings.DeleteBehavior);
                    string journalKey = string.Format("{0}:{1}:{2}", forumId.ToString(), TopicId.ToString(), replyId.ToString());
                    JournalController.Instance.DeleteJournalItemByKey(PortalId, journalKey);

                }
                else
                {
                    return BuildOutput(string.Empty, OutputCodes.UnsupportedRequest, false);
                }

            }

            // If it's a hard delete, delete associated attachments
            // attachmentList will only be populated if the DeleteBehavior is 0
            if (attachmentList != null)
            {
                var fileManager = FileManager.Instance;
                var folderManager = FolderManager.Instance;
                var attachmentFolder = folderManager.GetFolder(PortalId, "activeforums_Attach");

                foreach (var attachment in attachmentList)
                {
                    attachmentController.Delete(attachment.AttachmentId);

                    var file = attachment.FileId.HasValue
                                   ? fileManager.GetFile(attachment.FileId.Value)
                                   : fileManager.GetFile(attachmentFolder, attachment.FileName);

                    // Only delete the file if it exists in the attachment folder
                    if (file != null && file.FolderId == attachmentFolder.FolderID)
                        fileManager.DeleteFile(file);
                }
            }

            // Return the result
            DataCache.CacheClearPrefix(ModuleId, string.Format(CacheKeys.ForumViewPrefix, ModuleId));
            return BuildOutput(TopicId + "|" + replyId, OutputCodes.Success, true);
        }
        private string LoadTopic()
        {
            int topicId = -1;
            int forumId = -1;
            if (Params.ContainsKey(Literals.TopicId) && SimulateIsNumeric.IsNumeric(Params[Literals.TopicId]))
            {
                topicId = int.Parse(Params[Literals.TopicId].ToString());
            }
            if (topicId > 0)
            {
                TopicsController tc = new TopicsController();
                DotNetNuke.Modules.ActiveForums.Entities.TopicInfo t = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(topicId);
                forumId = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Forum_GetByTopicId(topicId);

                DotNetNuke.Modules.ActiveForums.Entities.ForumInfo f = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Forums_Get(PortalId, ModuleId, forumId, false, -1);
                if (f != null)
                {
                    if (DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(f.Security.ModEdit, ForumUser.UserRoles))
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append("{");
                        sb.Append(Utilities.JSON.Pair("topicid", t.TopicId.ToString()));
                        sb.Append(",");
                        sb.Append(Utilities.JSON.Pair("subject", t.Content.Subject));
                        sb.Append(",");
                        sb.Append(Utilities.JSON.Pair("authorid", t.Content.AuthorId.ToString()));
                        sb.Append(",");
                        sb.Append(Utilities.JSON.Pair("locked", t.IsLocked.ToString()));
                        sb.Append(",");
                        sb.Append(Utilities.JSON.Pair("pinned", t.IsPinned.ToString()));
                        sb.Append(",");
                        sb.Append(Utilities.JSON.Pair("priority", t.Priority.ToString()));
                        sb.Append(",");
                        sb.Append(Utilities.JSON.Pair("status", t.StatusId.ToString()));
                        sb.Append(",");
                        sb.Append(Utilities.JSON.Pair("forumid", forumId.ToString()));
                        sb.Append(",");
                        sb.Append(Utilities.JSON.Pair("forumname", f.ForumName));
                        sb.Append(",");
                        sb.Append(Utilities.JSON.Pair("tags", t.Tags));
                        sb.Append(",");
                        sb.Append(Utilities.JSON.Pair("categories", t.Categories));
                        sb.Append(",");
                        sb.Append("\"properties\":[");
                        string sCats = string.Empty;
                        if (f.Properties != null)
                        {
                            int i = 0;
                            foreach (PropertiesInfo p in f.Properties)
                            {
                                sb.Append("{");
                                sb.Append(Utilities.JSON.Pair("propertyid", p.PropertyId.ToString()));
                                sb.Append(",");
                                sb.Append(Utilities.JSON.Pair("datatype", p.DataType));
                                sb.Append(",");
                                sb.Append(Utilities.JSON.Pair("propertyname", p.Name));
                                sb.Append(",");
                                string pvalue = p.DefaultValue;
                                foreach (PropertiesInfo tp in t.TopicProperties)
                                {
                                    if (tp.PropertyId == p.PropertyId)
                                    {
                                        pvalue = tp.DefaultValue;
                                    }
                                }

                                sb.Append(Utilities.JSON.Pair("propertyvalue", pvalue));
                                if (p.DataType.Contains("list"))
                                {
                                    sb.Append(",\"listdata\":[");
                                    if (p.DataType.Contains("list|categories"))
                                    {
                                        using (IDataReader dr = DataProvider.Instance().Tags_List(PortalId, f.ModuleId, true, 0, 200, "ASC", "TagName", forumId, f.ForumGroupId))
                                        {
                                            dr.NextResult();
                                            while (dr.Read())
                                            {
                                                sCats += "{";
                                                sCats += Utilities.JSON.Pair("id", dr["TagId"].ToString());
                                                sCats += ",";
                                                sCats += Utilities.JSON.Pair("name", dr["TagName"].ToString());
                                                sCats += ",";
                                                sCats += Utilities.JSON.Pair("selected", IsSelected(dr["TagName"].ToString(), t.Categories).ToString());
                                                sCats += "},";
                                            }
                                            dr.Close();
                                        }
                                        if (!(string.IsNullOrEmpty(sCats)))
                                        {
                                            sCats = sCats.Substring(0, sCats.Length - 1);
                                        }
                                        sb.Append(sCats);
                                    }
                                    else
                                    {
                                        DotNetNuke.Common.Lists.ListController lists = new DotNetNuke.Common.Lists.ListController();
                                        string lName = p.DataType.Substring(p.DataType.IndexOf("|") + 1);
                                        DotNetNuke.Common.Lists.ListEntryInfoCollection lc = lists.GetListEntryInfoCollection(lName, string.Empty);
                                        int il = 0;
                                        foreach (DotNetNuke.Common.Lists.ListEntryInfo l in lc)
                                        {
                                            sb.Append("{");
                                            sb.Append(Utilities.JSON.Pair("itemId", l.Value));
                                            sb.Append(",");
                                            sb.Append(Utilities.JSON.Pair("itemName", l.Text));
                                            sb.Append("}");
                                            il += 1;
                                            if (il < lc.Count)
                                            {
                                                sb.Append(",");
                                            }
                                        }
                                    }
                                    sb.Append("]");
                                }
                                sb.Append("}");
                                i += 1;
                                if (i < f.Properties.Count)
                                {
                                    sb.Append(",");
                                }

                            }
                        }




                        sb.Append("],\"categories\":[");
                        sCats = string.Empty;
                        using (IDataReader dr = DataProvider.Instance().Tags_List(PortalId, f.ModuleId, true, 0, 200, "ASC", "TagName", forumId, f.ForumGroupId))
                        {
                            dr.NextResult();
                            while (dr.Read())
                            {
                                sCats += "{";
                                sCats += Utilities.JSON.Pair("id", dr["TagId"].ToString());
                                sCats += ",";
                                sCats += Utilities.JSON.Pair("name", dr["TagName"].ToString());
                                sCats += ",";
                                sCats += Utilities.JSON.Pair("selected", IsSelected(dr["TagName"].ToString(), t.Categories).ToString());
                                sCats += "},";
                            }
                            dr.Close();
                        }
                        if (!(string.IsNullOrEmpty(sCats)))
                        {
                            sCats = sCats.Substring(0, sCats.Length - 1);
                        }
                        sb.Append(sCats);
                        sb.Append("]");
                        sb.Append("}");
                        return BuildOutput(sb.ToString(), OutputCodes.Success, true, true);
                    }
                }

            }
            return BuildOutput(string.Empty, OutputCodes.UnsupportedRequest, false); 
        }
        private string SaveTopic()
        {
            int topicId = -1;
            int forumId = -1;
            if (Params.ContainsKey(Literals.TopicId) && SimulateIsNumeric.IsNumeric(Params[Literals.TopicId]))
            {
                topicId = int.Parse(Params[Literals.TopicId].ToString());
            }
            if (topicId > 0)
            {
                DotNetNuke.Modules.ActiveForums.Entities.TopicInfo t = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(topicId); 
                forumId = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Forum_GetByTopicId(topicId);
                DotNetNuke.Modules.ActiveForums.Entities.ForumInfo ForumInfo = DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Forums_Get(PortalId, ModuleId, forumId, false, -1);
                if (DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasPerm(ForumInfo.Security.ModEdit, ForumUser.UserRoles))
                {
                    string subject = Params["subject"].ToString();
                    subject = Utilities.XSSFilter(subject, true);
                    t.TopicUrl = DotNetNuke.Modules.ActiveForums.Controllers.UrlController.BuildTopicUrl(PortalId: PortalId, ModuleId: ForumInfo.ModuleId, TopicId: topicId, subject: subject, forumInfo: ForumInfo);

                    t.Content.Subject = subject;
                    t.IsPinned = bool.Parse(Params["pinned"].ToString());
                    t.IsLocked = bool.Parse(Params["locked"].ToString());
                    t.Priority = int.Parse(Params["priority"].ToString());
                    t.StatusId = int.Parse(Params["status"].ToString());
                    if (ForumInfo.Properties != null)
                    {
                        StringBuilder tData = new StringBuilder();
                        tData.Append("<topicdata>");
                        tData.Append("<properties>");
                        foreach (PropertiesInfo p in ForumInfo.Properties)
                        {
                            string pkey = "prop-" + p.PropertyId.ToString();

                            tData.Append("<property id=\"" + p.PropertyId.ToString() + "\">");
                            tData.Append("<name><![CDATA[");
                            tData.Append(p.Name);
                            tData.Append("]]></name>");
                            if (Params[pkey] != null)
                            {
                                tData.Append("<value><![CDATA[");
                                tData.Append(Utilities.XSSFilter(Params[pkey].ToString()));
                                tData.Append("]]></value>");
                            }
                            else
                            {
                                tData.Append("<value></value>");
                            }
                            tData.Append("</property>");
                        }
                        tData.Append("</properties>");
                        tData.Append("</topicdata>");
                        t.TopicData = tData.ToString();
                    }
                }
                DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Save(t);
                Utilities.UpdateModuleLastContentModifiedOnDate(ModuleId);
                if (Params["tags"] != null)
                {
                    DataProvider.Instance().Tags_DeleteByTopicId(PortalId, ForumInfo.ModuleId, topicId);
                    string tagForm = string.Empty;
                    if (Params["tags"] != null)
                    {
                        tagForm = Params["tags"].ToString();
                    }
                    if (!(tagForm == string.Empty))
                    {
                        string[] Tags = tagForm.Split(',');
                        foreach (string tag in Tags)
                        {
                            string sTag = Utilities.CleanString(PortalId, tag.Trim(), false, EditorTypes.TEXTBOX, false, false, ForumInfo.ModuleId, string.Empty, false);
                            DataProvider.Instance().Tags_Save(PortalId, ForumInfo.ModuleId, -1, sTag, 0, 1, 0, topicId, false, -1, -1);
                        }
                    }
                }

                if (Params["categories"] != null)
                {
                    string[] cats = Params["categories"].ToString().Split(';');
                    DataProvider.Instance().Tags_DeleteTopicToCategory(PortalId, ForumInfo.ModuleId, -1, topicId);
                    foreach (string c in cats)
                    {
                        int cid = -1;
                        if (!(string.IsNullOrEmpty(c)) && SimulateIsNumeric.IsNumeric(c))
                        {
                            cid = Convert.ToInt32(c);
                            if (cid > 0)
                            {
                                DataProvider.Instance().Tags_AddTopicToCategory(PortalId, ForumInfo.ModuleId, cid, topicId);
                            }
                        }
                    }
                }
            }


            return BuildOutput(string.Empty, OutputCodes.UnsupportedRequest, false);
        }
        private bool IsSelected(string TagName, string selectedValues)
        {
            if (string.IsNullOrEmpty(selectedValues))
            {
                return false;
            }
            else
            {
                foreach (string s in selectedValues.Split('|'))
                {
                    if (!(string.IsNullOrEmpty(s)))
                    {
                        if (s.ToLowerInvariant().Trim() == TagName.ToLowerInvariant().Trim())
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}