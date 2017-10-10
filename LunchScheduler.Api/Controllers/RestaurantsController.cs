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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using LunchScheduler.Models;
using LunchScheduler.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace LunchScheduler.Api.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class RestaurantsController : ControllerBase
    {
        private string _token;

        public RestaurantsController(LunchContext repository) : base(repository)
        { }

        [HttpGet]
        public async Task<IEnumerable<Restaurant>> Get(string location)
        {
            return await GetRestaurantsAsync(location);
        }

        /// <summary>
        /// Returns all Yelp locations near the given input.
        /// </summary>
        public async Task<IEnumerable<Restaurant>> GetRestaurantsAsync(string location)
        {
            var response = await GetAsync($"businesses/search?location={location}");
            var restaurants = response["businesses"].Select(x => new Restaurant
            {
                YelpId = x["id"].Value<string>(),
                Name = x["name"].Value<string>(),
                Rating = x["rating"].Value<float>(),
                PhotoUrl = x["image_url"].Value<string>(),
                Latitude = x["coordinates"]["latitude"].Value<double>(),
                Longitude = x["coordinates"]["longitude"].Value<double>(),
                Address = x["location"]["address1"].Value<string>() +
                    x["location"]["address2"].Value<string>(),
                Distance = MetersToMiles(x["distance"].Value<double>()),
                Price = x["price"]?.Value<string>(),
                Category = String.Join(", ", x["categories"].Select(y => y["title"].Value<string>()))
            });
            return restaurants;
        }

        /// <summary>
        /// Returns JSON from the given Yelp API endpoint. 
        /// </summary>
        private async Task<JToken> GetAsync(string endpoint)
        {
            if (_token == null)
            {
                _token = await GetTokenAsync();
            }

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://api.yelp.com/v3/");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
                var response = await client.GetAsync(endpoint);
                string json = await response.Content.ReadAsStringAsync();
                return JToken.Parse(json);
            }
        }

        /// <summary>
        /// Obtains an authorization token for using the Yelp API. 
        /// </summary>
        private async Task<string> GetTokenAsync()
        {
            using (var client = new HttpClient())
            {
                var parameters = new Dictionary<string, string>
                {
                    { "grant_type", "client_credentials" },
                    { "client_id", Constants.YelpClientId },
                    { "client_secret", Constants.YelpClientSecret }
                };

                using (var content = new FormUrlEncodedContent(parameters))
                {
                    var response = await client.PostAsync("https://api.yelp.com/oauth2/token", content);
                    string json = await response.Content.ReadAsStringAsync();
                    return JToken.Parse(json)["access_token"].ToString();
                }
            }
        }

        /// <summary>
        /// Converts distance in meters to miles. 
        /// </summary>
        private static double MetersToMiles(double meters) => meters * Constants.MilesPerMeter; 
    }
}
