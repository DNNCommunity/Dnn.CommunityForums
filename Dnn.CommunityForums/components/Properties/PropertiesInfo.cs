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

    [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No longer used.")]
    public class PropertiesInfo
    {
        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No longer used.")]
        public int PropertyId { get; set; }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No longer used.")]
        public int PortalId { get; set; }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No longer used.")]
        public int ObjectType { get; set; }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No longer used.")]
        public int ObjectOwnerId { get; set; }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No longer used.")]
        public string Name { get; set; }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No longer used.")]
        public string DataType { get; set; }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No longer used.")]
        public int DefaultAccessControl { get; set; }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No longer used.")]
        public bool IsHidden { get; set; }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No longer used.")]
        public bool IsRequired { get; set; }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No longer used.")]
        public bool IsReadOnly { get; set; }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No longer used.")]
        public string ValidationExpression { get; set; } = string.Empty;

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No longer used.")]
        public string EditTemplate { get; set; } = string.Empty;

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No longer used.")]
        public string ViewTemplate { get; set; } = string.Empty;

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No longer used.")]
        public int SortOrder { get; set; }

        [Obsolete("Deprecated in Community Forums. Removed in 10.00.00. No longer used.")]
        public string DefaultValue { get; set; }
    }
}
