//
// Community Forums
// Copyright (c) 2013-2021
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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace DotNetNuke.Modules.ActiveForums
{
	public class ControlsConfig
	{
        #region Private Members
        #endregion
        #region Public Properties
        public string AdminRoles { get; set; }
     //   public string AppPath { get; set; }
     //   public int ContentId { get; set; }
        public string DefaultViewRoles { get; set; }
     //   public int ForumId { get; set; }
        public int InstanceId { get; set; }
        //public string MembersLink { get; set; }
        public int PageId { get; set; } = -1;
      //  public string ProfileLink { get; set; }
        public int SiteId { get; set; }
        public string TemplatePath { get; set; }
     //  public string ThemePath { get; set; }
     //   public int TopicId { get; set; }
        public User User { get; set; }

        #endregion


    }
}

