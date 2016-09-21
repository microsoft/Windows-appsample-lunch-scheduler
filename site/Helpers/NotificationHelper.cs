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
using Microsoft.Azure.NotificationHubs;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Twilio;
using Twilio.Lookups;
using Twilio.TwiML;
using Task = System.Threading.Tasks.Task;

namespace LunchScheduler.Site.Helpers
{
    /// <summary>
    /// Wrapper for sending/receiving notifications using Azure notification hubs and Twilio.
    /// </summary>
    public static class NotificationHelper
    {
        /// <summary>
        /// SID for Twilio. Configure at https://www.twilio.com/user/account/phone-numbers/incoming.
        /// </summary>
        private const string TwilioSid = "<TODO: Your Twilio SID here>";

        /// <summary>
        /// Key for twilio. Configure at https://www.twilio.com/user/account/phone-numbers/incoming. 
        /// </summary>
        private const string TwilioKey = "<TODO: Your Twilio key here>";

        /// <summary>
        /// Outgoing phone number for Twilio. Configure at https://www.twilio.com/user/account/phone-numbers/incoming.
        /// </summary>
        private const string TwilioOutgoingNumber = "<TODO: Your Twilio number here>";

        /// <summary>
        /// Name of the Azure notification hub. 
        /// </summary>
        private const string HubName = "<TODO: Your hub name here>";

        /// <summary>
        /// Endpoint for the Azure notification hub. Configure at https://www.portal.azure.com
        /// </summary>
        private const string HubEndpoint = "<TODO: Your endpoint here>";

        /// <summary>
        /// Client for interacting with the Azure notification hubs REST API.
        /// </summary>
        private static readonly NotificationHubClient HubClient = 
            NotificationHubClient.CreateClientFromConnectionString(
                HubEndpoint, HubName); 

        /// <summary>
        /// Client for interacting with the Twilio REST API.
        /// </summary>
        private static readonly TwilioRestClient TwilioClient = new TwilioRestClient(TwilioSid, TwilioKey);

        /// <summary>
        /// Client for interacting with the Twilio Lookups REST API.
        /// </summary>
        private static readonly LookupsClient LookupClient = new LookupsClient(TwilioSid, TwilioKey);

        /// <summary>
        /// Sends a toast to Windows over the WNS using the Azure notification hub.
        /// </summary>
        public static async Task SendWindowsNotificationAsync(Device device, string text)
        {
            string payload = "<?xml version=\"1.0\" encoding=\"utf-8\"?><toast>" + 
                "<visual><binding template=\"ToastText01\"><text id=\"1\">" + text + 
                "</text></binding></visual></toast>";
            await HubClient.SendNotificationAsync(new WindowsNotification(payload), new[] { device.Tag }); 
        }

        /// <summary>
        /// Sends a text to a mobile phone using Twilio. 
        /// </summary>
        public static void SendText(Device device, string text) => 
            TwilioClient.SendMessage(TwilioOutgoingNumber, ScrubNumber(device.PhoneNumber), text);

        /// <summary>
        /// Creates an HTTP response to reply to a text received via Twilio.
        /// </summary>
        public static HttpResponseMessage CreateTwilioHttpResponse(string message)
        {
            var twilioResponse = new TwilioResponse();
            twilioResponse.Message(message);
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(twilioResponse.Element.ToString())
            };
            httpResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("application/xml");
            return httpResponse;
        }

        /// <summary>
        /// Converts a fuzzy phone number string to an E.164 compliant number.  
        /// </summary>
        public static string ScrubNumber(string number) => 
            LookupClient.GetPhoneNumber(number)?.PhoneNumber; 

    }
}