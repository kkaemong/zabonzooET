using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FinanceResultPanelUI : MonoBehaviour
{
    [SerializeField] private FinanceSelectManager manager;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text coinDeltaText;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private string nextSceneName = "StageSelect";

    private void Awake()
    {
        ResolveReferences();
        BindButtons();
    }

    public void Initialize(FinanceSelectManager owner)
    {
        manager = owner;
        ResolveReferences();
        BindButtons();
    }

    public void Show(FinanceSelectionResult result)
    {
        if (result == null)
        {
            return;
        }

        ResolveReferences();
        BindButtons();

        if (titleText != null)
        {
            titleText.text = string.IsNullOrWhiteSpace(result.optionName)
                ? "\uACB0\uACFC"
                : result.optionName;
        }

        if (descriptionText != null)
        {
            descriptionText.text = !string.IsNullOrWhiteSpace(result.resultMessage)
                ? result.resultMessage
                : string.IsNullOrWhiteSpace(result.description)
                    ? "\uC120\uD0DD\uD55C \uAE08\uC735 \uACB0\uACFC\uAC00 \uBC18\uC601\uB429\uB2C8\uB2E4."
                    : result.description;
        }

        if (coinDeltaText != null)
        {
            coinDeltaText.text = result.coinDelta >= 0
                ? $"+{result.coinDelta} \uCF54\uC778"
                : $"{result.coinDelta} \uCF54\uC778";
        }

        gameObject.SetActive(true);
    }

    public void OnClickClose()
    {
        manager?.CloseResultPanel();
    }

    public void OnClickGoToNextScene()
    {
        if (manager != null)
        {
            manager.CompleteSelectionAndReturnToStageSelect();
            return;
        }

        string targetSceneName = ResolveNextSceneName();
        if (string.IsNullOrWhiteSpace(targetSceneName))
        {
            Debug.LogWarning($"FinanceResultPanelUI.OnClickGoToNextScene: nextSceneName is not set on '{name}'.", this);
            return;
        }

        SceneManager.LoadScene(targetSceneName);
    }

    private void ResolveReferences()
    {
        if (manager == null)
        {
            manager = UnityEngine.Object.FindFirstObjectByType<FinanceSelectManager>();
        }

        titleText ??= FindDeepChild(transform, "ResultTitleText")?.GetComponent<TMP_Text>();
        descriptionText ??= FindDeepChild(transform, "ResultDescriptionText")?.GetComponent<TMP_Text>();
        coinDeltaText ??= FindDeepChild(transform, "ResultCoinDeltaText")?.GetComponent<TMP_Text>();
        ResolveButtons();
    }

    private void ResolveButtons()
    {
        closeButton ??= FindDeepChild(transform, "CloseButton")?.GetComponent<Button>();
        nextButton ??= FindDeepChild(transform, "NextButton")?.GetComponent<Button>();

        if (closeButton != null && nextButton != null)
        {
            return;
        }

        Button[] buttons = GetComponentsInChildren<Button>(true);
        if (buttons.Length < 2)
        {
            return;
        }

        System.Array.Sort(buttons, (left, right) =>
        {
            RectTransform leftRect = left.transform as RectTransform;
            RectTransform rightRect = right.transform as RectTransform;
            float leftX = leftRect != null ? leftRect.anchoredPosition.x : 0f;
            float rightX = rightRect != null ? rightRect.anchoredPosition.x : 0f;
            return leftX.CompareTo(rightX);
        });

        closeButton ??= buttons[0];
        nextButton ??= buttons[buttons.Length - 1];
    }

    private void BindButtons()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(OnClickClose);
            closeButton.onClick.AddListener(OnClickClose);
        }

        if (nextButton != null)
        {
            nextButton.onClick.RemoveListener(OnClickGoToNextScene);
            nextButton.onClick.AddListener(OnClickGoToNextScene);
        }
    }

    private string ResolveNextSceneName()
    {
        if (!string.IsNullOrWhiteSpace(nextSceneName) && Application.CanStreamedLevelBeLoaded(nextSceneName))
        {
            return nextSceneName;
        }

        if (Application.CanStreamedLevelBeLoaded("StageSelect"))
        {
            return "StageSelect";
        }

        if (Application.CanStreamedLevelBeLoaded("StageSelectScene"))
        {
            return "StageSelectScene";
        }

        return string.Empty;
    }

    private static Transform FindDeepChild(Transform parent, string childName)
    {
        if (parent == null)
        {
            return null;
        }

        if (parent.name == childName)
        {
            return parent;
        }

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform found = FindDeepChild(parent.GetChild(i), childName);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }
}
