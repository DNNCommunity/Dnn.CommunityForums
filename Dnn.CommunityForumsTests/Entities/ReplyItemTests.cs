﻿// Copyright (c) by DNN Community
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
    public class ReplyInfoTests : DotNetNuke.Modules.ActiveForumsTests.TestBase
    {
        [Test]
        public void GetReplyStatusForUserTest1()
        {
            // Arrange
            var mockForum = new Mock<DotNetNuke.Modules.ActiveForums.Entities.ForumInfo>(DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings())
            {
                Object =
                {
                    PortalSettings = DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings(),
                    ForumID = 1,
                    ForumName = "Test Forum",
                    TotalTopics = 0,
                    Security = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetEmptyPermissions(-1),
                    ForumGroup = new DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo { GroupName = "Test Forum Group" }
                }
            };


            var mockTopic = new Mock<DotNetNuke.Modules.ActiveForums.Entities.TopicInfo>
            {
                Object =
                {
                    ForumId = 1,
                    TopicId = 1,
                    Forum = mockForum.Object,
                    Content = new DotNetNuke.Modules.ActiveForums.Entities.ContentInfo
                    {
                        ContentId = 1,
                        ModuleId = 1,
                        Subject = "Test Topic",
                        Body = "Test Topic"
                    }
                },
            };

            var mockContent = new Mock<DotNetNuke.Modules.ActiveForums.Entities.ContentInfo>
            {
                Object =
                {
                    ContentId = 2,
                    ModuleId = 1,
                    Subject = "Test Reply",
                    Body = "Test Reply",
                },
            };
            var mockReply = new Mock<DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo>()
            {
                Object =
                {
                    ReplyId = 1,
                    TopicId = mockTopic.Object.TopicId,
                    Topic = mockTopic.Object,
                    ContentId = mockContent.Object.ContentId,
                    Content = mockContent.Object,

                }
            };

            var mockUser = new Mock<ForumUserInfo>()
            {
                Object =
                {

                    UserId = 1,
                    UserRoles = Globals.DefaultAnonRoles + "|-1;||",
                    UserInfo = new DotNetNuke.Entities.Users.UserInfo
                    {
                        DisplayName = "Test User",
                        Profile = new DotNetNuke.Entities.Users.UserProfile
                        {
                            PreferredLocale = "en-US",
                        },
                    },
                },
            };

            var expectedResult = DotNetNuke.Modules.ActiveForums.Enums.ReplyStatus.Forbidden;

            // Act
            var actualResult = mockReply.Object.GetReplyStatusForUser(mockUser.Object);

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }
    }
}
