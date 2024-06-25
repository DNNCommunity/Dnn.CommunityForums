using NUnit.Framework;
using DotNetNuke.Modules.ActiveForums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using DotNetNuke.Modules.ActiveForums.Services;
using DotNetNuke.Abstractions;

namespace DotNetNuke.Modules.ActiveForums.Controllers.Tests
{
    [TestFixture()]
    public class TokenControllerTests
    {
        [Test()]
        public void ReplaceTopicTokensTest()
        {
            //Arrange
            var mockTopic = new Mock<DotNetNuke.Modules.ActiveForums.Entities.TopicInfo>();
            mockTopic.Object.ForumId = 1;
            mockTopic.Object.Content = new DotNetNuke.Modules.ActiveForums.Entities.ContentInfo();
            mockTopic.Object.Content.Subject = "Test Topic";
            var templateStringBuilder = new StringBuilder("blah blah [SPLITBUTTONS1] blah [SPLITBUTTONS2] [TOPICSUBJECT] blah");
            string expectedResult = $"blah blah [SPLITBUTTONS1] blah  [TOPICSUBJECT] blah";
            //Act
            string actualResult = DotNetNuke.Modules.ActiveForums.Controllers.TokenController.ReplaceTopicTokens(templateStringBuilder, mockTopic.Object).ToString();
            //Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test()]
        public void ReplaceForumTokensTest()
        {
            //Arrange
            var mockForum = new Mock<DotNetNuke.Modules.ActiveForums.Entities.ForumInfo>();
            mockForum.Object.ForumID = 1;
            mockForum.Object.ForumName = "Test Forum";
            mockForum.Object.ForumGroup = new DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo();
            mockForum.Object.ForumGroup.GroupName = "Test Forum Group";
             
            var navigationManager = (INavigationManager)new Services.URLNavigator();
            
            var templateStringBuilder = new StringBuilder("blah blah [GROUPNAME] blah blah");
            string expectedResult = $"blah blah Test Forum Group blah blah";
             
            //Act
            string actualResult = DotNetNuke.Modules.ActiveForums.Controllers.TokenController.ReplaceForumTokens(templateStringBuilder, mockForum.Object, null, null, navigationManager, null, 0, 0, CurrentUserTypes.Auth).ToString();
            //Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }
        [Test()]
        public void RemovePrefixedToken1()
        {
            //Arrange
            var templateStringBuilder = new StringBuilder("blah blah [TOKENTOREMOVE:5] blah blah");
            string tokenPrefix = "TOKENTOREMOVE";
            string expectedResult = $"blah blah  blah blah";
            //Act
            string actualResult = DotNetNuke.Modules.ActiveForums.Controllers.TokenController.RemovePrefixedToken(templateStringBuilder, tokenPrefix).ToString();
            //Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }
        [Test()]
        public void RemovePrefixedToken2()
        {
            //Arrange
            var templateStringBuilder = new StringBuilder("blah blah [TOKENTOREMOVE] blah blah");
            string tokenPrefix = "[TOKENTOREMOVE";
            string expectedResult = $"blah blah  blah blah";
            //Act
            string actualResult = DotNetNuke.Modules.ActiveForums.Controllers.TokenController.RemovePrefixedToken(templateStringBuilder, tokenPrefix).ToString();
            //Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }
    }
}