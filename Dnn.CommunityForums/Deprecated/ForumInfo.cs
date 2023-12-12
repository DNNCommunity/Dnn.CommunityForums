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
using DotNetNuke.ComponentModel.DataAnnotations;
using System.Web.Caching;
using DotNetNuke.Modules.ActiveForums.Entities;

namespace DotNetNuke.Modules.ActiveForums.Entities
{
    public partial class ForumInfo
    {
        [Obsolete("Deprecated in Community Forums. Scheduled for removal in 10.00.00. Not Used.")]
        [IgnoreColumn()]
        public int LastPostLastPostID { get; set; }
        [Obsolete("Deprecated in Community Forums. Scheduled for removal in 10.00.00. Not Used.")]
        [IgnoreColumn()]
        public int LastPostParentPostID { get; set; }
        [Obsolete("Deprecated in Community Forums. Scheduled for removal in 10.00.00. Not Used.")]
        [IgnoreColumn()]
        public int CustomFieldType { get; set; }
        [Obsolete("Deprecated in Community Forums. Scheduled for removal in 10.00.00. Not Used.")]
        [IgnoreColumn()]
        public int AttachMaxHeight
        {
            get { return Utilities.SafeConvertInt(ForumSettings[ForumSettingKeys.AttachMaxHeight], 500); }
        }
        [Obsolete("Deprecated in Community Forums. Scheduled for removal in 10.00.00. Not Used.")]
        [IgnoreColumn()]
        public int AttachMaxWidth
        {
            get { return Utilities.SafeConvertInt(ForumSettings[ForumSettingKeys.AttachMaxWidth], 500); }
        }
        [Obsolete("Deprecated in Community Forums. Scheduled for removal in 10.00.00. Not Used.")]
        [IgnoreColumn()]
        public int EditorStyle
        {
            get { return Utilities.SafeConvertInt(ForumSettings[ForumSettingKeys.EditorStyle], 1); }
        }
        [Obsolete("Deprecated in Community Forums. Scheduled for removal in 10.00.00. Not Used.")]
        [IgnoreColumn()]
        public string EditorToolBar
        {
            get { return Utilities.SafeConvertString(ForumSettings[ForumSettingKeys.EditorToolbar], "bold,italic,underline"); }
        }
    }
}