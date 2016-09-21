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

using LunchScheduler.App.Helpers;
using LunchScheduler.Models;
using Microsoft.WindowsAzure.Messaging;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Networking.PushNotifications;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace LunchScheduler.App
{
    /// <summary>
    /// Main application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Name of the Azure notification hub. 
        /// </summary>
        private const string HubName = "<TODO: Your hub name here>";

        /// <summary>
        /// Endpoint for the Azure notification hub. Configure at https://www.portal.azure.com
        /// </summary>
        private const string HubEndpoint = "<TODO: Your endpoint here>";

        /// <summary>
        /// The current user of the app. Null if no user is not yet logged in.
        /// </summary>
        public static User User { get; set; }

        /// <summary>
        /// Global helper for making calls to the REST API service.
        /// </summary>
        public static ApiHelper Api { get; set; } = new ApiHelper();
      
        /// <summary>
        /// Width of window for narrow layout (phone, etc.).
        /// </summary>
        public const int NarrowWidth = 600;
        public const double FontSizeSmall = 15;
        public const double FontSizeMedium = 20;

        /// <summary>
        /// Creates a new App.
        /// </summary>
        public App()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Fires when the app is activated. 
        /// </summary>
        protected override async void OnActivated(IActivatedEventArgs args) => await InitalizeAsync(); 

        /// <summary>
        /// Fires when the app is launched.
        /// </summary>
        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            await InitalizeAsync();
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(500, 800)); 
            ApplicationView.PreferredLaunchViewSize = new Size(3000, 3000);
        }

        /// <summary>
        /// Prepares the app for use.
        /// </summary>
        private async Task InitalizeAsync()
        {
            Application.Current.UnhandledException += (s, e) => AsyncErrorHandler.HandleException(e.Exception);  

            AuthenticationHelper.UserLoggedIn += async (s, e) =>
            {
                User = e;
                SynchronizationHelper.RecursiveRegisterPropertyChanged(e);
                var channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
                var hub = new NotificationHub(HubName, HubEndpoint);
                await hub.RegisterNativeAsync(channel.Uri, new[] { User.Email });
            };

            await AuthenticationHelper.TryLogInSilently();

            if (App.User != null)
            {
                Window.Current.Content = new Views.Shell();
            }
            else
            {
                Window.Current.Content = new Views.Welcome();
            }
            Window.Current.Activate();
        }
    }
}