using System;
using UnityEngine;

public class UserDataManager : MonoBehaviour
{
    public static UserDataManager Instance { get; private set; }

    public UserData CurrentUser { get; private set; }

    public event Action<UserData> OnUserDataChanged;

    private const string SaveKey = "USER_DATA";

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
        EnsureCurrentUser();
        NotifyChanged();
    }

    public void SetUser(string userId, string nickname, int coin)
    {
        EnsureCurrentUser();
        CurrentUser.userId = userId;
        CurrentUser.nickname = nickname;
        CurrentUser.coin = coin;
        NotifyChanged();
    }

    public bool IsStageUnlocked(StageInfo stageInfo)
    {
        if (stageInfo == null)
        {
            return false;
        }

        EnsureCurrentUser();

        if (stageInfo.DefaultUnlocked)
        {
            return true;
        }

        return !string.IsNullOrWhiteSpace(stageInfo.StageId) &&
            CurrentUser.unlockedStageIds.Contains(stageInfo.StageId);
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

        string json = JsonUtility.ToJson(CurrentUser);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    public void Load()
    {
        if (!PlayerPrefs.HasKey(SaveKey))
        {
            CurrentUser = CreateDefaultUser();
            return;
        }

        string json = PlayerPrefs.GetString(SaveKey);
        CurrentUser = JsonUtility.FromJson<UserData>(json);

        if (CurrentUser == null)
        {
            CurrentUser = CreateDefaultUser();
        }
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
        }

        if (CurrentUser.clearedStageIds == null)
        {
            CurrentUser.clearedStageIds = new System.Collections.Generic.List<string>();
        }

        if (CurrentUser.unlockedStageIds == null)
        {
            CurrentUser.unlockedStageIds = new System.Collections.Generic.List<string>();
        }
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
