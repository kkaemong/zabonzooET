using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class QuizManager : MonoBehaviour
{
    public static QuizManager Instance { get; private set; }

    [Header("UI")]
    public GameObject quizPanel;
    public GameObject questionTextObj;
    public GameObject btnTextAObj;
    public GameObject btnTextBObj;
    public GameObject btnTextCObj;
    public GameObject btnTextDObj;
    public GameObject buttonAObj;
    public GameObject buttonBObj;
    public GameObject buttonCObj;
    public GameObject buttonDObj;

    [Header("Boost")]
    public float boostMultiplier = 2f;
    public float boostDuration = 3f;

    private APIManager.QuizQuestionResponse currentApiQuiz;
    private APIManager.QuizResultResponse pendingQuizResult;
    private float questionStartTime;
    private GameManager gm;
    private GameObject buttonGroupObj;
    private GameObject confirmButtonObj;
    private GameObject confirmButtonTextObj;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        gm = FindObjectOfType<GameManager>();
        EnsureBindings();
    }

    public void ShowQuiz()
    {
        if (gm == null)
        {
            gm = FindObjectOfType<GameManager>();
        }

        EnsureBindings();
        gm?.PauseGame();

        if (GameManager.Instance != null &&
            GameManager.Instance.sfxSource != null &&
            GameManager.Instance.quizPopSound != null)
        {
            GameManager.Instance.sfxSource.PlayOneShot(
                GameManager.Instance.quizPopSound,
                GameManager.Instance.quizPopVolume);
        }

        if (quizPanel == null)
        {
            Debug.LogError("QuizManager.ShowQuiz: quizPanel is not assigned.");
            gm?.ResumeGame();
            return;
        }

        currentApiQuiz = null;
        pendingQuizResult = null;

        quizPanel.SetActive(true);
        SetAnswerButtonsVisible(false);
        SetConfirmButtonVisible(false);
        if (buttonGroupObj != null)
        {
            buttonGroupObj.SetActive(false);
        }

        SetText(questionTextObj, "문제를 불러오는 중입니다...");

        long runId = GameManager.Instance != null ? GameManager.Instance.currentRunId : -1;
        if (APIManager.Instance == null)
        {
            SetText(questionTextObj, "<size=40>APIManager를 찾을 수 없습니다.</size>");
            StartCoroutine(ResumeAfterDelay(3f));
            return;
        }

        APIManager.Instance.GetQuiz(runId, OnQuizFetched, error =>
        {
            SetText(questionTextObj, $"<size=40>퀴즈를 불러오지 못했습니다.\n\n{error}</size>");
            StartCoroutine(ResumeAfterDelay(3f));
        });
    }

    private IEnumerator ResumeAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        currentApiQuiz = null;
        pendingQuizResult = null;
        SetConfirmButtonVisible(false);
        SetAnswerButtonsVisible(false);

        if (quizPanel != null)
        {
            quizPanel.SetActive(false);
        }

        gm?.ResumeGame();
    }

    private void OnQuizFetched(APIManager.QuizQuestionResponse response)
    {
        if (response == null)
        {
            SetText(questionTextObj, "<size=40>퀴즈 응답이 비어 있습니다.</size>");
            StartCoroutine(ResumeAfterDelay(3f));
            return;
        }

        currentApiQuiz = response;
        pendingQuizResult = null;
        questionStartTime = Time.realtimeSinceStartup;

        SetText(questionTextObj, currentApiQuiz.questionText);

        APIManager.QuizChoiceResponse[] choices = currentApiQuiz.choices;
        Debug.Log($"[QuizManager] Quiz fetched. question='{currentApiQuiz.questionText}', choiceCount={(choices == null ? 0 : choices.Length)}");

        SetText(btnTextAObj, choices != null && choices.Length > 0 ? choices[0].choiceText : "-");
        SetText(btnTextBObj, choices != null && choices.Length > 1 ? choices[1].choiceText : "-");
        SetText(btnTextCObj, choices != null && choices.Length > 2 ? choices[2].choiceText : "-");
        SetText(btnTextDObj, choices != null && choices.Length > 3 ? choices[3].choiceText : "-");

        AddButtonListener(buttonAObj, () => OnAnswerSelected(0));
        AddButtonListener(buttonBObj, () => OnAnswerSelected(1));
        AddButtonListener(buttonCObj, () => OnAnswerSelected(2));
        AddButtonListener(buttonDObj, () => OnAnswerSelected(3));

        SetConfirmButtonVisible(false);
        SetAnswerButtonsVisible(true);
        if (buttonGroupObj != null)
        {
            buttonGroupObj.SetActive(true);
        }
    }

    private int GetChoiceNumber(int index)
    {
        if (currentApiQuiz?.choices == null ||
            index < 0 ||
            index >= currentApiQuiz.choices.Length ||
            currentApiQuiz.choices[index] == null)
        {
            return -1;
        }

        return index + 1;
    }

    private void SetText(GameObject target, string value)
    {
        if (target == null)
        {
            Debug.LogWarning($"[QuizManager] SetText target is null. value='{value}'");
            return;
        }

        Text text = target.GetComponent<Text>();
        if (text != null)
        {
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 10;
            text.resizeTextMaxSize = 50;
            text.text = value;
            return;
        }

        TextMeshProUGUI tmp = target.GetComponent<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 10;
            tmp.fontSizeMax = 50;
            tmp.text = value;
        }
    }

    private void EnsureBindings()
    {
        quizPanel = ResolveQuizPanel();

        if (quizPanel == null)
        {
            Debug.LogWarning("[QuizManager] QuizPanel could not be found.");
            return;
        }

        quizPanel.SetActive(false);

        questionTextObj = ResolveTextTarget(questionTextObj, quizPanel.transform, "QuestionText");

        Transform buttonGroup = FindDeepChildByName(quizPanel.transform, "ButtonGroup");
        if (buttonGroup != null)
        {
            buttonGroupObj = buttonGroup.gameObject;
            buttonAObj = ResolveChild(buttonAObj, buttonGroup, "ButtonA");
            buttonBObj = ResolveChild(buttonBObj, buttonGroup, "ButtonB");
            buttonCObj = ResolveChild(buttonCObj, buttonGroup, "ButtonC");
            buttonDObj = ResolveChild(buttonDObj, buttonGroup, "ButtonD");

            btnTextAObj = ResolveButtonText(btnTextAObj, buttonAObj);
            btnTextBObj = ResolveButtonText(btnTextBObj, buttonBObj);
            btnTextCObj = ResolveButtonText(btnTextCObj, buttonCObj);
            btnTextDObj = ResolveButtonText(btnTextDObj, buttonDObj);

            EnsureConfirmButton();
        }

        if (questionTextObj == null ||
            buttonGroupObj == null ||
            btnTextAObj == null ||
            btnTextBObj == null ||
            btnTextCObj == null ||
            btnTextDObj == null)
        {
            Debug.LogWarning("[QuizManager] QuizPanel bindings are incomplete.");
        }
    }

    private void EnsureConfirmButton()
    {
        if (buttonGroupObj == null || buttonAObj == null)
        {
            return;
        }

        if (!IsUsableSceneObject(confirmButtonObj))
        {
            confirmButtonObj = Instantiate(buttonAObj, buttonGroupObj.transform);
            confirmButtonObj.name = "ConfirmButton";

            RectTransform templateRect = buttonAObj.GetComponent<RectTransform>();
            RectTransform confirmRect = confirmButtonObj.GetComponent<RectTransform>();
            if (templateRect != null && confirmRect != null)
            {
                confirmRect.anchorMin = new Vector2(0.5f, 0f);
                confirmRect.anchorMax = new Vector2(0.5f, 0f);
                confirmRect.pivot = new Vector2(0.5f, 0.5f);
                confirmRect.sizeDelta = templateRect.sizeDelta;
                confirmRect.localScale = templateRect.localScale;
                confirmRect.localRotation = templateRect.localRotation;
                confirmRect.anchoredPosition = new Vector2(0f, 52f);
            }
        }

        confirmButtonTextObj = ResolveButtonText(confirmButtonTextObj, confirmButtonObj);
        SetText(confirmButtonTextObj, "확인");
        SetConfirmButtonVisible(false);
    }

    private GameObject ResolveQuizPanel()
    {
        if (IsUsableSceneObject(quizPanel))
        {
            return quizPanel;
        }

        Transform scoreCanvas = FindInLoadedScenes("ScoreCanvas");
        if (scoreCanvas != null)
        {
            Transform nestedPanel = FindDeepChildByName(scoreCanvas, "QuizPanel");
            if (nestedPanel != null)
            {
                return nestedPanel.gameObject;
            }
        }

        Transform directPanel = FindInLoadedScenes("QuizPanel");
        return directPanel != null ? directPanel.gameObject : null;
    }

    private GameObject ResolveTextTarget(GameObject current, Transform parent, string childName)
    {
        if (IsUsableSceneObject(current))
        {
            return current;
        }

        Transform found = FindDeepChildByName(parent, childName);
        if (found != null)
        {
            return found.gameObject;
        }

        Text uiText = parent.GetComponentInChildren<Text>(true);
        if (uiText != null)
        {
            return uiText.gameObject;
        }

        TextMeshProUGUI tmpText = parent.GetComponentInChildren<TextMeshProUGUI>(true);
        return tmpText != null ? tmpText.gameObject : null;
    }

    private GameObject ResolveChild(GameObject current, Transform parent, string childName)
    {
        if (IsUsableSceneObject(current))
        {
            return current;
        }

        Transform found = FindDeepChildByName(parent, childName);
        return found != null ? found.gameObject : null;
    }

    private GameObject ResolveButtonText(GameObject current, GameObject buttonObject)
    {
        if (IsUsableSceneObject(current))
        {
            return current;
        }

        if (buttonObject == null)
        {
            return null;
        }

        Transform directText = FindDeepChildByName(buttonObject.transform, "Text");
        if (directText != null)
        {
            return directText.gameObject;
        }

        Text uiText = buttonObject.GetComponentInChildren<Text>(true);
        if (uiText != null)
        {
            return uiText.gameObject;
        }

        TextMeshProUGUI tmpText = buttonObject.GetComponentInChildren<TextMeshProUGUI>(true);
        return tmpText != null ? tmpText.gameObject : null;
    }

    private bool IsUsableSceneObject(GameObject target)
    {
        return target != null && target.scene.IsValid() && target.scene.isLoaded;
    }

    private Transform FindInLoadedScenes(string objectName)
    {
        Scene activeScene = SceneManager.GetActiveScene();
        Transform foundInActive = FindInScene(activeScene, objectName);
        if (foundInActive != null)
        {
            return foundInActive;
        }

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene loadedScene = SceneManager.GetSceneAt(i);
            if (loadedScene == activeScene)
            {
                continue;
            }

            Transform found = FindInScene(loadedScene, objectName);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private Transform FindInScene(Scene scene, string objectName)
    {
        if (!scene.IsValid() || !scene.isLoaded)
        {
            return null;
        }

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Transform found = FindDeepChildByName(root.transform, objectName);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private Transform FindDeepChildByName(Transform parent, string childName)
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
            Transform found = FindDeepChildByName(parent.GetChild(i), childName);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private void AddButtonListener(GameObject target, UnityAction action)
    {
        if (target == null)
        {
            return;
        }

        Button button = target.GetComponent<Button>();
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(action);
    }

    private void SetAnswerButtonsVisible(bool visible)
    {
        SetGameObjectVisible(buttonAObj, visible);
        SetGameObjectVisible(buttonBObj, visible);
        SetGameObjectVisible(buttonCObj, visible);
        SetGameObjectVisible(buttonDObj, visible);
    }

    private void SetConfirmButtonVisible(bool visible)
    {
        SetGameObjectVisible(confirmButtonObj, visible);
    }

    private void SetGameObjectVisible(GameObject target, bool visible)
    {
        if (target != null)
        {
            target.SetActive(visible);
        }
    }

    private void OnAnswerSelected(int selectedIndex)
    {
        int selectedAnswerNumber = GetChoiceNumber(selectedIndex);
        if (selectedAnswerNumber < 0 || currentApiQuiz == null)
        {
            return;
        }

        SetAnswerButtonsVisible(false);
        SetConfirmButtonVisible(false);
        if (buttonGroupObj != null)
        {
            buttonGroupObj.SetActive(false);
        }

        SetText(questionTextObj, "정답 확인 중...");

        APIManager.QuizResultRequest request = new APIManager.QuizResultRequest
        {
            stageId = GameManager.Instance != null ? GameManager.Instance.CurrentStageCode : "ERA_1980",
            quizId = currentApiQuiz.quizQuestionId,
            selectedAnswer = selectedAnswerNumber,
            responseTime = Time.realtimeSinceStartup - questionStartTime,
            timeOver = false,
            runId = GameManager.Instance != null ? GameManager.Instance.currentRunId : -1
        };

        APIManager.Instance.SubmitQuizResult(request, OnQuizResultReceived, error =>
        {
            SetText(questionTextObj, $"<size=40>퀴즈 결과 전송에 실패했습니다.\n\n{error}</size>");
            StartCoroutine(ResumeAfterDelay(3f));
        });
    }

    private void OnQuizResultReceived(APIManager.QuizResultResponse result)
    {
        pendingQuizResult = result;
        ShowResultState(result);
    }

    private void ShowResultState(APIManager.QuizResultResponse result)
    {
        bool isCorrect = result != null && result.correct;

        if (GameManager.Instance != null)
        {
            if (isCorrect && GameManager.Instance.quizCorrectSound != null)
            {
                GameManager.Instance.PlaySFX(GameManager.Instance.quizCorrectSound, GameManager.Instance.quizCorrectVolume);
            }
            else if (!isCorrect && GameManager.Instance.quizWrongSound != null)
            {
                GameManager.Instance.PlaySFX(GameManager.Instance.quizWrongSound, GameManager.Instance.quizWrongVolume);
            }
        }

        string resultWord = isCorrect ? "<color=#00FF00>[정답]</color>" : "<color=#FF4D4D>[오답]</color>";
        string resultMessage = result != null && !string.IsNullOrWhiteSpace(result.message)
            ? result.message
            : (isCorrect ? "정답입니다." : "오답입니다.");
        string explanation = ResolveQuizExplanation(isCorrect);
        string explanationBlock = string.IsNullOrWhiteSpace(explanation)
            ? string.Empty
            : $"\n\n<size=28>{explanation}</size>";

        SetText(questionTextObj, $"{resultWord}\n<size=36>{resultMessage}</size>{explanationBlock}");

        if (buttonGroupObj != null)
        {
            buttonGroupObj.SetActive(true);
        }

        EnsureConfirmButton();
        SetAnswerButtonsVisible(false);
        SetText(confirmButtonTextObj, "확인");
        AddButtonListener(confirmButtonObj, ConfirmQuizResult);
        SetConfirmButtonVisible(true);
    }

    private string ResolveQuizExplanation(bool isCorrect)
    {
        if (currentApiQuiz == null)
        {
            return string.Empty;
        }

        string explanation = isCorrect
            ? currentApiQuiz.correctExplanation
            : currentApiQuiz.wrongExplanation;

        return !string.IsNullOrWhiteSpace(explanation)
            ? explanation.Trim()
            : string.Empty;
    }

    private void ConfirmQuizResult()
    {
        bool isCorrect = pendingQuizResult != null && pendingQuizResult.correct;

        if (gm != null && pendingQuizResult != null)
        {
            if (pendingQuizResult.hpChange > 0)
            {
                gm.AddLife(pendingQuizResult.hpChange);
            }
            else if (pendingQuizResult.hpChange < 0)
            {
                player playerComponent = FindObjectOfType<player>();
                if (playerComponent != null)
                {
                    playerComponent.TakeDamage(-pendingQuizResult.hpChange);
                }
            }
        }

        if (quizPanel != null)
        {
            quizPanel.SetActive(false);
        }

        SetConfirmButtonVisible(false);
        SetAnswerButtonsVisible(false);
        currentApiQuiz = null;
        pendingQuizResult = null;

        gm?.ResumeGame();

        if (isCorrect && gm != null)
        {
            gm.ApplyTemporarySpeedBoost(boostMultiplier, boostDuration);

            player playerComponent = FindObjectOfType<player>();
            if (playerComponent != null)
            {
                playerComponent.TriggerQuizInvincibility(3f);
            }
        }
    }
}
