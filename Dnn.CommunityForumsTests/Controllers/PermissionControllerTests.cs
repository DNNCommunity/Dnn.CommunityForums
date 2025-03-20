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

    using DotNetNuke.Modules.ActiveForums.Controllers;
    using NUnit.Framework;

    [TestFixture]
    public class PermissionControllerTests : DotNetNuke.Modules.ActiveForumsTests.TestBase
    {
        [Test]
        public void GetNamesForRolesTest()
        {
            // Arrange
            var roles = $"{Common.Globals.glbRoleAllUsers};{Common.Globals.glbRoleUnauthUser}";
            var portalId = DotNetNuke.Tests.Utilities.Constants.PORTAL_Zero;
            var expectedResult = $"{Common.Globals.glbRoleAllUsersName};{Common.Globals.glbRoleUnauthUserName};";

            // Act
            var actualResult = PermissionController.GetNamesForRoles(portalId, roles);

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void GetRoleNameTest()
        {
            // Arrange
            var role = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_RegisteredUsers}";
            var portalId = DotNetNuke.Tests.Utilities.Constants.PORTAL_Zero;
            var expectedResult = $"{DotNetNuke.Tests.Utilities.Constants.RoleName_RegisteredUsers}";

            // Act
            string actualResult = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleName(portalId, role);

            // we can't actually test this method because internally it is using caching, so we can at least test that the method throws an exception
            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
            //Assert.Throws<NullReferenceException>(() => PermissionController.GetRoleName(portalId, roles));
        }

        [Test]
        [TestCase(new string[] { "1", "2", "3" }, new string[] { "1", "2", "3" }, ExpectedResult = true)]
        [TestCase(new string[] { "1", "2" }, new string[] { "3" }, ExpectedResult = false)]
        [TestCase(new string[] { "1" }, new string[] { "1", "2", "3" }, ExpectedResult = true)]
        public bool HasRequiredPermTest(string[] authRoles, string[] userRoles)
        {
            // Arrange
            // Act
            return PermissionController.HasRequiredPerm(authRoles, userRoles);

            // Assert
        }

        [Test]
        [TestCase(arg1: "0;1;-3;-1;38;||", arg2: "0;1;-1;-3;38;||", ExpectedResult = true)]
        [TestCase(arg1: "0;1;-1;-3;38;|||", arg2: "0;1;-1;-3;38;||", ExpectedResult = true)]
        public bool SortPermissionSetMembersTest(string permSet, string expectedResults)
        {
            // Arrange
            // Act
            // Assert
            return DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.SortPermissionSetMembers(permSet).Equals(expectedResults);
        }
    }
}
