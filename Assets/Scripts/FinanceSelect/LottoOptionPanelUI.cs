using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LottoOptionPanelUI : MonoBehaviour
{
    [SerializeField] private FinanceSelectManager manager;
    [SerializeField] private RouletteController rouletteController;
    [SerializeField] private Button spinButton;
    [SerializeField] private GameObject statusTextRoot;
    [SerializeField] private TMP_Text resultMessageText;
    [SerializeField] private float resultDisplayDuration = 1.5f;
    [SerializeField] private LottoOptionData[] options;

    private FinanceSelectionResult pendingResult;

    private void Awake()
    {
        if (manager == null)
        {
            manager = GetComponentInParent<FinanceSelectManager>();
        }

        if (manager == null)
        {
            manager = UnityEngine.Object.FindFirstObjectByType<FinanceSelectManager>();
        }

        if (rouletteController == null)
        {
            Debug.LogWarning($"LottoOptionPanelUI.Awake: rouletteController is not assigned on '{name}'.", this);
        }

        if (spinButton == null)
        {
            Debug.LogWarning($"LottoOptionPanelUI.Awake: spinButton is not assigned on '{name}'.", this);
        }

        if (options == null || options.Length == 0)
        {
            Debug.LogWarning($"LottoOptionPanelUI.Awake: no lotto options configured on '{name}'.", this);
        }

        if (spinButton != null)
        {
            spinButton.onClick.RemoveAllListeners();
            spinButton.onClick.AddListener(OnClickSpinButton);
        }
    }

    private void OnEnable()
    {
        pendingResult = null;
        SetOptionsVisible(true);
        UpdateResultMessage("룰렛을 돌려 보세요.");

        if (rouletteController != null && options != null && options.Length > 0)
        {
            rouletteController.SetSliceCount(options.Length);
        }

        if (spinButton != null)
        {
            spinButton.interactable = true;
        }
    }

    public void OnClickSpinButton()
    {
        if (rouletteController == null)
        {
            Debug.LogWarning($"LottoOptionPanelUI.OnClickSpinButton: rouletteController is missing on '{name}'.", this);
            return;
        }

        if (rouletteController.IsSpinning)
        {
            Debug.LogWarning($"LottoOptionPanelUI.OnClickSpinButton: roulette is already spinning on '{name}'.", this);
            return;
        }

        if (options == null || options.Length == 0)
        {
            Debug.LogWarning($"LottoOptionPanelUI.OnClickSpinButton: no lotto options configured on '{name}'.", this);
            return;
        }

        int targetIndex = UnityEngine.Random.Range(0, options.Length);
        LottoOptionData targetOption = options[targetIndex];
        Debug.Log($"LottoOptionPanelUI.OnClickSpinButton: selected targetIndex={targetIndex}, option='{targetOption.optionName}'.", this);

        pendingResult = new FinanceSelectionResult
        {
            choiceType = FinanceChoiceType.Lotto,
            optionId = targetOption.optionId,
            optionName = targetOption.optionName,
            description = targetOption.description,
            coinDelta = targetOption.coinDelta,
            resultMessage = targetOption.resultMessage
        };

        if (spinButton != null)
        {
            spinButton.interactable = false;
        }

        SetOptionsVisible(false);
        UpdateResultMessage("룰렛 결과를 확인하는 중...");
        rouletteController.Play(targetIndex, OnSpinComplete);
    }

    private void OnSpinComplete()
    {
        if (spinButton != null)
        {
            spinButton.interactable = true;
        }

        if (pendingResult != null)
        {
            Debug.Log($"LottoOptionPanelUI.OnSpinComplete: result='{pendingResult.optionName}', coinDelta={pendingResult.coinDelta}.", this);
            UpdateResultMessage(BuildResultMessage(pendingResult));
            StartCoroutine(NotifyResultAfterDelay());
        }
    }

    private IEnumerator NotifyResultAfterDelay()
    {
        yield return new WaitForSeconds(resultDisplayDuration);

        if (pendingResult != null)
        {
            manager?.OnLottoOptionSelected(pendingResult);
        }
    }

    private void SetOptionsVisible(bool visible)
    {
        if (statusTextRoot != null)
        {
            statusTextRoot.SetActive(visible);
        }
    }

    private void UpdateResultMessage(string message)
    {
        if (resultMessageText != null)
        {
            resultMessageText.text = message;
        }
    }

    private string BuildResultMessage(FinanceSelectionResult result)
    {
        string deltaText = result.coinDelta >= 0
            ? $"+{result.coinDelta}코인"
            : $"{result.coinDelta}코인";

        if (!string.IsNullOrWhiteSpace(result.resultMessage))
        {
            return $"{result.resultMessage}\n{deltaText}";
        }

        return $"{result.optionName}\n{deltaText}";
    }
}

[Serializable]
public class LottoOptionData
{
    public string optionId;
    public string optionName;
    [TextArea(2, 4)] public string description;
    public int coinDelta;
    [TextArea(2, 4)] public string resultMessage;
}
