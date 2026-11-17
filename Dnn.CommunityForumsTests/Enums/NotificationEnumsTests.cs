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

namespace DotNetNuke.Modules.ActiveForumsTests.Enums
{
    using System;
    using System.Linq;

    using DotNetNuke.Modules.ActiveForums.Enums;

    using NUnit.Framework;

    [TestFixture]
    public class NotificationEnumsTests : TestBase
    {
        [TestCase(typeof(NotificationEvent), 16)]
        [TestCase(typeof(NotificationMethod), 4)]
        [TestCase(typeof(NotificationOptions), 3)]
        public void NotificationEnums_HaveIndependentFlags(Type enumType, int expectedFlagCount)
        {
            var values = Enum.GetValues(enumType).Cast<object>().Select(Convert.ToInt32).ToArray();

            Assert.That(enumType.IsDefined(typeof(FlagsAttribute), false), Is.True);
            Assert.That(values, Has.Length.EqualTo(expectedFlagCount + 1));
            Assert.That(values.Count(value => value == 0), Is.EqualTo(1));
            Assert.That(values.Where(value => value != 0).Distinct().Count(), Is.EqualTo(expectedFlagCount));
            Assert.That(values.Where(value => value != 0), Has.All.Matches<int>(value => (value & (value - 1)) == 0));
        }

        [Test]
        public void NotificationSelections_CanCombineAndRestrictFlags()
        {
            var events = NotificationEvent.NewTopicInSubscribedForum | NotificationEvent.MyPostIsLiked;
            var methods = NotificationMethod.Email | NotificationMethod.DnnNotification;
            var forumOptions = NotificationOptions.Summary | NotificationOptions.Link;
            var userOptions = NotificationOptions.Content | NotificationOptions.Link;

            Assert.That(events.HasFlag(NotificationEvent.NewTopicInSubscribedForum), Is.True);
            Assert.That(events.HasFlag(NotificationEvent.MyPostIsLiked), Is.True);
            Assert.That(events.HasFlag(NotificationEvent.SomeoneMentionedMe), Is.False);
            Assert.That(methods.HasFlag(NotificationMethod.Email), Is.True);
            Assert.That(methods.HasFlag(NotificationMethod.DnnNotification), Is.True);
            Assert.That(methods.HasFlag(NotificationMethod.Sms), Is.False);
            Assert.That(userOptions & forumOptions, Is.EqualTo(NotificationOptions.Link));
        }
    }
}
