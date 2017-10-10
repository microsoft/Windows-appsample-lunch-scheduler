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
using LunchScheduler.Common;
using LunchScheduler.Models;
using LunchScheduler.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace LunchScheduler.Views
{
    public sealed partial class FriendsPage : Page
    {
        public MainViewModel ViewModel => App.ViewModel;

        public FriendsPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Only do this for new navigation, not back navigation.
            if (e.NavigationMode == NavigationMode.New)
            {
                var user = e.Parameter as User;
                
                if (user != null)
                {
                    FriendsList.SelectedItem = user;
                }
            }

            if (ViewModel.LunchBeingCreated != null)
            {
                FriendsList.SelectionMode = ListViewSelectionMode.Multiple;
            }
        }

        private void FriendsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var lbc = ViewModel.LunchBeingCreated;

            if (lbc == null)
            {
                return;
            }

            // Remove users
            List<Invitation> invitesToRemove = new List<Invitation>();

            foreach (User user in e.RemovedItems)
            {
                foreach (Invitation invitation in lbc.Invitations)
                {
                    if (invitation.User.Id == user.Id)
                    {
                        invitesToRemove.Add(invitation);
                    }
                }
            }

            foreach (Invitation invitation in invitesToRemove)
            {
                ViewModel.LunchBeingCreated.Invitations.Remove(invitation);
            }

            // Add users
            foreach (User user in e.AddedItems)
            {
                ViewModel.LunchBeingCreated.Invitations.Add(new Invitation(lbc, user));
            }
        }

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var user = (e.OriginalSource as FrameworkElement).DataContext as User;

            if (ViewModel.LunchBeingCreated != null)
            {
                if (!FriendsList.SelectedItems.Contains(user))
                {
                    FriendsList.SelectedItems.Add(user);
                    //ViewModel.LunchBeingCreated.Invitations.Add(new Invitation(ViewModel.LunchBeingCreated, user));
                }
            }
            else
            {
                NavigationService.Navigate(typeof(CreateLunchPage), user);
            }
        }

        private void StackPanel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (ViewModel.LunchBeingCreated is null)
            {
                FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
            }
        }
    }
}
