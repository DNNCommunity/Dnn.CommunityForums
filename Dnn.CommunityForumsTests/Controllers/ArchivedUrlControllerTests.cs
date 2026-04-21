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

namespace DotNetNuke.Modules.ActiveForumsTests.Controllers
{
    using NUnit.Framework;

    [TestFixture]
    public class ArchivedUrlControllerTests
    {
        [Test]
        public void NormalizeUrl_TrimsAndLowerCases()
        {
            // Arrange
            const string input = "  /Forums/Topic-One/  ";

            // Act
            var result = DotNetNuke.Modules.ActiveForums.Controllers.ArchivedUrlController.NormalizeUrl(input);

            // Assert
            Assert.That(result, Is.EqualTo("/forums/topic-one/"));
        }

        [Test]
        public void IsUrlMatch_ReturnsTrue_WhenArchivedUrlIsEquivalentWithoutLeadingSlash()
        {
            // Arrange
            const string archivedUrl = "forums/topic-one/";
            const string normalizedUrl = "/forums/topic-one/";
            const string normalizedUrlWithoutLeadingSlash = "forums/topic-one/";

            // Act
            var result = DotNetNuke.Modules.ActiveForums.Controllers.ArchivedUrlController.IsUrlMatch(archivedUrl, normalizedUrl, normalizedUrlWithoutLeadingSlash);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void ComputeUrlHash_ReturnsSameHashForCaseVariantsAfterNormalization()
        {
            // Arrange
            var first = DotNetNuke.Modules.ActiveForums.Controllers.ArchivedUrlController.NormalizeUrl("/FORUMS/Topic-One/");
            var second = DotNetNuke.Modules.ActiveForums.Controllers.ArchivedUrlController.NormalizeUrl("/forums/topic-one/");

            // Act
            var firstHash = DotNetNuke.Modules.ActiveForums.Controllers.ArchivedUrlController.ComputeUrlHash(first);
            var secondHash = DotNetNuke.Modules.ActiveForums.Controllers.ArchivedUrlController.ComputeUrlHash(second);

            // Assert
            Assert.That(firstHash, Is.EqualTo(secondHash));
        }
    }
}

