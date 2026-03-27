using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

public class StageSelectTopBar : MonoBehaviour
{
    [SerializeField] private string backSceneName = "Lobby";
    [SerializeField] private Button backButton;
    [SerializeField] private TMP_Text coinText;
    [SerializeField] private TMP_Text nicknameText;

    private void Awake()
    {
        if (string.IsNullOrWhiteSpace(backSceneName) ||
            string.Equals(backSceneName, "lobby", StringComparison.OrdinalIgnoreCase))
        {
            backSceneName = "Lobby";
        }

        if (backButton == null)
        {
            backButton = FindButton("BackButton");
        }

        EnsureBackButtonGraphic();

        if (coinText == null)
        {
            coinText = FindText("CoinText");
        }

        if (nicknameText == null)
        {
            nicknameText = FindText("NicknameText");
        }

        BindBackButton();
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

        if (string.Equals(targetSceneName, "Lobby", StringComparison.OrdinalIgnoreCase))
        {
            LobbyReturnFlowBridge.ReturnToLobbyMain();
            return;
        }

        SceneManager.LoadScene(targetSceneName);
    }

    private void BindBackButton()
    {
        if (backButton == null)
        {
            return;
        }

        backButton.onClick.RemoveListener(OnClickBack);

        if (HasPersistentListener(backButton, this, nameof(OnClickBack)))
        {
            return;
        }

        backButton.onClick.AddListener(OnClickBack);
    }

    private void EnsureBackButtonGraphic()
    {
        if (backButton == null)
        {
            return;
        }

        Image buttonImage = backButton.GetComponent<Image>();
        if (buttonImage == null)
        {
            return;
        }

        if (!buttonImage.enabled)
        {
            buttonImage.enabled = true;
        }

        Color color = buttonImage.color;
        if (color.a > 0f)
        {
            color.a = 0f;
            buttonImage.color = color;
        }

        buttonImage.raycastTarget = true;
        backButton.targetGraphic = buttonImage;
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

    private Button FindButton(string objectName)
    {
        foreach (Button button in GetComponentsInChildren<Button>(true))
        {
            if (button.name == objectName)
            {
                return button;
            }
        }

        return null;
    }

    private static bool HasPersistentListener(Button button, UnityEngine.Object target, string methodName)
    {
        if (button == null || target == null || string.IsNullOrWhiteSpace(methodName))
        {
            return false;
        }

        for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
        {
            if (button.onClick.GetPersistentTarget(i) == target &&
                string.Equals(button.onClick.GetPersistentMethodName(i), methodName, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
