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
using System.Xml;
using DotNetNuke.ComponentModel.DataAnnotations;
using System.Web.Caching;
using DotNetNuke.UI.UserControls;

namespace DotNetNuke.Modules.ActiveForums
{
    [Obsolete("Deprecated in Community Forums. Scheduled for removal in 09.00.00. Use DotNetNuke.Modules.ActiveForums.Entities.TopicInfo")]
    public class TopicInfo : DotNetNuke.Modules.ActiveForums.Entities.TopicInfo { }
}

namespace DotNetNuke.Modules.ActiveForums.Entities
{
    [TableName("activeforums_Topics")]
    [PrimaryKey("TopicId", AutoIncrement = true)]
    [Scope("TopicId")]
    [Cacheable("activeforums_Topics", CacheItemPriority.Normal)]
    public class TopicInfo
    {
        private DotNetNuke.Modules.ActiveForums.Entities.ContentInfo _contentInfo;
        private DotNetNuke.Modules.ActiveForums.Entities.ForumInfo _forumInfo;
        private DotNetNuke.Modules.ActiveForums.Author _Author;
        private int forumId;

        public int TopicId { get; set; }
        public int ForumId 
        { 
            get
            {
                //TODO : clean this up to use DAL2
                if (forumId < 1)
                {
                    forumId = new Data.ForumsDB().Forum_GetByTopicId(TopicId);
                }
                return forumId; 
            } 
            set => forumId = value;
        }
        public int PortalId { get => Forum.PortalId; }
        public int ModuleId { get => Content.ModuleId; }
        public int ContentId { get; set; }
        public int ViewCount { get; set; }
        public int ReplyCount { get; set; }
        public bool IsLocked { get; set; }
        public bool IsPinned { get; set; }
        public string TopicIcon { get; set; }
        public int StatusId { get; set; }
        public bool IsApproved { get; set; }
        public bool IsRejected { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsAnnounce { get; set; }
        public bool IsArchived { get; set; }
        public DateTime AnnounceStart { get; set; }
        public DateTime AnnounceEnd { get; set; }
        public TopicTypes TopicType { get; set; }
        public int Priority { get; set; } = 0;
        [ColumnName("URL")]
        public string TopicUrl { get; set; } = string.Empty;
        [IgnoreColumn()]
        public string URL => !(string.IsNullOrEmpty(TopicUrl)) && !(string.IsNullOrEmpty(ForumURL)) ? ForumURL + TopicUrl : string.Empty;
        [IgnoreColumn()]
        public string ForumURL => !(string.IsNullOrEmpty(Forum.PrefixURL)) && !(string.IsNullOrEmpty(TopicUrl)) ? "/" + Forum.PrefixURL + "/" : string.Empty;
        public int NextTopic { get; set; }
        public int PrevTopic { get; set; }
        public string TopicData { get; set; } = string.Empty;
        [IgnoreColumn()]
        public DotNetNuke.Modules.ActiveForums.Entities.ContentInfo Content
        {
            get
            {
                if (_contentInfo == null)
                {
                    _contentInfo = new DotNetNuke.Modules.ActiveForums.Controllers.ContentController().GetById(ContentId);
                }
                return _contentInfo;
            }
            set
            { _contentInfo = value; }
        }
        [IgnoreColumn()]
        public DotNetNuke.Modules.ActiveForums.Entities.ForumInfo Forum
        {
            get
            {
                if (_forumInfo == null)
                {
                    _forumInfo = new DotNetNuke.Modules.ActiveForums.Controllers.ForumController().GetById(ForumId);
                }
                return _forumInfo;
            }
            set
            { _forumInfo = value; }
        }
        [IgnoreColumn()]
        public DotNetNuke.Modules.ActiveForums.Author Author
        {
            get
            {
                if (Author == null)
                {
                    Author = new DotNetNuke.Modules.ActiveForums.Author();
                    Author.AuthorId = Content.AuthorId;
                    var userInfo = DotNetNuke.Entities.Users.UserController.Instance.GetUser(PortalId, Content.AuthorId);
                    if (userInfo != null)
                    {
                        Author.Email = userInfo?.Email;
                        Author.FirstName = userInfo?.FirstName;
                        Author.LastName = userInfo?.LastName;
                        Author.DisplayName = userInfo?.DisplayName;
                        Author.Username = userInfo?.Username;
                    }
                    else
                    {
                        Author.DisplayName = Content.AuthorId > 0 ? "Deleted User" : "Anonymous";
                    }
                }
                return Author;
            }
            set
            { Author = value; }
        }
        [IgnoreColumn()]
        public DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo Security { get; set; }
        [IgnoreColumn()]
        public string Tags { get; set; }
        [IgnoreColumn()]
        public string Categories { get; set; } = string.Empty;
        [IgnoreColumn()]
        public TopicInfo()
        {
            Content = new DotNetNuke.Modules.ActiveForums.Entities.ContentInfo();
            Security = new DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo();
            Author = new Author();
        }
        [IgnoreColumn()]
        public List<PropertiesInfo> TopicProperties
        {
            get
            {
                if (TopicData == string.Empty)
                {
                    return null;
                }
                else
                {
                    List<PropertiesInfo> pl = new List<PropertiesInfo>();
                    XmlDocument xDoc = new XmlDocument();
                    xDoc.LoadXml(TopicData);
                    if (xDoc != null)
                    {
                        System.Xml.XmlNode xRoot = xDoc.DocumentElement;
                        System.Xml.XmlNodeList xNodeList = xRoot.SelectNodes("//properties/property");
                        if (xNodeList.Count > 0)
                        {
                            int i = 0;
                            for (i = 0; i < xNodeList.Count; i++)
                            {
                                string pName = Utilities.HTMLDecode(xNodeList[i].ChildNodes[0].InnerText);
                                string pValue = Utilities.HTMLDecode(xNodeList[i].ChildNodes[1].InnerText);
                                int pId = Convert.ToInt32(xNodeList[i].Attributes["id"].Value);
                                PropertiesInfo p = new PropertiesInfo();
                                p.Name = pName;
                                p.DefaultValue = pValue;
                                p.PropertyId = pId;
                                pl.Add(p);
                            }
                        }
                    }
                    return pl;
                }
            }
        }
         
    } 

}
