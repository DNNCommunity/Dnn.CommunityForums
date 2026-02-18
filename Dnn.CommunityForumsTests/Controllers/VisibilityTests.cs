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
            Assert.That(result.Equals(true));
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
            Assert.That(result.Equals(true));
        }

        [Test]
        public void IsPropertyVisible_Anonymous_HidesPrivate_ReturnsFalse()
        {
            // Arrange
            var user = new UserInfo { UserID = 1 };
            var profile = new UserProfile();
            var prop = new DotNetNuke.Entities.Profile.ProfilePropertyDefinition
            {
                PropertyName = "FirstName",
            };
            prop.ProfileVisibility.VisibilityMode = UserVisibilityMode.AdminOnly;
            user.Profile = profile;
            user.Profile.ProfileProperties.Add(prop);

            var accessingUser = new UserInfo { UserID = -1 };

            // Act
            var result = ForumUserController.IsPropertyVisible(user, "FirstName", accessingUser);

            // Assert
            Assert.That(result.Equals(false));
        }

        [Test]
        public void IsPropertyVisible_MembersOnly_ReturnsTrueForMember()
        {
            // Arrange
            var user = new UserInfo { UserID = 1 };
            var prop = new DotNetNuke.Entities.Profile.ProfilePropertyDefinition
            {
                PropertyName = "FirstName",
            };
            prop.ProfileVisibility.VisibilityMode = UserVisibilityMode.MembersOnly;
            user.Profile = new UserProfile();
            user.Profile.ProfileProperties.Add(prop);

            var accessingUser = new UserInfo { UserID = 10 };

            // Act
            var result = ForumUserController.IsPropertyVisible(user, "FirstName", accessingUser);

            // Assert
            Assert.That(result.Equals(true));
        }

        [Test]
        public void IsPropertyVisible_MembersOnly_ReturnsFalseForAnon()
        {
            // Arrange
            var user = new UserInfo { UserID = 1 };
            var prop = new DotNetNuke.Entities.Profile.ProfilePropertyDefinition
            {
                PropertyName = "FirstName",
            };
            prop.ProfileVisibility.VisibilityMode = UserVisibilityMode.MembersOnly;
            user.Profile = new UserProfile();
            user.Profile.ProfileProperties.Add(prop);

            var accessingUser = new UserInfo { UserID = -1 };

            // Act
            var result = ForumUserController.IsPropertyVisible(user, "FirstName", accessingUser);

            // Assert
            Assert.That(result.Equals(false));
        }
    }
}
