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
using LunchScheduler.Common;
using LunchScheduler.Models;
using LunchScheduler.ViewModels;
using Microsoft.Toolkit.Uwp.UI.Animations;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace LunchScheduler.Views
{
    public sealed partial class LunchesPage : Page
    {
        public MainViewModel ViewModel => App.ViewModel;

        List<Invitation> DeclinedInvitations = new List<Invitation>();

        bool ConfirmDeclinedInvitations = true;

        public LunchesPage()
        {
            this.InitializeComponent();
            
            MyInvitationsCVS.Source = ViewModel.User.Lunches;
        }
  
        private async void AwaitingResponse_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
            {
                var hideAnimation = ResponseGrid.Blur(8).Fade(0.0f).Scale(scaleY: 0);
                hideAnimation.SetDurationForAll(200);
                await hideAnimation.StartAsync();
                ResponseGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                ResponseGrid.Visibility = Visibility.Visible;
                var showAnimation = ResponseGrid.Blur(0).Fade(1.0f).Scale(scaleY: 1);
                showAnimation.SetDurationForAll(200);
                showAnimation.Start();
            }

            if (((GridView)sender).Items.Count <= 0)
            {
                NoInvitesText.Visibility = Visibility.Visible;
            }
            else
            {
                NoInvitesText.Visibility = Visibility.Collapsed;
            }
        }

        private void CreateLunch_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(typeof(CreateLunchPage));
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (ConfirmDeclinedInvitations)
            {
                DeclinedInvitations.Clear();

                foreach (Invitation invitation in ViewModel.User.Invitations)
                {
                    if (invitation.Response == InviteResponseKind.Declined)
                    {
                        DeclinedInvitations.Add(invitation);
                    }
                }

                if (DeclinedInvitations.Count > 0)
                {
                    e.Cancel = true;
                    ShowConfirmationDialog(e);
                }
            }
            else
            {
                ConfirmDeclinedInvitations = true;
            }

            base.OnNavigatingFrom(e);
        }

        private async void ShowConfirmationDialog(NavigatingCancelEventArgs e)
        {
            ContentDialog ConfirmDeclineDialog = new ContentDialog
            {
                Title = "Remove declined invitations?",
                Content = "If you continue, your declined invitations will be removed.",
                CloseButtonText = "Stay here",
                PrimaryButtonText = "Yes, remove them"
            };

            ContentDialogResult result = await ConfirmDeclineDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                foreach (Invitation invitation in DeclinedInvitations)
                {
                    ViewModel.DeclineLunch(invitation);
                }

                ConfirmDeclinedInvitations = false;
                NavigationService.Navigate(e.SourcePageType, e.Parameter);
            }
            else
            {
                ConfirmDeclinedInvitations = true;
            }
        }
    }
}
