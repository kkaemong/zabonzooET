using System;
using System.Collections;
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
    [SerializeField] private float runResultWaitTimeoutSeconds = 8f;

    [Header("Panels")]
    [SerializeField] private GameObject dimmedPanel;
    [SerializeField] private GameObject savingOptionPanel;
    [SerializeField] private GameObject investOptionPanel;
    [SerializeField] private GameObject lottoOptionPanel;
    [SerializeField] private GameObject resultPanel;

    private readonly Dictionary<FinanceChoiceType, APIManager.FinanceOptionResponse> optionsByChoiceType = new();

    private FinanceResultPanelUI resultPanelUI;
    private SavingOptionPanelUI savingOptionPanelUI;
    private InvestOptionPanelUI investOptionPanelUI;
    private LottoOptionPanelUI lottoOptionPanelUI;
    private FinanceSelectionResult pendingResult;

    private int currentIndex = -1;
    private bool isOptionPanelOpen;
    private bool isReturningToStageSelect;
    private bool isLoadingOptions;
    private bool areFinanceOptionsLoaded;
    private bool isSubmittingSelection;
    private string financeOptionsError = string.Empty;

    private void Awake()
    {
        AutoBindPanels();
        EnsurePanelComponents();
        InitializeResultPanel();
        InitializeCards();
        CloseOptionPanels();
        CloseResultPanel();
    }

    private void Start()
    {
        EnsureDependencies();

        if (FinanceFlowContext.HasRunResultResponse)
        {
            SynchronizeLocalUserCoin(FinanceFlowContext.RunRemainingTotalCoin);
        }

        BeginLoadFinanceOptions();
    }

    private void OnEnable()
    {
        if (cards.Count == 0)
        {
            return;
        }

        SelectCard(GetInitialSelectionIndex());
        RefreshCardInteractableState();
    }

    private void Update()
    {
        if (cards.Count == 0 ||
            isLoadingOptions ||
            isSubmittingSelection ||
            isOptionPanelOpen ||
            (resultPanel != null && resultPanel.activeSelf))
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
        if (isLoadingOptions || isSubmittingSelection)
        {
            return;
        }

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

        RefreshCardInteractableState();
    }

    public void CloseResultPanel()
    {
        if (pendingResult != null && pendingResult.isServerConfirmed)
        {
            return;
        }

        if (resultPanel == null)
        {
            AutoBindPanels();
        }

        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }

        RefreshCardInteractableState();
    }

    public void OnClickBackButton()
    {
        if (isSubmittingSelection)
        {
            return;
        }

        if (resultPanel != null && resultPanel.activeSelf)
        {
            CloseResultPanel();
            return;
        }

        CloseOptionPanels();
    }

    public void OnSavingOptionSelected(FinanceSelectionResult result)
    {
        BeginSelectionSubmission(result);
    }

    public void OnInvestOptionSelected(FinanceSelectionResult result)
    {
        BeginSelectionSubmission(result);
    }

    public void OnLottoOptionSelected(FinanceSelectionResult result)
    {
        BeginSelectionSubmission(result);
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
        if (!IsValidIndex(currentIndex) || isOptionPanelOpen || isLoadingOptions || isSubmittingSelection)
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
        if (!TryGetFinanceOption(choiceType, out APIManager.FinanceOptionResponse option))
        {
            string message = string.IsNullOrWhiteSpace(financeOptionsError)
                ? $"{ResolveChoiceLabel(choiceType)} 정보를 아직 불러오지 못했습니다. 잠시 후 다시 시도해 주세요."
                : $"{ResolveChoiceLabel(choiceType)} 정보를 불러오지 못했습니다.\n{financeOptionsError}";
            OpenResultPanel(BuildFailureResult(null, message));
            return;
        }

        isOptionPanelOpen = true;

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
                if (savingOptionPanelUI != null && savingOptionPanel != null)
                {
                    savingOptionPanelUI.Configure(this, option);
                    savingOptionPanel.SetActive(true);
                }
                break;

            case FinanceChoiceType.Invest:
                if (investOptionPanelUI != null && investOptionPanel != null)
                {
                    investOptionPanelUI.Configure(this, option);
                    investOptionPanel.SetActive(true);
                }
                break;

            case FinanceChoiceType.Lotto:
                if (lottoOptionPanelUI != null && lottoOptionPanel != null)
                {
                    lottoOptionPanelUI.Configure(this, option);
                    lottoOptionPanel.SetActive(true);
                }
                break;
        }

        RefreshCardInteractableState();
    }

    private void OpenResultPanel(FinanceSelectionResult result)
    {
        if (result == null)
        {
            return;
        }

        if (resultPanel == null)
        {
            AutoBindPanels();
        }

        if (resultPanel == null)
        {
            Debug.LogWarning("FinanceSelectManager.OpenResultPanel: resultPanel is not assigned.", this);
            return;
        }

        if (resultPanelUI == null)
        {
            InitializeResultPanel();
        }

        if (resultPanelUI != null)
        {
            resultPanelUI.Show(result);
        }
        else
        {
            resultPanel.SetActive(true);
        }

        RefreshCardInteractableState();
    }

    private void BeginLoadFinanceOptions()
    {
        EnsureDependencies();

        if (APIManager.Instance == null)
        {
            areFinanceOptionsLoaded = false;
            isLoadingOptions = false;
            financeOptionsError = "APIManager를 찾을 수 없습니다.";
            RefreshCardInteractableState();
            return;
        }

        isLoadingOptions = true;
        areFinanceOptionsLoaded = false;
        financeOptionsError = string.Empty;
        optionsByChoiceType.Clear();
        RefreshCardInteractableState();

        APIManager.Instance.GetFinanceOptions(
            ResolveStageCode(),
            HandleFinanceOptionsLoaded,
            HandleFinanceOptionsLoadError);
    }

    private void HandleFinanceOptionsLoaded(APIManager.FinanceOptionsResponse response)
    {
        isLoadingOptions = false;
        areFinanceOptionsLoaded = true;
        financeOptionsError = string.Empty;
        optionsByChoiceType.Clear();

        APIManager.FinanceOptionResponse[] options = response != null && response.options != null
            ? response.options
            : Array.Empty<APIManager.FinanceOptionResponse>();

        for (int i = 0; i < options.Length; i++)
        {
            APIManager.FinanceOptionResponse option = options[i];
            FinanceChoiceType choiceType = ResolveChoiceType(option?.optionType);
            if (choiceType == FinanceChoiceType.None || option == null)
            {
                continue;
            }

            optionsByChoiceType[choiceType] = option;
        }

        ApplyCardTitles();
        RefreshCardInteractableState();
    }

    private void HandleFinanceOptionsLoadError(string error)
    {
        isLoadingOptions = false;
        areFinanceOptionsLoaded = false;
        financeOptionsError = string.IsNullOrWhiteSpace(error)
            ? "백엔드에서 금융 선택지를 불러오지 못했습니다."
            : error.Trim();

        Debug.LogError($"FinanceSelectManager.HandleFinanceOptionsLoadError: {financeOptionsError}", this);
        RefreshCardInteractableState();
    }

    private void BeginSelectionSubmission(FinanceSelectionResult selection)
    {
        if (selection == null || isSubmittingSelection)
        {
            return;
        }

        StartCoroutine(SubmitFinanceSelectionRoutine(selection));
    }

    private IEnumerator SubmitFinanceSelectionRoutine(FinanceSelectionResult selection)
    {
        isSubmittingSelection = true;
        CloseOptionPanels();
        RefreshCardInteractableState();

        yield return WaitForRunResultIfNeeded();

        if (!TryResolveFinanceBaseCoin(out int baseCoin, out string prerequisiteError))
        {
            pendingResult = BuildFailureResult(selection, prerequisiteError);
            OpenResultPanel(pendingResult);
            isSubmittingSelection = false;
            RefreshCardInteractableState();
            yield break;
        }

        if (APIManager.Instance == null)
        {
            pendingResult = BuildFailureResult(selection, "백엔드 연결을 찾을 수 없습니다.");
            OpenResultPanel(pendingResult);
            isSubmittingSelection = false;
            RefreshCardInteractableState();
            yield break;
        }

        bool completed = false;
        string error = string.Empty;
        APIManager.FinanceEventResponse response = null;

        APIManager.Instance.SubmitFinanceEvent(
            ResolveStageCode(),
            selection.choiceCode,
            selection.optionId,
            baseCoin,
            value =>
            {
                response = value;
                completed = true;
            },
            message =>
            {
                error = message;
                completed = true;
            });

        while (!completed)
        {
            yield return null;
        }

        pendingResult = response != null && string.IsNullOrWhiteSpace(error)
            ? BuildConfirmedResult(selection, response)
            : BuildFailureResult(selection, BuildFinanceEventErrorMessage(error));

        OpenResultPanel(pendingResult);
        isSubmittingSelection = false;
        RefreshCardInteractableState();
    }

    private IEnumerator WaitForRunResultIfNeeded()
    {
        if (!FinanceFlowContext.IsRunResultPending)
        {
            if (FinanceFlowContext.HasRunResultResponse)
            {
                SynchronizeLocalUserCoin(FinanceFlowContext.RunRemainingTotalCoin);
            }

            yield break;
        }

        float timeoutAt = Time.realtimeSinceStartup + Mathf.Max(1f, runResultWaitTimeoutSeconds);
        while (FinanceFlowContext.IsRunResultPending && Time.realtimeSinceStartup < timeoutAt)
        {
            yield return null;
        }

        if (FinanceFlowContext.IsRunResultPending)
        {
            FinanceFlowContext.SetRunResultError("스테이지 결과 반영이 지연되고 있습니다.");
            yield break;
        }

        if (FinanceFlowContext.HasRunResultResponse)
        {
            SynchronizeLocalUserCoin(FinanceFlowContext.RunRemainingTotalCoin);
        }
    }

    private bool TryResolveFinanceBaseCoin(out int baseCoin, out string errorMessage)
    {
        baseCoin = 0;
        errorMessage = string.Empty;

        if (FinanceFlowContext.HasRunResultResponse)
        {
            if (!FinanceFlowContext.RunFinanceEventAvailable)
            {
                errorMessage = "클리어 결과가 없어 금융 선택을 진행할 수 없습니다.";
                return false;
            }

            baseCoin = Mathf.Max(FinanceFlowContext.RunRemainingTotalCoin, 0);
            return true;
        }

        if (!string.IsNullOrWhiteSpace(FinanceFlowContext.RunResultError))
        {
            errorMessage = $"스테이지 결과를 서버에 반영하지 못했습니다.\n{FinanceFlowContext.RunResultError}";
            return false;
        }

        if (FinanceFlowContext.RunId > 0 || FinanceFlowContext.HasPendingGameClear)
        {
            errorMessage = "스테이지 결과가 아직 준비되지 않았습니다. 잠시 후 다시 시도해 주세요.";
            return false;
        }

        EnsureUserDataManager();
        baseCoin = UserDataManager.Instance != null && UserDataManager.Instance.CurrentUser != null
            ? Mathf.Max(UserDataManager.Instance.CurrentUser.coin, 0)
            : 0;
        return true;
    }

    private void ApplyCardTitles()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            FinanceChoiceCardUI card = cards[i];
            if (card == null)
            {
                continue;
            }

            if (TryGetFinanceOption(card.choiceType, out APIManager.FinanceOptionResponse option) &&
                !string.IsNullOrWhiteSpace(option.title))
            {
                card.SetTitle(option.title);
            }
        }
    }

    private void RefreshCardInteractableState()
    {
        bool allowInteraction =
            !isLoadingOptions &&
            !isSubmittingSelection &&
            !isOptionPanelOpen &&
            (resultPanel == null || !resultPanel.activeSelf);

        SetCardsInteractable(allowInteraction);
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
        savingOptionPanelUI = EnsurePanelComponent<SavingOptionPanelUI>(savingOptionPanel);
        investOptionPanelUI = EnsurePanelComponent<InvestOptionPanelUI>(investOptionPanel);
        lottoOptionPanelUI = EnsurePanelComponent<LottoOptionPanelUI>(lottoOptionPanel);
    }

    private void ApplySelectionOutcome()
    {
        if (pendingResult == null || !pendingResult.isServerConfirmed)
        {
            return;
        }

        SynchronizeLocalUserCoin(pendingResult.finalCoin);

        if (UserDataManager.Instance != null)
        {
            string clearedStageId = ResolveStageAssetId(string.IsNullOrWhiteSpace(pendingResult.stageId)
                ? FinanceFlowContext.StageCode
                : pendingResult.stageId);
            if (!string.IsNullOrWhiteSpace(clearedStageId))
            {
                UserDataManager.Instance.MarkStageCleared(clearedStageId);
            }

            string unlockedStageId = ResolveStageAssetId(pendingResult.nextEra);
            if (!string.IsNullOrWhiteSpace(unlockedStageId))
            {
                UserDataManager.Instance.UnlockStage(unlockedStageId);
            }
        }

        FinanceFlowContext.Clear();
        GameManager.coinCount = 0;
        pendingResult = null;
    }

    private void EnsureDependencies()
    {
        EnsureUserDataManager();
        EnsureApiManager();
    }

    private void SynchronizeLocalUserCoin(int coin)
    {
        EnsureUserDataManager();
        if (UserDataManager.Instance == null)
        {
            return;
        }

        UserData currentUser = UserDataManager.Instance.CurrentUser;
        if (currentUser != null && currentUser.coin == coin)
        {
            return;
        }

        UserDataManager.Instance.SetUser(BuildUserSnapshot(coin));
    }

    private UserData BuildUserSnapshot(int coin)
    {
        UserData source = UserDataManager.Instance != null ? UserDataManager.Instance.CurrentUser : null;
        return new UserData
        {
            userId = source != null && !string.IsNullOrWhiteSpace(source.userId) ? source.userId : "guest",
            backendUserId = source != null && source.backendUserId != 0
                ? source.backendUserId
                : (BackendRuntimeSession.UserId > 0 ? BackendRuntimeSession.UserId : -1),
            nickname = source != null && !string.IsNullOrWhiteSpace(source.nickname) ? source.nickname : "Guest",
            coin = Mathf.Max(coin, 0),
            clearedStageIds = source != null && source.clearedStageIds != null
                ? new List<string>(source.clearedStageIds)
                : new List<string>(),
            unlockedStageIds = source != null && source.unlockedStageIds != null
                ? new List<string>(source.unlockedStageIds)
                : new List<string>()
        };
    }

    private bool TryGetFinanceOption(FinanceChoiceType choiceType, out APIManager.FinanceOptionResponse option)
    {
        option = null;

        if (!areFinanceOptionsLoaded)
        {
            return false;
        }

        return optionsByChoiceType.TryGetValue(choiceType, out option) && option != null;
    }

    private static FinanceChoiceType ResolveChoiceType(string optionType)
    {
        if (string.IsNullOrWhiteSpace(optionType))
        {
            return FinanceChoiceType.None;
        }

        switch (optionType.Trim().ToUpperInvariant())
        {
            case "SAVING":
                return FinanceChoiceType.Saving;

            case "INVESTMENT":
            case "INVEST":
                return FinanceChoiceType.Invest;

            case "LOTTO":
                return FinanceChoiceType.Lotto;

            default:
                return FinanceChoiceType.None;
        }
    }

    private static string ResolveChoiceLabel(FinanceChoiceType choiceType)
    {
        switch (choiceType)
        {
            case FinanceChoiceType.Saving:
                return "예금";

            case FinanceChoiceType.Invest:
                return "투자";

            case FinanceChoiceType.Lotto:
                return "복권";

            default:
                return "금융 리포트";
        }
    }

    private static FinanceSelectionResult BuildFailureResult(FinanceSelectionResult selection, string message)
    {
        return new FinanceSelectionResult
        {
            choiceType = selection != null ? selection.choiceType : FinanceChoiceType.None,
            choiceCode = selection != null ? selection.choiceCode : string.Empty,
            optionId = selection != null ? selection.optionId : string.Empty,
            optionName = selection != null ? selection.optionName : "금융 리포트",
            description = selection != null ? selection.description : string.Empty,
            resultMessage = string.IsNullOrWhiteSpace(message)
                ? "금융 리포트를 불러오지 못했습니다."
                : message.Trim(),
            isServerConfirmed = false
        };
    }

    private static FinanceSelectionResult BuildConfirmedResult(
        FinanceSelectionResult selection,
        APIManager.FinanceEventResponse response)
    {
        return new FinanceSelectionResult
        {
            choiceType = selection != null ? selection.choiceType : ResolveChoiceType(response.choice),
            choiceCode = response.choice ?? (selection != null ? selection.choiceCode : string.Empty),
            optionId = selection != null ? selection.optionId : string.Empty,
            optionName = selection != null ? selection.optionName : response.choice,
            stageId = response.stageId,
            description = selection != null ? selection.description : string.Empty,
            baseCoin = response.baseCoin,
            coinDelta = response.changeCoin,
            finalCoin = response.finalCoin,
            resultType = response.resultType,
            resultMessage = response.detailResult,
            aiFeedback = response.aiFeedback,
            nextEra = response.nextEra,
            finalClear = response.finalClear,
            isServerConfirmed = true
        };
    }

    private static string BuildFinanceEventErrorMessage(string error)
    {
        return string.IsNullOrWhiteSpace(error)
            ? "금융 리포트를 불러오지 못했습니다."
            : $"금융 리포트를 불러오지 못했습니다.\n{error.Trim()}";
    }

    private static string ResolveStageAssetId(string eraCode)
    {
        switch ((eraCode ?? string.Empty).Trim().ToUpperInvariant())
        {
            case "ERA_1980":
                return "Stage_1980";

            case "ERA_2000":
                return "Stage_2000";

            case "ERA_2020":
                return "Stage_2020";

            default:
                return string.Empty;
        }
    }

    private string ResolveStageCode()
    {
        if (!string.IsNullOrWhiteSpace(FinanceFlowContext.StageCode))
        {
            return FinanceFlowContext.StageCode;
        }

        return GameManager.Instance != null
            ? GameManager.Instance.CurrentStageCode
            : "ERA_1980";
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

    private static void EnsureApiManager()
    {
        if (APIManager.Instance != null)
        {
            return;
        }

        GameObject managerObject = new GameObject("APIManager");
        managerObject.AddComponent<APIManager>();
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
