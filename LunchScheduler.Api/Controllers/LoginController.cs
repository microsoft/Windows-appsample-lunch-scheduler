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
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using LunchScheduler.Models;
using LunchScheduler.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;

namespace LunchScheduler.Api.Controllers
{
    public class LoginController : ControllerBase
    {
        public LoginController(LunchContext repository) : base(repository)
        { }

        [HttpPost("api/login")]
        public async Task<IActionResult> Login([FromBody]LoginRequest request)
        {
            // If the user already has a valid token attached to their request, log them in 
            // automatically. Otherwise, need to verify info with the provider. 
            // This flow does *not* refresh their token, which forces them to log in with the 
            // provider occasionally to re-validate their login (currently every 30 days). 

            if (User != null)
            {
                return Ok(User);
            }
            User user = await GetUserFromProviderAsync(request.Provider, request.Token);

            if (user == null)
            {
                return Unauthorized(); 
            }

            // Check the database to see if this user has registered before. 
            // If so, refresh their database info with the latest tokens and info, 
            // then return that user. Otherwise, create a new user. 
            // In either case, generate our own JWT and attach it so we don't 
            // need to hit their provider to verify their identity every API call. 

            User match = await Repository.Users.FirstOrDefaultAsync(x =>
                x.AuthenticationProviderKind == user.AuthenticationProviderKind &&
                x.AuthenticationProviderId == user.AuthenticationProviderId);

            var jwt = GenerateJwt(user.Id);
            user.AuthorizationToken = new JwtSecurityTokenHandler().WriteToken(jwt);
            user.AuthorizationTokenExpiration = jwt.ValidTo;

            if (match == null)
            {
                Repository.Add(user);
            }
            else
            {
                match.Name = user.Name;
                match.PhotoUrl = user.PhotoUrl; 
            }

            Repository.SaveChanges();

            // Return a new copy of the user that doesn't include any extra EF properties. 

            return Ok(new User
            {
                AuthenticationProviderId = user.AuthenticationProviderId,
                AuthenticationProviderKind = user.AuthenticationProviderKind,
                AuthorizationToken = user.AuthorizationToken,
                AuthorizationTokenExpiration = user.AuthorizationTokenExpiration,
                PhotoUrl = user.PhotoUrl,
                Name = user.Name,
                Id = user.Id
            }); 
        }

        [HttpPost("api/logout")]
        public async Task Logout(Guid userId)
        {
            var entity = await Repository.Users.FindAsync(User.Id);
            if (entity != null && entity.Id == User.Id)
            {
                entity.AuthorizationToken = null;
                entity.AuthorizationTokenExpiration = DateTime.MinValue;
                await Repository.SaveChangesAsync();
            }
        }

        private async Task<User> GetUserFromProviderAsync(AuthenticationProviderKind? provider, string token)
        {
            switch (provider)
            {
                case AuthenticationProviderKind.AzureActiveDirectory:
                    return await GetAadUserAsync(token);
                case AuthenticationProviderKind.Facebook:
                    return await GetFacebookUserAsync(token); 
                case AuthenticationProviderKind.Demo:
                    throw new NotSupportedException(); 
                default:
                    throw new NotImplementedException();
            }
        }

        private async Task<User> GetFacebookUserAsync(string token)
        {
            using (var client = new HttpClient())
            {
                string url = "https://graph.facebook.com/me?me?fields=id,name,picture";
                var response = await client.GetAsync(url); 
                if (response.IsSuccessStatusCode)
                {
                    JToken json = JToken.Parse(await response.Content.ReadAsStringAsync());
                    var user = new User
                    {
                        Name = json["name"].Value<string>(),
                        AuthenticationProviderId = json["id"].Value<string>(),
                        PhotoUrl = json["picture"]["data"]["url"].Value<string>(),
                        AuthenticationProviderKind = AuthenticationProviderKind.Facebook
                    };
                    return user; 
                }
                else
                {
                    return null; 
                }
            }
        }

        private async Task<User> GetAadUserAsync(string token)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await client.GetAsync("https://graph.microsoft.com/v1.0/me"); 
                if (response.IsSuccessStatusCode)
                {
                    JToken json = JToken.Parse(await response.Content.ReadAsStringAsync());
                    string id = json["id"].Value<string>();
                    var user = new User
                    {
                        Name = json["displayName"].Value<string>(),
                        AuthenticationProviderId = id,
                        AuthenticationProviderKind = AuthenticationProviderKind.AzureActiveDirectory,
                    };
                    return user;
                }
                else
                {
                    return null; 
                }
            }
        }

        private JwtSecurityToken GenerateJwt(Guid userId)
        {
            DateTime now = DateTime.UtcNow;
            var identity = new ClaimsIdentity(new GenericIdentity(userId.ToString(), "Token"));

            Claim[] claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64),
            };


            var jwt = new JwtSecurityToken(
                issuer: Constants.JwtIssuer,
                audience: Constants.JwtAudience,
                claims: claims,
                notBefore: now,
                expires: now.Add(new TimeSpan(30, 0, 0, 0)),
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(
                    Encoding.ASCII.GetBytes(Constants.JwtSecretKey)),
                    SecurityAlgorithms.HmacSha256));

            return jwt;
        }
    }

    public class LoginRequest
    {
        public AuthenticationProviderKind Provider { get; set; }
        public string Token { get; set; }
    }
}
