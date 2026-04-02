using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Handles ingame HTTP communication with the backend.
/// </summary>
public class APIManager : MonoBehaviour
{
    private const string Era2020StageCode = "ERA_2020";

    private readonly struct LocalQuizDefinition
    {
        public LocalQuizDefinition(long quizId, string questionText, int timeLimitSec, int correctAnswerNumber, string explanation, params string[] choices)
        {
            QuizId = quizId;
            QuestionText = questionText;
            TimeLimitSec = timeLimitSec;
            CorrectAnswerNumber = correctAnswerNumber;
            Explanation = explanation;
            Choices = choices ?? Array.Empty<string>();
        }

        public long QuizId { get; }
        public string QuestionText { get; }
        public int TimeLimitSec { get; }
        public int CorrectAnswerNumber { get; }
        public string Explanation { get; }
        public string[] Choices { get; }
    }

    private static readonly LocalQuizDefinition[] Era2020QuizBank =
    {
        new LocalQuizDefinition(
            202001,
            "2020년대 '주주환원' 흐름에서,\n기업이 벌어들인 이익 일부를\n주주에게 현금으로 나눠주는 것은?",
            15,
            1,
            "배당금은 기업이 벌어들인 이익 일부를 주주에게 현금이나 주식으로 돌려주는 대표적인 주주환원 방식입니다.",
            "배당금",
            "증거금",
            "공모가",
            "액면가"),
        new LocalQuizDefinition(
            202002,
            "2024년 대한민국\n수출액 반등의 주역은?",
            15,
            3,
            "2024년 수출 반등은 메모리 업황 회복과 AI 수요 확대에 힘입은 반도체 수출 증가가 핵심 동력이었습니다.",
            "자동차",
            "조선업",
            "반도체",
            "석유화학"),
        new LocalQuizDefinition(
            202003,
            "2026년 삼성전자가 발표한,\n주식을 불태워 1주당 가치를\n극대화 하는 이 '주주환원 정책'은?",
            15,
            3,
            "자사주 소각은 회사가 보유한 자기주식을 없애 유통 주식 수를 줄이고, 주당 가치와 주주가치를 높이려는 대표적인 주주환원 정책입니다.",
            "액면분할",
            "유상증자",
            "자사주 소각",
            "무상감자"),
    };

    public static APIManager Instance { get; private set; }

    [Header("Server")]
    [Tooltip("Fallback backend URL. Runtime session data can override this value.")]
    public string baseUrl = LobbyAuthApi.DefaultBaseUrl;

    [Tooltip("Optional bearer token for isolated ingame API tests.")]
    public string authToken = string.Empty;

    private int era2020QuizIndex;

    [Serializable]
    public class RunResultRequest
    {
        public string stageId;
        public long runId;
        public int playTime;
        public int distance;
        public int collectedCoin;
        public int remainingHp;
        public bool quizCorrect;
        public string financeChoice;
        public bool cleared;
        public string[] usedItems;
    }

    [Serializable]
    public class QuizChoiceResponse
    {
        public long quizChoiceId;
        public string choiceText;
    }

    [Serializable]
    public class QuizQuestionResponse
    {
        public long quizQuestionId;
        public string questionText;
        public int timeLimitSec;
        public QuizChoiceResponse[] choices;
        public string explanation;
    }

    [Serializable]
    public class QuizResultRequest
    {
        public string stageId;
        public long quizId;
        public int selectedAnswer;
        public double responseTime;
        public bool timeOver;
        public long runId;
    }

    [Serializable]
    public class QuizResultResponse
    {
        public bool correct;
        public string effectType;
        public double speedMultiplier;
        public int hpChange;
        public string monsterAction;
        public string message;
        public int currentLife;
        public int maxLife;
        public int quizCount;
    }

    [Serializable]
    public class GameStartRequest
    {
        public string stageCode;
    }

    [Serializable]
    public class GameStartResponse
    {
        public long runId;
        public long stageId;
        public string stageCode;
        public string stageName;
        public int targetDistance;
        public int life;
        public int maxLife;
        public string status;
    }

    [Serializable]
    public class FinanceSubOptionResponse
    {
        public string code;
        public string name;
        public string description;
    }

    [Serializable]
    public class FinanceOptionResponse
    {
        public string optionType;
        public string title;
        public string description;
        public FinanceSubOptionResponse[] subOptions;
    }

    [Serializable]
    public class FinanceOptionsResponse
    {
        public FinanceOptionResponse[] options;
    }

    [Serializable]
    public class FinanceEventRequest
    {
        public string stageId;
        public int baseCoin;
        public string choice;
        public string subOptionCode;
    }

    [Serializable]
    public class FinanceEventResponse
    {
        public string stageId;
        public string choice;
        public int baseCoin;
        public int changeCoin;
        public int finalCoin;
        public string resultType;
        public string detailResult;
        public string aiFeedback;
        public string nextEra;
        public bool finalClear;
    }

    [Serializable]
    public class RunResultResponse
    {
        public long runId;
        public bool cleared;
        public int rewardCoin;
        public int remainingTotalCoin;
        public string currentEra;
        public string nextStep;
        public bool financeEventAvailable;
        public string nextEra;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        SyncRuntimeSession();
    }

    public void StartGame(string stageCode, Action<GameStartResponse> onSuccess, Action<string> onError)
    {
        SyncRuntimeSession();
        ResetLocalQuizSequence(stageCode);

        GameStartRequest body = new GameStartRequest { stageCode = stageCode };
        string json = JsonUtility.ToJson(body);

        StartCoroutine(PostRequestWithCallback(
            BuildUrl("/api/game/start"),
            json,
            responseJson =>
            {
                if (TryDeserialize(responseJson, out GameStartResponse response, onError, "game/start"))
                {
                    onSuccess?.Invoke(response);
                }
            },
            onError));
    }

    public void SendGameResult(
        long runId,
        int coins,
        int distance,
        int hp,
        bool cleared,
        string currentStage = "ERA_1980",
        string financeChoice = "NONE")
    {
        SendGameResult(runId, coins, distance, hp, cleared, currentStage, financeChoice, null, null);
    }

    public void SendGameResult(
        long runId,
        int coins,
        int distance,
        int hp,
        bool cleared,
        string currentStage,
        string financeChoice,
        Action<RunResultResponse> onSuccess,
        Action<string> onError)
    {
        SyncRuntimeSession();

        long backendUserId = ResolveBackendUserId();
        if (backendUserId <= 0)
        {
            const string message = "[APIManager] Cannot send run-result because backend user id is missing.";
            Debug.LogWarning(message);
            onError?.Invoke(message);
            return;
        }

        RunResultRequest body = new RunResultRequest
        {
            stageId = currentStage,
            runId = runId,
            playTime = 0,
            distance = distance,
            collectedCoin = coins,
            remainingHp = hp,
            quizCorrect = false,
            financeChoice = string.IsNullOrWhiteSpace(financeChoice) ? "NONE" : financeChoice,
            cleared = cleared,
            usedItems = Array.Empty<string>()
        };

        string url = BuildUrl($"/api/game/run-result?userId={backendUserId}");
        string json = JsonUtility.ToJson(body);

        if (onSuccess == null && onError == null)
        {
            StartCoroutine(PostRequest(url, json));
            return;
        }

        StartCoroutine(PostRequestWithCallback(
            url,
            json,
            responseJson =>
            {
                if (TryDeserialize(responseJson, out RunResultResponse response, onError, "game/run-result"))
                {
                    onSuccess?.Invoke(response);
                }
            },
            onError));
    }

    public void GetStageInfo(string stageCode, Action<string> onSuccess = null, Action<string> onError = null)
    {
        SyncRuntimeSession();
        StartCoroutine(GetRequestWithCallback(
            BuildUrl($"/api/game/stage?stageCode={UnityWebRequest.EscapeURL(stageCode)}"),
            onSuccess,
            onError));
    }

    public void GetFinanceOptions(string stageCode, Action<FinanceOptionsResponse> onSuccess, Action<string> onError)
    {
        SyncRuntimeSession();
        string normalizedStageCode = string.IsNullOrWhiteSpace(stageCode)
            ? ResolveCurrentStageCode()
            : stageCode;

        StartCoroutine(GetRequestWithCallback(
            BuildUrl($"/api/game/finance-options?stageCode={UnityWebRequest.EscapeURL(normalizedStageCode)}"),
            responseJson =>
            {
                if (TryDeserialize(responseJson, out FinanceOptionsResponse response, onError, "game/finance-options"))
                {
                    onSuccess?.Invoke(response);
                }
            },
            onError));
    }

    public void GetFinanceOptions()
    {
        GetFinanceOptions(ResolveCurrentStageCode(), null, error =>
        {
            Debug.LogError($"[APIManager] Failed to get finance options: {error}");
        });
    }

    public void SubmitFinanceEvent(
        string stageCode,
        string choice,
        string subOptionCode,
        int baseCoin,
        Action<FinanceEventResponse> onSuccess,
        Action<string> onError)
    {
        SyncRuntimeSession();

        long backendUserId = ResolveBackendUserId();
        if (backendUserId <= 0)
        {
            onError?.Invoke("Cannot submit finance event because backend user id is missing.");
            return;
        }

        FinanceEventRequest body = new FinanceEventRequest
        {
            stageId = string.IsNullOrWhiteSpace(stageCode) ? ResolveCurrentStageCode() : stageCode,
            baseCoin = Mathf.Max(baseCoin, 0),
            choice = choice,
            subOptionCode = subOptionCode,
        };

        StartCoroutine(PostRequestWithCallback(
            BuildUrl($"/api/game/finance-event?userId={backendUserId}"),
            JsonUtility.ToJson(body),
            responseJson =>
            {
                if (TryDeserialize(responseJson, out FinanceEventResponse response, onError, "game/finance-event"))
                {
                    onSuccess?.Invoke(response);
                }
            },
            onError));
    }

    public void GetQuiz(long runId, Action<QuizQuestionResponse> onSuccess, Action<string> onError)
    {
        SyncRuntimeSession();

        if (UsesLocalEra2020Quiz(ResolveCurrentStageCode()))
        {
            onSuccess?.Invoke(CreateEra2020QuizQuestionResponse());
            return;
        }

        StartCoroutine(GetRequestWithCallback(
            BuildUrl($"/api/game/quiz?runId={runId}"),
            responseJson =>
            {
                if (TryDeserialize(responseJson, out QuizQuestionResponse response, onError, "game/quiz"))
                {
                    onSuccess?.Invoke(response);
                }
            },
            onError));
    }

    public void SubmitQuizResult(QuizResultRequest requestData, Action<QuizResultResponse> onSuccess, Action<string> onError)
    {
        SyncRuntimeSession();

        if (UsesLocalEra2020Quiz(requestData?.stageId))
        {
            onSuccess?.Invoke(EvaluateEra2020QuizResult(requestData));
            return;
        }

        string json = JsonUtility.ToJson(requestData);

        StartCoroutine(PostRequestWithCallback(
            BuildUrl("/api/game/quiz-result"),
            json,
            responseJson =>
            {
                if (TryDeserialize(responseJson, out QuizResultResponse response, onError, "game/quiz-result"))
                {
                    onSuccess?.Invoke(response);
                }
            },
            onError));
    }

    private string BuildUrl(string path)
    {
        string normalizedBaseUrl = BackendUrlResolver.Resolve(baseUrl);

        if (string.IsNullOrWhiteSpace(path))
        {
            return normalizedBaseUrl;
        }

        return path.StartsWith("/", StringComparison.Ordinal)
            ? normalizedBaseUrl + path
            : normalizedBaseUrl + "/" + path;
    }

    private void ResetLocalQuizSequence(string stageCode)
    {
        if (UsesLocalEra2020Quiz(stageCode))
        {
            era2020QuizIndex = 0;
        }
    }

    private bool UsesLocalEra2020Quiz(string stageCode)
    {
        return string.Equals(stageCode, Era2020StageCode, StringComparison.OrdinalIgnoreCase);
    }

    private QuizQuestionResponse CreateEra2020QuizQuestionResponse()
    {
        int questionIndex = Mathf.Clamp(era2020QuizIndex, 0, Era2020QuizBank.Length - 1);
        LocalQuizDefinition definition = Era2020QuizBank[questionIndex];
        era2020QuizIndex = Mathf.Min(questionIndex + 1, Era2020QuizBank.Length);

        QuizChoiceResponse[] choices = new QuizChoiceResponse[definition.Choices.Length];
        for (int i = 0; i < definition.Choices.Length; i++)
        {
            choices[i] = new QuizChoiceResponse
            {
                quizChoiceId = definition.QuizId * 10 + i + 1,
                choiceText = definition.Choices[i]
            };
        }

        Debug.Log($"[APIManager] Using hardcoded 2020 quiz #{questionIndex + 1}: {definition.QuestionText}");

        return new QuizQuestionResponse
        {
            quizQuestionId = definition.QuizId,
            questionText = definition.QuestionText,
            timeLimitSec = definition.TimeLimitSec,
            choices = choices,
            explanation = definition.Explanation
        };
    }

    private QuizResultResponse EvaluateEra2020QuizResult(QuizResultRequest requestData)
    {
        if (requestData == null)
        {
            return CreateLocalQuizResultResponse(false, "오답입니다.. 체력이 감소했습니다.");
        }

        if (requestData.timeOver)
        {
            return CreateLocalQuizResultResponse(false, "시간 초과로 오답 처리되었습니다.");
        }

        if (!TryGetEra2020QuizDefinition(requestData.quizId, out LocalQuizDefinition definition))
        {
            Debug.LogWarning($"[APIManager] Unknown hardcoded 2020 quiz id: {requestData.quizId}");
            return CreateLocalQuizResultResponse(false, "오답입니다.. 체력이 감소했습니다.");
        }

        bool isCorrect = requestData.selectedAnswer == definition.CorrectAnswerNumber;
        return CreateLocalQuizResultResponse(
            isCorrect,
            isCorrect ? "정답입니다! 체력이 증가했습니다." : "오답입니다.. 체력이 감소했습니다.");
    }

    private QuizResultResponse CreateLocalQuizResultResponse(bool isCorrect, string message)
    {
        int hpChange = isCorrect ? 1 : -1;
        player playerComponent = FindObjectOfType<player>();
        int maxLife = 3;
        int currentLifeBeforeApply = playerComponent != null ? playerComponent.health : maxLife;
        int currentLifeAfterApply = Mathf.Clamp(currentLifeBeforeApply + hpChange, 0, maxLife);

        return new QuizResultResponse
        {
            correct = isCorrect,
            effectType = isCorrect ? "BUFF" : "DEBUFF",
            speedMultiplier = 1.0,
            hpChange = hpChange,
            monsterAction = "NONE",
            message = message,
            currentLife = currentLifeAfterApply,
            maxLife = maxLife,
            quizCount = Mathf.Clamp(era2020QuizIndex, 0, Era2020QuizBank.Length)
        };
    }

    private bool TryGetEra2020QuizDefinition(long quizId, out LocalQuizDefinition definition)
    {
        for (int i = 0; i < Era2020QuizBank.Length; i++)
        {
            if (Era2020QuizBank[i].QuizId == quizId)
            {
                definition = Era2020QuizBank[i];
                return true;
            }
        }

        definition = default;
        return false;
    }

    private long ResolveBackendUserId()
    {
        if (BackendRuntimeSession.UserId > 0)
        {
            return BackendRuntimeSession.UserId;
        }

        if (UserDataManager.Instance != null && UserDataManager.Instance.CurrentUser != null && UserDataManager.Instance.CurrentUser.backendUserId > 0)
        {
            return UserDataManager.Instance.CurrentUser.backendUserId;
        }

        return -1;
    }

    private string ResolveCurrentStageCode()
    {
        return GameManager.Instance != null
            ? GameManager.Instance.CurrentStageCode
            : "ERA_1980";
    }

    private void SyncRuntimeSession()
    {
        string sessionBaseUrl = !string.IsNullOrWhiteSpace(BackendRuntimeSession.BaseUrl)
            ? BackendRuntimeSession.BaseUrl
            : baseUrl;
        baseUrl = BackendUrlResolver.Resolve(sessionBaseUrl);
    }

    private bool TryDeserialize<T>(string json, out T result, Action<string> onError, string endpoint)
    {
        try
        {
            result = JsonUtility.FromJson<T>(json);
            if (result == null)
            {
                onError?.Invoke($"Failed to parse response from {endpoint}: empty payload");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[APIManager] Failed to deserialize {endpoint} response.\n{ex}\nPayload: {json}");
            onError?.Invoke($"Failed to parse response from {endpoint}: {ex.Message}");
            result = default;
            return false;
        }
    }

    private void ApplyCommonHeaders(UnityWebRequest request, bool includeJsonContentType)
    {
        if (includeJsonContentType)
        {
            request.SetRequestHeader("Content-Type", "application/json");
        }

        if (!string.IsNullOrWhiteSpace(authToken))
        {
            request.SetRequestHeader("Authorization", "Bearer " + authToken);
        }

        if (!BackendUrlResolver.UsesBrowserManagedCookies &&
            !string.IsNullOrWhiteSpace(BackendRuntimeSession.SessionCookie))
        {
            request.SetRequestHeader("Cookie", BackendRuntimeSession.SessionCookie);
        }
    }

    private IEnumerator PostRequest(string url, string jsonBody)
    {
        using UnityWebRequest request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
        byte[] body = Encoding.UTF8.GetBytes(jsonBody ?? string.Empty);
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        ApplyCommonHeaders(request, includeJsonContentType: true);

        Debug.Log($"<color=cyan>[API POST]</color> {url}\n{jsonBody}");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError($"<color=red>[API POST ERROR]</color> {request.responseCode} {request.error}\n{request.downloadHandler.text}");
            yield break;
        }

        Debug.Log($"<color=green>[API POST OK]</color> {request.downloadHandler.text}");
    }

    private IEnumerator GetRequestWithCallback(string url, Action<string> onSuccess, Action<string> onError)
    {
        using UnityWebRequest request = UnityWebRequest.Get(url);
        ApplyCommonHeaders(request, includeJsonContentType: false);

        Debug.Log($"<color=cyan>[API GET]</color> {url}");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            string errorMessage = $"{request.responseCode} {request.error}";
            if (!string.IsNullOrWhiteSpace(request.downloadHandler.text))
            {
                errorMessage += $"\n{request.downloadHandler.text}";
            }

            Debug.LogError($"<color=red>[API GET ERROR]</color> {url}\n{errorMessage}");
            onError?.Invoke(errorMessage);
            yield break;
        }

        Debug.Log($"<color=green>[API GET OK]</color> {request.downloadHandler.text}");
        onSuccess?.Invoke(request.downloadHandler.text);
    }

    private IEnumerator PostRequestWithCallback(string url, string jsonBody, Action<string> onSuccess, Action<string> onError)
    {
        using UnityWebRequest request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
        byte[] body = Encoding.UTF8.GetBytes(jsonBody ?? string.Empty);
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        ApplyCommonHeaders(request, includeJsonContentType: true);

        Debug.Log($"<color=cyan>[API POST]</color> {url}\n{jsonBody}");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            string errorMessage = $"{request.responseCode} {request.error}";
            if (!string.IsNullOrWhiteSpace(request.downloadHandler.text))
            {
                errorMessage += $"\n{request.downloadHandler.text}";
            }

            Debug.LogError($"<color=red>[API POST ERROR]</color> {url}\n{errorMessage}");
            onError?.Invoke(errorMessage);
            yield break;
        }

        Debug.Log($"<color=green>[API POST OK]</color> {request.downloadHandler.text}");
        onSuccess?.Invoke(request.downloadHandler.text);
    }
}
