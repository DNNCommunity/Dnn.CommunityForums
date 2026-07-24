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
    public static class StringExtensions
    {
        public static string EmptyIfNull(this string text)
        {
            return text ?? string.Empty;
        }

        public static string TruncateWithEllipsis(this string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
            {
                return text;
            }

            return text.Substring(0, maxLength) + "...";
        }

        public static string EncodeInvalidXmlChars(this string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            var sb = new System.Text.StringBuilder();
            foreach (var c in text)
            {
                // Characters invalid in XML: control chars 0x00-0x08, 0x0B-0x0C, 0x0E-0x1F, 0x7F-0x9F
                if ((c >= 0x00 && c <= 0x08) ||
                    (c >= 0x0B && c <= 0x0C) ||
                    (c >= 0x0E && c <= 0x1F) ||
                    (c >= 0x7F && c <= 0x9F))
                {
                    // Encode as numeric character reference
                    sb.AppendFormat("&#x{0:X};", (int)c);
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }
    }
}
