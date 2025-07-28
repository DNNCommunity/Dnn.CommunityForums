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
    using DotNetNuke.ComponentModel.DataAnnotations;

    [TableName("activeforums_Topics_Tags")]
    [PrimaryKey("TopicTagId", AutoIncrement = true)]
    public class TopicTagInfo
    {
        private DotNetNuke.Modules.ActiveForums.Entities.TagInfo tagInfo;
        private DotNetNuke.Modules.ActiveForums.Entities.TopicInfo topicInfo;
        
        public int TopicTagId { get; set; }
        public int TopicId { get; set; }

        public int TagId { get; set; }

        [IgnoreColumn]
        internal DotNetNuke.Modules.ActiveForums.Entities.TagInfo Tag
        {
            get
            {
                if (this.tagInfo == null)
                {
                    this.tagInfo = new DotNetNuke.Modules.ActiveForums.Controllers.TagController().GetById(this.TagId);
                    if (this.tagInfo == null)
                    {
                        this.tagInfo = new DotNetNuke.Modules.ActiveForums.Entities.TagInfo();
                    }
                }

                return this.tagInfo;
            }
        }
    }
}
