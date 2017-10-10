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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LunchScheduler.Models;
using LunchScheduler.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LunchScheduler.Api.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class MeController : ControllerBase
    {
        public MeController(LunchContext repository) : base(repository)
        { }

        [HttpGet("lunches")]
        private async Task<IEnumerable<Lunch>> Lunch()
        {
            var userHostedLunches = await Repository.Lunches
                .Include(x => x.Location)
                .Include(x => x.Host)
                .Include(x => x.Invitations)
                .ThenInclude(x => x.User)
                .Where(x => x.HostId == User.Id)
                .ToListAsync();

            var userAcceptedLunches = await Repository.Invitations
                .Where(x => x.UserId == User.Id && x.Response == InviteResponseKind.Accepted)
                .Select(x => x.Lunch)
                .Include(x => x.Host)
                .Include(x => x.Location)
                .Include(x => x.Invitations)
                .ThenInclude(x => x.User)
                .ToListAsync();

            return userHostedLunches.Concat(userAcceptedLunches).ToList();
        }

        [HttpGet("friends")]
        public async Task<IEnumerable<User>> Friends(Lunch lunch)
        {
            var friends = await Repository.Friends
                .Where(x => x.UserId == User.Id)
                .Include(x => x.Friend)
                .ToListAsync();
            return friends.Select(x => x.Friend);
        }

        [HttpGet("invitations")]
        public async Task<IEnumerable<Invitation>> Invitations()
        {
            var invites = await Repository.Invitations
                .Where(x => x.UserId == User.Id && x.Response == InviteResponseKind.None)
                .Include(x => x.User)
                .Include(x => x.Lunch)
                .Include(x => x.Lunch.Host)
                .Include(x => x.Lunch.Location)
                .Include(x => x.Lunch.Invitations)
                .ThenInclude(x => x.User)
                .ToListAsync();
            return invites;
        }
    }
}
