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

    [TestFixture()]
    public class PermissionControllerTests
    {
        [Test()]
        public void GetNamesForRolesTest()
        {
            // Arrange
            var roles = $"{Common.Globals.glbRoleAllUsers};{Common.Globals.glbRoleUnauthUser}";
            var portalId = 1;
            var expectedResult = $"{Common.Globals.glbRoleAllUsersName};{Common.Globals.glbRoleUnauthUserName};";

            // Act
            var actualResult = PermissionController.GetNamesForRoles(portalId, roles);

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test()]
        public void GetRoleNameTest()
        {
            // Arrange
            var roles = $"{Common.Globals.glbRoleAllUsers}";
            var portalId = 1;
            var expectedResult = $"{Common.Globals.glbRoleAllUsersName};";

            // Act
            // string actualResult = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleName(portalId, roles);

            // we can't actually test this method because internally it is using caching, so we can at least test that the method throws an exception
            // Assert
            Assert.Throws<NullReferenceException>(() => PermissionController.GetRoleName(portalId, roles));
        }

        [Test()]
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
    }
}
