using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetNuke.Web.Api;
using DotNetNuke.Abstractions;
using DotNetNuke.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using DotNetNuke.Entities.Modules;
using System.Runtime.CompilerServices;

namespace DotNetNuke.Modules.ActiveForums.Services
{
    internal class UrlNavigator : PortalModuleBase /*, INavigationManager*/
    {
        private readonly INavigationManager navigationManager;

        public UrlNavigator()
        {
            this.navigationManager = DependencyProvider.GetService<INavigationManager>();
        }

        public UrlNavigator(INavigationManager navigationManager)
        {
            this.navigationManager = navigationManager;
        }
        public static string NavigateUrl(INavigationManager navigation, int tabId, string[] @params)
        {
            return navigation?.NavigateURL(tabId, string.Empty, @params);
        }
        public string NavigateUrl(int tabId, string[] @params)
        {
            return navigationManager?.NavigateURL(tabId, string.Empty, @params);
        }

        //string INavigationManager.NavigateURL()
        //{
        //    throw new NotImplementedException();
        //}

        //string INavigationManager.NavigateURL(int tabID)
        //{
        //    throw new NotImplementedException();
        //}

        //string INavigationManager.NavigateURL(int tabID, bool isSuperTab)
        //{
        //    throw new NotImplementedException();
        //}

        //string INavigationManager.NavigateURL(string controlKey)
        //{
        //    throw new NotImplementedException();
        //}

        //string INavigationManager.NavigateURL(string controlKey, params string[] additionalParameters)
        //{
        //    throw new NotImplementedException();
        //}

        //string INavigationManager.NavigateURL(int tabID, string controlKey)
        //{
        //    throw new NotImplementedException();
        //}

        //string INavigationManager.NavigateURL(int tabID, string controlKey, params string[] additionalParameters)
        //{
        //    throw new NotImplementedException();
        //}

        //string INavigationManager.NavigateURL(int tabID, IPortalSettings settings, string controlKey, params string[] additionalParameters)
        //{
        //    throw new NotImplementedException();
        //}

        //string INavigationManager.NavigateURL(int tabID, bool isSuperTab, IPortalSettings settings, string controlKey, params string[] additionalParameters)
        //{
        //    throw new NotImplementedException();
        //}

        //string INavigationManager.NavigateURL(int tabID, bool isSuperTab, IPortalSettings settings, string controlKey, string language, params string[] additionalParameters)
        //{
        //    throw new NotImplementedException();
        //}

        //string INavigationManager.NavigateURL(int tabID, bool isSuperTab, IPortalSettings settings, string controlKey, string language, string pageName, params string[] additionalParameters)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
