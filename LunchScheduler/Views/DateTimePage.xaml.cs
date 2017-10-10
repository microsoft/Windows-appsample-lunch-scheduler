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
using System.Linq;
using LunchScheduler.ViewModels;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace LunchScheduler.Views
{
    public sealed partial class DateTimePage : Page
    {
        public MainViewModel ViewModel => App.ViewModel;

        public DateTimePage()
        {
            this.InitializeComponent();
        }

        private void LunchDateCalendar_CalendarViewDayItemChanging(CalendarView sender, CalendarViewDayItemChangingEventArgs args)
        {
            // Render basic day items.
            if (args.Phase == 0)
            {
                // Register callback for next phase.
                args.RegisterUpdateCallback(LunchDateCalendar_CalendarViewDayItemChanging);
            }
            // Set blackout dates.
            else if (args.Phase == 1)
            {
                // Blackout dates in the past.
                if (args.Item.Date.Date < DateTimeOffset.Now.Date)
                {
                    args.Item.IsBlackout = true;
                }
                // Register callback for next phase.
                args.RegisterUpdateCallback(LunchDateCalendar_CalendarViewDayItemChanging);
            }
            // Set density bars.
            else if (args.Phase == 2)
            {
                // Avoid unnecessary processing.
                // You don't need to set bars on past dates.
                if (args.Item.Date >= DateTimeOffset.Now)
                {
                    // Get accepted lunches for the date being rendered.
                    var currentLunches = ViewModel.User.Lunches.Where(x => x.Date.Date.Equals(args.Item.Date.Date));

                    var acceptColor = (Color)App.Current.Resources["BrandAcceptColor"];
                    var declineColor = (Color)App.Current.Resources["BrandDeclineColor"];

                    List<Color> densityColors = new List<Color>();
                    // Set a density bar color for each of the days lunches.
                    // It's assumed that there can't be more than 10 lunches in a day. Otherwise,
                    // further processing is needed to fit within the max of 10 density bars.
                    foreach (Models.Lunch lunch in currentLunches)
                    {
                        densityColors.Add(acceptColor);
                    }

                    // Get pending lunches for the date being rendered.
                    var pendingLunches = ViewModel.User.Invitations.Where(x => x.Lunch.Date.Date.Equals(args.Item.Date.Date));
                    foreach (Models.Invitation invitation in pendingLunches)
                    {
                        densityColors.Add(declineColor);
                    }

                    args.Item.SetDensityColors(densityColors);
                }
            }
        }

        private void LunchDatePicker_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            if (args.NewDate != null)
            {
                ViewModel.LunchBeingCreated.Date = args.NewDate.Value.Date.Date;
            }
        }

        private void LunchTimePicker_TimeChanged(object sender, TimePickerValueChangedEventArgs e)
        {
            SetTime(e.NewTime.Hours, e.NewTime.Minutes);
        }

        private void TextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ((FrameworkElement)sender).Visibility = Visibility.Collapsed;
            LunchTimePicker.Visibility = Visibility.Visible;

            // Default time to noon.
            SetTime(12, 0);
        }

        private void SetTime(int hours, int minutes)
        {
            var lbc = ViewModel.LunchBeingCreated;

            if (lbc != null)
            {
                lbc.Date = new DateTime(lbc.Date.Year, lbc.Date.Month, lbc.Date.Day, hours, minutes, 0);
            }
        }

        private void LunchDatePicker_Loaded(object sender, RoutedEventArgs e)
        {
            ((CalendarDatePicker)sender).MinDate = DateTime.Now;
        }
    }
}
