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

namespace DotNetNuke.Modules.ActiveForums
{
    using System;
    using System.Collections.Generic;

    [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
    public class Templates
    {
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public enum TemplateTypes : int
        {
            All, // 0
            System, // 1
            ForumView, // 2
            TopicView, // 3
            TopicsView, // 4
            TopicForm, // 5
            ReplyForm, // 6
            QuickReplyForm, // 7
            Email, // 8
            Profile, // 9
            ModEmail, // 10
            PostInfo, // 11
        }
    }

    [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
    public class TemplateInfo
    {
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public int TemplateId { get; set; }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public int PortalId { get; set; }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public int ModuleId { get; set; }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public Templates.TemplateTypes TemplateType { get; set; }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public bool IsSystem { get; set; }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public string Subject { get; set; }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public string Title { get; set; }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public string Template { get; set; }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public string TemplateHTML { get; set; }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public string TemplateText { get; set; }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public DateTime DateCreated { get; set; }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public DateTime DateUpdated { get; set; }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public string FileName { get; set; }

    }

    [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
    public class TemplateController
    {
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public int Template_Save(TemplateInfo templateInfo) => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public List<TemplateInfo> Template_List(int portalId, int moduleId) => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public List<TemplateInfo> Template_List(int portalId, int moduleId, Templates.TemplateTypes templateType) => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public void Template_Delete(int templateId, int portalId, int moduleId) => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public TemplateInfo Template_Get(string templateName, int portalId, int moduleId) => throw new NotImplementedException();

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public TemplateInfo Template_Get(int templateId) => throw new NotImplementedException();
    }
}
