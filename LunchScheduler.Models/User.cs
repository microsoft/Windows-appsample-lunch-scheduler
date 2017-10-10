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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;

namespace LunchScheduler.Models
{
    /// <summary>
    /// Represents a user. 
    /// </summary>
    [DebuggerDisplay("Name: {Name}")]
    public class User : ModelBase
    {
        /// <summary>
        /// Creates a new User. 
        /// </summary>
        public User()
        {
            Friends.CollectionChanged += OnFriendsChanged;
        }

        private string _name;
        /// <summary>
        /// Gets or sets the user's name.
        /// </summary>
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private string _photoUrl;
        /// <summary>
        /// Gets or sets the full URL to the user's photo on the server. 
        /// Used directly for XAML binding. 
        /// </summary>
        public string PhotoUrl
        {
            get => _photoUrl;
            set => SetProperty(ref _photoUrl, value);
        }

        private DateTime _authorizationTokenExpiration;
        /// <summary>
        /// Gets or sets the date and time the user's current authorization token 
        /// expires and will require refresh.
        /// </summary>
        public DateTime AuthorizationTokenExpiration
        {
            get => _authorizationTokenExpiration;
            set => SetProperty(ref _authorizationTokenExpiration, value);
        }

        private string _authorizationToken;
        /// <summary>
        /// Gets or sets the user's authorization token. Used by the Lunch Scheduler 
        /// API to verify the user's identity and authorize calls.
        /// </summary>
        public string AuthorizationToken
        {
            get => _authorizationToken;
            set => SetProperty(ref _authorizationToken, value);
        }

        private AuthenticationProviderKind _authProviderKind;
        /// <summary>
        /// Gets or sets the provider (e.g., MSA, Facebook, etc) the user authenticated with. 
        /// </summary>
        public AuthenticationProviderKind AuthenticationProviderKind
        {
            get => _authProviderKind;
            set => SetProperty(ref _authProviderKind, value);
        }

        private string _authProviderId;
        /// <summary>
        /// Gets or sets the user's Id with their authentication provider. Used to link a user to 
        /// their provider even if their change their name, email address, or other info changes. 
        /// </summary>
        public string AuthenticationProviderId
        {
            get => _authProviderId;
            set => SetProperty(ref _authProviderId, value);
        }

        private List<Friendship> _friendships = new List<Friendship>();
        /// <summary>
        /// Gets or sets the user's list of friendships. This property is required by the database
        /// since Entity Framework Core does not support automatically creating join tables.
        /// </summary>
        public virtual List<Friendship> Friendships
        {
            get => _friendships;
            set => SetProperty(ref _friendships, value);
        }

        private ObservableCollection<User> _friends = new ObservableCollection<User>();
        /// <summary>
        /// Gets or sets the user's friends. 
        /// </summary>
        public virtual ObservableCollection<User> Friends
        {
            get => _friends;
            set
            {
                Friends.CollectionChanged -= OnFriendsChanged;
                SetProperty(ref _friends, value);
                Friends.CollectionChanged += OnFriendsChanged;
            }
        }

        private ObservableCollection<Lunch> _lunches = new ObservableCollection<Lunch>();
        /// <summary>
        /// Gets or sets lunches the user created. Does *not* include lunches the 
        /// user was invited to; see <see cref="Invitations"/>. 
        /// </summary>
        public virtual ObservableCollection<Lunch> Lunches
        {
            get => _lunches;
            set
            {
                Lunches.CollectionChanged -= OnLunchesChanged;
                SetProperty(ref _lunches, value);
                OnLunchesChanged(this, new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add, value));
                Lunches.CollectionChanged += OnLunchesChanged;
            }
        }

        private ObservableCollection<Invitation> _invitations = new ObservableCollection<Invitation>();
        /// <summary>
        /// Gets or sets lunches the user was invited to. Does *not* include lunches
        /// the user created; see <see cref="Lunches"/>. 
        /// </summary>
        public virtual ObservableCollection<Invitation> Invitations
        {
            get => _invitations;
            set => SetProperty(ref _invitations, value);
        }

        private ObservableCollection<Group> _groups = new ObservableCollection<Group>();
        /// <summary>
        /// Gets or sets the user's groups. Includes both group's the user has created 
        /// and is a member of. 
        /// </summary>
        public virtual ObservableCollection<Group> Groups
        {
            get => _groups;
            set => SetProperty(ref _groups, value);
        }

        /// <summary>
        /// Called when a friend is added or removed from the user's <see cref="Friends"/> 
        /// to automatically mirror the the change in the user's <see cref="Friendships"/>. 
        /// </summary>
        private void OnFriendsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems?.Count > 0)
            {
                foreach (var newFriend in e.NewItems.Cast<User>())
                {
                    if (!Friendships.Any(x => x.FriendId == newFriend.Id))
                    {
                        Friendships.Add(new Friendship(this, newFriend));
                    }
                }
            }
            if (e.OldItems?.Count > 0)
            {
                foreach (var removedFriend in e.OldItems.Cast<User>())
                {
                    int i = Friendships.FindIndex(x => x.FriendId == removedFriend.Id);
                    Friendships.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Called when a lunch is added or removed. Each lunch contains a pointer to host, 
        /// which in turn contains a collection of lunches. This creates a self-referencing loop
        /// that will fail JSON serialization but is needed in the app code. As a workaround, the 
        /// host is left null on serialization, and then reset to the user by this method client-side
        /// on deserialization. 
        /// </summary>
        private void OnLunchesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e?.NewItems != null)
            {
                foreach (var lunch in e.NewItems.Cast<Lunch>()
                    .Where(x => x.HostId == Id && x.Host == null))
                {
                    lunch.Host = this;
                }
            }
        }
    }
}
