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
    using System.Linq;

    using DotNetNuke.Modules.ActiveForums.Entities;
    using DotNetNuke.Modules.ActiveForumsTests.ObjectGraphs;

    using NUnit.Framework;

    [TestFixture]
    public class TagInfoTests : DotNetNuke.Modules.ActiveForumsTests.TestBase
    {
        [Test]
        public void TagsGraph_HasExpectedCount()
        {
            // Assert
            Assert.That(this.TagsGraph, Has.Count.EqualTo(10));
        }

        [TestCase(ForumsObjectGraph.Tag1Id, "announcements")]
        [TestCase(ForumsObjectGraph.Tag2Id, "support")]
        [TestCase(ForumsObjectGraph.Tag3Id, "howto")]
        [TestCase(ForumsObjectGraph.Tag4Id, "tips")]
        [TestCase(ForumsObjectGraph.Tag5Id, "release-notes")]
        [TestCase(ForumsObjectGraph.Tag6Id, "migration")]
        [TestCase(ForumsObjectGraph.Tag7Id, "performance")]
        [TestCase(ForumsObjectGraph.Tag8Id, "glanton")]
        [TestCase(ForumsObjectGraph.Tag9Id, "security")]
        [TestCase(ForumsObjectGraph.Tag10Id, "feedback")]
        public void TagsGraph_TagName_MatchesExpected(int tagId, string expectedName)
        {
            // Arrange
            var tag = this.TagsGraph.Find(t => t.TagId == tagId);

            // Assert
            Assert.That(tag, Is.Not.Null);
            Assert.That(tag.TagName, Is.EqualTo(expectedName));
        }

        [TestCase(ForumsObjectGraph.Tag1Id, ForumsObjectGraph.ModuleId)]
        [TestCase(ForumsObjectGraph.Tag5Id, ForumsObjectGraph.ModuleId)]
        [TestCase(ForumsObjectGraph.Tag10Id, ForumsObjectGraph.ModuleId)]
        public void TagsGraph_ModuleId_MatchesExpected(int tagId, int expectedModuleId)
        {
            // Arrange
            var tag = this.TagsGraph.Find(t => t.TagId == tagId);

            // Assert
            Assert.That(tag, Is.Not.Null);
            Assert.That(tag.ModuleId, Is.EqualTo(expectedModuleId));
        }

        [TestCase(ForumsObjectGraph.Tag1Id, ForumsObjectGraph.PortalId)]
        [TestCase(ForumsObjectGraph.Tag6Id, ForumsObjectGraph.PortalId)]
        public void TagsGraph_PortalId_MatchesExpected(int tagId, int expectedPortalId)
        {
            // Arrange
            var tag = this.TagsGraph.Find(t => t.TagId == tagId);

            // Assert
            Assert.That(tag, Is.Not.Null);
            Assert.That(tag.PortalId, Is.EqualTo(expectedPortalId));
        }

        [TestCase(ForumsObjectGraph.Tag1Id, 3)]
        [TestCase(ForumsObjectGraph.Tag2Id, 7)]
        [TestCase(ForumsObjectGraph.Tag3Id, 5)]
        [TestCase(ForumsObjectGraph.Tag4Id, 2)]
        [TestCase(ForumsObjectGraph.Tag7Id, 6)]
        public void TagsGraph_Items_MatchesExpected(int tagId, int expectedItems)
        {
            // Arrange
            var tag = this.TagsGraph.Find(t => t.TagId == tagId);

            // Assert
            Assert.That(tag, Is.Not.Null);
            Assert.That(tag.Items, Is.EqualTo(expectedItems));
        }

        [Test]
        public void MockTagController_GetById_ReturnsCorrectTag()
        {
            // Arrange
            var expected = this.TagsGraph.Find(t => t.TagId == ForumsObjectGraph.Tag3Id);

            // Act
            var actual = this.MockTagController.Object.GetById(ForumsObjectGraph.Tag3Id);

            // Assert
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual.TagId, Is.EqualTo(expected.TagId));
            Assert.That(actual.TagName, Is.EqualTo(expected.TagName));
        }

        [Test]
        public void MockTagController_GetByName_ReturnsCorrectTag()
        {
            // Arrange
            var expected = this.TagsGraph.Find(t => t.TagId == ForumsObjectGraph.Tag2Id);

            // Act
            var actual = this.MockTagController.Object.GetByName(ForumsObjectGraph.ModuleId, expected.TagName);

            // Assert
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual.TagId, Is.EqualTo(expected.TagId));
            Assert.That(actual.TagName, Is.EqualTo(expected.TagName));
        }

        [Test]
        public void TagsGraph_AllTags_HaveNonEmptyNames()
        {
            // Assert
            Assert.That(
                this.TagsGraph.All(t => !string.IsNullOrWhiteSpace(t.TagName)),
                Is.True);
        }

        [Test]
        public void TagsGraph_AllTags_HavePositiveTagId()
        {
            // Assert
            Assert.That(
                this.TagsGraph.All(t => t.TagId > 0),
                Is.True);
        }

        [Test]
        public void TagsGraph_TagIds_AreUnique()
        {
            // Arrange
            var ids = this.TagsGraph.Select(t => t.TagId).ToList();

            // Assert
            Assert.That(ids, Is.Unique);
        }

        [Test]
        public void SetupTagInfo_BuildsTenTags_AndResolvesGlantonByName()
        {
            // Arrange
            var tagController = this.MockTagController.Object;

            // Act
            var glantonTag = tagController.GetByName(ForumsObjectGraph.ModuleId, "glanton");

            using (Assert.EnterMultipleScope())
            {
                Assert.That(this.TagsGraph, Is.Not.Null);
                Assert.That(this.TagsGraph.Count, Is.EqualTo(10));
                Assert.That(glantonTag, Is.Not.Null);
                Assert.That(glantonTag.TagId, Is.EqualTo(ForumsObjectGraph.Tag8Id));
                Assert.That(glantonTag.TagName, Is.EqualTo("glanton"));
            }
        }
    }
}
