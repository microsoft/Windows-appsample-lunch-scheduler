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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using LunchScheduler.Common;
using LunchScheduler.ViewModels;
using LunchScheduler.Views;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace LunchScheduler
{
    public sealed partial class MainPage : Page
    {
        public MainViewModel ViewModel => App.ViewModel;

        private ObservableCollection<ShellNavigationItem> _navigationItems = new ObservableCollection<ShellNavigationItem>();
        public ObservableCollection<ShellNavigationItem> NavigationItems
        {
            get { return _navigationItems; }
            set { Set(ref _navigationItems, value); }
        }

        public MainPage()
        {
            this.InitializeComponent();

            NavigationService.Frame = ContentFrame;
            NavigationService.Frame.Navigated += NavigationService_Navigated;
            PopulateNavItems();

            //SystemNavigationManager.GetForCurrentView().BackRequested += MainPage_BackRequested;

            // Hide default title bar.
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            UpdateTitleBarLayout(coreTitleBar);
            // Set AppTitleBar element as a draggable region.
            Window.Current.SetTitleBar(AppTitleBar);

            // Register a handler for when the size of the overlaid caption control changes.
            // TODO: For example, when the app moves to a screen with a different DPI.
            coreTitleBar.LayoutMetricsChanged += CoreTitleBar_LayoutMetricsChanged;
        }
        private void CoreTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            UpdateTitleBarLayout(sender);
        }

        private void UpdateTitleBarLayout(CoreApplicationViewTitleBar coreTitleBar)
        {
            LeftPaddingColumn.Width = new GridLength(coreTitleBar.SystemOverlayLeftInset);
            RightPaddingColumn.Width = new GridLength(coreTitleBar.SystemOverlayRightInset);
            AppTitleBar.Height = coreTitleBar.Height;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            ConnectedAnimation imageAnimation =
                ConnectedAnimationService.GetForCurrentView().GetAnimation("image");
            if (imageAnimation != null)
            {
                imageAnimation.TryStart(UserPicture);
            }

            await ViewModel.LoadRestaurantsAsync();
        }

        private void PopulateNavItems()
        {
            _navigationItems.Clear();

            _navigationItems.Add(ShellNavigationItem.FromType<OverviewPage>("Overview", Symbol.Home));
            _navigationItems.Add(ShellNavigationItem.FromType<LunchesPage>("My lunches", Symbol.Calendar));
            _navigationItems.Add(ShellNavigationItem.FromType<FriendsPage>("People", Symbol.People));
            _navigationItems.Add(ShellNavigationItem.FromType<PlacesPage>("Places", Symbol.Map));
        }

        private void NavigationService_Navigated(object sender, NavigationEventArgs e)
        {
            NavList.Visibility = Visibility.Visible;
            NavIndicator.Visibility = Visibility.Visible;

            var spt = e.SourcePageType;

            if (typeof(Views.OverviewPage).Equals(spt))
            {
                MoveNavIndicator(0);
            }
            else if (typeof(Views.LunchesPage).Equals(spt))
            {
                MoveNavIndicator(1);
            }
            else if (typeof(Views.FriendsPage).Equals(spt))
            {
                MoveNavIndicator(2);
            }
            else if (typeof(PlacesPage).Equals(spt))
            {
                MoveNavIndicator(3);
            }
            else
            {
                NavList.SelectedIndex = -1;
                NavList.Visibility = Visibility.Collapsed;
                NavIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void MoveNavIndicator(int index)
        {
            NavIndicatorOffset.X = index * NavIndicator.X2;
        }

        private void NavList_Loaded(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(typeof(OverviewPage));
        }

        private void NavList_ItemClick(object sender, ItemClickEventArgs e)
        {
            var navigationItem = e.ClickedItem as ShellNavigationItem;
            if (navigationItem != null)
            {
                NavigationService.Navigate(navigationItem.PageType);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return;
            }

            storage = value;
            OnPropertyChanged(propertyName);
        }

        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private async void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            await App.AuthenticationViewModel.LogoutAsync();
            Frame rootFrame = Window.Current.Content as Frame;
            rootFrame.Navigate(typeof(LoginPage)); 
        }

        private void UserPicture_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }
    }
}

