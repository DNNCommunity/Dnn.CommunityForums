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
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using DotNetNuke.Modules.ActiveForums.Controllers;
    using DotNetNuke.Modules.ActiveForums.Entities;
    using Moq;
    using NUnit.Framework;

    [TestFixture()]
    public class TopicPropertyControllerTests
    {
        [Test()]
        public void SerializeTest()
        {
            // Arrange
            var mockForum = new Mock<ForumInfo>();
            mockForum.Object.ForumID = 1;
            mockForum.Object.ForumName = "Test Forum";
            mockForum.Object.Properties = new List<PropertyInfo>();
            var prop1 = new PropertyInfo();
            prop1.PropertyId = 1;
            prop1.Name = "Test Property";
            prop1.DefaultValue = "Test Value";
            mockForum.Object.Properties.Add(prop1);

            var mockPropertyList = new Mock<List<TopicPropertyInfo>>();
            var prop2 = new TopicPropertyInfo();
            prop2.PropertyId = 1;
            prop2.Name = "Test Property";
            prop2.Value = "Test Value";
            mockPropertyList.Object.Add(prop2);

            // Act
            var actualResult = TopicPropertyController.Serialize(mockForum.Object, mockPropertyList.Object);

            // Assert
            var expectedResult = "<topicdata><properties><property id=\"1\"><name><![CDATA[Test Property]]></name><value><![CDATA[Test Value]]></value></property></properties></topicdata>";
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test()]
        public void DeserializeTest()
        {
            // Arrange
            var mockPropertyList = new Mock<List<TopicPropertyInfo>>();
            var prop1 = new TopicPropertyInfo();
            prop1.PropertyId = 1;
            prop1.Name = "Test Property";
            prop1.Value = "Test Value";
            mockPropertyList.Object.Add(prop1);

            var serialized = "<topicdata><properties><property id=\"1\"><name><![CDATA[Test Property]]></name><value><![CDATA[Test Value]]></value></property></properties></topicdata>";

            // Act
            var actualResult = TopicPropertyController.Deserialize(serialized);

            // Assert
            Assert.That(actualResult, Has.Count.EqualTo(mockPropertyList.Object.Count));
            Assert.That(actualResult.First().PropertyId, Is.EqualTo(mockPropertyList.Object[0].PropertyId));
            Assert.That(actualResult.First().Name, Is.EqualTo(mockPropertyList.Object[0].Name));
            Assert.That(actualResult.First().Value, Is.EqualTo(mockPropertyList.Object[0].Value));
        }
    }
}
