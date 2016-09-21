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
//  ---------------------------------------------------------------------------------

using LunchScheduler.App.Controls;
using LunchScheduler.App.Helpers;
using LunchScheduler.Models;
using PropertyChanged;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Windows.UI.Xaml;

namespace LunchScheduler.App.ViewModels
{
    /// <summary>
    /// View model for the Friends view.
    /// </summary>
    [ImplementPropertyChanged]
    public class FriendsViewModel
    {
        /// <summary>
        /// Gets the user's frirends.
        /// </summary>
        public ObservableCollection<User> Friends => App.User?.Friends;

        /// <summary>
        /// Gets or sets the currently selected friend. 
        /// </summary>
        public User SelectedFriend { get; set; }

        /// <summary>
        /// Gets or sets whether the "No friends" string is visible.
        /// </summary>
        public Visibility NoFriendsStringVisibility =>
            Friends.Count > 0 ? Visibility.Collapsed : Visibility.Visible;

        /// <summary>
        /// Creates a new FriendsViewModel.
        /// </summary>
        public FriendsViewModel()
        {
            Friends.CollectionChanged += (s, e) => PropertyChanged?.Invoke(this,
                new PropertyChangedEventArgs(nameof(NoFriendsStringVisibility))); 
        }

        /// <summary>
        /// Shows the add friend dialog to add a friend. 
        /// </summary>
        public async void AddFriend()
        {
            await DispatchHelper.RunAsync(async () =>
            {
                var dialog = new AddFriendDialog();
                dialog.FriendAdded += async (s, e) =>
                {
                    var friend = await App.Api.GetAsync<User>("Users", new { Email = e });
                    if (null != friend)
                    {
                        Friends.Add(friend);
                    }
                }; 
                await dialog.ShowAsync();
            });
        }

        /// <summary>
        /// Removes the selected friend.
        /// </summary>
        public void DeleteFriend() => Friends.Remove(SelectedFriend);

        /// <summary>
        /// Event handler for registering for change notification. Actual implementation
        /// is injected at compile-time by Fody. 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
    }
}