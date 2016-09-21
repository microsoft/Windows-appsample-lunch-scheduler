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

using LunchScheduler.App.Views;
using PropertyChanged;
using System;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Controls;

namespace LunchScheduler.App.ViewModels
{
    /// <summary>
    /// View model for the Shell.
    /// </summary>
    [ImplementPropertyChanged]
    public class ShellViewModel
    {        
        /// <summary>
        /// Static instance of this shell. Used by other parts of the app for navigation 
        /// outside the menu.
        /// </summary>
        public static ShellViewModel Current { get; set; }

        /// <summary>
        /// Creates a new ShellViewModel.
        /// </summary>
        public ShellViewModel(Frame frame)
        {
            Frame = frame;
            Items = new ObservableCollection<MenuItem>(new[]
            {
                new MenuItem
                {
                    Name = "Lunches",
                    Icon = "\uE10F",
                    Target = typeof(Lunches)
                },
                new MenuItem
                {
                    Name = "Friends",
                    Icon = "\uE77B",
                    Target = typeof(Friends)
                },
                new MenuItem
                {
                    Name = "Settings",
                    Icon = "\uE713",
                    Target = typeof(Settings)
                }
            });
            SelectedItem = Items[0];
            Current = this;
        }

        /// <summary>
        /// The content frame aside the hamburger.
        /// </summary>
        public Frame Frame { get; set; }

        /// <summary>
        /// Contains the hamburger menu items.
        /// </summary>
        public ObservableCollection<MenuItem> Items { get; }

        /// <summary>
        /// Gets or sets the selected hamburger menu item.
        /// </summary>
        public MenuItem SelectedItem { get; set; }

        /// <summary>
        /// Gets or sets whether the full hamburger menu is open.
        /// </summary>
        public bool IsMenuOpen { get; set; }

        /// <summary>
        /// Toggles the hamburger menu open and closed.
        /// </summary>
        public void ToggleMenu() => IsMenuOpen = !IsMenuOpen;

        /// <summary>
        /// Navigates to the selected menu item.
        /// </summary>
        public void OnSelectedItemChanged() => Navigate(SelectedItem.Target);

        /// <summary>
        /// Navigates to the given page.
        /// </summary>
        public void Navigate(Type target) => Frame.Navigate(target); 
    }

    /// <summary>
    /// Represents a menu item. 
    /// </summary>
    [ImplementPropertyChanged]
    public class MenuItem
    {
        /// <summary>
        /// The name of the menu item.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The icon to show in the menu.
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// The page to navigate to when the menu item is clicked.
        /// </summary>
        public Type Target { get; set; }
    }
}