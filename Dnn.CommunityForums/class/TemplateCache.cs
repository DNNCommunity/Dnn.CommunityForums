﻿// Copyright (c) by DNN Community
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

namespace DotNetNuke.Modules.ActiveForums
{
    using System;
    using System.Text;
    using System.Text.RegularExpressions;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Modules.ActiveForums.Enums;

    [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
    internal static class TemplateCache
    {
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. Not Used.")]
        public static string GetCachedTemplate(int moduleId, string TemplateType, int TemplateId)
        {
            if (TemplateId < 1)
            {
                return DotNetNuke.Modules.ActiveForums.Controllers.TemplateController.Template_Get(moduleId, (Enums.TemplateType)Enum.Parse(typeof(Enums.TemplateType), TemplateType, true), string.Empty);
            }

            return DotNetNuke.Modules.ActiveForums.Controllers.TemplateController.Template_Get(moduleId, (DotNetNuke.Modules.ActiveForums.Enums.TemplateType)Enum.Parse(typeof(DotNetNuke.Modules.ActiveForums.Enums.TemplateType), TemplateId.ToString()), string.Empty);
        }
    }
}
