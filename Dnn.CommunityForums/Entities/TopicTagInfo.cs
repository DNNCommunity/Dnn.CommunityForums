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

namespace DotNetNuke.Modules.ActiveForums.Entities
{
    using DotNetNuke.ComponentModel.DataAnnotations;

    /* activeforums_Topics_Tags has a composite primary key, which is not supported by DAL2/PetaPoco */
    [TableName("activeforums_Topics_Tags")]
    public class TopicTagInfo
    {
        private DotNetNuke.Modules.ActiveForums.Entities.TagInfo _tagInfo;
        private DotNetNuke.Modules.ActiveForums.Entities.TopicInfo _topicInfo;

        public int TopicId { get; set; }

        public int TagId { get; set; }

        [IgnoreColumn()]
        public DotNetNuke.Modules.ActiveForums.Entities.TagInfo Tag
        {
            get
            {
                if (_tagInfo == null)
                {
                    _tagInfo = new DotNetNuke.Modules.ActiveForums.Controllers.TagController().GetById(TagId);
                    if (_tagInfo == null)
                    {
                        _tagInfo = new DotNetNuke.Modules.ActiveForums.Entities.TagInfo();
                    }
                }
                return _tagInfo;
            }
        }

        [IgnoreColumn()]
        public DotNetNuke.Modules.ActiveForums.Entities.TopicInfo Topic
        {
            get
            {
                if (_topicInfo == null)
                {
                    _topicInfo = new DotNetNuke.Modules.ActiveForums.Controllers.TopicController().GetById(TopicId);
                    if (_topicInfo == null)
                    {
                        _topicInfo = new DotNetNuke.Modules.ActiveForums.Entities.TopicInfo(); 
                        _topicInfo.Content = new DotNetNuke.Modules.ActiveForums.Entities.ContentInfo();

                    }
                }
                return _topicInfo;
            }
        }
    }
}
