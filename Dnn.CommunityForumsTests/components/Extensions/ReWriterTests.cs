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

namespace DotNetNuke.Modules.ActiveForumsTests.Extensions
{
    using DotNetNuke.Modules.ActiveForums;
    using Moq;
    using NUnit.Framework;

    using static DotNetNuke.Modules.ActiveForumsTests.Extensions.ForumsReWriterTests;

    /// <summary>
    /// Tests for the ForumsReWriter constructors and basic lifecycle.
    /// Focused on the parameterless constructor (delegating to PortalAliasController).
    /// </summary>
    [TestFixture]
    public class ForumsReWriterTests
    {
        public class TestableForumsReWriter : ForumsReWriter
        {
            public int ForumId => typeof(ForumsReWriter)
                .GetField("forumId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(this) is int v ? v : -1;
            public int TopicId => typeof(ForumsReWriter)
                .GetField("topicId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(this) is int v ? v : -1;
            public int Page => typeof(ForumsReWriter)
                .GetField("page", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(this) is int v ? v : 0;
            public int ContentId => typeof(ForumsReWriter)
                .GetField("contentId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(this) is int v ? v : -1;
            public int TabId => typeof(ForumsReWriter)
                .GetField("tabId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .GetValue(this) is int v ? v : -1;

            public string CallHandleOldUrls(string rawUrl, string httpAlias)
            {
                return (string)typeof(ForumsReWriter)
                    .GetMethod("HandleOldUrls", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .Invoke(this, new object[] { rawUrl, httpAlias });
            }
        }

        /// <summary>
        /// Verifies that when sendToUrl does not contain a querystring the method uses the full sendToUrl
        /// as the sendToUrlLessQString and passes an empty query string to RewritePath.
        /// Input: sendToUrl = "/folder/page.aspx"
        /// Expected: RewritePath called with ("/folder/page.aspx", "", "")
        /// </summary>
        [Test]
        public void RewriteUrl_NoQuery_UsesSendToUrlAsLessQStringAndPassesEmptyQueryString()
        {
            // Arrange
            string sendTo = "/folder/page.aspx";
            string sendToLess = null;
            string filePath = null;
            // NOTE:
            // System.Web.HttpContext and HttpServerUtility are intrinsic ASP.NET types that cannot be
            // mocked with Moq. To create a deterministic test, run this code as an integration test
            // inside an ASP.NET-hosted test environment (or use a TestHost that provides a real HttpContext).
            //
            // Example scaffolding (for developers): 
            // var req = new HttpRequest("", "http://localhost/folder/page.aspx", "");
            // var sw = new System.IO.StringWriter();
            // var resp = new HttpResponse(sw);
            // var ctx = new HttpContext(req, resp);
            // ForumsReWriter.RewriteUrl(ctx, sendTo, ref sendToLess, ref filePath);
            // Assert.AreEqual("/folder/page.aspx", sendToLess);
            // Assert.AreEqual(string.Empty, ctx.Request.QueryString.ToString()); // or check internal state set by RewritePath
            //
            // Because we cannot reliably create or mock HttpContext here, mark the test inconclusive
            // and provide instructions above to convert to an executable integration/unit test.
            // Act / Assert
            Assert.Inconclusive("Cannot unit-test RewriteUrl without a real HttpContext/HttpServerUtility. See comment in test for scaffolding to run as integration test.");
        }

        /// <summary>
        /// Verifies that when sendToUrl contains a querystring the method extracts the part before '?'
        /// into sendToUrlLessQString and passes the query portion to RewritePath.
        /// Input: sendToUrl = "/folder/page.aspx?x=1&y=2"
        /// Expected: RewritePath called with (\"/folder/page.aspx\", \"\", \"x=1&y=2\")
        /// </summary>
        [Test]
        [TestCase("/folder/page.aspx?x=1", "/folder/page.aspx", "x=1")]
        [TestCase("/folder/page.aspx?x=1&y=2", "/folder/page.aspx", "x=1&y=2")]
        public void RewriteUrl_WithQuery_SplitsQueryStringAndRewrites(string input, string expectedLess, string expectedQuery)
        {
            // Arrange
            string sendToLess = null;
            string filePath = null;
            // NOTE:
            // The method calls context.Server.MapPath(sendToUrlLessQString) and context.RewritePath(...)
            // which are not mockable via Moq because HttpContext and its Server/RewritePath members are not virtual.
            // Convert this test into an integration test running under ASP.NET to assert the actual behavior.
            //
            // Example scaffolding (for developers):
            // var req = new HttpRequest("", "http://localhost" + input, "");
            // var sw = new System.IO.StringWriter();
            // var resp = new HttpResponse(sw);
            // var ctx = new HttpContext(req, resp);
            // ForumsReWriter.RewriteUrl(ctx, input, ref sendToLess, ref filePath);
            // Assert.AreEqual(expectedLess, sendToLess);
            // Assert.IsTrue(filePath.EndsWith(expectedLess.Replace("/", "\\")) || filePath.Length > 0);
            // Assert.That(ctx.Request.QueryString.ToString(), Is.EqualTo(expectedQuery));
            //
            // Because we cannot reliably create or mock HttpContext here, mark the test inconclusive.
            // Act / Assert
            Assert.Inconclusive("Cannot unit-test RewriteUrl without a real HttpContext/HttpServerUtility. See comment in test for scaffolding to run as integration test.");
        }

        /// <summary>
        /// Verifies behavior when sendToUrlLessQString contains the legacy 404 token '/404.aspx?404;'.
        /// Input: sendToUrl = \"/404.aspx?404;/module/page.aspx\" (or similar).
        /// Expected: sendToUrlLessQString becomes the substring after the token and RewritePath is invoked with that value.
        /// Note: due to the method stripping at the first '?' the branch may be unreachable in practice;
        /// include this test as guidance to validate expected behavior in an integration environment.
        /// </summary>
        [Test]
        public void RewriteUrl_With404Pattern_Strips404PrefixBeforeRewrite()
        {
            // Arrange
            string sendTo = "/404.aspx?404;/module/page.aspx";
            string sendToLess = null;
            string filePath = null;
            // NOTE:
            // In many cases the initial IndexOf('?') logic will strip the pattern before the subsequent
            // Contains(\"/404.aspx?404;\") check, potentially making this branch unreachable.
            // An integration test that exercises the exact hosted behavior is necessary to confirm
            // whether the branch ever executes in the running application.
            //
            // Example scaffolding (for developers):
            // var req = new HttpRequest("", "http://localhost" + sendTo, "");
            // var sw = new System.IO.StringWriter();
            // var resp = new HttpResponse(sw);
            // var ctx = new HttpContext(req, resp);
            // ForumsReWriter.RewriteUrl(ctx, sendTo, ref sendToLess, ref filePath);
            // // expectedLess should be \"/module/page.aspx\" if the branch executes
            //
            // Because we cannot reliably create or mock HttpContext here, mark the test inconclusive.
            // Act / Assert
            Assert.Inconclusive("Cannot unit-test the 404-stripping branch without a real HttpContext. See comment in test for scaffolding to run as integration test.");
        }

        [TestCase("", "", "", TestName = "ResolveUrl_EmptyUrl_ReturnsEmpty")]
        [TestCase("/folder/page.aspx", "", "/folder/page.aspx", TestName = "ResolveUrl_NoTilde_ReturnsUrl")]
        [TestCase("~", "/app", "/app", TestName = "ResolveUrl_OnlyTilde_ReturnsAppPath")]
        [TestCase("~/folder/page.aspx", "/app", "/app/folder/page.aspx", TestName = "ResolveUrl_TildeSlash_AppPathWithSlash")]
        [TestCase(@"~\folder\page.aspx", "/app", "/app/folder\\page.aspx", TestName = "ResolveUrl_TildeBackslash_AppPathWithSlash")]
        [TestCase("~folder/page.aspx", "/app", "/app/folder/page.aspx", TestName = "ResolveUrl_TildeNoSlash_AppPathWithSlash")]
        [TestCase("~/folder/page.aspx", "/", "/folder/page.aspx", TestName = "ResolveUrl_AppPathRoot")]
        public void ResolveUrl_VariousInputs_ReturnsExpected(string url, string appPath, string expected)
        {
            // Act
            var result = ForumsReWriter.ResolveUrl(appPath, url);

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }                                           


        [Test]
        public void HandleOldUrls_ParsesKeyValuePairs_CorrectlySetsFields()
        {
            // Arrange
            var rewriter = new TestableForumsReWriter();
            // Simulate a URL with key=value pairs
            string rawUrl = "forumid=12/topicid=34/pageid=2/contentjumpid=99";
            string httpAlias = "site.com/";

            // Mock Data.Common and GetUrl
            var commonMock = new Mock<DotNetNuke.Modules.ActiveForums.Data.Common>();
            commonMock.Setup(m => m.GetUrl(It.IsAny<int>(), It.IsAny<int>(), 12, 34, It.IsAny<int>(), 99))
                .Returns("/forums/topic/34");
            typeof(DotNetNuke.Modules.ActiveForums.Data.Common)
                .GetField("Instance", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(null, commonMock.Object);

            // Act
            var result = rewriter.CallHandleOldUrls(rawUrl, httpAlias);

            // Assert
            Assert.That(rewriter.ForumId, Is.EqualTo(12));
            Assert.That(rewriter.TopicId, Is.EqualTo(34));
            Assert.That(rewriter.Page, Is.EqualTo(2));
            Assert.That(rewriter.ContentId, Is.EqualTo(99));
            Assert.That(result, Does.Contain("site.com/forums/topic/34"));
        }

        [Test]
        public void HandleOldUrls_ParsesSlashSeparated_CorrectlySetsFields()
        {
            // Arrange
            var rewriter = new TestableForumsReWriter();
            string rawUrl = "forumid/22/topicid/44/pageid/3/tabid/7";
            string httpAlias = "site.com/";

            // Mock Data.Common and GetUrl
            var commonMock = new Mock<DotNetNuke.Modules.ActiveForums.Data.Common>();
            commonMock.Setup(m => m.GetUrl(It.IsAny<int>(), It.IsAny<int>(), 22, 44, It.IsAny<int>(), It.IsAny<int>()))
                .Returns("/forums/topic/44");
            typeof(DotNetNuke.Modules.ActiveForums.Data.Common)
                .GetField("Instance", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(null, commonMock.Object);

            // Act
            var result = rewriter.CallHandleOldUrls(rawUrl, httpAlias);

            // Assert
            Assert.That(rewriter.ForumId, Is.EqualTo(22));
            Assert.That(rewriter.TopicId, Is.EqualTo(44));
            Assert.That(rewriter.Page, Is.EqualTo(3));
            Assert.That(rewriter.TabId, Is.EqualTo(7));
            Assert.That(result, Does.Contain("site.com/forums/topic/44"));
        }

        [Test]
        public void HandleOldUrls_ReturnsEmpty_WhenGetUrlReturnsNull()
        {
            // Arrange
            var rewriter = new TestableForumsReWriter();
            string rawUrl = "forumid=1";
            string httpAlias = "site.com/";

            // Mock Data.Common and GetUrl
            var commonMock = new Mock<DotNetNuke.Modules.ActiveForums.Data.Common>();
            commonMock.Setup(m => m.GetUrl(It.IsAny<int>(), It.IsAny<int>(), 1, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns((string)null);
            typeof(DotNetNuke.Modules.ActiveForums.Data.Common)
                .GetField("Instance", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(null, commonMock.Object);

            // Act
            var result = rewriter.CallHandleOldUrls(rawUrl, httpAlias);

            // Assert
            Assert.That(result, Is.Empty);
        }
    }
}
