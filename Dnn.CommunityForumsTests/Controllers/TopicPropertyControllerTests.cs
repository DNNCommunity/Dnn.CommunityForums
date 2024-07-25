using NUnit.Framework;
using DotNetNuke.Modules.ActiveForums.Controllers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Moq;
using DotNetNuke.Modules.ActiveForums.Entities;

namespace DotNetNuke.Modules.ActiveForums.Controllers.Tests
{
    [TestFixture()]
    public class TopicPropertyControllerTests
    {
        [Test()]
        public void SerializeTest()
        {
            //Arrange
            var mockForum = new Mock<DotNetNuke.Modules.ActiveForums.Entities.ForumInfo>();
            mockForum.Object.ForumID = 1;
            mockForum.Object.ForumName = "Test Forum";
            mockForum.Object.Properties = new List<DotNetNuke.Modules.ActiveForums.Entities.PropertyInfo>();
            var prop1 = new DotNetNuke.Modules.ActiveForums.Entities.PropertyInfo();
            prop1.PropertyId = 1;
            prop1.Name = "Test Property";
            prop1.DefaultValue = "Test Value";
            mockForum.Object.Properties.Add(prop1);

            var mockPropertyList = new Mock<List<DotNetNuke.Modules.ActiveForums.Entities.TopicPropertyInfo>>();
            var prop2 = new DotNetNuke.Modules.ActiveForums.Entities.TopicPropertyInfo();
            prop2.PropertyId = 1;
            prop2.Name = "Test Property";
            prop2.Value = "Test Value";
            mockPropertyList.Object.Add(prop2);

            //Act
            var actualResult = DotNetNuke.Modules.ActiveForums.Controllers.TopicPropertyController.Serialize(mockForum.Object, mockPropertyList.Object);


            //Assert
            var expectedResult = "<topicdata><properties><property id=\"1\"><name><![CDATA[Test Property]]></name><value><![CDATA[Test Value]]></value></property></properties></topicdata>";
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test()]
        public void DeserializeTest()
        {
            //Arrange

            var mockPropertyList = new Mock<List<DotNetNuke.Modules.ActiveForums.Entities.TopicPropertyInfo>>();
            var prop1 = new DotNetNuke.Modules.ActiveForums.Entities.TopicPropertyInfo();
            prop1.PropertyId = 1;
            prop1.Name = "Test Property";
            prop1.Value = "Test Value";
            mockPropertyList.Object.Add(prop1);

            var serialized = "<topicdata><properties><property id=\"1\"><name><![CDATA[Test Property]]></name><value><![CDATA[Test Value]]></value></property></properties></topicdata>";

            //Act
            var actualResult = DotNetNuke.Modules.ActiveForums.Controllers.TopicPropertyController.Deserialize(serialized);


            //Assert
            Assert.That(actualResult, Has.Count.EqualTo((mockPropertyList.Object).Count));
            Assert.That(actualResult.First().PropertyId, Is.EqualTo((mockPropertyList.Object)[0].PropertyId));
            Assert.That(actualResult.First().Name, Is.EqualTo((mockPropertyList.Object)[0].Name));
            Assert.That(actualResult.First().Value, Is.EqualTo((mockPropertyList.Object)[0].Value));
        }
    }
}
