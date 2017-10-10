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

using LunchScheduler.Models;
using LunchScheduler.ViewModels;
using Microsoft.Toolkit.Uwp.UI.Animations;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace LunchScheduler.CustomControls
{
    public sealed partial class ResponsePane : UserControl
    {
        public MainViewModel ViewModel => App.ViewModel;

        public ResponsePane()
        {
            this.InitializeComponent();

            DataContextChanged += ResponsePane_DataContextChanged;

            RunNormalAnimations();
        }

        private void ResponsePane_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            var invitation = DataContext as Invitation;

            if (invitation != null)
            {
                if (invitation.Response == InviteResponseKind.None)
                {
                    SetPendingState();
                }
                else if (invitation.Response == InviteResponseKind.Accepted)
                {
                    SetAcceptedState();
                }
                else if (invitation.Response == InviteResponseKind.Declined)
                {
                    SetDeclinedState();
                }
            }
        }

        private void SubmitResponse(InviteResponseKind response, Invitation invitation)
        {
            if (invitation is null)
            {
                invitation = DataContext as Invitation;
            }

            if (invitation != null)
            {
                if (response == InviteResponseKind.Accepted)
                {
                    invitation.Response = response;
                    ViewModel.JoinLunch(invitation);
                }
                else if (response == InviteResponseKind.Declined)
                {
                    ViewModel.DeclineLunch(invitation);
                }
            }
        }

        private async void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            ((Button)sender).IsEnabled = false;
            // Get the invitation here. The data context could change while the 
            // animations are running, before the response is submitted.
            var invitation = DataContext as Invitation;
            await SetAcceptedStateAsync();
            SubmitResponse(InviteResponseKind.Accepted, invitation);
            ((Button)sender).IsEnabled = true;
        }

        private void DeclineButton_Click(object sender, RoutedEventArgs e)
        {
            SetDeclinedState();

            var invitation = DataContext as Invitation;
            if (invitation != null)
            {
                invitation.Response = InviteResponseKind.Declined;
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            SetPendingState();

            var invitation = DataContext as Invitation;
            if (invitation != null)
            {
                invitation.Response = InviteResponseKind.None;
            }
        }

        private void SetPendingState()
        {
            VisualStateManager.GoToState(this, "Pending", true);
            RunNormalAnimations();
        }

        private void SetAcceptedState()
        {
            VisualStateManager.GoToState(this, "Accepted", true);
            RunResponseAnimations();
            DeclineRoot.Fade(value: 0.0f, duration: 0, delay: 0).Start();
            AcceptRoot.Offset(offsetX: 0.0f, offsetY: 26.0f, duration: 800, delay: 0).Start();
        }

        private async Task SetAcceptedStateAsync()
        {
            VisualStateManager.GoToState(this, "Accepted", true);
            RunResponseAnimations();
            DeclineRoot.Fade(value: 0.0f, duration: 0, delay: 0).Start();
            await AcceptRoot.Offset(offsetX: 0.0f, offsetY: 26.0f, duration: 800, delay: 0).StartAsync();
        }

        private void SetDeclinedState()
        {
            VisualStateManager.GoToState(this, "Declined", true);
            RunResponseAnimations();
            AcceptRoot.Fade(value: 0.0f, duration: 0, delay: 0).Start();
            DeclineRoot.Fade(value: 1.0f, duration: 0, delay: 0).Start();
            DeclineRoot.Offset(offsetX: 0.0f, offsetY: -26.0f, duration: 800, delay: 0).Start();
        }

        private void RunResponseAnimations()
        {
            // Show
            ResponseSubmittedRoot.Fade(value: 1.0f, duration: 600, delay: 0).Start();

            // Hide
            //HeaderTextBlock.Fade(value: 0.0f, duration: 200, delay: 0).Start();
            InviteesRoot.Fade(value: 0.0f, duration: 400, delay: 0).Start();
            LunchInfoRoot.Fade(value: 0.0f, duration: 400, delay: 0).Start();
        }

        private void RunNormalAnimations()
        {
            // Show
            //HeaderTextBlock.Fade(value: 1.0f, duration: 200, delay: 0).Start();
            InviteesRoot.Fade(value: 1.0f, duration: 400, delay: 0).Start();
            LunchInfoRoot.Fade(value: 1.0f, duration: 400, delay: 0).Start();
            DeclineRoot.Fade(value: 1.0f, duration: 0, delay: 0).Start();
            AcceptRoot.Fade(value: 1.0f, duration: 0, delay: 0).Start();

            // Hide
            ResponseSubmittedRoot.Fade(value: 0.0f, duration: 400, delay: 0).Start();

            // Move
            AcceptRoot.Offset(offsetX: 0.0f, offsetY: 0.0f, duration: 300, delay: 0).Start();
            DeclineRoot.Offset(offsetX: 0.0f, offsetY: 0.0f, duration: 300, delay: 0).Start();
        }
    }
}
