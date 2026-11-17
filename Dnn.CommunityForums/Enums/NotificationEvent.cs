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

namespace DotNetNuke.Modules.ActiveForums.Enums
{
    using System;

    /// <summary>Events for which notifications can be selected.</summary>
    [Flags]
    public enum NotificationEvent
    {
        /// <summary>No notification events.</summary>
        None = 0,
        /// <summary>A new topic was created in a subscribed forum.</summary>
        NewTopicInSubscribedForum = 1 << 0,
        /// <summary>A new post was created in a subscribed topic.</summary>
        NewPostToSubscribedTopic = 1 << 1,
        /// <summary>The user was mentioned.</summary>
        SomeoneMentionedMe = 1 << 2,
        /// <summary>A topic was pinned in a subscribed forum.</summary>
        TopicPinnedInSubscribedForum = 1 << 3,
        /// <summary>A post in a subscribed forum was liked.</summary>
        PostInSubscribedForumIsLiked = 1 << 4,
        /// <summary>The user's post was liked.</summary>
        MyPostIsLiked = 1 << 5,
        /// <summary>The user's topic was pinned.</summary>
        MyTopicIsPinned = 1 << 6,
        /// <summary>The user was awarded a badge.</summary>
        BadgeAwardedToMe = 1 << 7,
        /// <summary>A new reply was created in a subscribed forum.</summary>
        NewReplyInSubscribedForum = 1 << 8,
        /// <summary>A post is awaiting moderation.</summary>
        PostAwaitingModeration = 1 << 9,
        /// <summary>A post was reported to moderators.</summary>
        PostReported = 1 << 10,
        /// <summary>A user was banned.</summary>
        UserBanned = 1 << 11,
        /// <summary>The user's post was approved.</summary>
        MyPostApproved = 1 << 12,
        /// <summary>The user's post was rejected.</summary>
        MyPostRejected = 1 << 13,
        /// <summary>The user's post was deleted by a moderator.</summary>
        MyPostDeleted = 1 << 14,
        /// <summary>The user's topic was moved by a moderator.</summary>
        MyTopicMoved = 1 << 15,
    }
}
