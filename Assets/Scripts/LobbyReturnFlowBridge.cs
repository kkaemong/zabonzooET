using UnityEngine;
using UnityEngine.SceneManagement;

public static class LobbyReturnFlowBridge
{
    private const string PreferredLobbySceneName = "Lobby";

    private static PendingLobbyReturn pendingReturn;

    public static void ReturnToLobbyMain()
    {
        UserData userData = UserDataManager.Instance != null ? UserDataManager.Instance.CurrentUser : null;

        pendingReturn = new PendingLobbyReturn
        {
            baseUrl = string.IsNullOrWhiteSpace(BackendRuntimeSession.BaseUrl)
                ? LobbyAuthApi.DefaultBaseUrl
                : BackendRuntimeSession.BaseUrl,
            userId = BackendRuntimeSession.UserId > 0
                ? BackendRuntimeSession.UserId
                : (userData != null ? userData.backendUserId : -1),
            sessionCookie = BackendRuntimeSession.SessionCookie,
            loginId = !string.IsNullOrWhiteSpace(BackendRuntimeSession.LoginId)
                ? BackendRuntimeSession.LoginId
                : (userData != null ? userData.userId : string.Empty),
            nickname = !string.IsNullOrWhiteSpace(BackendRuntimeSession.Nickname)
                ? BackendRuntimeSession.Nickname
                : (userData != null ? userData.nickname : string.Empty),
            coin = userData != null ? Mathf.Max(userData.coin, 0) : -1,
        };

        if (Application.CanStreamedLevelBeLoaded(PreferredLobbySceneName))
        {
            SceneManager.LoadScene(PreferredLobbySceneName);
            return;
        }

        Debug.LogWarning("LobbyReturnFlowBridge: Lobby scene is not in build settings.");
    }

    public static bool TryConsumePendingReturn(
        out string baseUrl,
        out long userId,
        out string sessionCookie,
        out string loginId,
        out string nickname,
        out int coin)
    {
        if (pendingReturn == null)
        {
            baseUrl = string.Empty;
            userId = -1;
            sessionCookie = string.Empty;
            loginId = string.Empty;
            nickname = string.Empty;
            coin = -1;
            return false;
        }

        baseUrl = pendingReturn.baseUrl;
        userId = pendingReturn.userId;
        sessionCookie = pendingReturn.sessionCookie;
        loginId = pendingReturn.loginId;
        nickname = pendingReturn.nickname;
        coin = pendingReturn.coin;
        pendingReturn = null;
        return true;
    }

    private sealed class PendingLobbyReturn
    {
        public string baseUrl;
        public long userId;
        public string sessionCookie;
        public string loginId;
        public string nickname;
        public int coin;
    }
}
