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
using System;
using System.Data.Entity;
using System.Diagnostics;
using System.Net;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Net.Http;

namespace LunchScheduler.Site
{
    /// <summary>
    /// Attribute that indicates a controller requires authorization to access.
    /// </summary>
    public class AuthRequired : FilterAttribute, IAuthorizationFilter
    {
        private LunchSchedulerContext _db;
        private HttpActionContext _context;

        /// <summary>
        /// Runs whenever a controller decorated with the [AuthRequired] attribute is called
        /// to determine whether the user is authorized (continue) or not (return 403 (Unauthorized)). 
        /// </summary>
        public async Task<HttpResponseMessage> ExecuteAuthorizationFilterAsync(HttpActionContext actionContext,
            CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation)
        {
            _context = actionContext;
            _db = new LunchSchedulerContext();
            try
            {
                var result = await Authorize();
                if (!result.IsSuccessStatusCode)
                {
                    return result;
                }
                return await continuation(); 
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failiure authenticating: " + ex.Message);
                return _context.Request.CreateResponse(HttpStatusCode.Unauthorized);
            }
        }

        /// <summary>
        /// Checks that the user's claimed identity (email and token) matches 
        /// thier actual identity according to their provider. 
        /// </summary>
        private async Task<HttpResponseMessage> Authorize()
        {
            // TODO: Consider more robust security.
            // This sample provides a basic demonstration of how to use tokens to communicate 
            // with identity providers (IDPs) and authorize users. However, in a production application, 
            // making repeated calls to IDPs is not reccomended as it is not performant and may result 
            // in your app being rate-limited. Instead, you should perform an initial verification of 
            // the user's identity, and then persist it on your own token (or a similar mechanism). 

            string email = _context.Request.Headers.From;
            string token = _context.Request.Headers.Authorization.Parameter;
            ProviderType provider;
            bool providerParsed = Enum.TryParse<ProviderType>(
                _context.Request.Headers.Authorization.Scheme, out provider);

            if (!providerParsed || String.IsNullOrEmpty(email) || String.IsNullOrEmpty(token))
            {
                return _context.Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            var user = await _db.Users.FirstOrDefaultAsync(x => x.Email == email);
            if (null == user)
            {
                return _context.Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            ProviderInfo response = await IdpHelper.GetUserInfoAsync(provider, token);

            if (response?.Email != email)
            {
                return _context.Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            _context.ControllerContext.RequestContext.Principal =
                new GenericPrincipal(new GenericIdentity(email), null);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
