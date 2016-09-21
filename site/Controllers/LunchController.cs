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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace LunchScheduler.Site.Controllers
{
    /// <summary>
    /// Gets existing lunches and creates new ones.
    /// </summary>
    [MobileAppController]
    [AuthRequired]
    public class LunchController : ControllerBase
    {
        /// <summary>
        /// Gets all lunches the current user has created or is invited to. 
        /// </summary>
        public IEnumerable<Lunch> Get()
        {
            DateTime lookback = DateTime.UtcNow.Subtract(new TimeSpan(24, 0, 0));
            var result = Db.Lunches.Where(x => (x.Host.Email == User.Identity.Name ||
                x.Invites.Any(y => y.User.Email == User.Identity.Name)) &&
                x.Time > lookback).ToList();
            return result;
        }

        /// <summary>
        /// Inserts a new lunch into the database, then notifies all invitees. 
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Lunch lunch)
        {
            Db.Lunches.Add(lunch);
            await Db.SaveChangesAsync(); 
            await NotifyAsync(lunch);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

        /// <summary>
        /// Notifies all invitees about a new lunch.
        /// </summary>
        private async Task NotifyAsync(Lunch lunch)
        {
            foreach (var invite in lunch.Invites)
            {
                foreach (var textDevice in invite.User.Devices?.Where(x => x.NotificationType == NotificationType.Text))
                {
                    NotifyText(textDevice, lunch); 
                }
                await NotifyWindowsAppAsync(new Device {Tag = invite.User.Email}, lunch); 
            }

            foreach (var textDevice in lunch.Host.Devices.Where(x => x.NotificationType == NotificationType.Text))
            {
                NotifyText(textDevice, lunch);
            }
            await NotifyWindowsAppAsync(new Device { Tag = lunch.Host.Email }, lunch);
        }

        /// <summary>
        /// Sends a toast to Windows devices about the new lunch.
        /// </summary>
        private async Task NotifyWindowsAppAsync(Device device, Lunch lunch)
        {
            string message = $"{lunch.Host.Name} wants to get lunch {GetDateTime(lunch)}!";
            await NotificationHelper.SendWindowsNotificationAsync(device, message);
        }

        /// <summary>
        /// Sends a text notification to a device about the new lunch.
        /// </summary>
        private void NotifyText(Device device, Lunch lunch)
        {
            string message = $"{lunch.Host.Name} invited you to lunch at {lunch.Location.Name} " +
                $"{GetDateTime(lunch)}!";
            NotificationHelper.SendText(device, message);
        }

        /// <summary>
        /// Converts a DateTime to a nicer phrased string (e.g., today/tomorrow). 
        /// </summary>
        private string GetDateTime(Lunch lunch)
        {
            string time = lunch.Time.ToShortTimeString();
            if (DateTime.Now.Date == lunch.Time.Date)
            {
                return "today at " + time;
            }
            else if (DateTime.Now.Date.AddDays(1) == lunch.Time.Date)
            {
                return "tomorrow at " + time;
            }
            else
            {
                return $"on {DateTime.Now.ToString("MM/dd")} at {time}";
            }
        }
    }
}