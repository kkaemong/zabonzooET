using System.Text;
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

    private FinanceSelectionResult currentResult;

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

        currentResult = result;
        ResolveReferences();
        BindButtons();

        if (titleText != null)
        {
            titleText.text = BuildTitle(result);
        }

        if (descriptionText != null)
        {
            descriptionText.text = BuildDescription(result);
        }

        if (coinDeltaText != null)
        {
            coinDeltaText.text = BuildCoinText(result);
        }

        if (closeButton != null)
        {
            closeButton.interactable = !result.isServerConfirmed;
        }

        gameObject.SetActive(true);
    }

    public void OnClickClose()
    {
        if (currentResult != null && currentResult.isServerConfirmed)
        {
            return;
        }

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

    private static string BuildTitle(FinanceSelectionResult result)
    {
        if (result == null)
        {
            return "금융 리포트";
        }

        return string.IsNullOrWhiteSpace(result.optionName)
            ? "금융 리포트"
            : result.optionName.Trim();
    }

    private static string BuildDescription(FinanceSelectionResult result)
    {
        if (result == null)
        {
            return "금융 리포트를 불러오지 못했습니다.";
        }

        StringBuilder builder = new StringBuilder();

        AppendSection(
            builder,
            !string.IsNullOrWhiteSpace(result.resultMessage)
                ? result.resultMessage
                : result.description);

        AppendSection(builder, result.aiFeedback);

        if (result.isServerConfirmed)
        {
            if (result.finalClear)
            {
                AppendSection(builder, "최종 시대까지 모두 클리어했습니다.");
            }
            else if (!string.IsNullOrWhiteSpace(result.nextEra))
            {
                AppendSection(builder, $"다음 시대: {FormatEra(result.nextEra)}");
            }
        }

        if (builder.Length == 0)
        {
            builder.Append("선택한 금융 결과가 반영됩니다.");
        }

        return builder.ToString();
    }

    private static string BuildCoinText(FinanceSelectionResult result)
    {
        if (result == null)
        {
            return string.Empty;
        }

        if (!result.isServerConfirmed && result.coinDelta == 0 && result.finalCoin <= 0)
        {
            return string.Empty;
        }

        string deltaText = result.coinDelta >= 0
            ? $"+{result.coinDelta:N0} 코인"
            : $"{result.coinDelta:N0} 코인";

        if (!result.isServerConfirmed)
        {
            return deltaText;
        }

        return $"{deltaText}\n보유 코인 {Mathf.Max(result.finalCoin, 0):N0}";
    }

    private static void AppendSection(StringBuilder builder, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        if (builder.Length > 0)
        {
            builder.Append("\n\n");
        }

        builder.Append(value.Trim());
    }

    private static string FormatEra(string eraCode)
    {
        switch ((eraCode ?? string.Empty).Trim().ToUpperInvariant())
        {
            case "ERA_1980":
                return "1980년대";

            case "ERA_2000":
                return "2000년대";

            case "ERA_2020":
                return "2020년대";

            default:
                return eraCode;
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
