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
using Microsoft.Azure.Mobile.Server.Config;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Authenticators;
using System.Collections.Generic;
using System.Linq;

namespace LunchScheduler.Site.Controllers
{
    /// <summary>
    /// Handles suggesting locations to have lunch.
    /// </summary>
    [MobileAppController]
    [AuthRequired]
    public class LocationsController : ControllerBase
    {
        /// <summary>
        /// Yelp API consumer key. See https://www.yelp.com/developers/documentation/v2/authentication.
        /// </summary>
        private const string ConsumerKey = "<TODO: Your Yelp consumer key here>";

        /// <summary>
        /// Yelp API consumer secret. See https://www.yelp.com/developers/documentation/v2/authentication.
        /// </summary>
        private const string ConsumerSecret = "<TODO: Your Yelp consumer secret here>";

        /// <summary>
        /// Yelp API token. See https://www.yelp.com/developers/documentation/v2/authentication. 
        /// </summary>
        private const string Token = "<TODO: Your Yelp token here>";

        /// <summary>
        /// Yelp API token secret. See https://www.yelp.com/developers/documentation/v2/authentication. 
        /// </summary>
        private const string TokenSecret = "<TODO: Your Yelp token secret here>";

        /// <summary>
        /// Base url for the Yelp API.
        /// </summary>
        private const string BaseYelpUrl = "http://api.yelp.com/v2/search"; 

        /// <summary>
        /// Gets top-rated lunch locations near the given coordinates using the Yelp API.
        /// </summary>
        public IEnumerable<Location> Get(double latitude, double longitude)
        {
            if (longitude == 0 || latitude == 0)
            {
                return null;
            }
            var client = new RestClient(BaseYelpUrl)
            {
                Authenticator = OAuth1Authenticator.ForProtectedResource(
                    ConsumerKey, ConsumerSecret, Token, TokenSecret)
            };
            var request = new RestRequest(Method.GET);
            request.AddParameter("term", "lunch");
            request.AddParameter("ll", $"{latitude},{longitude}");
            var result = client.Execute(request);
            JToken token = JToken.Parse(result.Content); 

            var yelpLocations = token["businesses"].Select(x => new Location
            {
                Name = x["name"].ToString(),
                ImageUrl = x["image_url"].ToString(),
                Address = x["location"]["address"].First.ToString(),
                Rating = float.Parse(x["rating"].ToString())
            }).OrderBy(x => x.Rating);
            return yelpLocations;
        }
    }
}