using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InvestOptionPanelUI : MonoBehaviour
{
    [SerializeField] private FinanceSelectManager manager;
    [SerializeField] private InvestOptionButton[] optionButtons = Array.Empty<InvestOptionButton>();

    private void Awake()
    {
        InitializePanel();
    }

    private void OnEnable()
    {
        InitializePanel();
    }

    public void OnOptionClicked(InvestOptionButton optionButton)
    {
        if (manager == null || optionButton == null)
        {
            return;
        }

        FinanceSelectionResult result = new FinanceSelectionResult
        {
            choiceType = FinanceChoiceType.Invest,
            optionId = optionButton.OptionId,
            optionName = optionButton.OptionName,
            description = optionButton.Description
        };

        manager.OnInvestOptionSelected(result);
    }

    private void InitializePanel()
    {
        if (manager == null)
        {
            manager = GetComponentInParent<FinanceSelectManager>();
        }

        if (manager == null)
        {
            manager = UnityEngine.Object.FindFirstObjectByType<FinanceSelectManager>();
        }

        AutoBindOptionButtons();

        if (optionButtons == null)
        {
            return;
        }

        for (int i = 0; i < optionButtons.Length; i++)
        {
            optionButtons[i]?.Initialize(this);
        }
    }

    private void AutoBindOptionButtons()
    {
        if (HasUsableOptionButtons())
        {
            return;
        }

        List<InvestOptionButton> discoveredButtons = new List<InvestOptionButton>();
        Button[] buttons = GetComponentsInChildren<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];
            if (button == null || !button.name.StartsWith("OptionButton", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            discoveredButtons.Add(InvestOptionButton.CreateRuntime(button));
        }

        if (discoveredButtons.Count > 0)
        {
            optionButtons = discoveredButtons.ToArray();
        }
    }

    private bool HasUsableOptionButtons()
    {
        if (optionButtons == null || optionButtons.Length == 0)
        {
            return false;
        }

        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (optionButtons[i] == null || !optionButtons[i].IsUsable)
            {
                return false;
            }
        }

        return true;
    }
}

[Serializable]
public class InvestOptionButton
{
    [SerializeField] private string optionId;
    [SerializeField] private string optionName;
    [SerializeField] [TextArea(2, 4)] private string description;
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;

    private InvestOptionPanelUI owner;

    public string OptionId => optionId;
    public string OptionName => optionName;
    public string Description => description;
    public bool IsUsable => button != null;

    public void Initialize(InvestOptionPanelUI panel)
    {
        owner = panel;
        EnsureRuntimeDefaults();

        if (nameText != null)
        {
            nameText.text = optionName;
        }

        if (descriptionText != null)
        {
            descriptionText.text = description;
        }

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(HandleClick);
        }
    }

    private void HandleClick()
    {
        owner?.OnOptionClicked(this);
    }

    public static InvestOptionButton CreateRuntime(Button sourceButton)
    {
        TMP_Text primaryText = FindPrimaryText(sourceButton);
        string resolvedName = ResolveOptionName(primaryText, sourceButton);

        return new InvestOptionButton
        {
            optionId = resolvedName,
            optionName = resolvedName,
            description = string.Empty,
            button = sourceButton,
            nameText = primaryText,
            descriptionText = null
        };
    }

    private void EnsureRuntimeDefaults()
    {
        if (button == null)
        {
            return;
        }

        if (nameText == null)
        {
            nameText = FindPrimaryText(button);
        }

        if (string.IsNullOrWhiteSpace(optionName))
        {
            optionName = ResolveOptionName(nameText, button);
        }

        if (string.IsNullOrWhiteSpace(optionId))
        {
            optionId = optionName;
        }
    }

    private static TMP_Text FindPrimaryText(Button sourceButton)
    {
        if (sourceButton == null)
        {
            return null;
        }

        TMP_Text[] texts = sourceButton.GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i] != null && texts[i].name.IndexOf("State", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return texts[i];
            }
        }

        return texts.Length > 0 ? texts[0] : null;
    }

    private static string ResolveOptionName(TMP_Text text, Button sourceButton)
    {
        if (text != null && !string.IsNullOrWhiteSpace(text.text))
        {
            return text.text.Trim();
        }

        return sourceButton != null ? sourceButton.name : "InvestOption";
    }
}
