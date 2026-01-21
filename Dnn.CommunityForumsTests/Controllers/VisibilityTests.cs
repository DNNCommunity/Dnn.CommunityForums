// Copyright (c) by DNN Community
//
// DNN Community licenses this file to you under the MIT license.
//
// See the LICENSE file in the project root for more information.

namespace DotNetNuke.Modules.ActiveForumsTests.Controllers
{
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Modules.ActiveForums.Controllers;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class VisibilityTests : TestBase
    {
        [Test]
        public void IsPropertyVisible_Admin_ReturnsTrue()
        {
            // Arrange
            var user = new UserInfo { UserID = 1 };
            var accessingUser = new UserInfo { IsSuperUser = true };

            // Act
            var result = ForumUserController.IsPropertyVisible(user, "FirstName", accessingUser);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void IsPropertyVisible_Owner_ReturnsTrue()
        {
            // Arrange
            var user = new UserInfo { UserID = 1 };
            var accessingUser = new UserInfo { UserID = 1 };

            // Act
            var result = ForumUserController.IsPropertyVisible(user, "FirstName", accessingUser);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void IsPropertyVisible_Anonymous_HidesPrivate_ReturnsFalse()
        {
            // Arrange
            var user = new UserInfo { UserID = 1 };
            var profile = new UserProfile();
            var prop = new DotNetNuke.Entities.Profile.ProfilePropertyDefinition
            {
                PropertyName = "FirstName"
            };
            prop.ProfileVisibility.VisibilityMode = UserVisibilityMode.AdminOnly;
            user.Profile = profile;
            user.Profile.ProfileProperties.Add(prop);

            var accessingUser = new UserInfo { UserID = -1 };

            // Act
            var result = ForumUserController.IsPropertyVisible(user, "FirstName", accessingUser);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void IsPropertyVisible_MembersOnly_ReturnsTrueForMember()
        {
            // Arrange
            var user = new UserInfo { UserID = 1 };
            var prop = new DotNetNuke.Entities.Profile.ProfilePropertyDefinition
            {
                PropertyName = "FirstName"
            };
            prop.ProfileVisibility.VisibilityMode = UserVisibilityMode.MembersOnly;
            user.Profile = new UserProfile();
            user.Profile.ProfileProperties.Add(prop);

            var accessingUser = new UserInfo { UserID = 10 };

            // Act
            var result = ForumUserController.IsPropertyVisible(user, "FirstName", accessingUser);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void IsPropertyVisible_MembersOnly_ReturnsFalseForAnon()
        {
            // Arrange
            var user = new UserInfo { UserID = 1 };
            var prop = new DotNetNuke.Entities.Profile.ProfilePropertyDefinition
            {
                PropertyName = "FirstName"
            };
            prop.ProfileVisibility.VisibilityMode = UserVisibilityMode.MembersOnly;
            user.Profile = new UserProfile();
            user.Profile.ProfileProperties.Add(prop);

            var accessingUser = new UserInfo { UserID = -1 };

            // Act
            var result = ForumUserController.IsPropertyVisible(user, "FirstName", accessingUser);

            // Assert
            Assert.IsFalse(result);
        }
    }
}
