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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace LunchScheduler.Common
{
    public static class NavigationService
    {
        private static Frame _frame;
        public static Frame Frame
        {
            get
            {
                if (_frame == null)
                {
                    _frame = Window.Current.Content as Frame;
                }

                return _frame;
            }
            set
            {
                _frame = value;
            }
        }

        public static bool CanGoBack => Frame.CanGoBack;
        public static bool CanGoForward => Frame.CanGoForward;

        public static void GoBack() => Frame.GoBack();
        public static void GoForward() => Frame.GoForward();

        public static bool Navigate(Type pageType, object parameter = null, NavigationTransitionInfo infoOverride = null)
        {
            // Don't open the same page multiple times
            if (Frame.Content?.GetType() != pageType)
            {
                return Frame.Navigate(pageType, parameter, infoOverride);
            }
            else
            {
                return false;
            }
        }

        public static bool Navigate<T>(object parameter = null, NavigationTransitionInfo infoOverride = null) where T : Page => Navigate(typeof(T), parameter, infoOverride);
    }
}
