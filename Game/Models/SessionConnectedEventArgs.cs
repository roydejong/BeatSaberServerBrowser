namespace ServerBrowser.Game.Models
{
    public struct SessionConnectedEventArgs
    {
        public IConnectedPlayer ConnectionOwner;
        public IConnectedPlayer LocalPlayer;
        public int MaxPlayers;
    }
}