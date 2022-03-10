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
using System.Xml;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Journal;
using System.Text.RegularExpressions;

namespace DotNetNuke.Modules.ActiveForums
{
	#region TopicInfo Class
	public class TopicInfo
	{
		public TopicInfo()
		{
			Content = new Content();
			Security = new PermissionInfo();
			Author = new Author();
		}

		#region Private Members
		#endregion
		#region Public Properties
		public int TopicId { get; set; }
		public int ContentId { get; set; }
		public int ViewCount { get; set; }
		public int ReplyCount { get; set; }
		public bool IsLocked { get; set; }
		public bool IsPinned { get; set; }
		public string TopicIcon { get; set; }
		public int StatusId { get; set; }
		public bool IsApproved { get; set; }
		public bool IsDeleted { get; set; }
		public bool IsAnnounce { get; set; }
		public bool IsArchived { get; set; }
		public TopicTypes TopicType { get; set; }
		public DateTime AnnounceStart { get; set; }
		public DateTime AnnounceEnd { get; set; }
		public Content Content { get; set; }
		public PermissionInfo Security { get; set; }
		public Author Author { get; set; }
		public string Tags { get; set; }
		public int Priority { get; set; } = 0;
		public string Categories { get; set; } = string.Empty;
		public string TopicUrl { get; set; } = string.Empty;
		public string ForumURL { get; set; } = string.Empty;
		public string TopicData { get; set; } = string.Empty;

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

		#endregion
	}
	#endregion
}

