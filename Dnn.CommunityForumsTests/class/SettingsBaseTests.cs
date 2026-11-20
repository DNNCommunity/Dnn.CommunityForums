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
    using System.Web;

    using DotNetNuke.Modules.ActiveForums;

    using NUnit.Framework;

    [TestFixture]
    public class SettingsBaseTests : TestBase
    {
        [TearDown]
        public void TearDown()
        {
            HttpContext.Current = null;
        }

        [Test]
        public void PageId_WithPageJumpHashFragment_ReturnsParsedPageNumber()
        {
            // Arrange
            var settingsBase = this.CreateSettingsBase(this.DefaultSite, $"{Literals.PageId}=2#2586");

            // Act
            var pageId = settingsBase.PageIdDuringLoad;

            // Assert
            Assert.That(pageId, Is.EqualTo(2));
        }

        [Test]
        public void PageId_WithInvalidQueryString_ReturnsFirstPage()
        {
            // Arrange
            var settingsBase = this.CreateSettingsBase($"{this.DefaultSite}?{ParamKeys.PageId}=invalid");

            // Act
            var pageId = settingsBase.PageIdDuringLoad;

            // Assert
            Assert.That(pageId, Is.EqualTo(1));
        }

        private TestSettingsBase CreateSettingsBase(string url, string @params = "")
        {
            var mockApp = this.CreateMockHttpApplication(url);
            HttpContext.Current = mockApp.Object.Context;

            var settingsBase = new TestSettingsBase
            {
                Params = @params,
            };
            var page = new System.Web.UI.Page();
            page.Controls.Add(settingsBase);
            page.ProcessRequest(HttpContext.Current);

            return settingsBase;
        }

        private sealed class TestSettingsBase : SettingsBase
        {
            public int PageIdDuringLoad { get; private set; }

            protected override void OnLoad(System.EventArgs e)
            {
                this.PageIdDuringLoad = this.PageId;
            }
        }
    }
}
