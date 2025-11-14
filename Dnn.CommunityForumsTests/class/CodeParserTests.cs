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

namespace DotNetNuke.Modules.ActiveForumsTests
{
    using System.Reflection;

    using DotNetNuke.Modules.ActiveForums;
    using NUnit.Framework;

    [TestFixture]
    public partial class CodeParserTests : DotNetNuke.Modules.ActiveForumsTests.TestBase
    {
        [TestCase("[CODE]test[/CODE]", "<div class=\"afcodeblock\"><pre><code>test</code></pre></div>")]
        [TestCase("No code here", "No code here")]
        public void ParseCode_ReturnsExpectedHtml(string input, string expected)
        {
            var result = CodeParser.ParseCode(input);
            Assert.That(result, Does.Contain(expected));
        }

        [TestCase("[CODE]test[/CODE]", "<pre><code>test</code></pre>")]
        [TestCase("[/CODE]", "</code></pre>")]
        public void ConvertCodeBrackets_ConvertsBrackets(string input, string expected)
        {
            var result = CodeParser.ConvertCodeBrackets(input);
            Assert.That(result, Does.Contain(expected));
        }

        [TestCase("&#91;CODE&#93;", "[CODE]")]
        [TestCase("&amp;#91;CODE&amp;#93;", "[CODE]")]
        [TestCase("&#93;", "]")]
        [TestCase("&amp;#93;", "]")]
        public void HandleBrackets_ReplacesEntities(string input, string expected)
        {
            var method = typeof(CodeParser).GetMethod("HandleBrackets", BindingFlags.NonPublic | BindingFlags.Static);
            var result = method.Invoke(null, new object[] { input }) as string;
            Assert.That(result, Is.EqualTo(expected));
        }
    }
}
