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

namespace DotNetNuke.Modules.ActiveForumsTests.Services.Tokens
{
    using System;
    using System.Globalization;
    using System.Text;

    using DotNetNuke.Services.Tokens;
    using DotNetNuke.Modules.ActiveForums;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class TokenReplacerTests : DotNetNuke.Modules.ActiveForumsTests.TestBase
    {
        [Test]
        public void ReplaceTopicTokensTest1()
        {
            // Arrange
            var featureSettings = new System.Collections.Hashtable
            {
                { ForumSettingKeys.DefaultTrustLevel, TrustTypes.NotTrusted },
            };
            const string emptyPermissions = ";||";
            var mockPermissions = new Mock<DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo>
            {
                Object =
                {
                    View = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_RegisteredUsers}{emptyPermissions}",
                    Read = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_RegisteredUsers}{emptyPermissions}",
                    Create = emptyPermissions,
                    Reply = emptyPermissions,
                    Edit = emptyPermissions,
                    Delete = emptyPermissions,
                    Lock = emptyPermissions,
                    Pin = emptyPermissions,
                    Attach = emptyPermissions,
                    Poll = emptyPermissions,
                    Trust = emptyPermissions,
                    Subscribe = emptyPermissions,
                    Announce = emptyPermissions,
                    Prioritize = emptyPermissions,
                    Moderate = emptyPermissions,
                    Move = emptyPermissions,
                    Split = emptyPermissions,
                    ManageUsers = emptyPermissions,
                },
            };

            var mockForum = new Mock<DotNetNuke.Modules.ActiveForums.Entities.ForumInfo>(DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings())
            {
                Object =
                {
                    PortalSettings = DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings(),
                    ForumID = 1,
                    ForumName = "Test Forum",
                    TotalTopics = 0,
                    Security = mockPermissions.Object,
                    ForumGroup = new DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo
                    {
                        GroupName = "Test Forum Group",
                    },
                    FeatureSettings = new DotNetNuke.Modules.ActiveForums.Entities.FeatureSettings(featureSettings),
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

            var mockAuther = new Mock<Modules.ActiveForums.Entities.AuthorInfo>(mockUser.Object);
            var mockTopic = new Mock<DotNetNuke.Modules.ActiveForums.Entities.TopicInfo>
            {
                Object =
                {
                    ForumId = 1,
                    TopicId = 1,
                    Forum = mockForum.Object,
                    Author = mockAuther.Object,
                    Content = new DotNetNuke.Modules.ActiveForums.Entities.ContentInfo
                    {
                        AuthorId = mockAuther.Object.AuthorId,
                        ContentId = 1,
                        ModuleId = 1,
                        Subject = "Test Topic",
                        Body = "Test Topic",
                    },
                },
            };

            var format = "blah blah blah {0} blah";

            var expectedResult = "blah blah blah Test Topic blah";
            bool propNotFound = false;

            // Act
            var actualResult = mockTopic.Object.GetProperty("subject", format, new CultureInfo(mockUser.Object.UserInfo.Profile.PreferredLocale), mockUser.Object.UserInfo, Scope.DefaultSettings, ref propNotFound);

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void ReplaceTopicTokensTest2()
        {
            // Arrange
            var featureSettings = new System.Collections.Hashtable
            {
                { ForumSettingKeys.DefaultTrustLevel, TrustTypes.NotTrusted },
            };
            const string emptyPermissions = ";||";
            var mockPermissions = new Mock<DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo>
            {
                Object =
                {
                    View = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_RegisteredUsers}{emptyPermissions}",
                    Read = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_RegisteredUsers}{emptyPermissions}",
                    Create = emptyPermissions,
                    Reply = emptyPermissions,
                    Edit = emptyPermissions,
                    Delete = emptyPermissions,
                    Lock = emptyPermissions,
                    Pin = emptyPermissions,
                    Attach = emptyPermissions,
                    Poll = emptyPermissions,
                    Trust = emptyPermissions,
                    Subscribe = emptyPermissions,
                    Announce = emptyPermissions,
                    Prioritize = emptyPermissions,
                    Moderate = emptyPermissions,
                    Move = emptyPermissions,
                    Split = emptyPermissions,
                    ManageUsers = emptyPermissions,
                },
            };

            var mockForum = new Mock<DotNetNuke.Modules.ActiveForums.Entities.ForumInfo>(DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings())
            {
                Object =
                {
                    PortalSettings = DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings(),
                    ForumID = 1,
                    ForumName = "Test Forum",
                    TotalTopics = 0,
                    Security = mockPermissions.Object,
                    ForumGroup = new DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo
                    {
                        GroupName = "Test Forum Group",
                    },
                    FeatureSettings = new DotNetNuke.Modules.ActiveForums.Entities.FeatureSettings(featureSettings),
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

            var mockAuther = new Mock<Modules.ActiveForums.Entities.AuthorInfo>(mockUser.Object);
            var mockTopic = new Mock<DotNetNuke.Modules.ActiveForums.Entities.TopicInfo>
            {
                Object =
                {
                    ForumId = 1,
                    TopicId = 1,
                    Forum = mockForum.Object,
                    Author = mockAuther.Object,
                    Content = new DotNetNuke.Modules.ActiveForums.Entities.ContentInfo
                    {
                        AuthorId = mockAuther.Object.AuthorId,
                        ContentId = 1,
                        ModuleId = 1,
                        Subject = "Test Topic",
                        Body = "Test Topic",
                    },
                },
            };

            var navigationManager = new Modules.ActiveForums.Services.Tests.NavigationManager(null); // new Services.URLNavigator().NavigationManager();

            var templateStringBuilder = new StringBuilder("blah blah [SPLITBUTTONS1] blah [SPLITBUTTONS2]  [FORUMTOPIC:SUBJECT] blah");
            var expectedResult = "blah blah [SPLITBUTTONS1] blah [SPLITBUTTONS2]  Test Topic blah";

            var requestUri = new Uri("https://localhost/forums");
            var rawUrl = "/forums";

            // Act
            var actualResult = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.ReplaceTopicTokens(templateStringBuilder, mockTopic.Object, DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings(), this.MainSettings.Object, navigationManager, mockUser.Object, requestUri, rawUrl).ToString();

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void ReplaceForumTokensTest1()
        {
            // Arrange
            var featureSettings = new System.Collections.Hashtable
            {
                { ForumSettingKeys.DefaultTrustLevel, TrustTypes.NotTrusted },
            };

            const string emptyPermissions = ";||";
            var mockPermissions = new Mock<DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo>
            {
                Object =
                {
                    View = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_RegisteredUsers}{emptyPermissions}",
                    Read = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_RegisteredUsers}{emptyPermissions}",
                    Create = emptyPermissions,
                    Reply = emptyPermissions,
                    Edit = emptyPermissions,
                    Delete = emptyPermissions,
                    Lock = emptyPermissions,
                    Pin = emptyPermissions,
                    Attach = emptyPermissions,
                    Poll = emptyPermissions,
                    Trust = emptyPermissions,
                    Subscribe = emptyPermissions,
                    Announce = emptyPermissions,
                    Prioritize = emptyPermissions,
                    Moderate = emptyPermissions,
                    Move = emptyPermissions,
                    Split = emptyPermissions,
                    ManageUsers = emptyPermissions,
                },
            };

            var mockForum = new Mock<DotNetNuke.Modules.ActiveForums.Entities.ForumInfo>(DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings())
            {
                Object =
                {
                    PortalSettings = DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings(),
                    ForumID = 1,
                    ForumName = "Test Forum",
                    TotalTopics = 0,
                    Security = mockPermissions.Object,
                    ForumGroup = new DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo
                    {
                        GroupName = "Test Forum Group",
                    },
                    FeatureSettings = new DotNetNuke.Modules.ActiveForums.Entities.FeatureSettings(featureSettings),
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

            var expectedResult = "blah blah Test Forum Group blah blah";
            bool propNotFound = false;

            // Act
            var actualResult = mockForum.Object.GetProperty("groupname", "blah blah {0} blah blah", new CultureInfo(mockUser.Object.UserInfo.Profile.PreferredLocale), mockUser.Object.UserInfo, Scope.DefaultSettings, ref propNotFound);

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void ReplaceForumTokensTest2()
        {
            // Arrange
            var featureSettings = new System.Collections.Hashtable
            {
                { ForumSettingKeys.DefaultTrustLevel, TrustTypes.NotTrusted },
            };
            const string emptyPermissions = ";||";
            var mockPermissions = new Mock<DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo>
            {
                Object =
                {
                    View = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_RegisteredUsers}{emptyPermissions}",
                    Read = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_RegisteredUsers}{emptyPermissions}",
                    Create = emptyPermissions,
                    Reply = emptyPermissions,
                    Edit = emptyPermissions,
                    Delete = emptyPermissions,
                    Lock = emptyPermissions,
                    Pin = emptyPermissions,
                    Attach = emptyPermissions,
                    Poll = emptyPermissions,
                    Trust = emptyPermissions,
                    Subscribe = emptyPermissions,
                    Announce = emptyPermissions,
                    Prioritize = emptyPermissions,
                    Moderate = emptyPermissions,
                    Move = emptyPermissions,
                    Split = emptyPermissions,
                    ManageUsers = emptyPermissions,
                },
            };

            var mockForum = new Mock<DotNetNuke.Modules.ActiveForums.Entities.ForumInfo>(DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings())
            {
                Object =
                {
                    PortalSettings = DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings(),
                    ForumID = 1,
                    ForumName = "Test Forum",
                    TotalTopics = 0,
                    Security = mockPermissions.Object,
                    ForumGroup = new DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo
                    {
                        GroupName = "Test Forum Group",
                        PortalSettings = DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings(),
                        FeatureSettings = new DotNetNuke.Modules.ActiveForums.Entities.FeatureSettings(featureSettings),
                        Security = mockPermissions.Object,
                    },
                    FeatureSettings = new DotNetNuke.Modules.ActiveForums.Entities.FeatureSettings(featureSettings),
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

            var navigationManager = new DotNetNuke.Modules.ActiveForums.Services.Tests.NavigationManager(null); // new Services.URLNavigator().NavigationManager();

            var templateStringBuilder = new StringBuilder("blah blah [FORUMGROUP:GROUPNAME] blah blah");
            var expectedResult = "blah blah Test Forum Group blah blah";

            var requestUri = new Uri("https://localhost/forums");
            var rawUrl = "/forums";

            // Act


            var actualResult = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.ReplaceForumTokens(templateStringBuilder, mockForum.Object, DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings(), this.MainSettings.Object, navigationManager, mockUser.Object, 0, CurrentUserTypes.Auth, requestUri, rawUrl).ToString();

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void RemovePrefixedToken1()
        {
            // Arrange
            var templateStringBuilder = new StringBuilder("blah blah [TOKENTOREMOVE:5] blah blah");
            var tokenPrefix = "TOKENTOREMOVE";
            var expectedResult = "blah blah  blah blah";

            // Act
            var actualResult = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.RemovePrefixedToken(templateStringBuilder, tokenPrefix).ToString();

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void RemovePrefixedToken2()
        {
            // Arrange
            var templateStringBuilder = new StringBuilder("blah blah [TOKENTOREMOVE] blah blah");
            var tokenPrefix = "[TOKENTOREMOVE";
            var expectedResult = "blah blah  blah blah";

            // Act
            var actualResult = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.RemovePrefixedToken(templateStringBuilder, tokenPrefix).ToString();

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }
    }
}
