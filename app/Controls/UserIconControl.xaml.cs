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

using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace LunchScheduler.App.Controls
{
    /// <summary>
    /// Circular icon containing a user's name and photo.
    /// </summary>
    public sealed partial class UserIconControl : UserControl
    {
        /// <summary>
        /// Creates a new UserIconControl.
        /// </summary>
        public UserIconControl()
        {
            this.InitializeComponent();
            ((FrameworkElement)Content).DataContext = this;
        }

        /// <summary>
        /// Gets or sets the user's display name. 
        /// </summary>
        public string UserName
        {
            get { return (string)GetValue(UserNameProperty); }
            set { SetValueDp(UserNameProperty, value); }
        }
        public static readonly DependencyProperty UserNameProperty = DependencyProperty.Register(
            nameof(UserName), typeof(string), typeof(UserIconControl), null);

        /// <summary>
        /// Gets or sets the url to the user's photo. 
        /// </summary>
        public string PhotoUrl
        {
            get { return (string)GetValue(PhotoUrlProperty); }
            set { SetValueDp(PhotoUrlProperty, value); }
        }
        
        /// <summary>
        /// Dependency property for the user's photo url.
        /// </summary>
        public static readonly DependencyProperty PhotoUrlProperty = DependencyProperty.Register(
            nameof(PhotoUrl), typeof(string), typeof(UserIconControl), null);


        /// <summary>
        /// Sets a green or red circle around the user's icon to indicate whether they have
        /// accepted or rejected a lunch invite.
        /// </summary>
        public bool? ResponseMark
        {
            get { return (bool?)GetValue(ResponseMarkProperty); }
            set
            {
                SetValueDp(ResponseMarkProperty, value);
                if (null != value)
                {
                    Ellipse.Stroke = (bool)value ?
                        new SolidColorBrush(Colors.Green) :
                        new SolidColorBrush(Colors.Red);
                    Ellipse.StrokeThickness = 4;
                }
                else
                {
                    Ellipse.StrokeThickness = 0; 
                }
            }
        }

        /// <summary>
        /// Dependency property for the user's response mark.
        /// </summary>
        public static readonly DependencyProperty ResponseMarkProperty = DependencyProperty.Register(
            nameof(ResponseMark), typeof(bool?), typeof(UserIconControl), null);

        /// <summary>
        /// Sets the value of a dependency property and raises change notification.
        /// </summary>
        private void SetValueDp(DependencyProperty property, object value, [CallerMemberName]string p = null)
        {
            SetValue(property, value);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
        }

        /// <summary>
        /// Handles failiure to load a user's image.
        /// </summary>
        private void ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            // Do nothing, leave the ellipse empty.
        }

        /// <summary>
        /// Provides change notification.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
    }
}