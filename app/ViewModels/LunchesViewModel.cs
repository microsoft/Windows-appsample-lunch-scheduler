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
using PropertyChanged;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using WinRTXamlToolkit.Tools;

namespace LunchScheduler.App.ViewModels
{
    /// <summary>
    /// View model for Lunches view.
    /// </summary>
    [ImplementPropertyChanged]
    public class LunchesViewModel
    {
        /// <summary>
        /// Creates a new LunchesViewModel.
        /// </summary>
        public LunchesViewModel()
        {
            Task.Run(Initialize);
        }

        /// <summary>
        /// Contains all lunches. 
        /// </summary>
        public ObservableCollection<Lunch> Lunches { get; set; } = new ObservableCollection<Lunch>();

        /// <summary>
        /// Gets or sets the currently selected lunch. 
        /// </summary>
        public Lunch SelectedLunch { get; set; }

        /// <summary>
        /// Sets whether to display the "No lunches" string. 
        /// </summary>
        public Visibility NoLunchesStringVisibility { get; set; } = Visibility.Visible;

        /// <summary>
        /// Sets whether to display the "Accept" or "Reject" lunch buttons. 
        /// </summary>
        public Visibility ResponseButtonVisibility => 
            null != SelectedLunch ? Visibility.Visible : Visibility.Collapsed;

        /// <summary>
        /// Accepts the selected lunch.
        /// </summary>
        public async void Accept() => await SetResponse(true);

        /// <summary>
        /// Declines the selected lunch. 
        /// </summary>
        public async void Decline() => await SetResponse(false);

        /// <summary>
        /// Sends the user's response (accept or decline) to the server.
        /// </summary>
        private async Task SetResponse(bool response)
        {
            var invite = SelectedLunch.Invites.FirstOrDefault(x => x.User.Email == App.User.Email);
            invite.Response = response;
            await App.Api.PostAsync("Invite", invite);
        }

        /// <summary>
        /// Navigates to the NewLunch page. 
        /// </summary>
        public void NewLunch() => ShellViewModel.Current.Navigate(typeof(Views.NewLunch));

        /// <summary>
        /// Fires when the selected lunch changes. 
        /// </summary>
        public void OnSelectedLunchChanged() => PropertyChanged?.Invoke(this,
            new PropertyChangedEventArgs(nameof(ResponseButtonVisibility))); 
            
        /// <summary>
        /// Loads current lunches. 
        /// </summary>
        private async Task Initialize()
        {
            var lunches = await App.Api.GetAsync<IEnumerable<Lunch>>("Lunch");
            if (null != lunches && lunches.Any())
            {
                await DispatchHelper.RunAsync(() =>
                {
                    NoLunchesStringVisibility = Visibility.Collapsed;
                    lunches.ForEach(x => Lunches.Add(x));
                });
            }
        }

        /// <summary>
        /// Event handler for registering for change notification. Actual implementation
        /// is injected at compile-time by Fody. 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
    }
}