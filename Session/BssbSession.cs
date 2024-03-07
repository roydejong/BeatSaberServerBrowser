using System;
using System.Threading;
using System.Threading.Tasks;
using ServerBrowser.Data;
using SiraUtil.Logging;
using Zenject;

namespace ServerBrowser.Session
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class BssbSession : IInitializable, IDisposable
    {
        [Inject] private readonly SiraLog _log = null!;
        [Inject] private readonly BssbApi _api = null!;

        [Inject] private readonly IPlatformUserModel _platformUserModel;

        public UserInfo? UserInfo { get; private set; }

        private readonly CancellationTokenSource _ctsUserInfo = new();
        
        public event Action<UserInfo>? UserInfoChangedEvent;

        public void Initialize()
        {
            _ = LoadUserInfo();
        }

        private async Task LoadUserInfo()
        {
            try
            {
                UserInfo = await _platformUserModel.GetUserInfo(_ctsUserInfo.Token);
                _log.Info($"Loaded user info (platform={UserInfo.platform}, platformUserId={UserInfo.platformUserId}, userName={UserInfo.userName})");
                UserInfoChangedEvent?.Invoke(UserInfo);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                _log.Error($"Error loading user info: {e}");
            }
        }

        public void Dispose()
        {
            _ctsUserInfo.Cancel();
            _ctsUserInfo.Dispose();
        }
    }
}