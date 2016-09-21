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

using Newtonsoft.Json.Linq;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace LunchScheduler.App.Controls
{
    /// <summary>
    /// DialogBox for adding a friend.
    /// </summary>
    [ImplementPropertyChanged]
    public sealed partial class AddFriendDialog : ContentDialog
    {
        /// <summary>
        /// Creates a new AddFriendDialog.
        /// </summary>
        public AddFriendDialog()
        {
            this.InitializeComponent();
            ((FrameworkElement)this.Content).DataContext = this;
            PrimaryButtonClick += (s, e) =>
            {
                string email = SearchFriendTextBox.Text.Split('(')[1].Replace(")", "");
                FriendAdded?.Invoke(this, email);
            };
        }

        /// <summary>
        /// The text in the entry box.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// The names of users matching the user's partial entry.
        /// </summary>
        public ObservableCollection<string> Names { get; set; } = new ObservableCollection<string>();

        /// <summary>
        /// Gets users matching the currently entered text. Fires whenever the text changes.
        /// </summary>
        private async void TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var names = await App.Api.GetAsync<IEnumerable<JToken>>("Usernames",
                    new { Name = SearchFriendTextBox.Text });
                if (null != names)
                {
                    Names = new ObservableCollection<string>(names
                        .Select(x => new { Name = x["Name"].ToString(), Email = x["Email"].ToString() })
                        .Where(x => x.Email != App.User.Email)
                        .Select(x => $"{x.Name} ({x.Email})")); 
                }
            }
        }

        /// <summary>
        /// Fills the AutoSuggestionBox when a specific user is chosen.
        /// </summary>
        private void SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args) =>
            SearchFriendTextBox.Text = args.SelectedItem.ToString();

        /// <summary>
        /// Fires when a friend is added.
        /// </summary>
        public event EventHandler<string> FriendAdded;
    }
}
