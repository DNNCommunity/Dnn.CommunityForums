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
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN doworkafaffdaafdfdfffdfdffd WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
//
using DotNetNuke.Modules.ActiveForums;
using System.Collections.Generic;
using System;

namespace DotNetNuke.Modules.ActiveForums.Controllers
{
    public static class MessageController
    {
        public static bool Send(DotNetNuke.Modules.ActiveForums.Entities.Message message)
        {
            try
            {
                var subs = new List<SubscriptionInfo>();
                var si = new SubscriptionInfo { Email = message.SendTo, DisplayName = string.Empty, LastName = string.Empty, FirstName = string.Empty };
                subs.Add(si);
                var oEmail = new Email
                {
                    PortalId = message.PortalId,
                    UseQueue = false,
                    Recipients = subs,
                    Subject = message.Subject,
                    From = message.SendFrom,
                    BodyText = message.BodyText,
                    BodyHTML = message.Body,
                };
                try
                {
                    var objThread = new System.Threading.Thread(oEmail.Send);
                    objThread.Start();
                    return true;
                }
                catch (Exception ex)
                {
                    DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                    return false;
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                return false;
            }
        }
    }
}