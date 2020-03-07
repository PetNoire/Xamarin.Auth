﻿using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Xamarin.Auth
{
    #if XAMARIN_AUTH_INTERNAL
    internal static class OAuth2AuthenticatorExtensions
    #else
    public static class OAuth2AuthenticatorExtensions
    #endif
    {
        # region
        //---------------------------------------------------------------------------------------
        /// Pull Request - manually added/fixed
        ///		Adding method to request a refresh token #79
        ///		https://github.com/xamarin/Xamarin.Auth/pull/79
        ///		
        /// <summary>
        /// Method that requests a new access token based on an initial refresh token
        /// </summary>
        /// <param name="refreshToken">Refresh token, typically from the <see cref="AccountStore"/>'s refresh_token property</param>
        /// <returns>Time in seconds the refresh token expires in</returns>
        #if XAMARIN_AUTH_INTERNAL
        internal static Task<int> RequestRefreshTokenAsync(this OAuth2Authenticator authenticator, string refreshToken)
        #else
        public static Task<IDictionary<string,string>> RequestRefreshTokenAsync(this OAuth2Authenticator authenticator, string refreshToken)
        #endif

        {
            var queryValues = new Dictionary<string, string>
            {
                {"refresh_token", refreshToken},
                {"client_id", authenticator.ClientId},
                {"grant_type", "refresh_token"}
            };

            if (!string.IsNullOrEmpty(authenticator.ClientSecret))
            {
                queryValues["client_secret"] = authenticator.ClientSecret;
            }

            return authenticator.RequestAccessTokenAsync(queryValues)
                    .ContinueWith
                        (
                            result =>
                            {
                                var accountProperties = result.Result;

                                authenticator.OnRetrievedAccountProperties(accountProperties);

                                return accountProperties;
                            }
                        );
        }
        //---------------------------------------------------------------------------------------
        #endregion
        
    }
    public class OAuth2RefreshRequest : OAuth2Request
    {
        public OAuth2RefreshRequest(OAuth2Authenticator authenticator, string method, Uri uri, IDictionary<string, string> parameters, ref Account account) : base(method, uri, parameters, account)
        {
            var acc = account;
            var task = Task.Run(async () => await authenticator.RequestRefreshTokenAsync(acc.Properties["refresh_token"]));
            acc = new Account("", task.Result);
            if (!acc.Properties.ContainsKey("refresh_token"))
            {
                acc.Properties.Add("refresh_token", account.Properties["refresh_token"]);
            }
            base.Account = account = acc;
        }
    }
}

