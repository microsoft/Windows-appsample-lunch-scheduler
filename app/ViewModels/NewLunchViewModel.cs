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
using LunchScheduler.App.Views;
using LunchScheduler.Models;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml;

namespace LunchScheduler.App.ViewModels
{
    /// <summary>
    /// View model for the NewLunch view.
    /// </summary>
    [ImplementPropertyChanged]
    internal class NewLunchViewModel
    {
        /// <summary>
        /// Creates a new NewLunchViewModel. 
        /// </summary>
        public NewLunchViewModel()
        {
            Task.Run(Initialize);
        }

        /// <summary>
        /// Gets the user's friends. 
        /// </summary>
        public ObservableCollection<User> Friends => App.User.Friends ?? new ObservableCollection<User>(); 

        /// <summary>
        /// Gets or sets the location options for the lunch. 
        /// </summary>
        public ObservableCollection<Location> Locations { get; set; } = new ObservableCollection<Location>();

        /// <summary>
        /// Gets or sets the friends invited to lunch. 
        /// </summary>
        public ObservableCollection<User> SelectedFriends { get; set; } = new ObservableCollection<User>();

        /// <summary>
        /// Gets or sets the selected lunch location. 
        /// </summary>
        public Location SelectedLocation { get; set; }

        /// <summary>
        /// Gets or sets the selected lunch date. Defaults to the current date. 
        /// </summary>
        public DateTimeOffset SelectedDate { get; set; } = DateTime.Now.Date;

        /// <summary>
        /// Gets or sets the selected lunch time. Defaults to 1 hour from the current time. 
        /// </summary>
        public TimeSpan SelectedTime { get; set; } = DateTime.Now.TimeOfDay.Add(new TimeSpan(1, 0, 0));

        /// <summary>
        /// Gets or sets the name of the lunch.
        /// </summary>
        public string Name { get; set; } = $"{App.User.Name}'s lunch";

        /// <summary>
        /// Gets or sets the custom location text. 
        /// </summary>
        public string CustomLocation { get; set; }

        /// <summary>
        /// Gets or sets whether to show the custom location text box. 
        /// </summary>
        public Visibility CustomLocationVisibility { get; set; } = Visibility.Collapsed; 

        /// <summary>
        /// Fires when the selected location changes. If the selected location is "custom," shows the 
        /// lunch location text box. 
        /// </summary>
        public void OnSelectedLocationChanged() => CustomLocationVisibility = SelectedLocation.Name == "Custom" ? 
            Visibility.Visible : Visibility.Collapsed; 

        /// <summary>
        /// Handles the user clicking the "Ask to lunch" button. 
        /// </summary>
        public async void AskToLunch()
        {
            var location = SelectedLocation.Name == "Custom"
                ? new Location {Name = CustomLocation}
                : SelectedLocation; 
            var lunch = new Lunch(Name,
                App.User,
                SelectedFriends.Concat(new[] { App.User }),
                location,
                SelectedDate.Add(SelectedTime).LocalDateTime);
            await App.Api.PostAsync("Lunch", lunch);
            ShellViewModel.Current.Navigate(typeof(Lunches));
        }

        /// <summary>
        /// Loads possible lunch locations based on the user's current location. 
        /// </summary>
        private async Task Initialize()
        {
            await DispatchHelper.RunAsync(async () =>
            {
                var restaurants = new List<Location>
                {
                    new Location
                    {
                        Name = "Custom",
                        ImageUrl = @"ms-appx:///Assets/Plus.png"
                    }
                };
                if (await Geolocator.RequestAccessAsync() == GeolocationAccessStatus.Allowed)
                {
                    var location = await new Geolocator().GetGeopositionAsync();
                    var nearby = await App.Api.GetAsync<IEnumerable<Location>>("Locations",
                        new { location.Coordinate.Point.Position.Longitude, location.Coordinate.Point.Position.Latitude });
                    restaurants.AddRange(nearby); 
                }
                restaurants.ForEach(x => Locations.Add(x));
            });
        }
    }
}