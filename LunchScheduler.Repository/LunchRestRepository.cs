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
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LunchScheduler.Models;
using Newtonsoft.Json;

namespace LunchScheduler.Repository
{
    public class LunchRestRepository : ILunchRepository
    {
        private readonly string _baseUrl;
        private readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        public LunchRestRepository(string url)
        {
            _baseUrl = url;
        }

        public User User { get; set; }

        public async Task<User> LoginAsyc(AuthenticationProviderKind provider, string token)
        {
            var user = await PostAsync<User>("login",
                new { Provider = provider.ToString(), Token = token });
            User = user;
            return user;
        }

        public async Task LogoutAsync()
        {
            await PostAsync("logout", User);
            User = null;
        }

        public async Task<IEnumerable<Restaurant>> GetRestaurantsAsync(string location) => 
            await GetAsync<IEnumerable<Restaurant>>("restaurants", new { Location = location });

        public async Task<IEnumerable<Restaurant>> GetRestaurantsAsync(double lat, double lng) =>
            await GetRestaurantsAsync($"{lat},{lng}");

        public async Task CreateLunchAsync(Lunch lunch) =>
            await PostAsync<Lunch>("lunch", lunch);

        public async Task CancelLunchAsync(Lunch lunch) => 
            await DeleteAsync("lunch", lunch); 

        public async Task RespondToInvitationAsync(Invitation invite) =>
            await PostAsync<Invitation>("invitation", invite);

        public async Task<IEnumerable<User>> GetSuggestedFriendsAsync() =>
            await GetAsync<IEnumerable<User>>("friends/suggested"); 

        public async Task AddFriendAsync(Friendship friend) =>
            await PostAsync<Friendship>("friends", friend);

        public async Task RemoveFriendAsync(Friendship friend) => 
            await DeleteAsync("friends", friend);

        public async Task CreateGroupAsync(Group group) =>
            await PostAsync<Group>("groups", group);

        public async Task DeleteGroupAsync(Group group) =>
            await DeleteAsync("groups", group);

        public async Task JoinGroupAsync(GroupMembership membership) =>
            await PostAsync<GroupMembership>("groups/membership", membership);

        public async Task LeaveGroupAsync(GroupMembership membership) =>
            await DeleteAsync("groups/membership", membership);

        public async Task<IEnumerable<User>> GetFriendsAsync() => 
            await GetAsync<IEnumerable<User>>("me/friends"); 

        public async Task<IEnumerable<Lunch>> GetLunchesAsync() => 
            await GetAsync<IEnumerable<Lunch>>("me/lunches");

        public async Task<IEnumerable<Invitation>> GetInvitationsAsync() => 
            await GetAsync<IEnumerable<Invitation>>("me/invitations");

        private async Task<T> GetAsync<T>(string endpoint)
        {
            using (var client = CreateHttpClient())
            {
                var response = await client.GetAsync(endpoint);
                return await HandleResponse<T>(response);
            }
        }

        private async Task<T> GetAsync<T>(string endpoint, object parameters)
        {
            using (var client = CreateHttpClient())
            {
                string urlParams = String.Join("&", parameters.GetType().GetRuntimeProperties()
                    .Select(x => $"{x.Name}={x.GetValue(parameters)}")) ?? ""; 
                var response = await client.GetAsync($"{endpoint}?{urlParams}");
                return await HandleResponse<T>(response);
            }
        }

        private async Task PostAsync(string endpoint, object content)
        {
            using (HttpClient client = CreateHttpClient())
            {
                string json = JsonConvert.SerializeObject(content, _jsonSettings); 
                var response = await client.PostAsync(endpoint, new StringContent(
                    json, Encoding.UTF8, "application/json")); 
                HandleResponse(response);
            }
        }

        private async Task<T> PostAsync<T>(string endpoint, object content)
        {
            using (HttpClient client = CreateHttpClient())
            {
                string json = JsonConvert.SerializeObject(content, _jsonSettings);
                var response = await client.PostAsync(endpoint, new StringContent(
                    json, Encoding.UTF8, "application/json"));
                return await HandleResponse<T>(response);
            }
        }

        private async Task DeleteAsync(string endpoint, object content)
        {
            using (HttpClient client = CreateHttpClient())
            {
                var encode = content.GetType().GetRuntimeProperties()
                    .ToDictionary(x => x.Name, x => x.GetValue(content).ToString());
                var response = await client.DeleteAsync(
                    $"{endpoint}?{new FormUrlEncodedContent(encode).ToString()}");
                HandleResponse(response); 
            }
        }

        private void HandleResponse(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw new LunchApiException(response);
            }
        }

        private async Task<T> HandleResponse<T>(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    string json = await response.Content.ReadAsStringAsync();
                    T data = JsonConvert.DeserializeObject<T>(json);
                    return data;
                }
                catch (Exception)
                {
                    throw;
                }
            }
            else
            {
                throw new LunchApiException(response); 
            }
        }

        private HttpClient CreateHttpClient()
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri(_baseUrl)
            };
            if (User != null)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    "Bearer", User.AuthorizationToken);
            }
            return client;
        }
    }

    public class LunchApiException : Exception
    {
        public LunchApiException()
        { }

        public LunchApiException(string message) : base(message)
        { }

        public LunchApiException(HttpResponseMessage httpResponse) :
            base($"{httpResponse.StatusCode}: {httpResponse.ReasonPhrase}")
        { }
    }
}
