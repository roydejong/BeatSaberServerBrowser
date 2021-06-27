using ServerBrowser.Game.Models;

namespace ServerBrowser.Presence
{
    public interface IPresenceProvider
    {
        /// <summary>
        /// Gets whether this presence provider can be used.
        /// </summary>
        public bool GetIsAvailable();
        
        /// <summary>
        /// Starts the presence provider.
        /// </summary>
        public void Start();
        
        /// <summary>
        /// Stops the presence provider.
        /// </summary>
        public void Stop();
        
        /// <summary>
        /// Update the presence provider, given the current multiplayer game state.
        /// </summary>
        public void Update(MultiplayerActivity? activity);
    }
}