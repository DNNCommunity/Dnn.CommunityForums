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
    using DotNetNuke.Modules.ActiveForums;
    using DotNetNuke.Modules.ActiveForums.Entities;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class TopicInfoTests
    {
        [Test]
        public void GetTopicStatusForUserTest1()
        {
            // Arrange
            var mockTopic = new Mock<DotNetNuke.Modules.ActiveForums.Entities.TopicInfo>();
            mockTopic.Object.ForumId = 1;
            mockTopic.Object.Forum = new DotNetNuke.Modules.ActiveForums.Entities.ForumInfo();
            mockTopic.Object.Forum.ForumName = "Test Forum";
            mockTopic.Object.Forum.Security = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetEmptyPermissions(-1);
            mockTopic.Object.Forum.ForumGroup = new DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo();
            mockTopic.Object.Forum.ForumGroup.GroupName = "Test Forum Group";
            mockTopic.Object.Content = new ContentInfo();
            mockTopic.Object.Content.Subject = "Test Topic";
            mockTopic.Object.Content.Body = "Test Topic";

            var mockForum = new Mock<DotNetNuke.Modules.ActiveForums.Entities.ForumInfo>();
            mockForum.Object.ForumID = 1;
            mockForum.Object.ForumName = "Test Forum";
            mockForum.Object.TotalTopics = 0;
            mockForum.Object.Security = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetEmptyPermissions(-1);
            mockForum.Object.ForumGroup = new DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo();
            mockForum.Object.ForumGroup.GroupName = "Test Forum Group";

            var mockUser = new Mock<ForumUserInfo>();
            mockUser.Object.UserId = 1;
            mockUser.Object.UserRoles = Globals.DefaultAnonRoles + "|-1;||";
            mockUser.Object.UserInfo = new DotNetNuke.Entities.Users.UserInfo();
            mockUser.Object.UserInfo.DisplayName = "Test User";
            mockUser.Object.UserInfo.Profile = new DotNetNuke.Entities.Users.UserProfile();
            mockUser.Object.UserInfo.Profile.PreferredLocale = "en-US";

            var expectedResult = DotNetNuke.Modules.ActiveForums.Enums.TopicStatus.Forbidden;

            // Act
            var actualResult = mockTopic.Object.GetTopicStatusForUser(mockUser.Object);

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void GetTopicStatusForUserTest2()
        {
            // Arrange
            var mockTopic = new Mock<DotNetNuke.Modules.ActiveForums.Entities.TopicInfo>();
            mockTopic.Object.ForumId = 1;
            mockTopic.Object.Forum = new DotNetNuke.Modules.ActiveForums.Entities.ForumInfo();
            mockTopic.Object.Forum.ForumName = "Test Forum";
            mockTopic.Object.Forum.Security = new DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo()
            {
                View = Globals.DefaultAnonRoles + "|-1;||",
            };
            mockTopic.Object.Forum.ForumGroup = new DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo();
            mockTopic.Object.Forum.ForumGroup.GroupName = "Test Forum Group";
            mockTopic.Object.Content = new ContentInfo();
            mockTopic.Object.Content.Subject = "Test Topic";
            mockTopic.Object.Content.Body = "Test Topic";

            var mockForum = new Mock<DotNetNuke.Modules.ActiveForums.Entities.ForumInfo>();
            mockForum.Object.ForumID = 1;
            mockForum.Object.ForumName = "Test Forum";
            mockForum.Object.TotalTopics = 2;
            mockForum.Object.Security = new DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo()
            {
                View = Globals.DefaultAnonRoles + "|-1;||",
            };
            mockForum.Object.ForumGroup = new DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo();
            mockForum.Object.ForumGroup.GroupName = "Test Forum Group";

            var mockUser = new Mock<ForumUserInfo>();
            mockUser.Object.UserId = 1;
            mockUser.Object.UserRoles = Globals.DefaultAnonRoles + "|-1;||";
            mockUser.Object.UserInfo = new DotNetNuke.Entities.Users.UserInfo();
            mockUser.Object.UserInfo.DisplayName = "Test User";
            mockUser.Object.UserInfo.Profile = new DotNetNuke.Entities.Users.UserProfile();
            mockUser.Object.UserInfo.Profile.PreferredLocale = "en-US";

            var expectedResult = DotNetNuke.Modules.ActiveForums.Enums.TopicStatus.Unread;

            // Act
            var actualResult = mockTopic.Object.GetTopicStatusForUser(mockUser.Object);

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }
    }
}
