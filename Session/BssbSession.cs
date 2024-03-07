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
        [Inject] private readonly BssbApi _api = null!;
        [Inject] private readonly IPlatformUserModel _platformUserModel = null!;

        public UserInfo? UserInfo { get; private set; }
        public bool IsLoggedIn { get; private set; }
        public string? AvatarUrl { get; private set; }

        private CancellationTokenSource _ctsUserInfo = new();
        private float? _nextLoginRetry = null;
        private int _loginAttempts = 0;
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
                _ = Login();
        }

        private async Task LoadUserInfo()
        {
            try
            {
                UserInfo = await _platformUserModel.GetUserInfo(_ctsUserInfo.Token);

                if (UserInfo != null)
                {
                    _log.Info($"Loaded user info (platform={UserInfo.platform}" +
                              $", platformUserId={UserInfo.platformUserId}" +
                              $", userName={UserInfo.userName})");
                    
                    UserInfoChangedEvent?.Invoke(UserInfo);
                    _ = Login();
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

        private async Task Login()
        {
            if (UserInfo == null)
                return;

            _nextLoginRetry = null;
            _loginAttempts++;
            
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
                ScheduleLoginRetry();
                return;
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
                ScheduleLoginRetry();
                return;
            }

            IsLoggedIn = true;
            _loginAttempts = 0;
            _nextLoginRetry = null;
            _log.Info($"Logged in succesfully");
            LoggedInEvent?.Invoke(loginResponse);
        }
        
        private void ScheduleLoginRetry()
        {
            var retryDelay = Mathf.Clamp(Mathf.Pow(2f, _loginAttempts), 2f, 128f);
            _nextLoginRetry = Time.realtimeSinceStartup + retryDelay;
        }
    }
}