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
    }
}
