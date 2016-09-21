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
using System.Threading.Tasks;
using System.Web.Http;

namespace LunchScheduler.Site.Controllers
{
    /// <summary>
    /// Handles user login.
    /// </summary>
    [MobileAppController]
    public class LoginController : ControllerBase
    {
        /// <summary>
        /// Authenticates a user.
        /// </summary>
        public async Task<User> Post([FromBody]ProviderInfo request)
        {
            var user = await Db.Users.FirstOrDefaultAsync(x => x.Email == request.Email);
            if (null == user)
            {
                return null;
            }
            ProviderInfo info = await IdpHelper.GetUserInfoAsync(request.Provider, request.Token);
            if (info.Email == request.Email)
            {
                user.Auth = info;
                return user;
            }
            return null;
        }
    }
}