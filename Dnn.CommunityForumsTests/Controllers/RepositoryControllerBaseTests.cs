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
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    using DotNetNuke.Data;
    using DotNetNuke.Modules.ActiveForums.Controllers;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    internal class RepositoryControllerBaseTests
    {
        [Test]
        public void Update_ItemWithDateUpdated_SetsUtcTimestampAndPersistsItem()
        {
            // Arrange
            var repository = new Mock<IRepository<TestEntity>>();
            var controller = CreateController(repository);
            var item = new TestEntity();
            var beforeUpdate = DateTime.UtcNow;

            // Act
            controller.Update(item);
            var afterUpdate = DateTime.UtcNow;

            // Assert
            Assert.That(item.DateUpdated, Is.GreaterThanOrEqualTo(beforeUpdate));
            Assert.That(item.DateUpdated, Is.LessThanOrEqualTo(afterUpdate));
            Assert.That(item.DateUpdated.Kind, Is.EqualTo(DateTimeKind.Utc));
            repository.Verify(r => r.Update(item), Times.Once);
        }

        [Test]
        public void Update_WithSqlCondition_UpdatesEachFoundItem()
        {
            // Arrange
            const string SqlCondition = "WHERE (ModuleId = @0)";
            var items = new List<TestEntity>
            {
                new TestEntity(),
                new TestEntity(),
            };
            var repository = new Mock<IRepository<TestEntity>>();
            repository.Setup(r => r.Find(SqlCondition, It.Is<object[]>(args => args.Length == 1 && (int)args[0] == 7))).Returns(items);

            var controller = CreateController(repository);

            // Act
            controller.Update(SqlCondition, 7);

            // Assert
            repository.Verify(r => r.Update(It.IsAny<TestEntity>()), Times.Exactly(items.Count));
            repository.Verify(r => r.Update(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }

        private static RepositoryControllerBase<TestEntity> CreateController(Mock<IRepository<TestEntity>> repository)
        {
            var controller = new RepositoryControllerBase<TestEntity>();
            typeof(RepositoryControllerBase<TestEntity>)
                .GetField("_repo", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(controller, repository.Object);

            return controller;
        }

        private class TestEntity
        {
            public DateTime DateUpdated { get; set; }
        }
    }
}
