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
using PropertyChanged;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml;

namespace LunchScheduler.App.ViewModels
{
    /// <summary>
    /// View model for the Welcome view.
    /// </summary>
    [ImplementPropertyChanged]
    public class WelcomeViewModel
    {
        /// <summary>
        /// The base Uri for all OOBE images. 
        /// </summary>
        private const string ImageBase = @"ms-appx:///Assets/Welcome/";

        /// <summary>
        /// Creates a new WelcomeViewModel with the default OOBE items. 
        /// </summary>
        public WelcomeViewModel()
        {
            WelcomeItems = new ObservableCollection<WelcomeItem>(new[]
            {
                new WelcomeItem(
                    "Welcome to Lunch Scheduler",
                    "The easiest way to organize lunch with your friends, family, or coworkers.",
                    ImageBase + "Lunchbox.png"),

                new WelcomeItem(
                    "Organizing made easy",
                    "Hungry? Avoid endless back-and-forth emails or texts. Simply tap 'New Lunch' " +
                    "on the main menu and pick a who, where, and when. We'll take care of the rest.",
                    ImageBase + "Plate.png"),

                new WelcomeItem(
                    "Discover local delights",
                    "When you're scheduling a new lunch, Lunch Scheduler can use your current location to " +
                    "suggest tasty restaurants nearby.",
                    ImageBase + "Location.png",
                    async () => await DispatchHelper.RunAsync(async () => await Geolocator.RequestAccessAsync())),

                new WelcomeItem(
                    "Get signed up",
                    "To keep track of everything, you'll need an account with Lunch Scheduler. You can log in " +
                    "with an existing Microsoft, Google, or Facebook account. Lunch Scheduler will never post " +
                    "to your wall or send you unsolicted email. And there's no extra passwords to remember.",
                    ImageBase + "Keys.png",
                    AccountsSettingsPane.Show),

                new WelcomeItem(
                    "Be notified",
                    "Invited to lunch? We'll let you know with a push notification or text message." +
                    " You can see the location and who's invited before you RSVP.",
                    ImageBase + "Notification.png",
                    async () => await new PhoneSignUpDialog().ShowAsync()),

                new WelcomeItem(
                    "Good to go!",
                    "You're all set! Enjoy lunching!",
                    ImageBase + "Check.png",
                    () => Window.Current.Content = new Views.Shell()),

                new WelcomeItem("", "", "")
            }); 

            SelectedWelcomeItem = WelcomeItems[0];
        }

        /// <summary>
        /// The collection of Welcome Items.
        /// </summary>
        public ObservableCollection<WelcomeItem> WelcomeItems { get; }

        /// <summary>
        /// Gets or sets the current welcome item and invokes 
        /// the previous item's "OnNavigatedFrom" command, if any. 
        /// </summary>
        public WelcomeItem SelectedWelcomeItem { get; set; }

        /// <summary>
        /// The enabled manipulation modes for the welcome FlipView.
        /// </summary>
        public bool IsEnabled { get; set; } = true; 

        /// <summary>
        ///  Invokes a command when the user navigates from a welcome item. 
        /// </summary>
        private async void OnSelectedWelcomeItemChanged()
        {
            IsEnabled = false;
            if (null != WelcomeItems)
            {
                int index = WelcomeItems.IndexOf(SelectedWelcomeItem) - 1;
                if (index >= 0 && null != WelcomeItems[index]?.OnNavigatedFrom)
                {
                    WelcomeItems[index].OnNavigatedFrom();
                }
                ChangeFontSize();
                await Task.Run(async () =>
                {
                    await Task.Delay(1000);
                    await DispatchHelper.RunAsync(() => IsEnabled = true);
                });
            }
        }

        /// <summary>
        /// Make text larger or smaller depending on window size.
        /// </summary>
        public void ChangeFontSize() => SelectedWelcomeItem.ContentFontSize = 
            Window.Current.Bounds.Width < App.NarrowWidth ? App.FontSizeSmall : App.FontSizeMedium;
    }

    /// <summary>
    /// Represents an welcome item displayed during the OOTB experience. 
    /// </summary>
    [ImplementPropertyChanged]
    public class WelcomeItem
    {
        /// <summary>
        /// Creates a new welcome item. 
        /// </summary>
        public WelcomeItem(string headerText, string contentText,
            string image, Action onNavigatedFrom = null)
        {
            HeaderText = headerText;
            ContentText = contentText;
            if (!String.IsNullOrEmpty(image))
            {
                Image = new Uri(image);
            }
            OnNavigatedFrom = onNavigatedFrom;

            // Make text larger or smaller depending on window size
            if (Window.Current.Bounds.Width < App.NarrowWidth)
            {
                ContentFontSize = App.FontSizeSmall;
            }
            else
            {
                ContentFontSize = App.FontSizeMedium;
            }
        }

        /// <summary>
        /// Gets or sets the content text.
        /// </summary>
        public string ContentText { get; set; }

        /// <summary>
        /// The font size of the content text.
        /// </summary>
        public double ContentFontSize { get; set; }

        /// <summary>
        /// Gets or sets the header text.
        /// </summary>
        public string HeaderText { get; set; }

        /// <summary>
        /// Gets or sets the image.
        /// </summary>
        public Uri Image { get; set; }

        /// <summary>
        /// An action to fire when the user navigates away from the welcome item. 
        /// </summary>
        public Action OnNavigatedFrom { get; set; }
    }
}