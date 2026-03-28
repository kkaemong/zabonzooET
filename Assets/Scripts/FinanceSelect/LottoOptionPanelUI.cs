using System;
using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private LottoOptionData[] options = Array.Empty<LottoOptionData>();

    private FinanceSelectionResult pendingResult;
    private LottoOptionData[] runtimeOptions = Array.Empty<LottoOptionData>();

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

        LottoOptionData[] activeOptions = ResolveOptions();
        if (rouletteController != null && activeOptions.Length > 0)
        {
            rouletteController.SetSliceCount(activeOptions.Length);
        }

        if (spinButton != null)
        {
            spinButton.interactable = activeOptions.Length > 0;
        }
    }

    public void Configure(FinanceSelectManager owner, APIManager.FinanceOptionResponse option)
    {
        manager = owner;
        runtimeOptions = BuildRuntimeOptions(option);

        if (rouletteController != null && runtimeOptions.Length > 0)
        {
            rouletteController.SetSliceCount(runtimeOptions.Length);
        }
    }

    public void OnClickSpinButton()
    {
        if (rouletteController == null || rouletteController.IsSpinning)
        {
            return;
        }

        LottoOptionData[] activeOptions = ResolveOptions();
        if (activeOptions.Length == 0)
        {
            Debug.LogWarning($"LottoOptionPanelUI.OnClickSpinButton: no lotto options configured on '{name}'.", this);
            return;
        }

        int targetIndex = UnityEngine.Random.Range(0, activeOptions.Length);
        LottoOptionData targetOption = activeOptions[targetIndex];
        pendingResult = new FinanceSelectionResult
        {
            choiceType = FinanceChoiceType.Lotto,
            choiceCode = "LOTTO",
            optionId = targetOption.optionId,
            optionName = targetOption.optionName,
            description = targetOption.description,
            resultMessage = string.IsNullOrWhiteSpace(targetOption.resultMessage)
                ? targetOption.description
                : targetOption.resultMessage,
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

        if (pendingResult == null)
        {
            return;
        }

        UpdateResultMessage(BuildResultMessage(pendingResult));
        StartCoroutine(NotifyResultAfterDelay());
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

    private LottoOptionData[] ResolveOptions()
    {
        return runtimeOptions != null && runtimeOptions.Length > 0
            ? runtimeOptions
            : options ?? Array.Empty<LottoOptionData>();
    }

    private static LottoOptionData[] BuildRuntimeOptions(APIManager.FinanceOptionResponse option)
    {
        APIManager.FinanceSubOptionResponse[] subOptions = option != null && option.subOptions != null
            ? option.subOptions
            : Array.Empty<APIManager.FinanceSubOptionResponse>();

        List<LottoOptionData> results = new List<LottoOptionData>();
        for (int i = 0; i < subOptions.Length; i++)
        {
            APIManager.FinanceSubOptionResponse subOption = subOptions[i];
            if (subOption == null)
            {
                continue;
            }

            results.Add(new LottoOptionData
            {
                optionId = subOption.code,
                optionName = subOption.name,
                description = subOption.description,
                resultMessage = subOption.description,
                coinDelta = 0,
            });
        }

        return results.ToArray();
    }

    private string BuildResultMessage(FinanceSelectionResult result)
    {
        if (!string.IsNullOrWhiteSpace(result.resultMessage))
        {
            return result.resultMessage;
        }

        return result.optionName;
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
