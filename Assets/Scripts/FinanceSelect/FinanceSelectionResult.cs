using System;

[Serializable]
public class FinanceSelectionResult
{
    public FinanceChoiceType choiceType;
    public string optionId;
    public string optionName;
    public string description;
    public int coinDelta;
    public string resultMessage;
}
