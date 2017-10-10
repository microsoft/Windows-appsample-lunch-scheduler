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
using System.Linq;
using System.Threading;
using LunchScheduler.Models;
using LunchScheduler.Repository;
using Microsoft.AspNetCore.Mvc;

namespace LunchScheduler.Api.Controllers
{
    public abstract class ControllerBase : Controller
    {
        private readonly string _sub = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";

        public ControllerBase(LunchContext repository)
        {
            _repository = repository;
            _user = new Lazy<User>(() => GetUser(),
                LazyThreadSafetyMode.ExecutionAndPublication);
        }

        private LunchContext _repository;
        public LunchContext Repository => _repository; 

        private Lazy<User> _user;
        protected new User User => _user.Value;

        protected User GetUser()
        {
            var claim = HttpContext.User.Claims.FirstOrDefault(x => x.Type == _sub); 
            if (claim != null && Guid.TryParse(claim.Value, out Guid id))
            {
                var user = Repository.Users.Find(id);
                return user; 
            }
            else
            {
                return null; 
            }
        }
    }
}
