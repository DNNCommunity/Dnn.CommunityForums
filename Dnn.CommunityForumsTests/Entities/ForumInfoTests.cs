namespace DotNetNuke.Modules.ActiveForumsTests.Entities
{
    using DotNetNuke.Modules.ActiveForums;
    using DotNetNuke.Modules.ActiveForums.Entities;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class ForumInfoTests
    {
        [Test]
        public void GetForumStatusForUserTest1()
        {
            // Arrange
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

            var expectedResult = DotNetNuke.Modules.ActiveForums.Enums.ForumStatus.Forbidden;

            // Act
            var actualResult = mockForum.Object.GetForumStatusForUser(mockUser.Object);

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void GetForumStatusForUserTest2()
        {
            // Arrange
            var mockForum = new Mock<DotNetNuke.Modules.ActiveForums.Entities.ForumInfo>();
            mockForum.Object.ForumID = 1;
            mockForum.Object.ForumName = "Test Forum";
            mockForum.Object.TotalTopics = 0;
            mockForum.Object.Security = new DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo()
            {
                View = Globals.DefaultAnonRoles + "|-1;||",
            };
            mockForum.Object.ForumGroup = new DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo();
            mockForum.Object.ForumGroup.GroupName = "Test Forum Group";

            var mockUser = new Mock<ForumUserInfo>();
            mockUser.Object.UserId = 1;
            mockUser.Object.UserRoles = Globals.DefaultAnonRoles + "|-1;||";
            mockUser.Object.UserInfo = new DotNetNuke.Entities.Users.UserInfo
            {
                DisplayName = "Test User",
                Profile = new DotNetNuke.Entities.Users.UserProfile
                {
                    PreferredLocale = "en-US"
                }
            };

            var expectedResult = DotNetNuke.Modules.ActiveForums.Enums.ForumStatus.Empty;

            // Act
            var actualResult = mockForum.Object.GetForumStatusForUser(mockUser.Object);

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }
    }
}
