//
// Community Forums
// Copyright (c) 2013-2024
// by DNN Community
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
//
namespace DotNetNuke.Modules.ActiveForums.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Data;
    using DotNetNuke.Modules.ActiveForums.Entities;

    class TopicRatingController
    {
        readonly IDataContext ctx;
        IRepository<DotNetNuke.Modules.ActiveForums.Entities.TopicRatingInfo> repo;

        public TopicRatingController()
        {
            this.ctx = DataContext.Instance();
            this.repo = this.ctx.GetRepository<DotNetNuke.Modules.ActiveForums.Entities.TopicRatingInfo>();
        }

        public (int averageRating, int usersRating) Get(int userId, int topicId)
        {
            return (this.Average(topicId), this.GetForTopicAndUser(userId, topicId).FirstOrDefault().Rating);
        }

        public List<DotNetNuke.Modules.ActiveForums.Entities.TopicRatingInfo> GetForTopicAndUser(int userId, int topicId)
        {
            return this.repo.Find("WHERE TopicId = @0 AND UserId = @1", topicId, userId).ToList();
        }

        public List<DotNetNuke.Modules.ActiveForums.Entities.TopicRatingInfo> GetForTopic(int topicId)
        {
            return this.repo.Find("WHERE TopicId = @0", topicId).ToList();
        }

        public List<DotNetNuke.Modules.ActiveForums.Entities.TopicRatingInfo> GetForUser(int userId)
        {
            return this.repo.Find("WHERE UserId = @0", userId).ToList();
        }

        public int Average(int topicId)
        {
            return Utilities.SafeConvertInt(Math.Round(this.repo.Find("WHERE TopicId = @0", topicId).Average(r => r.Rating), 0));
        }

        public int Count(int topicId)
        {
            return this.repo.Find("WHERE TopicId = @0", topicId).Count();
        }

        public int Rate(int userId, int topicId, int rating, string IpAddress)
        {
            DotNetNuke.Modules.ActiveForums.Entities.TopicRatingInfo topicRating = this.GetForTopicAndUser(userId: userId, topicId:topicId).FirstOrDefault();
            if (topicRating != null)
            {
                topicRating.Rating = rating;
                topicRating.IPAddress = IpAddress;
                topicRating.DateUpdated = DateTime.UtcNow;
                this.repo.Update(topicRating);
            }
            else
            {
                topicRating = new Entities.TopicRatingInfo();
                topicRating.TopicId = topicId;
                topicRating.UserId = userId;
                topicRating.Rating = rating;
                topicRating.IPAddress = IpAddress;
                topicRating.DateAdded = DateTime.UtcNow;
                topicRating.DateUpdated = DateTime.UtcNow;
                this.repo.Insert(topicRating);
            }

            return this.Average(topicId);
        }
    }
}
