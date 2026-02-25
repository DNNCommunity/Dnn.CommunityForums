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

namespace DotNetNuke.Modules.ActiveForums.Extensions
{
    using System.Linq;

    internal static class PortalAliasExtensions
    {
        internal static DotNetNuke.Abstractions.Portals.IPortalAliasService GetDefaultPortalService(DotNetNuke.Entities.Portals.IPortalAliasController portalAliasController)
        {
            return (DotNetNuke.Abstractions.Portals.IPortalAliasService)DotNetNuke.Entities.Portals.PortalAliasController.Instance;
            
        }
        

        internal static string GetDefaultPortalAlias(this DotNetNuke.Entities.Portals.PortalSettings portalSettings)
        {


            var defaultPortalAlias = string.Empty;



            try
            {
                if (!string.IsNullOrEmpty(portalSettings?.DefaultPortalAlias))
                {
                    return portalSettings.DefaultPortalAlias;
                }
            }
            catch (System.ArgumentNullException)
            {
                var portalAlias = GetDefaultPortalService(DotNetNuke.Entities.Portals.PortalAliasController.Instance).GetPortalAliasesByPortalId(portalSettings.PortalId);
                defaultPortalAlias = portalAlias.Where(pa => pa.IsPrimary).FirstOrDefault().HttpAlias;
                
            }
            

            return defaultPortalAlias;
        }
    }
}
