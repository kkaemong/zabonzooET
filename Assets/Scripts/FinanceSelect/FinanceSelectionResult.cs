using System;

[Serializable]
public class FinanceSelectionResult
{
    public FinanceChoiceType choiceType;
    public string choiceCode;
    public string optionId;
    public string optionName;
    public string stageId;
    public string description;
    public int baseCoin;
    public int coinDelta;
    public int finalCoin;
    public string resultType;
    public string resultMessage;
    public string aiFeedback;
    public string nextEra;
    public bool finalClear;
    public bool isServerConfirmed;
}
