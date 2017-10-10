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
using System.Threading.Tasks;

namespace LunchScheduler.Models
{
    /// <summary>
    /// Defines methods for interacting with the Lunch Scheduler backend. 
    /// </summary>
    public interface ILunchRepository
    {
        /// <summary>
        /// Gets or sets the current user logged in to the app. This is used for authorization
        /// by all API calls except for <see cref="LoginAsyc(AuthenticationProviderKind, string)"/> . 
        /// </summary>
        User User { get; set; }

        /// <summary>
        /// Logs the user into the app using the given authentication provider and access token.
        /// </summary>
        Task<User> LoginAsyc(AuthenticationProviderKind provider, string token);

        /// <summary>
        /// Logs the user out of the app and clears their access tokens. 
        /// </summary>
        Task LogoutAsync();

        /// <summary>
        /// Returns the logged in user's friends. 
        /// </summary>
        Task<IEnumerable<User>> GetFriendsAsync();

        /// <summary>
        /// Returns lunches created by the logged in user.
        /// </summary>
        Task<IEnumerable<Lunch>> GetLunchesAsync();
        
        /// <summary>
        /// Returns lunches the logged in user is invited to. 
        /// </summary>
        Task<IEnumerable<Invitation>> GetInvitationsAsync(); 

        /// <summary>
        /// Returns restaurants near the given address or search area. 
        /// </summary>
        Task<IEnumerable<Restaurant>> GetRestaurantsAsync(string address);

        /// <summary>
        /// Returns restaurants near the given coordinates. 
        /// </summary>
        Task<IEnumerable<Restaurant>> GetRestaurantsAsync(double lat, double lng);

        /// <summary>
        /// Creates a new lunch.
        /// </summary>
        Task CreateLunchAsync(Lunch lunch);

        /// <summary>
        /// Cancels an existing lunch. 
        /// </summary>
        Task CancelLunchAsync(Lunch lunch);

        /// <summary>
        /// Updates the user's response to a lunch invitation. 
        /// </summary>
        Task RespondToInvitationAsync(Invitation invite);

        /// <summary>
        /// Adds a user as a friend. 
        /// </summary>
        Task AddFriendAsync(Friendship friend);

        /// <summary>
        /// Removes a user as a friend. 
        /// </summary>
        Task RemoveFriendAsync(Friendship friend);

        /// <summary>
        /// Returns suggested friends for the user to add. 
        /// </summary>
        Task<IEnumerable<User>> GetSuggestedFriendsAsync(); 

        /// <summary>
        /// Creates a new group.
        /// </summary>
        Task CreateGroupAsync(Group group);

        /// <summary>
        /// Deletes a group. 
        /// </summary>
        Task DeleteGroupAsync(Group group);

        /// <summary>
        /// Adds the user to the given group. 
        /// </summary>
        Task JoinGroupAsync(GroupMembership membership);

        /// <summary>
        /// Removes the user from the given group. 
        /// </summary>
        Task LeaveGroupAsync(GroupMembership membership);
    }
}
