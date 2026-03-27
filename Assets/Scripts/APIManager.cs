using System;
using System.Collections;
using System.Text;
using System.Text.Json;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Handles ingame HTTP communication with the backend.
/// </summary>
public class APIManager : MonoBehaviour
{
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        IncludeFields = true
    };

    public static APIManager Instance { get; private set; }

    [Header("Server")]
    [Tooltip("Fallback backend URL. Runtime session data can override this value.")]
    public string baseUrl = LobbyAuthApi.DefaultBaseUrl;

    [Tooltip("Optional bearer token for isolated ingame API tests.")]
    public string authToken = string.Empty;

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
        public string subOptionId;
        public string subOptionName;
        public string description;
        public int expectedReturn;
    }

    [Serializable]
    public class FinanceOptionResponse
    {
        public string optionId;
        public string optionName;
        public string description;
        public FinanceSubOptionResponse[] subOptions;
    }

    [Serializable]
    public class FinanceOptionsResponse
    {
        public FinanceOptionResponse[] options;
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
        SyncRuntimeSession();

        long backendUserId = ResolveBackendUserId();
        if (backendUserId <= 0)
        {
            Debug.LogWarning("[APIManager] Cannot send run-result because backend user id is missing.");
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
        StartCoroutine(PostRequest(url, JsonUtility.ToJson(body)));
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

    public void GetQuiz(long runId, Action<QuizQuestionResponse> onSuccess, Action<string> onError)
    {
        SyncRuntimeSession();
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
        string normalizedBaseUrl = string.IsNullOrWhiteSpace(baseUrl)
            ? LobbyAuthApi.DefaultBaseUrl
            : baseUrl.TrimEnd('/');

        if (string.IsNullOrWhiteSpace(path))
        {
            return normalizedBaseUrl;
        }

        return path.StartsWith("/", StringComparison.Ordinal)
            ? normalizedBaseUrl + path
            : normalizedBaseUrl + "/" + path;
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
        if (!string.IsNullOrWhiteSpace(BackendRuntimeSession.BaseUrl))
        {
            baseUrl = BackendRuntimeSession.BaseUrl;
        }
    }

    private bool TryDeserialize<T>(string json, out T result, Action<string> onError, string endpoint)
    {
        try
        {
            result = JsonSerializer.Deserialize<T>(json, JsonOptions);
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

        if (!string.IsNullOrWhiteSpace(BackendRuntimeSession.SessionCookie))
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
