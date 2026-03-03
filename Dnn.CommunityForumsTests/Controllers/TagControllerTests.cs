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

namespace DotNetNuke.Modules.ActiveForumsTests.Controllers
{
    using DotNetNuke.Modules.ActiveForums.Controllers;
    using DotNetNuke.Modules.ActiveForums.Entities;
    using DotNetNuke.Modules.ActiveForumsTests.ObjectGraphs;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    internal class TagControllerTests : TestBase
    {
        [Test]
        public void GetBodyWithTagsProcessed_UserWithoutPermission_LeavesTagsAsPlainText()
        {
            // Arrange
            var mockPost = new Mock<IPostInfo>();

            var forum = this.ForumsGraph.Find(f => f.ForumID == ForumsObjectGraph.AdministratorsOnlyForumId);

            var topic = new TopicInfo
            {
                Forum = forum,
            };

            var forumUser = this.ForumUserGraph.Find(u => u.UserId == DotNetNuke.Tests.Utilities.Constants.UserID_User12);
            var author = new AuthorInfo(forumUser);

            var content = new ContentInfo
            {
                Body = "This is a test with #tag1 and #tag2.",
            };

            mockPost.SetupGet(p => p.Topic).Returns(topic);
            mockPost.SetupGet(p => p.Author).Returns(author);
            mockPost.SetupGet(p => p.Content).Returns(content);

            // Act
            var result = TagController.GetBodyWithTagsProcessed(mockPost.Object, mockPost.Object.Topic.Forum, this.MockNavigationManager.Object);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Does.Contain("#tag1"));
            Assert.That(result, Does.Contain("#tag2"));
            Assert.That(result, Does.Not.Contain("<a href"));
        }

        [Test]
        public void GetBodyWithTagsProcessed_EditorPluginTagWithHashInUrl_RemovesHashFromUrl()
        {
            // Arrange
            var mockPost = new Mock<IPostInfo>();

            var forum = this.ForumsGraph.Find(f => f.ForumID == ForumsObjectGraph.AnnouncementsForumId);

            var topic = new TopicInfo
            {
                Forum = forum,
            };

            var forumUser = this.ForumUserGraph.Find(u => u.UserId == DotNetNuke.Tests.Utilities.Constants.UserID_User12);
            var author = new AuthorInfo(forumUser);

            var content = new ContentInfo
            {
                Body = "<a href=\"https://localhost/forums/afv/search?aftg=#tag1\">#tag1</a> and <a href=\"/forums/afv/search?aftg=#tag2\">#tag2</a>",
            };

            mockPost.SetupGet(p => p.Topic).Returns(topic);
            mockPost.SetupGet(p => p.Author).Returns(author);
            mockPost.SetupGet(p => p.Content).Returns(content);

            // Act
            var result = TagController.GetBodyWithTagsProcessed(mockPost.Object, mockPost.Object.Topic.Forum, this.MockNavigationManager.Object);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Does.Not.Contain("/afv/search?aftg=#"));
            Assert.That(result, Does.Contain("aftg=tag1"));
            Assert.That(result, Does.Contain("aftg=tag2"));
        }

        [Test]
        public void GetBodyWithTagsProcessed_BodyWithMultipleTags_ProcessesAll()
        {
            // Arrange
            var mockPost = new Mock<IPostInfo>();
            var forum = this.ForumsGraph.Find(f => f.ForumID == ForumsObjectGraph.AdministratorsOnlyForumId);

            var topic = new TopicInfo
            {
                Forum = forum,
            };

            var forumUser = this.ForumUserGraph.Find(u => u.UserId == DotNetNuke.Tests.Utilities.Constants.UserID_User12);
            var author = new AuthorInfo(forumUser);

            var content = new ContentInfo
            {
                Body = "Text with #tag1 and #tag2 and #tag3.",
            };

            mockPost.SetupGet(p => p.Topic).Returns(topic);
            mockPost.SetupGet(p => p.Author).Returns(author);
            mockPost.SetupGet(p => p.Content).Returns(content);

            // Act
            var result = TagController.GetBodyWithTagsProcessed(mockPost.Object, mockPost.Object.Topic.Forum, this.MockNavigationManager.Object);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Does.Contain("#tag1"));
            Assert.That(result, Does.Contain("#tag2"));
            Assert.That(result, Does.Contain("#tag3"));
        }

        [Test]
        public void GetBodyWithTagsProcessed_TagsInContentTag_DoesntProcessTags()
        {
            // Arrange
            var mockPost = new Mock<IPostInfo>();

            var forum = this.ForumsGraph.Find(f => f.ForumID == ForumsObjectGraph.AnnouncementsForumId);

            var topic = new TopicInfo
            {
                Forum = forum,
            };

            var forumUser = this.ForumUserGraph.Find(u => u.UserId == DotNetNuke.Tests.Utilities.Constants.UserID_User12);
            var author = new AuthorInfo(forumUser);

            var content = new ContentInfo
            {
                Body = "<code>.dcf-mention-popup .item.is-selected { background: #f0f0f0; }</code> but this is an outside #tag1 which should get processed",
            };

            mockPost.SetupGet(p => p.Topic).Returns(topic);
            mockPost.SetupGet(p => p.Author).Returns(author);
            mockPost.SetupGet(p => p.Content).Returns(content);

            // Act
            var result = TagController.GetBodyWithTagsProcessed(mockPost.Object, mockPost.Object.Topic.Forum, this.MockNavigationManager.Object);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Does.Not.Contain("aftg=f0f0f0"));
            Assert.That(result, Does.Not.Contain("aftg=#f0f0f0"));
            Assert.That(result, Does.Contain("aftg=tag1"));
            Assert.That(result, Does.Contain("#f0f0f0"));
            Assert.That(result, Does.Contain("#tag1"));
        }

        [Test]
        public void GetBodyWithTagsProcessed_UserWithPermission_DoesNotThrowWhenFriendlyUrlsAreEvaluated()
        {
            // Arrange
            var mockPost = new Mock<IPostInfo>();
            var forum = this.ForumsGraph.Find(f => f.ForumID == ForumsObjectGraph.AnnouncementsForumId);
            var topic = new TopicInfo
            {
                Forum = forum,
            };

            var forumUser = this.ForumUserGraph.Find(u => u.UserId == DotNetNuke.Tests.Utilities.Constants.UserID_User12);
            var author = new AuthorInfo(forumUser);

            var content = new ContentInfo
            {
                Body = "This is a test with #tag1.",
            };

            mockPost.SetupGet(p => p.Topic).Returns(topic);
            mockPost.SetupGet(p => p.Author).Returns(author);
            mockPost.SetupGet(p => p.Content).Returns(content);

            // Act + Assert
            Assert.DoesNotThrow(() => TagController.GetBodyWithTagsProcessed(mockPost.Object, mockPost.Object.Topic.Forum, this.MockNavigationManager.Object));
        }

    }
}
