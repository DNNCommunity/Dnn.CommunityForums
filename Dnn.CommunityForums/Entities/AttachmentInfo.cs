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
    using System.Web.Caching;

    using DotNetNuke.ComponentModel.DataAnnotations;
    using DotNetNuke.Services.Tokens;

    [TableName("activeforums_Attachments")]
    [PrimaryKey("AttachID", AutoIncrement = true)]
    public class AttachmentInfo
    {
        [ColumnName("AttachID")]
        public int AttachmentId { get; set; }

        public int ContentId { get; set; }

        [ColumnName("UserID")]
        public int UserId { get; set; }

        [ColumnName("Filename")]
        public string FileName { get; set; }

        [ColumnName("FileID")]
        public int? FileId { get; set; }

        public string ContentType { get; set; }

        public DateTime? DateAdded { get; set; }

        public DateTime? DateUpdated { get; set; }

        public byte[] FileData { get; set; }

        public long FileSize { get; set; }

        public bool DisplayInline { get; set; } = false;
    }
}
