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

namespace DotNetNuke.Modules.ActiveForumsTests.Services.Sitemap
{
    using System;

    using DotNetNuke.Modules.ActiveForums.Services.Search;
    using DotNetNuke.Modules.ActiveForums.Services.Sitemap;
    using DotNetNuke.Services.Sitemap;

    using NUnit.Framework;

    [TestFixture]
    public class ForumsSitemapProviderTests
    {
        [Test]
        public void DetermineChangeFrequency_ShouldReturnHourly_ForRecentAndHighlyActiveTopic()
        {
            // Arrange
            DateTime nowUtc = DateTime.UtcNow;
            DateTime topicDateUtc = nowUtc.AddDays(-2);
            var metrics = CreateMetrics(topicDateUtc, topicDateUtc, 6, 0.4);

            // Act
            SitemapChangeFrequency frequency = ForumsSitemapProvider.DetermineChangeFrequency(metrics, nowUtc);

            // Assert
            Assert.That(frequency, Is.EqualTo(SitemapChangeFrequency.Hourly));
        }

        [Test]
        public void DetermineChangeFrequency_ShouldReturnYearly_ForVeryOldTopicWithoutReplies()
        {
            // Arrange
            DateTime nowUtc = DateTime.UtcNow;
            DateTime topicDateUtc = nowUtc.AddDays(-400);
            var metrics = CreateMetrics(topicDateUtc, topicDateUtc, 0, 0);

            // Act
            SitemapChangeFrequency frequency = ForumsSitemapProvider.DetermineChangeFrequency(metrics, nowUtc);

            // Assert
            Assert.That(frequency, Is.EqualTo(SitemapChangeFrequency.Yearly));
        }

        [Test]
        public void DeterminePriority_ShouldBoost_WhenTopicHasRepliesAndRecentActivity()
        {
            // Arrange
            DateTime nowUtc = DateTime.UtcNow;
            DateTime topicDateUtc = nowUtc.AddDays(-14);
            var metrics = CreateMetrics(topicDateUtc, topicDateUtc, 8, 12);

            // Act
            float priority = ForumsSitemapProvider.DeterminePriority(metrics, nowUtc);

            // Assert
            Assert.That(priority, Is.GreaterThan(0.6F));
        }

        [Test]
        public void DeterminePriority_ShouldCapAtOne_ForHighlyPopularTopic()
        {
            // Arrange
            DateTime nowUtc = DateTime.UtcNow;
            DateTime topicDateUtc = nowUtc.AddDays(-10);
            var metrics = CreateMetrics(topicDateUtc, topicDateUtc, 30, 10);

            // Act
            float priority = ForumsSitemapProvider.DeterminePriority(metrics, nowUtc);

            // Assert
            Assert.That(priority, Is.EqualTo(1F));
        }

        [Test]
        public void DeterminePriority_ShouldRemainBase_WhenTopicIsOldAndInactive()
        {
            // Arrange
            DateTime nowUtc = DateTime.UtcNow;
            DateTime topicDateUtc = nowUtc.AddDays(-90);
            var metrics = CreateMetrics(topicDateUtc, topicDateUtc.AddDays(-10), 0, 0);

            // Act
            float priority = ForumsSitemapProvider.DeterminePriority(metrics, nowUtc);

            // Assert
            Assert.That(priority, Is.EqualTo(0.5F));
        }

        private static ForumsSitemapProvider.SitemapUrlMetrics CreateMetrics(DateTime topicDateUtc, DateTime firstReplyDateUtc, int replyCount, double replySpacingInDays)
        {
            var metrics = new ForumsSitemapProvider.SitemapUrlMetrics();
            metrics.Update(new SearchSitemapResult
            {
                ReplyId = -1,
                DateCreated = topicDateUtc,
                DateUpdated = topicDateUtc,
            });

            for (int index = 0; index < replyCount; index++)
            {
                DateTime replyDateUtc = firstReplyDateUtc.AddDays(index * replySpacingInDays);
                metrics.Update(new SearchSitemapResult
                {
                    ReplyId = index + 1,
                    DateCreated = replyDateUtc,
                    DateUpdated = replyDateUtc,
                });
            }

            return metrics;
        }
    }
}
