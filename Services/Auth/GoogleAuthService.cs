using System;
using System.Configuration;
using System.Net;
using AttandanceSyncApp.Models.Auth;
using AttandanceSyncApp.Models.DTOs;
using AttandanceSyncApp.Services.Interfaces.Auth;
using Newtonsoft.Json;

namespace AttandanceSyncApp.Services.Auth
{
    /// <summary>
    /// Service for handling Google OAuth authentication.
    /// Manages authorization URLs, token exchange, and ID token validation with Google.
    /// </summary>
    public class GoogleAuthService : IGoogleAuthService
    {
        /// Google OAuth client ID from configuration.
        private readonly string _clientId;
        /// Google OAuth client secret from configuration.
        private readonly string _clientSecret;
        /// OAuth redirect URI from configuration.
        private readonly string _redirectUri;

        /// <summary>
        /// Initializes a new GoogleAuthService with configuration from app settings.
        /// </summary>
        public GoogleAuthService()
        {
            // Load Google OAuth configuration from app settings
            _clientId = ConfigurationManager.AppSettings["GoogleClientId"];
            _clientSecret = ConfigurationManager.AppSettings["GoogleClientSecret"];
            _redirectUri = ConfigurationManager.AppSettings["GoogleRedirectUri"];
        }

        /// <summary>
        /// Generates the Google OAuth authorization URL for user login.
        /// </summary>
        /// <param name="state">State parameter for CSRF protection.</param>
        /// <returns>Google OAuth authorization URL.</returns>
        public string GetAuthorizationUrl(string state)
        {
            // Request openid, email, and profile scopes
            var scope = "openid email profile";
            return $"https://accounts.google.com/o/oauth2/v2/auth?" +
                   $"client_id={_clientId}&" +
                   $"redirect_uri={Uri.EscapeDataString(_redirectUri)}&" +
                   $"response_type=code&" +
                   $"scope={Uri.EscapeDataString(scope)}&" +
                   $"state={state}&" +
                   $"access_type=offline";
        }

        /// <summary>
        /// Exchanges an authorization code for Google OAuth tokens and validates the ID token.
        /// </summary>
        /// <param name="code">The authorization code from Google OAuth callback.</param>
        /// <returns>Google user information from validated ID token.</returns>
        public ServiceResult<GoogleUserInfo> ExchangeCodeForTokens(string code)
        {
            try
            {
                using (var client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";

                    // Build token exchange request
                    var postData = $"code={Uri.EscapeDataString(code)}" +
                                   $"&client_id={Uri.EscapeDataString(_clientId)}" +
                                   $"&client_secret={Uri.EscapeDataString(_clientSecret)}" +
                                   $"&redirect_uri={Uri.EscapeDataString(_redirectUri)}" +
                                   $"&grant_type=authorization_code";

                    // Exchange code for tokens
                    var response = client.UploadString("https://oauth2.googleapis.com/token", postData);
                    var tokenResponse = JsonConvert.DeserializeObject<GoogleTokenResponse>(response);

                    // Validate and extract user info from ID token
                    return ValidateIdToken(tokenResponse.IdToken);
                }
            }
            catch (Exception ex)
            {
                return ServiceResult<GoogleUserInfo>.FailureResult($"Token exchange failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Validates a Google ID token and extracts user information.
        /// Verifies the token belongs to this application and hasn't expired.
        /// </summary>
        /// <param name="idToken">The Google ID token to validate.</param>
        /// <returns>Google user information if token is valid.</returns>
        public ServiceResult<GoogleUserInfo> ValidateIdToken(string idToken)
        {
            try
            {
                using (var client = new WebClient())
                {
                    // Validate token with Google's tokeninfo endpoint
                    var response = client.DownloadString($"https://oauth2.googleapis.com/tokeninfo?id_token={idToken}");
                    var tokenInfo = JsonConvert.DeserializeObject<GoogleTokenInfo>(response);

                    // Verify the token is for our app
                    if (tokenInfo.Aud != _clientId)
                    {
                        return ServiceResult<GoogleUserInfo>.FailureResult("Token not intended for this application");
                    }

                    // Check token expiration
                    long unixSeconds = long.Parse(tokenInfo.Exp);

                    DateTimeOffset expTime = new DateTimeOffset(
                        DateTime.SpecifyKind(
                            new DateTime(1970, 1, 1).AddSeconds(unixSeconds),
                            DateTimeKind.Utc
                        )
                    );

                    if (expTime <= DateTimeOffset.UtcNow)
                    {
                        return ServiceResult<GoogleUserInfo>.FailureResult("Token has expired");
                    }


                    var userInfo = new GoogleUserInfo
                    {
                        GoogleId = tokenInfo.Sub,
                        Email = tokenInfo.Email,
                        Name = tokenInfo.Name ?? tokenInfo.Email.Split('@')[0],
                        Picture = tokenInfo.Picture,
                        EmailVerified = tokenInfo.EmailVerified == "true"
                    };

                    return ServiceResult<GoogleUserInfo>.SuccessResult(userInfo);
                }
            }
            catch (WebException ex)
            {
                return ServiceResult<GoogleUserInfo>.FailureResult($"Token validation failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                return ServiceResult<GoogleUserInfo>.FailureResult($"Token validation failed: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Internal class for deserializing Google OAuth token response.
    /// </summary>
    internal class GoogleTokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("id_token")]
        public string IdToken { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }
    }

    /// <summary>
    /// Internal class for deserializing Google token info response.
    /// </summary>
    internal class GoogleTokenInfo
    {
        [JsonProperty("aud")]
        public string Aud { get; set; }

        [JsonProperty("sub")]
        public string Sub { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("email_verified")]
        public string EmailVerified { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("picture")]
        public string Picture { get; set; }

        [JsonProperty("exp")]
        public string Exp { get; set; }
    }
}
