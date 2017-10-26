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
//
//  Microsoft License for use of Images
//
//  Microsoft grants you a worldwide, non-exclusive, non-transferrable, revocable, 
//  royalty-free license to use the Microsoft photographs or images contained in this
//  Microsoft sample project, Lunch Scheduler, (“Images”) solely for your purposes
//  of internal using or testing the sample application.You may not copy, modify,
//  reproduce, distribute, publicly display, offer for sale,
//  sell, market, or promote the Microsoft Images.
//  ---------------------------------------------------------------------------------

namespace LunchScheduler.Models
{
    /// <summary>
    /// Contains constant variables like API keys and app secrets. You'll need to modify some of these 
    /// values before running this service on your own machines or deploying it to Azure.
    /// <para />
    /// NOTE: These values are aggregated here for convenience. In a real dev environment, never store sensitive
    /// data (like API keys) in code or source control. Instead, use a more secure method, such as machine environment 
    /// variables. For more information, see: https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Indicates whether the app is running in demo mode. 
        /// </summary>
        public const bool IsDemoMode = true;

        /// <summary>
        /// Indicates whether the app is not running in demo mode. 
        /// </summary>
        public const bool IsNotDemoMode = !IsDemoMode;

        /// <summary>
        ///  The API endpoint for the app. Use this to connect the app to the LunchScheduler.Api service project. 
        /// </summary>
        public const string ApiUrl = "";

        /// <summary>
        /// The database connection string for an Azure SQL database. Get this using the Azure portal.
        /// </summary>
        public const string SqlAzureConnectionString = "";

        /// The Yelp client Id. Required for Yelp API calls. 
        /// See https://www.yelp.com/developers/documentation/v3 for info on getting an Id.  
        /// </summary>
        public const string YelpClientId = "";

        /// <summary>
        /// The Yelp client secret Id. Required for Yelp API calls. 
        /// See https://www.yelp.com/developers/documentation/v3 for info on getting a secret.  
        /// </summary>
        public const string YelpClientSecret = "";

        /// <summary>
        /// The JSON Web Token secret key. Used by LunchScheduler.Api to issue secure authentication/authorization 
        /// tokens to the app. See https://jwt.io/ for an overview.
        /// </summary>
        public const string JwtSecretKey = "";

        /// <summary>
        /// The JSON Web Token issuer. 
        /// </summary>
        public const string JwtIssuer = "lunch-scheduler";

        /// <summary>
        /// The JSON Web Token audience. 
        /// </summary>
        public const string JwtAudience = "lunch-scheduler-user";

        /// <summary>
        /// The Facebook Id. Register with https://developers.facebook.com to retrieve an app ID, and a Microsoft Store ID.
        /// </summary>
        public const string FacebookAppId = "";

        /// <summary>
        /// The Azure App Id. Register with https://apps.dev.microsoft.com to retrieve an client/application ID.
        /// </summary>
        public const string GraphAppId = "";

        /// <summary>
        /// The Azure redirect Uri. The Uri is unique to each Windows 10 app to ensure messages sent to that 
        /// Uri are only received by that app. 
        /// </summary>
        public const string GraphRedirectUri = "";

        /// <summary>
        /// The Microsoft Graph permission scopes required by the app. 
        /// </summary>
        public static string[] GraphScopes = { "User.Read", "Mail.Send", "User.ReadBasic.All" };

        /// <summary>
        // The Bing Maps registration key. Register with https://www.bingmapsportal.com for a key. 
        /// </summary>
        public const string MapServiceToken = "";

        /// <summary>
        /// The number of miles in a meter. Used for conversions with the Yelp API. 
        /// </summary>
        public const double MilesPerMeter = 0.00062137;

        /// <summary>
        /// Microsoft campus address for finding restaurants in demo mode.
        /// </summary>
        public const string DemoAddress = "One Microsoft Way, Redmond, WA";

        /// <summary>
        /// Microsoft campus lat for finding restaurants in demo mode.
        /// </summary>
        public const double DemoLatitude = 47.640068;

        /// <summary>
        /// Microsoft campus lng for finding restaurants in demo mode.
        /// </summary>
        public const double DemoLongitude = -122.129858;
    }
}
