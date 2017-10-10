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
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using LunchScheduler.Common;
using LunchScheduler.Models;
using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.Toolkit.Uwp.Services.Facebook;
using Newtonsoft.Json.Linq;
using Windows.Security.Authentication.Web.Core;
using Windows.Security.Credentials;
using Windows.Storage;
using Windows.System;
using Windows.UI.ApplicationSettings;
using Windows.UI.Popups;
using User = LunchScheduler.Models.User;
using Windows.ApplicationModel.Core;

namespace LunchScheduler.ViewModels
{
    /// <summary>
    /// Handles authentication and the login flow. 
    /// </summary>
    public class AuthenticationViewModel : Observable
    {
        /// <summary>
        /// The dictonary key for the current username in roaming storage.
        /// </summary>
        private const string RoamingUserIdKey = "CurrentUserId";

        /// <summary>
        /// The dictonary key for the current username in roaming storage.
        /// </summary>
        private const string RoamingProviderKey = "CurrentProviderId";

        /// <summary>
        /// The dictonary key for the app in the credential locker. 
        /// </summary>
        private const string CredentialManagerResourceKey = "LunchScheduler";

        public AuthenticationViewModel()
        {
            AccountsSettingsPane.GetForCurrentView().AccountCommandsRequested += BuildSettingsPaneAsync;
            if (!String.IsNullOrEmpty(Constants.FacebookAppId))
            {
                FacebookService.Instance.Initialize(Constants.FacebookAppId,
                    FacebookPermissions.PublicProfile | FacebookPermissions.UserFriends);
            }
        }

        private bool _isLoggingIn = true;
        /// <summary>
        /// Gets or sets whether a login operation is currently taking progress. 
        /// Used by the UI to determine whether to show a progress ring or the 
        /// login button. 
        /// </summary>
        public bool IsLoggingIn
        {
            get => _isLoggingIn;
            set => DispatcherHelper.ExecuteOnUIThreadAsync(() => Set(ref _isLoggingIn, value));
        }

        /// <summary>
        /// Shows the <see cref="AccountsSettingsPane"/>. 
        /// </summary>
        public void Login()
        {
            AccountsSettingsPane.Show();
        }

        /// <summary>
        /// Checks if the user has credentials stored and (if so) attempts to automatically log in. 
        /// </summary>
        public async Task TryLogInSilentlyAsync()
        {
            OnLoginStarted();
            try
            {
                if (!ApplicationData.Current.RoamingSettings.Values.ContainsKey(RoamingUserIdKey) ||
                    !ApplicationData.Current.RoamingSettings.Values.ContainsKey(RoamingProviderKey))
                {
                    OnLoginFailed();
                    return;
                }

                string userId = ApplicationData.Current.RoamingSettings.Values[RoamingUserIdKey].ToString();
                var provider = (AuthenticationProviderKind)ApplicationData.Current.RoamingSettings.Values[RoamingProviderKey];

                var vault = new PasswordVault();
                var credential = vault.RetrieveAll().FirstOrDefault(x => x.Resource == CredentialManagerResourceKey &&
                    x.UserName == userId);
                if (null == credential)
                {
                    OnLoginFailed();
                    return;
                }
                credential.RetrievePassword();
                switch (provider)
                {
                    case AuthenticationProviderKind.Demo:
                        await GetDemoUserAsync(credential.Password);
                        break;
                    case AuthenticationProviderKind.AzureActiveDirectory:
                        await GetAadUserAsync();
                        break;
                    case AuthenticationProviderKind.Facebook:
                        await GetFacebookUserAsync(credential.Password);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            catch (Exception)
            {
                OnLoginFailed();
            }
        }


        /// <summary>
        /// Logs out the current user and removes their credentials from storage. 
        /// </summary>
        public async Task LogoutAsync()
        {
            ApplicationData.Current.RoamingSettings.Values.Remove(RoamingUserIdKey);
            ApplicationData.Current.RoamingSettings.Values.Remove(RoamingProviderKey);

            var vault = new PasswordVault();
            foreach (var c in vault.RetrieveAll().Where(x => x.Resource == CredentialManagerResourceKey))
            {
                vault.Remove(c);
            }
            App.ViewModel.User = null;
            await App.Api.LogoutAsync();
        }

        /// <summary>
        /// Builds the <see cref="AccountsSettingsPane"/> with the list of supported providers and options. 
        /// For more information, see: https://docs.microsoft.com/windows/uwp/security/web-account-manager. 
        /// </summary>
        public async void BuildSettingsPaneAsync(AccountsSettingsPane s,
            AccountsSettingsPaneCommandsRequestedEventArgs e)
        {
            var deferral = e.GetDeferral();

            e.HeaderText = "Choose \"Demo\" to try the app right away, no setup required.\r\n\r\n" +
                "Or, if you've followed the configuration steps in the README, choose a Microsoft, Azure Active Directory, " +
                "or Facebook account to login using a real account.";

            var demoProvider = new WebAccountProvider("Demo", "Demo", new Uri(@"ms-appx:///Assets/WebAccountManager/DemoIcon.png"));
            e.WebAccountProviderCommands.Add(new WebAccountProviderCommand(demoProvider, OnDemoAuthSelectedAsync));

            var aadProvider = await WebAuthenticationCoreManager.FindAccountProviderAsync(
                "https://login.microsoft.com", "organizations");
            e.WebAccountProviderCommands.Add(new WebAccountProviderCommand(aadProvider, OnAadAuthSelected));

            var facebookProvider = new WebAccountProvider("Facebook", "Facebook",
                new Uri(@"ms-appx:///Assets/WebAccountManager/FacebookIcon.png"));
            e.WebAccountProviderCommands.Add(new WebAccountProviderCommand(facebookProvider, OnFacebookAuthSelected));


            e.Commands.Add(new SettingsCommand("Help", "Get help setting up authentication", OnHelpSelected));

            deferral.Complete();
        }

        /// <summary>
        /// Called to handle demo auth when the user selects it from the <see cref="AccountsSettingsPane"/>. 
        /// </summary>
        private async void OnDemoAuthSelectedAsync(WebAccountProviderCommand command)
        {
            await Task.Run(async () => await GetDemoUserAsync(String.Empty));
        }

        private async Task GetDemoUserAsync(string token)
        {
            OnLoginStarted();
            var user = await App.Api.LoginAsyc(AuthenticationProviderKind.Demo, token);
            OnLoginSucceeded(user);
        }

        /// <summary>
        /// Called to handle AAD auth when the user selects it from the <see cref="AccountsSettingsPane"/>. 
        /// </summary>
        private async void OnAadAuthSelected(WebAccountProviderCommand command)
        {
            await Task.Run(async () => await GetAadUserAsync());
        }

        /// <summary>
        /// Gets an AAD user. Unlike Facebook auth, this method does not require a token. 
        /// AAD tokens expire quickly, but can be refreshed silently in the background without 
        /// interaction after the user's initial consent. Attempting this silently refresh is 
        /// part of this method.  
        /// </summary>
        private async Task GetAadUserAsync()
        {
            OnLoginStarted();

            var provider = await WebAuthenticationCoreManager.FindAccountProviderAsync(
               "https://login.microsoft.com", "organizations");

            // Create a WebTokenRequest so the WebAuthenticationCoreManager can request a token. This is a  
            // inbox Windows 10 UWP API that provides a native UI experience for the user.

            var tokenRequest = new WebTokenRequest(provider, "User.Read", Models.Constants.GraphAppId);
            tokenRequest.Properties.Add("resource", "https://graph.microsoft.com");

            // AAD tokens expire quickly, but can be refreshed without user interaction. 
            // First, try and obtain a token silently and use it if we get one. 
            // If this doesn't work, then display the full authorization UI so the user can grant permission.

            var tokenResult = await WebAuthenticationCoreManager.GetTokenSilentlyAsync(tokenRequest);
            if (tokenResult.ResponseStatus != WebTokenRequestStatus.Success)
            {
                tokenResult = await CoreApplication.MainView.CoreWindow.Dispatcher.AwaitableRunAsync(async () =>
                {
                    return await WebAuthenticationCoreManager.RequestTokenAsync(tokenRequest);
                });
            }

            if (tokenResult.ResponseStatus == WebTokenRequestStatus.Success)
            {
                string token = tokenResult.ResponseData[0].Token;

                var user = await App.Api.LoginAsyc(AuthenticationProviderKind.AzureActiveDirectory, token);
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    // Fill in the user's profile. In the production version of the app, this data is returned 
                    // by the server. They calls are duplicated here so the app can run in demo with a "live" account 
                    // even if a server is not set up. 

                    var profileResponse = await client.GetAsync("https://graph.microsoft.com/v1.0/me");
                    JToken json = JToken.Parse(await profileResponse.Content.ReadAsStringAsync());
                    user.Name = json["displayName"].Value<string>();

                    // Microsoft Graph returns the user's photo as a byte[] array, not a directly-consumable URL.
                    // To avoid re-downloading and converting the bytes to a BitmapIamge each time or forcing 
                    // the server to download and re-send package the bytes, download it directly, cache the 
                    // file to disk, and save the path. We can then consume it from XAML binding as normal. 

                    var photoResponse = await client.GetAsync("https://graph.microsoft.com/v1.0/me/photo/$value");
                    if (photoResponse.IsSuccessStatusCode)
                    {
                        byte[] bytes = await photoResponse.Content.ReadAsByteArrayAsync();
                        string path = Path.Combine(ApplicationData.Current.LocalFolder.Path,
                            Path.GetRandomFileName() + ".png");
                        await System.IO.File.WriteAllBytesAsync(path, bytes);
                        user.PhotoUrl = path;
                    }
                    else
                    {
                        // If we couldn't get the user's photo, don't do anything. 
                        // The PersonPicture control will display their initials instead. 
                    }

                    OnLoginSucceeded(user);
                }

            }
            else
            {
                OnLoginFailed();
            }
        }

        /// <summary>
        /// Called to handle Facebook auth when the user selects it from the <see cref="AccountsSettingsPane"/>. 
        /// </summary>
        private async void OnFacebookAuthSelected(WebAccountProviderCommand command)
        {
            await Task.Run(async () =>
            {
                OnLoginStarted();
                if (await FacebookService.Instance.LoginAsync())
                {
                    string token = FacebookService.Instance.Provider.AccessTokenData.AccessToken;
                    await GetFacebookUserAsync(token);
                }
                else
                {
                    OnLoginFailed();
                }
            });
        }

        private async Task GetFacebookUserAsync(string token)
        {
            var user = await App.Api.LoginAsyc(AuthenticationProviderKind.Facebook, token);
            user.Name = FacebookService.Instance.LoggedUser;
            user.PhotoUrl = (await FacebookService.Instance.GetUserPictureInfoAsync()).Url;
            OnLoginSucceeded(user);
        }


        /// <summary>
        /// Called to launch the README in the user's default browser when selected from the 
        /// <see cref="AccountsSettingsPane"/>. 
        /// </summary>
        private async void OnHelpSelected(IUICommand command)
        {
            await Launcher.LaunchUriAsync(new Uri("http://bing.com"));
        }

        /// <summary>
        /// Stores the user's login info in roamingsettings and the password vault for auto-login on
        /// future app launches. 
        /// </summary>
        private void StoreUserInfo(User user)
        {
            ApplicationData.Current.RoamingSettings.Values[RoamingUserIdKey] = user.Id;
            ApplicationData.Current.RoamingSettings.Values[RoamingProviderKey] = (int)user.AuthenticationProviderKind;

            var vault = new PasswordVault();
            foreach (var c in vault.RetrieveAll().Where(x => x.Resource == CredentialManagerResourceKey))
            {
                vault.Remove(c);
            }
            var cred = new PasswordCredential
            {
                UserName = user.Id.ToString(),
                Password = user.AuthorizationToken,
                Resource = CredentialManagerResourceKey
            };
            vault.Add(cred);
        }

        /// <summary>
        /// Called when login starts. 
        /// </summary>
        private void OnLoginStarted()
        {
            IsLoggingIn = true;
        }

        /// <summary>
        /// Called when login fails. 
        /// </summary>
        private void OnLoginFailed(string error = null)
        {
            IsLoggingIn = false;
        }

        /// <summary>
        /// Called when login completes successfully. 
        /// </summary>
        private void OnLoginSucceeded(User user)
        {
            StoreUserInfo(user);
            DispatcherHelper.ExecuteOnUIThreadAsync(() => App.ViewModel.User = user);
            LoginCompleted?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Fires when login completes successfully. 
        /// </summary>
        public event EventHandler LoginCompleted;
    }
}