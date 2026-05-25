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
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Modules.ActiveForums;

    using Moq;

    using NUnit.Framework;

    /// <summary>
    /// Tests for ForumsReWriter
    /// </summary>
    [TestFixture]
    public class ForumsReWriterTests : TestBase
    {
        private ForumsReWriter rewriter;

        [SetUp]
        public new void SetUp()
        {
            base.SetUp();
            this.rewriter = new ForumsReWriter(
                this.portalAliasService.Object,
                this.MockTagController.Object,
                this.MockCategoryController.Object,
                this.MockForumGroupController.Object,
                this.MockForumController.Object);
        }

        [Test]
        public void ProcessBeginRequest_InitializesFieldsToNullValues()
        {
            // Arrange
            var mockApp = this.CreateMockHttpApplication($"{this.DefaultSite}/api/test");

            // Act
            this.rewriter.ProcessBeginRequest(mockApp.Object);

            using (Assert.EnterMultipleScope())
            {
                // Assert - fields should be initialized to null/default values
                Assert.That(this.rewriter.ForumGroupId, Is.EqualTo(Null.NullInteger));
                Assert.That(this.rewriter.ForumId, Is.EqualTo(Null.NullInteger));
                Assert.That(this.rewriter.PortalId, Is.EqualTo(Null.NullInteger));
                Assert.That(this.rewriter.TabId, Is.EqualTo(Null.NullInteger));
                Assert.That(this.rewriter.ModuleId, Is.EqualTo(Null.NullInteger));
                Assert.That(this.rewriter.TopicId, Is.EqualTo(Null.NullInteger));
                Assert.That(this.rewriter.Page, Is.EqualTo(0));
                Assert.That(this.rewriter.Timespan, Is.EqualTo(0));
                Assert.That(this.rewriter.ContentId, Is.EqualTo(Null.NullInteger));
                Assert.That(this.rewriter.MainSettings, Is.Null);
                Assert.That(this.rewriter.CategoryId, Is.EqualTo(Null.NullInteger));
                Assert.That(this.rewriter.TagId, Is.EqualTo(Null.NullInteger));
                Assert.That(this.rewriter.OtherPrefix, Is.Empty);
                Assert.That(this.rewriter.GroupSegment, Is.Null);
                Assert.That(this.rewriter.ForumSegment, Is.Null);
                Assert.That(this.rewriter.TopicSegment, Is.Null);
                Assert.That(this.rewriter.OtherId, Is.EqualTo(Null.NullInteger));
                Assert.That(this.rewriter.GridType, Is.Null);
                Assert.That(this.rewriter.ViewUrlTypeValue, Is.EqualTo(ForumsReWriter.ViewUrlType.Default));
            }
        }

        [Test]
        [TestCase("/api/something")]
        [TestCase("/ctl/module")]
        [TestCase("/portals/0/file.pdf")]
        [TestCase("/desktopmodules/activeforums/themes/default.css")]
        [TestCase("/image.gif")]
        [TestCase("/photo.jpg")]
        [TestCase("/styles.css")]
        [TestCase("/logo.png")]
        [TestCase("/icon.svg")]
        [TestCase("/animation.swf")]
        [TestCase("/pointer.cur")]
        [TestCase("/favicon.ico")]
        [TestCase("/script.js")]
        [TestCase("/page.aspx")]
        [TestCase("/page.htm")]
        [TestCase("/page.html")]
        [TestCase("/handler.ashx")]
        [TestCase("/service.asmx")]
        [TestCase("/resource.axd")]
        [TestCase("/document.txt")]
        [TestCase("/document.pdf")]
        [TestCase("/data.xml")]
        [TestCase("/data.csv")]
        [TestCase("/workbook.xls")]
        [TestCase("/workbook.xlsx")]
        [TestCase("/document.doc")]
        [TestCase("/document.docx")]
        [TestCase("/presentation.ppt")]
        [TestCase("/presentation.pptx")]
        [TestCase("/archive.zip")]
        [TestCase("/archive.zipx")]
        [TestCase("/font.eot")]
        [TestCase("/font.ttf")]
        [TestCase("/font.otf")]
        [TestCase("/font.woff")]
        [TestCase("/font.woff2")]
        [TestCase("/image.webp")]
        public void ProcessBeginRequest_ExcludedExtensions_ReturnsEarly(string path)
        {
            // Arrange
            var mockApp = this.CreateMockHttpApplication($"{this.DefaultSite}{path}");

            // Act
            this.rewriter.ProcessBeginRequest(mockApp.Object);

            // Assert - ModuleId remains uninitialized after exclusion check
            Assert.That(this.rewriter.ModuleId, Is.EqualTo(Null.NullInteger));
        }

        [Test]
        [TestCase("?dnnprintmode=true")]
        [TestCase("?afv=post")]
        [TestCase("?afv=confirmaction")]
        [TestCase("?afv=sendto")]
        [TestCase("?afv=modreport")]
        [TestCase("?afv=search")]
        [TestCase("?afv=modtopics")]
        public void ProcessBeginRequest_ExcludedViewTypes_ReturnsEarly(string queryString)
        {
            // Arrange
            var mockApp = this.CreateMockHttpApplication($"{this.DefaultSite}/forums{queryString}");

            // Act
            this.rewriter.ProcessBeginRequest(mockApp.Object);

            // Assert
            Assert.That(this.rewriter.ModuleId, Is.EqualTo(Null.NullInteger));
        }

        [Test]
        [TestCase("/forums/unanswered/", GridTypes.Unanswered)]
        [TestCase("/forums/notread/", GridTypes.NotRead)]
        [TestCase("/forums/mytopics/", GridTypes.MyTopics)]
        [TestCase("/forums/activetopics/", GridTypes.ActiveTopics)]
        [TestCase("/forums/afprofile/", GridTypes.MySettings)]
        [TestCase("/forums/mostliked/", GridTypes.MostLiked)]
        [TestCase("/forums/mostreplies/", GridTypes.MostReplies)]
        [TestCase("/forums/afsubscriptions/", GridTypes.MySubscriptions)]
        [TestCase("/forums/announcements/", GridTypes.Announcements)]
        [TestCase("/forums/unresolved/", GridTypes.Unresolved)]
        [TestCase("/forums/badgeusers/", GridTypes.BadgeUsers)]
        [TestCase("/forums/userbadges/", GridTypes.UserBadges)]
        [TestCase("/forums/recyclebin/", GridTypes.RecycleBin)]
        [TestCase("/forums/tags/", GridTypes.Tags)]
        public void GetGridTypeFromUrl_KnownGridType_ReturnsExpectedValue(string url, string expected)
        {
            this.rewriter.GetViewTypeFromUrl(url);

            Assert.That(this.rewriter.GridType, Is.EqualTo(expected));
        }

        [Test]
        [TestCase("/forums/some-topic/", TestName = "GetGridTypeFromUrl_UnknownSegment_ReturnsNull")]
        [TestCase("", TestName = "GetGridTypeFromUrl_EmptyUrl_ReturnsNull")]
        [TestCase(null, TestName = "GetGridTypeFromUrl_NullUrl_ReturnsNull")]
        public void GetGridTypeFromUrl_NonGridTypeUrl_ReturnsNull(string url)
        {
            this.rewriter.GetViewTypeFromUrl(url);

            Assert.That(this.rewriter.GridType, Is.Null);
        }

        [Test]
        [TestCase("/FORUMS/UNANSWERED/", GridTypes.Unanswered)]
        [TestCase("/Forums/MostLiked/", GridTypes.MostLiked)]
        public void GetGridTypeFromUrl_IsCaseInsensitive(string url, string expected)
        {
            this.rewriter.GetViewTypeFromUrl(url);

            Assert.That(this.rewriter.GridType, Is.EqualTo(expected));
        }

        [Test]
        [TestCase("/forums/activetopics/", GridTypes.ActiveTopics, 0, 0, -1, TestName = "GetViewTypeFromUrl_GridTypeOnly_NoTimespanNoPage")]
        [TestCase("/forums/activetopics/2", GridTypes.ActiveTopics, 0, 2, -1, TestName = "GetViewTypeFromUrl_PageOnly_NoTimespan")]
        [TestCase("/forums/activetopics/2/", GridTypes.ActiveTopics, 0, 2, -1, TestName = "GetViewTypeFromUrl_PageOnly_WithTrailingSlash_NoTimespan")]
        [TestCase("/forums/activetopics/ts/30", GridTypes.ActiveTopics, 30, 0, -1, TestName = "GetViewTypeFromUrl_TimespanOnly_NoPage")]
        [TestCase("/forums/activetopics/ts/30/", GridTypes.ActiveTopics, 30, 0, -1, TestName = "GetViewTypeFromUrl_TimespanOnly_WithTrailingSlash_NoPage")]
        [TestCase("/forums/activetopics/ts/30/2", GridTypes.ActiveTopics, 30, 2, -1, TestName = "GetViewTypeFromUrl_TimespanAndPage")]
        [TestCase("/forums/activetopics/ts/30/2/", GridTypes.ActiveTopics, 30, 2, -1, TestName = "GetViewTypeFromUrl_TimespanAndPage_WithTrailingSlash")]
        [TestCase("/forums/unanswered/ts/7/3", GridTypes.Unanswered, 7, 3, -1, TestName = "GetViewTypeFromUrl_Unanswered_TimespanAndPage")]
        [TestCase("/forums/unanswered/ts/7", GridTypes.Unanswered, 7, 0, -1, TestName = "GetViewTypeFromUrl_Unanswered_TimespanOnly")]
        [TestCase("/forums/unanswered/5", GridTypes.Unanswered, 0, 5, -1, TestName = "GetViewTypeFromUrl_Unanswered_PageOnly")]
        [TestCase("/FORUMS/ACTIVETOPICS/TS/30/2", GridTypes.ActiveTopics, 30, 2, -1, TestName = "GetViewTypeFromUrl_TimespanAndPage_IsCaseInsensitive")]
        [TestCase("/forums/some-topic/", null, 0, 0, -1, TestName = "GetViewTypeFromUrl_UnknownGridType_NoTimespanNoPage")]
        [TestCase("/forums/afv/grid/afgt/tags/tag/glanton/ts/2147483647", GridTypes.Tags, 2147483647, 0, 8, TestName = "GetViewTypeFromUrl_TagsGrid_TimespanNoPage")]
        [TestCase("/forums/afv/grid/afgt/tags/tag/glanton/ts/2147483647/3", GridTypes.Tags, 2147483647, 3, 8, TestName = "GetViewTypeFromUrl_TagsGrid_TimespanWithPage")]
        [TestCase("/forums/afv/grid/afgt/tags/tag/glanton//ts/2147483647/3", GridTypes.Tags, 2147483647, 3, 8, TestName = "GetViewTypeFromUrl_TagsGrid_Doubleslash")]
        public void GetViewTypeFromUrl_ExtractsGridTypeTimespanAndPage(string url, string expectedGridType, int expectedTimespan, int expectedPage, int expectedTagId)
        {
            this.rewriter.GetViewTypeFromUrl(url);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.rewriter.GridType, Is.EqualTo(expectedGridType));
                Assert.That(this.rewriter.Timespan, Is.EqualTo(expectedTimespan));
                Assert.That(this.rewriter.Page, Is.EqualTo(expectedPage));
                Assert.That(this.rewriter.TagId, Is.EqualTo(expectedTagId));
            }
        }

        [Test]
        [TestCase("/general", "general", null, null, 0)]
        [TestCase("/general/", "general", null, null, 0)]
        [TestCase("/general/lwsn-infor", "general", "lwsn-infor", null, 0)]
        [TestCase("/general/2026/topic/49", "general", "2026", "topic", 49)]
        [TestCase("/general/2026/topic/49/", "general", "2026", "topic", 49)]
        public void HandleNonLikesPages_ParsesExpectedSegments(
            string input,
            string expectedGroup,
            string expectedForum,
            string expectedTopic,
            int expectedPage)
        {
            // Arrange & Act
            this.rewriter.HandleNonLikesPages(input);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(this.rewriter.GroupSegment, Is.EqualTo(expectedGroup));
                Assert.That(this.rewriter.ForumSegment, Is.EqualTo(expectedForum));
                Assert.That(this.rewriter.Page, Is.EqualTo(expectedPage));
            }
            if (expectedTopic != null)
            {
                Assert.That(this.rewriter.TopicSegment, Is.EqualTo(expectedTopic));
            }
        }

        [Test]
        [TestCase("/category/books/other", "category", true, "books", "/other")]
        [TestCase("/category/books/", "category", true, "books", "/")]
        [TestCase("/category/books//other", "category", true, "books", "/other")]
        [TestCase("/something/else", "category", false, "", "/something/else")]
        [TestCase(null, "category", false, "", null)]
        [TestCase("/category/books", "", false, "", "/category/books")]
        [TestCase("category/books", "", false, "", "category/books")]
        public void ExtractPrefixedSegment_WorksAsExpected(string inputUrl, string prefix, bool expectedResult, string expectedName, string expectedUrl)
        {
            var url = inputUrl;
            var result = this.rewriter.ExtractPrefixedSegment(ref url, prefix, out var name);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result, Is.EqualTo(expectedResult));
                Assert.That(name, Is.EqualTo(expectedName));
                Assert.That(this.rewriter.OtherPrefix, Is.EqualTo(expectedName));
            }

            if (expectedUrl == null)
            {
                Assert.That(url, Is.Null);
            }
            else
            {
                Assert.That(url, Is.EqualTo(expectedUrl));
            }
        }

        [Test]
        // HandleLikesPages: URL with leading slash, contentId, no page number
        [TestCase("/group/forum/likes/42/", 42, 0, "group/forum/likes/42")]
        // HandleLikesPages: URL without leading slash, contentId, no page number
        [TestCase("group/forum/likes/99/", 99, 0, "group/forum/likes/99")]
        // HandleLikesPages: URL with leading slash, contentId, and page number
        [TestCase("/group/forum/likes/7/3", 7, 3, "group/forum/likes/7")]
        // HandleLikesPages: URL with trailing slash after page number
        [TestCase("/forum/likes/123/2/", 123, 2, "forum/likes/123")]
        // HandleLikesPages: No page number, content id only
        [TestCase("/likes/500/", 500, 0, "likes/500")]
        // HandleLikesPages: Page property is set from URL page segment
        [TestCase("/forum/likes/55/4", 55, 4, "forum/likes/55")]
        [TestCase("/forum/likes/55/10", 55, 10, "forum/likes/55")]
        [TestCase("/forum/likes/55/1", 55, 1, "forum/likes/55")]
        [TestCase("/forum/likes/88/", 88, 0, "forum/likes/88", Description = "No page number in URL; Page should remain at default (0)")]
        [TestCase("/forum/likes/88", 88, 0, "forum/likes/88", Description = "No page number in URL; Page should remain at default (0)")]
        public void HandleLikesPages_SetsContentIdAndPageAndReturnsUrlWithoutPage(
            string searchURL,
            int expectedContentId,
            int expectedPage,
            string expectedLikeUrlWithoutPage)
        {
            // Arrange & Act
            var result = this.rewriter.HandleLikesPages(searchURL, "likes");

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(this.rewriter.Page, Is.EqualTo(expectedPage), "Page should match");
                Assert.That(this.rewriter.ContentId, Is.EqualTo(expectedContentId), "contentId should match");
                Assert.That(result, Does.EndWith(expectedLikeUrlWithoutPage), "Returned URL should match expected suffix");
            }
        }

        // HandleLikesPages: URL with no likes segment — no match, returns URLs as is but without leading slash
        [Test]
        [TestCase("/group/forum/topic/", "group/forum/topic/", TestName = "HandleLikesPages_WhenNoLikesSegment_UrlWithTrailingSlash_ReturnsUrlWithTrailingSlash")]
        [TestCase("group/forum/topic", "group/forum/topic", TestName = "HandleLikesPages_WhenNoLikesSegment_UrlWithoutTrailingSlash_ReturnsUrlWithoutTrailingSlash")]
        public void HandleLikesPages_WhenNoLikesSegment_ReturnsUrlWithTrailingSlash(
            string searchURL,
            string expectedResult)
        {
            // Arrange & Act
            var result = this.rewriter.HandleLikesPages(searchURL, "likes");

            // Assert
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        // HandleLikesPages: Leading slash is stripped from the returned value
        [Test]
        public void HandleLikesPages_StripsLeadingSlashFromReturnedUrl()
        {
            // Arrange & Act
            var result = this.rewriter.HandleLikesPages("/forum/likes/10/", "likes");

            // Assert
            Assert.That(result, Does.Not.StartWith("/"), "Returned URL should not start with a slash after stripping");
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
            string sendTo = $"/folder/page.aspx";

            var mockApp = this.CreateMockHttpApplication($"{this.DefaultSite}");
            var req = mockApp.Object.Request;

            // Act
            DotNetNuke.Modules.ActiveForums.ForumsReWriter.RewriteUrl(mockApp.Object.Context, sendTo);

            // Assert
            Assert.That(mockApp.Object.Context.Request.QueryString.ToString(), Is.Empty);
        }

        /// <summary>
        /// Verifies that when sendToUrl contains a querystring the method extracts the part before '?'
        /// </summary>
        [Test]
        [TestCase("/folder/page.aspx?x=1", "/folder/page.aspx", "x=1")]
        [TestCase("/folder/page.aspx?x=1&y=2", "/folder/page.aspx", "x=1&y=2")]
        public void RewriteUrl_WithQuery_SplitsQueryStringAndRewrites(string sendTo, string expectedPath, string expectedQuery)
        {
            // Arrange & Act
            var mockApp = this.CreateMockHttpApplication($"{this.DefaultSite}");
            DotNetNuke.Modules.ActiveForums.ForumsReWriter.RewriteUrl(mockApp.Object.Context, sendTo);
            using (Assert.EnterMultipleScope())
            {

                // Assert
                Assert.That(mockApp.Object.Context.Request.QueryString.ToString(), Is.EqualTo(expectedQuery));
                Assert.That(mockApp.Object.Context.Request.Path.ToString(), Is.EqualTo(expectedPath));
            }
        }

        /// <summary>
        /// Verifies behavior when sendToUrlLessQString contains the legacy 404 token '/404.aspx?404;'.
        /// </summary>
        [Test]
        public void RewriteUrl_With404Pattern_Strips404PrefixBeforeRewrite()
        {
            // Arrange
            string sendTo = "/404.aspx?404;/module/page.aspx";
            string expectedPath = "/module/page.aspx";
            var mockApp = this.CreateMockHttpApplication($"{this.DefaultSite}");

            // Act
            DotNetNuke.Modules.ActiveForums.ForumsReWriter.RewriteUrl(mockApp.Object.Context, sendTo);

            // Assert
            Assert.That(mockApp.Object.Context.Request.Path.ToString(), Is.EqualTo(expectedPath));
        }

        [Test]
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
            string rawUrl = $"{ParamKeys.ForumId}=1/{ParamKeys.TopicId}=1/{ParamKeys.PageId}=2/{ParamKeys.ContentJumpId}=99";

            // Act
            this.rewriter.HandleOldUrls(rawUrl);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(this.rewriter.ForumId, Is.EqualTo(1));
                Assert.That(this.rewriter.TopicId, Is.EqualTo(1));
                Assert.That(this.rewriter.Page, Is.EqualTo(2));
                Assert.That(this.rewriter.ContentId, Is.EqualTo(99));
            }
        }

        [Test]
        public void HandleOldUrls_ParsesSlashSeparated_CorrectlySetsFields()
        {
            // Arrange
            string rawUrl = $"{ParamKeys.ForumId}/1/{ParamKeys.TopicId}/1/{ParamKeys.PageId}/3/{ParamKeys.TabId}/7";

            // Act
            this.rewriter.HandleOldUrls(rawUrl);

            using (Assert.EnterMultipleScope())
            {
                // Assert
                Assert.That(this.rewriter.ForumId, Is.EqualTo(1));
                Assert.That(this.rewriter.TopicId, Is.EqualTo(1));
                Assert.That(this.rewriter.Page, Is.EqualTo(3));
                Assert.That(this.rewriter.TabId, Is.EqualTo(7));
            }
        }

        [Test]
        public void HandleOldUrls_ParsesPageJumpId_KeyValue_SetsPage()
        {
            // Arrange
            string rawUrl = $"{ParamKeys.PageJumpId}=5/{ParamKeys.ForumId}=2";

            // Act
            this.rewriter.HandleOldUrls(rawUrl);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.rewriter.Page, Is.EqualTo(5));
                Assert.That(this.rewriter.ForumId, Is.EqualTo(2));
            }
        }

        [Test]
        public void HandleOldUrls_ParsesPageJumpId_SlashSeparated_SetsPage()
        {
            // Arrange
            string rawUrl = $"{ParamKeys.ForumId}/3/{ParamKeys.PageJumpId}/4";

            // Act
            this.rewriter.HandleOldUrls(rawUrl);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.rewriter.ForumId, Is.EqualTo(3));
                Assert.That(this.rewriter.Page, Is.EqualTo(4));
            }
        }

        [Test]
        public void HandleOldUrls_ParsesContentJumpId_SlashSeparated_SetsContentId()
        {
            // Arrange
            string rawUrl = $"{ParamKeys.TopicId}/10/{ParamKeys.ContentJumpId}/77";

            // Act
            this.rewriter.HandleOldUrls(rawUrl);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.rewriter.TopicId, Is.EqualTo(10));
                Assert.That(this.rewriter.ContentId, Is.EqualTo(77));
            }
        }

        [Test]
        public void HandleOldUrls_AmpersandSeparated_CorrectlySetsFields()
        {
            // Arrange — & causes splitter to switch from "/" to "&"
            string rawUrl = $"?{ParamKeys.ForumId}=8&{ParamKeys.TopicId}=9&{ParamKeys.PageId}=2&{ParamKeys.TabId}=5";

            // Act
            this.rewriter.HandleOldUrls(rawUrl);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.rewriter.ForumId, Is.EqualTo(8));
                Assert.That(this.rewriter.TopicId, Is.EqualTo(9));
                Assert.That(this.rewriter.Page, Is.EqualTo(2));
                Assert.That(this.rewriter.TabId, Is.EqualTo(5));
            }
        }

        [Test]
        [TestCase("AFF=1/AFT=2", 1, 2, TestName = "HandleOldUrls_KeyValuePairs_IsCaseInsensitive")]
        [TestCase("aff/1/AFT/2", 1, 2, TestName = "HandleOldUrls_SlashSeparated_IsCaseInsensitive")]
        public void HandleOldUrls_KeyNames_IsCaseInsensitive(string rawUrl, int expectedForumId, int expectedTopicId)
        {
            // Act
            this.rewriter.HandleOldUrls(rawUrl);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.rewriter.ForumId, Is.EqualTo(expectedForumId));
                Assert.That(this.rewriter.TopicId, Is.EqualTo(expectedTopicId));
            }
        }

        [Test]
        [TestCase("", TestName = "HandleNonLikesPages_EmptyString_DoesNotThrow")]
        [TestCase(null, TestName = "HandleNonLikesPages_Null_DoesNotThrow")]
        public void HandleNonLikesPages_EmptyOrNull_LeavesSegmentsNull(string input)
        {
            // Act & Assert — should not throw
            Assert.That(() => this.rewriter.HandleNonLikesPages(input), Throws.Nothing);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.rewriter.GroupSegment, Is.Null);
                Assert.That(this.rewriter.ForumSegment, Is.Null);
                Assert.That(this.rewriter.TopicSegment, Is.Null);
                Assert.That(this.rewriter.Page, Is.EqualTo(0));
            }
        }

        [Test]
        public void ProcessBeginRequest_WhenPortalAliasNotFound_ReturnsEarlyWithNullPortalId()
        {
            // Arrange — portalAliasService returns null for all lookups
            this.portalAliasService
                .Setup(s => s.GetPortalAliases())
                .Returns(new System.Collections.Generic.Dictionary<string, IPortalAliasInfo>());
            this.portalAliasService
                .Setup(s => s.GetPortalAlias(It.IsAny<string>()))
                .Returns((IPortalAliasInfo)null);

            var mockApp = this.CreateMockHttpApplication($"{this.DefaultSite}/forums/general/");

            // Act
            this.rewriter.ProcessBeginRequest(mockApp.Object);

            // Assert — processing stopped before PortalId was set
            Assert.That(this.rewriter.PortalId, Is.EqualTo(Null.NullInteger));
        }
    }
}
