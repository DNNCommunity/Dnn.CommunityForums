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

namespace DotNetNuke.Modules.ActiveForums
{
    using System;

    using DotNetNuke.DependencyInjection;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Fires up upon DNN Startup.
    /// </summary>
    public class Startup : IDnnStartup
    {
        /// <inheritdoc/>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(x => DotNetNuke.Modules.ActiveForums.Controllers.ForumController.Instance);
            services.AddSingleton(x => DotNetNuke.Modules.ActiveForums.Controllers.ForumGroupController.Instance);
            services.AddSingleton(x => DotNetNuke.Modules.ActiveForums.Controllers.ContentController.Instance);
            services.AddSingleton(x => DotNetNuke.Modules.ActiveForums.Controllers.AttachmentController.Instance);
            services.AddSingleton(x => DotNetNuke.Modules.ActiveForums.Controllers.TagController.Instance);
            services.AddSingleton(x => DotNetNuke.Modules.ActiveForums.Controllers.CategoryController.Instance);
            services.AddSingleton(x => DotNetNuke.Modules.ActiveForums.Controllers.ForumTopicController.Instance);
            services.AddSingleton(x => DotNetNuke.Modules.ActiveForums.Controllers.ForumTrackingController.Instance);
            services.AddSingleton(x => DotNetNuke.Modules.ActiveForums.Controllers.TopicController.Instance);
            services.AddSingleton(x => DotNetNuke.Modules.ActiveForums.Controllers.ReplyController.Instance);
            services.AddSingleton(x => DotNetNuke.Modules.ActiveForums.Controllers.TopicTagController.Instance);
            services.AddSingleton(x => DotNetNuke.Modules.ActiveForums.Controllers.TopicCategoryController.Instance);
            services.AddSingleton(x => DotNetNuke.Modules.ActiveForums.Controllers.TopicRatingController.Instance);
            services.AddSingleton(x => DotNetNuke.Modules.ActiveForums.Controllers.TopicTrackingController.Instance);
            services.AddSingleton(x => DotNetNuke.Modules.ActiveForums.Controllers.BadgeController.Instance);
            services.AddSingleton(x => DotNetNuke.Modules.ActiveForums.Controllers.UserBadgeController.Instance);
            services.AddSingleton(x => DotNetNuke.Modules.ActiveForums.Controllers.LikeController.Instance);
            services.AddSingleton(x => DotNetNuke.Modules.ActiveForums.Controllers.UserMentionController.Instance);
            services.AddSingleton(x => DotNetNuke.Modules.ActiveForums.Controllers.ForumUserController.Instance);
            services.AddSingleton(x => DotNetNuke.Modules.ActiveForums.Controllers.ArchivedURLController.Instance);
            services.AddSingleton(x => DotNetNuke.Modules.ActiveForums.Controllers.PermissionController.Instance);
            services.AddSingleton(x => DotNetNuke.Modules.ActiveForums.Controllers.SettingsController.Instance);
            services.AddSingleton(x => DotNetNuke.Modules.ActiveForums.Controllers.SubscriptionController.Instance);
            services.AddSingleton(x => DotNetNuke.Modules.ActiveForums.Controllers.ProcessQueueController.Instance);
        }
    }
}
