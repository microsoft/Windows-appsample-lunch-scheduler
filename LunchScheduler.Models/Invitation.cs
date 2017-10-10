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

namespace LunchScheduler.Models
{
    /// <summary>
    /// Represents an invitation to a lunch. The host does not receive an 
    /// invitation and is directly attached to the <see cref="Lunch"/>. 
    /// </summary>
    public class Invitation : ModelBase
    {
        /// <summary>
        /// Creates a new invitation. 
        /// </summary>
        public Invitation()
        { }

        /// <summary>
        /// Creates a new invitation. 
        /// </summary>
        public Invitation(Lunch lunch, User user)
        {
            Lunch = lunch;
            User = user; 
        }

        private Guid _lunchId; 
        /// <summary>
        /// Gets or sets the Id of the lunch.
        /// </summary>
        public Guid LunchId
        {
            get => _lunchId;
            set => SetProperty(ref _lunchId, value); 
        }

        private Lunch _lunch;
        /// <summary>
        /// Gets or sets the Lunch the user is invited to.
        /// </summary>
        public Lunch Lunch
        {
            get => _lunch;
            set
            {
                if (value == null)
                {
                    throw new ArgumentException(nameof(Lunch) + " cannot be null"); 
                }
                SetProperty(ref _lunch, value); 
                LunchId = value.Id; 
            }
        }

        private InviteResponseKind _response;
        /// <summary>
        /// Gets or sets the user's response (e.g., whether they are attending). 
        /// </summary>
        public InviteResponseKind Response
        {
            get => _response;
            set => SetProperty(ref _response, value); 
        }

        private Guid _userId;
        /// <summary>
        /// Gets or sets the Id of the user invited to lunch.
        /// </summary>
        public Guid UserId
        {
            get => _userId;
            set => SetProperty(ref _userId, value); 
        }

        private User _user;
        /// <summary>
        /// Gets or sets the user invited to lunch.
        /// </summary>
        public User User
        {
            get => _user;
            set
            {
                SetProperty(ref _user, value);
            }
        }
    }
}
