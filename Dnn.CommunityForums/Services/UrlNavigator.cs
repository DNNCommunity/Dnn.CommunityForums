//
// Community Forums
// Copyright (c) 2013-2024
// by DNN Community
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
//
namespace DotNetNuke.Modules.ActiveForums.Services
{
    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using Microsoft.Extensions.DependencyInjection;

    internal class URLNavigator : PortalModuleBase /*, INavigationManager*/
    {
        private readonly INavigationManager navigationManager;

        internal URLNavigator()
        {
            this.navigationManager = DependencyProvider.GetService<INavigationManager>();
        }

        internal INavigationManager NavigationManager()
        {
           return this.navigationManager;
        }

        internal URLNavigator(INavigationManager navigationManager)
        {
            this.navigationManager = navigationManager;
        }

        internal static string NavigateURL(INavigationManager navigation, int tabId, string[] @params)
        {
            return navigation?.NavigateURL(tabId, string.Empty, @params);
        }

        internal string NavigateURL()
        {
            return navigationManager?.NavigateURL();
        }

        internal string NavigateURL(int tabID)
        {
            return navigationManager?.NavigateURL(tabID);
        }

        internal string NavigateURL(int tabID, bool isSuperTab)
        {
            return navigationManager?.NavigateURL(tabID, isSuperTab);
        }

        internal string NavigateURL(int tabId, string[] @params)
        {
            return navigationManager?.NavigateURL(tabId, string.Empty, @params);
        }

        internal string NavigateURL(string controlKey)
        {
            return navigationManager?.NavigateURL(controlKey);
        }

        internal string NavigateURL(string controlKey, params string[] additionalParameters)
        {
            return navigationManager?.NavigateURL(controlKey, additionalParameters);
        }

        internal string NavigateURL(int tabID, string controlKey)
        {
            return navigationManager?.NavigateURL(tabID, controlKey);
        }

        internal string NavigateURL(int tabID, string controlKey, params string[] additionalParameters)
        {
            return navigationManager?.NavigateURL(tabID, controlKey, additionalParameters);
        }

        internal string NavigateURL(int tabID, IPortalSettings settings, string controlKey, params string[] additionalParameters)
        {
            return navigationManager?.NavigateURL(tabID, settings, controlKey, additionalParameters);
        }

        internal  string NavigateURL(int tabID, bool isSuperTab, IPortalSettings settings, string controlKey, params string[] additionalParameters)
        {
            return navigationManager?.NavigateURL(tabID, isSuperTab, settings, controlKey, additionalParameters);
        }

        internal string  NavigateURL(int tabID, bool isSuperTab, IPortalSettings settings, string controlKey, string language, params string[] additionalParameters)
        {
            return navigationManager?.NavigateURL(tabID, isSuperTab, settings, controlKey, language, additionalParameters);
        }

        internal string NavigateURL(int tabID, bool isSuperTab, IPortalSettings settings, string controlKey, string language, string pageName, params string[] additionalParameters)
        {
            return navigationManager?.NavigateURL(tabID, isSuperTab, settings, controlKey, language, pageName, additionalParameters);
        }
    }
}
