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

namespace DotNetNuke.Modules.ActiveForums.Tests.Extensions
{
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Modules.ActiveForums.Extensions;

    using NUnit.Framework;

    [TestFixture]
    internal class HashSetExtensionsTests
    {
        [Test]
        public void ToHashSetFromDelimitedString_WithValidDelimitedString_ReturnsHashSetOfIntegers()
        {
            // Arrange
            var delimitedString = "1;2;3;4;5";

            // Act
            var result = delimitedString.ToHashSetFromDelimitedString<int>(";");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(5));
            Assert.That(result, Contains.Item(1));
            Assert.That(result, Contains.Item(2));
            Assert.That(result, Contains.Item(3));
            Assert.That(result, Contains.Item(4));
            Assert.That(result, Contains.Item(5));
        }

        [Test]
        public void ToHashSetFromDelimitedString_WithCustomDelimiter_ReturnsHashSetOfIntegers()
        {
            // Arrange
            var delimitedString = "10,20,30,40";
            var delimiter = ",";

            // Act
            var result = delimitedString.ToHashSetFromDelimitedString<int>(delimiter);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(4));
            Assert.That(result, Contains.Item(10));
            Assert.That(result, Contains.Item(20));
            Assert.That(result, Contains.Item(30));
            Assert.That(result, Contains.Item(40));
        }

        [Test]
        public void ToHashSetFromDelimitedString_WithSingleValue_ReturnsHashSetWithOneItem()
        {
            // Arrange
            var delimitedString = "42";

            // Act
            var result = delimitedString.ToHashSetFromDelimitedString<int>(string.Empty);

            // Assert
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result, Contains.Item(42));
        }

        [Test]
        public void ToHashSetFromDelimitedString_WithEmptyString_ReturnsEmptyHashSet()
        {
            // Arrange
            var delimitedString = string.Empty;

            // Act
            var result = delimitedString.ToHashSetFromDelimitedString<int>(string.Empty);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.Zero);
        }

        [Test]
        public void ToHashSetFromDelimitedString_WithNullString_ReturnsEmptyHashSet()
        {
            // Arrange
            string delimitedString = null;

            // Act
            var result = delimitedString.ToHashSetFromDelimitedString<int>(null);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.Zero);
        }

        [Test]
        public void ToHashSetFromDelimitedString_WithWhitespaceAroundValues_ReturnsHashSetWithoutWhitespace()
        {
            // Arrange
            var delimitedString = "1; 2; 3";

            // Act
            var result = delimitedString.ToHashSetFromDelimitedString<int>(";");

            // Assert
            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result, Contains.Item(1));
            Assert.That(result, Contains.Item(2));
        }

        [Test]
        public void ToHashSetFromDelimitedString_WithDuplicateValues_ReturnedHashSetContainsUniqueValues()
        {
            // Arrange
            var delimitedString = "5;5;5;10;10;15";

            // Act
            var result = delimitedString.ToHashSetFromDelimitedString<int>(";");

            // Assert
            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result, Contains.Item(5));
            Assert.That(result, Contains.Item(10));
            Assert.That(result, Contains.Item(15));
        }

        [Test]
        public void ToHashSetFromDelimitedString_WithPipeDelimiter_ReturnsHashSetOfIntegers()
        {
            // Arrange
            var delimitedString = "100|200|300";
            var delimiter = "|";

            // Act
            var result = delimitedString.ToHashSetFromDelimitedString<int>(delimiter);

            // Assert
            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result, Contains.Item(100));
            Assert.That(result, Contains.Item(200));
            Assert.That(result, Contains.Item(300));
        }

        [Test]
        public void ToHashSetFromDelimitedString_WithOnlyDelimiters_ReturnsEmptyHashSet()
        {
            // Arrange
            var delimitedString = ";;;";

            // Act
            var result = delimitedString.ToHashSetFromDelimitedString<int>(";");

            // Assert
            Assert.That(result.Count, Is.Zero);
        }

        [Test]
        public void FromHashSetToDelimitedString_WithIntegerHashSet_ReturnsDelimitedSortedString()
        {
            // Arrange
            var hashSet = new HashSet<int> { 5, 1, 3, 2, 4 };
            var delimiter = ",";

            // Act
            var result = hashSet.FromHashSetToDelimitedString(delimiter);

            // Assert
            Assert.That(result, Is.EqualTo("1,2,3,4,5"));
        }

        [Test]
        public void FromHashSetToDelimitedString_WithStringHashSet_ReturnsDelimitedSortedString()
        {
            // Arrange
            var hashSet = new HashSet<string> { "zebra", "apple", "banana" };
            var delimiter = ",";

            // Act
            var result = hashSet.FromHashSetToDelimitedString(delimiter);

            // Assert
            Assert.That(result, Is.EqualTo("apple,banana,zebra"));
        }

        [Test]
        public void FromHashSetToDelimitedString_WithEmptyHashSet_ReturnsEmptyString()
        {
            // Arrange
            var hashSet = new HashSet<int>();
            var delimiter = ",";

            // Act
            var result = hashSet.FromHashSetToDelimitedString(delimiter);

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public void FromHashSetToDelimitedString_WithSingleElement_ReturnsThatElement()
        {
            // Arrange
            var hashSet = new HashSet<int> { 42 };
            var delimiter = ",";

            // Act
            var result = hashSet.FromHashSetToDelimitedString(delimiter);

            // Assert
            Assert.That(result, Is.EqualTo("42"));
        }

        [Test]
        public void FromHashSetToDelimitedString_WithAlternativeDelimiter_ReturnsCorrectlyDelimitedString()
        {
            // Arrange
            var hashSet = new HashSet<int> { 3, 1, 2 };
            var delimiter = ";";

            // Act
            var result = hashSet.FromHashSetToDelimitedString(delimiter);

            // Assert
            Assert.That(result, Is.EqualTo("1;2;3"));
        }

        [Test]
        public void FromHashSetToDelimitedString_WithDuplicateElements_ReturnsDedupedString()
        {
            // Arrange
            var hashSet = new HashSet<int> { 1, 2, 3 };
            var delimiter = ",";

            // Act
            var result = hashSet.FromHashSetToDelimitedString(delimiter);

            // Assert
            Assert.That(result, Is.EqualTo("1,2,3"));
        }

        [Test]
        public void RoundTrip_StringToHashSetAndBack_PreservesData()
        {
            // Arrange
            var originalString = "5,1,3,2,4";
            var delimiter = ",";

            // Act
            var hashSet = originalString.ToHashSetFromDelimitedString<int>(delimiter);
            var result = hashSet.FromHashSetToDelimitedString(delimiter);

            // Assert
            Assert.That(result, Is.EqualTo("1,2,3,4,5"));
        }
    }
}

