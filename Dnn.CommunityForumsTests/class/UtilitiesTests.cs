﻿using NUnit.Framework;
using DotNetNuke.Modules.ActiveForums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using Moq;
using System.Runtime.InteropServices.WindowsRuntime;

namespace DotNetNuke.Modules.ActiveForumsTests
{
    [TestFixture()]
    public partial class UtilitiesTests
    {
        [Test()]
        [TestCase(-1, 0)]
        [TestCase(5, 4)]
        public void IsTrustedTest(int userTrustLevel, int userPostCount)
        {
            //Arrange

            //Act
            bool isTrusted = Utilities.IsTrusted(0, userTrustLevel, false, 0, userPostCount);
            //Assert
            Assert.That(isTrusted, Is.False);
        }

        [Test()]
        public void NullDateTest()
        {
            //Arrange
            DateTime expectedResult = DateTime.Parse("1/1/1900", new CultureInfo("en-US", false).DateTimeFormat).ToUniversalTime();
            //Act
            var actualResult = Utilities.NullDate();
            //Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test()]
        [TestCase("  this is a : messy string for a +url = 0 -", "this-is-a-messy-string-for-a-url-0")]
        public void CleanStringForUrlTest(string input, string expectedResult)
        {
            //Arrange 
            //Act
            string actualResult = Utilities.CleanStringForUrl(input);
            //Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }
        [Test()]
        [TestCase(0, 0, false, ExpectedResult = true)] // flood interval disables
        [TestCase(20, 25, false, ExpectedResult = true)]
        [TestCase(200, 25, true, ExpectedResult = true)] // user is an admin
        [TestCase(200, 25, false, ExpectedResult = false)] // interval is 200, last post is 25, expect false
        public bool HasFloodIntervalPassedTest1(int floodInterval, int secondsSinceLastPost, bool isAdmin)
        {
            //Arrange
            var mockUser = new Mock<User>();
            mockUser.Object.IsAdmin = isAdmin;
            mockUser.Object.Profile.DateLastPost = DateTime.UtcNow.AddSeconds(-1 * secondsSinceLastPost);
            mockUser.Object.Profile.DateLastReply = DateTime.UtcNow.AddSeconds(-1 * secondsSinceLastPost);
            var mockForum = new Mock<Forum>();
            //Act
            bool actualResult = Utilities.HasFloodIntervalPassed(floodInterval, mockUser.Object, mockForum.Object);
            //Assert
            return actualResult;
        }

        [Test()]
        [Obsolete("Deprecated in Community Forums. Removed in 09.00.00. Use HttpUtility.HtmlEncode.")]
        public void HtmlEncodeTestEmptyTag()
        {
            //Arrange
            //Act
            string actualResult = Utilities.HtmlEncode(string.Empty);
            //Assert
            Assert.That(actualResult, Is.Empty);
        }
        [Test()]
        [Obsolete("Deprecated in Community Forums. Removed in 09.00.00. Use HttpUtility.HtmlEncode.")]
        public void HtmlEncodeTest()
        {
            //Arrange
            string tag = "<p>";
            string expectedResult = "&lt;p&gt;";
            //Act
            string actualResult = Utilities.HtmlEncode(tag);
            //Assert
            Assert.That(expectedResult, Is.EqualTo(actualResult));
        }
        [Test()]
        [Obsolete("Deprecated in Community Forums. Removed in 09.00.00. Use HttpUtility.HtmlDecode.")]
        public void HtmlDecodeTestEmptyTag()
        {
            //Arrange
            //Act
            string actualResult = Utilities.HtmlDecode(string.Empty);
            //Assert
            Assert.That(actualResult, Is.Empty);
        }
        [Test()]
        [Obsolete("Deprecated in Community Forums. Removed in 09.00.00. Use HttpUtility.HtmlDecode.")]
        public void HtmlDecodeTest()
        {
            //Arrange
            string tag = "&lt;p&gt;";
            string expectedResult = "<p>";
            //Act
            string actualResult = Utilities.HtmlDecode(tag);
            //Assert
            Assert.That(expectedResult, Is.EqualTo(actualResult));
        }
        [Test()]
        [TestCase("", ExpectedResult = false)]
        [TestCase("<p>test<p>", ExpectedResult = true)]
        [TestCase("<p>test</p>", ExpectedResult = true)]
        [TestCase(" test ", ExpectedResult = false)]
        public bool HasHTMLTest(string value)
        {
            //Arrange
            //Act
            return Utilities.HasHTML(value);
            //Assert 
        }

    }
}