using NUnit.Framework;
using DotNetNuke.Modules.ActiveForums.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;

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
            string expectedResult = $"{DotNetNuke.Common.Globals.glbRoleAllUsersName};{DotNetNuke.Common.Globals.glbRoleUnauthUserName}";
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
            string expectedResult = $"{DotNetNuke.Common.Globals.glbRoleAllUsersName}";
            //Act
            string actualResult = DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.GetRoleName(portalId, roles);
            //Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test()]
        public void GetRoleIdsTest()
        {
            Assert.Fail();
        }
    }
}