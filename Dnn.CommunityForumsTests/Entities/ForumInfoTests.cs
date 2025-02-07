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
    public class ForumInfoTests : DotNetNuke.Modules.ActiveForumsTests.TestBase
    {
        [Test]
        public void GetForumStatusForUserTest1()
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

            var mockUser = new Mock<ForumUserInfo>();
            mockUser.Object.UserId = 1;
            mockUser.Object.UserRoles = Globals.DefaultAnonRoles + "|-1;||";
            mockUser.Object.UserInfo = new DotNetNuke.Entities.Users.UserInfo();
            mockUser.Object.UserInfo.DisplayName = "Test User";
            mockUser.Object.UserInfo.Profile = new DotNetNuke.Entities.Users.UserProfile();
            mockUser.Object.UserInfo.Profile.PreferredLocale = "en-US";

            var expectedResult = DotNetNuke.Modules.ActiveForums.Enums.ForumStatus.Forbidden;

            // Act
            var actualResult = mockForum.Object.GetForumStatusForUser(mockUser.Object);

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void GetForumStatusForUserTest2()
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
                    Security = new DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo()
                    {
                        View = Globals.DefaultAnonRoles + "|-1;||",
                    },
                    ForumGroup = new DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo
                    {
                        GroupName = "Test Forum Group",
                    },
                },
            };

            var mockUser = new Mock<ForumUserInfo>
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

            var expectedResult = DotNetNuke.Modules.ActiveForums.Enums.ForumStatus.Empty;

            // Act
            var actualResult = mockForum.Object.GetForumStatusForUser(mockUser.Object);

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }
    }
}
