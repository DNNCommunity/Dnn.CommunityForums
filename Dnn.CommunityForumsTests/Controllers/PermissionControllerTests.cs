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
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;

    using DotNetNuke.Modules.ActiveForums;
    using DotNetNuke.Modules.ActiveForums.Controllers;
    using DotNetNuke.Security.Roles;
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

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void GetAdministratorsRoleIdTest()
        {
            // Arrange
            var role = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}";
            var portalId = DotNetNuke.Tests.Utilities.Constants.PORTAL_Zero;
            var expectedResult = Convert.ToInt32($"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}");

            // Act
            var actualResult = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetAdministratorsRoleId(DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings());

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void GetAdministratorsRoleNameTest()
        {
            // Arrange
            var role = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}";
            var portalId = DotNetNuke.Tests.Utilities.Constants.PORTAL_Zero;
            var expectedResult = $"{DotNetNuke.Tests.Utilities.Constants.RoleName_Administrators}";

            // Act
            string actualResult = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetAdministratorsRoleName(DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings());

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void GetRegisteredUsersRoleIdTest()
        {
            // Arrange
            var role = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_RegisteredUsers}";
            var portalId = DotNetNuke.Tests.Utilities.Constants.PORTAL_Zero;
            var expectedResult = Convert.ToInt32($"{DotNetNuke.Tests.Utilities.Constants.RoleID_RegisteredUsers}");

            // Act
            var actualResult = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRegisteredUsersRoleId(DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings());

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void GetRegisteredUsersRoleNameTest()
        {
            // Arrange
            var role = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_RegisteredUsers}";
            var portalId = DotNetNuke.Tests.Utilities.Constants.PORTAL_Zero;
            var expectedResult = $"{DotNetNuke.Tests.Utilities.Constants.RoleName_RegisteredUsers}";

            // Act
            string actualResult = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRegisteredUsersRoleName(DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings());

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void GetRolesTest()
        {
            // Arrange
            var expectedResult = new System.Collections.Generic.List<DotNetNuke.Security.Roles.RoleInfo>()
            {
                new RoleInfo()
                {
                    RoleID = DotNetNuke.Tests.Utilities.Constants.RoleID_RegisteredUsers,
                    RoleName = DotNetNuke.Tests.Utilities.Constants.RoleName_RegisteredUsers,
                    Status = RoleStatus.Approved,
                    PortalID = DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings().PortalId,
                },
                new RoleInfo()
                {
                    RoleID = DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators,
                    RoleName = DotNetNuke.Tests.Utilities.Constants.RoleName_Administrators,
                    Status = RoleStatus.Approved,
                    PortalID = DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings().PortalId,
                },
                new RoleInfo()
                {
                    RoleID = Convert.ToInt32(DotNetNuke.Common.Globals.glbRoleUnauthUser),
                    RoleName = DotNetNuke.Common.Globals.glbRoleUnauthUserName,
                    Status = RoleStatus.Approved,
                    PortalID = DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings().PortalId,
                },
                new RoleInfo()
                {
                    RoleID = Convert.ToInt32(DotNetNuke.Common.Globals.glbRoleAllUsers),
                    RoleName = DotNetNuke.Common.Globals.glbRoleAllUsersName,
                    Status = RoleStatus.Approved,
                    PortalID = DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings().PortalId,
                },
            };

            // Act
            var actualResult = PermissionController.GetRoles(DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings().PortalId);

            // Assert
            Assert.That(actualResult.Count.Equals(expectedResult.Count));
            Assert.That(actualResult.Select(r => r.RoleID).OrderBy(r => r).ToList().SequenceEqual(expectedResult.Select(r => r.RoleID).OrderBy(r => r).ToList()));
        }

        [Test]
        public void GetRoleIdsFromRoleStringTest()
        {
            // Arrange
            var roleString = "1;2;3";
            var expectedResult = new HashSet<int> { 1, 2, 3 };

            // Act
            var actualResult = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIdsFromRoleString(roleString);

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
            var rolesNames = new string[] { DotNetNuke.Common.Globals.glbRoleAllUsersName, DotNetNuke.Common.Globals.glbRoleUnauthUserName };
            var expectedResult = new HashSet<int> { Convert.ToInt32(DotNetNuke.Common.Globals.glbRoleAllUsers),  Convert.ToInt32( DotNetNuke.Common.Globals.glbRoleUnauthUser) };

            // Act
            var actualResult = PermissionController.GetRoleIdsFromRoleNameArray(DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings().PortalId, rolesNames);

            // Assert
            Assert.That(actualResult.SetEquals(expectedResult));
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
        public bool HasRequiredPermTest(int[] authRoles, int[] userRoles)
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
            const string emptyPermissions = ";|||";
            var mockPermissions = new Mock<DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo>
            {
                Object =
                {
                    View = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_RegisteredUsers}{emptyPermissions}",
                    Read = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_RegisteredUsers}{emptyPermissions}",
                    Create = emptyPermissions,
                    Reply = emptyPermissions,
                    Edit = emptyPermissions,
                    Delete = emptyPermissions,
                    Lock = emptyPermissions,
                    Pin = emptyPermissions,
                    Attach = emptyPermissions,
                    Poll = emptyPermissions,
                    Block = emptyPermissions,
                    Trust = emptyPermissions,
                    Subscribe = emptyPermissions,
                    Announce = emptyPermissions,
                    Prioritize = emptyPermissions,
                    Moderate = emptyPermissions,
                    Move = emptyPermissions,
                    Split = emptyPermissions,
                    Ban = emptyPermissions,
                },
            };

            var permSet = mockPermissions.Object.View;
            var objType = DotNetNuke.Modules.ActiveForums.ObjectType.RoleId;
            var objId = DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators;

            var expectedResult = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_RegisteredUsers};{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}";

            // Act
            var actualResult = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.AddPermToSet(objId.ToString(), objType, permSet);

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void RemovePermFromSetTest()
        {
            // Arrange
            const string emptyPermissions = ";|||";
            var mockPermissions = new Mock<DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo>
            {
                Object =
                {
                    View = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_RegisteredUsers};{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Read = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_RegisteredUsers}{emptyPermissions}",
                    Create = emptyPermissions,
                    Reply = emptyPermissions,
                    Edit = emptyPermissions,
                    Delete = emptyPermissions,
                    Lock = emptyPermissions,
                    Pin = emptyPermissions,
                    Attach = emptyPermissions,
                    Poll = emptyPermissions,
                    Block = emptyPermissions,
                    Trust = emptyPermissions,
                    Subscribe = emptyPermissions,
                    Announce = emptyPermissions,
                    Prioritize = emptyPermissions,
                    Moderate = emptyPermissions,
                    Move = emptyPermissions,
                    Split = emptyPermissions,
                    Ban = emptyPermissions,
                },
            };

            var permSet = mockPermissions.Object.View;
            var objType = DotNetNuke.Modules.ActiveForums.ObjectType.RoleId;
            var objId = DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators;

            var expectedResult = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_RegisteredUsers}{emptyPermissions}";

            // Act
            var actualResult = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.RemovePermFromSet(objId.ToString(), objType, permSet);

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void GetPermSetForRequestedAccessTest()
        {
            // Arrange
            const string emptyPermissions = ";|||";
            var mockPermissions = new Mock<DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo>
            {
                Object =
                {
                    View = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_RegisteredUsers};{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Read = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_RegisteredUsers}{emptyPermissions}",
                    Create = emptyPermissions,
                    Reply = emptyPermissions,
                    Edit = emptyPermissions,
                    Delete = emptyPermissions,
                    Lock = emptyPermissions,
                    Pin = emptyPermissions,
                    Attach = emptyPermissions,
                    Poll = emptyPermissions,
                    Block = emptyPermissions,
                    Trust = emptyPermissions,
                    Subscribe = emptyPermissions,
                    Announce = emptyPermissions,
                    Prioritize = emptyPermissions,
                    Moderate = emptyPermissions,
                    Move = emptyPermissions,
                    Split = emptyPermissions,
                    Ban = emptyPermissions,
                },
            };

            var expectedResult = mockPermissions.Object.View;

            // Act
            var actualResult = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetPermSetForRequestedAccess(mockPermissions.Object, DotNetNuke.Modules.ActiveForums.SecureActions.View);

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
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
        public void SetPermSetForRequestedAccessTest()
        {
            // Arrange
            // Act
            // Assert
            Assert.Fail();
        }

        [Test]
        public void AddObjectToPermissionSetTest()
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
            const string emptyPermissions = ";|||";
            var mockPermissions = new Mock<DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo>
            {
                Object =
                {
                    View = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_RegisteredUsers}{emptyPermissions}",
                    Read = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_RegisteredUsers}{emptyPermissions}",
                    Create = emptyPermissions,
                    Reply = emptyPermissions,
                    Edit = emptyPermissions,
                    Delete = emptyPermissions,
                    Lock = emptyPermissions,
                    Pin = emptyPermissions,
                    Attach = emptyPermissions,
                    Poll = emptyPermissions,
                    Block = emptyPermissions,
                    Trust = emptyPermissions,
                    Subscribe = emptyPermissions,
                    Announce = emptyPermissions,
                    Prioritize = emptyPermissions,
                    Moderate = emptyPermissions,
                    Move = emptyPermissions,
                    Split = emptyPermissions,
                    Ban = emptyPermissions,
                },
            };

            var objType = DotNetNuke.Modules.ActiveForums.ObjectType.RoleId;
            var objId = DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators;

            var expectedResult = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}";

            // Act
            var actualResult = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.AddObjectToPermSet(mockPermissions.Object, SecureActions.Delete, objId.ToString(), objType).Delete;

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
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
            const string emptyPermissions = ";|||";
            var mockPermissions = new Mock<DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo>
            {
                Object =
                {
                    ModuleId = this.mockModule.Object.ModuleID,
                    View = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Read = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Create = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Reply = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Edit = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Delete = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Lock = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Pin = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Attach = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Poll = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Block = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Trust = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Subscribe = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Announce = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Prioritize = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Moderate = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Move = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Split = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Ban = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                },
            };

            var expectedResult = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}";

            // Act
            var actualResult = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetAdminPermissions(DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings().AdministratorRoleId.ToString(), this.mockModule.Object.ModuleID);

            // Assert
            Assert.That(actualResult.ModuleId, Is.EqualTo(mockPermissions.Object.ModuleId));
            Assert.That(actualResult.EqualPermissions(mockPermissions.Object), Is.True);
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
            const string emptyPermissions = ";|||";
            var mockPermissions = new Mock<DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo>
            {
                Object =
                {
                    ModuleId = this.mockModule.Object.ModuleID,
                    View = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Read = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Create = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Reply = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Edit = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Delete = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Lock = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Pin = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Attach = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Poll = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Block = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Trust = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Subscribe = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Announce = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Prioritize = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Moderate = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Move = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Split = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Ban = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                },
            };

            var expectedResult = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators};";

            // Act
            var actualResult = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetSecureObjectList(DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings(), mockPermissions.Object, DotNetNuke.Modules.ActiveForums.ObjectType.RoleId);

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
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
        [TestCase(DotNetNuke.Tests.Utilities.Constants.UserID_User12, false, new object[] { DotNetNuke.Tests.Utilities.Constants.RoleID_RegisteredUsers, DotNetNuke.Common.Globals.glbRoleAllUsers })]
        [TestCase(DotNetNuke.Tests.Utilities.Constants.UserID_Admin, false, new object[] { DotNetNuke.Tests.Utilities.Constants.RoleID_RegisteredUsers, DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators, DotNetNuke.Common.Globals.glbRoleUnauthUser, DotNetNuke.Common.Globals.glbRoleAllUsers })]
        public void GetUsersRoleIdsTest(int userId, bool isSuperUser, object[] expectedRoles)
        {
            // Arrange
            var mockUserInfo = new Mock<DotNetNuke.Entities.Users.UserInfo>
            {
                Object =
                {
                    PortalID = DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings().PortalId,
                    UserID = userId,
                    IsSuperUser = isSuperUser,
                    Profile = new DotNetNuke.Entities.Users.UserProfile()
                    {
                        PreferredLocale = "en-US",
                    },
                }
            };

            var expectedResult = expectedRoles.Select(r => Convert.ToInt32(r)).ToHashSet<int>();

            // Act
            var actualResult = PermissionController.GetUsersRoleIds(DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings(), mockUserInfo.Object);

            // Assert
            Assert.That(actualResult.SetEquals(expectedResult), Is.True);
        }

        [Test]
        public void GetPortalRoleIdsTest()
        {
            // Arrange
            var roles = new string[]
            {
                DotNetNuke.Tests.Utilities.Constants.RoleName_RegisteredUsers,
                DotNetNuke.Tests.Utilities.Constants.RoleName_Administrators,
                DotNetNuke.Common.Globals.glbRoleUnauthUserName,
                DotNetNuke.Common.Globals.glbRoleAllUsersName,
            };

            var expectedResult = string.Join(";", new List<int>
            {
                DotNetNuke.Tests.Utilities.Constants.RoleID_RegisteredUsers,
                DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators,
                Convert.ToInt32(DotNetNuke.Common.Globals.glbRoleUnauthUser),
                Convert.ToInt32(DotNetNuke.Common.Globals.glbRoleAllUsers),

            }.OrderBy(r => r));

            // Act
            var actualResult = PermissionController.GetPortalRoleIds(DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings().PortalId, roles);

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }
    }
}
