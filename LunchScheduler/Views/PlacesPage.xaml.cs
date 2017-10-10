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
using System.Linq;
using System.Threading.Tasks;
using LunchScheduler.Models;
using LunchScheduler.ViewModels;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Input;

namespace LunchScheduler.Views
{
    public sealed partial class PlacesPage : Page
    {
        public MainViewModel ViewModel => App.ViewModel;

        private double mapDefaultZoomLevel;

        public PlacesPage()
        {
            InitializeComponent();
            Loaded += async (s, e) => await ResetMapAsync();
            ViewModel.Restaurants.CollectionChanged += async (s, e) => await ResetMapAsync();
            ViewModel.PropertyChanged += async (s, e) =>
            {
                if (e.PropertyName == nameof(ViewModel.SelectedRestaurant))
                {
                    await SetMapViewForSelectedRestaurant();
                }
            };
        }

        private async Task SetMapViewForSelectedRestaurant()
        {
            var restaurant = ViewModel.SelectedRestaurant;
            if (restaurant == null) return;
            var restaurantGeopoint = new Geopoint(new BasicGeoposition
            {
                Latitude = restaurant.Latitude,
                Longitude = restaurant.Longitude
            });
            Map.IsLocationInView(restaurantGeopoint, out bool isRestaurantInView);
            if (!isRestaurantInView)
            {
                await ResetMapAsync();
            }
        }

        private async Task ResetMapAsync()
        {
            if (ViewModel.MappedLocations.Count() == 1)
            {
                Map.Center = ViewModel.MappedLocations[0].Geopoint;
                Map.ZoomLevel = 12;
            }
            else if (ViewModel.MappedLocations.Any())
            {
                var bounds = GeoboundingBox.TryCompute(
                    ViewModel.MappedLocations.Select(location => location.Geopoint.Position));
                var margin = new Thickness(40, 40, 40, 40);
                await Map.TrySetViewBoundsAsync(bounds, margin, MapAnimationKind.Default);
            }
            mapDefaultZoomLevel = Map.ZoomLevel;
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (((sender as Grid).Tag as MappedLocation).Restaurant is Restaurant restaurant)
            {
                ViewModel.SelectedRestaurant = restaurant;
                RestaurantList.ScrollIntoView(restaurant);
            }
        }

        private void RestaurantList_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            var templateRoot = args.ItemContainer.ContentTemplateRoot as Grid;
            var textBlock = templateRoot?.Children[0] as TextBlock;
            textBlock.Text = (args.ItemIndex + 1).ToString();
        }
    }
}
