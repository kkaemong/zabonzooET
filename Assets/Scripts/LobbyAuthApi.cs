using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public sealed class LobbyAuthApi
{
    public static string DefaultBaseUrl => BackendUrlResolver.DefaultBaseUrl;

    private readonly string _baseUrl;
    private string _sessionCookie;
    private bool _hasSession;
    private long _currentUserId = -1;
    private string _currentLoginId = string.Empty;
    private string _currentNickname = string.Empty;

    public LobbyAuthApi(string baseUrl)
    {
        _baseUrl = BackendUrlResolver.Resolve(baseUrl);
    }

    public bool HasSession => _hasSession || !string.IsNullOrEmpty(_sessionCookie);

    public long CurrentUserId => _currentUserId;
    public string SessionCookie => _sessionCookie;
    public string BaseUrl => _baseUrl;

    public void RestoreSession(string sessionCookie, long userId, string loginId, string nickname)
    {
        _sessionCookie = string.IsNullOrWhiteSpace(sessionCookie) ? null : sessionCookie.Trim();
        _currentUserId = userId > 0 ? userId : -1;
        _currentLoginId = loginId ?? string.Empty;
        _currentNickname = nickname ?? string.Empty;
        _hasSession =
            !string.IsNullOrEmpty(_sessionCookie) ||
            (_currentUserId > 0 && !string.IsNullOrWhiteSpace(_currentLoginId));
    }

    public IEnumerator SignUp(
        string loginId,
        string password,
        string nickname,
        Action<ApiMessage> onSuccess,
        Action<ApiError> onError)
    {
        SignUpRequestBody body = new SignUpRequestBody
        {
            loginId = loginId,
            password = password,
            nickname = nickname,
        };

        yield return SendApiRequest(
            "/api/users/signup",
            UnityWebRequest.kHttpVerbPOST,
            JsonUtility.ToJson(body),
            captureCookie: false,
            onSuccess,
            onError);
    }

    public IEnumerator Login(
        string loginId,
        string password,
        Action<ApiMessage> onSuccess,
        Action<ApiError> onError)
    {
        ClearSessionState();

        LoginRequestBody body = new LoginRequestBody
        {
            loginId = loginId,
            password = password,
        };

        yield return SendApiRequest(
            "/api/users/login",
            UnityWebRequest.kHttpVerbPOST,
            JsonUtility.ToJson(body),
            captureCookie: true,
            onSuccess,
            onError);
    }

    public IEnumerator GetProfile(Action<GameProfileData> onSuccess, Action<ApiError> onError)
    {
        ApiError userError = null;
        yield return EnsureCurrentUser(error => userError = error);
        if (userError != null)
        {
            onError?.Invoke(userError);
            yield break;
        }

        using UnityWebRequest request = BuildRequest($"/api/game/user-stat?userId={_currentUserId}", UnityWebRequest.kHttpVerbGET, null);
        if (!TrySend(request, onError, out UnityWebRequestAsyncOperation operation))
        {
            yield break;
        }

        yield return operation;

        if (request.result == UnityWebRequest.Result.Success)
        {
            MarkSessionAuthenticated();

            UserStatBody userStat = TryParse<UserStatBody>(request.downloadHandler.text);
            if (userStat == null)
            {
                onError?.Invoke(new ApiError(request.responseCode, null, "프로필 응답을 해석하지 못했습니다."));
                yield break;
            }

            string nickname = !string.IsNullOrWhiteSpace(_currentNickname) ? _currentNickname : _currentLoginId;
            int coin = userStat.coinBalance;
            int hp = userStat.baseHp > 0 ? userStat.baseHp : 100;

            onSuccess?.Invoke(new GameProfileData(
                _currentUserId,
                _currentLoginId,
                nickname,
                coin,
                coin,
                hp,
                userStat.baseSpeed,
                userStat.boosterBonusSec,
                "ERA_1980"));
            yield break;
        }

        onError?.Invoke(CreateError(request));
    }

    public IEnumerator GetShopItems(Action<GameShopData> onSuccess, Action<ApiError> onError)
    {
        using UnityWebRequest request = BuildRequest("/api/game/shop", UnityWebRequest.kHttpVerbGET, null);
        if (!TrySend(request, onError, out UnityWebRequestAsyncOperation operation))
        {
            yield break;
        }

        yield return operation;

        if (request.result == UnityWebRequest.Result.Success)
        {
            MarkSessionAuthenticated();
            GameShopBody shop = TryParse<GameShopBody>(request.downloadHandler.text);
            if (shop == null || shop.items == null)
            {
                onError?.Invoke(new ApiError(request.responseCode, null, "Failed to parse shop response."));
                yield break;
            }

            ShopItemData[] items = new ShopItemData[shop.items.Length];
            for (int index = 0; index < shop.items.Length; index++)
            {
                ShopItemBody item = shop.items[index];
                items[index] = item == null
                    ? null
                    : new ShopItemData(
                        item.itemId,
                        item.itemName,
                        item.price,
                        item.description,
                        item.purchasable);
            }

            onSuccess?.Invoke(new GameShopData(items));
            yield break;
        }

        onError?.Invoke(CreateError(request));
    }

    public IEnumerator GetInventory(Action<GameInventoryData> onSuccess, Action<ApiError> onError)
    {
        ApiError userError = null;
        yield return EnsureCurrentUser(error => userError = error);
        if (userError != null)
        {
            onError?.Invoke(userError);
            yield break;
        }

        using UnityWebRequest request = BuildRequest($"/api/game/inventory?userId={_currentUserId}", UnityWebRequest.kHttpVerbGET, null);
        if (!TrySend(request, onError, out UnityWebRequestAsyncOperation operation))
        {
            yield break;
        }

        yield return operation;

        if (request.result == UnityWebRequest.Result.Success)
        {
            MarkSessionAuthenticated();
            GameInventoryBody inventory = TryParse<GameInventoryBody>(request.downloadHandler.text);
            if (inventory == null || inventory.items == null)
            {
                onSuccess?.Invoke(new GameInventoryData(Array.Empty<InventoryItemData>()));
                yield break;
            }

            InventoryItemData[] items = new InventoryItemData[inventory.items.Length];
            for (int index = 0; index < inventory.items.Length; index++)
            {
                InventoryItemBody item = inventory.items[index];
                items[index] = item == null
                    ? null
                    : new InventoryItemData(
                        item.itemId,
                        item.itemName,
                        item.quantity,
                        item.description);
            }

            onSuccess?.Invoke(new GameInventoryData(items));
            yield break;
        }

        onError?.Invoke(CreateError(request));
    }

    public IEnumerator PurchaseShopItem(
        long itemId,
        int quantity,
        Action<ShopPurchaseData> onSuccess,
        Action<ApiError> onError)
    {
        ApiError userError = null;
        yield return EnsureCurrentUser(error => userError = error);
        if (userError != null)
        {
            onError?.Invoke(userError);
            yield break;
        }

        ShopPurchaseRequestBody body = new ShopPurchaseRequestBody
        {
            userId = _currentUserId,
            itemId = itemId,
            quantity = quantity,
        };

        using UnityWebRequest request = BuildRequest(
            "/api/game/shop/purchase",
            UnityWebRequest.kHttpVerbPOST,
            JsonUtility.ToJson(body));
        if (!TrySend(request, onError, out UnityWebRequestAsyncOperation operation))
        {
            yield break;
        }

        yield return operation;

        if (request.result == UnityWebRequest.Result.Success)
        {
            MarkSessionAuthenticated();
            ShopPurchaseResponseBody response = TryParse<ShopPurchaseResponseBody>(request.downloadHandler.text);
            if (response == null)
            {
                onError?.Invoke(new ApiError(request.responseCode, null, "Failed to parse purchase response."));
                yield break;
            }

            onSuccess?.Invoke(new ShopPurchaseData(
                response.itemId,
                response.itemName,
                response.purchasedQuantity,
                response.usedCoin,
                response.remainingCoin));
            yield break;
        }

        onError?.Invoke(CreateError(request));
    }

    public IEnumerator RepairUfo(
        string partName,
        Action<UfoRepairData> onSuccess,
        Action<ApiError> onError)
    {
        ApiError userError = null;
        yield return EnsureCurrentUser(error => userError = error);
        if (userError != null)
        {
            onError?.Invoke(userError);
            yield break;
        }

        UfoRepairRequestBody body = new UfoRepairRequestBody
        {
            partName = partName,
        };

        using UnityWebRequest request = BuildRequest(
            $"/api/game/ufo-repair?userId={_currentUserId}",
            UnityWebRequest.kHttpVerbPOST,
            JsonUtility.ToJson(body));
        if (!TrySend(request, onError, out UnityWebRequestAsyncOperation operation))
        {
            yield break;
        }

        yield return operation;

        if (request.result == UnityWebRequest.Result.Success)
        {
            MarkSessionAuthenticated();
            UfoRepairResponseBody response = TryParse<UfoRepairResponseBody>(request.downloadHandler.text);
            if (response == null)
            {
                onError?.Invoke(new ApiError(request.responseCode, null, "수리 응답을 해석하지 못했습니다."));
                yield break;
            }

            onSuccess?.Invoke(new UfoRepairData(
                response.partName,
                response.repairCost,
                response.remainingCoin,
                response.effect,
                response.message));
            yield break;
        }

        onError?.Invoke(CreateError(request));
    }

    public sealed class ApiMessage
    {
        public ApiMessage(string message, string errorCode)
        {
            Message = message;
            ErrorCode = errorCode;
        }

        public string Message { get; }

        public string ErrorCode { get; }
    }

    public sealed class ApiError
    {
        public ApiError(long statusCode, string errorCode, string message)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
            Message = message;
        }

        public long StatusCode { get; }

        public string ErrorCode { get; }

        public string Message { get; }
    }

    public sealed class GameProfileData
    {
        public GameProfileData(
            long userId,
            string loginId,
            string nickname,
            int coin,
            int totalCoin,
            int hp,
            float baseSpeed,
            int boosterBonusSec,
            string currentStage)
        {
            UserId = userId;
            LoginId = loginId;
            Nickname = nickname;
            Coin = coin;
            TotalCoin = totalCoin;
            Hp = hp;
            BaseSpeed = baseSpeed;
            BoosterBonusSec = boosterBonusSec;
            CurrentStage = currentStage;
        }

        public long UserId { get; }

        public string LoginId { get; }

        public string Nickname { get; }

        public int Coin { get; }

        public int TotalCoin { get; }

        public int Hp { get; }

        public float BaseSpeed { get; }

        public int BoosterBonusSec { get; }

        public string CurrentStage { get; }
    }

    public sealed class GameInventoryData
    {
        public GameInventoryData(InventoryItemData[] items)
        {
            Items = items ?? Array.Empty<InventoryItemData>();
        }

        public InventoryItemData[] Items { get; }
    }

    public sealed class InventoryItemData
    {
        public InventoryItemData(long itemId, string itemName, int quantity, string description)
        {
            ItemId = itemId;
            ItemName = itemName;
            Quantity = quantity;
            Description = description;
        }

        public long ItemId { get; }

        public string ItemName { get; }

        public int Quantity { get; }

        public string Description { get; }
    }

    public sealed class GameShopData
    {
        public GameShopData(ShopItemData[] items)
        {
            Items = items;
        }

        public ShopItemData[] Items { get; }
    }

    public sealed class ShopItemData
    {
        public ShopItemData(long itemId, string itemName, int price, string description, bool purchasable)
        {
            ItemId = itemId;
            ItemName = itemName;
            Price = price;
            Description = description;
            Purchasable = purchasable;
        }

        public long ItemId { get; }

        public string ItemName { get; }

        public int Price { get; }

        public string Description { get; }

        public bool Purchasable { get; }
    }

    public sealed class UfoRepairData
    {
        public UfoRepairData(string partName, int repairCost, int remainingCoin, string effect, string message)
        {
            PartName = partName;
            RepairCost = repairCost;
            RemainingCoin = remainingCoin;
            Effect = effect;
            Message = message;
        }

        public string PartName { get; }

        public int RepairCost { get; }

        public int RemainingCoin { get; }

        public string Effect { get; }

        public string Message { get; }
    }

    public sealed class ShopPurchaseData
    {
        public ShopPurchaseData(long itemId, string itemName, int purchasedQuantity, int usedCoin, int remainingCoin)
        {
            ItemId = itemId;
            ItemName = itemName;
            PurchasedQuantity = purchasedQuantity;
            UsedCoin = usedCoin;
            RemainingCoin = remainingCoin;
        }

        public long ItemId { get; }

        public string ItemName { get; }

        public int PurchasedQuantity { get; }

        public int UsedCoin { get; }

        public int RemainingCoin { get; }
    }

    [Serializable]
    private sealed class LoginRequestBody
    {
        public string loginId;
        public string password;
    }

    [Serializable]
    private sealed class SignUpRequestBody
    {
        public string loginId;
        public string password;
        public string nickname;
    }

    [Serializable]
    private sealed class UfoRepairRequestBody
    {
        public string partName;
    }

    [Serializable]
    private sealed class ShopPurchaseRequestBody
    {
        public long userId;
        public long itemId;
        public int quantity;
    }

    [Serializable]
    private sealed class ApiResponseBody
    {
        public bool success;
        public string message;
        public string errorCode;
    }

    [Serializable]
    private sealed class SessionUserBody
    {
        public long userId;
        public string loginId;
        public string nickname;
    }

    [Serializable]
    private sealed class UserStatBody
    {
        public int coinBalance;
        public int baseHp;
        public float baseSpeed;
        public int boosterBonusSec;
    }

    [Serializable]
    private sealed class GameShopBody
    {
        public ShopItemBody[] items;
    }

    [Serializable]
    private sealed class GameInventoryBody
    {
        public InventoryItemBody[] items;
    }

    [Serializable]
    private sealed class ShopItemBody
    {
        public long itemId;
        public string itemName;
        public int price;
        public string description;
        public bool purchasable;
    }

    [Serializable]
    private sealed class InventoryItemBody
    {
        public long itemId;
        public string itemName;
        public int quantity;
        public string description;
    }

    [Serializable]
    private sealed class UfoRepairResponseBody
    {
        public string partName;
        public int repairCost;
        public int remainingCoin;
        public string effect;
        public string message;
    }

    [Serializable]
    private sealed class ShopPurchaseResponseBody
    {
        public long itemId;
        public string itemName;
        public int purchasedQuantity;
        public int usedCoin;
        public int remainingCoin;
    }

    private IEnumerator SendApiRequest(
        string path,
        string method,
        string jsonBody,
        bool captureCookie,
        Action<ApiMessage> onSuccess,
        Action<ApiError> onError)
    {
        using UnityWebRequest request = BuildRequest(path, method, jsonBody);
        if (!TrySend(request, onError, out UnityWebRequestAsyncOperation operation))
        {
            yield break;
        }

        yield return operation;

        if (request.result == UnityWebRequest.Result.Success)
        {
            if (captureCookie)
            {
                CaptureSessionCookie(request);
                MarkSessionAuthenticated();
            }

            ApiResponseBody body = TryParse<ApiResponseBody>(request.downloadHandler.text);
            if (body == null)
            {
                onError?.Invoke(new ApiError(request.responseCode, null, "서버 응답을 해석하지 못했습니다."));
                yield break;
            }

            onSuccess?.Invoke(new ApiMessage(body.message, body.errorCode));
            yield break;
        }

        onError?.Invoke(CreateError(request));
    }

    private IEnumerator EnsureCurrentUser(Action<ApiError> onError)
    {
        if (_currentUserId > 0)
        {
            yield break;
        }

        using UnityWebRequest request = BuildRequest("/api/users/me", UnityWebRequest.kHttpVerbGET, null);
        if (!TrySend(request, onError, out UnityWebRequestAsyncOperation operation))
        {
            yield break;
        }

        yield return operation;

        if (request.result == UnityWebRequest.Result.Success)
        {
            MarkSessionAuthenticated();

            SessionUserBody body = TryParse<SessionUserBody>(request.downloadHandler.text);
            if (body == null || body.userId <= 0)
            {
                onError?.Invoke(new ApiError(request.responseCode, null, "세션 사용자 정보를 읽지 못했습니다."));
                yield break;
            }

            _currentUserId = body.userId;
            _currentLoginId = body.loginId ?? string.Empty;
            _currentNickname = body.nickname ?? string.Empty;
            yield break;
        }

        onError?.Invoke(CreateError(request));
    }

    private UnityWebRequest BuildRequest(string path, string method, string jsonBody)
    {
        UnityWebRequest request = new UnityWebRequest(BuildUrl(path), method)
        {
            downloadHandler = new DownloadHandlerBuffer(),
            timeout = 15,
        };

        request.SetRequestHeader("Accept", "application/json");

        if (!BackendUrlResolver.UsesBrowserManagedCookies && !string.IsNullOrEmpty(_sessionCookie))
        {
            request.SetRequestHeader("Cookie", _sessionCookie);
        }

        if (!string.IsNullOrEmpty(jsonBody))
        {
            byte[] payload = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(payload);
            request.SetRequestHeader("Content-Type", "application/json");
        }

        return request;
    }

    private string BuildUrl(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return _baseUrl;
        }

        return path[0] == '/' ? _baseUrl + path : $"{_baseUrl}/{path}";
    }

    private void CaptureSessionCookie(UnityWebRequest request)
    {
        if (BackendUrlResolver.UsesBrowserManagedCookies)
        {
            _hasSession = true;
            return;
        }

        string setCookie = request.GetResponseHeader("Set-Cookie");
        if (string.IsNullOrEmpty(setCookie))
        {
            return;
        }

        int separator = setCookie.IndexOf(';');
        _sessionCookie = separator >= 0 ? setCookie.Substring(0, separator) : setCookie;
        _hasSession = true;
    }

    private void MarkSessionAuthenticated()
    {
        _hasSession = true;
    }

    private void ClearSessionState()
    {
        _sessionCookie = null;
        _hasSession = false;
        _currentUserId = -1;
        _currentLoginId = string.Empty;
        _currentNickname = string.Empty;
    }

    private ApiError CreateError(UnityWebRequest request)
    {
        if (request.responseCode == 401 || request.responseCode == 403)
        {
            ClearSessionState();
        }

        string responseText = request.downloadHandler != null ? request.downloadHandler.text : null;
        string requestUrl = request != null ? request.url : _baseUrl;
        string requestError = request != null ? request.error : null;

        ApiResponseBody body = TryParse<ApiResponseBody>(responseText);
        if (body != null && !string.IsNullOrWhiteSpace(body.message))
        {
            return new ApiError(request.responseCode, body.errorCode, body.message);
        }

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            return new ApiError(
                request.responseCode,
                null,
                "백엔드에 연결하지 못했습니다. 서버 주소와 실행 상태를 확인해 주세요.");
        }

        return new ApiError(
            request.responseCode,
            null,
            string.IsNullOrWhiteSpace(request.error)
                ? "서버 요청에 실패했습니다."
                : request.error);
    }

    private bool TrySend(
        UnityWebRequest request,
        Action<ApiError> onError,
        out UnityWebRequestAsyncOperation operation)
    {
        try
        {
            operation = request.SendWebRequest();
            return true;
        }
        catch (InvalidOperationException exception)
        {
            operation = null;
            onError?.Invoke(new ApiError(0, null, BuildExceptionMessage(exception)));
            return false;
        }
    }

    private string BuildExceptionMessage(InvalidOperationException exception)
    {
        if (exception != null &&
            !string.IsNullOrWhiteSpace(exception.Message) &&
            exception.Message.IndexOf("Insecure connection", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return "HTTP 백엔드 요청이 차단되었습니다. Player Settings의 Insecure HTTP 설정 또는 서버 주소를 확인해 주세요.";
        }

        return string.IsNullOrWhiteSpace(exception?.Message)
            ? "네트워크 요청을 시작하지 못했습니다."
            : exception.Message;
    }

    private static T TryParse<T>(string json) where T : class
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonUtility.FromJson<T>(json);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }
}
