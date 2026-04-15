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

namespace DotNetNuke.Modules.ActiveForumsTests.Components.Sitemap
{
    using System.Collections.Generic;

    using DotNetNuke.Modules.ActiveForums.Sitemap;
    using NUnit.Framework;

    [TestFixture]
    public class ForumsSitemapProviderTests
    {
        [Test]
        public void IsPublicForum_WithAllUsersRole_ReturnsTrue()
        {
            // Arrange
            var readRoleIds = new List<int> { int.Parse(DotNetNuke.Common.Globals.glbRoleAllUsers) };

            // Act
            bool result = ForumsSitemapProvider.IsPublicForum(readRoleIds);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void IsPublicForum_WithNoPublicRoles_ReturnsFalse()
        {
            // Arrange
            var readRoleIds = new List<int> { 2, 3, 4 };

            // Act
            bool result = ForumsSitemapProvider.IsPublicForum(readRoleIds);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void EnsureAbsoluteUrl_WithRelativePathAndSecure_ReturnsHttpsUrl()
        {
            // Arrange
            string link = "/forums/topic/abc";
            string portalAlias = "dnncommunity.org";

            // Act
            string result = ForumsSitemapProvider.EnsureAbsoluteUrl(link, portalAlias, true);

            // Assert
            Assert.That(result, Is.EqualTo("https://dnncommunity.org/forums/topic/abc"));
        }

        [Test]
        public void EnsureAbsoluteUrl_WithAbsoluteUrl_ReturnsUnchanged()
        {
            // Arrange
            string link = "https://dnncommunity.org/forums/topic/abc";

            // Act
            string result = ForumsSitemapProvider.EnsureAbsoluteUrl(link, "ignored", false);

            // Assert
            Assert.That(result, Is.EqualTo(link));
        }
    }
}
