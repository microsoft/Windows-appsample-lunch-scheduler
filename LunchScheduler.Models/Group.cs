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
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace LunchScheduler.Models
{
    /// <summary>
    /// Represents a group of users. 
    /// </summary>
    [DebuggerDisplay("Name: {Name}")]
    public class Group : ModelBase
    {
        private string _name;
        /// <summary>
        /// Gets or sets the group's name.
        /// </summary>
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public Guid _ownerId;
        /// <summary>
        /// Gets or sets the Id of the group's owner. 
        /// </summary>
        public Guid OwnerId
        {
            get => _ownerId;
            set => SetProperty(ref _ownerId, value);
        }

        private User _owner;
        /// <summary>
        /// Gets or sets the group's owner. 
        /// </summary>
        public User Owner
        {
            get => _owner;
            set
            {
                if (_owner == null)
                {
                    throw new ArgumentException(nameof(Owner) + " cannot be null");
                }
                SetProperty(ref _owner, value);
                OwnerId = value.Id;
            }
        }

        private ObservableCollection<GroupMembership> _members; 
        /// <summary>
        /// Gets or sets the group's members. Should not contain the group's <see cref="Owner"/>. 
        /// </summary>
        public ObservableCollection<GroupMembership> Members
        {
            get => _members;
            set => SetProperty(ref _members, value); 
        }
    }
}
