using System.Web;

namespace DotNetNuke.Modules.ActiveForumsTests.Controllers
{
    using DotNetNuke.Modules.ActiveForums;
    using DotNetNuke.Modules.ActiveForums.Controllers;
    using Moq;
    using NUnit.Framework;
    using System.Text;
    
    [TestFixture]
    public class TokenControllerTests
    {
        [Test]
        public void ReplaceTopicTokensTest()
        {
            // Arrange
            var mockRequest = new Mock<HttpRequest>();

            var mockTopic = new Mock<Modules.ActiveForums.Entities.TopicInfo>();
            mockTopic.Object.ForumId = 1;
            mockTopic.Object.Forum = new Modules.ActiveForums.Entities.ForumInfo();
            mockTopic.Object.Forum.ForumName = "Test Forum";
            mockTopic.Object.Forum.Security = PermissionController.GetEmptyPermissions(-1);
            mockTopic.Object.Forum.ForumGroup = new Modules.ActiveForums.Entities.ForumGroupInfo();
            mockTopic.Object.Forum.ForumGroup.GroupName = "Test Forum Group";
            mockTopic.Object.Content = new Modules.ActiveForums.Entities.ContentInfo();
            mockTopic.Object.Content.Subject = "Test Topic";
            mockTopic.Object.Content.Body = "Test Topic";

            var mockForum = new Mock<Modules.ActiveForums.Entities.ForumInfo>();
            mockForum.Object.ForumID = 1;
            mockForum.Object.ForumName = "Test Forum";
            mockForum.Object.ForumGroup = new Modules.ActiveForums.Entities.ForumGroupInfo();
            mockForum.Object.ForumGroup.GroupName = "Test Forum Group";

            var mockUser = new Mock<Modules.ActiveForums.Entities.ForumUserInfo>();
            mockUser.Object.UserId = 1;
            mockUser.Object.UserInfo = new DotNetNuke.Entities.Users.UserInfo();
            mockUser.Object.UserInfo.DisplayName = "Test User";
            mockUser.Object.UserInfo.Profile = new DotNetNuke.Entities.Users.UserProfile();
            mockUser.Object.UserRoles = Globals.DefaultAnonRoles + "|-1;||";
            mockUser.Object.UserInfo.Profile.PreferredLocale = "en-US";

            var navigationManager = new Modules.ActiveForums.Services.Tests.NavigationManager(null); // new Services.URLNavigator().NavigationManager();

            var templateStringBuilder = new StringBuilder("blah blah [SPLITBUTTONS1] blah [SPLITBUTTONS2] [TOPICSUBJECT] blah");

            var expectedResult = "blah blah [SPLITBUTTONS1] blah  [TOPICSUBJECT] blah";
            // Act

            var actualResult = TokenController.ReplaceTopicTokens(templateStringBuilder, mockTopic.Object, null, null, navigationManager, mockUser.Object, mockRequest.Object, 0).ToString();
            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void ReplaceForumTokensTest()
        {
            // Arrange
            var mockForum = new Mock<Modules.ActiveForums.Entities.ForumInfo>();
            mockForum.Object.ForumID = 1;
            mockForum.Object.ForumName = "Test Forum";
            mockForum.Object.Security = PermissionController.GetEmptyPermissions(-1);
            mockForum.Object.ForumGroup = new Modules.ActiveForums.Entities.ForumGroupInfo();
            mockForum.Object.ForumGroup.GroupName = "Test Forum Group";

            var mockUser = new Mock<Modules.ActiveForums.Entities.ForumUserInfo>();
            mockUser.Object.UserId = 1;
            mockUser.Object.UserInfo = new DotNetNuke.Entities.Users.UserInfo();
            mockUser.Object.UserInfo.DisplayName = "Test User";
            mockUser.Object.UserInfo.Profile = new DotNetNuke.Entities.Users.UserProfile();
            mockUser.Object.UserRoles = Globals.DefaultAnonRoles + "|-1;||";
            mockUser.Object.UserInfo.Profile.PreferredLocale = "en-US";

            var navigationManager = new Modules.ActiveForums.Services.Tests.NavigationManager(null); // new Services.URLNavigator().NavigationManager();

            var templateStringBuilder = new StringBuilder("blah blah [GROUPNAME] blah blah");
            var expectedResult = "blah blah Test Forum Group blah blah";

            // Act
            var actualResult = TokenController.ReplaceForumTokens(templateStringBuilder, mockForum.Object, null, null, navigationManager, mockUser.Object, HttpContext.Current.Request, 0, CurrentUserTypes.Auth).ToString();
            
            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }
        [Test]
        public void RemovePrefixedToken1()
        {
            // Arrange
            var templateStringBuilder = new StringBuilder("blah blah [TOKENTOREMOVE:5] blah blah");
            var tokenPrefix = "TOKENTOREMOVE";
            var expectedResult = "blah blah  blah blah";
            
            // Act
            var actualResult = TokenController.RemovePrefixedToken(templateStringBuilder, tokenPrefix).ToString();
            
            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }
        [Test]
        public void RemovePrefixedToken2()
        {
            // Arrange
            var templateStringBuilder = new StringBuilder("blah blah [TOKENTOREMOVE] blah blah");
            var tokenPrefix = "[TOKENTOREMOVE";
            var expectedResult = "blah blah  blah blah";
            
            // Act
            var actualResult = TokenController.RemovePrefixedToken(templateStringBuilder, tokenPrefix).ToString();
            
            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }
    }
}
