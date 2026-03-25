using System;
using System.Collections.Generic;
using UnityEngine;

public class UserDataManager : MonoBehaviour
{
    public static UserDataManager Instance { get; private set; }

    public UserData CurrentUser { get; private set; }

    public event Action<UserData> OnUserDataChanged;

    private const string SaveKey = "USER_DATA";
    private const string SaveKeyPrefix = "USER_DATA_";

    private string activeSaveKey = SaveKey;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Load();
    }

    public void SetUser(UserData userData)
    {
        CurrentUser = userData ?? CreateDefaultUser();
        NormalizeCurrentUser();
        activeSaveKey = BuildSaveKey(CurrentUser.userId);
        Save();
        NotifyChanged();
    }

    public void SetUser(string userId, string nickname, int coin)
    {
        string normalizedUserId = string.IsNullOrWhiteSpace(userId) ? "guest" : userId.Trim();
        string targetSaveKey = BuildSaveKey(normalizedUserId);

        if (activeSaveKey != targetSaveKey)
        {
            activeSaveKey = targetSaveKey;
            Load();
        }
        else
        {
            EnsureCurrentUser();
        }

        if (CurrentUser == null || !string.Equals(CurrentUser.userId, normalizedUserId, StringComparison.Ordinal))
        {
            CurrentUser = CreateDefaultUser();
        }

        CurrentUser.userId = normalizedUserId;
        CurrentUser.nickname = nickname;
        CurrentUser.coin = Mathf.Max(coin, 0);

        NormalizeCurrentUser();
        Save();
        NotifyChanged();
    }

    public void EnsureStageRegistered(StageInfo stageInfo)
    {
        if (stageInfo == null || string.IsNullOrWhiteSpace(stageInfo.StageId))
        {
            return;
        }

        EnsureCurrentUser();
        bool changed = false;

        if (stageInfo.DefaultUnlocked)
        {
            changed = UnlockStage(stageInfo.StageId, saveImmediately: false, notify: false);
        }

        NormalizeCurrentUser();

        if (changed)
        {
            Save();
        }
    }

    public bool IsStageUnlocked(string stageId)
    {
        EnsureCurrentUser();
        return !string.IsNullOrWhiteSpace(stageId) && CurrentUser.unlockedStageIds.Contains(stageId);
    }

    public bool IsStageCleared(string stageId)
    {
        EnsureCurrentUser();
        return !string.IsNullOrWhiteSpace(stageId) && CurrentUser.clearedStageIds.Contains(stageId);
    }

    public bool UnlockStage(string stageId)
    {
        return UnlockStage(stageId, saveImmediately: true, notify: true);
    }

    public void MarkStageCleared(string stageId)
    {
        EnsureCurrentUser();

        if (string.IsNullOrWhiteSpace(stageId))
        {
            return;
        }

        if (!CurrentUser.clearedStageIds.Contains(stageId))
        {
            CurrentUser.clearedStageIds.Add(stageId);
        }

        UnlockStage(stageId, saveImmediately: false, notify: false);
        Save();
        NotifyChanged();
    }

    public void AddCoin(int amount)
    {
        EnsureCurrentUser();
        CurrentUser.coin += amount;
        Save();
        NotifyChanged();
    }

    public bool SpendCoin(int amount)
    {
        EnsureCurrentUser();

        if (CurrentUser.coin < amount)
        {
            return false;
        }

        CurrentUser.coin -= amount;
        Save();
        NotifyChanged();
        return true;
    }

    public void Save()
    {
        EnsureCurrentUser();
        NormalizeCurrentUser();

        string json = JsonUtility.ToJson(CurrentUser);
        PlayerPrefs.SetString(activeSaveKey, json);
        PlayerPrefs.Save();
    }

    public void Load()
    {
        if (!PlayerPrefs.HasKey(activeSaveKey))
        {
            CurrentUser = CreateDefaultUser();
            NormalizeCurrentUser();
            return;
        }

        string json = PlayerPrefs.GetString(activeSaveKey);
        CurrentUser = JsonUtility.FromJson<UserData>(json);

        if (CurrentUser == null)
        {
            CurrentUser = CreateDefaultUser();
        }

        NormalizeCurrentUser();
    }

    private void NotifyChanged()
    {
        OnUserDataChanged?.Invoke(CurrentUser);
    }

    private void EnsureCurrentUser()
    {
        if (CurrentUser == null)
        {
            CurrentUser = CreateDefaultUser();
            NormalizeCurrentUser();
        }
    }

    private void NormalizeCurrentUser()
    {
        CurrentUser ??= CreateDefaultUser();
        CurrentUser.userId = string.IsNullOrWhiteSpace(CurrentUser.userId) ? "guest" : CurrentUser.userId.Trim();
        CurrentUser.nickname ??= "Guest";
        CurrentUser.clearedStageIds ??= new List<string>();
        CurrentUser.unlockedStageIds ??= new List<string>();
    }

    private bool UnlockStage(string stageId, bool saveImmediately, bool notify)
    {
        EnsureCurrentUser();

        if (string.IsNullOrWhiteSpace(stageId))
        {
            return false;
        }

        if (CurrentUser.unlockedStageIds.Contains(stageId))
        {
            return false;
        }

        CurrentUser.unlockedStageIds.Add(stageId);

        if (saveImmediately)
        {
            Save();
        }

        if (notify)
        {
            NotifyChanged();
        }

        return true;
    }

    private static string BuildSaveKey(string userId)
    {
        return string.IsNullOrWhiteSpace(userId) || userId == "guest"
            ? SaveKey
            : SaveKeyPrefix + userId;
    }

    private UserData CreateDefaultUser()
    {
        return new UserData
        {
            userId = "guest",
            nickname = "Guest",
            coin = 0
        };
    }
}
