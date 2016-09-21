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
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;

namespace LunchScheduler.App.ViewModels
{
    /// <summary>
    /// View model for the Settings view. 
    /// </summary>
    [ImplementPropertyChanged]
    public class SettingsViewModel
    {
        /// <summary>
        /// Creates a new SettingsViewModel. 
        /// </summary>
        public SettingsViewModel()
        {
            Task.Run(Initialize);
        }

        /// <summary>
        /// Gets the current user. 
        /// </summary>
        public User CurrentUser => App.User;

        /// <summary>
        /// Gets or sets the privacy text. 
        /// </summary>
        public string PrivacyText { get; private set; }

        /// <summary>
        /// Gets or sets the text of the clear collections text box.
        /// </summary>
        public string ClearCollectionText { get; set; }


        /// <summary>
        /// Loads the privacy text from local file. 
        /// </summary>
        private async Task Initialize()
        {
            var file = await StorageFile.GetFileFromApplicationUriAsync(
                new Uri("ms-appx:///Assets/Privacy.txt"));
            PrivacyText = await FileIO.ReadTextAsync(file);
        }

        /// <summary>
        /// Logs the current user out of the app and returns to the OOBE screen. 
        /// </summary>
        public void Logout()
        { 
            AuthenticationHelper.Logout();
            Window.Current.Content = new Views.Welcome(); 
        }

        /// <summary>
        /// Shows the phone notification registration dialog.
        /// </summary>
        public async void RegisterPhone() => await DispatchHelper.RunAsync(
            async () => await new PhoneSignUpDialog().ShowAsync()); 
    }
}