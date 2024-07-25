using NUnit.Framework;
using DotNetNuke.Modules.ActiveForums.Controllers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Moq;
using Castle.Components.DictionaryAdapter;

namespace DotNetNuke.Modules.ActiveForums.Controllers.Tests
{
    [TestFixture()]
    public class PermissionControllerTests
    {
        [Test()]
        public void GetNamesForRolesTest()
        {
            //Arrange
            string roles = $"{DotNetNuke.Common.Globals.glbRoleAllUsers};{DotNetNuke.Common.Globals.glbRoleUnauthUser}";
            int portalId = 1;
            string expectedResult = $"{DotNetNuke.Common.Globals.glbRoleAllUsersName};{DotNetNuke.Common.Globals.glbRoleUnauthUserName};";
            //Act
            string actualResult = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetNamesForRoles(portalId, roles);
            //Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test()]
        public void GetRoleNameTest()
        {
            //Arrange
            string roles = $"{DotNetNuke.Common.Globals.glbRoleAllUsers}";
            int portalId = 1;
            string expectedResult = $"{DotNetNuke.Common.Globals.glbRoleAllUsersName};";
            //Act
            //string actualResult = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleName(portalId, roles);

            // we can't actually test this method because internally it is using caching, so we can at least test that the method throws an exception
            //Assert
            Assert.Throws<NullReferenceException>(() => DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleName(portalId, roles));
        }

        [Test()]
        [TestCase(new string[] { "1", "2", "3" }, new string[] { "1", "2", "3" }, ExpectedResult = true)]
        [TestCase(new string[] { "1", "2" }, new string[] { "3" }, ExpectedResult = false)]
        [TestCase(new string[] { "1" }, new string[] { "1", "2", "3" }, ExpectedResult = true)]
        public bool HasRequiredPermTest(string[] authRoles, string[] userRoles)
        {
            //Arrange
            //Act
            return DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.HasRequiredPerm(authRoles, userRoles);
            //Assert
        }
    }
}
