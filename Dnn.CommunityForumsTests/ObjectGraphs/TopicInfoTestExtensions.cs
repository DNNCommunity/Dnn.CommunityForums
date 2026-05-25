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

namespace DotNetNuke.Modules.ActiveForumsTests.ObjectGraphs
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    using DotNetNuke.Modules.ActiveForums.Entities;

    /// <summary>
    /// Test-only extensions that attach an in-memory reply collection to a
    /// <see cref="TopicInfo"/> without modifying the production entity class.
    /// </summary>
    internal static class TopicInfoTestExtensions
    {
        private static readonly ConditionalWeakTable<TopicInfo, List<ReplyInfo>> Replies =
            new ConditionalWeakTable<TopicInfo, List<ReplyInfo>>();

        /// <summary>
        /// Returns the in-memory reply list attached to this topic, or an empty list
        /// if none has been set.
        /// </summary>
        internal static List<ReplyInfo> GetReplies(this TopicInfo topic)
        {
            List<ReplyInfo> list;
            return Replies.TryGetValue(topic, out list) ? list : new List<ReplyInfo>();
        }

        /// <summary>
        /// Attaches a reply list to this topic and returns the topic for fluent chaining.
        /// </summary>
        internal static TopicInfo WithReplies(this TopicInfo topic, List<ReplyInfo> replies)
        {
            List<ReplyInfo> existing;
            if (Replies.TryGetValue(topic, out existing))
            {
                Replies.Remove(topic);
            }

            Replies.Add(topic, replies);
            return topic;
        }
    }
}
