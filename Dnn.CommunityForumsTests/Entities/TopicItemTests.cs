namespace DotNetNuke.Modules.ActiveForumsTests.Entities
{
    using DotNetNuke.Modules.ActiveForums;
    using DotNetNuke.Modules.ActiveForums.Entities;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class TopicInfoTests
    {
        [Test]
        public void GetTopicStatusForUserTest1()
        {
            // Arrange
            var mockTopic = new Mock<DotNetNuke.Modules.ActiveForums.Entities.TopicInfo>();
            mockTopic.Object.ForumId = 1;
            mockTopic.Object.Forum = new DotNetNuke.Modules.ActiveForums.Entities.ForumInfo();
            mockTopic.Object.Forum.ForumName = "Test Forum";
            mockTopic.Object.Forum.Security = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetEmptyPermissions(-1);
            mockTopic.Object.Forum.ForumGroup = new DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo();
            mockTopic.Object.Forum.ForumGroup.GroupName = "Test Forum Group";
            mockTopic.Object.Content = new ContentInfo();
            mockTopic.Object.Content.Subject = "Test Topic";
            mockTopic.Object.Content.Body = "Test Topic";

            var mockForum = new Mock<DotNetNuke.Modules.ActiveForums.Entities.ForumInfo>();
            mockForum.Object.ForumID = 1;
            mockForum.Object.ForumName = "Test Forum";
            mockForum.Object.TotalTopics = 0;
            mockForum.Object.Security = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetEmptyPermissions(-1);
            mockForum.Object.ForumGroup = new DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo();
            mockForum.Object.ForumGroup.GroupName = "Test Forum Group";

            var mockUser = new Mock<ForumUserInfo>();
            mockUser.Object.UserId = 1;
            mockUser.Object.UserRoles = Globals.DefaultAnonRoles + "|-1;||";
            mockUser.Object.UserInfo = new DotNetNuke.Entities.Users.UserInfo();
            mockUser.Object.UserInfo.DisplayName = "Test User";
            mockUser.Object.UserInfo.Profile = new DotNetNuke.Entities.Users.UserProfile();
            mockUser.Object.UserInfo.Profile.PreferredLocale = "en-US";

            var expectedResult = DotNetNuke.Modules.ActiveForums.Enums.TopicStatus.Forbidden;

            // Act
            var actualResult = mockTopic.Object.GetTopicStatusForUser(mockUser.Object);

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void GetTopicStatusForUserTest2()
        {
            // Arrange
            var mockTopic = new Mock<DotNetNuke.Modules.ActiveForums.Entities.TopicInfo>();
            mockTopic.Object.ForumId = 1;
            mockTopic.Object.Forum = new DotNetNuke.Modules.ActiveForums.Entities.ForumInfo();
            mockTopic.Object.Forum.ForumName = "Test Forum";
            mockTopic.Object.Forum.Security = new DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo()
            {
                View = Globals.DefaultAnonRoles + "|-1;||",
            };
            mockTopic.Object.Forum.ForumGroup = new DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo();
            mockTopic.Object.Forum.ForumGroup.GroupName = "Test Forum Group";
            mockTopic.Object.Content = new ContentInfo();
            mockTopic.Object.Content.Subject = "Test Topic";
            mockTopic.Object.Content.Body = "Test Topic";

            var mockForum = new Mock<DotNetNuke.Modules.ActiveForums.Entities.ForumInfo>();
            mockForum.Object.ForumID = 1;
            mockForum.Object.ForumName = "Test Forum";
            mockForum.Object.TotalTopics = 2;
            mockForum.Object.Security = new DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo()
            {
                View = Globals.DefaultAnonRoles + "|-1;||",
            };
            mockForum.Object.ForumGroup = new DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo();
            mockForum.Object.ForumGroup.GroupName = "Test Forum Group";

            var mockUser = new Mock<ForumUserInfo>();
            mockUser.Object.UserId = 1;
            mockUser.Object.UserRoles = Globals.DefaultAnonRoles + "|-1;||";
            mockUser.Object.UserInfo = new DotNetNuke.Entities.Users.UserInfo();
            mockUser.Object.UserInfo.DisplayName = "Test User";
            mockUser.Object.UserInfo.Profile = new DotNetNuke.Entities.Users.UserProfile();
            mockUser.Object.UserInfo.Profile.PreferredLocale = "en-US";

            var expectedResult = DotNetNuke.Modules.ActiveForums.Enums.TopicStatus.Unread;

            // Act
            var actualResult = mockTopic.Object.GetTopicStatusForUser(mockUser.Object);

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }
    }
}
