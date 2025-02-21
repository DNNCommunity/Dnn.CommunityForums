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

namespace DotNetNuke.Modules.ActiveForumsTests
{
    using System;
    using System.Globalization;

    using DotNetNuke.Modules.ActiveForums;
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
        [TestCase(200, 25, true, false, ExpectedResult = true)] // interval is 200, anonymous, last post is 25, expect true
        [TestCase(200, 25, false, false, ExpectedResult = false)] // interval is 200, not anonymous, last post is 25, expect false
        public bool HasFloodIntervalPassedTest1(int floodInterval, object secondsSinceLastPost, bool isAnonymous, bool isSuperUser)
        {
            //Arrange
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
                }
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

            // Act
            bool actualResult = DotNetNuke.Modules.ActiveForums.Utilities.HasFloodIntervalPassed(floodInterval, mockUser.Object, mockForum.Object);

            // Assert
            return actualResult;
        }

        [Test]
        [Obsolete("Deprecated in Community Forums. Removed in 09.00.00. Use HttpUtility.HtmlEncode.")]
        public void HtmlEncodeTestEmptyTag()
        {
            // Arrange
            // Act
            string actualResult = Utilities.HtmlEncode(string.Empty);

            // Assert
            Assert.That(actualResult, Is.Empty);
        }

        [Test]
        [Obsolete("Deprecated in Community Forums. Removed in 09.00.00. Use System.Net.WebUtility.HtmlEncode.")]
        public void HtmlEncodeTest()
        {
            // Arrange
            string tag = "<p>";
            string expectedResult = "&lt;p&gt;";

            // Act
            string actualResult = Utilities.HtmlEncode(tag);

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        [Obsolete("Deprecated in Community Forums. Removed in 09.00.00. Use System.Net.WebUtility.HtmlDecode.")]
        public void HtmlDecodeTestEmptyTag()
        {
            // Arrange
            // Act
            string actualResult = Utilities.HtmlDecode(string.Empty);

            // Assert
            Assert.That(actualResult, Is.Empty);
        }

        [Test]
        [Obsolete("Deprecated in Community Forums. Removed in 09.00.00. Use System.Net.WebUtility.HtmlDecode.")]
        public void HtmlDecodeTest()
        {
            // Arrange
            string tag = "&lt;p&gt;";
            string expectedResult = "<p>";

            // Act
            string actualResult = Utilities.HtmlDecode(tag);

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
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
    }
}
