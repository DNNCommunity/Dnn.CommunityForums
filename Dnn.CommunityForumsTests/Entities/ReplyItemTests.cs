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
    public class ReplyInfoTests
    {
        [Test]
        public void GetReplyStatusForUserTest1()
        {
            // Arrange
            var mockReply = new Mock<DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo>();
            mockReply.Object.ForumId = 1;
            mockReply.Object.Forum = new DotNetNuke.Modules.ActiveForums.Entities.ForumInfo();
            mockReply.Object.Forum.ForumName = "Test Forum";
            mockReply.Object.Forum.Security = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetEmptyPermissions(-1);
            mockReply.Object.Forum.ForumGroup = new DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo();
            mockReply.Object.Forum.ForumGroup.GroupName = "Test Forum Group";
            mockReply.Object.Content = new ContentInfo();
            mockReply.Object.Content.Subject = "Test Reply";
            mockReply.Object.Content.Body = "Test Reply";

            mockReply.Object.Topic = new DotNetNuke.Modules.ActiveForums.Entities.TopicInfo();
            mockReply.Object.Topic.ForumId = 1;
            mockReply.Object.Topic.Forum = new DotNetNuke.Modules.ActiveForums.Entities.ForumInfo();
            mockReply.Object.Topic.Forum.ForumName = "Test Forum";
            mockReply.Object.Topic.Forum.Security = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetEmptyPermissions(-1);
            mockReply.Object.Topic.Forum.ForumGroup = new DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo();
            mockReply.Object.Topic.Forum.ForumGroup.GroupName = "Test Forum Group";
            mockReply.Object.Topic.Content = new ContentInfo();
            mockReply.Object.Topic.Content.Subject = "Test Topic";
            mockReply.Object.Topic.Content.Body = "Test Topic";

            var mockForum = new Mock<DotNetNuke.Modules.ActiveForums.Entities.ForumInfo>();
            mockForum.Object.ForumID = 1;
            mockForum.Object.ForumName = "Test Forum";
            mockForum.Object.TotalTopics = 0;
            mockForum.Object.Security = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetEmptyPermissions(-1);
            mockForum.Object.ForumGroup = new DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo();
            mockForum.Object.ForumGroup.GroupName = "Test Forum Group";

            var mockUser = new Mock<ForumUserInfo>();
            mockUser.Object.UserId = 1;
            mockUser.Object.UserPermSet = Globals.DefaultAnonRoles + "|-1;||";
            mockUser.Object.UserInfo = new DotNetNuke.Entities.Users.UserInfo();
            mockUser.Object.UserInfo.DisplayName = "Test User";
            mockUser.Object.UserInfo.Profile = new DotNetNuke.Entities.Users.UserProfile();
            mockUser.Object.UserInfo.Profile.PreferredLocale = "en-US";

            var expectedResult = DotNetNuke.Modules.ActiveForums.Enums.ReplyStatus.Forbidden;

            // Act
            var actualResult = mockReply.Object.GetReplyStatusForUser(mockUser.Object);

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));

        }
    }
}
