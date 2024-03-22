namespace ServerBrowser.Requests
{
    public class BssbLoginRequest
    {
        public UserInfo? UserInfo { get; set; }
        public string? AuthenticationToken { get; set; }
    }
}