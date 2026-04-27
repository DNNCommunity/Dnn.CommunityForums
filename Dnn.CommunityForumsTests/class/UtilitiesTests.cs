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
    using System.Collections;
    using System.Globalization;
    using System.Text.RegularExpressions;

    using DotNetNuke.Modules.ActiveForums;
    using DotNetNuke.Modules.ActiveForums.Entities;
    using DotNetNuke.Modules.ActiveForumsTests.ObjectGraphs;

    using NUnit.Framework;

    [TestFixture]
    public partial class UtilitiesTests : DotNetNuke.Modules.ActiveForumsTests.TestBase
    {
        private const string UrlPattern = @"http[s]?://([\w+?\.\w+])+([a-zA-Z0-9\~\!\@\\#\$\%\^\&amp;\*\(\)_\-\=\+\\\/\?\.\:\;\'\,]*)?";

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
            DateTime expectedResult = DateTime.Parse("1/1/1900", new System.Globalization.CultureInfo("en-US", false).DateTimeFormat).ToUniversalTime();

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
        [TestCase(0, 0, false, false, false, ExpectedResult = true, TestName = "HasFloodIntervalPassedTest_FloodIntervalDisabled")] // flood interval disables
        [TestCase(20, 25, false, false, false, ExpectedResult = true, TestName = "HasFloodIntervalPassedTest_FloodIntervalPassed_UntrustedUser")]
        [TestCase(200, 25, false, true, true, ExpectedResult = true, TestName = "HasFloodIntervalPassedTest_FloodIntervalPassed_AdminByPass")] // user is an admin
        [TestCase(200, null, false, false, true, ExpectedResult = true, TestName = "HasFloodIntervalPassedTest_FloodIntervalPassed_NoPreviousPostButAdminByPass")] // user is an admin
        [TestCase(200, 25, true, false, false, ExpectedResult = true, TestName = "HasFloodIntervalPassedTest_FloodIntervalNotPassed_ButByPassforAnonymousWillRequireCaptcha")] // interval is 200, anonymous, last post is 25, expect true (anonymous users will require captcha)
        [TestCase(200, 25, false, false, false, ExpectedResult = false, TestName = "HasFloodIntervalPassedTest_FloodIntervalNotPassed_UntrustedUser")] // interval is 200, not anonymous, last post is 25, expect false
        [TestCase(200, 25, false, false, true, ExpectedResult = true, TestName = "HasFloodIntervalPassedTest_FloodIntervalNotPassed_ButTrustedUser")] // interval is 200, not anonymous, last post is 25, expect true
        [TestCase(20, 25, false, false, true, ExpectedResult = true, TestName = "HasFloodIntervalPassedTest_FloodIntervalPassed_TrustedUser")] // interval is 20, not anonymous, trusted user, last post is 25, expect true
        [TestCase(20, 25, false, false, false, ExpectedResult = true, TestName = "HasFloodIntervalPassedTest_FloodIntervalPassed_UntrustedUser")] // interval is 20, not anonymous, untrusted, last post is 25, expect true
        public bool HasFloodIntervalPassedTest1(int floodInterval, object secondsSinceLastPost, bool isAnonymous, bool isSuperUser, bool isTrustedUser)
        {
            // Arrange
            var featureSettings = new Hashtable
            {
                { ForumSettingKeys.DefaultTrustLevel, TrustTypes.NotTrusted },
            };

            // Use a public forum from the graph and apply FeatureSettings
            var forum = this.ForumsGraph.Find(f => f.ForumID == ForumsObjectGraph.AnnouncementsForumId);
            forum.FeatureSettings = new FeatureSettings(featureSettings);

            // Resolve the forum user from the graph
            ForumUserInfo forumUser;
            if (isAnonymous)
            {
                forumUser = this.ForumUserGraph.Find(u => u.UserId == DotNetNuke.Tests.Utilities.Constants.USER_AnonymousUserId);
            }
            else if (isSuperUser)
            {
                forumUser = this.ForumUserGraph.Find(u => u.UserId == DotNetNuke.Tests.Utilities.Constants.UserID_Admin);
                forumUser.UserInfo.IsSuperUser = true;
            }
            else if (isTrustedUser)
            {
                forumUser = this.ForumUserGraph.Find(u => u.UserId == DotNetNuke.Tests.Utilities.Constants.UserID_User12);
            }
            else
            {
                forumUser = this.ForumUserGraph.Find(u => u.UserId == DotNetNuke.Tests.Utilities.Constants.USER_TenId);
            }

            if (secondsSinceLastPost != null)
            {
                forumUser.DateLastPost = DateTime.UtcNow.AddSeconds(-1 * (int)secondsSinceLastPost);
                forumUser.DateLastReply = DateTime.UtcNow.AddSeconds(-1 * (int)secondsSinceLastPost);
            }

            // Act
            bool actualResult = Utilities.HasFloodIntervalPassed(floodInterval, forumUser, forum);

            // Assert
            return actualResult;
        }

        [Test]
        [TestCase(0, 0, false, false, true, ExpectedResult = true)] // edit interval disabled
        [TestCase(20, 25, false, false, true, ExpectedResult = true)]
        [TestCase(200, 25, false, true, true, ExpectedResult = true)] // user is a superuser; expect true
        [TestCase(20, 25, true, false, false, ExpectedResult = true)] // interval is 20, anonymous, post created 25 minutes ago, expect true
        [TestCase(30, 25, false, false, false, ExpectedResult = false)] // interval is 30, not anonymous but untrusted user, last post created 25 minutes ago, expect false
        [TestCase(30, 25, false, false, true, ExpectedResult = true)] // interval is 30, not anonymous but trusted user, last post created 25 minutes ago, expect true
        public bool HasEditIntervalPassedTest(int editInterval, int minutesSincePostCreated, bool isAnonymous, bool isSuperUser, bool isTrustedUser)
        {
            // Arrange
            var featureSettings = new Hashtable
            {
                { ForumSettingKeys.DefaultTrustLevel, TrustTypes.NotTrusted },
            };

            // Use a public forum from the graph and apply FeatureSettings
            var forum = this.ForumsGraph.Find(f => f.ForumID == ForumsObjectGraph.AnnouncementsForumId);
            forum.FeatureSettings = new FeatureSettings(featureSettings);

            // Use the announcement topic from the graph; override DateCreated to match the test case
            var topic = this.TopicReplyGraph.Find(t => t.TopicId == TopicReplyObjectGraph.AnnouncementTopicId);
            topic.Content.DateCreated = DateTime.UtcNow.AddMinutes(-1 * minutesSincePostCreated);

            // Resolve the forum user from the graph
            ForumUserInfo forumUser;
            if (isAnonymous)
            {
                forumUser = this.ForumUserGraph.Find(u => u.UserId == DotNetNuke.Tests.Utilities.Constants.USER_AnonymousUserId);
            }
            else if (isSuperUser)
            {
                forumUser = this.ForumUserGraph.Find(u => u.UserId == DotNetNuke.Tests.Utilities.Constants.UserID_Admin);
                forumUser.UserInfo.IsSuperUser = true;
            }
            else if (isTrustedUser)
            {
                forumUser = this.ForumUserGraph.Find(u => u.UserId == DotNetNuke.Tests.Utilities.Constants.UserID_User12);
            }
            else
            {
                forumUser = this.ForumUserGraph.Find(u => u.UserId == DotNetNuke.Tests.Utilities.Constants.USER_TenId);
            }

            // Act
            bool actualResult = Utilities.HasEditIntervalPassed(editInterval, forumUser, forum, topic);

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
            var actualResult = Utilities.Text.CheckSqlString(input);

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
            // Act
            var actualResult = Utilities.IsNumeric(obj);

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

        [Test]
        public void GetSha256HashTest()
        {
            // Arrange
            var input = "webmaster@dnncommunity.org";
            var expectedResult = "db4c6321693845e820dfbebbd7df8e5c980f7aa3c9f4a884ad10ee57d0ba159f";

            // Act
            var actualResult = DotNetNuke.Modules.ActiveForums.Utilities.GetSha256Hash(input);

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void EncodeCodeBlocks_NullOrEmpty_ReturnsInput()
        {
            Assert.Multiple(() =>
            {
                Assert.That(Utilities.EncodeCodeBlocks(null), Is.Null);
                Assert.That(Utilities.EncodeCodeBlocks(string.Empty), Is.Empty);
            });
        }

        [Test]
        public void EncodeCodeBlocks_NoCodeTags_ReturnsInput()
        {
            var input = "This is a test without code tags.";
            var result = Utilities.EncodeCodeBlocks(input);
            Assert.That(result, Is.EqualTo(input));
        }

        [Test]
        public void EncodeCodeBlocks_WithCodeTags_EncodesBlock()
        {
            var input = "Some text [code]int x = 1;[/code] more text";
            var expectedEncoded = System.Net.WebUtility.HtmlEncode("[code]int x = 1;[/code]");
            var result = Utilities.EncodeCodeBlocks(input);
            Assert.That(result.Contains(expectedEncoded), Is.True);
        }

        [Test]
        public void EncodeCodeBlocks_WithAngleCodeTags_EncodesBlock()
        {
            var input = "Some text <code>int y = 2;</code> more text";
            var expectedEncoded = System.Net.WebUtility.HtmlEncode("<code>int y = 2;</code>");
            var result = Utilities.EncodeCodeBlocks(input);
            Assert.Multiple(() =>
            {
                Assert.That(result.Contains(expectedEncoded), Is.True);
                Assert.That(result.Contains("<code>int y = 2;</code>"), Is.False);
            });
        }

        [Test]
        public void EncodeCodeBlocks_MultipleCodeBlocks_EncodesAll()
        {
            var input = "[code]a[/code] and <code>b</code>";
            var expected1 = System.Net.WebUtility.HtmlEncode("[code]a[/code]");
            var expected2 = System.Net.WebUtility.HtmlEncode("<code>b</code>");
            var result = Utilities.EncodeCodeBlocks(input);
            Assert.Multiple(() =>
            {
                Assert.That(result.Contains(expected1), Is.True);
                Assert.That(result.Contains(expected2), Is.True);
            });
        }

        [Test]
        public void RemoveScriptTags_NullInput_ReturnsNull()
        {
            // Arrange
            string input = null;

            // Act
            string result = Utilities.RemoveScriptTags(input);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void RemoveScriptTags_EmptyInput_ReturnsEmptyString()
        {
            // Arrange
            string input = string.Empty;

            // Act
            string result = Utilities.RemoveScriptTags(input);

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void RemoveScriptTags_NoScriptTags_ReturnsSameText()
        {
            // Arrange
            string input = "This is regular text with <p>some HTML</p> but no script tags.";

            // Act
            string result = Utilities.RemoveScriptTags(input);

            // Assert
            Assert.That(result, Is.EqualTo(input));
        }

        [Test]
        public void RemoveScriptTags_WithScriptTags_RemovesScriptTags()
        {
            // Arrange
            string input = "Text before <script>alert('test');</script> text after";
            string expected = "Text before  text after";

            // Act
            string result = Utilities.RemoveScriptTags(input);

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void RemoveScriptTags_MultipleScriptTags_RemovesAllScriptTags()
        {
            // Arrange
            string input = "<script>script1</script>middle<script>script2</script>";
            string expected = "middle";

            // Act
            string result = Utilities.RemoveScriptTags(input);

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void RemoveScriptTags_ScriptTagsWithAttributes_RemovesEntireTag()
        {
            // Arrange
            string input = "<script type=\"text/javascript\" src=\"test.js\">console.log('test');</script>";
            string expected = "";

            // Act
            string result = Utilities.RemoveScriptTags(input);

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void RemoveScriptTags_ScriptInsideCodeBlock_PreservesScript()
        {
            // Arrange
            string input = "Before <code><script>alert('test');</script></code> After";
            string expected = input;

            // Act
            string result = Utilities.RemoveScriptTags(input);

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void RemoveScriptTags_ScriptInsidePreBlock_PreservesScript()
        {
            // Arrange
            string input = "Before <pre><script>alert('test');</script></pre> After";
            string expected = input;

            // Act
            string result = Utilities.RemoveScriptTags(input);

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void RemoveScriptTags_EncodedScriptTags_RemovesEncodedTags()
        {
            // Arrange
            string input = "Before &lt;script&gt;alert('test');&lt;/script&gt; After";
            string expected = "Before  After";

            // Act
            string result = Utilities.RemoveScriptTags(input);

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void RemoveScriptTags_MixedRegularAndEncodedTags_RemovesBoth()
        {
            // Arrange
            string input = "<script>alert(1);</script> normal &lt;script&gt;alert(2);&lt;/script&gt;";
            string expected = " normal ";

            // Act
            string result = Utilities.RemoveScriptTags(input);

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        [TestCase("https://localhost/en-us/portals/0/images/logo.png", ExpectedResult = "https://localhost/portals/0/images/logo.png")]
        [TestCase("https://localhost/portals/0/images/logo.png", ExpectedResult = "https://localhost/portals/0/images/logo.png")]
        public string RemoveCultureFromUrl(string url)
        {
            // Arrange

            // Act
            return Utilities.RemoveCultureFromUrl(DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings(), url);

            // Assert
        }

        [Test]
        public void ResolveUrl_DefaultPortalAliasThrowsException()
        {
            // Arrange
            var expectedPortalId = DotNetNuke.Tests.Utilities.Constants.PORTAL_Zero;
            var portalSettings = DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings();

            // Act

            // Assert
            Assert.That(portalSettings.PortalId, Is.EqualTo(expectedPortalId));
            Assert.Throws<ArgumentNullException>(() => { var x = portalSettings.DefaultPortalAlias; });
        }

        [Test]
        public void ResolveUrl_WithRelativePathAndSecure_ReturnsHttpsUrl()
        {
            // Arrange
            string link = "/forums/topic/abc";
            string portalAlias = "dnncommunity.org";

            // Act
            string result = Utilities.ResolveUrl(url: link, defaultPortalAlias: portalAlias, sslEnabled: true);

            // Assert
            Assert.That(result, Is.EqualTo("https://dnncommunity.org/forums/topic/abc"));
        }

        [Test]
        public void ResolveUrl_WithAbsoluteHttpsUrlAndSecure_ReturnsUnchanged()
        {
            // Arrange
            string link = "https://dnncommunity.org/forums/topic/abc";

            // Act
            string result = Utilities.ResolveUrl(url: link, defaultPortalAlias: this.DefaultPortalAlias, sslEnabled: true);

            // Assert
            Assert.That(result, Is.EqualTo("https://example.com/en-us/forums/topic/abc"));
        }

        [Test]
        public void ResolveUrl_WithAliasWithoutSubpath_ReturnsOriginalUrl()
        {
            // Arrange
            var url = "/portals/0/images/logo.png";
            var expectedResult = $"https://{this.DefaultPortalAlias}{url}";

            // Act
            var result = Utilities.ResolveUrl(url: url, defaultPortalAlias: this.DefaultPortalAlias, sslEnabled: true);

            // Assert
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        [Test]
        public void ResolveUrlInTag_ReplacesHrefAndSrcWithResolvedUrls()
        {
            // Arrange
            var path = "/en-us/portals/0/images/logo.png";
            var template = $"<a href=\"{path}\">link</a><img src=\"{path}\" />";
            var expectedResult = $"<a href=\"https://{this.DefaultPortalAlias}{path}\">link</a><img src=\"https://{this.DefaultPortalAlias}{path}\" />".Replace("/en-us/en-us/", "/en-us/");
            // Act
            var result = DotNetNuke.Modules.ActiveForums.Utilities.ResolveUrlInTag(template: template, defaultPortalAlias: this.DefaultPortalAlias, sslEnabled: true);

            // Assert
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        [Test]
        public void ResolveUrl_WithAliasContainingSubpath_PrependsDomain()
        {
            // Arrange
            var portalSettings = DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings();
            var originalAlias = "example.com";
            var url = "/en-us/portals/0/images/logo.png";

            // Act
            var result = Utilities.ResolveUrl(url: url, defaultPortalAlias: this.DefaultPortalAlias, sslEnabled: false);

            // Assert
            var expectedResult = DotNetNuke.Common.Globals.AddHTTP(originalAlias) + url;
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        [Test]
        [TestCase("/forums/topic/abc", "https://example.com/en-us/forums/topic/abc", ExpectedResult = true)]
        public bool ResolveUrl_WithRelativePathAndSecure_ReturnsHttpsUrl(string link, string expected)
        {
            // Arrange & Act
            string result = Utilities.ResolveUrl(url: link, defaultPortalAlias: this.DefaultPortalAlias, sslEnabled: true);

            // Assert
            return result == expected;
        }

        [Test]
        [TestCase("https://dnncommunity.org/forums/topic/abc", "https://example.com/en-us/forums/topic/abc", ExpectedResult = true)]
        public bool ResolveUrl_WithAbsoluteHttpsUrlAndSecure_ReturnsUnchanged(string link, string expected)
        {
            // Arrange & Act
            string result = Utilities.ResolveUrl(url: link, defaultPortalAlias: this.DefaultPortalAlias, sslEnabled: true);

            // Assert
            return result == expected;
        }

        [Test]
        public void EscapeJavaScriptSingleQuotedString_EscapesSingleQuotesBackslashesAndNewLines()
        {
            // Arrange
            const string input = "L'utilisateur\\test\r\nnext\tvalue</script>\u2028\u2029";
            const string expected = "L\\'utilisateur\\\\test\\r\\nnext\\tvalue<\\/script>\\u2028\\u2029";

            // Act
            var result = Utilities.EscapeJavaScriptSingleQuotedString(input);

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void IsInsideScriptBlock_ReturnsTrueOnlyForScriptContent()
        {
            // Arrange
            const string content = "<div>'[RESX:Label]'</div><script>var msg='[RESX:Label]';</script>";
            int htmlIndex = content.IndexOf("[RESX:Label]", StringComparison.Ordinal);
            int scriptIndex = content.LastIndexOf("[RESX:Label]", StringComparison.Ordinal);

            // Act
            bool isHtmlScriptContent = Utilities.IsInsideScriptBlock(content, htmlIndex);
            bool isScriptScriptContent = Utilities.IsInsideScriptBlock(content, scriptIndex);

            // Assert
            Assert.That(isHtmlScriptContent, Is.False);
            Assert.That(isScriptScriptContent, Is.True);
        }

        [Test]
        public void IsInsideJavaScriptSingleQuotedString_ReturnsTrueOnlyForSingleQuotedScriptString()
        {
            // Arrange
            const string content = "<div title='[RESX:Label]'></div><script>var a='[RESX:Label]';var b=\"[RESX:Label]\";</script>";
            int htmlIndex = content.IndexOf("[RESX:Label]", StringComparison.Ordinal);
            int singleQuoteScriptIndex = content.IndexOf("[RESX:Label]", htmlIndex + 1, StringComparison.Ordinal);
            int doubleQuoteScriptIndex = content.LastIndexOf("[RESX:Label]", StringComparison.Ordinal);

            // Act
            bool isHtmlSingleQuoted = Utilities.IsInsideJavaScriptSingleQuotedString(content, htmlIndex);
            bool isSingleQuotedScript = Utilities.IsInsideJavaScriptSingleQuotedString(content, singleQuoteScriptIndex);
            bool isDoubleQuotedScript = Utilities.IsInsideJavaScriptSingleQuotedString(content, doubleQuoteScriptIndex);

            // Assert
            Assert.That(isHtmlSingleQuoted, Is.False);
            Assert.That(isSingleQuotedScript, Is.True);
            Assert.That(isDoubleQuotedScript, Is.False);
        }

        [Test]
        [TestCase("Check out this image: http://example.com/image.jpg", ExpectedResult = true)]
        public bool AutoLinks_WithImageUrl_ShouldNotCreateLink(string text)
        {
            // Arrange & Act
            var result = Utilities.AutoLinks(text, this.DefaultSite);

            // Assert
            return !result.Contains("<a href=") && result.Contains("http://example.com/image.jpg");
        }

        [Test]
        [TestCase("jpg", ExpectedResult = false)]
        [TestCase("gif", ExpectedResult = false)]
        [TestCase("png", ExpectedResult = false)]
        [TestCase("jpeg", ExpectedResult = false)]
        [TestCase("pdf", ExpectedResult = true)]
        [TestCase("docx", ExpectedResult = true)]
        public bool AutoLinks_ShouldCreateLinkBasedOnExtension(string extension)
        {
            // Arrange
            var text = $"URL: https://example.com/en-us/file.{extension}";

            // Act
            var result = Utilities.AutoLinks(text, this.DefaultSite);

            // Assert
            return result.Contains("<a href=");
        }

        [Test]
        [TestCase("Visit http://external.com for more info", ExpectedResult = true)]
        public bool AutoLinks_WithExternalUrl_ShouldCreateExternalLink(string text)
        {
            // Arrange & Act
            var result = Utilities.AutoLinks(text, this.DefaultSite);

            // Assert
            return result.Contains("target=\"_blank\"") && result.Contains("rel=\"nofollow\"") && result.Contains("<a href=\"http://external.com/\"");
        }

        [Test]
        [TestCase("Visit https://example.com/en-us/page for more info", ExpectedResult = true)]
        public bool AutoLinks_WithInternalUrl_ShouldCreateInternalLink(string text)
        {
            // Arrange & Act
            var result = Utilities.AutoLinks(text, this.DefaultSite);

            // Assert
            return !result.Contains("target=\"_blank\"") && !result.Contains("rel=\"nofollow\"") && result.Contains("<a href=\"https://example.com/en-us/page\"");
        }

        [Test]
        [TestCase("http://external.com/this-is-a-very-long-url-that-exceeds-the-maximum-length-allowed-for-display", ExpectedResult = true)]
        public bool AutoLinks_WithLongUrl_ShouldTruncateLinkText(string longUrl)
        {
            // Arrange
            var text = $"Check this out: {longUrl}";

            // Act
            var result = Utilities.AutoLinks(text, this.DefaultSite);

            // Assert
            return result.Contains("...") && result.Contains($"href=\"{longUrl}\"");
        }

        [Test]
        [TestCase("<a href=\"http://example.com\">Link</a>", ExpectedResult = true)]
        public bool AutoLinks_WithExistingHrefAttribute_ShouldNotModify(string text)
        {
            // Arrange & Act
            var result = Utilities.AutoLinks(text, this.DefaultSite);

            // Assert
            return result == text;
        }

        [Test]
        [TestCase("<img src=\"http://example.com/image.png\" />", ExpectedResult = true)]
        public bool AutoLinks_WithExistingSrcAttribute_ShouldNotModify(string text)
        {
            // Arrange & Act
            var result = Utilities.AutoLinks(text, this.DefaultSite);

            // Assert
            return result.Contains("src=\"http://example.com/image.png\"") && !result.Contains("<a href=");
        }

        [Test]
        [TestCase("value=http://example.com/test", ExpectedResult = true)]
        public bool AutoLinks_WithEqualsSignBeforeUrl_ShouldNotCreateLink(string text)
        {
            // Arrange & Act
            var result = Utilities.AutoLinks(text, this.DefaultSite);

            // Assert
            return !result.Contains("<a href=");
        }

        [Test]
        [TestCase("Visit https://secure.external.com", ExpectedResult = true)]
        public bool AutoLinks_WithHttpsUrl_ShouldCreateLink(string text)
        {
            // Arrange & Act
            var result = Utilities.AutoLinks(text, this.DefaultSite);

            // Assert
            return result.Contains("target=\"_blank\"") && result.Contains("rel=\"nofollow\"") && result.Contains("<a href=\"https://secure.external.com/\"");
        }

        [Test]
        [TestCase("Visit http://external1.com and http://external2.com", ExpectedResult = true)]
        public bool AutoLinks_WithMultipleUrls_ShouldCreateMultipleLinks(string text)
        {
            // Arrange & Act
            var result = Utilities.AutoLinks(text, this.DefaultSite);

            // Assert
            return result.Contains("<a href=\"http://external1.com/\"") && result.Contains("<a href=\"http://external2.com/\"");
        }

        [Test]
        [TestCase("Visit https://example.com/en-us/internal and https://external.com", ExpectedResult = true)]
        public bool AutoLinks_WithMixedInternalAndExternalUrls_ShouldHandleBothCorrectly(string text)
        {
            // Arrange & Act
            var result = Utilities.AutoLinks(text, this.DefaultSite);

            // Assert
            return result.Contains("<a href=\"https://example.com/en-us/internal\">https://example.com/en-us/internal</a>")
                && result.Contains("<a href=\"https://external.com/\" target=\"_blank\" rel=\"nofollow\">https://external.com</a>");
        }

        [Test]
        [TestCase("&lt;a href=\"https://example.com\"&gt;Link&lt;/a&gt;", ExpectedResult = true)]
        public bool AutoLinks_WithEncodedHref_ShouldDecodeAndProcess(string text)
        {
            // Arrange & Act
            var result = Utilities.AutoLinks(text, this.DefaultSite);

            // Assert
            return result.Contains("https://example.com");
        }

        [Test]
        [TestCase(null, ExpectedResult = true)]
        public bool AutoLinks_WithNullText_ShouldReturnEmptyString(string text)
        {
            // Arrange & Act
            var result = Utilities.AutoLinks(text, this.DefaultSite);

            // Assert
            return string.IsNullOrEmpty(result);
        }

        [Test]
        [TestCase("", ExpectedResult = true)]
        public bool AutoLinks_WithEmptyText_ShouldReturnEmptyString(string text)
        {
            // Arrange & Act
            var result = Utilities.AutoLinks(text, this.DefaultSite);

            // Assert
            return result == string.Empty;
        }

        [Test]
        [TestCase("Visit https://external.com/page?id=123&name=test", ExpectedResult = true)]
        public bool AutoLinks_WithUrlContainingQueryParameters_ShouldCreateLink(string text)
        {
            // Arrange & Act
            var result = Utilities.AutoLinks(text, this.DefaultSite);

            // Assert
            return result.Contains("<a href=\"https://external.com/page?id=123&name=test\"");
        }

        [Test]
        [TestCase("Visit https://external.com/page#section", ExpectedResult = true)]
        public bool AutoLinks_WithUrlContainingHash_ShouldCreateLink(string text)
        {
            // Arrange & Act
            var result = Utilities.AutoLinks(text, this.DefaultSite);

            // Assert
            return result.Contains("<a href=\"https://external.com/page#section\"");
        }

        [Test]
        [TestCase("Visit https://EXAMPLE.COM/en-us/page", ExpectedResult = true)]
        public bool AutoLinks_WithCaseInsensitiveSiteMatch_ShouldTreatAsInternal(string text)
        {
            // Arrange & Act
            var result = Utilities.AutoLinks(text, this.DefaultSite);

            // Assert
            return !result.Contains("target=\"_blank\"") && !result.Contains("rel=\"nofollow\"");
        }

        [Test]
        [TestCase(
            "<a href=\"https://example.com/en-us/Activity-Feed?userId=19\"> @Timo Breumelhof Host (SU)</a>",
            "<a href=\"https://example.com/en-us/Activity-Feed?userId=19\"> @Timo Breumelhof Host (SU)</a>",
            ExpectedResult = true)]
        [TestCase(
            "<a href=\"https://example.com/en-us/Activity-Feed?userId=19\"><img class=\"af-avatar\" loading=\"lazy\" src=\"https://example.com/en-us/DnnImageHandler.ashx?mode=profilepic&userId=19&h=20&w=20\" /> @Timo Breumelhof Host (SU)</a>",
            "<a href=\"https://example.com/en-us/Activity-Feed?userId=19\"><img class=\"af-avatar\" loading=\"lazy\" src=\"https://example.com/en-us/DnnImageHandler.ashx?mode=profilepic&userId=19&h=20&w=20\" /> @Timo Breumelhof Host (SU)</a>",
            ExpectedResult = true)]
        [TestCase(
            "<a href=\"https://example.com/en-us/Activity-Feed?userId=19\">@Timo Breumelhof Host (SU)</a> is the best! just check out https://dnncommunity.org.",
            "<a href=\"https://example.com/en-us/Activity-Feed?userId=19\">@Timo Breumelhof Host (SU)</a> is the best! just check out <a href=\"https://dnncommunity.org/\" target=\"_blank\" rel=\"nofollow\">https://dnncommunity.org</a>.",
            ExpectedResult = true)]
        [TestCase(
            "<a href=\"https://example.com/en-us/Activity-Feed?userId=19\">@Timo Breumelhof Host (SU)</a> is the best! just check out <a href=\"https://dnncommunity.org/\" target=\"_blank\" rel=\"nofollow\">https://dnncommunity.org/</a>",
            "<a href=\"https://example.com/en-us/Activity-Feed?userId=19\">@Timo Breumelhof Host (SU)</a> is the best! just check out <a href=\"https://dnncommunity.org/\" target=\"_blank\" rel=\"nofollow\">https://dnncommunity.org/</a>",
            ExpectedResult = true)]
        [TestCase(
            "<a href=\"https://example.com/en-us/Activity-Feed?userId=19\"><img class=\"af-avatar\" loading=\"lazy\" src=\"https://example.com/en-us/DnnImageHandler.ashx?mode=profilepic&userId=19&h=20&w=20\" /> @Timo Breumelhof Host (SU)</a> is the best! just check out https://dnncommunity.org.",
            "<a href=\"https://example.com/en-us/Activity-Feed?userId=19\"><img class=\"af-avatar\" loading=\"lazy\" src=\"https://example.com/en-us/DnnImageHandler.ashx?mode=profilepic&userId=19&h=20&w=20\" /> @Timo Breumelhof Host (SU)</a> is the best! just check out <a href=\"https://dnncommunity.org/\" target=\"_blank\" rel=\"nofollow\">https://dnncommunity.org</a>.",
            ExpectedResult = true)]
        [TestCase(
            "<p><a href=\"https://example.com/en-us/Activity-Feed?userId=19\"><img class=\"af-avatar\" loading=\"lazy\" src=\"https://example.com/en-us/DnnImageHandler.ashx?mode=profilepic&userId=19&h=20&w=20\" /> @Timo Breumelhof Host (SU)</a> is the best! just check out <a href=\"https://dnncommunity.org/\" target=\"_blank\" rel=\"nofollow\">https://dnncommunity.org</a></p>",
            "<p><a href=\"https://example.com/en-us/Activity-Feed?userId=19\"><img class=\"af-avatar\" loading=\"lazy\" src=\"https://example.com/en-us/DnnImageHandler.ashx?mode=profilepic&userId=19&h=20&w=20\" /> @Timo Breumelhof Host (SU)</a> is the best! just check out <a href=\"https://dnncommunity.org/\" target=\"_blank\" rel=\"nofollow\">https://dnncommunity.org/</a></p>",
            ExpectedResult = true)]
        public bool AutoLinks_ProcessesCorrectly_WhenEmbeddedUrlsInsideAnchorTag(string text, string expected)
        {
            // Arrange & Act
            var result = Utilities.AutoLinks(text, this.DefaultSite);

            // Assert
            return result == expected;
        }

        [Test]
        public void AutoLinks_ProcessesCorrectly_WhenEmbeddedUrlsInsideAnchorTag2()
        {
            // Arrange
            var text = "<a href=\"https://example.com/en-us/Activity-Feed?userId=19\">@Timo Breumelhof Host (SU)</a> is the best! just check out https://dnncommunity.org.";
            var expected = "<a href=\"https://example.com/en-us/Activity-Feed?userId=19\">@Timo Breumelhof Host (SU)</a> is the best! just check out <a href=\"https://dnncommunity.org/\" target=\"_blank\" rel=\"nofollow\">https://dnncommunity.org</a>.";

            // Act
            var result = Utilities.AutoLinks(text, this.DefaultSite);

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        [TestCase("http://example.com/image.jpg", "http://example.com/image.jpg", ExpectedResult = true)]
        public bool ReplaceLink_ReturnsImageUrl_Unchanged(string text, string expected)
        {
            // Arrange & Act
            var match = System.Text.RegularExpressions.Regex.Match(text, UrlPattern, RegexOptions.IgnoreCase);
            var result = Utilities.ReplaceLink(match, this.DefaultSite, text);

            // Assert
            return result == expected;
        }

        [Test]
        [TestCase("jpg", ExpectedResult = true)]
        [TestCase("gif", ExpectedResult = true)]
        [TestCase("png", ExpectedResult = true)]
        [TestCase("jpeg", ExpectedResult = true)]
        public bool ReplaceLink_ReturnsImageExtensionUrl_Unchanged(string extension)
        {
            // Arrange
            string text = $"http://example.com/photo.{extension}";

            // Act
            var match = System.Text.RegularExpressions.Regex.Match(text, UrlPattern, RegexOptions.IgnoreCase);
            var result = Utilities.ReplaceLink(match, this.DefaultSite, text);

            // Assert
            return result == text;
        }

        [Test]
        [TestCase("href=\"http://example.com/page\"", "http://example.com/page", ExpectedResult = true)]
        public bool ReplaceLink_ReturnsUrl_Unchanged_WhenPrecededByHref(string text, string expected)
        {
            // Arrange & Act
            var match = System.Text.RegularExpressions.Regex.Match(text, UrlPattern, RegexOptions.IgnoreCase);
            var result = Utilities.ReplaceLink(match, this.DefaultSite, text);

            // Assert
            return result == expected;
        }

        [Test]
        [TestCase("src=\"http://example.com/page\"", "http://example.com/page", ExpectedResult = true)]
        public bool ReplaceLink_ReturnsUrl_Unchanged_WhenPrecededBySrc(string text, string expected)
        {
            // Arrange & Act
            var match = System.Text.RegularExpressions.Regex.Match(text, UrlPattern, RegexOptions.IgnoreCase);
            var result = Utilities.ReplaceLink(match, this.DefaultSite, text);

            // Assert
            return result == expected;
        }

        [Test]
        [TestCase("value=http://example.com/page", "http://example.com/page", ExpectedResult = true)]
        public bool ReplaceLink_ReturnsUrl_Unchanged_WhenPrecededByEquals(string text, string expected)
        {
            // Arrange & Act
            var match = System.Text.RegularExpressions.Regex.Match(text, UrlPattern, RegexOptions.IgnoreCase);
            var result = Utilities.ReplaceLink(match, this.DefaultSite, text);

            // Assert
            return result == expected;
        }

        [Test]
        [TestCase("https://example.com/en-us/page", "<a href=\"https://example.com/en-us/page\">https://example.com/en-us/page</a>", ExpectedResult = true)]
        public bool ReplaceLink_ReturnsInSiteAnchor_WhenUrlContainsDefaultSite(string text, string expected)
        {
            // Arrange & Act
            var match = System.Text.RegularExpressions.Regex.Match(text, UrlPattern, RegexOptions.IgnoreCase);
            var result = Utilities.ReplaceLink(match, this.DefaultSite, text);

            // Assert
            return result == expected;
        }

        [Test]
        [TestCase("http://contoso.com/page", "<a href=\"http://contoso.com/page\" target=\"_blank\" rel=\"nofollow\">http://contoso.com/page</a>", ExpectedResult = true)]
        public bool ReplaceLink_ReturnsOutSiteAnchor_WhenUrlIsExternal(string text, string expected)
        {
            // Arrange & Act
            var match = System.Text.RegularExpressions.Regex.Match(text, UrlPattern, RegexOptions.IgnoreCase);
            var result = Utilities.ReplaceLink(match, this.DefaultSite, text);

            // Assert
            return result == expected;
        }

        [Test]
        [TestCase(60, ExpectedResult = true)]
        public bool ReplaceLink_TruncatesLongUrlLabel(int urlLength)
        {
            // Arrange
            string url = "http://contoso.com/" + new string('a', urlLength);
            string expectedLabel = string.Concat(url.Substring(0, 25), "...", url.Substring(url.Length - 20));
            string expected = $"<a href=\"{url}\" target=\"_blank\" rel=\"nofollow\">{expectedLabel}</a>";

            // Act
            var match = System.Text.RegularExpressions.Regex.Match(url, UrlPattern, RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                return false;
            }

            string result = Utilities.ReplaceLink(match, this.DefaultSite, url);

            // Assert
            return result == expected;
        }
    }
}
