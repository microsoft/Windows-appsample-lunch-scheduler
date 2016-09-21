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
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace LunchScheduler.Site.Helpers
{
    /// <summary>
    /// Wrapper for authentication calls to identity providers (IDPs). 
    /// </summary>
    public static class IdpHelper
    {
        /// <summary>
        /// Facebook App Id. Set at https://developers.facebook.com/apps. 
        /// </summary>
        private const string AppId = "<TODO: Your Facebook app Id here>";

        /// <summary>
        /// Facebook app secret. Set at https://developers.facebook.com/apps.
        /// </summary>
        private const string AppSecret = "<TODO: Your Facebook app secret here>";

        /// <summary>
        /// Gets info on a user from their provider's API.
        /// </summary>
        public static Task<ProviderInfo> GetUserInfoAsync(ProviderType type, string token)
        {
            try
            {
                switch (type)
                {
                    case ProviderType.Msa:
                        return GetMsaUserInfoAsync(token);
                    case ProviderType.Facebook:
                        return GetFacebookUserInfoAsync(token);
                    default:
                        return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets info on a MSA user via the Live API using the given token. 
        /// </summary>
        private static async Task<ProviderInfo> GetMsaUserInfoAsync(string token)
        {
            using (var client = new HttpClient())
            {
                var result = await client.GetAsync(@"https://apis.live.net/v5.0/me?access_token=" + 
                    token);
                string content = await result.Content.ReadAsStringAsync();
                var jtoken = JToken.Parse(content);
                if (null != jtoken["error"])
                {
                    return null;
                }
                var info = new ProviderInfo
                {
                    AccountId = jtoken["id"].ToString(),
                    Name = jtoken["name"].ToString(),
                    Token = token,
                    Provider = ProviderType.Msa,
                    Email = jtoken["emails"]["account"].ToString(),
                    PhotoUrl = "https://apis.live.net/v5.0/" + jtoken["id"] + "/picture",
                };
                return info;
            }
        }

        /// <summary>
        /// Gets info on a Facebook user via the Graph API using the given token. 
        /// </summary>
        private static async Task<ProviderInfo> GetFacebookUserInfoAsync(string token)
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(
                    "https://graph.facebook.com/me?fields=id,name,email,picture.width(500)&access_token="
                    + token);
                string content = await response.Content.ReadAsStringAsync();
                JToken jtoken = JToken.Parse(content);
                if (null != jtoken["error"])
                {
                    return null;
                }
                var info = new ProviderInfo
                {
                    AccountId = jtoken["id"].ToString(),
                    Email = jtoken["email"].ToString(),
                    Name = jtoken["name"].ToString(),
                    PhotoUrl = jtoken["picture"]["data"]["url"].ToString(),
                    Token = token,
                    Provider = ProviderType.Facebook
                };
                await RequestLongLivedTokenAsync(info);
                return info;
            }
        }

        /// <summary>
        /// Requests a long-lived Facebook token and replaces the existing token with it. 
        /// Long-lived tokens last 60 days and refresh automatically when used. 
        /// </summary>
        private static async Task RequestLongLivedTokenAsync(ProviderInfo info)
        {
            Uri uri = new Uri(Uri.EscapeUriString($@"https://graph.facebook.com/oauth/" +
                $"access_token?grant_type=fb_exchange_token&client_id={AppId}&" +
                $"client_secret={AppSecret}&fb_exchange_token={info.Token}"));

            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(uri);
                var content = await response.Content.ReadAsStringAsync();
                try
                {
                    string longToken = content.Split('=', '&')[1];
                    if (longToken.Length > 50)
                    {
                        info.Token = longToken;
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    // If the swap fails, do nothing.
                }
            }
        }
    }
}