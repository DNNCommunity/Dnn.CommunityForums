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

namespace DotNetNuke.Modules.ActiveForumsTests.Services.Tokens
{
    using System;
    using System.Text;
    using System.Text.RegularExpressions;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.Tokens;

    internal class ForumsModuleTokenProvider : DotNetNuke.Services.Tokens.TokenProvider
    {
        private const string ExpressionDefault =
            "(?:(?<text>\\[\\])|\\[(?:(?<object>[^{}\\]\\[:]+):(?<property>[^\\]\\[\\|]+))(?:\\|(?:(?<format>[^\\]\\[]+)\\|(?<ifEmpty>[^\\]\\[]+))|\\|(?:(?<format>[^\\|\\]\\[]+)))?\\])|(?<text>\\[[^\\]\\[]+\\])|(?<text>[^\\]\\[]+)";

        public override bool ContainsTokens(string content, TokenContext context)
        {
            throw new NotImplementedException();
        }

        protected string ReplacedTokenValue(TokenContext context, string objectName, string propertyName, string format)
        {
            string result = string.Empty;
            bool propertyNotFound = false;
            if (context.PropertySource.ContainsKey(objectName.ToLowerInvariant()))
            {
                result = context.PropertySource[objectName.ToLowerInvariant()].GetProperty(propertyName, format, context.Language, context.AccessingUser, context.CurrentAccessLevel, ref propertyNotFound);
            }
            else
            {
                string message = "Error accessing [{0}:{1}], {0} is an unknown datasource";
                result = string.Format(message, objectName, propertyName);
            }

            if (propertyNotFound)
            {
                string message = "Error accessing [{0}:{1}], {1} is unknown for datasource {0}";
                result = string.Format(message, objectName, propertyName);
            }

            return result;
        }

        public override string Tokenize(string content, TokenContext context)
        {
            var result = new StringBuilder();
            if (!string.IsNullOrEmpty(content))
            {
                foreach (Match currentMatch in  RegexUtils.GetCachedRegex(ExpressionDefault).Matches(content))
                {
                    string objectName = currentMatch.Result("${object}");
                    if (!string.IsNullOrEmpty(objectName))
                    {
                        string propertyName = currentMatch.Result("${property}");
                        string format = currentMatch.Result("${format}");
                        string ifEmptyReplacment = currentMatch.Result("${ifEmpty}");
                        string conversion = this.ReplacedTokenValue(context, objectName, propertyName, format);
                        if (!string.IsNullOrEmpty(ifEmptyReplacment) && string.IsNullOrEmpty(conversion))
                        {
                            conversion = ifEmptyReplacment;
                        }

                        result.Append(conversion);
                    }
                    else
                    {
                        result.Append(currentMatch.Result("${text}"));
                    }
                }
            }

            return result.ToString();
        }
    }
}
