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

namespace DotNetNuke.Modules.ActiveForums.Controllers
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Controller for managing Attachments in the DNN Community Forums module.
    /// </summary>
    internal class AttachmentController : DotNetNuke.Modules.ActiveForums.Controllers.RepositoryControllerBase<DotNetNuke.Modules.ActiveForums.Entities.AttachmentInfo>
    {
        internal IEnumerable<DotNetNuke.Modules.ActiveForums.Entities.AttachmentInfo> GetByContentId(int contentId)
        {
            return this.Find("WHERE ContentId = @0", contentId);
        }

        internal new void Delete(DotNetNuke.Modules.ActiveForums.Entities.AttachmentInfo attachmentInfo)
        {
            base.Delete(attachmentInfo);
        }

        internal new DotNetNuke.Modules.ActiveForums.Entities.AttachmentInfo Insert(DotNetNuke.Modules.ActiveForums.Entities.AttachmentInfo attachmentInfo)
        {
            base.Insert(attachmentInfo);
            return this.GetById(attachmentInfo.AttachmentId);
        }

        internal new DotNetNuke.Modules.ActiveForums.Entities.AttachmentInfo Update(DotNetNuke.Modules.ActiveForums.Entities.AttachmentInfo attachmentInfo)
        {
            base.Update(attachmentInfo);
            return this.GetById(attachmentInfo.AttachmentId);
        }

        internal new DotNetNuke.Modules.ActiveForums.Entities.AttachmentInfo Save(DotNetNuke.Modules.ActiveForums.Entities.AttachmentInfo attachmentInfo)
        {
            this.Save(attachmentInfo, attachmentInfo.AttachmentId);
            return this.GetById(attachmentInfo.AttachmentId);
        }
    }
}
