// Copyright (c) 2013-2024 by DNN Community
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
    using System.Runtime.Serialization;

    // Used to exchange data with SQL
    public class Attachment
    {
        public int AttachmentId { get; set; }

        public int ContentId { get; set; }

        public int UserId { get; set; }

        public string FileName { get; set; }

        public long FileSize { get; set; }

        public string ContentType { get; set; }

        public int? FileId { get; set; }

        public byte[] FileData { get; set; }

        public bool? AllowDownload { get; set; }
    }

    public class PermissionAttachment : Attachment
    {
        public string CanRead { get; set; }
    }

    // Used to exchange data with the client via JSON
    [DataContract]
    public class ClientAttachment
    {
        [DataMember(Name = "id", IsRequired = false, EmitDefaultValue = false)]
        public int? AttachmentId { get; set; }

        [DataMember(Name = "fileName", IsRequired = true)]
        public string FileName { get; set; }

        [DataMember(Name = "contentType", IsRequired = true)]
        public string ContentType { get; set; }

        [DataMember(Name = "fileSize")]
        public long FileSize { get; set; }

        [DataMember(Name = "fileId", IsRequired = false, EmitDefaultValue = false)]
        public int? FileId { get; set; }

        [DataMember(Name = "uploadId", IsRequired = false, EmitDefaultValue = false)]
        public string UploadId { get; set; }
    }
}
