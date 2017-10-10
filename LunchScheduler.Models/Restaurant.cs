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
using System.Diagnostics;

namespace LunchScheduler.Models
{
    /// <summary>
    /// Represents a restaurant. 
    /// </summary>
    [DebuggerDisplay("{Name}")]
    public class Restaurant : ModelBase
    {
        private string _name; 
        /// <summary>
        /// Gets or sets the restaurant's name.
        /// </summary>
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value); 
        }

        private double _rating; 
        /// <summary>
        /// Gets or sets the restaurant's Yelp rating. Must be between 0 and 5, inclusive.
        /// </summary>
        public double Rating
        {
            get => _rating;
            set
            {
                if (value < 0 || value > 5)
                {
                    throw new ArgumentException("Value must be between 0 and 5, inclusive");  
                }
                SetProperty(ref _rating, value); 
            }
        }

        private string _photoUrl;
        /// <summary>
        /// Gets or sets the URL for the restaurant's photo.
        /// </summary>
        public string PhotoUrl
        {
            get => _photoUrl;
            set => SetProperty(ref _photoUrl, value); 
        }

        private string _address;
        /// <summary>
        /// Gets or sets the restaurant's address. 
        /// </summary>
        public string Address
        {
            get => _address;
            set => SetProperty(ref _address, value); 
        }

        private double _lat; 
        /// <summary>
        /// Gets or sets the restaurant's latitude.
        /// </summary>
        public double Latitude
        {
            get => _lat;
            set => SetProperty(ref _lat, value); 
        }

        private double _lng; 
        /// <summary>
        /// Gets or sets the restaurant's longitude. 
        /// </summary>
        public double Longitude
        {
            get => _lng;
            set => SetProperty(ref _lng, value); 
        }

        private string _yelpId; 
        /// <summary>
        /// Gets or sets the restaurant's Id returned by the Yelp API.
        /// Used to find existing duplicate Yelp entries. 
        /// </summary>
        public string YelpId
        {
            get => _yelpId;
            set => SetProperty(ref _yelpId, value); 
        }

        private string _price;
        /// <summary>
        /// Gets or sets the restaurant's price range, expressed as '$' symbols. 
        /// </summary>
        public string Price
        {
            get => _price;
            set
            {
                SetProperty(ref _price, value); 
            }
        }

        private string _category;
        /// <summary>
        /// Gets or sets the restaurant's category (e.g., coffee, lunch, fast food, etc). 
        /// </summary>
        public string Category
        {
            get => _category;
            set => SetProperty(ref _category, value);
        }

        private double _distance;
        /// <summary>
        /// Gets or sets the distance the restaurant is from the search location. 
        /// </summary>
        public double Distance
        {
            get => _distance;
            set => SetProperty(ref _distance, value);
        }
    }
}
