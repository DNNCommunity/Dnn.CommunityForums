//
// Community Forums
// Copyright (c) 2013-2024
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
namespace DotNetNuke.Modules.ActiveForums
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Xml;

    public static class ConfigUtils
    {
        private const string Web_config_system_webserver_modules_node_XPath = "//system.webServer/modules";
        private const string ForumsRewriterTypeNameFull = "DotNetNuke.Modules.ActiveForums.ForumsReWriter, DotNetNuke.Modules.ActiveForums";
        private const string ForumsRewriterTypeNameShort = "ForumsReWriter";
        private const string Attribute_name = "name";
        private const string Attribute_type = "type";
        private const string Attribute_precondition = "preCondition";

        public static bool IsRewriterInstalled(string configPath)
        {
            bool isInstalled = false;
            try
            {
                XmlDocument xDoc = new XmlDocument();
                XmlReaderSettings readerSettings = new XmlReaderSettings
                {
                    IgnoreWhitespace = true,
                    IgnoreComments = true,
                    CloseInput = true,
                };
                using (XmlReader reader = XmlReader.Create(configPath, readerSettings))
                {
                    xDoc.Load(reader);
                    reader.Close();
                }

                if (xDoc != null)
                {
                    System.Xml.XmlNode xRoot = xDoc.DocumentElement;
                    System.Xml.XmlNode xNode = xRoot.SelectSingleNode(Web_config_system_webserver_modules_node_XPath);
                    if (xNode != null)
                    {
                        foreach (XmlNode n in xNode.ChildNodes)
                        {
                            if ((n.Attributes[Attribute_name].Value == ForumsRewriterTypeNameShort) && (n.Attributes[Attribute_type].Value == ForumsRewriterTypeNameFull))
                            {
                                isInstalled = true;
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }

            return isInstalled;
        }

        public static bool InstallRewriter(string configPath)
        {
            if (IsRewriterInstalled(configPath))
            {
                return true;
            }

            try
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(configPath);
                if (xDoc != null)
                {
                    System.Xml.XmlNode xRoot = xDoc.DocumentElement;
                    System.Xml.XmlNode xNode = xRoot.SelectSingleNode(Web_config_system_webserver_modules_node_XPath);
                    if (xNode != null)
                    {
                        XmlElement xNewNode = xDoc.CreateElement("add");
                        XmlAttribute xAttrib = xDoc.CreateAttribute(Attribute_name);
                        xAttrib.Value = ForumsRewriterTypeNameShort;
                        xNewNode.Attributes.Append(xAttrib);
                        xAttrib = xDoc.CreateAttribute(Attribute_type);
                        xAttrib.Value = ForumsRewriterTypeNameFull;
                        xNewNode.Attributes.Append(xAttrib);
                        xAttrib = xDoc.CreateAttribute(Attribute_precondition);
                        xAttrib.Value = "managedHandler";
                        xNewNode.Attributes.Append(xAttrib);
                        xNode.PrependChild(xNewNode);
                        xDoc.Save(configPath);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                return false;
            }
        }

        public static bool UninstallRewriter(string configPath)
        {
            if (!IsRewriterInstalled(configPath))
            {
                return true;
            }

            try
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(configPath);
                if (xDoc != null)
                {
                    System.Xml.XmlNode xRoot = xDoc.DocumentElement;
                    System.Xml.XmlNode xNode = xRoot.SelectSingleNode(Web_config_system_webserver_modules_node_XPath);
                    if (xNode != null)
                    {
                        foreach (XmlNode n in xNode.ChildNodes)
                        {
                            if (n.Attributes[Attribute_name].Value == ForumsRewriterTypeNameShort)
                            {
                                xNode.RemoveChild(n);
                                break;
                            }
                        }

                        xDoc.Save(configPath);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                return false;
            }
        }
    }
}
