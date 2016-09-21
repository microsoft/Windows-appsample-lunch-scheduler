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

using LunchScheduler.Models;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace LunchScheduler.App.Helpers
{
    /// <summary>
    /// Handles sending updates to the database. 
    /// </summary>
    public static class SynchronizationHelper
    {
        /// <summary>
        /// Semaphore for ensuring only a single user update is sent at a time. 
        /// </summary>
        private static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Recursively registers all properties and properties in collections in the object
        /// for change notification. 
        /// </summary>
        public static void RecursiveRegisterPropertyChanged(object item)
        {
            foreach (object property in item.GetType().GetProperties().Select(x => x.GetValue(item))
                .Where(x => null != x && x.GetType().GetInterfaces().Contains(typeof(INotifyPropertyChanged))))
            {
                // For collections, only raise an event when items are added or removed
                // Otherwise, duplicate events will fire when collection properties like Count, Items[], etc change 
                if (property is INotifyCollectionChanged)
                {
                    ((INotifyCollectionChanged)property).CollectionChanged += CollectionChanged;
                    foreach (object child in (IEnumerable)property)
                    {
                        RecursiveRegisterPropertyChanged(child); 
                    }
                }
                else
                {
                    ((INotifyPropertyChanged)property).PropertyChanged += async (s, e) => await SyncAsync();
                }
            }
        }

        /// <summary>
        /// Sends the updated user object to the database using a semaphore to prevent desynchronization. 
        /// </summary>
        private static async Task SyncAsync()
        {
            await Semaphore.WaitAsync();
            await App.Api.PostAsync<User, HttpResponseMessage>("Users", App.User);
            Semaphore.Release();
        }

        /// <summary>
        /// Fires whenever a collection changes. 
        /// </summary>
        private static async void CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in args.NewItems)
                {
                    RecursiveRegisterPropertyChanged(item);
                }
                await SyncAsync();
            }
            else if (args.Action == NotifyCollectionChangedAction.Remove)
            {
                await SyncAsync();

                // TODO: Consider possible memory issues. 
                // RecursiveRegisterPropertyChanged creates a strong reference to each
                // object, which prevents them from ever being garbage collected. 
                // Because Lunch Scheduler does not create a lot of objects, the amount of 
                // memory leaked is neglible. However, if you are reusing this component 
                // in a scenario with a lot of objects, you should use weak references instead.
                // For more info, see https://msdn.microsoft.com/library/aa970850(v=vs.100).aspx.
            }
        }
    }
}