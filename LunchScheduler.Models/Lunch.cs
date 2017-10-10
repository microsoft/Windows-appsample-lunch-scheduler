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
    /// Represents a lunch.
    /// </summary>
    [DebuggerDisplay("Host = {Host.Name}")]
    public class Lunch : ModelBase
    {
        private string _notes;
        /// <summary>
        /// Gets or sets notes for the lunch. 
        /// </summary>
        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value); 
        }

        private DateTime _date;
        /// <summary>
        /// Gets or sets the lunch's start date and time.
        /// </summary>
        public DateTime Date
        {
            get => _date;
            set => SetProperty(ref _date, value);
        }

        private Guid _hostId; 
        /// <summary>
        /// Gets or sets the Id for the user who created the lunch.
        /// </summary>
        public Guid HostId
        {
            get => _hostId;
            set => SetProperty(ref _hostId, value); 
        }

        private User _host;
        /// <summary>
        /// Gets or sets the user who created the lunch. 
        /// </summary>
        public User Host
        {
            get => _host;
            set
            {
                if (value == null)
                {
                    throw new ArgumentException(nameof(Host) + " cannot be null"); 
                }
                SetProperty(ref _host, value);
                HostId = value.Id; 
            }
        }

        private ObservableCollection<Invitation> _invitations = new ObservableCollection<Invitation>(); 
        /// <summary>
        /// Gets or sets the collection of invitations, which contain the users invited to 
        /// the lunch and their response.
        /// </summary>
        public ObservableCollection<Invitation> Invitations
        {
            get => _invitations;
            set => SetProperty(ref _invitations, value); 
        }

        private Guid _locationId;
        /// <summary>
        /// Gets or sets the Id of the lunch's location. 
        /// </summary>
        public Guid LocationId
        {
            get => _locationId;
            set => SetProperty(ref _locationId, value);
        }

        private Restaurant _location; 
        /// <summary>
        /// Gets or sets the lunch's location. 
        /// </summary>
        public Restaurant Location
        {
            get => _location;
            set => SetProperty(ref _location, value); 
        }

        private LunchState _state; 
        public LunchState State
        {
            get => _state;
            set => SetProperty(ref _state, value); 
        }
    }
}