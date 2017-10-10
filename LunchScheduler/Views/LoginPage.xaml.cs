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
using System.Threading.Tasks;
using LunchScheduler.Common;
using LunchScheduler.Models;
using Microsoft.Toolkit.Uwp.Helpers;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using LunchScheduler.ViewModels;

namespace LunchScheduler.Views
{
    public sealed partial class LoginPage : Page
    {
        public LoginPage()
        {
            this.InitializeComponent();
            DataContext = ViewModel;
        }

        public AuthenticationViewModel ViewModel => App.AuthenticationViewModel; 

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ViewModel.LoginCompleted += OnLoginCompleted;
            await App.ViewModel.UpdateLocationAsync();
            await Task.Run(ViewModel.TryLogInSilentlyAsync);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            ViewModel.LoginCompleted -= OnLoginCompleted; 
            base.OnNavigatingFrom(e);
        }

        /// <summary>
        /// Navigates to the app's main page when the view model finishes logging in. 
        /// </summary>
        private async void OnLoginCompleted(object sender, EventArgs e)
        {
            // Even though this method is in code-behind, explicitly run this code from the UI thread, 
            // since it could be triggered by an event fired by the view model on a background thread.

            await DispatcherHelper.ExecuteOnUIThreadAsync(async () =>
            {
                var img = new BitmapImage();
                img.UriSource = new Uri(App.ViewModel.User.PhotoUrl);
                LoginPicture.ProfilePicture = img; 
                LoginText.Text = $"Welcome {App.ViewModel.User.Name}!";

                App.ViewModel.User.Friends = new ObservableCollection<User>(await App.Api.GetFriendsAsync());
                App.ViewModel.User.Lunches = new ObservableCollection<Lunch>(await App.Api.GetLunchesAsync());
                App.ViewModel.User.Invitations = new ObservableCollection<Invitation>(await App.Api.GetInvitationsAsync()); 
                try
                {
                    ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("image", LoginPicture);
                }
                catch (Exception)
                {
                    // TODO: When the user loads very quickly, this method will be called before 
                    // the image is loaded and throw an exception. Need to add an IsLoaded check to 
                    // determine whether to animate. 
                }
                NavigationService.Frame = this.Frame;
                NavigationService.Navigate(typeof(MainPage));
            });
        }
    }
}
