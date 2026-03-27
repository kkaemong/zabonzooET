using System;

public static class FinanceFlowContext
{
    public static string StageCode { get; private set; } = "ERA_1980";
    public static int CollectedCoin { get; private set; }
    public static long RunId { get; private set; } = -1;
    public static bool HasPendingGameClear { get; private set; }

    public static void Capture(string stageCode, int collectedCoin, long runId)
    {
        StageCode = string.IsNullOrWhiteSpace(stageCode) ? "ERA_1980" : stageCode.Trim();
        CollectedCoin = Math.Max(collectedCoin, 0);
        RunId = runId;
        HasPendingGameClear = true;
    }

    public static void Clear()
    {
        StageCode = "ERA_1980";
        CollectedCoin = 0;
        RunId = -1;
        HasPendingGameClear = false;
    }
}
