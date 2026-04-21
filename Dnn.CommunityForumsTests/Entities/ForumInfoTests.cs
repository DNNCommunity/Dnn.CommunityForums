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
                    Security = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetEmptyPermissions(this.MockModule.Object.ModuleID),
                    ForumGroup = new DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo { GroupName = "Test Forum Group" },
                },
            };

            var mockUserInfo = new Mock<DotNetNuke.Entities.Users.UserInfo>
            {
                Object =
                {
                    PortalID = DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings().PortalId,
                    UserID = DotNetNuke.Tests.Utilities.Constants.UserID_User12,
                    IsSuperUser = false,
                    Profile = new DotNetNuke.Entities.Users.UserProfile()
                    {
                        PreferredLocale = "en-US",
                    },
                },
            };
            var mockUser = new Mock<DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo>(this.MockModule.Object.ModuleID, DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings(), mockUserInfo.Object)
            {
                Object =
                {
                    PortalId = DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings().PortalId,
                    UserId = mockUserInfo.Object.UserID,
                    UserInfo = mockUserInfo.Object,
                },
            };

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

            var mockUserInfo = new Mock<DotNetNuke.Entities.Users.UserInfo>
            {
                Object =
                {
                    PortalID = DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings().PortalId,
                    UserID = DotNetNuke.Tests.Utilities.Constants.UserID_User12,
                    IsSuperUser = false,
                    Profile = new DotNetNuke.Entities.Users.UserProfile()
                    {
                        PreferredLocale = "en-US",
                    },
                },
            };
            var mockUser = new Mock<DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo>(this.MockModule.Object.ModuleID, DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings(), mockUserInfo.Object)
            {
                Object =
                {
                    PortalId = DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings().PortalId,
                    UserId = mockUserInfo.Object.UserID,
                    UserInfo = mockUserInfo.Object,
                },
            };

            var expectedResult = DotNetNuke.Modules.ActiveForums.Enums.ForumStatus.Empty;

            // Act
            var actualResult = mockForum.Object.GetForumStatusForUser(mockUser.Object);

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void IsPublicForum_WithAllUsersRole_ReturnsTrue()
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
                        Read = Globals.DefaultAnonRoles + DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators.ToString(),
                        View = Globals.DefaultAnonRoles + DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators.ToString(),
                    },
                    ForumGroup = new DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo
                    {
                        GroupName = "Test Forum Group",
                    },
                },
            };

            // Act
            bool result = mockForum.Object.IsPublicForum;

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void IsPublicForum_WithNoPublicRoles_ReturnsFalse()
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
                        Read = DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators.ToString(),
                        View = DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators.ToString(),
                    },
                    ForumGroup = new DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo
                    {
                        GroupName = "Test Forum Group",
                    },
                },
            };

            // Act
            bool result = mockForum.Object.IsPublicForum;

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void TotalLikeCount_WithCachedValue_ReturnsCachedValue()
        {
            // Arrange
            var forum = new DotNetNuke.Modules.ActiveForums.Entities.ForumInfo(DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings());
            SetTotalLikeCountCache(forum, 12);

            // Act
            int totalLikeCount = forum.TotalLikeCount;

            // Assert
            Assert.That(totalLikeCount, Is.EqualTo(12));
        }

        [Test]
        public void AverageLikeScore_WithNoTopics_ReturnsZero()
        {
            // Arrange
            var forum = new DotNetNuke.Modules.ActiveForums.Entities.ForumInfo(DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings())
            {
                TotalTopics = 0,
            };

            SetTotalLikeCountCache(forum, 12);

            // Act
            double averageLikeScore = forum.AverageLikeScore;

            // Assert
            Assert.That(averageLikeScore, Is.EqualTo(0D));
        }

        [Test]
        public void AverageLikeScore_WithTopicsAndCachedLikes_ReturnsExpectedAverage()
        {
            // Arrange
            var forum = new DotNetNuke.Modules.ActiveForums.Entities.ForumInfo(DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings())
            {
                TotalTopics = 4,
            };

            SetTotalLikeCountCache(forum, 10);

            // Act
            double averageLikeScore = forum.AverageLikeScore;

            // Assert
            Assert.That(averageLikeScore, Is.EqualTo(2.5D));
        }

        private static void SetTotalLikeCountCache(DotNetNuke.Modules.ActiveForums.Entities.ForumInfo forum, int totalLikeCount)
        {
            var totalLikeCountField = typeof(DotNetNuke.Modules.ActiveForums.Entities.ForumInfo).GetField("totalLikeCount", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            totalLikeCountField?.SetValue(forum, totalLikeCount);
        }
    }
}
