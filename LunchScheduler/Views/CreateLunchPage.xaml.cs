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
using LunchScheduler.Common;
using LunchScheduler.CustomControls;
using LunchScheduler.Models;
using LunchScheduler.ViewModels;
using Microsoft.Toolkit.Uwp.UI.Animations;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace LunchScheduler.Views
{
    public sealed partial class CreateLunchPage : Page
    {
        public MainViewModel ViewModel => App.ViewModel;

        private int progressIndicatorOffset = 252;

        public CreateLunchPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel.CreateLunch();
            ViewModel.LunchBeingCreated.Host = ViewModel.User;

            User Invitee = null;

            if (e.Parameter is User)
            {
                Invitee = e.Parameter as User;
            }
            else if (e.Parameter is Restaurant)
            {
                ViewModel.LunchBeingCreated.Location = e.Parameter as Restaurant;
            }

            SystemNavigationManager.GetForCurrentView().BackRequested += CreateLunchPage_BackRequested;
            base.OnNavigatedTo(e);

            CreateLunchFrame.Navigate(typeof(FriendsPage), Invitee);
        }

        private void CreateLunchPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (CreateLunchFrame.CanGoBack && e.Handled == false)
            {
                e.Handled = true;
                CreateLunchFrame.GoBack();
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            SystemNavigationManager.GetForCurrentView().BackRequested -= CreateLunchPage_BackRequested;

            ViewModel.LunchBeingCreated = null;

            base.OnNavigatingFrom(e);
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (CreateLunchFrame.SourcePageType.Equals(typeof(FriendsPage)))
            {
                CreateLunchFrame.Navigate(typeof(DateTimePage));                
            }
            else if (CreateLunchFrame.SourcePageType.Equals(typeof(DateTimePage)))
            {
                CreateLunchFrame.Navigate(typeof(PlacesPage));
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        private async void SetProgressIndicators(int index)
        {
            if (index == 0)
            {
                ColonIndicator.Visibility = Visibility.Visible;
                FriendsIndicator.Visibility = Visibility.Visible;
                DateIndicator.Visibility = Visibility.Collapsed;
                TimeIndicator.Visibility = Visibility.Collapsed;
                HostIndicator.BadgeNumber = 0;

                ColonIndicator.Fade(100, 50, 0).Start();
                FriendsIndicator.Fade(100, 150, 0).Start();
                await FriendsIndicator.Scale(1, 1, 0, 24, 300).Offset(0, 0, 300).StartAsync();
            }
            else
            {
                ColonIndicator.Fade(0, 50, 0).Start();
                FriendsIndicator.Fade(0, 150, 0).Start();
                await FriendsIndicator.Scale(0, 0, 0, 24, 300).Offset(-80, 0, 300).StartAsync();

                // Set Visibility to Collapsed so the elements don't take up
                // space in the layout.
                ColonIndicator.Visibility = Visibility.Collapsed;
                FriendsIndicator.Visibility = Visibility.Collapsed;
                DateIndicator.Visibility = Visibility.Visible;
                TimeIndicator.Visibility = Visibility.Visible;
                HostIndicator.BadgeNumber = ViewModel.LunchBeingCreated.Invitations.Count;
            }
        }

        private async void MoveProgressIndicator(int index)
        {
            progressIndicatorOffset = (int)LeftProgressLine.X2 + 72;

            await SelectionIndicator.Offset(index * progressIndicatorOffset, 0, 300).StartAsync();
        }

        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            ConfirmationBackground.Visibility = Visibility.Visible;

            LunchDetailsDialog ConfirmationDialog = new LunchDetailsDialog
            {
                DataContext = ViewModel.LunchBeingCreated
            };

            ContentDialogResult result = await ConfirmationDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                ViewModel.CommitLunch();
                
                if (NavigationService.CanGoBack)
                {
                    NavigationService.GoBack();
                }
            }

            ConfirmationBackground.Visibility = Visibility.Collapsed;
        }

        private void CreateLunchFrame_Navigated(object sender, NavigationEventArgs e)
        {
            if (CreateLunchFrame.SourcePageType.Equals(typeof(FriendsPage)))
            {
                SetProgressIndicators(0);
                MoveProgressIndicator(0);
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;

                SubmitButton.Visibility = Visibility.Collapsed;
                NextButton.Visibility = Visibility.Visible;
            }
            else if (CreateLunchFrame.SourcePageType.Equals(typeof(DateTimePage)))
            {
                SetProgressIndicators(1);
                MoveProgressIndicator(1);
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;

                SubmitButton.Visibility = Visibility.Collapsed;
                NextButton.Visibility = Visibility.Visible;
            }
            else if (CreateLunchFrame.SourcePageType.Equals(typeof(PlacesPage)))
            {
                SetProgressIndicators(2);
                MoveProgressIndicator(2);
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;

                SubmitButton.Visibility = Visibility.Visible;
                NextButton.Visibility = Visibility.Collapsed;
            }
        }

        private void CreateLunchFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {

        }
    }
}
