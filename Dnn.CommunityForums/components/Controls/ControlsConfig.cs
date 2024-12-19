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
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;


	public class ControlsConfig
	{
        #region Public Properties
        public string AdminRoles { get; set; }

        public string AppPath { get; set; }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public int ContentId { get; set; }

        public string DefaultViewRoles { get; set; }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public int ForumId { get; set; }
        
        public int ModuleId { get; set; }

        public int ForumModuleId { get; set; }

        public string MembersLink { get; set; }

        public int PageId { get; set; } = -1;

        public string ProfileLink { get; set; }

        public int PortalId { get; set; }

        public string TemplatePath { get; set; }

        public string ThemePath { get; set; }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public int TopicId { get; set; }

        public DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo User { get; set; }

        #endregion


    }
}
