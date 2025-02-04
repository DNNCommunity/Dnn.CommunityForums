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

namespace DotNetNuke.Modules.ActiveForumsTests.Services.TOkens
{
    using DotNetNuke.Modules.ActiveForums;
    using DotNetNuke.Modules.ActiveForums.Controllers;
    using Moq;
    using NUnit.Framework;

    using System.Text;

    [TestFixture]
    public class TokenReplacerTests
    {
        [Test]
        public void ReplaceTopicTokensTest()
        {
            // Arrange
            var requestUri = new Uri("https://localhost/forums");
            var rawUrl = "/forums";

            var mockTopic = new Mock<Modules.ActiveForums.Entities.TopicInfo>();
            mockTopic.Object.ForumId = 1;
            mockTopic.Object.Forum = new Modules.ActiveForums.Entities.ForumInfo();
            mockTopic.Object.Forum.ForumName = "Test Forum";
            mockTopic.Object.Forum.Security = PermissionController.GetEmptyPermissions(-1);
            mockTopic.Object.Forum.ForumGroup = new Modules.ActiveForums.Entities.ForumGroupInfo();
            mockTopic.Object.Forum.ForumGroup.GroupName = "Test Forum Group";
            mockTopic.Object.Content = new Modules.ActiveForums.Entities.ContentInfo();
            mockTopic.Object.Content.Subject = "Test Topic";
            mockTopic.Object.Content.Body = "Test Topic";

            var mockForum = new Mock<Modules.ActiveForums.Entities.ForumInfo>();
            mockForum.Object.ForumID = 1;
            mockForum.Object.ForumName = "Test Forum";
            mockForum.Object.ForumGroup = new Modules.ActiveForums.Entities.ForumGroupInfo();
            mockForum.Object.ForumGroup.GroupName = "Test Forum Group";

            var mockUser = new Mock<Modules.ActiveForums.Entities.ForumUserInfo>();
            mockUser.Object.UserId = 1;
            mockUser.Object.UserInfo = new DotNetNuke.Entities.Users.UserInfo();
            mockUser.Object.UserInfo.DisplayName = "Test User";
            mockUser.Object.UserInfo.Profile = new DotNetNuke.Entities.Users.UserProfile();
            mockUser.Object.UserRoles = Globals.DefaultAnonRoles + "|-1;||";
            mockUser.Object.UserInfo.Profile.PreferredLocale = "en-US";

            var navigationManager = new Modules.ActiveForums.Services.Tests.NavigationManager(null); // new Services.URLNavigator().NavigationManager();

            var templateStringBuilder = new StringBuilder("blah blah [SPLITBUTTONS1] blah [SPLITBUTTONS2] [TOPICSUBJECT] blah");

            var expectedResult = "blah blah [SPLITBUTTONS1] blah  [TOPICSUBJECT] blah";

            // Act

            var actualResult = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.ReplaceTopicTokens(templateStringBuilder, mockTopic.Object, null, null, navigationManager, mockUser.Object, requestUri, rawUrl).ToString();

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void ReplaceForumTokensTest()
        {
            // Arrange
            var requestUri = new Uri("https://localhost/forums");
            var rawUrl = "/forums";

            var mockForum = new Mock<Modules.ActiveForums.Entities.ForumInfo>();
            mockForum.Object.ForumID = 1;
            mockForum.Object.ForumName = "Test Forum";
            mockForum.Object.Security = PermissionController.GetEmptyPermissions(-1);
            mockForum.Object.ForumGroup = new Modules.ActiveForums.Entities.ForumGroupInfo();
            mockForum.Object.ForumGroup.GroupName = "Test Forum Group";

            var mockUser = new Mock<Modules.ActiveForums.Entities.ForumUserInfo>();
            mockUser.Object.UserId = 1;
            mockUser.Object.UserInfo = new DotNetNuke.Entities.Users.UserInfo();
            mockUser.Object.UserInfo.DisplayName = "Test User";
            mockUser.Object.UserInfo.Profile = new DotNetNuke.Entities.Users.UserProfile();
            mockUser.Object.UserRoles = Globals.DefaultAnonRoles + "|-1;||";
            mockUser.Object.UserInfo.Profile.PreferredLocale = "en-US";

            var navigationManager = new Modules.ActiveForums.Services.Tests.NavigationManager(null); // new Services.URLNavigator().NavigationManager();

            var templateStringBuilder = new StringBuilder("blah blah [GROUPNAME] blah blah");
            var expectedResult = "blah blah Test Forum Group blah blah";

            // Act
            var actualResult = DotNetNuke.Modules.ActiveForums.Services.Tokens.TokenReplacer.ReplaceForumTokens(templateStringBuilder, mockForum.Object, null, null, navigationManager, mockUser.Object, 0, CurrentUserTypes.Auth, requestUri, rawUrl).ToString();

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
