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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using LunchScheduler.Common;
using LunchScheduler.Models;
using Microsoft.Toolkit.Uwp.Helpers;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Services.Maps;

namespace LunchScheduler.ViewModels
{
    public class MainViewModel : Observable
    {
        public MainViewModel()
        {
            Restaurants.CollectionChanged += async (s, e) =>
            {
                if (e.NewItems != null)
                {
                    foreach (Restaurant restaurant in e.NewItems)
                    {
                        MappedLocations.Add(
                            new MappedLocation
                            {
                                Restaurant = restaurant,
                                Index = Restaurants.IndexOf(restaurant) + 1,
                                Position = new BasicGeoposition
                                {
                                    Latitude = restaurant.Latitude,
                                    Longitude = restaurant.Longitude
                                }
                            });
                        if (restaurant.Distance == 0)
                        {
                            await SetDistanceAsync(restaurant);
                        }
                    }
                }
                if (e.OldItems != null)
                {
                    foreach (Restaurant restaurant in e.OldItems)
                    {
                        MappedLocations.Remove(MappedLocations.First(location => location.Restaurant == restaurant));
                    }
                    foreach (var location in MappedLocations)
                    {
                        location.Index = Restaurants.IndexOf(location.Restaurant) + 1;
                    }
                }
            };
        }

        private User _user;
        public User User
        {
            get => _user;
            set => Set(ref _user, value);
        }

        public double? HoursUntilNextLunch
        {
            get
            {
                var nextLunch = User.Lunches.OrderBy(lunch => lunch.Date).FirstOrDefault();
                return nextLunch == null ? (double?)null :
                    Math.Round((nextLunch.Date - DateTime.Now).TotalHours, 1);
            }
        }

        public string NextLunchText
        {
            get
            {
                var hours = HoursUntilNextLunch;
                var days = (int)Math.Truncate(((double)HoursUntilNextLunch/24));

                return $"Hi, {User.Name}!\n" + (User.Lunches.Count == 0 ?
                      "It looks like you don't have any lunches scheduled.\nWould you like to create one?" :
                      $"Your next lunch is in\n" + (days > 0 ?
                      $"{days} day{ (days != 1 ? "s" : "")}." : $"{hours} hour{(hours != 1 ? "s" : "")}."));
            }
        }

        public void UpdateNextLunchText() => OnPropertyChanged(nameof(NextLunchText));

        public async Task UpdateLocationAsync()
        {
            BasicGeoposition? currentPosition = null;
            currentPosition = Constants.IsDemoMode ?
                new BasicGeoposition { Latitude = Constants.DemoLatitude, Longitude = Constants.DemoLongitude } :
                await LocationHelper.GetCurrentLocationAsync();
            if (currentPosition.HasValue)
            {
                if (MappedLocations.Any() && MappedLocations[0].IsCurrentLocation)
                {
                    MappedLocations[0].Position = currentPosition.Value;
                }
                else 
                {
                    MappedLocations.Insert(0, new MappedLocation
                    {
                        IsCurrentLocation = true,
                        Position = currentPosition.Value
                    });
                }
            }
        }

        public static Point NormalizedAnchorPoint { get; } = new Point(0.5, 0.5);

        private Random random = new Random();
        public async Task SetDistanceAsync(Restaurant restaurant)
        {
            #pragma warning disable CS0162 // Unreachable code detected (caused by IsDemoMode constant)
            if (Constants.IsDemoMode)
            {
                restaurant.Distance = Math.Round(random.NextDouble() * 10, digits: 1);
            }
            else if (MappedLocations.Any() && MappedLocations[0].IsCurrentLocation)
            {
                var restaurantGeopoint = new Geopoint(new BasicGeoposition
                {
                    Latitude = restaurant.Latitude,
                    Longitude = restaurant.Longitude
                });
                MapRouteFinderResult routeResult = await MapRouteFinder.GetDrivingRouteAsync(
                    MappedLocations[0].Geopoint, restaurantGeopoint);
                if (routeResult.Status == MapRouteFinderStatus.Success)
                {
                    // Calculate the distance in miles. 
                    restaurant.Distance = Math.Round(routeResult.Route.LengthInMeters * Constants.MilesPerMeter, digits: 1);
                }
            }
            #pragma warning restore CS0162 // Unreachable code detected (caused by IsDemoMode constant)
        }

        private string _searchAddress;
        public string SearchAddress
        {
            get => Constants.IsDemoMode ? Constants.DemoAddress : _searchAddress;
            set
            {
                if (Set(ref _searchAddress, value) && !Constants.IsDemoMode)
                {
                    var ignore = LoadRestaurantsAsync();
                }
            }
        }

        // Nearby restaurants. 
        public ObservableCollection<Restaurant> Restaurants { get; } = new ObservableCollection<Restaurant>();

        public ObservableCollection<MappedLocation> MappedLocations { get; } = new ObservableCollection<MappedLocation>();

        private Restaurant _selectedRestaurant;
        public Restaurant SelectedRestaurant
        {
            get => _selectedRestaurant;
            set
            {
                if (_selectedRestaurant != value)
                {
                    if (_selectedRestaurant != null)
                    {
                        MappedLocations.First(location => location.Restaurant == _selectedRestaurant).IsSelectedRestaurant = false;
                    }
                    Set(ref _selectedRestaurant, value);
                    var selectedLocation = MappedLocations.First(location => location.Restaurant == _selectedRestaurant);
                    selectedLocation.IsSelectedRestaurant = true;

                    // Move the selected location to the end of the list so that it will appear above all other location on the map. 
                    MappedLocations.Remove(selectedLocation);
                    MappedLocations.Add(selectedLocation);

                    if (LunchBeingCreated != null)
                    {
                        LunchBeingCreated.Location = _selectedRestaurant;
                    }
                }
            }
        }

        private Lunch _lunchBeingCreated;
        public Lunch LunchBeingCreated
        {
            get => _lunchBeingCreated;
            set
            {
                if (_lunchBeingCreated != value)
                {
                    if (_lunchBeingCreated != null)
                    {
                        _lunchBeingCreated.PropertyChanged -= LunchBeingCreated_PropertyChanged;
                    }
                    Set(ref _lunchBeingCreated, value);
                    if (_lunchBeingCreated != null)
                    {
                        _lunchBeingCreated.PropertyChanged += LunchBeingCreated_PropertyChanged;
                        OnPropertyChanged(nameof(IsLunchBeingCreatedComplete));
                    }
                }
            }
        }

        private void LunchBeingCreated_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(IsLunchBeingCreatedComplete));
            OnPropertyChanged(nameof(IsLunchBeingCreatedIncomplete));
        }

        public bool IsLunchBeingCreatedComplete =>
            LunchBeingCreated.Date != default(DateTime) &&
            LunchBeingCreated.Location != null;

        public bool IsLunchBeingCreatedIncomplete => !IsLunchBeingCreatedComplete;

        public void JoinLunch(Invitation invitation)
        {
            User.Invitations.Remove(invitation);
            User.Lunches.Add(invitation.Lunch);
            App.Api.RespondToInvitationAsync(invitation);
        }

        public void LeaveLunch(Invitation invitation)
        {
            User.Invitations.Add(invitation);
            User.Lunches.Remove(invitation.Lunch);
            App.Api.RespondToInvitationAsync(invitation);
        }

        public void DeclineLunch(Invitation invitation)
        {
            User.Invitations.Remove(invitation);
            App.Api.RespondToInvitationAsync(invitation);
        }

        public void CreateLunch()
        {
            LunchBeingCreated = new Lunch();
            LunchBeingCreated.Invitations = new ObservableCollection<Invitation>();
        }

        public void CreateLunch(User invitee)
        {
            LunchBeingCreated = new Lunch();
            LunchBeingCreated.Invitations = new ObservableCollection<Invitation>(
                new Invitation[] { new Invitation(LunchBeingCreated, invitee) });
        }

        public void CreateLunch(Restaurant restaurant)
        {
            LunchBeingCreated = new Lunch() { Location = restaurant };
            LunchBeingCreated.Invitations = new ObservableCollection<Invitation>();
        }

        public void CommitLunch()
        {
            User.Lunches.Add(LunchBeingCreated);
            App.Api.CreateLunchAsync(LunchBeingCreated);
            LunchBeingCreated = null;
        }

        public async Task LoadFriendsAsync()
        {
            var friends = await App.Api.GetFriendsAsync();
            foreach (var f in friends)
            {
                await DispatcherHelper.ExecuteOnUIThreadAsync(() => User.Friends.Add(f));
            }
        }

        public async Task LoadRestaurantsAsync()
        {
            IEnumerable<Restaurant> restaurants = null;

            #pragma warning disable CS0162 // Unreachable code detected (caused by IsDemoMode constant)
            if (Constants.IsDemoMode)
            {
                restaurants = await App.Api.GetRestaurantsAsync(null);
            }
            else if (!String.IsNullOrWhiteSpace(SearchAddress))
            {
                restaurants = await App.Api.GetRestaurantsAsync(SearchAddress);
            }
            else if (MappedLocations.Any() && MappedLocations[0].IsCurrentLocation)
            {
                restaurants = await App.Api.GetRestaurantsAsync(
                    MappedLocations[0].Position.Latitude, MappedLocations[0].Position.Longitude);
            }
            else
            {
                restaurants = await App.Api.GetRestaurantsAsync(null);
            }
            #pragma warning restore CS0162 // Unreachable code detected (caused by IsDemoMode constant)

            await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
            {
                Restaurants.Clear();
                foreach (var restaurant in restaurants)
                {
                    Restaurants.Add(restaurant);
                }
            });
        }
    }
}
