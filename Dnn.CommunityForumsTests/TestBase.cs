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
    using System.Drawing.Text;
    using System.Linq;
    using System.Reflection;
    using System.Web;
    using System.Web.Hosting;
    using System.Security.Cryptography;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Modules.ActiveForums;
    using DotNetNuke.Modules.ActiveForums.ViewModels;
    using DotNetNuke.Modules.ActiveForumsTests.ObjectGraphs;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Services.Tokens;
    using DotNetNuke.Tests.Utilities.Mocks;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;
    using DotNetNuke.Services.Url.FriendlyUrl;

    public class TestBase
    {
        internal Mock<CachingProvider> mockCacheProvider;
        internal Mock<IPortalController> mockPortalController;
        internal Mock<IPortalAliasController> mockPortalAliasController;
        internal Mock<IPortalAliasService> mockPortalAliasService;
        internal Mock<RoleProvider> mockRoleProvider;
        internal Mock<ILocaleController> mockLocaleController;
        internal Mock<IModuleController> mockModuleController;
        internal Mock<IUserController> mockUserController;
        internal Mock<IRoleController> mockRoleController;
        internal Mock<IHostController> mockHostController;
        internal Mock<IHostSettingsService> mockHostSettings;
        internal Mock<PortalAliasInfo> mockPortalAliasInfo;
        internal Mock<List<IPortalAliasInfo>> mockPortalAliases;
        internal Mock<IPortalSettings> mockIPortalSettings;
        internal Mock<PortalSettings> mockPortalSettings;
        internal Mock<IPortalInfo> mockIPortalInfo;
        internal Mock<PortalInfo> mockPortalInfo;
        internal Mock<ITabController> mockTabController;
        internal Mock<FriendlyUrlProvider> mockFriendlyUrlProvider;

        internal string DefaultPortalAlias = "example.com/en-us";
        internal string DefaultSite = "https://example.com";
        internal Mock<DotNetNuke.Modules.ActiveForums.Controllers.IForumController> mockForumController;
        internal Mock<DotNetNuke.Modules.ActiveForums.Controllers.IForumGroupController> mockForumGroupController;
        internal Mock<DotNetNuke.Modules.ActiveForums.Controllers.ITagController> mockTagController;
        internal Mock<DotNetNuke.Modules.ActiveForums.Controllers.ICategoryController> mockCategoryController;
        internal Mock<DotNetNuke.Modules.ActiveForums.Controllers.ITopicTagController> mockTopicTagController;

        internal Mock<DotNetNuke.Entities.Modules.ModuleInfo> MockModule;
        internal Mock<DotNetNuke.Entities.Tabs.TabInfo> MockTab;

        internal Mock<DotNetNuke.Modules.ActiveForums.Entities.ForumInfo> MockForum;
        internal Mock<DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo> MockForumGroup;
        internal Mock<DotNetNuke.Modules.ActiveForums.Entities.TopicInfo> MockTopic;

        internal Mock<DotNetNuke.Modules.ActiveForums.Controllers.TopicController> MockTopicController;
        internal Mock<DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController> MockForumUserController;

        internal Mock<Modules.ActiveForums.Services.Tests.NavigationManager> MockNavigationManager;

        internal Mock<System.Web.HttpApplication> MockHttpApplication;

        internal Mock<DotNetNuke.Modules.ActiveForums.ModuleSettings> MainSettings;

        // DNN UserInfo instances — shared between userController mock and ForumUserGraph
        internal UserInfo AdminUserInfo;
        internal UserInfo User10Info;
        internal UserInfo User12Info;
        internal UserInfo AnonUserInfo;
        internal UserInfo ModeratorUserInfo;

        internal List<DotNetNuke.Modules.ActiveForums.Entities.ForumInfo> ForumsGraph;
        internal List<DotNetNuke.Modules.ActiveForums.Entities.ForumUserInfo> ForumUserGraph;
        internal List<DotNetNuke.Modules.ActiveForums.Entities.TopicInfo> TopicReplyGraph;
        internal List<DotNetNuke.Modules.ActiveForums.Entities.TagInfo> TagsGraph;
        internal List<DotNetNuke.Modules.ActiveForums.Entities.CategoryInfo> CategoriesGraph;

        [SetUp]
        public void SetUp()
        {
            var serviceCollection = new ServiceCollection();
            var mockApplicationStatusInfo = new Mock<IApplicationStatusInfo>();
            mockApplicationStatusInfo.Setup(info => info.Status).Returns(UpgradeStatus.Install);

            MockComponentProvider.CreateNew<LoggingProvider>();
            MockComponentProvider.CreateEventLogController();

            serviceCollection.AddTransient<IApplicationStatusInfo>(container => Mock.Of<IApplicationStatusInfo>());
            serviceCollection.AddTransient<INavigationManager>(container => Mock.Of<INavigationManager>());
            serviceCollection.AddTransient<IHostSettingsService, HostController>();
            serviceCollection.AddTransient<IHostController, HostController>();
            serviceCollection.AddSingleton<IPortalAliasService, PortalAliasController>();
            serviceCollection.AddSingleton<IPortalAliasController, PortalAliasController>();
            serviceCollection.AddSingleton<IPortalController, PortalController>();

            serviceCollection.AddSingleton<DotNetNuke.Modules.ActiveForums.Controllers.IForumController, DotNetNuke.Modules.ActiveForums.Controllers.ForumController>();
            serviceCollection.AddSingleton<DotNetNuke.Modules.ActiveForums.Controllers.IForumGroupController, DotNetNuke.Modules.ActiveForums.Controllers.IForumGroupController>();

            ComponentFactory.Container = new SimpleContainer();
            this.mockRoleProvider = MockComponentProvider.CreateRoleProvider();
            this.mockCacheProvider = MockComponentProvider.CreateDataCacheProvider();

            // this is needed to mock the TokenProvider when running unit tests
            ComponentFactory.RegisterComponentInstance<TokenProvider>(new ForumsModuleTokenProvider());

            this.mockHostSettings = new Mock<IHostSettingsService>();
            ComponentFactory.RegisterComponentInstance<IHostSettingsService>(this.mockHostSettings.Object);

            this.mockHostController = new Mock<IHostController>();
            DotNetNuke.Entities.Controllers.HostController.RegisterInstance(this.mockHostController.Object);
            ComponentFactory.RegisterComponentInstance<HostController>(this.mockHostController.Object);

            this.mockLocaleController = new Mock<ILocaleController>();
            ComponentFactory.RegisterComponentInstance<ILocaleController>(this.mockLocaleController.Object);
            
            this.mockFriendlyUrlProvider = new Mock<FriendlyUrlProvider>();
            ComponentFactory.RegisterComponentInstance<FriendlyUrlProvider>(this.mockFriendlyUrlProvider.Object);


            this.mockPortalAliasController = new Mock<IPortalAliasController>();
            DotNetNuke.Entities.Portals.PortalAliasController.SetTestableInstance(this.mockPortalAliasController.Object);
            this.mockPortalAliasService = new Mock<IPortalAliasService>();

            this.mockPortalController = new Mock<IPortalController>();
            DotNetNuke.Entities.Portals.PortalController.SetTestableInstance(this.mockPortalController.Object);

            this.mockModuleController = new Mock<IModuleController>();
            DotNetNuke.Entities.Modules.ModuleController.SetTestableInstance(this.mockModuleController.Object);

            this.mockUserController = new Mock<IUserController>();
            DotNetNuke.Entities.Users.UserController.SetTestableInstance(this.mockUserController.Object);

            this.mockRoleController = new Mock<IRoleController>();
            DotNetNuke.Security.Roles.RoleController.SetTestableInstance(this.mockRoleController.Object);

            this.mockTabController = new Mock<ITabController>();
            DotNetNuke.Entities.Tabs.TabController.SetTestableInstance(this.mockTabController.Object);

            this.SetupHostSettings();
            this.SetupCachingProvider();
            this.SetupModuleInfo();
            this.SetupLocaleController();
            this.SetupUserInfo();
            this.SetupPortalAliasSettings();
            this.SetupTabInfo();
            this.SetupPortalSettings();
            this.SetupMockNavigationManager();

            this.SetupRoleInfo();
            this.SetupUserRoleInfo();
            this.SetupRoleProvider();

            this.SetupMockHttpApplication();

            this.MainSettings = new Mock<DotNetNuke.Modules.ActiveForums.ModuleSettings>();

            this.mockForumController = new Mock<DotNetNuke.Modules.ActiveForums.Controllers.IForumController>();
            DotNetNuke.Modules.ActiveForums.Controllers.ForumController.SetTestableInstance(this.mockForumController.Object);

            this.mockForumGroupController = new Mock<DotNetNuke.Modules.ActiveForums.Controllers.IForumGroupController>();
            DotNetNuke.Modules.ActiveForums.Controllers.ForumGroupController.SetTestableInstance(this.mockForumGroupController.Object);

            this.mockTagController = new Mock<DotNetNuke.Modules.ActiveForums.Controllers.ITagController>();
            DotNetNuke.Modules.ActiveForums.Controllers.TagController.SetTestableInstance(this.mockTagController.Object);

            this.mockTopicTagController = new Mock<DotNetNuke.Modules.ActiveForums.Controllers.ITopicTagController>();
            DotNetNuke.Modules.ActiveForums.Controllers.TopicTagController.SetTestableInstance(this.mockTopicTagController.Object);

            this.mockCategoryController = new Mock<DotNetNuke.Modules.ActiveForums.Controllers.ICategoryController>();
            DotNetNuke.Modules.ActiveForums.Controllers.CategoryController.SetTestableInstance(this.mockCategoryController.Object);

            this.SetupMainSettings();
            this.SetupForumInfo();
            this.SetupForumUserInfo();
            this.SetupTopicReplyInfo();
            this.SetupTagInfo();
            this.SetupCategoryInfo();
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
            var user12RoleInfoforAllUsers = new UserRoleInfo { PortalID = DotNetNuke.Tests.Utilities.Constants.PORTAL_Zero, RoleName = DotNetNuke.Tests.Utilities.Constants.RoleName_RegisteredUsers, RoleID = Convert.ToInt32(DotNetNuke.Common.Globals.glbRoleAllUsers), UserID = DotNetNuke.Tests.Utilities.Constants.UserID_User12 };
            var user12RoleInfoforAnonymousUsers = new UserRoleInfo { PortalID = DotNetNuke.Tests.Utilities.Constants.PORTAL_Zero, RoleName = DotNetNuke.Common.Globals.glbRoleUnauthUserName, RoleID = Convert.ToInt32(DotNetNuke.Common.Globals.glbRoleUnauthUser), UserID = DotNetNuke.Tests.Utilities.Constants.USER_TenId };
            this.mockRoleProvider.Setup(rp => rp.GetUserRoles(It.Is<UserInfo>(u => u.UserID == DotNetNuke.Tests.Utilities.Constants.UserID_User12), It.IsAny<bool>())).Returns(new List<UserRoleInfo> { user12RoleInfoforRegisteredUsers, user12RoleInfoforAllUsers, user12RoleInfoforAnonymousUsers });

            var userFirstSocialGroupOwner = new UserRoleInfo { PortalID = DotNetNuke.Tests.Utilities.Constants.PORTAL_Zero, RoleName = DotNetNuke.Tests.Utilities.Constants.RoleName_FirstSocialGroup, RoleID = DotNetNuke.Tests.Utilities.Constants.RoleID_FirstSocialGroup, UserID = DotNetNuke.Tests.Utilities.Constants.UserID_FirstSocialGroupOwner, IsOwner = true };
            this.mockRoleProvider.Setup(rp => rp.GetUserRoles(It.Is<UserInfo>(u => u.UserID == DotNetNuke.Tests.Utilities.Constants.UserID_FirstSocialGroupOwner), It.IsAny<bool>())).Returns(new List<UserRoleInfo> { userFirstSocialGroupOwner });

            this.mockRoleProvider.Setup(rp => rp.GetUserRoles(It.Is<UserInfo>(u => u.UserID == DotNetNuke.Tests.Utilities.Constants.USER_AnonymousUserId), It.IsAny<bool>())).Returns(new List<UserRoleInfo> { });
        }

        private void SetupUserInfo()
        {
            // Assign to fields so SetupForumUserInfo() can reuse the same instances
            this.AdminUserInfo = new UserInfo
            {
                PortalID = DotNetNuke.Tests.Utilities.Constants.PORTAL_Zero,
                UserID = DotNetNuke.Tests.Utilities.Constants.UserID_Admin,
                Username = DotNetNuke.Tests.Utilities.Constants.UserName_Admin,
                DisplayName = DotNetNuke.Tests.Utilities.Constants.UserDisplayName_Admin,
            };
            this.mockUserController.Setup(uc => uc.GetUser(It.Is<int>(u => u == DotNetNuke.Tests.Utilities.Constants.UserID_Admin), It.IsAny<int>())).Returns(this.AdminUserInfo);
            this.mockUserController.Setup(uc => uc.GetUserById(It.IsAny<int>(), It.Is<int>(u => u == DotNetNuke.Tests.Utilities.Constants.UserID_Admin))).Returns(this.AdminUserInfo);

            this.User10Info = new UserInfo
            {
                PortalID = DotNetNuke.Tests.Utilities.Constants.PORTAL_Zero,
                UserID = DotNetNuke.Tests.Utilities.Constants.USER_TenId,
                Username = DotNetNuke.Tests.Utilities.Constants.USER_TenName,
                DisplayName = DotNetNuke.Tests.Utilities.Constants.USER_TenName,
            };
            this.mockUserController.Setup(uc => uc.GetUser(It.Is<int>(u => u == DotNetNuke.Tests.Utilities.Constants.USER_TenId), It.IsAny<int>())).Returns(this.User10Info);
            this.mockUserController.Setup(uc => uc.GetUserById(It.IsAny<int>(), It.Is<int>(u => u == DotNetNuke.Tests.Utilities.Constants.USER_TenId))).Returns(this.User10Info);

            this.User12Info = new UserInfo
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
            this.mockUserController.Setup(uc => uc.GetUser(It.Is<int>(u => u == DotNetNuke.Tests.Utilities.Constants.UserID_User12), It.IsAny<int>())).Returns(this.User12Info);
            this.mockUserController.Setup(uc => uc.GetUserById(It.IsAny<int>(), It.Is<int>(u => u == DotNetNuke.Tests.Utilities.Constants.UserID_User12))).Returns(this.User12Info);

            this.AnonUserInfo = new UserInfo
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
            this.mockUserController.Setup(uc => uc.GetUser(It.Is<int>(u => u == DotNetNuke.Tests.Utilities.Constants.USER_AnonymousUserId), It.IsAny<int>())).Returns(this.AnonUserInfo);
            this.mockUserController.Setup(uc => uc.GetUserById(It.IsAny<int>(), It.Is<int>(u => u == DotNetNuke.Tests.Utilities.Constants.USER_AnonymousUserId))).Returns(this.AnonUserInfo);

            this.ModeratorUserInfo = new UserInfo
            {
                PortalID = DotNetNuke.Tests.Utilities.Constants.PORTAL_Zero,
                UserID = DotNetNuke.Tests.Utilities.Constants.UserID_FirstSocialGroupOwner,
                Username = DotNetNuke.Tests.Utilities.Constants.RoleName_FirstSocialGroup,
                DisplayName = "Forum Moderator",
            };
            this.mockUserController.Setup(uc => uc.GetUser(It.Is<int>(u => u == DotNetNuke.Tests.Utilities.Constants.UserID_FirstSocialGroupOwner), It.IsAny<int>())).Returns(this.ModeratorUserInfo);
            this.mockUserController.Setup(uc => uc.GetUserById(It.IsAny<int>(), It.Is<int>(u => u == DotNetNuke.Tests.Utilities.Constants.UserID_FirstSocialGroupOwner))).Returns(this.ModeratorUserInfo);
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
                    PortalID = DotNetNuke.Tests.Utilities.Constants.PORTAL_Zero,
                },
                new RoleInfo()
                {
                    RoleID = DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators,
                    RoleName = DotNetNuke.Tests.Utilities.Constants.RoleName_Administrators,
                    Status = RoleStatus.Approved,
                    PortalID = DotNetNuke.Tests.Utilities.Constants.PORTAL_Zero,
                },
                new RoleInfo()
                {
                    RoleID = Convert.ToInt32(DotNetNuke.Common.Globals.glbRoleUnauthUser),
                    RoleName = DotNetNuke.Common.Globals.glbRoleUnauthUserName,
                    Status = RoleStatus.Approved,
                    PortalID = DotNetNuke.Tests.Utilities.Constants.PORTAL_Zero,
                },
                new RoleInfo()
                {
                    RoleID = Convert.ToInt32(DotNetNuke.Common.Globals.glbRoleAllUsers),
                    RoleName = DotNetNuke.Common.Globals.glbRoleAllUsersName,
                    Status = RoleStatus.Approved,
                    PortalID = DotNetNuke.Tests.Utilities.Constants.PORTAL_Zero,
                },
            };
            this.mockRoleController.Setup(rc => rc.GetRoles(It.IsAny<int>())).Returns(roles);
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
            this.mockRoleController.Setup(rc => rc.GetUserRoles(It.Is<UserInfo>(u => u.UserID == DotNetNuke.Tests.Utilities.Constants.USER_TenId), true)).Returns(roles.Where(r => r.UserID == DotNetNuke.Tests.Utilities.Constants.USER_TenId).ToList());
            this.mockRoleController.Setup(rc => rc.GetUserRoles(It.Is<UserInfo>(u => u.UserID == DotNetNuke.Tests.Utilities.Constants.UserID_User12), true)).Returns(roles.Where(r => r.UserID == DotNetNuke.Tests.Utilities.Constants.UserID_User12).ToList());
            this.mockRoleController.Setup(rc => rc.GetUserRoles(It.Is<UserInfo>(u => u.UserID == DotNetNuke.Tests.Utilities.Constants.UserID_Admin), true)).Returns(roles.Where(r => r.UserID == DotNetNuke.Tests.Utilities.Constants.UserID_Admin).ToList());
            this.mockRoleController.Setup(rc => rc.GetUserRoles(It.Is<UserInfo>(u => u.UserID == DotNetNuke.Tests.Utilities.Constants.USER_AnonymousUserId), true)).Returns(roles.Where(r => r.UserID == DotNetNuke.Tests.Utilities.Constants.USER_AnonymousUserId).ToList());
        }

        private void SetupPortalSettings()
        {
            this.mockIPortalSettings = new Mock<IPortalSettings>()
            {
                Object =
                {
                    PortalId = DotNetNuke.Tests.Utilities.Constants.PORTAL_Zero,
                    PortalName = "Test Portal",
                    AdministratorRoleId = DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators,
                    AdministratorRoleName = DotNetNuke.Tests.Utilities.Constants.RoleName_Administrators,
                    RegisteredRoleId = DotNetNuke.Tests.Utilities.Constants.RoleID_RegisteredUsers,
                    RegisteredRoleName = DotNetNuke.Tests.Utilities.Constants.RoleName_RegisteredUsers,
                    CultureCode = "en-US",
                    Email = "mail@example.com",
                    Description = "Test Portal",
                },
            };

            this.mockPortalSettings = new Mock<PortalSettings>()
            {
                Object =
                {
                    PortalId = DotNetNuke.Tests.Utilities.Constants.PORTAL_Zero,
                    PortalName = "Test Portal",
                    AdministratorRoleId = DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators,
                    AdministratorRoleName = DotNetNuke.Tests.Utilities.Constants.RoleName_Administrators,
                    RegisteredRoleId = DotNetNuke.Tests.Utilities.Constants.RoleID_RegisteredUsers,
                    RegisteredRoleName = DotNetNuke.Tests.Utilities.Constants.RoleName_RegisteredUsers,
                    CultureCode = "en-US",
                    Email = "mail@example.com",
                    Description = "Test Portal",
                    ActiveTab = this.MockTab.Object,
                },
            };

            this.mockPortalAliasInfo = new Mock<PortalAliasInfo>()
            {
                Object =
                {
                    PortalID = DotNetNuke.Tests.Utilities.Constants.PORTAL_Zero,
                    HTTPAlias = "example.com",
                    IsPrimary = true,
                    CultureCode = "en-US",
                },
            };

            this.mockIPortalInfo = new Mock<IPortalInfo>
            { Object =
                {
                    PortalId = DotNetNuke.Tests.Utilities.Constants.PORTAL_Zero,
                    AdministratorId = DotNetNuke.Tests.Utilities.Constants.UserID_Admin,
                    AdministratorRoleId = DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators,
                    AdministratorRoleName = DotNetNuke.Tests.Utilities.Constants.RoleName_Administrators,
                    RegisteredRoleId = DotNetNuke.Tests.Utilities.Constants.RoleID_RegisteredUsers,
                    RegisteredRoleName = DotNetNuke.Tests.Utilities.Constants.RoleName_RegisteredUsers,
                    Description = "Test Portal",
                    CultureCode = "en-US",
                    PortalName = "Test Portal",
                },
            };

            this.mockPortalInfo = new Mock<PortalInfo>
            { Object =
                {
                    PortalID = DotNetNuke.Tests.Utilities.Constants.PORTAL_Zero,
                    AdministratorId = DotNetNuke.Tests.Utilities.Constants.UserID_Admin,
                    AdministratorRoleId = DotNetNuke.Tests.Utilities.Constants.RoleID_Administrators,
                    AdministratorRoleName = DotNetNuke.Tests.Utilities.Constants.RoleName_Administrators,
                    RegisteredRoleId = DotNetNuke.Tests.Utilities.Constants.RoleID_RegisteredUsers,
                    RegisteredRoleName = DotNetNuke.Tests.Utilities.Constants.RoleName_RegisteredUsers,
                    Description = "Test Portal",
                    CultureCode = "en-US",
                    PortalName = "Test Portal",
                },
            };
            this.mockPortalController.Setup(pc => pc.GetPortalSettings(It.IsAny<int>())).Returns(new Dictionary<string, string>());
            this.mockPortalController.Setup(pc => pc.GetCurrentPortalSettings()).Returns(this.mockPortalSettings.Object);
            this.mockPortalController.Setup(pc => pc.GetCurrentSettings()).Returns(this.mockIPortalSettings.Object);
            this.mockPortalController.Setup(pc => pc.GetPortal(It.IsAny<int>())).Returns(this.mockPortalInfo.Object);
            this.mockPortalController.Setup(pc => pc.GetPortal(It.IsAny<int>(), It.IsAny<string>())).Returns(this.mockPortalInfo.Object);
        }

        private void SetupLocaleController()
        {
            var locale = new Locale()
            {
                Code = "en-US",
                PortalId = DotNetNuke.Tests.Utilities.Constants.PORTAL_Zero,
                Text = "English (United States)",
            };

            this.mockLocaleController.Setup(lc => lc.GetLocales(It.IsAny<int>())).Returns(new Dictionary<string, Locale>() { { "en-US", locale } });
        }

        private void SetupHostSettings()
        {
            this.mockHostController.SetReturnsDefault(new Host());
            this.mockHostController.Setup(hc => hc.GetBoolean("UseFriendlyUrls")).Returns(true);
            this.mockHostController.Setup(hc => hc.GetBoolean("UseFriendlyUrls", It.IsAny<bool>())).Returns(true);
            this.mockHostController.Setup(hc => hc.GetSettingsDictionary()).Returns(new Dictionary<string, string>
            {
                { "hosttitle", "DNN Community" },
                { "UseFriendlyUrls", "true" },
            });

            this.mockHostSettings.Setup(hs => hs.GetBoolean("UseFriendlyUrls", It.IsAny<bool>())).Returns(true);
            this.mockHostSettings.Setup(hs => hs.GetSettingsDictionary()).Returns(new Dictionary<string, string>
            {
                { "hosttitle", "DNN Community" },
                { "UseFriendlyUrls", "true" },
            });
        }

        private void SetupPortalAliasSettings()
        {
            IPortalAliasInfo portalAlias = new PortalAliasInfo();
            portalAlias.PortalId = DotNetNuke.Tests.Utilities.Constants.PORTAL_Zero;
            portalAlias.HttpAlias = "example.com";
            portalAlias.IsPrimary = true;
            portalAlias.CultureCode = "en-US";

            IDictionary<string, IPortalAliasInfo> portalAliases = new Dictionary<string, IPortalAliasInfo>
            {
                { portalAlias.HttpAlias, portalAlias },
            };

            this.mockPortalAliasService.Setup(c => c.GetPortalAliases()).Returns(portalAliases);
            this.mockPortalAliasService.Setup(c => c.GetPortalAliasesByPortalId(It.IsAny<int>())).Returns(portalAliases.Values.AsEnumerable());

            var portalAliasInfoList = new List<PortalAliasInfo> { (PortalAliasInfo)portalAlias };
            this.mockPortalAliasService.Setup(pac => pac.GetPortalAliasesByPortalId(It.IsAny<int>())).Returns(portalAliasInfoList);
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
                    PortalID = DotNetNuke.Tests.Utilities.Constants.PORTAL_Zero,
                    ModuleTitle = "Test Module",
                    TabID = 1,
                },
            };

            this.mockModuleController.Setup(mc => mc.GetModule(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).Returns(this.MockModule.Object);
        }

        private void SetupTabInfo()
        {
            this.MockTab = new Mock<DotNetNuke.Entities.Tabs.TabInfo>
            {
                Object =
                {
                    ModuleID = 1,
                    PortalID = DotNetNuke.Tests.Utilities.Constants.PORTAL_Zero,
                    TabID = 1,
                },
            };

            this.mockTabController.Setup(tc => tc.GetTab(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).Returns(this.MockTab.Object);
            this.mockTabController.Setup(tc => tc.GetTabsByPortal(It.IsAny<int>())).Returns(new TabCollection() { this.MockTab.Object });
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

        private void SetupForumInfo()
        {
            var portalSettings = DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings();

            this.ForumsGraph = ForumsObjectGraph.BuildForumsGraph(portalSettings);

            foreach (var forum in this.ForumsGraph)
            {
                var capturedForum = forum;
                this.mockForumGroupController
                    .Setup(c => c.GetById(capturedForum.ForumGroupId, It.IsAny<int>()))
                    .Returns(capturedForum.ForumGroup);
                this.mockForumGroupController
                    .Setup(c => c.GetByUrlPrefix(It.IsAny<int>(), capturedForum.ForumGroup.PrefixURL))
                    .Returns(capturedForum.ForumGroup);
            }

            this.mockForumGroupController
                .Setup(c => c.GetById(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(this.ForumsGraph.FirstOrDefault().ForumGroup);

            foreach (var forum in this.ForumsGraph)
            {
                var capturedForum = forum;
                this.mockForumController
                    .Setup(fc => fc.GetById(It.IsAny<int>(), capturedForum.ForumID))
                    .Returns(capturedForum);
                this.mockForumController
                    .Setup(c => c.GetByUrlPrefix(It.IsAny<int>(), capturedForum.PrefixURL))
                    .Returns(capturedForum);
            }

            this.MockForumGroup = new Mock<DotNetNuke.Modules.ActiveForums.Entities.ForumGroupInfo>(this.ForumsGraph.First().ForumGroup);
            this.MockForum = new Mock<DotNetNuke.Modules.ActiveForums.Entities.ForumInfo>
            {
                Object =
                {
                    ForumID = this.ForumsGraph.First().ForumID,
                    ForumGroupId = this.ForumsGraph.First().ForumGroupId,
                    ForumGroup = this.ForumsGraph.First().ForumGroup,
                    PortalSettings = portalSettings,
                    ForumName = this.ForumsGraph.First().ForumName,
                    PrefixURL = this.ForumsGraph.First().PrefixURL,
                    TotalTopics = 5,
                    Security = new DotNetNuke.Modules.ActiveForums.Entities.PermissionInfo
                    {
                        View = DotNetNuke.Modules.ActiveForumsTests.ObjectGraphs.ForumsObjectGraph.PublicForumPermission,
                    },
                },
            };

            this.MockTopic = new Mock<DotNetNuke.Modules.ActiveForums.Entities.TopicInfo>
            {
                Object =
                {
                    ForumId = this.MockForum.Object.ForumID,
                    TopicId = 1,
                    Forum = this.MockForum.Object,
                    Content = new DotNetNuke.Modules.ActiveForums.Entities.ContentInfo
                    {
                        ContentId = 1,
                        ModuleId = 1,
                        Subject = "Test Topic",
                        Body = "Test Topic",
                    },
                    TopicUrl = "test-topic",
                },
            };

            this.MockTopicController = new Mock<DotNetNuke.Modules.ActiveForums.Controllers.TopicController>(-1);
            this.MockTopicController
                .Setup(tc => tc.GetById(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DotNetNuke.Modules.ActiveForums.Entities.ForumInfo>()))
                .Returns(this.MockTopic.Object);
        }

        private void SetupForumUserInfo()
        {
            var portalSettings = DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings();

            // Build the graph using the same UserInfo instances registered with userController
            this.ForumUserGraph = ForumUserObjectGraph.BuildFullGraph(
                ForumsObjectGraph.ModuleId,
                portalSettings,
                adminUserInfo: this.AdminUserInfo,
                user10Info: this.User10Info,
                user12Info: this.User12Info,
                anonUserInfo: this.AnonUserInfo,
                moderatorUserInfo: this.ModeratorUserInfo);

            this.MockForumUserController = new Mock<DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController>(ForumsObjectGraph.ModuleId);

            foreach (var forumUser in this.ForumUserGraph)
            {
                var capturedUser = forumUser;
                this.MockForumUserController
                    .Setup(c => c.GetByUserId(capturedUser.PortalId, capturedUser.ModuleId, capturedUser.UserId))
                    .Returns(capturedUser);
            }
        }

        private void SetupTopicReplyInfo()
        {
            this.TopicReplyGraph = TopicReplyObjectGraph.BuildFullGraph(
                moduleId: ForumsObjectGraph.ModuleId,
                portalId: ForumsObjectGraph.PortalId,
                forumsGraph: this.ForumsGraph,
                forumUserGraph: this.ForumUserGraph);

            foreach (var topic in this.TopicReplyGraph)
            {
                var capturedTopic = topic;
                this.MockTopicController
                    .Setup(tc => tc.GetById(capturedTopic.ModuleId, capturedTopic.TopicId, capturedTopic.Forum))
                    .Returns(capturedTopic);
            }
        }

        private void SetupTagInfo()
        {
            this.TagsGraph = DotNetNuke.Modules.ActiveForumsTests.ObjectGraphs.ForumsObjectGraph.BuildTagsGraph();
            foreach (var tag in this.TagsGraph)
            {
                var capturedTag = tag;
                this.mockTagController
                    .Setup(tc => tc.GetById(capturedTag.TagId))
                    .Returns(capturedTag);
                this.mockTagController
                    .Setup(tc => tc.GetByName(ForumsObjectGraph.ModuleId, capturedTag.TagName))
                    .Returns(capturedTag);
                this.mockTagController
                    .Setup(tc => tc.GetByName(It.IsAny<int>(), capturedTag.TagName))
                    .Returns(capturedTag);
            }
        }

        private void SetupCategoryInfo()
        {
            this.CategoriesGraph = DotNetNuke.Modules.ActiveForumsTests.ObjectGraphs.ForumsObjectGraph.BuildCategoriesGraph();
            foreach (var category in this.CategoriesGraph)
            {
                var capturedCategory = category;
                this.mockCategoryController
                    .Setup(cc => cc.GetById(capturedCategory.CategoryId))
                    .Returns(capturedCategory);
                this.mockCategoryController
                    .Setup(cc => cc.GetByName(ForumsObjectGraph.ModuleId, capturedCategory.CategoryName))
                    .Returns(capturedCategory);
                this.mockCategoryController
                    .Setup(cc => cc.GetByName(It.IsAny<int>(), capturedCategory.CategoryName))
                    .Returns(capturedCategory);
            }
        }

        internal void SetupMockNavigationManager()
        {
            this.MockNavigationManager = new Mock<DotNetNuke.Modules.ActiveForums.Services.Tests.NavigationManager>(this.mockPortalController.Object);
        }

        internal void SetupMockHttpApplication()
        {
            this.MockHttpApplication = this.CreateMockHttpApplication(this.DefaultSite);
        }

        internal Mock<System.Web.HttpApplication> CreateMockHttpApplication(string url)
        {
            var uri = new Uri(url);

            // Use the 5-argument SimpleWorkerRequest to ensure appVirtualDir and appPhysicalDir
            // are set, which is required for RuntimeConfig to resolve encoding during RewritePath.
            var workerRequest = new SimpleWorkerRequest(
                "/",
                System.IO.Path.GetTempPath(),
                uri.AbsolutePath,
                uri.Query.TrimStart('?'),
                new System.IO.StringWriter());
            var context = new HttpContext(workerRequest);

            var app = new Mock<System.Web.HttpApplication>();
            var contextField = typeof(HttpApplication).GetField("_context", BindingFlags.NonPublic | BindingFlags.Instance);
            contextField?.SetValue(app.Object, context);

            return app;
        }
    }
}
