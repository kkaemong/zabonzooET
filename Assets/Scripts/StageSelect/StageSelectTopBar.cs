using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class StageSelectTopBar : MonoBehaviour
{
    [SerializeField] private string backSceneName = "Lobby";
    [SerializeField] private TMP_Text coinText;
    [SerializeField] private TMP_Text nicknameText;

    private void Awake()
    {
        if (coinText == null)
        {
            coinText = FindText("CoinText");
        }

        if (nicknameText == null)
        {
            nicknameText = FindText("NicknameText");
        }
    }

    private void OnEnable()
    {
        RefreshUserInfo();

        if (UserDataManager.Instance != null)
        {
            UserDataManager.Instance.OnUserDataChanged += HandleUserDataChanged;
        }
    }

    private void OnDisable()
    {
        if (UserDataManager.Instance != null)
        {
            UserDataManager.Instance.OnUserDataChanged -= HandleUserDataChanged;
        }
    }

    public void OnClickBack()
    {
        string targetSceneName = string.IsNullOrWhiteSpace(backSceneName) ? "Lobby" : backSceneName;

        if (!Application.CanStreamedLevelBeLoaded(targetSceneName) &&
            string.Equals(targetSceneName, "lobby", StringComparison.OrdinalIgnoreCase))
        {
            targetSceneName = "Lobby";
        }

        if (string.IsNullOrWhiteSpace(targetSceneName))
        {
            Debug.LogWarning($"StageSelectTopBar.OnClickBack: Back scene name is not set on '{name}'.", this);
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(targetSceneName))
        {
            Debug.LogWarning($"StageSelectTopBar.OnClickBack: Scene '{targetSceneName}' is not in build settings.", this);
            return;
        }

        SceneManager.LoadScene(targetSceneName);
    }

    private void HandleUserDataChanged(UserData userData)
    {
        RefreshUserInfo();
    }

    private void RefreshUserInfo()
    {
        UserData userData = UserDataManager.Instance != null ? UserDataManager.Instance.CurrentUser : null;

        if (coinText != null)
        {
            coinText.text = userData != null ? userData.coin.ToString() : "0";
        }

        if (nicknameText != null)
        {
            nicknameText.text = userData != null ? userData.nickname : string.Empty;
        }
    }

    private TMP_Text FindText(string objectName)
    {
        foreach (TMP_Text text in GetComponentsInChildren<TMP_Text>(true))
        {
            if (text.name == objectName)
            {
                return text;
            }
        }

        return null;
    }
}
