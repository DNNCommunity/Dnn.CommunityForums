namespace DotNetNuke.Modules.ActiveForumsTests.Extensions
{
    using DotNetNuke.Modules.ActiveForums.Extensions;

    using Moq;

    using NUnit.Framework;

    using System;

    [TestFixture]
    public class StringExtensionsTests
    {
        [Test]
        [TestCase(null, ExpectedResult = "")]
        [TestCase("test", ExpectedResult = "test")]
        public string EmptyIfNull(string input)
        {
            // Arrange

            // Act
            var result = input.EmptyIfNull();

            // Assert
            return result;
        }

        [TestCase(null, 20, ExpectedResult = null)]
        [TestCase("12345678901234567890", 10, ExpectedResult = "1234567890...")]
        [TestCase("12345678901234567890", 20, ExpectedResult = "12345678901234567890")]
        public string TruncateWithEllipsis(string input, int length)
        {
            // Arrange

            // Act
            var result = input.TruncateWithEllipsis(length);

            // Assert
            return result;
        }

        [Test]
        [TestCase(null, ExpectedResult = null)]
        [TestCase("", ExpectedResult = "")]
        public string EncodeInvalidXmlChars_WithNullOrEmpty_ReturnsAsIs(string input)
        {
            // Arrange

            // Act
            var result = input.EncodeInvalidXmlChars();

            // Assert
            return result;
        }

        [Test]
        [TestCase("Hello World", ExpectedResult = "Hello World")]
        [TestCase("Simple text with numbers 123", ExpectedResult = "Simple text with numbers 123")]
        public string EncodeInvalidXmlChars_WithValidChars_ReturnsUnchanged(string input)
        {
            // Arrange

            // Act
            var result = input.EncodeInvalidXmlChars();

            // Assert
            return result;
        }

        [Test]
        public void EncodeInvalidXmlChars_WithControlChars0x00To0x08_EncodesProperly()
        {
            // Arrange
            var input = new string(new[] { (char)0x00, (char)0x01, (char)0x08 });

            // Act
            var result = input.EncodeInvalidXmlChars();

            // Assert
            Assert.That(result, Is.EqualTo("&#x0;&#x1;&#x8;"));
        }

        [Test]
        public void EncodeInvalidXmlChars_WithControlChars0x0BTo0x0C_EncodesProperly()
        {
            // Arrange
            var input = new string(new[] { (char)0x0B, (char)0x0C });

            // Act
            var result = input.EncodeInvalidXmlChars();

            // Assert
            Assert.That(result, Is.EqualTo("&#xB;&#xC;"));
        }

        [Test]
        public void EncodeInvalidXmlChars_WithControlChars0x0ETo0x1F_EncodesProperly()
        {
            // Arrange
            var input = new string(new[] { (char)0x0E, (char)0x1F });

            // Act
            var result = input.EncodeInvalidXmlChars();

            // Assert
            Assert.That(result, Is.EqualTo("&#xE;&#x1F;"));
        }

        [Test]
        public void EncodeInvalidXmlChars_WithControlChars0x7FTo0x9F_EncodesProperly()
        {
            // Arrange
            var input = new string(new[] { (char)0x7F, (char)0x9F });

            // Act
            var result = input.EncodeInvalidXmlChars();

            // Assert
            Assert.That(result, Is.EqualTo("&#x7F;&#x9F;"));
        }

        [Test]
        public void EncodeInvalidXmlChars_WithMixedValidAndInvalidChars_EncodesOnlyInvalid()
        {
            // Arrange
            var input = "Hello\x00World\x08Test";

            // Act
            var result = input.EncodeInvalidXmlChars();

            // Assert
            Assert.That(result, Is.EqualTo("Hello&#x0;World&#x8;Test"));
        }

        [Test]
        public void EncodeInvalidXmlChars_WithAllInvalidRanges_EncodesAll()
        {
            // Arrange
            var input = new string(new[] 
            { 
                'A',
                (char)0x01,  // Range 0x00-0x08
                'B',
                (char)0x0B,  // Range 0x0B-0x0C
                'C',
                (char)0x0E,  // Range 0x0E-0x1F
                'D',
                (char)0x85,  // Range 0x7F-0x9F
                'E'
            });

            // Act
            var result = input.EncodeInvalidXmlChars();

            // Assert
            Assert.That(result, Is.EqualTo("A&#x1;B&#xB;C&#xE;D&#x85;E"));
        }

        [Test]
        public void EncodeInvalidXmlChars_WithBoundaryChar0x09_ReturnsUnchanged()
        {
            // Arrange - 0x09 is a valid character (tab)
            var input = "Test" + (char)0x09 + "Char";

            // Act
            var result = input.EncodeInvalidXmlChars();

            // Assert
            Assert.That(result, Is.EqualTo("Test" + (char)0x09 + "Char"));
        }

        [Test]
        public void EncodeInvalidXmlChars_WithBoundaryChar0x0D_ReturnsUnchanged()
        {
            // Arrange - 0x0D is a valid character (carriage return)
            var input = "Test" + (char)0x0D + "Char";

            // Act
            var result = input.EncodeInvalidXmlChars();

            // Assert
            Assert.That(result, Is.EqualTo("Test" + (char)0x0D + "Char"));
        }

        [Test]
        public void EncodeInvalidXmlChars_WithBoundaryChar0x20_ReturnsUnchanged()
        {
            // Arrange - 0x20 is a valid character (space)
            var input = "Test Char";

            // Act
            var result = input.EncodeInvalidXmlChars();

            // Assert
            Assert.That(result, Is.EqualTo("Test Char"));
        }

        [Test]
        public void EncodeInvalidXmlChars_WithBoundaryChar0x7E_ReturnsUnchanged()
        {
            // Arrange - 0x7E is a valid character (tilde)
            var input = "Test~Char";

            // Act
            var result = input.EncodeInvalidXmlChars();

            // Assert
            Assert.That(result, Is.EqualTo("Test~Char"));
        }

        [Test]
        public void EncodeInvalidXmlChars_WithBoundaryChar0xA0_ReturnsUnchanged()
        {
            // Arrange - 0xA0 is a valid character (non-breaking space)
            var input = "Test" + (char)0xA0 + "Char";

            // Act
            var result = input.EncodeInvalidXmlChars();

            // Assert
            Assert.That(result, Is.EqualTo("Test" + (char)0xA0 + "Char"));
        }

        [Test]
        public void EncodeInvalidXmlChars_WithConsecutiveInvalidChars_EncodesAll()
        {
            // Arrange
            var input = new string(new[] { (char)0x00, (char)0x01, (char)0x02 });

            // Act
            var result = input.EncodeInvalidXmlChars();

            // Assert
            Assert.That(result, Is.EqualTo("&#x0;&#x1;&#x2;"));
        }

        [Test]
        public void EncodeInvalidXmlChars_WithSpecialCharacter_EncodesCorrectly()
        {
            // Arrange - Test with a character that should be encoded
            var input = $"Before{(char)0x1F}After";

            // Act
            var result = input.EncodeInvalidXmlChars();

            // Assert
            Assert.That(result, Is.EqualTo("Before&#x1F;After"));
        }
    }
}
