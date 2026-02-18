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

namespace DotNetNuke.Modules.ActiveForums.ViewModels
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Entities.Host;

    public class Reply : DotNetNuke.Modules.ActiveForums.ViewModels.IPost
    {
        public Reply()
        {
        }

        public Reply(DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo reply)
            : base(reply)
        {
            this.TopicAuthorId = reply.Topic.Author?.AuthorId ?? DotNetNuke.Common.Utilities.Null.NullInteger;
            this.TopicAuthorDisplayName = reply.Topic.Author?.DisplayName ?? reply.Content.AuthorName ?? string.Empty;
            this.TopicAuthorFirstName = reply.Topic.Author?.FirstName ?? string.Empty;
            this.TopicAuthorLastName = reply.Topic.Author?.LastName ?? string.Empty;
            this.TopicAuthorUsername = reply.Topic.Author?.Username ?? string.Empty;
            this.TopicAuthorEmail = reply.Topic.Author?.Email ?? string.Empty;
            this.TopicBody = reply.Topic.Content?.Body ?? string.Empty;
            this.TopicDateCreated = reply.Topic.Content?.DateCreated;
            this.TopicDateUpdated = reply.Topic.Content?.DateUpdated;
        }

        public int TopicAuthorId { get; set; }

        public string TopicAuthorDisplayName { get; set; }

        public string TopicAuthorUsername { get; set; }

        public string TopicAuthorEmail { get; set; }

        public string TopicAuthorFirstName { get; set; }

        public string TopicAuthorLastName { get; set; }

        public string TopicBody { get; set; }

        public DateTime? TopicDateCreated { get; set; }

        public DateTime? TopicDateUpdated { get; set; }

    }
}
