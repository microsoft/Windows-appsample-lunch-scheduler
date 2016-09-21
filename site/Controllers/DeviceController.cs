//  ---------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// 
//  The MIT License (MIT)
// 
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
// 
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
// 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
//  ---------------------------------------------------------------------------------

using LunchScheduler.Site.Helpers;
using Microsoft.Azure.Mobile.Server.Config;
using Newtonsoft.Json.Linq;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace LunchScheduler.Site.Controllers
{
    /// <summary>
    /// Handles device verification.
    /// </summary>
    [MobileAppController]
    public class DeviceController : ControllerBase
    {
        /// <summary>
        /// Returns whether a phone number exists and is verified. 
        /// </summary>
        public async Task<bool> Get(string number)
        {
            var user = await Db.Users.FirstOrDefaultAsync(x => x.Devices.Any(y => y.PhoneNumber.Contains(number)));
            return null != user && user.Devices.Any(y => y.Verified);
        }

        /// <summary>
        /// Processes text messages received from the Twilio service.
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]JToken message)
        {
            string twilioMessage = "Thanks!"; 
            try
            {
                string number = NotificationHelper.ScrubNumber(message["From"].ToString());
                foreach (var user in Db.Users.Where(x => x.Devices.Any(y => y.PhoneNumber == number)))
                {
                    foreach (var device in user.Devices.Where(x => x.PhoneNumber == number))
                    {
                        device.Verified = true;
                    }
                }
                await Db.SaveChangesAsync(); 
            }
            catch (Exception ex)
            {
                twilioMessage = $"Exception: {ex.Message}. Input: {message}"; 
            }
            return NotificationHelper.CreateTwilioHttpResponse(twilioMessage); 
        }
    }
}