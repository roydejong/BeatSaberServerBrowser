using System;
using System.Threading;
using System.Threading.Tasks;
using ServerBrowser.Data;
using ServerBrowser.Requests;
using ServerBrowser.Responses;
using SiraUtil.Logging;
using UnityEngine;
using Zenject;

namespace ServerBrowser.Session
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class BssbSession : IInitializable, IDisposable, ITickable
    {
        [Inject] private readonly SiraLog _log = null!;
        [Inject] private readonly BssbConfig _config = null!;
        [Inject] private readonly BssbApi _api = null!;
        [Inject] private readonly IPlatformUserModel _platformUserModel = null!;

        public UserInfo? UserInfo { get; private set; }
        public bool IsLoggedIn { get; private set; }
        public string? AvatarUrl { get; private set; }

        private CancellationTokenSource _ctsUserInfo = new();
        private float? _nextLoginRetry = null;
        private int _loginAttempts = 0;
        private bool _loginRequested = false;
        private bool _isLoggingIn = false;
        
        private PlatformAuthenticationTokenProvider? _tokenProvider = null;
        
        public event Action<UserInfo>? UserInfoChangedEvent;
        public event Action<string?>? AvatarUrlChangedEvent;
        public event Action<BssbLoginResponse>? LoggedInEvent;

        public void Initialize()
        {
            _ = LoadUserInfo();
        }

        public void Dispose()
        {
            _ctsUserInfo.Cancel();
            _ctsUserInfo.Dispose();
        }

        public void Tick()
        {
            if (_nextLoginRetry.HasValue && Time.realtimeSinceStartup >= _nextLoginRetry.Value)
                _ = EnsureLoggedIn();
        }

        private async Task LoadUserInfo()
        {
            try
            {
                UserInfo = await _platformUserModel.GetUserInfo(_ctsUserInfo.Token);

                if (UserInfo != null)
                {
                    UserInfoChangedEvent?.Invoke(UserInfo);
                    
                    if (_loginRequested)
                        _ = EnsureLoggedIn();
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                _log.Error($"Error loading user info: {e}");
            }
        }

        private async Task<string?> GetPlatformAuthToken()
        {
            if (UserInfo == null)
                return null;

            try
            {
                _tokenProvider ??= new PlatformAuthenticationTokenProvider(_platformUserModel, UserInfo);
                return UserInfo.platform switch
                {
                    UserInfo.Platform.Steam => (await _tokenProvider.GetAuthenticationToken()).sessionToken,
                    _ => (await _tokenProvider.GetXPlatformAccessToken(CancellationToken.None)).token,
                };
            }
            catch (Exception ex)
            {
                _log.Error($"Error getting platform auth token: {ex}");
                return null;
            }
        }

        private async Task<bool> Login()
        {
            if (_isLoggingIn)
                // Try to prevent multiple login attempts at the same time
                return false;
            
            if (!_config.AnyPrivacyDisclaimerAccepted)
                // Refuse to login if the user has not accepted the privacy disclaimer
                return false;
            
            if (UserInfo == null)
                // Required info not loaded yet or not available
                return false;

            _isLoggingIn = true;
            _nextLoginRetry = null;
            _loginAttempts++;

            try
            {
                var loginResponse = await _api.SendLoginRequest(new BssbLoginRequest()
                {
                    UserInfo = UserInfo,
                    AuthenticationToken = await GetPlatformAuthToken()

                });

                if (loginResponse is not { Success: true })
                {
                    var logError = loginResponse?.ErrorMessage ?? "Unknown error";
                    _log.Error($"Login failed: {logError}");
                    IsLoggedIn = false;
                    return false;
                }

                if (AvatarUrl != loginResponse.AvatarUrl)
                {
                    AvatarUrl = loginResponse.AvatarUrl;
                    AvatarUrlChangedEvent?.Invoke(AvatarUrl);
                }

                if (!loginResponse.Authenticated)
                {
                    // Even though the login request was valid, the user has not authenticated themselves with the platform
                    // This means that authentication with Steam/Oculus/etc. failed, so we are not fully logged in
                    var logError = loginResponse?.ErrorMessage ?? "Unknown error";
                    _log.Error($"Login authentication failed: {logError}");
                    IsLoggedIn = false;
                    return false;
                }

                IsLoggedIn = true;
                _loginAttempts = 0;
                _nextLoginRetry = null;
                _log.Info($"Logged in succesfully");
                LoggedInEvent?.Invoke(loginResponse);
                return true;
            }
            finally
            {
                _isLoggingIn = false;
                if (!IsLoggedIn)
                    ScheduleLoginRetry();
            }
        }
        
        public async Task<bool> EnsureLoggedIn()
        {
            _loginRequested = true;
            
            if (IsLoggedIn)
                return true;
            
            if (_isLoggingIn)
                return false;
            
            return await Login();
        }
        
        private void ScheduleLoginRetry()
        {
            var retryDelay = Mathf.Clamp(Mathf.Pow(2f, _loginAttempts), 2f, 128f);
            _nextLoginRetry = Time.realtimeSinceStartup + retryDelay;
        }
    }
}