using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class StageSelectManager : MonoBehaviour
{
    [Header("Cards")]
    [SerializeField] private List<StageCardUI> stageCards = new();
    [SerializeField] private int defaultSelectedIndex;

    [Header("Options")]
    [SerializeField] private bool wrapSelection = true;

    private int currentIndex = -1;

    private void Awake()
    {
        EnsureUserDataManager();
        InitializeCards();
    }

    private void OnEnable()
    {
        if (stageCards.Count == 0)
        {
            return;
        }

        SelectCard(GetInitialSelectionIndex());
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null || stageCards.Count == 0)
        {
            return;
        }

        if (keyboard.leftArrowKey.wasPressedThisFrame)
        {
            MoveSelection(-1);
        }

        if (keyboard.rightArrowKey.wasPressedThisFrame)
        {
            MoveSelection(1);
        }

        if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)
        {
            StartSelectedStage();
        }
    }

    public void RequestSelect(StageCardUI card)
    {
        SelectCard(stageCards.IndexOf(card));
    }

    public bool IsSelected(StageCardUI card)
    {
        return stageCards.IndexOf(card) == currentIndex;
    }

    public void RequestStart(StageCardUI card)
    {
        int index = stageCards.IndexOf(card);
        SelectCard(index);
        StartSelectedStage();
    }

    public void StartSelectedStage()
    {
        if (!IsValidIndex(currentIndex))
        {
            return;
        }

        StageCardUI selectedCard = stageCards[currentIndex];
        if (selectedCard == null)
        {
            return;
        }

        StageInfo stageInfo = selectedCard.StageInfo;
        if (stageInfo == null)
        {
            Debug.LogWarning($"StageSelectManager: StageInfo is missing for card index {currentIndex}.", this);
            return;
        }

        if (!IsStageUnlocked(stageInfo))
        {
            Debug.LogWarning($"StageSelectManager: Stage '{stageInfo.StageId}' is locked.", this);
            return;
        }

        string targetSceneName = selectedCard.SceneName;
        if (string.IsNullOrWhiteSpace(targetSceneName))
        {
            Debug.LogWarning($"StageSelectManager: Scene name is missing for stage '{stageInfo.StageId}'.", this);
            return;
        }

        SceneManager.LoadScene(targetSceneName);
    }

    private void InitializeCards()
    {
        bool hasAnyUnlockedStage = false;

        for (int i = 0; i < stageCards.Count; i++)
        {
            StageCardUI card = stageCards[i];
            if (card == null)
            {
                Debug.LogWarning($"StageSelectManager.InitializeCards: stageCards[{i}] is null.", this);
                continue;
            }

            if (card.StageInfo != null)
            {
                UserDataManager.Instance.EnsureStageRegistered(card.StageInfo);
                if (card.StageInfo.DefaultUnlocked && !UserDataManager.Instance.IsStageUnlocked(card.StageInfo.StageId))
                {
                    UserDataManager.Instance.UnlockStage(card.StageInfo.StageId);
                }

                hasAnyUnlockedStage |= UserDataManager.Instance.IsStageUnlocked(card.StageInfo.StageId);
            }

            card.Initialize(this);
            card.RefreshContent();
        }

        if (!hasAnyUnlockedStage)
        {
            for (int i = 0; i < stageCards.Count; i++)
            {
                StageCardUI card = stageCards[i];
                if (card?.StageInfo == null)
                {
                    continue;
                }

                UserDataManager.Instance.UnlockStage(card.StageInfo.StageId);
                card.RefreshContent();
            }
        }
    }

    private void MoveSelection(int direction)
    {
        if (stageCards.Count == 0)
        {
            return;
        }

        int nextIndex = currentIndex < 0
            ? GetInitialSelectionIndex()
            : currentIndex + direction;

        if (wrapSelection)
        {
            if (nextIndex < 0)
            {
                nextIndex = stageCards.Count - 1;
            }
            else if (nextIndex >= stageCards.Count)
            {
                nextIndex = 0;
            }
        }
        else
        {
            nextIndex = Mathf.Clamp(nextIndex, 0, stageCards.Count - 1);
        }

        SelectCard(nextIndex);
    }

    private void SelectCard(int index)
    {
        if (!IsValidIndex(index))
        {
            return;
        }

        currentIndex = index;

        for (int i = 0; i < stageCards.Count; i++)
        {
            StageCardUI card = stageCards[i];
            if (card == null)
            {
                continue;
            }

            card.SetSelected(i == currentIndex);
            card.RefreshContent();
        }

        GameObject selectedTarget = stageCards[currentIndex].SelectableTarget;
        EventSystem.current?.SetSelectedGameObject(selectedTarget);
    }

    private int GetInitialSelectionIndex()
    {
        if (IsValidIndex(defaultSelectedIndex))
        {
            return defaultSelectedIndex;
        }

        for (int i = 0; i < stageCards.Count; i++)
        {
            StageCardUI card = stageCards[i];
            if (card != null && card.StageInfo != null && IsStageUnlocked(card.StageInfo))
            {
                return i;
            }
        }

        return Mathf.Clamp(defaultSelectedIndex, 0, Mathf.Max(0, stageCards.Count - 1));
    }

    private void EnsureUserDataManager()
    {
        if (UserDataManager.Instance != null)
        {
            return;
        }

        GameObject managerObject = new GameObject("UserDataManager");
        managerObject.AddComponent<UserDataManager>();
    }

    private bool IsValidIndex(int index)
    {
        return index >= 0 && index < stageCards.Count;
    }

    private static bool IsStageUnlocked(StageInfo stageInfo)
    {
        if (stageInfo == null)
        {
            return false;
        }

        return stageInfo.DefaultUnlocked
            || (UserDataManager.Instance != null && UserDataManager.Instance.IsStageUnlocked(stageInfo.StageId));
    }
}
