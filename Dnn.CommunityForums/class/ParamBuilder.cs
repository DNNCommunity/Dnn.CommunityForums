// Copyright (c) 2013-2024 by DNN Community
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
    using System.Web;

    [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
    public class ParamBuilder
    {
        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public Dictionary<string, object> Params { get; set; }
        
        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public string[] ParamArray
        {
            get
            {
                return this.Params == null ? new string[] { } : this.Params.Keys.Select(key => string.Format("{0}={1}", System.Net.WebUtility.UrlEncode(key), System.Net.WebUtility.UrlEncode(this.Params[key].ToString()))).ToArray();
            }
        }

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public string QueryString
        {
            get
            {
                return this.ParamArray.Aggregate(string.Empty, (current, param) => current + (string.IsNullOrWhiteSpace(current) ? string.Empty : "&") + param);
            }
        }

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public ParamBuilder()
        {
            this.Params = new Dictionary<string, object>();
        }

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public ParamBuilder(Dictionary<string, object> parameters)
        {
            this.Params = parameters;
        }

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public ParamBuilder(string queryString, bool urlEncoded = true)
        {
            if (string.IsNullOrWhiteSpace(queryString))
            {
                return;
            }

            queryString = queryString.Trim();

            if (queryString.StartsWith("?"))
            {
                queryString = queryString.Substring(1);
            }

            this.Params = new Dictionary<string, object>();

            var parameters = queryString.Split('&');
            foreach (var p in parameters)
            {
                var elements = p.Split('=');
                var key = elements[0];
                var value = elements.Length > 0 ? elements[1] : string.Empty;

                if (urlEncoded)
                {
                    key = System.Net.WebUtility.UrlDecode(key);
                    value = System.Net.WebUtility.UrlDecode(value);
                }

                this.Params[key] = value;
            }
        }

        [Obsolete("Deprecated in Community Forums. Removing in 10.00.00. Not Used.")]
        public void Set(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            if (this.Params == null)
            {
                this.Params = new Dictionary<string, object>();
            }

            this.Params[key] = value ?? string.Empty;
        }
    }
}
