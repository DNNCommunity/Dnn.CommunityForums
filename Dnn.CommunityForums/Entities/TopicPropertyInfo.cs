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

namespace DotNetNuke.Modules.ActiveForums.Entities
{
    using System;

    using DotNetNuke.ComponentModel.DataAnnotations;
    using DotNetNuke.Data;

    [TableName("communityforums_Topic_Properties")]
    [PrimaryKey("Id", AutoIncrement = true)]
    public class TopicPropertyInfo
    {
        private PropertyInfo propertyInfo;
        private TopicInfo topicInfo;
        private string name;

        public int Id { get; set; }

        public int TopicId { get; set; }

        public int PropertyId { get; set; }

        [IgnoreColumn]
        public string Name
        {
            get => this.name ?? this.GetProperty().Name;
            set => this.name = value;
        }

        public string Value { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime DateUpdated { get; set; }

        public TopicInfo GetTopic()
        {
            if (this.topicInfo == null)
            {
                this.topicInfo = ((IRepository<TopicInfo>)Controllers.TopicController.Instance).GetById(this.TopicId) ?? new TopicInfo();
            }

            return this.topicInfo;
        }

        public PropertyInfo GetProperty()
        {
            if (this.propertyInfo == null)
            {
                this.propertyInfo = new Controllers.PropertyController().GetById(this.PropertyId) ?? new PropertyInfo();
            }

            return this.propertyInfo;
        }
    }
}
