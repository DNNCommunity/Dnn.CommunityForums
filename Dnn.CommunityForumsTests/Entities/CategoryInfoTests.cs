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
    public class CategoryInfoTests : DotNetNuke.Modules.ActiveForumsTests.TestBase
    {
        [Test]
        public void CategoriesGraph_HasExpectedCount()
        {
            // Assert
            Assert.That(this.CategoriesGraph, Has.Count.EqualTo(10));
        }

        [TestCase(ForumsObjectGraph.Category1Id, "Bug Reports")]
        [TestCase(ForumsObjectGraph.Category2Id, "Feature Requests")]
        [TestCase(ForumsObjectGraph.Category3Id, "How-To")]
        [TestCase(ForumsObjectGraph.Category4Id, "Tips & Tricks")]
        [TestCase(ForumsObjectGraph.Category5Id, "Release Notes")]
        [TestCase(ForumsObjectGraph.Category6Id, "Installation")]
        [TestCase(ForumsObjectGraph.Category7Id, "Performance")]
        [TestCase(ForumsObjectGraph.Category8Id, "Members News")]
        [TestCase(ForumsObjectGraph.Category9Id, "Admin Policies")]
        [TestCase(ForumsObjectGraph.Category10Id, "Feedback")]
        public void CategoriesGraph_CategoryName_MatchesExpected(int categoryId, string expectedName)
        {
            // Arrange
            var category = this.CategoriesGraph.Find(c => c.CategoryId == categoryId);

            // Assert
            Assert.That(category, Is.Not.Null);
            Assert.That(category.CategoryName, Is.EqualTo(expectedName));
        }

        [TestCase(ForumsObjectGraph.Category1Id, ForumsObjectGraph.AnnouncementsForumId)]
        [TestCase(ForumsObjectGraph.Category2Id, ForumsObjectGraph.AnnouncementsForumId)]
        [TestCase(ForumsObjectGraph.Category3Id, ForumsObjectGraph.GeneralDiscussionForumId)]
        [TestCase(ForumsObjectGraph.Category5Id, ForumsObjectGraph.HelpAndSupportForumId)]
        [TestCase(ForumsObjectGraph.Category8Id, ForumsObjectGraph.MembersOnlyForumId)]
        [TestCase(ForumsObjectGraph.Category9Id, ForumsObjectGraph.AdministratorsOnlyForumId)]
        public void CategoriesGraph_ForumId_MatchesExpected(int categoryId, int expectedForumId)
        {
            // Arrange
            var category = this.CategoriesGraph.Find(c => c.CategoryId == categoryId);

            // Assert
            Assert.That(category, Is.Not.Null);
            Assert.That(category.ForumId, Is.EqualTo(expectedForumId));
        }

        [TestCase(ForumsObjectGraph.Category1Id, ForumsObjectGraph.GeneralGroupId)]
        [TestCase(ForumsObjectGraph.Category8Id, ForumsObjectGraph.PrivateGroupId)]
        [TestCase(ForumsObjectGraph.Category9Id, ForumsObjectGraph.PrivateGroupId)]
        public void CategoriesGraph_ForumGroupId_MatchesExpected(int categoryId, int expectedGroupId)
        {
            // Arrange
            var category = this.CategoriesGraph.Find(c => c.CategoryId == categoryId);

            // Assert
            Assert.That(category, Is.Not.Null);
            Assert.That(category.ForumGroupId, Is.EqualTo(expectedGroupId));
        }

        [TestCase(ForumsObjectGraph.Category1Id, ForumsObjectGraph.ModuleId)]
        [TestCase(ForumsObjectGraph.Category5Id, ForumsObjectGraph.ModuleId)]
        public void CategoriesGraph_ModuleId_MatchesExpected(int categoryId, int expectedModuleId)
        {
            // Arrange
            var category = this.CategoriesGraph.Find(c => c.CategoryId == categoryId);

            // Assert
            Assert.That(category, Is.Not.Null);
            Assert.That(category.ModuleId, Is.EqualTo(expectedModuleId));
        }

        [TestCase(ForumsObjectGraph.Category1Id, ForumsObjectGraph.PortalId)]
        [TestCase(ForumsObjectGraph.Category6Id, ForumsObjectGraph.PortalId)]
        public void CategoriesGraph_PortalId_MatchesExpected(int categoryId, int expectedPortalId)
        {
            // Arrange
            var category = this.CategoriesGraph.Find(c => c.CategoryId == categoryId);

            // Assert
            Assert.That(category, Is.Not.Null);
            Assert.That(category.PortalId, Is.EqualTo(expectedPortalId));
        }

        [Test]
        public void MockCategoryController_GetById_ReturnsCorrectCategory()
        {
            // Arrange
            var expected = this.CategoriesGraph.Find(c => c.CategoryId == ForumsObjectGraph.Category3Id);

            // Act
            var actual = this.MockCategoryController.Object.GetById(ForumsObjectGraph.Category3Id);

            // Assert
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual.CategoryId, Is.EqualTo(expected.CategoryId));
            Assert.That(actual.CategoryName, Is.EqualTo(expected.CategoryName));
        }

        [Test]
        public void MockCategoryController_GetByName_ReturnsCorrectCategory()
        {
            // Arrange
            var expected = this.CategoriesGraph.Find(c => c.CategoryId == ForumsObjectGraph.Category2Id);

            // Act
            var actual = this.MockCategoryController.Object.GetByName(ForumsObjectGraph.ModuleId, expected.CategoryName);

            // Assert
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual.CategoryId, Is.EqualTo(expected.CategoryId));
            Assert.That(actual.CategoryName, Is.EqualTo(expected.CategoryName));
        }

        [Test]
        public void CategoriesGraph_AllCategories_HaveNonEmptyNames()
        {
            // Assert
            Assert.That(
                this.CategoriesGraph.All(c => !string.IsNullOrWhiteSpace(c.CategoryName)),
                Is.True);
        }

        [Test]
        public void CategoriesGraph_AllCategories_HavePositiveCategoryId()
        {
            // Assert
            Assert.That(
                this.CategoriesGraph.All(c => c.CategoryId > 0),
                Is.True);
        }

        [Test]
        public void CategoriesGraph_CategoryIds_AreUnique()
        {
            // Arrange
            var ids = this.CategoriesGraph.Select(c => c.CategoryId).ToList();

            // Assert
            Assert.That(ids, Is.Unique);
        }

        [TestCase(ForumsObjectGraph.Category1Id, 1)]
        [TestCase(ForumsObjectGraph.Category2Id, 2)]
        [TestCase(ForumsObjectGraph.Category3Id, 1)]
        public void CategoriesGraph_Priority_MatchesExpected(int categoryId, int expectedPriority)
        {
            // Arrange
            var category = this.CategoriesGraph.Find(c => c.CategoryId == categoryId);

            // Assert
            Assert.That(category, Is.Not.Null);
            Assert.That(category.Priority, Is.EqualTo(expectedPriority));
        }
    }
}
