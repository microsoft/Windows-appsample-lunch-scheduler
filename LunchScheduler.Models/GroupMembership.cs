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
    /// Represents a group membership. Required since
    /// Entity Framework Core does not support automatically 
    /// creating join tables. 
    /// </summary>
    public class GroupMembership : ModelBase
    {
        /// <summary>
        /// Creates an empty group membership. 
        /// </summary>
        public GroupMembership()
        { }

        /// <summary>
        /// Creates a new group membership where the given user is a 
        /// member of the given group. 
        /// </summary>
        public GroupMembership(Group group, User member)
        {
            Group = group;
            Member = member; 
        }

        private Guid _groupId;
        /// <summary>
        /// Gets or sets the group Id. 
        /// </summary>
        public Guid GroupId
        {
            get => _groupId;
            set => SetProperty(ref _groupId, value); 
        }

        private Group _group; 
        /// <summary>
        /// Gets or sets the group. 
        /// </summary>
        public Group Group
        {
            get => _group; 
            set
            {
                if (value == null)
                {
                    throw new ArgumentException(nameof(Group) + " cannot be null"); 
                }
                SetProperty(ref _group, value);
                GroupId = value.Id; 
            }
        }

        private Guid _memberId;
        /// <summary>
        /// Gets or sets the Id of the user who is a member of the group. 
        /// </summary>
        public Guid MemberId
        {
            get => _groupId;
            set => SetProperty(ref _memberId, value);
        }

        private User _member;
        /// <summary>
        /// Gets or sets user who is a member of the group. 
        /// </summary>
        public User Member
        {
            get => _member;
            set
            {
                if (value == null)
                {
                    throw new ArgumentException(nameof(Member) + " cannot be null");
                }
                SetProperty(ref _member, value);
                MemberId = value.Id;
            }
        }
    }
}
