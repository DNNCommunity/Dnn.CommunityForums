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



namespace DotNetNuke.Modules.ActiveForums.Entities
{
    [TableName("activeforums_Topics")]
    [PrimaryKey("TopicId", AutoIncrement = true)]
    [Scope("TopicId")]
    [Cacheable("activeforums_Topics", CacheItemPriority.Normal)]
    public class TopicInfo
    {
        public int TopicId { get; set; }
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
        public int NextTopic { get; set; }
        public int PrevTopic { get; set; }
        public string TopicData { get; set; } = string.Empty;
        [IgnoreColumn()]
        public Content Content { get; set; }
        [IgnoreColumn()]
        public PermissionInfo Security { get; set; }
        [IgnoreColumn()]
        public Author Author { get; set; }
        [IgnoreColumn()]
        public string Tags { get; set; }
        [IgnoreColumn()]
        public string Categories { get; set; } = string.Empty;
        [IgnoreColumn()]
        public string ForumURL { get; set; } = string.Empty;

        [IgnoreColumn()]
        public TopicInfo()
        {
            Content = new Content();
            Security = new PermissionInfo();
            Author = new Author();
        }
        [IgnoreColumn()]
        public string URL
        {
            get
            {
                if (!(string.IsNullOrEmpty(TopicUrl)) && !(string.IsNullOrEmpty(ForumURL)))
                {
                    if (TopicUrl.StartsWith("/") & ForumURL.EndsWith("/"))
                    {
                        ForumURL = ForumURL.Substring(0, ForumURL.Length - 1);
                        if (!(ForumURL.StartsWith("/")))
                        {
                            ForumURL = "/" + ForumURL;
                        }
                    }
                    return ForumURL + TopicUrl;
                }
                else
                {
                    return string.Empty;
                }
            }
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
