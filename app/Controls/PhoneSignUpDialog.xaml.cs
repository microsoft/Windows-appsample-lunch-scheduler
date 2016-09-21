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

using LunchScheduler.Models;
using PropertyChanged;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace LunchScheduler.App.Controls
{
    /// <summary>
    /// Dialog box for signing up a phone for text notifications.
    /// </summary>
    [ImplementPropertyChanged]
    public sealed partial class PhoneSignUpDialog : ContentDialog
    {
        /// <summary>
        /// Indicates whether the phone sign up process was cancelled by the user.
        /// </summary>
        private bool _cancelled = false; 
        
        /// <summary>
        /// Creates a new PhoneSignUpDialog.
        /// </summary>
        public PhoneSignUpDialog()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Registers the user's phone number, sends a verification text, and waits for the user to respond.
        /// </summary>
        private async void RegisterClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            App.User.Devices.Clear(); 
            if (EntryStackPanel.Visibility == Visibility.Visible)
            {
                // Progress 
                args.Cancel = true;
                EntryStackPanel.Visibility = Visibility.Collapsed;
                ProgressStackPanel.Visibility = Visibility.Visible;
                PrimaryButtonText = "";
                PrimaryButtonClick -= RegisterClick; 

                await VerifyPhoneNumberAsync();

                // Results
                ProgressStackPanel.Visibility = Visibility.Collapsed;
                ResultStackPanel.Visibility = Visibility.Visible; 
            }
        }

        /// <summary>
        /// Waits up to 3 minutes for the user to verify their number.
        /// </summary>
        private async Task VerifyPhoneNumberAsync()
        {
            var device = new Device
            {
                NotificationsEnabled = true,
                PhoneNumber = PhoneNumberTextBox.Text,
                NotificationType = NotificationType.Text
            };
            App.User.Devices.Add(device);

            // Check verification every 3 seconds for up to 3 minutes. 
            // If they don't respond within the time frame, verify fails. 
            bool verified = false;
            int attempts = 0;
            while (!verified && !_cancelled && attempts < 30)
            {
                await Task.Delay(3000);
                verified = await App.Api.GetAsync<bool>("Device", new { Number = device.PhoneNumber });
                attempts++;
            }

            ResultsTextBlock.Text = verified ? "Success! We've verified your number." :
                "We couldn't verify your number. If you want to try again, you can do so from the settings page.";
            PrimaryButtonText = "Continue";
            SecondaryButtonText = ""; 
        }

        /// <summary>
        /// Processes user cancel and tells the dialog to stop waiting.
        /// </summary>
        private void CancelClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            _cancelled = true; 
        }
    }
}
