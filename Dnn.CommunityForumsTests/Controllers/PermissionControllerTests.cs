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

using System.Collections.Specialized;

namespace DotNetNuke.Modules.ActiveForumsTests.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Castle.Components.DictionaryAdapter;
    using DotNetNuke.Modules.ActiveForums.Controllers;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class PermissionControllerTests : DotNetNuke.Modules.ActiveForumsTests.TestBase
    {
        // Role-related tests
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
        public void GetAdministratorsRoleIdTest()
        {
            // Arrange
            // Act
            // Assert
            Assert.Fail();
        }

        [Test]
        public void GetAdministratorsRoleNameTest()
        {
            // Arrange
            // Act
            // Assert
            Assert.Fail();
        }

        [Test]
        public void GetRegisteredUsersRoleIdTest()
        {
            // Arrange
            // Act
            // Assert
            Assert.Fail();
        }

        [Test]
        public void GetRegisteredUsersRoleNameTest()
        {
            // Arrange
            // Act
            // Assert
            Assert.Fail();
        }

        [Test]
        public void GetRolesTest()
        {
            // Arrange
            // Act
            // Assert
            Assert.Fail();
        }

        [Test]
        public void GetRoleIdsTest()
        {
            // Arrange
            // Act
            // Assert
            Assert.Fail();
        }

        [Test]
        public void GetRoleIdsFromRoleStringTest()
        {
            // Arrange
            var roleString = "1;2;3";
            var expectedResult = new HashSet<int> { 1, 2, 3 };

            // Act
            var actualResult = PermissionController.GetRoleIdsFromRoleString(roleString);

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void GetRoleIdsFromPermSetTest()
        {
            // Arrange
            var permSet = "-1;3;4|;|";
            var expectedResult = new HashSet<int> { -1, 3, 4 };

            // Act
            var actualResult = PermissionController.GetRoleIdsFromPermSet(permSet);

            // Assert
            Assert.That(actualResult.SetEquals(expectedResult));
        }

        [Test]
        public void GetRoleIdsFromRoleIdArrayTest()
        {
            // Arrange
            var roles = new string[] { "1", "2", "3" };
            var expectedResult = new HashSet<int> { 1, 2, 3 };

            // Act
            var actualResult = PermissionController.GetRoleIdsFromRoleIdArray(roles);

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void GetRoleIdsFromRoleNameArrayTest()
        {
            // Arrange
            // Act
            // Assert
            Assert.Fail();
        }

        [Test]
        public void GetRolesNVCTest()
        {
            // Arrange
            var portalId = 0;
            var roles = new string[] { DotNetNuke.Common.Globals.glbRoleAllUsers, DotNetNuke.Common.Globals.glbRoleUnauthUser };
            var expectedResult = new NameValueCollection();
            expectedResult.Add(DotNetNuke.Common.Globals.glbRoleAllUsers, DotNetNuke.Common.Globals.glbRoleAllUsersName);
            expectedResult.Add(DotNetNuke.Common.Globals.glbRoleUnauthUser, DotNetNuke.Common.Globals.glbRoleUnauthUserName);


            // Act
            var actualResult = PermissionController.GetRolesNVC(portalId, string.Join(";", roles));

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        [TestCase(new int[] { 1, 2, 3 }, new int[] { 1, 2, 3 }, ExpectedResult = true)]
        [TestCase(new int[] { 1, 2 }, new int[] { 3 }, ExpectedResult = false)]
        [TestCase(new int[] { 1 }, new int[] { 1, 2, 3 }, ExpectedResult = true)]
        public bool HasRequiredPermTest2(int[] authRoles, int[] userRoles)
        {
            // Arrange
            // Act
            // Assert
            return PermissionController.HasRequiredPerm(authRoles.ToHashSet(), userRoles.ToHashSet());
        }

        [Test]
        public void HasAccessTest()
        {
            Assert.Fail();
        }

        // Permission set manipulation tests
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

        [Test]
        public void SortPermissionSetMembersTest1()
        {
            // Arrange
            // Act
            // Assert
            Assert.Fail();
        }

        [Test]
        public void AddPermToSetTest()
        {
            // Arrange
            // Act
            // Assert
            Assert.Fail();
        }

        [Test]
        public void AddPermToSetTest1()
        {
            // Arrange
            // Act
            // Assert
            Assert.Fail();
        }

        [Test]
        public void RemovePermFromSetTest()
        {
            // Arrange
            // Act
            // Assert
            Assert.Fail();
        }

        [Test]
        public void RemovePermFromSetTest1()
        {
            // Arrange
            // Act
            // Assert
            Assert.Fail();
        }

        [Test]
        public void GetPermSetTest1()
        {
            // Arrange
            // Act
            // Assert
            Assert.Fail();
        }

        [Test]
        public void SavePermSetTest()
        {
            // Arrange
            // Act
            // Assert
            Assert.Fail();
        }

        [Test]
        public void GetPermSetForRequestedAccessTest1()
        {
            // Arrange
            // Act
            // Assert
            Assert.Fail();
        }

        [Test]
        public void GetPermSetForRequestedAccessTest2()
        {
            // Arrange
            // Act
            // Assert
            Assert.Fail();
        }

        [Test]
        public void SetPermSetForRequestedAccessTest()
        {
            // Arrange
            // Act
            // Assert
            Assert.Fail();
        }

        // Object-to-permission tests
        [Test]
        public void AddObjectToPermissionsTest()
        {
            // Arrange
            // Act
            // Assert
            Assert.Fail();
        }

        [Test]
        public void AddObjectToPermSetTest()
        {
            // Arrange
            // Act
            // Assert
            Assert.Fail();
        }

        [Test]
        public void RemoveObjectFromPermissionsTest()
        {
            // Arrange
            // Act
            // Assert
            Assert.Fail();
        }

        [Test]
        public void RemoveObjectFromAllTest()
        {
            // Arrange
            // Act
            // Assert
            Assert.Fail();
        }

        // Specific permission controller method tests
        [Test]
        public void GetDefaultPermissionsTest()
        {
            // Arrange
            // Act
            // Assert
            Assert.Fail();
        }

        [Test]
        public void CreateDefaultSetsTest()
        {
            // Arrange
            // Act
            // Assert
            Assert.Fail();
        }

        [Test]
        public void GetAdminPermissionsTest()
        {
            // Arrange
            // Act
            // Assert
            Assert.Fail();
        }

        [Test]
        public void CreateAdminPermissionsTest()
        {
            // Arrange
            // Act
            // Assert
            Assert.Fail();
        }

        [Test]
        public void GetEmptyPermissionsTest()
        {
            // Arrange
            // Act
            // Assert
            Assert.Fail();
        }

        [Test]
        public void GetRoleIdsForRequestedAccessTest()
        {
            // Arrange
            // Act
            // Assert
            Assert.Fail();
        }

        public void UpdateSecurityForSocialGroupForumTest()
        {
            // Arrange
            // Act
            // Assert
            Assert.Fail();
        }

        [Test]
        public void RemoveUnusedTest()
        {
            // Arrange
            // Act
            // Assert
            Assert.Fail();
        }

        [Test]
        public void RemoveIfUnusedTest()
        {
            // Arrange
            // Act
            // Assert
            Assert.Fail();
        }

        [Test]
        public void WhichRolesCanViewForumTest()
        {
            // Arrange
            // Act
            // Assert
            Assert.Fail();
        }

        [Test]
        public void CheckForumIdsForViewForRSSTest()
        {
            // Arrange
            // Act
            // Assert
            Assert.Fail();
        }

        [Test]
        public void GetSecureObjectListTest()
        {
            // Arrange
            // Act
            // Assert
            Assert.Fail();
        }

        [Test]
        public void GetObjFromSecObjTest()
        {
            // Arrange
            // Act
            // Assert
            Assert.Fail();
        }

        [Test]
        public void GetUsersRoleIdsTest()
        {
            // Arrange
            // Act
            // Assert
            Assert.Fail();
        }

        [Test]
        public void GetPortalRoleIdsTest()
        {
            // Arrange
            // Act
            // Assert
            Assert.Fail();
        }
    }
}
