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

using System;
using System.Collections.Generic;
using System.Linq;

namespace DotNetNuke.Modules.ActiveForums.Extensions
{
    internal static class HashSetExtensions
    {
        /// <summary>
        /// Converts a delimited string to a HashSet of type T.
        /// </summary>
        /// <typeparam name="T">The type to convert the string elements to.</typeparam>
        /// <param name="delimitedString">The delimited string to convert.</param>
        /// <param name="delimiter">The delimiter used to split the string.</param>
        /// <returns>A HashSet of type T parsed from the delimited string.</returns>
        internal static HashSet<T> ToHashSetFromDelimitedString<T>(this string delimitedString, string delimiter)
        {
            var hashSet = new HashSet<T>();
            if (!string.IsNullOrEmpty(delimitedString))
            {
                hashSet.UnionWith(delimitedString.Split(delimiter.ToString().ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Distinct().Select(r => (T)Convert.ChangeType(r, typeof(T))));
            }

            return hashSet;
        }
        /// <summary>
        /// Converts a HashSet of T to a delimited string, sorted in ascending order.
        /// </summary>
        /// <typeparam name="T">The type of elements in the HashSet. Must implement IComparable&lt;T&gt;.</typeparam>
        /// <param name="set">The collection to convert.</param>
        /// <param name="delimiter">The delimiter to use when joining the elements.</param>
        /// <returns>A delimited string of distinct, sorted elements.</returns>
        internal static string FromHashSetToDelimitedString<T>(this HashSet<T> set, string delimiter) where T : IComparable<T>
        {
            return string.Join(separator: delimiter, values: set.Distinct().OrderBy(r => r));
        }
    }
}

