using UnityEngine;
using UnityEngine.SceneManagement;

public static class LobbyStageFlowBridge
{
    private const string PreferredStageSelectSceneName = "StageSelect";
    private const string LegacyStageSelectSceneName = "StageSelectScene";

    public static void EnterStageSelect(string loginId, string nickname, int coin, long backendUserId = -1)
    {
        EnsureUserDataManager(loginId, nickname, coin, backendUserId);

        if (Application.CanStreamedLevelBeLoaded(PreferredStageSelectSceneName))
        {
            SceneManager.LoadScene(PreferredStageSelectSceneName);
            return;
        }

        if (Application.CanStreamedLevelBeLoaded(LegacyStageSelectSceneName))
        {
            SceneManager.LoadScene(LegacyStageSelectSceneName);
            return;
        }

        Debug.LogWarning(
            $"Stage Select scene is not integrated yet. " +
            $"Requested by loginId='{loginId}', nickname='{nickname}', coin={coin}.");
    }

    private static void EnsureUserDataManager(string loginId, string nickname, int coin, long backendUserId)
    {
        if (UserDataManager.Instance == null)
        {
            GameObject managerObject = new GameObject("UserDataManager");
            managerObject.AddComponent<UserDataManager>();
        }

        UserDataManager.Instance.SetUser(loginId, nickname, coin, backendUserId);
    }
}
