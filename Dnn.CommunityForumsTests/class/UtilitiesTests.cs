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

using System.Collections;

namespace DotNetNuke.Modules.ActiveForumsTests
{
    using System;
    using System.Globalization;

    using DotNetNuke.Modules.ActiveForums;
    using DotNetNuke.Modules.ActiveForums.Entities;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public partial class UtilitiesTests : DotNetNuke.Modules.ActiveForumsTests.TestBase
    {
        [Test]
        [TestCase(-1, 0)]
        [TestCase(5, 4)]
        public void IsTrustedTest(int userTrustLevel, int userPostCount)
        {
            // Arrange

            // Act
            bool isTrusted = Utilities.IsTrusted(0, userTrustLevel, false, 0, userPostCount);

            // Assert
            Assert.That(isTrusted, Is.False);
        }

        [Test]
        public void NullDateTest()
        {
            // Arrange
            DateTime expectedResult = DateTime.Parse("1/1/1900", new CultureInfo("en-US", false).DateTimeFormat).ToUniversalTime();

            // Act
            var actualResult = Utilities.NullDate();

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        [TestCase("  this is a : messy string for a +url = 0 -", "this-is-a-messy-string-for-a-url-0")]
        public void CleanStringForUrlTest(string input, string expectedResult)
        {
            // Arrange
            // Act
            string actualResult = Utilities.CleanStringForUrl(input);

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        [TestCase(0, 0, false, false, ExpectedResult = true)] // flood interval disables
        [TestCase(20, 25, false, false, ExpectedResult = true)]
        [TestCase(200, 25, false, true, ExpectedResult = true)] // user is an admin
        [TestCase(200, null, false, false, ExpectedResult = true)] // user is an admin
        [TestCase(200, 25, true, false, ExpectedResult = true)] // interval is 200, anonymous, last post is 25, expect true (anonymous users will require captcha)
        [TestCase(200, 25, false, false, ExpectedResult = false)] // interval is 200, not anonymous, last post is 25, expect false
        [TestCase(20, 25, false, false, ExpectedResult = true)] // interval is 20, not anonymous, last post is 25, expect true
        public bool HasFloodIntervalPassedTest1(int floodInterval, object secondsSinceLastPost, bool isAnonymous, bool isSuperUser)
        {
            //Arrange
            var featureSettings = new System.Collections.Hashtable
            {
                { ForumSettingKeys.DefaultTrustLevel, TrustTypes.NotTrusted },
            };
            var mockPermissions = new Mock<DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo>();
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
                    UserID = isAnonymous ? DotNetNuke.Common.Utilities.Null.NullInteger : DotNetNuke.Tests.Utilities.Constants.UserID_User12,
                    IsSuperUser = isSuperUser,
                    Profile = new DotNetNuke.Entities.Users.UserProfile()
                    {
                        PreferredLocale = "en-US",
                    },
                },
            };
            var mockUser = new Mock<DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo>(DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings(), mockUserInfo.Object)
            {
                Object =
                {
                    PortalId = DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings().PortalId,
                    UserId = mockUserInfo.Object.UserID,
                    UserInfo = mockUserInfo.Object,
                },
            };

            if (secondsSinceLastPost != null)
            {
                mockUser.Object.DateLastPost = DateTime.UtcNow.AddSeconds(-1 * (int)secondsSinceLastPost);
                mockUser.Object.DateLastReply = DateTime.UtcNow.AddSeconds(-1 * (int)secondsSinceLastPost);
            }

            // Act
            bool actualResult = DotNetNuke.Modules.ActiveForums.Utilities.HasFloodIntervalPassed(floodInterval, mockUser.Object, mockForum.Object);

            // Assert
            return actualResult;
        }

        [Test]
        [TestCase(0, 0, false, false, ExpectedResult = true)] // edit interval disabled
        [TestCase(20, 25, false, false, ExpectedResult = true)]
        [TestCase(200, 25, false, true, ExpectedResult = true)] // user is a superuser; expect true
        [TestCase(20, 25, true, false, ExpectedResult = true)] // interval is 20, anonymous, post created 25 minutes ago, expect true
        [TestCase(30, 25, false, false, ExpectedResult = false)] // interval is 30, not anonymous, last post created 25 minutes ago, expect false
        public bool HasEditIntervalPassedTest(int editInterval, int minutesSincePostCreated, bool isAnonymous, bool isSuperUser)
        {

            // Arrange
            var featureSettings = new System.Collections.Hashtable
            {
                { ForumSettingKeys.DefaultTrustLevel, TrustTypes.NotTrusted },
            };
            var mockPermissions = new Mock<DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo>();
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
                        Body = "Test Topic",
                        DateCreated = DateTime.UtcNow.AddMinutes(-1 * minutesSincePostCreated),
                    },
                },
            };

            var mockUserInfo = new Mock<DotNetNuke.Entities.Users.UserInfo>
            {
                Object =
                {
                    PortalID = DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings().PortalId,
                    UserID = isAnonymous ? -1 : DotNetNuke.Tests.Utilities.Constants.UserID_User12,
                    IsSuperUser = isSuperUser && !isAnonymous,
                },
            }; var mockUser = new Mock<DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo>(DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings(), mockUserInfo.Object)
            {
                Object =
                {
                    PortalId = DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings().PortalId,
                    UserId = mockUserInfo.Object.UserID,
                    UserInfo = mockUserInfo.Object,
                },
            };

            // Act
            bool actualResult = DotNetNuke.Modules.ActiveForums.Utilities.HasEditIntervalPassed(editInterval, mockUser.Object, mockForum.Object, mockTopic.Object);

            // Assert
            return actualResult;
        }

        [Test]
        [TestCase("", ExpectedResult = false)]
        [TestCase("<p>test<p>", ExpectedResult = true)]
        [TestCase("<p>test</p>", ExpectedResult = true)]
        [TestCase(" test ", ExpectedResult = false)]
        public bool HasHTMLTest(string value)
        {
            // Arrange
            // Act
            return Utilities.HasHTML(value);

            // Assert
        }

        [Test]
        public void CheckSqlStringTest()
        {
            // Arrange
            var input = "SELECT * FROM TABLE1 UNION SELECT * FROM TABLE2";
            var expectedResult = "SELECT * 1  SELECT * 2";

            // Act
            var actualResult = DotNetNuke.Modules.ActiveForums.Utilities.Text.CheckSqlString(input);

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        [TestCase(0, ExpectedResult = true)]
        [TestCase("abc", ExpectedResult = false)]
        [TestCase(false, ExpectedResult = true)]
        [TestCase(true, ExpectedResult = true)]
        [TestCase(null, ExpectedResult = false)]
        [TestCase(-10, ExpectedResult = true)]
        [TestCase(01E1, ExpectedResult = true)]
        [TestCase("0", ExpectedResult = true)]
        public bool IsNumericTest(object obj)
        {
            // Arrange

            var expectedResult = true;

            // Act
            var actualResult = DotNetNuke.Modules.ActiveForums.Utilities.IsNumeric(obj);

            // Assert
            return actualResult;
        }

        [Test]
        public void EncodeBrackets_WithEmptyString_ReturnsEmptyString()
        {
            // Arrange
            var input = string.Empty;

            // Act
            var result = Utilities.EncodeBrackets(input);

            // Assert
            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        public void EncodeBrackets_WithSquareBrackets_ReturnsEncodedBrackets()
        {
            // Arrange
            var input = "This is a [test] string";

            // Act
            var result = Utilities.EncodeBrackets(input);

            // Assert
            Assert.That(result, Is.EqualTo("This is a &#91;test&#93; string"));
        }

        [Test]
        public void EncodeBrackets_WithCurlyBraces_ReturnsEncodedBraces()
        {
            // Arrange
            var input = "This is a {test} string";

            // Act
            var result = Utilities.EncodeBrackets(input);

            // Assert
            Assert.That(result, Is.EqualTo("This is a &#123;test&#125; string"));
        }

        [Test]
        public void EncodeBrackets_WithMixedBrackets_ReturnsAllEncodedBrackets()
        {
            // Arrange
            var input = "Test with [square] and {curly} brackets";

            // Act
            var result = Utilities.EncodeBrackets(input);

            // Assert
            Assert.That(result, Is.EqualTo("Test with &#91;square&#93; and &#123;curly&#125; brackets"));
        }

        [Test]
        public void EncodeBrackets_WithMultipleBrackets_EncodesAllInstances()
        {
            // Arrange
            var input = "[test][test2]{test3}{test4}";

            // Act
            var result = Utilities.EncodeBrackets(input);

            // Assert
            Assert.That(result, Is.EqualTo("&#91;test&#93;&#91;test2&#93;&#123;test3&#125;&#123;test4&#125;"));
        }

        [Test]
        public void EncodeBrackets_WithNoSpecialCharacters_ReturnsOriginalString()
        {
            // Arrange
            var input = "This is a normal string";

            // Act
            var result = Utilities.EncodeBrackets(input);

            // Assert
            Assert.That(result, Is.EqualTo("This is a normal string"));
        }

        [Test]
        public void EncodeBrackets_WithNull_ThrowsNullReferenceException()
        {
            // Arrange & Act & Assert
            Assert.Throws<System.NullReferenceException>(() => Utilities.EncodeBrackets(null));
        }

        [Test]
        public void DecodeBrackets_WithNoBrackets_ReturnsOriginalString()
        {
            // Arrange
            var input = "Hello World";

            // Act
            var result = Utilities.DecodeBrackets(input);

            // Assert
            Assert.That(result, Is.EqualTo(input));
        }

        [Test]
        public void DecodeBrackets_WithSquareBrackets_DecodesProperly()
        {
            // Arrange
            var input = "Hello &#91;World&#93;";
            var expected = "Hello [World]";

            // Act
            var result = Utilities.DecodeBrackets(input);

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void DecodeBrackets_WithCurlyBrackets_DecodesProperly()
        {
            // Arrange
            var input = "Hello &#123;World&#125;";
            var expected = "Hello {World}";

            // Act
            var result = Utilities.DecodeBrackets(input);

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void DecodeBrackets_WithMixedBrackets_DecodesProperly()
        {
            // Arrange
            var input = "&#91;Hello&#93; &#123;World&#125;";
            var expected = "[Hello] {World}";

            // Act
            var result = Utilities.DecodeBrackets(input);

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void DecodeBrackets_WithMultipleOccurrences_DecodesProperly()
        {
            // Arrange
            var input = "&#91;Test&#93; &#91;Test2&#93; &#123;Test3&#125; &#123;Test4&#125;";
            var expected = "[Test] [Test2] {Test3} {Test4}";

            // Act
            var result = Utilities.DecodeBrackets(input);

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void DecodeBrackets_WithEmptyString_ReturnsEmptyString()
        {
            // Arrange
            var input = string.Empty;

            // Act
            var result = Utilities.DecodeBrackets(input);

            // Assert
            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        public void DecodeBrackets_WithPartialEncodedBrackets_OnlyDecodesComplete()
        {
            // Arrange
            var input = "&#91Test&#93 &#123Test&#125";
            var expected = "&#91Test&#93 &#123Test&#125";

            // Act
            var result = Utilities.DecodeBrackets(input);

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }
    }
}
