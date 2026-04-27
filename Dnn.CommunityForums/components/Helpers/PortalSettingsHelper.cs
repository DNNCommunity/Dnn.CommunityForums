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

namespace DotNetNuke.Modules.ActiveForums.Helpers
{
    using System;
    using System.Linq;
    using System.Web;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework;

    internal class PortalSettingsHelper
    {
        private DotNetNuke.Entities.Portals.IPortalController portalController;
        private DotNetNuke.Abstractions.Portals.IPortalSettings portalSettings;

        public PortalSettingsHelper()
            : this(ServiceLocator<IPortalController, PortalController>.Instance, new DotNetNuke.Entities.Portals.PortalSettings())
        {
        }

        public PortalSettingsHelper(DotNetNuke.Entities.Portals.IPortalController portalController, DotNetNuke.Abstractions.Portals.IPortalSettings portalSettings)
        {
            this.portalController = portalController;
            this.portalSettings = portalSettings;
        }

        internal DotNetNuke.Entities.Portals.PortalSettings GetPortalSettings()
        {
            try
            {
                if (HttpContext.Current?.Items["PortalSettings"] != null)
                {
                    return (DotNetNuke.Entities.Portals.PortalSettings)HttpContext.Current.Items["PortalSettings"];
                }
                else
                {
                    return ServiceLocator<IPortalController, PortalController>.Instance.GetCurrentPortalSettings();
                }
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return null;
            }
        }

        internal DotNetNuke.Entities.Portals.PortalSettings GetPortalSettings(int portalId)
        {
            try
            {
                PortalSettings portalSettings = null;
                if (HttpContext.Current?.Items["PortalSettings"] != null)
                {
                    portalSettings = (DotNetNuke.Entities.Portals.PortalSettings)HttpContext.Current.Items["PortalSettings"];
                    if (portalSettings.PortalId != portalId)
                    {
                        portalSettings = null;
                    }
                }

                if (portalSettings == null)
                {
                    portalSettings = new PortalSettings(portalId);
                    PortalSettingsController psc = new DotNetNuke.Entities.Portals.PortalSettingsController();
                    psc.LoadPortalSettings(portalSettings);
                }

                var portalAliases = DotNetNuke.Entities.Portals.PortalAliasController.Instance.GetPortalAliasesByPortalId(portalId);
                portalSettings.PortalAlias = portalAliases?.FirstOrDefault(pa => pa.IsPrimary) ?? portalAliases?.FirstOrDefault();
                return portalSettings;
            }
            catch (Exception ex)
            {
                    Exceptions.LogException(ex);
                return null;
            }
        }
    }
}
