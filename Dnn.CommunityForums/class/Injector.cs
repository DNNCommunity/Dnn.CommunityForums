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
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    internal static class Injector
    {
        internal static string InjectCollapsibleOpened(string target, string title)
        {
            return $"<span class=\"dcf-collapsible dcf-collapsible-opened\" id=\"dcf-collapsible-{target}\" onclick=\"dcf_collapsible_toggle('{target}');\"><i class=\"fa fa-chevron-down\" title=\"{title}\"></i></span>";
        }

        internal static string InjectCollapsibleClosed(string target, string title)
        {
            return $"<span class=\"dcf-collapsible dcf-collapsible-closed\" id=\"dcf-collapsible-{target}\" onclick=\"dcf_collapsible_toggle('{target}');\"><i class=\"fa fa-chevron-left\" title=\"{title}\"></i></span>";
        }
    }
}
