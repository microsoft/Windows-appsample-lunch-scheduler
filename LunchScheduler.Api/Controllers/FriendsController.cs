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
    [Authorize]
    public class FriendsController : ControllerBase
    {
        public FriendsController(LunchContext repository) : base(repository)
        { }

        [HttpGet("api/friends/suggest")]
        public IEnumerable<User> Get()
        {
            // Suggested friends returns users who aren't currently a friend, 
            // but had lunch with the user recently.

            var currentFriendIds = Repository.Friends
                .Where(x => x.UserId == User.Id)
                .Select(x => x.FriendId)
                .ToList();

            var suggestedFriends = Repository.Invitations
                .Where(x => x.UserId == User.Id && !currentFriendIds.Contains(x.Id))
                .SelectMany(x => x.Lunch.Invitations.Select(y => y.User))
                .ToList();

            return suggestedFriends;
        }

        [HttpPost("api/friends")]
        public async Task<IActionResult> Add([FromBody]Friendship friend)
        {
            // Users can only manage their own friendship. 
            if (friend.UserId != User.Id)
            {
                return Unauthorized();
            }
            // No duplicate friends or adding yourself as a friend.
            if (await Repository.Friends.AnyAsync(x => x == friend) || friend.FriendId == User.Id)
            {
                return BadRequest();
            }

            Repository.Friends.Add(friend);
            await Repository.SaveChangesAsync(); 

            return Ok();
        }

        [HttpDelete("api/friends")]
        public async Task<IActionResult> Delete(Friendship friend)
        {
            // Users can only manage their own friendship. 
            if (friend.UserId != User.Id)
            {
                return Unauthorized();
            }

            var entry = await Repository.Friends.FindAsync(friend.Id);
            if (entry == null)
            {
                return NotFound();
            }

            Repository.Friends.Remove(entry);
            await Repository.SaveChangesAsync();
            return Ok();
        }
    }
}
