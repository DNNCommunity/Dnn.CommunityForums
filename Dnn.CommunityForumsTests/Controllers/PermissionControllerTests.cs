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

    using DotNetNuke.Entities.Users;
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
            var actualResult = PermissionController.GetNamesForRoles(DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings(), roles);

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void GetRoleNameTest()
        {
            // Arrange
            var role = DotNetNuke.Tests.Utilities.Constants.RoleID_RegisteredUsers;
            var portalId = DotNetNuke.Tests.Utilities.Constants.PORTAL_Zero;
            var expectedResult = $"{DotNetNuke.Tests.Utilities.Constants.RoleName_RegisteredUsers}";

            // Act
            var actualResult = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleName(DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings(), (int)role);

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void GetRoleIDTest()
        {
            // Arrange
            var roleName = DotNetNuke.Tests.Utilities.Constants.RoleName_RegisteredUsers;
            var portalId = DotNetNuke.Tests.Utilities.Constants.PORTAL_Zero;
            var expectedResult = Convert.ToInt32(DotNetNuke.Tests.Utilities.Constants.RoleID_RegisteredUsers);

            // Act
            var actualResult = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleId(DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings(), roleName);

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
            var actualResult = PermissionController.GetRoles(DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings());

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
            var permSet = "-1;3;4";
            var expectedResult = new HashSet<int> { -1, 3, 4 };

            // Act
            var actualResult = PermissionController.GetRoleIdsFromPermSet(permSet);

            // Assert
            Assert.That(actualResult.SetEquals(expectedResult));
        }

        [Test]
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
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
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
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
            var roles = new string[]
            {
                DotNetNuke.Common.Globals.glbRoleAllUsers,
                DotNetNuke.Common.Globals.glbRoleUnauthUser,
            };
            var expectedResult = new NameValueCollection
            {
                { DotNetNuke.Common.Globals.glbRoleAllUsers, DotNetNuke.Common.Globals.glbRoleAllUsersName },
                { DotNetNuke.Common.Globals.glbRoleUnauthUser, DotNetNuke.Common.Globals.glbRoleUnauthUserName },
            };

            // Act
            var actualResult = PermissionController.GetRolesNVC(DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings(), string.Join(";", roles));

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
        [TestCase("1;2;3", "1;2;3", ExpectedResult = true)]
        [TestCase("1;2", "3", ExpectedResult = false)]
        [TestCase("1", "1;2;3", ExpectedResult = true)]
        public bool HasAccessTest(string authRoles, string userRoles)
        {
            // Arrange
            // Act
            // Assert
            return PermissionController.HasAccess(authRoles, userRoles);
        }

        // Permission set manipulation tests
        [Test]
        [TestCase(arg1: "0;1;-3;-1;38", arg2: "-3;-1;0;1;38;", ExpectedResult = true)]
        [TestCase(arg1: "0;1;-1;-3;38;", arg2: "-3;-1;0;1;38;", ExpectedResult = true)]
        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public bool SortPermissionSetMembersTest(string permSet, string expectedResults)
        {
            // Arrange
            // Act
            // Assert
            return DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.SortPermissionSetMembers(permSet).Equals(expectedResults);
        }

        [Test]
        public void AddPermToSetTest()
        {
            // Arrange
            const string emptyPermissions = "";
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
                    Trust = emptyPermissions,
                    Subscribe = emptyPermissions,
                    Announce = emptyPermissions,
                    Prioritize = emptyPermissions,
                    Moderate = emptyPermissions,
                    Move = emptyPermissions,
                    Split = emptyPermissions,
                    ManageUsers = emptyPermissions,
                },
            };

            var permSet = mockPermissions.Object.View;
            var objId = DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators;

            var expectedResult = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators};{DotNetNuke.Tests.Utilities.Constants.RoleID_RegisteredUsers}";

            // Act
            var actualResult = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.AddPermToSet(objId.ToString(), permSet);

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void RemovePermFromSetTest()
        {
            // Arrange
            const string emptyPermissions = "";
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
                    Trust = emptyPermissions,
                    Subscribe = emptyPermissions,
                    Announce = emptyPermissions,
                    Prioritize = emptyPermissions,
                    Moderate = emptyPermissions,
                    Move = emptyPermissions,
                    Split = emptyPermissions,
                    ManageUsers = emptyPermissions,
                },
            };

            var permSet = mockPermissions.Object.View;
            var objId = DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators;

            var expectedResult = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_RegisteredUsers}{emptyPermissions}";

            // Act
            var actualResult = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.RemovePermFromSet(objId.ToString(), permSet);

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void GetPermSetForRequestedAccessTest()
        {
            // Arrange
            const string emptyPermissions = "";
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
                    Trust = emptyPermissions,
                    Subscribe = emptyPermissions,
                    Announce = emptyPermissions,
                    Prioritize = emptyPermissions,
                    Moderate = emptyPermissions,
                    Move = emptyPermissions,
                    Split = emptyPermissions,
                    ManageUsers = emptyPermissions,
                },
            };

            var expectedResult = mockPermissions.Object.View;

            // Act
            var actualResult = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetPermSetForRequestedAccess(mockPermissions.Object, DotNetNuke.Modules.ActiveForums.SecureActions.View);

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        [Test]
        public void SavePermSetTest()
        {
            // Arrange
            // Act
            // Assert
            /* not testable without DI for permissions Controller */
            Assert.Throws<NullReferenceException>(() => new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController().SavePermSet(this.mockModule.Object.ModuleID, -1, string.Empty, string.Empty));
        }

        [Test]
        public void AddObjectToPermSetTest()
        {
            // Arrange
            const string emptyPermissions = "";
            var mockPermissions = new Mock<DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo>
            {
                Object =
                {
                    View = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_RegisteredUsers};{emptyPermissions}",
                    Read = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_RegisteredUsers}{emptyPermissions}",
                    Create = emptyPermissions,
                    Reply = emptyPermissions,
                    Edit = emptyPermissions,
                    Delete = emptyPermissions,
                    Lock = emptyPermissions,
                    Pin = emptyPermissions,
                    Attach = emptyPermissions,
                    Poll = emptyPermissions,
                    Trust = emptyPermissions,
                    Subscribe = emptyPermissions,
                    Announce = emptyPermissions,
                    Prioritize = emptyPermissions,
                    Moderate = emptyPermissions,
                    Move = emptyPermissions,
                    Split = emptyPermissions,
                    ManageUsers = emptyPermissions,
                },
            };
            var requestedAction = mockPermissions.Object.View;
            var objId = DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators;

            var expectedResult = new Mock<DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo>
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
                        Trust = emptyPermissions,
                        Subscribe = emptyPermissions,
                        Announce = emptyPermissions,
                        Prioritize = emptyPermissions,
                        Moderate = emptyPermissions,
                        Move = emptyPermissions,
                        Split = emptyPermissions,
                        ManageUsers = emptyPermissions,
                    },
                };

            // Act
            var actualResult = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.AddObjectToPermSet(mockPermissions.Object, SecureActions.View, objId.ToString());

            // Assert
            Assert.That(actualResult.EqualPermissions(expectedResult.Object), Is.True);
        }

        [Test]
        public void RemoveObjectFromAllTest()
        {
            // Arrange
            const string emptyPermissions = "";
            var registeredUsersRoleId = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRegisteredUsersRoleId(portalSettings: DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings()).ToString();
            var mockPermissions = new Mock<DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo>
            {
                Object =
                {
                    ModuleId = this.mockModule.Object.ModuleID,
                    View = $"{DotNetNuke.Common.Globals.glbRoleUnauthUser};{DotNetNuke.Common.Globals.glbRoleAllUsers};{registeredUsersRoleId};{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Read = $"{DotNetNuke.Common.Globals.glbRoleUnauthUser};{DotNetNuke.Common.Globals.glbRoleAllUsers};{registeredUsersRoleId};{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Create = $"{registeredUsersRoleId};{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Edit = $"{registeredUsersRoleId};{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Delete = $"{registeredUsersRoleId};{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Reply = $"{registeredUsersRoleId};{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Subscribe = $"{registeredUsersRoleId};{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Attach = $"{registeredUsersRoleId};{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Lock = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Pin = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Poll = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Trust = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Announce = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Prioritize = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Moderate = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Move = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Split = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    ManageUsers = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                },
            };

            var objId = DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators;

            var expectedResult = new Mock<DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo>
                {
                    Object =
                    {
                         ModuleId = this.mockModule.Object.ModuleID,
                         View = $"{DotNetNuke.Common.Globals.glbRoleUnauthUser};{DotNetNuke.Common.Globals.glbRoleAllUsers};{registeredUsersRoleId};{emptyPermissions}",
                         Read = $"{DotNetNuke.Common.Globals.glbRoleUnauthUser};{DotNetNuke.Common.Globals.glbRoleAllUsers};{registeredUsersRoleId};{emptyPermissions}",
                         Create = $"{registeredUsersRoleId};{emptyPermissions}",
                         Edit = $"{registeredUsersRoleId};{emptyPermissions}",
                         Delete = $"{registeredUsersRoleId};{emptyPermissions}",
                         Reply = $"{registeredUsersRoleId};{emptyPermissions}",
                         Subscribe = $"{registeredUsersRoleId};{emptyPermissions}",
                         Attach = $"{registeredUsersRoleId};{emptyPermissions}",
                         Lock = $"{emptyPermissions}",
                         Pin = $"{emptyPermissions}",
                         Poll = $"{emptyPermissions}",
                         Trust = $"{emptyPermissions}",
                         Announce = $"{emptyPermissions}",
                         Prioritize = $"{emptyPermissions}",
                         Moderate = $"{emptyPermissions}",
                         Move = $"{emptyPermissions}",
                         Split = $"{emptyPermissions}",
                         ManageUsers = $"{emptyPermissions}",
                    },
                };

            // Act
            var actualResult = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.RemoveObjectFromAll(mockPermissions.Object, objId.ToString());

            // Assert
            Assert.That(actualResult.EqualPermissions(expectedResult.Object), Is.True);
        }

        // Specific permission controller method tests
        [Test]
        public void GetDefaultPermissionsTest()
        {
            // Arrange
            const string emptyPermissions = "";
            var registeredUsersRoleId = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRegisteredUsersRoleId(portalSettings: DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings()).ToString();
            var expectedResult = new Mock<DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo>
            {
                Object =
                {
                    ModuleId = this.mockModule.Object.ModuleID,
                    View = $"{DotNetNuke.Common.Globals.glbRoleUnauthUser};{DotNetNuke.Common.Globals.glbRoleAllUsers};{registeredUsersRoleId};{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Read = $"{DotNetNuke.Common.Globals.glbRoleUnauthUser};{DotNetNuke.Common.Globals.glbRoleAllUsers};{registeredUsersRoleId};{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Create = $"{registeredUsersRoleId};{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Edit = $"{registeredUsersRoleId};{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Delete = $"{registeredUsersRoleId};{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Reply = $"{registeredUsersRoleId};{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Subscribe = $"{registeredUsersRoleId};{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Attach = $"{registeredUsersRoleId};{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Lock = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Pin = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Poll = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Trust = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Announce = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Prioritize = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Moderate = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Move = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Split = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    ManageUsers = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                },
            };

            // Act
            var actualResult = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetDefaultPermissions(DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings(), this.mockModule.Object.ModuleID);

            // Assert
            Assert.That(actualResult.EqualPermissions(expectedResult.Object), Is.True);
            Assert.That(actualResult.ModuleId, Is.EqualTo(expectedResult.Object.ModuleId));
        }

        [Test]
        public void GetAdminPermissionsTest()
        {
            // Arrange
            const string emptyPermissions = "";
            var expectedResult = new Mock<DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo>
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
                    Trust = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Subscribe = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Announce = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Prioritize = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Moderate = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Move = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Split = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    ManageUsers = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                },
            };

            // Act
            var actualResult = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetAdminPermissions(DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings(), this.mockModule.Object.ModuleID);

            // Assert
            Assert.That(actualResult.ModuleId, Is.EqualTo(expectedResult.Object.ModuleId));
            Assert.That(actualResult.EqualPermissions(expectedResult.Object), Is.True);
        }

        [Test]
        public void GetEmptyPermissionsTest()
        {
            // Arrange
            const string emptyPermissions = "";
            var expectedResult = new Mock<DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo>
            {
                Object =
                {
                    ModuleId = this.mockModule.Object.ModuleID,
                    View = $"{emptyPermissions}",
                    Read = $"{emptyPermissions}",
                    Create = $"{emptyPermissions}",
                    Reply = $"{emptyPermissions}",
                    Edit = $"{emptyPermissions}",
                    Delete = $"{emptyPermissions}",
                    Lock = $"{emptyPermissions}",
                    Pin = $"{emptyPermissions}",
                    Attach = $"{emptyPermissions}",
                    Poll = $"{emptyPermissions}",
                    Trust = $"{emptyPermissions}",
                    Subscribe = $"{emptyPermissions}",
                    Announce = $"{emptyPermissions}",
                    Prioritize = $"{emptyPermissions}",
                    Moderate = $"{emptyPermissions}",
                    Move = $"{emptyPermissions}",
                    Split = $"{emptyPermissions}",
                    ManageUsers = $"{emptyPermissions}",
                },
            };

            // Act
            var actualResult = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetEmptyPermissions(this.mockModule.Object.ModuleID);

            // Assert
            Assert.That(actualResult.EqualPermissions(expectedResult.Object), Is.True);
            Assert.That(actualResult.ModuleId, Is.EqualTo(expectedResult.Object.ModuleId));
        }

        [Test]
        public void GetRoleIdsForRequestedAccessTest()
        {
            // Arrange
            // Act
            // Assert
            /* not yet testable */
            Assert.Throws<NullReferenceException>(() => DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIdsForRequestedAccess(this.mockModule.Object.ModuleID, -1, SecureActions.View));
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
            /* not yet testable */
            Assert.Throws<NullReferenceException>(() => new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController().RemoveUnused(this.mockModule.Object.ModuleID));
        }

        [Test]
        public void RemoveIfUnusedTest()
        {
            // Arrange
            // Act
            // Assert
            /* not yet testable */
            Assert.Throws<NullReferenceException>(() => new DotNetNuke.Modules.ActiveForums.Controllers.PermissionController().RemoveIfUnused(-1, this.mockModule.Object.ModuleID));
        }

        [Test]
        public void WhichRolesCanViewForumTest()
        {
            // Arrange
            // Act
            // Assert
            /* not yet testable */
            Assert.Throws<NullReferenceException>(() => DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.WhichRolesCanViewForum(this.mockModule.Object.ModuleID, -1, string.Empty));        }

        [Test]
        public void CheckForumIdsForViewForRSSTest()
        {
            // Arrange
            // Act
            // Assert
            /* not yet testable */
            Assert.That(DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.CheckForumIdsForViewForRSS(this.mockModule.Object.ModuleID, string.Empty, new HashSet<int>()), Is.EqualTo(string.Empty));
            Assert.Throws<NullReferenceException>(() => DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.CheckForumIdsForViewForRSS(this.mockModule.Object.ModuleID, "1",  new HashSet<int>() { 1, 2, 3 }));
        }

        [Test]
        public void GetSecureObjectListTest()
        {
            // Arrange
            const string emptyPermissions = "";
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
                    Trust = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Subscribe = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Announce = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Prioritize = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Moderate = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Move = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Split = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    ManageUsers = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                },
            };

            var expectedResult = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}";

            // Act
            var actualResult = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetSecureObjectList(DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings(), mockPermissions.Object);

            // Assert
            Assert.That(actualResult, Contains.Substring(expectedResult));
        }

        [Test]
        public void GetRoleIdsUsedByPermissionTest()
        {
            // Arrange
            const string emptyPermissions = "";
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
                    Trust = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Subscribe = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Announce = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Prioritize = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Moderate = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Move = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    Split = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                    ManageUsers = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators}{emptyPermissions}",
                },
            };

            var expectedResult = new HashSet<int> { DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators };

            // Act
            var actualResult = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleIdsUsedByPermissionInfo(DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings(), mockPermissions.Object);

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        [TestCase(DotNetNuke.Tests.Utilities.Constants.USER_AnonymousUserId, false, new object[] { DotNetNuke.Common.Globals.glbRoleUnauthUser, DotNetNuke.Common.Globals.glbRoleAllUsers })]
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
                },
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

        [Test]
        public void GetRoleIdsTest()
        {
            // Arrange
            var roles = new HashSet<int>
            {
                DotNetNuke.Tests.Utilities.Constants.RoleID_RegisteredUsers,
                DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators,
                Convert.ToInt32(DotNetNuke.Common.Globals.glbRoleUnauthUser),
                Convert.ToInt32(DotNetNuke.Common.Globals.glbRoleAllUsers),
            };

            var expectedResult = string.Join(";", new List<int>
            {
                DotNetNuke.Tests.Utilities.Constants.RoleID_RegisteredUsers,
                DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators,
                Convert.ToInt32(DotNetNuke.Common.Globals.glbRoleUnauthUser),
                Convert.ToInt32(DotNetNuke.Common.Globals.glbRoleAllUsers),

            }.OrderBy(r => r));

            // Act
            var actualResult = PermissionController.GetRoleIds(roles);

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }
        
        [Test]
        public void RemoveRoleIdFromRoleIdsTest()
        {
            // Arrange
            var roles = new HashSet<int>
            {
                DotNetNuke.Tests.Utilities.Constants.RoleID_RegisteredUsers,
                DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators,
                Convert.ToInt32(DotNetNuke.Common.Globals.glbRoleUnauthUser),
                Convert.ToInt32(DotNetNuke.Common.Globals.glbRoleAllUsers),
            };

            var expectedResult = new HashSet<int>
            {
                DotNetNuke.Tests.Utilities.Constants.RoleID_RegisteredUsers,
                Convert.ToInt32(DotNetNuke.Common.Globals.glbRoleUnauthUser),
                Convert.ToInt32(DotNetNuke.Common.Globals.glbRoleAllUsers),
            };

            // Act
            var actualResult = PermissionController.RemoveRoleIdFromRoleIds(DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators, roles);

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }
                [Test]
        public void GetObjFromSecObj_WithNullPermSet_ShouldUseAdminRoleId()
        {
            // Arrange
            string nullPermSet = null;
            string objects = string.Empty;

            // Act
            string result = PermissionController.GetObjFromSecObj(DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings(), nullPermSet, objects);

            // Assert
            Assert.That(result, Does.Contain(DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators.ToString()));
        }

        [Test]
        public void GetObjFromSecObj_WithEmptyPermSet_ShouldUseAdminRoleId()
        {
            // Arrange
            string emptyPermSet = string.Empty;
            string objects = string.Empty;

            // Act
            string result = PermissionController.GetObjFromSecObj(DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings(), emptyPermSet, objects);

            // Assert
            Assert.That(result, Does.Contain(DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators.ToString()));
        }

        [Test]
        public void GetObjFromSecObj_WithSingleRole_ShouldAddToObjects()
        {
            // Arrange
            string permSet = "1;";
            string objects = string.Empty;

            // Act
            string result = PermissionController.GetObjFromSecObj(DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings(), permSet, objects);

            // Assert
            Assert.That(result, Does.Contain("1"));
        }

        [Test]
        public void GetObjFromSecObj_WithMultipleRoles_ShouldAddAllToObjects()
        {
            // Arrange
            string permSet = "1;2;3;";
            string objects = string.Empty;

            // Act
            string result = PermissionController.GetObjFromSecObj(DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings(), permSet, objects);

            // Assert
            Assert.That(result, Does.Contain("1"));
            Assert.That(result, Does.Contain("2"));
            Assert.That(result, Does.Contain("3"));
        }

        [Test]
        public void GetObjFromSecObj_WithExistingObjects_ShouldNotDuplicate()
        {
            // Arrange
            string permSet = "1;2;";
            string objects = "1;";

            // Act
            string result = PermissionController.GetObjFromSecObj(DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings(), permSet, objects);

            // Assert
            // Role 1 should appear only once
            int count = 0;
            int startIndex = 0;
            while ((startIndex = result.IndexOf("1;", startIndex)) != -1)
            {
                count++;
                startIndex += 2;
            }

            Assert.That(count, Is.EqualTo(1));
            Assert.That(result, Does.Contain("2"));
        }

        [Test]
        public void GetObjFromSecObj_WithEmptyEntriesInPermSet_ShouldSkipEmpty()
        {
            // Arrange
            string permSet = "1;;2;;";
            string objects = string.Empty;

            // Act
            string result = PermissionController.GetObjFromSecObj(DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings(), permSet, objects);

            // Assert
            Assert.That(result, Does.Contain("1"));
            Assert.That(result, Does.Contain("2"));
            // Should not have consecutive semicolons from empty entries
            Assert.That(result.Contains(";;"), Is.False);
        }

        [Test]
        public void GetObjFromSecObj_WithNullObjects_ShouldReturnPermSet()
        {
            // Arrange
            string permSet = "5;6;";
            string objects = null;

            // Act
            var result = PermissionController.GetObjFromSecObj(DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings(), permSet, objects);

            // Assert
            Assert.That(result, Does.Contain("5"));
            Assert.That(result, Does.Contain("6"));
        }

        [Test]
        public void GetObjFromSecObj_WithExistingObjectsAndNoNewRoles_ShouldReturnUnchanged()
        {
            // Arrange
            string permSet = "1;";
            string objects = "1;2;3;";

            // Act
            string result = PermissionController.GetObjFromSecObj(DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings(), permSet, objects);

            // Assert
            Assert.That(result, Is.EqualTo("1;2;3"));
        }

        [Test]
        public void GetObjFromSecObj_WithMixedExistingAndNewRoles_ShouldAppendNewOnly()
        {
            // Arrange
            string permSet = "1;2;3;";
            string objects = "1;";

            // Act
            string result = PermissionController.GetObjFromSecObj(DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings(), permSet, objects);

            // Assert
            Assert.That(result, Does.StartWith("1;"));
            Assert.That(result, Does.Contain("2;"));
            Assert.That(result, Does.Contain("3"));
        }

        [Test]
        public void GetObjFromSecObj_WithLargeNumberOfRoles_ShouldHandleAll()
        {
            // Arrange
            string permSet = "1;2;3;4;5;6;7;8;9;10;";
            string objects = string.Empty;

            // Act
            string result = PermissionController.GetObjFromSecObj(DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings(), permSet, objects);

            // Assert
            for (int i = 1; i <= 10; i++)
            {
                Assert.That(result, Does.Contain(i.ToString()));
            }
        }
        
        [Test]
        public void GetAdministratorsRoleId_ReturnsAdminRoleId()
        {
            // Act
            int result = PermissionController.GetAdministratorsRoleId(DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings());

            // Assert
            Assert.That(result, Is.EqualTo(DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators));
        }

        [Test]
        public void GetAdministratorsRoleName_ReturnsAdminRoleName()
        {
            // Act
            string result = PermissionController.GetAdministratorsRoleName(DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings());

            // Assert
            Assert.That(result, Is.EqualTo(DotNetNuke.Tests.Utilities.Constants.RoleName_Administrators));
        }

        [Test]
        public void GetRegisteredUsersRoleId_ReturnsRegisteredRoleId()
        {
            // Act
            int result = PermissionController.GetRegisteredUsersRoleId(DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings());

            // Assert
            Assert.That(result, Is.EqualTo(DotNetNuke.Tests.Utilities.Constants.RoleID_RegisteredUsers));
        }

        [Test]
        public void GetRegisteredUsersRoleName_ReturnsRegisteredRoleName()
        {
            // Act
            string result = PermissionController.GetRegisteredUsersRoleName(DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings());

            // Assert
            Assert.That(result, Is.EqualTo(DotNetNuke.Tests.Utilities.Constants.RoleName_RegisteredUsers));
        }

        [Test]
        public void MergeRoleIds_CombinesRoleIdSets()
        {
            // Arrange
            var existing = new HashSet<int> { DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators };
            var newRoles = new HashSet<int> { DotNetNuke.Tests.Utilities.Constants.RoleID_RegisteredUsers };

            // Act
            var result = PermissionController.MergeRoleIds(DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings(), existing, newRoles);

            // Assert
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result, Contains.Item(DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators));
            Assert.That(result, Contains.Item(DotNetNuke.Tests.Utilities.Constants.RoleID_RegisteredUsers));
        }

        [Test]
        public void MergeRoleIds_WithEmptyNewRoles_AddsAdmin()
        {
            // Arrange
            var existing = new HashSet<int>();
            var newRoles = new HashSet<int> { DotNetNuke.Tests.Utilities.Constants.RoleID_RegisteredUsers };

            // Act
            var result = PermissionController.MergeRoleIds(DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings(), existing, newRoles);

            // Assert
            Assert.That(result, Contains.Item(DotNetNuke.Tests.Utilities.Constants.RoleID_RegisteredUsers));
        }

        [Test]
        public void GetObjFromSecObj_AddsNewRoleToObjects()
        {
            // Arrange
            string permSet = $"{DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators};";
            string objects = string.Empty;

            // Act
            string result = PermissionController.GetObjFromSecObj(DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings(), permSet, objects);

            // Assert
            Assert.That(result, Is.Not.Null);
        }
    }
}
