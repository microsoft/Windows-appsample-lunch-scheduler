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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace LunchScheduler.Common
{
    public class ShellNavigationItem : INotifyPropertyChanged
    {
        private bool _isSelected;

        private Visibility _selectedVis = Visibility.Collapsed;
        public Visibility SelectedVis
        {
            get { return _selectedVis; }
            set { Set(ref _selectedVis, value); }
        }

        public string Label { get; set; }
        public Symbol Symbol { get; set; }
        public char SymbolAsChar { get { return (char)Symbol; } }
        public Type PageType { get; set; }

        private IconElement _iconElement = null;
        public IconElement Icon
        {
            get
            {
                var foregroundBinding = new Binding
                {
                    Source = this,
                    Path = new PropertyPath("SelectedForeground"),
                    Mode = BindingMode.OneWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };

                if (_iconElement != null)
                {
                    BindingOperations.SetBinding(_iconElement, IconElement.ForegroundProperty, foregroundBinding);

                    return _iconElement;
                }

                var fontIcon = new FontIcon { FontSize = 16, Glyph = SymbolAsChar.ToString() };

                BindingOperations.SetBinding(fontIcon, FontIcon.ForegroundProperty, foregroundBinding);

                return fontIcon;
            }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                Set(ref _isSelected, value);
                SelectedVis = value ? Visibility.Visible : Visibility.Collapsed;
                SelectedForeground = value
                    ? Application.Current.Resources["SystemControlForegroundAccentBrush"] as SolidColorBrush
                    : GetStandardTextColorBrush();
            }
        }

        private SolidColorBrush GetStandardTextColorBrush()
        {
            var brush = Application.Current.Resources["SystemControlForegroundBaseHighBrush"] as SolidColorBrush;

            return brush;
        }

        private SolidColorBrush _selectedForeground = null;
        public SolidColorBrush SelectedForeground
        {
            get
            {
                return _selectedForeground ?? (_selectedForeground = GetStandardTextColorBrush());
            }
            set { Set(ref _selectedForeground, value); }
        }

        private ShellNavigationItem(string name, Symbol symbol, Type pageType)
        {
            this.Label = name;
            this.Symbol = symbol;
            this.PageType = pageType;
        }

        private ShellNavigationItem(string name, IconElement icon, Type pageType)
        {
            this.Label = name;
            this._iconElement = icon;
            this.PageType = pageType;
        }

        public static ShellNavigationItem FromType<T>(string name, Symbol symbol) where T : Page
        {
            return new ShellNavigationItem(name, symbol, typeof(T));
        }

        public static ShellNavigationItem FromType<T>(string name, IconElement icon) where T : Page
        {
            return new ShellNavigationItem(name, icon, typeof(T));
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
    }
}
