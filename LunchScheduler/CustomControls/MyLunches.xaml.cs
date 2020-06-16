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
using System.Linq;
using System.Text;
using LunchScheduler.Common;
using LunchScheduler.Models;
using LunchScheduler.ViewModels;
using LunchScheduler.Views;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace LunchScheduler.CustomControls
{
    public sealed partial class MyLunches : UserControl
    {
        public MainViewModel ViewModel => App.ViewModel;

        public string GreetingString = String.Empty;

        double RequiredGridWidthNormal = 0.0;
        bool IsNormalState = true;
        double AvailableGridSpace = 0.0;
        double GridAdjustmentSize = 0.0;

        public MyLunches()
        {
            this.InitializeComponent();

            ViewModel.User.Lunches.CollectionChanged += Lunches_CollectionChanged;

            Loaded += MyLunches_Loaded;
        }

        private void MyLunches_Loaded(object sender, RoutedEventArgs e)
        {
            // This should only be set once, while the items are in their Normal state.
            if (GridAdjustmentSize == 0.0)
            {
                GridAdjustmentSize = LunchIcon.ActualWidth + LunchIcon.Margin.Left + LunchIcon.Margin.Right +
                                                        NextLunchText.ActualWidth + NextLunchText.Margin.Left + NextLunchText.Margin.Right +
                                                        CreateLunchButton.ActualWidth + CreateLunchButton.Margin.Left + CreateLunchButton.Margin.Right +
                                                        LunchesList.Margin.Left + LunchesList.Margin.Right;
            }

            AvailableGridSpace = rootPanel.ActualWidth - GridAdjustmentSize;
            RequiredGridWidthNormal = 140 * LunchesList.Items.Count;
            UpdateItemSize();
        }

        private void CreateLunch_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(typeof(CreateLunchPage));
        }

        private async void LunchesList_ItemClick(object sender, ItemClickEventArgs e)
        {
            LunchDetailsDialog DetailsDialog = new LunchDetailsDialog
            {
                DataContext = e.ClickedItem,
                PrimaryButtonText = String.Empty,
                CloseButtonText = "OK"
            };

            // Only allow changing response on invitations recieved from others.
            if (!((Lunch)e.ClickedItem).Host.Id.Equals(ViewModel.User.Id))
            {
                DetailsDialog.PrimaryButtonText = "Change response";
            }

            ContentDialogResult result = await DetailsDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var invitation = ((Lunch)e.ClickedItem).Invitations.Where(x => x.User.Id == ViewModel.User.Id).FirstOrDefault();

                ContentDialog ResponseDialog = new ContentDialog()
                {
                    DataContext = invitation,
                    Title = "Change your response?",
                    Content = "You can change your response to Declined, or retract your reponse and respond later.",
                    PrimaryButtonText = "Decline",
                    SecondaryButtonText = "Respond later",
                    CloseButtonText = "Cancel"
                };

                ContentDialogResult response = await ResponseDialog.ShowAsync();

                if (response == ContentDialogResult.Primary)
                {
                    invitation.Response = InviteResponseKind.Declined;
                    ViewModel.LeaveLunch(invitation);
                }
                else if (response == ContentDialogResult.Secondary)
                {
                    invitation.Response = InviteResponseKind.None;
                    ViewModel.LeaveLunch(invitation);
                }
            }
        }

        private void Lunches_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RequiredGridWidthNormal = 140 * LunchesList.Items.Count;
            UpdateItemSize();
        }

        private void UpdateItemSize()
        {
            if (IsNormalState == true && AvailableGridSpace < RequiredGridWidthNormal)
            {
                VisualStateManager.GoToState(this, "Small", true);
                IsNormalState = false;
            }
            else if (IsNormalState == false && AvailableGridSpace > RequiredGridWidthNormal)
            {
                VisualStateManager.GoToState(this, "Normal", true);
                IsNormalState = true;
            }
        }

        private void rootPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            AvailableGridSpace = e.NewSize.Width - GridAdjustmentSize;
            UpdateItemSize();
        }
    }
}