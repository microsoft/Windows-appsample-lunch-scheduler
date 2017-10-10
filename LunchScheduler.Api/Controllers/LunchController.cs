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

using System.Collections.ObjectModel;
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
    public class LunchController : ControllerBase
    {
        public LunchController(LunchContext repository) : base(repository)
        { }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Lunch lunch)
        {
            // Don't allow users to create lunches they aren't hosting. 
            // Note: In a production app, more complete validation, such as 
            // ensuring all invitations, the location, and time are valid,  
            // is suggested.

            if (lunch.HostId != User.Id)
            {
                return Forbid();
            }

            // Updates aren't supported right now - if you want to change 
            // a lunch, you need to cancel it. 

            if (await Repository.Lunches.AnyAsync(x => x.Id == lunch.Id))
            {
                return BadRequest(); 
            }

            var entry = new Lunch
            {
                Id = lunch.Id,
                Date = lunch.Date,
                HostId = lunch.HostId,
                LocationId = lunch.LocationId,
                Location = lunch.Location,
                Notes = lunch.Notes,
                State = lunch.State,
                Invitations = new ObservableCollection<Invitation>(
                    lunch.Invitations.Select(x => new Invitation
                {
                    Id = x.Id,
                    LunchId = x.LunchId,
                    UserId = x.UserId,
                    Response = InviteResponseKind.None
                }))
            };

            await Repository.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(Lunch lunch)
        {
            if (lunch.HostId != User.Id)
            {
                Forbid();
            }

            var entity = await Repository.Lunches.Include(x => x.Invitations)
                .FirstOrDefaultAsync(x => x.Id == lunch.Id);
            if (entity != null)
            {
                Repository.Lunches.Remove(entity);
                await Repository.SaveChangesAsync();
                return Ok();
            }
            else
            {
                return NotFound();
            }
        }
    }
}
