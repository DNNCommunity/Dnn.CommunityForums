using NUnit.Framework;
using DotNetNuke.Modules.ActiveForums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using Moq;
using System.Runtime.InteropServices.WindowsRuntime;

namespace DotNetNuke.Modules.ActiveForums.Tests
{
    [TestFixture()]
    public class UtilitiesTests
    {
        [Test()]
        public void IsTrustedUserTrustLevelTest()
        {
            //Arrange
            int userTrustLevel = -1;
            //Act
            bool isTrusted = Utilities.IsTrusted(0, userTrustLevel, false, 0, 0);
            //Assert
            Assert.IsFalse(isTrusted);
        }
        [Test()]
        public void IsTrustedAutoTrustTest()
        {
            //Arrange
            int autoTrustLevel = 5;
            int userPostCount = 4;
            //Act
            bool isTrusted = Utilities.IsTrusted(0, 0, false, autoTrustLevel, userPostCount);
            //Assert
            Assert.IsFalse(isTrusted);
        }

        [Test()]
        public void NullDateTest()
        {
            //Arrange
            DateTime expectedResult = DateTime.Parse("1/1/1900", new CultureInfo("en-US", false).DateTimeFormat).ToUniversalTime();
            //Act
            var actualResult = Utilities.NullDate();
            //Assert
            Assert.AreEqual(expectedResult, actualResult);
        }

        [Test()]
        public void CleanStringForUrlTest()
        {
            //Arrange
            string input = "  this is a : messy string for a +url = 0 -";
            string expectedResult = "this-is-a-messy-string-for-a-url-0";
            //Act
            string actualResult = Utilities.CleanStringForUrl(input);
            //Assert
            Assert.AreEqual(expectedResult, actualResult);
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
    }
}