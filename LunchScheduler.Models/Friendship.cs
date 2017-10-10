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
using Newtonsoft.Json;

namespace LunchScheduler.Models
{
    /// <summary>
    /// Represents a friendship between two users. Required since  
    /// Entity Framework Core does not support automatically 
    /// creating join tables. 
    /// </summary>
    public class Friendship : ModelBase
    {
        /// <summary>
        /// Creates a new empty friendship. 
        /// </summary>
        public Friendship()
        { }

        /// <summary>
        /// Creates a new friendship where the given user 
        /// has the given friend. 
        /// </summary>
        public Friendship(User user, User friend)
        {
            User = user;
            Friend = friend; 
        }

        private Guid _userId;
        [JsonIgnore]
        public Guid UserId
        {
            get => _userId;
            set => SetProperty(ref _userId, value); 
        }

        private User _user;
        [JsonIgnore]
        public User User
        {
            get => _user; 
            set
            {
                if (value == null)
                {
                    throw new ArgumentException(nameof(User) + " cannot be null"); 
                }
                SetProperty(ref _user, value);
                UserId = value.Id; 
            }
        }

        private Guid _friendId; 
        public Guid FriendId
        {
            get => _friendId;
            set => SetProperty(ref _friendId, value); 
        }

        private User _friend;
        public User Friend
        {
            get => _friend; 
            set
            {
                if (value == null)
                {
                    throw new ArgumentException(nameof(Friend) + " cannot be null"); 
                }
                SetProperty(ref _friend, value);
                FriendId = value.Id; 
            }
        }

        public string Name => Friend.Name;
        public string PhotoUrl => Friend.PhotoUrl;

    }
}
