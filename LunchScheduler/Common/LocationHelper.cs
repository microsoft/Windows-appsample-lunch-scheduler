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
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Services.Maps;

namespace LunchScheduler.Common
{
    public static class LocationHelper
    {
        /// <summary>
        /// Gets the Geolocator singleton used by the LocationHelper.
        /// </summary>
        public static Geolocator Geolocator { get; } = new Geolocator();

        /// <summary>
        /// Gets or sets the CancellationTokenSource used to enable Geolocator.GetGeopositionAsync cancellation.
        /// </summary>
        private static CancellationTokenSource CancellationTokenSource { get; set; }

        /// <summary>
        /// Initializes the LocationHelper. 
        /// </summary>
        static LocationHelper()
        {
            MapService.ServiceToken = Models.Constants.MapServiceToken;
        }

        /// <summary>
        /// Gets the current location if the geolocator is available.
        /// </summary>
        /// <returns>The current location.</returns>
        public static async Task<BasicGeoposition?> GetCurrentLocationAsync()
        {
            try
            {
                // Request permission to access the user's location.
                var accessStatus = await Geolocator.RequestAccessAsync();

                switch (accessStatus)
                {
                    case GeolocationAccessStatus.Allowed:

                        CancellationTokenSource = new CancellationTokenSource();
                        var token = CancellationTokenSource.Token;

                        Geoposition position = await Geolocator.GetGeopositionAsync().AsTask(token);
                        return position.Coordinate.Point.Position;

                    case GeolocationAccessStatus.Denied: 
                    case GeolocationAccessStatus.Unspecified:
                    default:
                        return null;
                }
            }
            catch (TaskCanceledException)
            {
                // Do nothing.
            }
            finally
            {
                CancellationTokenSource = null;
            }
            return null;
        }

        /// <summary>
        /// Cancels any waiting GetGeopositionAsync call.
        /// </summary>
        public static void CancelGetCurrentLocation()
        {
            if (CancellationTokenSource != null)
            {
                CancellationTokenSource.Cancel();
                CancellationTokenSource = null;
            }
        }
    }
}
