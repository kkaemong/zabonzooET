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
    private float questionStartTime;
    private GameManager gm;
    private GameObject buttonGroupObj;

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

        if (GameManager.Instance != null && GameManager.Instance.sfxSource != null && GameManager.Instance.quizPopSound != null)
        {
            GameManager.Instance.sfxSource.PlayOneShot(GameManager.Instance.quizPopSound, GameManager.Instance.quizPopVolume);
        }

        if (quizPanel == null)
        {
            Debug.LogError("QuizManager.ShowQuiz: quizPanel is not assigned.");
            gm?.ResumeGame();
            return;
        }

        quizPanel.SetActive(true);
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

        if (buttonGroupObj != null)
        {
            buttonGroupObj.SetActive(true);
        }
    }

    private int GetChoiceNumber(int index)
    {
        if (currentApiQuiz?.choices == null || index < 0 || index >= currentApiQuiz.choices.Length || currentApiQuiz.choices[index] == null)
        {
            return -1;
        }

        // Backend expects a 1-based answer number, not the quizChoiceId primary key.
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
        }

        if (questionTextObj == null || buttonGroupObj == null ||
            btnTextAObj == null || btnTextBObj == null || btnTextCObj == null || btnTextDObj == null)
        {
            Debug.LogWarning("[QuizManager] QuizPanel bindings are incomplete.");
        }
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

    private void OnAnswerSelected(int selectedIndex)
    {
        int selectedAnswerNumber = GetChoiceNumber(selectedIndex);
        if (selectedAnswerNumber < 0 || currentApiQuiz == null)
        {
            return;
        }

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
        StartCoroutine(ShowExplanationCoroutine(result));
    }

    private IEnumerator ShowExplanationCoroutine(APIManager.QuizResultResponse result)
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

        string resultWord = isCorrect ? "<color=#00FF00>[정답]</color>" : "<color=#FF0000>[오답]</color>";
        string message = result != null && !string.IsNullOrWhiteSpace(result.message)
            ? $"\n\n<size=50>{result.message}</size>"
            : string.Empty;
        SetText(questionTextObj, $"{resultWord}{message}");

        yield return new WaitForSecondsRealtime(3f);

        if (gm != null && result != null)
        {
            if (result.hpChange > 0)
            {
                gm.AddLife(result.hpChange);
            }
            else if (result.hpChange < 0)
            {
                player playerComponent = FindObjectOfType<player>();
                if (playerComponent != null)
                {
                    playerComponent.TakeDamage(-result.hpChange);
                }
            }

            gm.ResumeGame();

            if (isCorrect)
            {
                gm.ApplyTemporarySpeedBoost(boostMultiplier, boostDuration);
            }
        }

        if (isCorrect)
        {
            player playerComponent = FindObjectOfType<player>();
            if (playerComponent != null)
            {
                playerComponent.TriggerQuizInvincibility(3f);
            }
        }

        if (quizPanel != null)
        {
            quizPanel.SetActive(false);
        }
    }
}
