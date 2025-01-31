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

namespace DotNetNuke.Modules.ActiveForums.Handlers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Xml;

    using DotNetNuke.Modules.ActiveForums.Data;
    using DotNetNuke.Modules.ActiveForums.Entities;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Journal;

    [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No longer used. ")]
    public class forumhelper : HandlerBase
    {
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No longer used. ")]
        public enum Actions : int
        {
            None,
            UserPing, /* no longer used */
            GetUsersOnline,/* no longer used */
            TopicSubscribe,/* no longer used */
            ForumSubscribe,/* no longer used */
            RateTopic,/* no longer used */
            DeleteTopic,/* no longer used */
            MoveTopic,/* no longer used */
            PinTopic,/* no longer used */
            LockTopic,/* no longer used */
            MarkAnswer,/* no longer used */
            TagsAutoComplete,/* no longer used */
            DeletePost,/* no longer used */
            LoadTopic, /* no longer used */
            SaveTopic,/* no longer used */
            ForumList,/* no longer used */
            LikePost, /*no longer used*/
        }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No longer used. ")]
        public override void ProcessRequest(HttpContext context)
        {
            throw new NotImplementedException();
        }
    }
}
