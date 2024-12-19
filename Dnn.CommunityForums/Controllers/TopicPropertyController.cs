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
    using System.Data;
    using System.Linq;
    using System.Text;
    using System.Xml;

    internal class TopicPropertyController : DotNetNuke.Modules.ActiveForums.Controllers.ControllerBase<DotNetNuke.Modules.ActiveForums.Entities.TopicPropertyInfo>

    {
        public static string Serialize(DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forum, IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.TopicPropertyInfo> properties)
        {
            StringBuilder tData = new StringBuilder();
            tData.Append("<topicdata>");
            tData.Append("<properties>");
            foreach (DotNetNuke.Modules.ActiveForums.Entities.PropertyInfo p in forum.Properties)
            {
                tData.Append("<property id=\"" + p.PropertyId.ToString() + "\">");
                tData.Append("<name><![CDATA[");
                tData.Append(p.Name);
                tData.Append("]]></name>");
                if (!string.IsNullOrEmpty(properties.Where(tp => tp.PropertyId == p.PropertyId).FirstOrDefault().Value))
                {
                    tData.Append("<value><![CDATA[");
                    tData.Append(Utilities.XSSFilter(properties.Where(pl => pl.PropertyId == p.PropertyId).FirstOrDefault().Value));
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
            return tData.ToString();
        }

        public static List<DotNetNuke.Modules.ActiveForums.Entities.TopicPropertyInfo> Deserialize(string xmlString)
        {
            List<DotNetNuke.Modules.ActiveForums.Entities.TopicPropertyInfo> tp = new List<DotNetNuke.Modules.ActiveForums.Entities.TopicPropertyInfo>();
            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(xmlString);
            if (xDoc != null)
            {
                System.Xml.XmlNode xRoot = xDoc.DocumentElement;
                System.Xml.XmlNodeList xNodeList = xRoot.SelectNodes("//properties/property");
                if (xNodeList.Count > 0)
                {
                    int i = 0;
                    for (i = 0; i < xNodeList.Count; i++)
                    {
                        string pName = System.Net.WebUtility.HtmlDecode(xNodeList[i].ChildNodes[0].InnerText);
                        string pValue = System.Net.WebUtility.HtmlDecode(xNodeList[i].ChildNodes[1].InnerText);
                        int pId = Convert.ToInt32(xNodeList[i].Attributes["id"].Value);
                        DotNetNuke.Modules.ActiveForums.Entities.TopicPropertyInfo p = new DotNetNuke.Modules.ActiveForums.Entities.TopicPropertyInfo();
                        p.Name = pName;
                        p.Value = pValue;
                        p.PropertyId = pId;
                        tp.Add(p);
                    }
                }
            }

            return tp;
        }
    }
}
