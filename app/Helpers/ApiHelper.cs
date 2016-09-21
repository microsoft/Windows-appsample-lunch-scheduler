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

using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LunchScheduler.App.Helpers
{
    /// <summary>
    /// Wrapper for making strongly-typed REST calls to the Lunch Scheduler web service.
    /// </summary>
    public class ApiHelper
    {
        /// <summary>
        /// Gets or sets whether to use an Azure URL or locahost for API calls. 
        /// </summary>
        public bool IsDebug { get; set; } = true; 

        /// <summary>
        /// The Base URL for the API. Switch to localhost for debugging. 
        /// </summary>
        private string BaseUrl => IsDebug ? @"http://localhost:16274/api/" : 
            @"https://<your_azure_url_here>.azurewebsites.net/api/";

        /// <summary>
        /// Makes an HTTP GET request to the given controller and returns the HttpResponseMessage.
        /// </summary>
        public async Task<HttpResponseMessage> GetAsync(string controller)
        {
            try
            {
                using (var client = BaseClient())
                {
                    return await client.GetAsync(controller);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in GetAsync({controller}):\r\n{ex.Message})");
                return null;
            }
        }

        /// <summary>
        /// Makes an HTTP GET request to the given controller and returns the deserialized response content.
        /// </summary>
        public async Task<T> GetAsync<T>(string controller)
        {
            try
            {
                using (var client = BaseClient())
                {
                    var response = await client.GetAsync(controller);
                    string json = await response.Content.ReadAsStringAsync();
                    T obj = JsonConvert.DeserializeObject<T>(json);
                    return obj;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in GetAsync<{(typeof(T)).Name}>({controller}):\r\n{ex.Message})");
                return default(T); 
            }
        }

        /// <summary>
        /// Makes an HTTP GET request to the given controller and includes all the given
        /// object's properties as URL parameters. Returns the deserialized response content.
        /// </summary>
        public async Task<T> GetAsync<T>(string controller, object request)
        {
            try
            {
                using (var client = BaseClient())
                {
                    var parameters = String.Join("&", request.GetType().GetRuntimeProperties().Select(x => 
                        $"{x.Name}={x.GetValue(request)}")); 
                    var response = await client.GetAsync($"{controller}?{parameters}"); 
                    string json = await response.Content.ReadAsStringAsync();
                    T obj = JsonConvert.DeserializeObject<T>(json);
                    return obj;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in GetAsync<{(typeof(T)).Name}>({controller}):\r\n{ex.Message})");
                return default(T);
            }
        }

        /// <summary>
        /// Makes an HTTP GET request to the given controller and includes all the selected properties 
        /// of the given object's as URL parameters. Returns the deserialized response content.
        /// </summary>
        public async Task<TU> GetAsync<T, TU>(string controller, T request, params Func<T, object>[] selectors)
        {
            try
            {
                using (var client = BaseClient())
                {
                    var parameters = new StringBuilder();
                    parameters.Append("?");
                    foreach (var item in selectors)
                    {
                        parameters.Append($"{item.GetMethodInfo().Name}&{Uri.EscapeDataString(item(request).ToString())}"); 
                    }
                    var response = await client.GetAsync($"{controller}?{parameters}");
                    string json = await response.Content.ReadAsStringAsync();
                    TU obj = JsonConvert.DeserializeObject<TU>(json);
                    return obj;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in GetAsync<{(typeof(T)).Name}>({controller}):\r\n{ex.Message})");
                return default(TU);
            }
        }

        /// <summary>
        /// Makes an HTTP POST request to the given controller with the given object as the body.
        /// Returns HttpResponseMessage.
        /// </summary>
        public async Task<HttpResponseMessage> PostAsync<T>(string controller, T body)
        {
            try
            {
                using (var client = BaseClient())
                {
                    return await client.PostAsync(controller, new JsonStringContent(body));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in GetAsync<{(typeof(T)).Name}>({controller}):\r\n{ex.Message})");
                return null; 
            }
        }

        /// <summary>
        /// Makes an HTTP POST request to the given controller with the given object as the body.
        /// Returns the deserialized response content.
        /// </summary>
        public async Task<TU> PostAsync<T, TU>(string controller, T body)
        {
            try
            {
                using (var client = BaseClient())
                {
                    string str = JsonConvert.SerializeObject(body); 
                    var response = await client.PostAsync(controller, new JsonStringContent(body));
                    string json = await response.Content.ReadAsStringAsync();
                    TU obj = JsonConvert.DeserializeObject<TU>(json);
                    return obj;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in GetAsync<{(typeof(T)).Name}, {typeof(TU).Name}>({controller}):\r\n{ex.Message})");
                return default(TU);
            }
        }

        /// <summary>
        /// Constructs the base HTTP client, including correct authorization and API version headers.
        /// </summary>
        private HttpClient BaseClient()
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri(BaseUrl)
            };
            client.DefaultRequestHeaders.Add("ZUMO-API-VERSION", "2.0.0");
            if (null != App.User)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    App.User.Auth.Provider.ToString(), App.User.Auth.Token);
                client.DefaultRequestHeaders.From = Uri.EscapeUriString(App.User.Email);
            }
            return client;
        }

        /// <summary>
        /// Helper class for formatting StringContent as Json. 
        /// </summary>
        private class JsonStringContent : StringContent
        {
            /// <summary>
            /// Creates new JsonStringContent, which is just StringContent formatted as application/json.
            /// </summary>
            public JsonStringContent(object obj)
                : base(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json")
            {
            }
        }
    }
}