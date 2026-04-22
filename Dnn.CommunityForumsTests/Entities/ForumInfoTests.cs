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
    using System.Collections.Generic;

    using DotNetNuke.Modules.ActiveForums;
    using DotNetNuke.Modules.ActiveForums.Entities;
    using DotNetNuke.Modules.ActiveForumsTests.ObjectGraphs;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class ForumInfoTests : DotNetNuke.Modules.ActiveForumsTests.TestBase
    {
        [Test]
        public void GetForumStatusForUserTest1()
        {
            // Arrange
            // Use the private (members-only) forum — no View permission for anonymous users.
            var forum = this.ForumsGraph.Find(f => f.ForumID == ForumsObjectGraph.AdministratorsOnlyForumId);
            var forumUser = this.ForumUserGraph.Find(u => u.UserId == DotNetNuke.Tests.Utilities.Constants.UserID_User12);

            var expectedResult = Modules.ActiveForums.Enums.ForumStatus.Forbidden;

            // Act
            var actualResult = forum.GetForumStatusForUser(forumUser);

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void GetForumStatusForUserTest2()
        {
            // Arrange
            // Use a public forum — anonymous users can view it so the result is Empty (no new posts).
            var forum = this.ForumsGraph.Find(f => f.ForumID == ForumsObjectGraph.AnnouncementsForumId);
            var forumUser = this.ForumUserGraph.Find(u => u.UserId == DotNetNuke.Tests.Utilities.Constants.UserID_User12);

            var expectedResult = Modules.ActiveForums.Enums.ForumStatus.Empty;

            // Act
            var actualResult = forum.GetForumStatusForUser(forumUser);

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void IsPublicForum_WithAllUsersRole_ReturnsTrue()
        {
            // Arrange — any forum in the General group has PublicViewPermission.
            var forum = this.ForumsGraph.Find(f => f.ForumID == ForumsObjectGraph.AnnouncementsForumId);

            // Act
            bool result = forum.IsPublicForum;

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void IsPublicForum_WithNoPublicRoles_ReturnsFalse()
        {
            // Arrange — Admin forums use AdministratorsPermission only.
            var forum = this.ForumsGraph.Find(f => f.ForumID == ForumsObjectGraph.AdministratorsOnlyForumId);

            // Act
            bool result = forum.IsPublicForum;

            // Assert
            Assert.That(result, Is.False);
        }
    }
}
