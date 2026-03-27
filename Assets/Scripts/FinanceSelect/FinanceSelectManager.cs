using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class FinanceSelectManager : MonoBehaviour
{
    [Header("Cards")]
    [SerializeField] private List<FinanceChoiceCardUI> cards = new();
    [SerializeField] private int defaultSelectedIndex;

    [Header("Options")]
    [SerializeField] private bool wrapSelection = true;

    [Header("Panels")]
    [SerializeField] private GameObject dimmedPanel;
    [SerializeField] private GameObject savingOptionPanel;
    [SerializeField] private GameObject investOptionPanel;
    [SerializeField] private GameObject lottoOptionPanel;
    [SerializeField] private GameObject resultPanel;

    private FinanceResultPanelUI resultPanelUI;
    private FinanceSelectionResult pendingResult;

    private int currentIndex = -1;
    private bool isOptionPanelOpen;
    private bool isReturningToStageSelect;

    private void Awake()
    {
        AutoBindPanels();
        EnsurePanelComponents();
        InitializeResultPanel();
        InitializeCards();
        CloseOptionPanels();
        CloseResultPanel();
    }

    private void OnEnable()
    {
        if (cards.Count == 0)
        {
            return;
        }

        SelectCard(GetInitialSelectionIndex());
    }

    private void Update()
    {
        if (cards.Count == 0 || isOptionPanelOpen || (resultPanel != null && resultPanel.activeSelf))
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            MoveSelection(-1);
        }

        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            MoveSelection(1);
        }

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            ConfirmSelectedCard();
        }
    }

    public void RequestSelect(FinanceChoiceCardUI card)
    {
        SelectCard(cards.IndexOf(card));
    }

    public bool IsSelected(FinanceChoiceCardUI card)
    {
        return cards.IndexOf(card) == currentIndex;
    }

    public void RequestConfirm(FinanceChoiceCardUI card)
    {
        int index = cards.IndexOf(card);
        SelectCard(index);
        ConfirmSelectedCard();
    }

    public void SetCardsInteractable(bool value)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i] != null)
            {
                cards[i].SetInteractable(value);
            }
        }
    }

    public void CloseOptionPanels()
    {
        isOptionPanelOpen = false;

        if (dimmedPanel != null)
        {
            dimmedPanel.SetActive(false);
        }

        if (savingOptionPanel != null)
        {
            savingOptionPanel.SetActive(false);
        }

        if (investOptionPanel != null)
        {
            investOptionPanel.SetActive(false);
        }

        if (lottoOptionPanel != null)
        {
            lottoOptionPanel.SetActive(false);
        }

        if (resultPanel == null || !resultPanel.activeSelf)
        {
            SetCardsInteractable(true);
        }
    }

    public void CloseResultPanel()
    {
        if (resultPanel == null)
        {
            AutoBindPanels();
        }

        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }

        SetCardsInteractable(true);
    }

    public void OnClickBackButton()
    {
        if (resultPanel != null && resultPanel.activeSelf)
        {
            CloseResultPanel();
            return;
        }

        CloseOptionPanels();
    }

    public void OnSavingOptionSelected(FinanceSelectionResult result)
    {
        if (result == null)
        {
            return;
        }

        Debug.Log($"Saving option selected: {result.optionId} / {result.optionName}", this);
        pendingResult = result;
        CloseOptionPanels();
        OpenResultPanel(result);
    }

    public void OnInvestOptionSelected(FinanceSelectionResult result)
    {
        if (result == null)
        {
            return;
        }

        Debug.Log($"Invest option selected: {result.optionId} / {result.optionName}", this);
        pendingResult = result;
        CloseOptionPanels();
        OpenResultPanel(result);
    }

    public void OnLottoOptionSelected(FinanceSelectionResult result)
    {
        if (result == null)
        {
            return;
        }

        Debug.Log($"Lotto option selected: {result.optionId} / {result.optionName}", this);
        pendingResult = result;
        CloseOptionPanels();
        OpenResultPanel(result);
    }

    public void CompleteSelectionAndReturnToStageSelect()
    {
        if (isReturningToStageSelect)
        {
            return;
        }

        isReturningToStageSelect = true;

        try
        {
            ApplySelectionOutcome();

            string targetSceneName = ResolveStageSelectSceneName();
            if (string.IsNullOrWhiteSpace(targetSceneName))
            {
                Debug.LogWarning("FinanceSelectManager.CompleteSelectionAndReturnToStageSelect: StageSelect scene is not in build settings.", this);
                return;
            }

            SceneManager.LoadScene(targetSceneName);
        }
        finally
        {
            isReturningToStageSelect = false;
        }
    }

    private void InitializeCards()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            FinanceChoiceCardUI card = cards[i];
            if (card == null)
            {
                Debug.LogWarning($"FinanceSelectManager.InitializeCards: cards[{i}] is null.", this);
                continue;
            }

            card.manager = this;
            card.SetSelected(false);
            card.SetInteractable(true);
        }
    }

    private void MoveSelection(int direction)
    {
        if (cards.Count == 0)
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
                nextIndex = cards.Count - 1;
            }
            else if (nextIndex >= cards.Count)
            {
                nextIndex = 0;
            }
        }
        else
        {
            nextIndex = Mathf.Clamp(nextIndex, 0, cards.Count - 1);
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

        for (int i = 0; i < cards.Count; i++)
        {
            FinanceChoiceCardUI card = cards[i];
            if (card == null)
            {
                continue;
            }

            card.SetSelected(i == currentIndex);
        }

        GameObject selectedTarget = cards[currentIndex].SelectableTarget;
        EventSystem.current?.SetSelectedGameObject(selectedTarget);
    }

    private void ConfirmSelectedCard()
    {
        if (!IsValidIndex(currentIndex) || isOptionPanelOpen)
        {
            return;
        }

        FinanceChoiceCardUI selectedCard = cards[currentIndex];
        if (selectedCard == null)
        {
            return;
        }

        OpenOptionPanel(selectedCard.choiceType);
    }

    private void OpenOptionPanel(FinanceChoiceType choiceType)
    {
        isOptionPanelOpen = true;
        SetCardsInteractable(false);

        if (dimmedPanel != null)
        {
            dimmedPanel.SetActive(true);
        }

        if (savingOptionPanel != null)
        {
            savingOptionPanel.SetActive(false);
        }

        if (investOptionPanel != null)
        {
            investOptionPanel.SetActive(false);
        }

        if (lottoOptionPanel != null)
        {
            lottoOptionPanel.SetActive(false);
        }

        switch (choiceType)
        {
            case FinanceChoiceType.Saving:
                if (savingOptionPanel != null)
                {
                    savingOptionPanel.SetActive(true);
                }
                break;

            case FinanceChoiceType.Invest:
                if (investOptionPanel != null)
                {
                    investOptionPanel.SetActive(true);
                }
                break;

            case FinanceChoiceType.Lotto:
                if (lottoOptionPanel != null)
                {
                    lottoOptionPanel.SetActive(true);
                }
                break;
        }
    }

    private void OpenResultPanel(FinanceSelectionResult result)
    {
        if (resultPanel == null)
        {
            AutoBindPanels();
        }

        if (resultPanel == null)
        {
            Debug.LogWarning("FinanceSelectManager.OpenResultPanel: resultPanel is not assigned.", this);
            return;
        }

        SetCardsInteractable(false);

        if (resultPanelUI == null)
        {
            InitializeResultPanel();
        }

        if (resultPanelUI != null)
        {
            resultPanelUI.Show(result);
            return;
        }

        resultPanel.SetActive(true);
    }

    private int GetInitialSelectionIndex()
    {
        if (IsValidIndex(defaultSelectedIndex))
        {
            return defaultSelectedIndex;
        }

        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i] != null)
            {
                return i;
            }
        }

        return Mathf.Clamp(defaultSelectedIndex, 0, Mathf.Max(0, cards.Count - 1));
    }

    private bool IsValidIndex(int index)
    {
        return index >= 0 && index < cards.Count;
    }

    private void AutoBindPanels()
    {
        if (dimmedPanel == null)
        {
            dimmedPanel = FindSceneObject("DimmedPanel");
        }

        if (savingOptionPanel == null)
        {
            savingOptionPanel = FindSceneObject("SavingOptionPanel");
        }

        if (investOptionPanel == null)
        {
            investOptionPanel = FindSceneObject("InvestOptionPanel");
        }

        if (lottoOptionPanel == null)
        {
            lottoOptionPanel = FindSceneObject("LottoOptionPanel");
        }

        if (resultPanel == null)
        {
            resultPanel = FindSceneObject("ResultPanel");
        }
    }

    private void InitializeResultPanel()
    {
        if (resultPanel == null)
        {
            AutoBindPanels();
            if (resultPanel == null)
            {
                return;
            }
        }

        resultPanelUI = resultPanel.GetComponent<FinanceResultPanelUI>();
        if (resultPanelUI == null)
        {
            resultPanelUI = resultPanel.AddComponent<FinanceResultPanelUI>();
        }

        resultPanelUI.Initialize(this);
        resultPanel.SetActive(false);
    }

    private void EnsurePanelComponents()
    {
        EnsurePanelComponent<SavingOptionPanelUI>(savingOptionPanel);
        EnsurePanelComponent<InvestOptionPanelUI>(investOptionPanel);
        EnsurePanelComponent<LottoOptionPanelUI>(lottoOptionPanel);
    }

    private void ApplySelectionOutcome()
    {
        if (pendingResult == null)
        {
            return;
        }

        EnsureUserDataManager();

        int totalCoinReward = ResolveCollectedCoinReward() + pendingResult.coinDelta;
        if (totalCoinReward != 0 && UserDataManager.Instance != null)
        {
            UserDataManager.Instance.AddCoin(totalCoinReward);
        }

        string clearedStageId = ResolveClearedStageId();
        if (!string.IsNullOrWhiteSpace(clearedStageId) && UserDataManager.Instance != null)
        {
            UserDataManager.Instance.MarkStageCleared(clearedStageId);
        }

        FinanceFlowContext.Clear();
        GameManager.coinCount = 0;
        pendingResult = null;
    }

    private static int ResolveCollectedCoinReward()
    {
        return FinanceFlowContext.HasPendingGameClear
            ? FinanceFlowContext.CollectedCoin
            : Mathf.Max(GameManager.coinCount, 0);
    }

    private static string ResolveClearedStageId()
    {
        if (!FinanceFlowContext.HasPendingGameClear)
        {
            return string.Empty;
        }

        switch (FinanceFlowContext.StageCode)
        {
            case "ERA_2000":
                return "Stage_2000";

            case "ERA_2020":
                return "Stage_2020";

            case "ERA_1980":
                return "Stage_1980";

            default:
                return string.Empty;
        }
    }

    private static string ResolveStageSelectSceneName()
    {
        if (Application.CanStreamedLevelBeLoaded("StageSelect"))
        {
            return "StageSelect";
        }

        if (Application.CanStreamedLevelBeLoaded("StageSelectScene"))
        {
            return "StageSelectScene";
        }

        return string.Empty;
    }

    private static void EnsureUserDataManager()
    {
        if (UserDataManager.Instance != null)
        {
            return;
        }

        GameObject managerObject = new GameObject("UserDataManager");
        managerObject.AddComponent<UserDataManager>();
    }

    private static T EnsurePanelComponent<T>(GameObject panelObject) where T : Component
    {
        if (panelObject == null)
        {
            return null;
        }

        T component = panelObject.GetComponent<T>();
        if (component == null)
        {
            component = panelObject.AddComponent<T>();
        }

        return component;
    }

    private static GameObject FindSceneObject(string objectName)
    {
        if (string.IsNullOrWhiteSpace(objectName))
        {
            return null;
        }

        Scene activeScene = SceneManager.GetActiveScene();
        if (!activeScene.IsValid() || !activeScene.isLoaded)
        {
            return null;
        }

        GameObject[] roots = activeScene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            Transform found = FindDeepChild(roots[i].transform, objectName);
            if (found != null)
            {
                return found.gameObject;
            }
        }

        return null;
    }

    private static Transform FindDeepChild(Transform parent, string childName)
    {
        if (parent == null)
        {
            return null;
        }

        if (parent.name == childName)
        {
            return parent;
        }

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform found = FindDeepChild(parent.GetChild(i), childName);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }
}
