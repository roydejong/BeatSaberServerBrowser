using System;

namespace ServerBrowser.Core
{
    /// <summary>
    /// Tracks the state of announces.
    /// </summary>
    public class AnnounceState
    {
        public string ServerCode = null;
        public string OwnerId = null;
        public string HostSecret = null;

        public bool IsPending = false;
        public bool DidAnnounce = false;
        public DateTime? LastSuccess = null;
        public bool DidFail = false;
        public DateTime? LastFailure = null;
    }
}