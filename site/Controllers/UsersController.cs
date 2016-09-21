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

using LunchScheduler.Models;
using LunchScheduler.Site.Helpers;
using Microsoft.Azure.Mobile.Server.Config;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace LunchScheduler.Site.Controllers
{
    /// <summary>
    /// Provides info about users and updates users.
    /// </summary>
    [MobileAppController]
    public class UsersController : ControllerBase
    {
        /// <summary>
        /// Gets a user with the given email, but with secure info (auth, devices) removed.
        /// </summary>
        public async Task<User> Get([FromUri]string email)
        {
            var user = await Db.Users.FirstOrDefaultAsync(x => x.Email == email);
            if (null != user)
            {
                user.Auth = null;
                user.Devices = null; 
            }
            return user; 
        }

        /// <summary>
        /// Updates a user.
        /// </summary>
        [AuthRequired]
        public async Task<HttpResponseMessage> Post([FromBody]User update)
        {
            var match = await Db.Users.FirstOrDefaultAsync(x => x.Email == update.Email);
            if (match?.Devices.Count < update.Devices.Count)
            {
                foreach (var device in update.Devices.Where(x => match.Devices.All(y => y.Id != x.Id)))
                {
                    device.PhoneNumber = NotificationHelper.ScrubNumber(device.PhoneNumber);
                    NotificationHelper.SendText(device, 
                        "Welcome to Lunchtime! Please respond to verify your number.");
                }
            }
            await Db.SaveChangesAsync(); 
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}