using System;

public static class FinanceFlowContext
{
    public static string StageCode { get; private set; } = "ERA_1980";
    public static int CollectedCoin { get; private set; }
    public static long RunId { get; private set; } = -1;
    public static bool HasPendingGameClear { get; private set; }
    public static bool IsRunResultPending { get; private set; }
    public static bool HasRunResultResponse { get; private set; }
    public static string RunResultError { get; private set; } = string.Empty;
    public static int RunRewardCoin { get; private set; }
    public static int RunRemainingTotalCoin { get; private set; }
    public static string RunCurrentEra { get; private set; } = string.Empty;
    public static string RunNextStep { get; private set; } = string.Empty;
    public static bool RunFinanceEventAvailable { get; private set; }
    public static string RunNextEra { get; private set; } = string.Empty;

    public static void Capture(string stageCode, int collectedCoin, long runId)
    {
        if (RunId != runId)
        {
            ResetRunResultState();
        }

        StageCode = string.IsNullOrWhiteSpace(stageCode) ? "ERA_1980" : stageCode.Trim();
        CollectedCoin = Math.Max(collectedCoin, 0);
        RunId = runId;
        HasPendingGameClear = true;
    }

    public static void BeginRunResultSubmission(long runId)
    {
        if (runId <= 0)
        {
            return;
        }

        if (RunId != runId)
        {
            ResetRunResultState();
        }

        RunId = runId;
        IsRunResultPending = true;
        HasRunResultResponse = false;
        RunResultError = string.Empty;
    }

    public static void ApplyRunResult(APIManager.RunResultResponse response)
    {
        if (response == null)
        {
            return;
        }

        RunId = response.runId > 0 ? response.runId : RunId;
        RunRewardCoin = response.rewardCoin;
        RunRemainingTotalCoin = response.remainingTotalCoin;
        RunCurrentEra = response.currentEra ?? string.Empty;
        RunNextStep = response.nextStep ?? string.Empty;
        RunFinanceEventAvailable = response.financeEventAvailable;
        RunNextEra = response.nextEra ?? string.Empty;
        IsRunResultPending = false;
        HasRunResultResponse = true;
        RunResultError = string.Empty;
    }

    public static void SetRunResultError(string errorMessage)
    {
        IsRunResultPending = false;
        HasRunResultResponse = false;
        RunResultError = errorMessage ?? string.Empty;
    }

    public static void Clear()
    {
        StageCode = "ERA_1980";
        CollectedCoin = 0;
        RunId = -1;
        HasPendingGameClear = false;
        ResetRunResultState();
    }

    private static void ResetRunResultState()
    {
        IsRunResultPending = false;
        HasRunResultResponse = false;
        RunResultError = string.Empty;
        RunRewardCoin = 0;
        RunRemainingTotalCoin = 0;
        RunCurrentEra = string.Empty;
        RunNextStep = string.Empty;
        RunFinanceEventAvailable = false;
        RunNextEra = string.Empty;
    }
}
