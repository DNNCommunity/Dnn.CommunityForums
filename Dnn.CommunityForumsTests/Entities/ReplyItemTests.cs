namespace DotNetNuke.Modules.ActiveForumsTests.Entities
{
    using DotNetNuke.Modules.ActiveForums;
    using DotNetNuke.Modules.ActiveForums.Entities;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class ReplyInfoTests
    {
        [Test]
        public void GetReplyStatusForUserTest1()
        {
            // Arrange
            var mockReply = new Mock<DotNetNuke.Modules.ActiveForums.Entities.ReplyInfo>();
            mockReply.Object.ForumId = 1;
            mockReply.Object.Forum = new DotNetNuke.Modules.ActiveForums.Entities.ForumInfo();
            mockReply.Object.Forum.ForumName = "Test Forum";
            mockReply.Object.Forum.Security = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetEmptyPermissions(-1);
            mockReply.Object.Forum.ForumGroup = new DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo();
            mockReply.Object.Forum.ForumGroup.GroupName = "Test Forum Group";
            mockReply.Object.Content = new ContentInfo();
            mockReply.Object.Content.Subject = "Test Reply";
            mockReply.Object.Content.Body = "Test Reply";

            mockReply.Object.Topic = new DotNetNuke.Modules.ActiveForums.Entities.TopicInfo();
            mockReply.Object.Topic.ForumId = 1;
            mockReply.Object.Topic.Forum = new DotNetNuke.Modules.ActiveForums.Entities.ForumInfo();
            mockReply.Object.Topic.Forum.ForumName = "Test Forum";
            mockReply.Object.Topic.Forum.Security = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetEmptyPermissions(-1);
            mockReply.Object.Topic.Forum.ForumGroup = new DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo();
            mockReply.Object.Topic.Forum.ForumGroup.GroupName = "Test Forum Group";
            mockReply.Object.Topic.Content = new ContentInfo();
            mockReply.Object.Topic.Content.Subject = "Test Topic";
            mockReply.Object.Topic.Content.Body = "Test Topic";

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

            var expectedResult = DotNetNuke.Modules.ActiveForums.Enums.ReplyStatus.Forbidden;

            // Act
            var actualResult = mockReply.Object.GetReplyStatusForUser(mockUser.Object);

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));

        }
    }
}
