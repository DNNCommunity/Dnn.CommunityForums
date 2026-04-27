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

namespace DotNetNuke.Modules.ActiveForumsTests.Entities
{
    using DotNetNuke.Modules.ActiveForums.Enums;
    using DotNetNuke.Modules.ActiveForumsTests.ObjectGraphs;

    using NUnit.Framework;

    [TestFixture]
    public class TopicInfoTests : DotNetNuke.Modules.ActiveForumsTests.TestBase
    {
        [Test]
        public void GetTopicStatusForUserTest1()
        {
            // Arrange — anonymous user cannot view a members-only topic → Forbidden
            var topic = this.TopicReplyGraph.Find(t => t.TopicId == TopicReplyObjectGraph.MembersOnlyTopicId);
            var forumUser = this.ForumUserGraph.Find(u => u.UserId == DotNetNuke.Tests.Utilities.Constants.USER_AnonymousUserId);

            var expectedResult = TopicStatus.Forbidden;

            // Act
            var actualResult = topic.GetTopicStatusForUser(forumUser);

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void GetTopicStatusForUserTest2()
        {
            // Arrange — registered user can view a public topic → Unread
            var topic = this.TopicReplyGraph.Find(t => t.TopicId == TopicReplyObjectGraph.AnnouncementTopicId);
            var forumUser = this.ForumUserGraph.Find(u => u.UserId == DotNetNuke.Tests.Utilities.Constants.UserID_User12);

            var expectedResult = TopicStatus.Unread;

            // Act
            var actualResult = topic.GetTopicStatusForUser(forumUser);

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }
    }
}
