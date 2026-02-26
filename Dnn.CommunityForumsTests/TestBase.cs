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

using DotNetNuke.Modules.ActiveForumsTests.Services.Tokens;

namespace DotNetNuke.Modules.ActiveForumsTests
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Modules.ActiveForums;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Services.Tokens;
    using DotNetNuke.Tests.Utilities.Mocks;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;

    public class TestBase
    {
        private Mock<CachingProvider> mockCacheProvider;
        private Mock<IPortalController> portalController;
        private Mock<IPortalAliasController> portalAliasController;
        private Mock<IPortalAliasService> portalAliasService;
        private Mock<RoleProvider> mockRoleProvider;
        private Mock<IModuleController> moduleController;
        private Mock<IUserController> userController;
        private Mock<IRoleController> roleController;
        private Mock<IHostController> mockHostController;

        internal string DefaultPortalAlias = "localhost/en-us";

        internal Mock<DotNetNuke.Entities.Modules.ModuleInfo> MockModule;

        internal Mock<DotNetNuke.Modules.ActiveForums.ModuleSettings> MainSettings;

        [SetUp]
        public void SetUp()
        {
            var serviceCollection = new ServiceCollection();
            var mockApplicationStatusInfo = new Mock<IApplicationStatusInfo>();
            mockApplicationStatusInfo.Setup(info => info.Status).Returns(UpgradeStatus.Install);
            serviceCollection.AddTransient<IApplicationStatusInfo>(container => Mock.Of<IApplicationStatusInfo>());
            serviceCollection.AddTransient<INavigationManager>(container => Mock.Of<INavigationManager>());
            serviceCollection.AddTransient<IHostSettingsService, HostController>();

            serviceCollection.AddSingleton<IPortalAliasService, PortalAliasController>();
            serviceCollection.AddSingleton<IPortalAliasController, PortalAliasController>();

            ComponentFactory.Container = new SimpleContainer();
            this.mockRoleProvider = MockComponentProvider.CreateRoleProvider();

            this.mockCacheProvider = MockComponentProvider.CreateDataCacheProvider();
            this.SetupCachingProvider();

            // this is needed to mock the TokenProvider when running unit tests
            ComponentFactory.RegisterComponentInstance<TokenProvider>(new ForumsModuleTokenProvider());

            MockComponentProvider.CreateNew<LoggingProvider>();
            MockComponentProvider.CreateEventLogController();

            this.MainSettings = new Mock<DotNetNuke.Modules.ActiveForums.ModuleSettings>();
            this.SetupMainSettings();

            this.mockHostController = new Mock<IHostController>();
            DotNetNuke.Entities.Controllers.HostController.RegisterInstance(this.mockHostController.Object);

            this.portalAliasController = new Mock<IPortalAliasController>();
            DotNetNuke.Entities.Portals.PortalAliasController.SetTestableInstance(this.portalAliasController.Object);
            this.SetupPortalAliasSettings();

            this.portalController = new Mock<IPortalController>();
            DotNetNuke.Entities.Portals.PortalController.SetTestableInstance(this.portalController.Object);
            this.SetupPortalSettings();

            this.moduleController = new Mock<IModuleController>();
            ModuleController.SetTestableInstance(this.moduleController.Object);
            this.SetupModuleInfo();

            this.userController = new Mock<IUserController>();
            DotNetNuke.Entities.Users.UserController.SetTestableInstance(this.userController.Object);
            this.SetupUserInfo();

            this.roleController = new Mock<IRoleController>();
            DotNetNuke.Security.Roles.RoleController.SetTestableInstance(this.roleController.Object);
            this.SetupRoleInfo();

            this.SetupUserRoleInfo();

            this.SetupRoleProvider();
        }

        [TearDown]
        public void TearDown()
        {
            DotNetNuke.Entities.Portals.PortalAliasController.ClearInstance();
            DotNetNuke.Entities.Portals.PortalController.ClearInstance();
            DotNetNuke.Entities.Modules.ModuleController.ClearInstance();
            DotNetNuke.Entities.Users.UserController.ClearInstance();
            DotNetNuke.Security.Roles.RoleController.ClearInstance();
            ComponentFactory.Container = null;
        }

        private void SetupRoleProvider()
        {
            var adminRoleInfoForAdministrators = new UserRoleInfo { PortalID = DotNetNuke.Tests.Utilities.Constants.PORTAL_Zero, RoleName = DotNetNuke.Tests.Utilities.Constants.RoleName_Administrators, RoleID = DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators, UserID = DotNetNuke.Tests.Utilities.Constants.UserID_Admin };
            var adminRoleInfoforRegisteredUsers = new UserRoleInfo { PortalID = DotNetNuke.Tests.Utilities.Constants.PORTAL_Zero, RoleName = DotNetNuke.Tests.Utilities.Constants.RoleName_RegisteredUsers, RoleID = DotNetNuke.Tests.Utilities.Constants.RoleID_RegisteredUsers, UserID = DotNetNuke.Tests.Utilities.Constants.UserID_Admin };
            var adminRoleInfoforAllUsers = new UserRoleInfo { PortalID = DotNetNuke.Tests.Utilities.Constants.PORTAL_Zero, RoleName = DotNetNuke.Common.Globals.glbRoleAllUsersName, RoleID = Convert.ToInt32(DotNetNuke.Common.Globals.glbRoleAllUsers), UserID = DotNetNuke.Tests.Utilities.Constants.UserID_Admin };
            var adminRoleInfoforAnonymousUsers = new UserRoleInfo { PortalID = DotNetNuke.Tests.Utilities.Constants.PORTAL_Zero, RoleName = DotNetNuke.Common.Globals.glbRoleUnauthUserName, RoleID = Convert.ToInt32(DotNetNuke.Common.Globals.glbRoleUnauthUser), UserID = DotNetNuke.Tests.Utilities.Constants.UserID_Admin };
            this.mockRoleProvider.Setup(rp => rp.GetUserRoles(It.Is<UserInfo>(u => u.UserID == DotNetNuke.Tests.Utilities.Constants.UserID_Admin), It.IsAny<bool>())).Returns(new List<UserRoleInfo> { adminRoleInfoForAdministrators, adminRoleInfoforRegisteredUsers, adminRoleInfoforAllUsers, adminRoleInfoforAnonymousUsers });

            var user10RoleInfoforRegisteredUsers = new UserRoleInfo { PortalID = DotNetNuke.Tests.Utilities.Constants.PORTAL_Zero, RoleName = DotNetNuke.Tests.Utilities.Constants.RoleName_RegisteredUsers, RoleID = DotNetNuke.Tests.Utilities.Constants.RoleID_RegisteredUsers, UserID = DotNetNuke.Tests.Utilities.Constants.USER_TenId };
            var user10RoleInfoforAllUsers = new UserRoleInfo { PortalID = DotNetNuke.Tests.Utilities.Constants.PORTAL_Zero, RoleName = DotNetNuke.Common.Globals.glbRoleAllUsersName, RoleID = Convert.ToInt32(DotNetNuke.Common.Globals.glbRoleAllUsers), UserID = DotNetNuke.Tests.Utilities.Constants.USER_TenId };
            var user10RoleInfoforAnonymousUsers = new UserRoleInfo { PortalID = DotNetNuke.Tests.Utilities.Constants.PORTAL_Zero, RoleName = DotNetNuke.Common.Globals.glbRoleUnauthUserName, RoleID = Convert.ToInt32(DotNetNuke.Common.Globals.glbRoleUnauthUser), UserID = DotNetNuke.Tests.Utilities.Constants.USER_TenId };
            this.mockRoleProvider.Setup(rp => rp.GetUserRoles(It.Is<UserInfo>(u => u.UserID == DotNetNuke.Tests.Utilities.Constants.USER_TenId), It.IsAny<bool>())).Returns(new List<UserRoleInfo> { user10RoleInfoforRegisteredUsers, user10RoleInfoforAllUsers, user10RoleInfoforAnonymousUsers });

            var user12RoleInfoforRegisteredUsers = new UserRoleInfo { PortalID = DotNetNuke.Tests.Utilities.Constants.PORTAL_Zero, RoleName = DotNetNuke.Tests.Utilities.Constants.RoleName_RegisteredUsers, RoleID = DotNetNuke.Tests.Utilities.Constants.RoleID_RegisteredUsers, UserID = DotNetNuke.Tests.Utilities.Constants.UserID_User12 };
            var user12RoleInfoforAllUsers = new UserRoleInfo { PortalID = DotNetNuke.Tests.Utilities.Constants.PORTAL_Zero, RoleName = DotNetNuke.Common.Globals.glbRoleAllUsersName, RoleID = Convert.ToInt32(DotNetNuke.Common.Globals.glbRoleAllUsers), UserID = DotNetNuke.Tests.Utilities.Constants.UserID_User12 };
            var user12RoleInfoforAnonymousUsers = new UserRoleInfo { PortalID = DotNetNuke.Tests.Utilities.Constants.PORTAL_Zero, RoleName = DotNetNuke.Common.Globals.glbRoleUnauthUserName, RoleID = Convert.ToInt32(DotNetNuke.Common.Globals.glbRoleUnauthUser), UserID = DotNetNuke.Tests.Utilities.Constants.USER_TenId };
            this.mockRoleProvider.Setup(rp => rp.GetUserRoles(It.Is<UserInfo>(u => u.UserID == DotNetNuke.Tests.Utilities.Constants.UserID_User12), It.IsAny<bool>())).Returns(new List<UserRoleInfo> { user12RoleInfoforRegisteredUsers, user12RoleInfoforAllUsers, user12RoleInfoforAnonymousUsers });

            var userFirstSocialGroupOwner = new UserRoleInfo { PortalID = DotNetNuke.Tests.Utilities.Constants.PORTAL_Zero, RoleName = DotNetNuke.Tests.Utilities.Constants.RoleName_FirstSocialGroup, RoleID = DotNetNuke.Tests.Utilities.Constants.RoleID_FirstSocialGroup, UserID = DotNetNuke.Tests.Utilities.Constants.UserID_FirstSocialGroupOwner, IsOwner = true };
            this.mockRoleProvider.Setup(rp => rp.GetUserRoles(It.Is<UserInfo>(u => u.UserID == DotNetNuke.Tests.Utilities.Constants.UserID_FirstSocialGroupOwner), It.IsAny<bool>())).Returns(new List<UserRoleInfo> { userFirstSocialGroupOwner });

            this.mockRoleProvider.Setup(rp => rp.GetUserRoles(It.Is<UserInfo>(u => u.UserID == DotNetNuke.Tests.Utilities.Constants.USER_AnonymousUserId), It.IsAny<bool>())).Returns(new List<UserRoleInfo> { });
        }

        private void SetupUserInfo()
        {
            var adminUserInfo = new UserInfo
            {
                PortalID = DotNetNuke.Tests.Utilities.Constants.PORTAL_Zero,
                UserID = DotNetNuke.Tests.Utilities.Constants.UserID_Admin,
                Username = DotNetNuke.Tests.Utilities.Constants.UserName_Admin,
                DisplayName = DotNetNuke.Tests.Utilities.Constants.UserDisplayName_Admin,
            };
            this.userController.Setup(uc => uc.GetUser(It.Is<int>(u => u == DotNetNuke.Tests.Utilities.Constants.UserID_Admin), It.IsAny<int>())).Returns(adminUserInfo);
            this.userController.Setup(uc => uc.GetUserById(It.IsAny<int>(), It.Is<int>(u => u == DotNetNuke.Tests.Utilities.Constants.UserID_Admin))).Returns(adminUserInfo);

            var user10Info = new UserInfo
            {
                PortalID = DotNetNuke.Tests.Utilities.Constants.PORTAL_Zero,
                UserID = DotNetNuke.Tests.Utilities.Constants.USER_TenId,
                Username = DotNetNuke.Tests.Utilities.Constants.USER_TenName,
                DisplayName = DotNetNuke.Tests.Utilities.Constants.USER_TenName,
            };
            this.userController.Setup(uc => uc.GetUser(It.Is<int>(u => u == DotNetNuke.Tests.Utilities.Constants.USER_TenId), It.IsAny<int>())).Returns(user10Info);
            this.userController.Setup(uc => uc.GetUserById(It.IsAny<int>(), It.Is<int>(u => u == DotNetNuke.Tests.Utilities.Constants.USER_TenId))).Returns(user10Info);

            var user12Info = new UserInfo
            {
                PortalID = DotNetNuke.Tests.Utilities.Constants.PORTAL_Zero,
                UserID = DotNetNuke.Tests.Utilities.Constants.UserID_User12,
                Username = DotNetNuke.Tests.Utilities.Constants.UserName_User12,
                DisplayName = DotNetNuke.Tests.Utilities.Constants.UserDisplayName_User12,
                Profile = new UserProfile
                {
                    PreferredLocale = "en-US",
                },
            };
            this.userController.Setup(uc => uc.GetUser(It.Is<int>(u => u == DotNetNuke.Tests.Utilities.Constants.UserID_User12), It.IsAny<int>())).Returns(user12Info);
            this.userController.Setup(uc => uc.GetUserById(It.IsAny<int>(), It.Is<int>(u => u == DotNetNuke.Tests.Utilities.Constants.UserID_User12))).Returns(user12Info);

            var anonUserInfo = new UserInfo
            {
                PortalID = DotNetNuke.Tests.Utilities.Constants.PORTAL_Zero,
                UserID = DotNetNuke.Tests.Utilities.Constants.USER_AnonymousUserId,
                Username = "Guest",
                DisplayName = "Unauthenticated User",
                Profile = new UserProfile
                {
                    PreferredLocale = "en-US",
                },
            };
            this.userController.Setup(uc => uc.GetUser(It.Is<int>(u => u == DotNetNuke.Tests.Utilities.Constants.USER_AnonymousUserId), It.IsAny<int>())).Returns(anonUserInfo);
            this.userController.Setup(uc => uc.GetUserById(It.IsAny<int>(), It.Is<int>(u => u == DotNetNuke.Tests.Utilities.Constants.USER_AnonymousUserId))).Returns(anonUserInfo);
        }

        private void SetupRoleInfo()
        {
            var roles = new System.Collections.Generic.List<DotNetNuke.Security.Roles.RoleInfo>()
            {
                new RoleInfo()
                {
                    RoleID = DotNetNuke.Tests.Utilities.Constants.RoleID_RegisteredUsers,
                    RoleName = DotNetNuke.Tests.Utilities.Constants.RoleName_RegisteredUsers,
                    Status = RoleStatus.Approved,
                    PortalID = this.portalController.Object.GetCurrentPortalSettings().PortalId,
                },
                new RoleInfo()
                {
                    RoleID = DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators,
                    RoleName = DotNetNuke.Tests.Utilities.Constants.RoleName_Administrators,
                    Status = RoleStatus.Approved,
                    PortalID = this.portalController.Object.GetCurrentPortalSettings().PortalId,
                },
                new RoleInfo()
                {
                    RoleID = Convert.ToInt32(DotNetNuke.Common.Globals.glbRoleUnauthUser),
                    RoleName = DotNetNuke.Common.Globals.glbRoleUnauthUserName,
                    Status = RoleStatus.Approved,
                    PortalID = this.portalController.Object.GetCurrentPortalSettings().PortalId,
                },
                new RoleInfo()
                {
                    RoleID = Convert.ToInt32(DotNetNuke.Common.Globals.glbRoleAllUsers),
                    RoleName = DotNetNuke.Common.Globals.glbRoleAllUsersName,
                    Status = RoleStatus.Approved,
                    PortalID = this.portalController.Object.GetCurrentPortalSettings().PortalId,
                },
            };
            this.roleController.Setup(rc => rc.GetRoles(It.IsAny<int>())).Returns(roles);
        }

        private void SetupUserRoleInfo()
        {
            var roles = new System.Collections.Generic.List<DotNetNuke.Entities.Users.UserRoleInfo>()
            {
                new UserRoleInfo()
                {
                    UserRoleID = 1,
                    RoleID = DotNetNuke.Tests.Utilities.Constants.RoleID_RegisteredUsers,
                    RoleName = DotNetNuke.Tests.Utilities.Constants.RoleName_RegisteredUsers,
                    Status = RoleStatus.Approved,
                    EffectiveDate = DotNetNuke.Common.Utilities.Null.NullDate,
                    ExpiryDate = DotNetNuke.Common.Utilities.Null.NullDate,
                    UserID = DotNetNuke.Tests.Utilities.Constants.UserID_User12,
                },
                new UserRoleInfo()
                {
                    UserRoleID = 2,
                    RoleID = Convert.ToInt32(DotNetNuke.Common.Globals.glbRoleAllUsers),
                    RoleName = DotNetNuke.Common.Globals.glbRoleAllUsersName,
                    Status = RoleStatus.Approved,
                    EffectiveDate = DotNetNuke.Common.Utilities.Null.NullDate,
                    ExpiryDate = DotNetNuke.Common.Utilities.Null.NullDate,
                    UserID = DotNetNuke.Tests.Utilities.Constants.UserID_User12,
                },
                new UserRoleInfo()
                {
                    UserRoleID = 2,
                    RoleID = DotNetNuke.Tests.Utilities.Constants.RoleID_RegisteredUsers,
                    RoleName = DotNetNuke.Tests.Utilities.Constants.RoleName_RegisteredUsers,
                    Status = RoleStatus.Approved,
                    EffectiveDate = DotNetNuke.Common.Utilities.Null.NullDate,
                    ExpiryDate = DotNetNuke.Common.Utilities.Null.NullDate,
                    UserID = DotNetNuke.Tests.Utilities.Constants.USER_TenId,
                },
                new UserRoleInfo()
                {
                    UserRoleID = 3,
                    RoleID = Convert.ToInt32(DotNetNuke.Common.Globals.glbRoleAllUsers),
                    RoleName = DotNetNuke.Common.Globals.glbRoleAllUsersName,
                    Status = RoleStatus.Approved,
                    EffectiveDate = DotNetNuke.Common.Utilities.Null.NullDate,
                    ExpiryDate = DotNetNuke.Common.Utilities.Null.NullDate,
                    UserID = DotNetNuke.Tests.Utilities.Constants.USER_TenId,
                },
                new UserRoleInfo()
                {
                    UserRoleID = 4,
                    RoleID = DotNetNuke.Tests.Utilities.Constants.RoleID_RegisteredUsers,
                    RoleName = DotNetNuke.Tests.Utilities.Constants.RoleName_RegisteredUsers,
                    Status = RoleStatus.Approved,
                    EffectiveDate = DotNetNuke.Common.Utilities.Null.NullDate,
                    ExpiryDate = DotNetNuke.Common.Utilities.Null.NullDate,
                    UserID = DotNetNuke.Tests.Utilities.Constants.UserID_Admin,
                },
                new UserRoleInfo()
                {
                    UserRoleID = 5,
                    RoleID = DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators,
                    RoleName = DotNetNuke.Tests.Utilities.Constants.RoleName_Administrators,
                    Status = RoleStatus.Approved,
                    EffectiveDate = DotNetNuke.Common.Utilities.Null.NullDate,
                    ExpiryDate = DotNetNuke.Common.Utilities.Null.NullDate,
                    UserID = DotNetNuke.Tests.Utilities.Constants.UserID_Admin,
                },
                new UserRoleInfo()
                {
                    UserRoleID = 6,
                    RoleID = Convert.ToInt32(DotNetNuke.Common.Globals.glbRoleUnauthUser),
                    RoleName = DotNetNuke.Common.Globals.glbRoleUnauthUserName,
                    Status = RoleStatus.Approved,
                    EffectiveDate = DotNetNuke.Common.Utilities.Null.NullDate,
                    ExpiryDate = DotNetNuke.Common.Utilities.Null.NullDate,
                    UserID = DotNetNuke.Tests.Utilities.Constants.UserID_Admin,
                },
                new UserRoleInfo()
                {
                    UserRoleID = 7,
                    RoleID = Convert.ToInt32(DotNetNuke.Common.Globals.glbRoleAllUsers),
                    RoleName = DotNetNuke.Common.Globals.glbRoleAllUsersName,
                    Status = RoleStatus.Approved,
                    EffectiveDate = DotNetNuke.Common.Utilities.Null.NullDate,
                    ExpiryDate = DotNetNuke.Common.Utilities.Null.NullDate,
                    UserID = DotNetNuke.Tests.Utilities.Constants.UserID_Admin,
                },
            };
            this.roleController.Setup(rc => rc.GetUserRoles(It.Is<UserInfo>(u => u.UserID == DotNetNuke.Tests.Utilities.Constants.USER_TenId), true)).Returns(roles.Where(r => r.UserID == DotNetNuke.Tests.Utilities.Constants.USER_TenId).ToList());
            this.roleController.Setup(rc => rc.GetUserRoles(It.Is<UserInfo>(u => u.UserID == DotNetNuke.Tests.Utilities.Constants.UserID_User12), true)).Returns(roles.Where(r => r.UserID == DotNetNuke.Tests.Utilities.Constants.UserID_User12).ToList());
            this.roleController.Setup(rc => rc.GetUserRoles(It.Is<UserInfo>(u => u.UserID == DotNetNuke.Tests.Utilities.Constants.UserID_Admin), true)).Returns(roles.Where(r => r.UserID == DotNetNuke.Tests.Utilities.Constants.UserID_Admin).ToList());
            this.roleController.Setup(rc => rc.GetUserRoles(It.Is<UserInfo>(u => u.UserID == DotNetNuke.Tests.Utilities.Constants.USER_AnonymousUserId), true)).Returns(roles.Where(r => r.UserID == DotNetNuke.Tests.Utilities.Constants.USER_AnonymousUserId).ToList());
        }

        private void SetupPortalSettings()
        {
            var portalAliasInfo = new PortalAliasInfo
            {
                PortalID = DotNetNuke.Tests.Utilities.Constants.PORTAL_Zero,
                HTTPAlias = "localhost",
                IsPrimary = true,
                CultureCode = "en-US",
            };
            var portalAliases = new List<PortalAliasInfo> { portalAliasInfo };
            var portalSettings = new PortalSettings
            {
                PortalId = DotNetNuke.Tests.Utilities.Constants.PORTAL_Zero,
                AdministratorRoleId = DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators,
                AdministratorRoleName = DotNetNuke.Tests.Utilities.Constants.RoleName_Administrators,
                RegisteredRoleId = DotNetNuke.Tests.Utilities.Constants.RoleID_RegisteredUsers,
                RegisteredRoleName = DotNetNuke.Tests.Utilities.Constants.RoleName_RegisteredUsers,
                PortalAlias = portalAliasInfo,
                CultureCode = "en-US",
            };

            this.portalController.Setup(c => c.GetPortalSettings(It.IsAny<int>())).Returns(new Dictionary<string, string>());
            this.portalController.Setup(pc => pc.GetCurrentPortalSettings()).Returns(portalSettings);
        }

        private void SetupPortalAliasSettings()
        {
            var portalAliasInfo = new PortalAliasInfo
            {
                PortalID = DotNetNuke.Tests.Utilities.Constants.PORTAL_Zero,
                HTTPAlias = "localhost",
                IsPrimary = true,
                CultureCode = "en-US",
            };
            var portalAliases = new List<PortalAliasInfo> { portalAliasInfo };

            this.portalAliasController.Setup(c => c.GetPortalAliasesByPortalId(It.IsAny<int>())).Returns(portalAliases);
        }

        private void SetupCachingProvider()
        {
            this.mockCacheProvider.Setup(c => c.GetItem(It.IsAny<string>())).Returns<string>(key =>
            {
                if (key.Contains("Portal-1_"))
                {
                    var portals = new List<PortalInfo>();
                    portals.Add(new PortalInfo() { PortalID = 0 });

                    return portals;
                }
                else if (key.Contains("PortalGroups"))
                {
                    return new List<PortalGroupInfo>();
                }

                return null;
            });
        }

        private void SetupModuleInfo()
        {
            this.MockModule = new Mock<DotNetNuke.Entities.Modules.ModuleInfo>
            {
                Object =
                {
                    ModuleID = 1,
                    PortalID = this.portalController.Object.GetCurrentPortalSettings().PortalId,
                    ModuleTitle = "Test Module",

                },

            };

            this.moduleController.Setup(mc => mc.GetModule(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).Returns(this.MockModule.Object);
        }

        private void SetupMainSettings()
        {
            this.MainSettings.Object.MainSettings = new Hashtable
            {
                { SettingKeys.AvatarRefresh, "DISABLED" },
                { SettingKeys.AllowSignatures, true },
                { SettingKeys.AllowSubscribe, true },
                { SettingKeys.AnswerPointValue, 1 },
                { SettingKeys.DateFormatString, "dd MMM yyyy" },
                { SettingKeys.DeleteBehavior, 1 },
                { SettingKeys.EditInterval, 20 },
                { SettingKeys.EnableAutoLink, true },
                { SettingKeys.EnablePoints, true },
                { SettingKeys.FloodInterval, 10 },
                { SettingKeys.MarkAnswerPointValue, 1 },
                { SettingKeys.ModPointValue, 1 },
                { SettingKeys.PageSize, 20 },
                { SettingKeys.ReplyPointValue, 1 },
                { SettingKeys.Theme, "community-default" },
                { SettingKeys.TimeFormatString, "hh:mm tt" },
                { SettingKeys.TopicPointValue, 1 },
                { SettingKeys.UserNameDisplay, "Displayname" },
                { SettingKeys.UseShortUrls, true },
                { SettingKeys.UseSkinBreadCrumb, true },
            };
        }
    }
}
