public static class BackendRuntimeSession
{
    public static string BaseUrl { get; private set; }
    public static long UserId { get; private set; } = -1;
    public static string SessionCookie { get; private set; }
    public static string LoginId { get; private set; }
    public static string Nickname { get; private set; }

    public static void Configure(string baseUrl, long userId, string sessionCookie, string loginId, string nickname)
    {
        BaseUrl = BackendUrlResolver.Resolve(baseUrl);
        UserId = userId;
        SessionCookie = string.IsNullOrWhiteSpace(sessionCookie) ? null : sessionCookie.Trim();
        LoginId = loginId ?? string.Empty;
        Nickname = nickname ?? string.Empty;
    }

    public static void Clear()
    {
        BaseUrl = null;
        UserId = -1;
        SessionCookie = null;
        LoginId = string.Empty;
        Nickname = string.Empty;
    }
}
