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
    public class GroupsController : ControllerBase
    {
        public GroupsController(LunchContext repository) : base(repository)
        { }

        [HttpPost("")]
        public async Task<IActionResult> Create([FromBody]Group group)
        {
            // Can't create groups for other users. 
            if (group.OwnerId != User.Id)
            {
                return Unauthorized(); 
            }

            // Don't create the exact same group twice. 
            if (Repository.Groups.Any(x => x.Id == group.Id))
            {
                return BadRequest(); 
            }

            await Repository.Groups.AddAsync(new Group
            {
                Id = group.Id,
                Name = group.Name,
                OwnerId = group.OwnerId
            });
            await Repository.SaveChangesAsync();
            return Ok(); 
        }

        [HttpDelete("")]
        public async Task Delete(Group group)
        {
            var entity = await Repository.Groups.FindAsync(group.Id);
            if (entity != null)
            {
                Repository.Groups.Remove(entity);
                await Repository.SaveChangesAsync();
            }
        }

        [HttpPost("membership")]
        public async Task<IActionResult> Join([FromBody]GroupMembership membership)
        {
            var group = await Repository.Groups
                .Include(x => x.Members)
                .FirstOrDefaultAsync(x => x.Id == membership.GroupId);

            // Can't join non-existant groups.
            if (group == null)
            {
                return NotFound();
            }
            // Only group owners or the member themselves can manage membership.
            if (group.OwnerId != User.Id && membership.MemberId != User.Id)
            {
                return Unauthorized(); 
            }
            // Can't join the same group twice.
            if (group.Members.Any(x => x == membership))
            {
                return BadRequest(); 
            }

            await Repository.GroupMemberships.AddAsync(new GroupMembership
            {
                Id = membership.Id,
                GroupId = membership.GroupId,
                MemberId = membership.MemberId
            });
            await Repository.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("membership")]
        public async Task Leave(Group group)
        {
            var entity = Repository.GroupMemberships.Find(group.Id);
            if (entity != null)
            {
                Repository.GroupMemberships.Remove(entity);
                await Repository.SaveChangesAsync();
            }
        }
    }
}
