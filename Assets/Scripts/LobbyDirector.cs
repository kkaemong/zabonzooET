using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyDirector : MonoBehaviour
{
    [Header("Background")]
    [SerializeField] private GameObject bgPlain;
    [SerializeField] private GameObject bgCrashed;
    [SerializeField] private SpriteRenderer bgPlainRenderer;
    [SerializeField] private SpriteRenderer bgCrashedRenderer;
    [SerializeField] private float backgroundOverscan = 1.02f;

    [Header("UFO")]
    [SerializeField] private GameObject ufoFlying;
    [SerializeField] private GameObject ufoCrashed;

    [Header("Effects")]
    [SerializeField] private ParticleSystem fxImpactDust;
    [SerializeField] private ParticleSystem fxSmokeA;
    [SerializeField] private ParticleSystem fxSmokeB;

    [Header("Core UI")]
    [SerializeField] private GameObject canvasLogin;
    [SerializeField] private GameObject canvasLobby;
    [SerializeField] private Button btnLogin;

    [Header("Backend")]
    [SerializeField] private string backendBaseUrl = LobbyAuthApi.DefaultBaseUrl;
    [SerializeField] private bool autoLoginAfterSignup = true;
    [SerializeField] private bool allowDirectLoginButton;
    [SerializeField] private long repairUserIdOverride = -1;

    [Header("Lobby Popups")]
    public GameObject canvasShop;
    public GameObject canvasGarage;
    public Button btnShopOpen;
    public Button btnGarageOpen;
    public Button btnShopClose;
    public Button btnGarageClose;

    [Header("Timing")]
    [SerializeField] private float loginFadeOutDuration = 0.4f;
    [SerializeField] private float lobbyFadeInDuration = 0.6f;
    [SerializeField] private float postCrashDelay = 1.2f;

    public int playerCoins = 3000;

    private const int GarageMaxLevel = 5;
    private const string GuestPlayerId = "guest";
    private const string PlayerStatePrefsPrefix = "MonET.Lobby.PlayerState.";
    private const string RepairUserIdPrefsPrefix = "MonET.Lobby.RepairUserId.";
    private const string LastKnownCoinPrefsPrefix = "MonET.Lobby.LastKnownCoin.";

    private CanvasGroup _loginCanvasGroup;
    private CanvasGroup _lobbyCanvasGroup;
    private bool _isTransitioning;
    private bool _hasCompletedLogin;
    private bool _authBusy;
    private bool _garageBusy;
    private bool _shopBusy;
    private Button _btnGameStart;

    private LobbyAuthApi _authApi;
    private AuthMode _authMode = AuthMode.Login;

    private Button _btnRegister;
    private InputField _inputId;
    private InputField _inputPassword;
    private InputField _inputNickname;
    private Text _subtitleText;
    private Text _btnLoginText;
    private Text _btnRegisterText;
    private Text _statusText;
    private Text _lobbyCoinText;
    private Text _shopCoinText;
    private Text _garageCoinText;
    private Text _garageProgressLabel;
    private Image _garageProgressFill;
    private Button _garageBulkUpgradeButton;
    private Text _garageBulkUpgradeLabel;
    private GameObject _garageUserIdPanel;
    private InputField _garageUserIdInput;
    private Button _garageUserIdApplyButton;
    private Text _garageUserIdApplyLabel;
    private Text _garageUserIdHintText;
    private Text _shopHintText;
    private readonly Dictionary<GaragePart, GarageUpgradeCard> _garageCards = new Dictionary<GaragePart, GarageUpgradeCard>();
    private readonly Dictionary<string, ShopItemCard> _shopCards = new Dictionary<string, ShopItemCard>(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, int> _ownedItemCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
    private GarageUpgradeState _garageState = GarageUpgradeState.CreateDefault(3000);
    private string _currentLoginId = string.Empty;
    private string _currentNickname = string.Empty;
    private long _currentUserId = -1;
    private int _currentBaseHp = 100;
    private float _currentBaseSpeed = 2f;
    private int _currentBoosterBonusSec;

    private enum AuthMode
    {
        Login,
        SignUp,
    }

    private enum GaragePart
    {
        Engine,
        Wheel,
        Armor,
        Magnet,
    }

    private sealed class ShopItemCard
    {
        public GameObject Root;
        public string ItemKey;
        public string DisplayName;
        public long ItemId;
        public int Price;
        public string Description;
        public bool Purchasable = true;
        public Text TagText;
        public Text TitleText;
        public Text SubtitleText;
        public Text DescriptionText;
        public Text PriceText;
        public Text OwnedCountText;
        public Button BuyButton;
        public Text BuyButtonLabel;
        public Image BuyButtonImage;
    }

    [Serializable]
    private sealed class GarageUpgradeState
    {
        public int coinBalance = -1;
        public int engineLevel = 1;
        public int wheelLevel = 1;
        public int armorLevel = 1;
        public int magnetLevel = 1;

        public static GarageUpgradeState CreateDefault(int coinBalance)
        {
            return new GarageUpgradeState
            {
                coinBalance = coinBalance,
                engineLevel = 1,
                wheelLevel = 1,
                armorLevel = 1,
                magnetLevel = 1,
            };
        }
    }

    private sealed class GarageUpgradeCard
    {
        public GameObject Root;
        public GaragePart Part;
        public string PartName;
        public string DisplayName;
        public int RepairCost;
        public string EffectText;
        public Text TitleText;
        public Text DescriptionText;
        public Text LevelText;
        public Text PriceText;
        public Button UpgradeButton;
        public Text UpgradeButtonLabel;
        public Image UpgradeButtonImage;
    }

    private void Awake()
    {
        EnsureCanvasGroups();
        ResolveAuthUiReferences();
        EnsureGeneratedAuthUi();
        ResolveCoinLabels();
        ResolveShopUiReferences();
        ResolveGarageUiReferences();
        EnsureGeneratedGarageUi();
        backendBaseUrl = BackendUrlResolver.Resolve(backendBaseUrl);
        _authApi = new LobbyAuthApi(backendBaseUrl);
    }

    private void Start()
    {
        FitLobbyBackgroundsToCamera();
        SetupInitialState();
        BindNavigationButtons();
        BindAuthButtons();
        BindShopButtons();
        BindGarageButtons();
        ApplyAuthMode(AuthMode.Login);
        TryResumeLobbyMainFromPendingReturn();
        RestoreAuthStateFromKnownUser();
        UpdateCoinLabels(playerCoins);
        RefreshShopUi();
        RefreshGarageUi();
        StartCoroutine(ResumeLobbyMainFromPendingReturnNextFrame());
    }

    private void FitLobbyBackgroundsToCamera()
    {
        Camera targetCamera = Camera.main;
        if (targetCamera == null || !targetCamera.orthographic)
        {
            return;
        }

        FitBackgroundRenderer(bgPlainRenderer, targetCamera);
        FitBackgroundRenderer(bgCrashedRenderer, targetCamera);
    }

    private void FitBackgroundRenderer(SpriteRenderer renderer, Camera targetCamera)
    {
        if (renderer == null || renderer.sprite == null)
        {
            return;
        }

        Vector2 spriteSize = renderer.sprite.bounds.size;
        if (spriteSize.x <= 0f || spriteSize.y <= 0f)
        {
            return;
        }

        float worldHeight = targetCamera.orthographicSize * 2f;
        float worldWidth = worldHeight * targetCamera.aspect;
        float scale = Mathf.Max(worldWidth / spriteSize.x, worldHeight / spriteSize.y) * Mathf.Max(backgroundOverscan, 1f);

        Vector3 currentScale = renderer.transform.localScale;
        renderer.transform.localScale = new Vector3(
            Mathf.Sign(currentScale.x) * scale,
            Mathf.Sign(currentScale.y) * scale,
            currentScale.z);
    }

    public void OnLoginComplete()
    {
        if (_hasCompletedLogin)
        {
            return;
        }

        _hasCompletedLogin = true;
        StartCoroutine(CrashSequence());
    }

    public void OpenShop()
    {
        if (_isTransitioning || canvasShop == null)
        {
            return;
        }

        RefreshShopUi();
        if (HasAuthenticatedSession())
        {
            StartCoroutine(SyncShopCatalogFlow(true));
            StartCoroutine(SyncInventoryFlow(true));
        }

        StartCoroutine(TransitionScreen(canvasLobby, canvasShop, false));
    }

    public void CloseShop()
    {
        if (_isTransitioning || canvasShop == null)
        {
            return;
        }

        StartCoroutine(TransitionScreen(canvasShop, canvasLobby, true));
    }

    public void OpenGarage()
    {
        if (_isTransitioning || canvasGarage == null)
        {
            return;
        }

        RefreshGarageUi();
        StartCoroutine(TransitionScreen(canvasLobby, canvasGarage, false));
    }

    public void CloseGarage()
    {
        if (_isTransitioning || canvasGarage == null)
        {
            return;
        }

        StartCoroutine(TransitionScreen(canvasGarage, canvasLobby, true));
    }

    public void OpenStageSelect()
    {
        if (_isTransitioning)
        {
            return;
        }

        TryResumeLobbyMainFromPendingReturn();
        RestoreAuthStateFromKnownUser();

        if (!HasAuthenticatedSession())
        {
            SetStatus("로그인 후 스테이지를 선택할 수 있습니다.", true);
            return;
        }

        string fallbackNickname = _inputNickname != null ? _inputNickname.text.Trim() : string.Empty;
        string nickname = !string.IsNullOrWhiteSpace(_currentNickname)
            ? _currentNickname
            : (!string.IsNullOrWhiteSpace(fallbackNickname) ? fallbackNickname : _currentLoginId);

        BackendRuntimeSession.Configure(
            backendBaseUrl,
            _currentUserId,
            _authApi != null ? _authApi.SessionCookie : null,
            _currentLoginId,
            nickname);

        LobbyStageFlowBridge.EnterStageSelect(_currentLoginId, nickname, playerCoins, _currentUserId);
    }

    public void SpendCoin(int amount, string itemName)
    {
        TrySpendCoin(amount, itemName);
    }

    public bool TrySpendCoin(int amount, string itemName)
    {
        if (amount <= 0)
        {
            return true;
        }

        if (playerCoins >= amount)
        {
            playerCoins -= amount;
            UpdateCoinLabels(playerCoins);
            Debug.Log($"[{itemName}] completed. Remaining coins: {playerCoins}");
            return true;
        }

        Debug.LogWarning($"[{itemName}] failed. Need {amount}, current {playerCoins}");
        return false;
    }

    private void EnsureCanvasGroups()
    {
        if (canvasLogin != null)
        {
            _loginCanvasGroup = canvasLogin.GetComponent<CanvasGroup>();
            if (_loginCanvasGroup == null)
            {
                _loginCanvasGroup = canvasLogin.AddComponent<CanvasGroup>();
            }
        }

        if (canvasLobby != null)
        {
            _lobbyCanvasGroup = canvasLobby.GetComponent<CanvasGroup>();
            if (_lobbyCanvasGroup == null)
            {
                _lobbyCanvasGroup = canvasLobby.AddComponent<CanvasGroup>();
            }
        }
    }

    private void ResolveAuthUiReferences()
    {
        Transform loginCard = canvasLogin != null ? canvasLogin.transform.Find("Panel_LoginCard") : null;
        if (loginCard == null)
        {
            return;
        }

        _inputId = FindComponent<InputField>(loginCard, "Input_ID");
        _inputPassword = FindComponent<InputField>(loginCard, "Input_PW");
        _inputNickname = FindComponent<InputField>(loginCard, "Input_Nickname");
        _btnRegister = FindComponent<Button>(loginCard, "Btn_Register");
        _subtitleText = FindComponent<Text>(loginCard, "Subtitle_Text");
        _btnLoginText = FindComponent<Text>(loginCard, "Btn_Login/Text");
        _btnRegisterText = FindComponent<Text>(loginCard, "Btn_Register/Text");
        _statusText = FindComponent<Text>(loginCard, "Text_Status");
    }

    private void EnsureGeneratedAuthUi()
    {
        Transform loginCard = canvasLogin != null ? canvasLogin.transform.Find("Panel_LoginCard") : null;
        if (loginCard == null)
        {
            return;
        }

        if (_inputPassword != null)
        {
            _inputPassword.contentType = InputField.ContentType.Password;
            _inputPassword.lineType = InputField.LineType.SingleLine;
            _inputPassword.ForceLabelUpdate();
        }

        if (_inputId != null)
        {
            _inputId.lineType = InputField.LineType.SingleLine;
        }

        if (_inputNickname == null && _inputId != null)
        {
            GameObject nicknameGo = Instantiate(_inputId.gameObject, loginCard, false);
            nicknameGo.name = "Input_Nickname";
            nicknameGo.transform.SetSiblingIndex(_inputPassword != null
                ? _inputPassword.transform.GetSiblingIndex() + 1
                : _inputId.transform.GetSiblingIndex() + 1);

            _inputNickname = nicknameGo.GetComponent<InputField>();
            _inputNickname.contentType = InputField.ContentType.Standard;
            _inputNickname.lineType = InputField.LineType.SingleLine;
            _inputNickname.characterLimit = 12;
            _inputNickname.text = string.Empty;

            Text nicknameText = nicknameGo.transform.Find("Text")?.GetComponent<Text>();
            if (nicknameText != null)
            {
                nicknameText.text = string.Empty;
            }

            Text placeholder = nicknameGo.transform.Find("Placeholder")?.GetComponent<Text>();
            if (placeholder != null)
            {
                placeholder.text = "\ub2c9\ub124\uc784";
            }
        }

        if (_statusText == null)
        {
            GameObject statusGo = new GameObject("Text_Status", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            statusGo.transform.SetParent(loginCard, false);
            _statusText = statusGo.GetComponent<Text>();

            Text styleSource = _subtitleText ?? _btnLoginText;
            if (styleSource != null)
            {
                _statusText.font = styleSource.font;
                _statusText.fontStyle = styleSource.fontStyle;
                _statusText.material = styleSource.material;
            }

            _statusText.fontSize = 18;
            _statusText.alignment = TextAnchor.MiddleCenter;
            _statusText.horizontalOverflow = HorizontalWrapMode.Wrap;
            _statusText.verticalOverflow = VerticalWrapMode.Overflow;
        }

        if (_statusText != null)
        {
            _statusText.text = string.Empty;
        }
    }

    private void ResolveCoinLabels()
    {
        _lobbyCoinText = canvasLobby != null
            ? FindComponent<Text>(canvasLobby.transform, "Panel_Coin/Text")
            : null;
        _shopCoinText = canvasShop != null
            ? FindComponent<Text>(canvasShop.transform, "CoinDisplay/CoinValue")
            : null;
        _garageCoinText = canvasGarage != null
            ? FindComponent<Text>(canvasGarage.transform, "Panel_Coin/Text")
            : null;
    }

    private void ResolveShopUiReferences()
    {
        _shopCards.Clear();

        if (canvasShop == null)
        {
            return;
        }

        Transform shopRoot = canvasShop.transform;
        _shopHintText = FindComponent<Text>(shopRoot, "ProgressBar/Label")
            ?? FindComponent<Text>(shopRoot, "Footer/HintText");

        RegisterShopCard(shopRoot, "shield", "실드", 1L, 500, "장애물 1회 방어", "Panel_Shop/ShopCardsContainer/Item_Shield", "Btn_Buy_Shield");
        RegisterShopCard(shopRoot, "booster", "부스터", 2L, 700, "일정 시간 속도 증가", "Panel_Shop/ShopCardsContainer/Item_Booster", "Btn_Buy_Booster");
        RegisterShopCard(shopRoot, "coin_magnet", "코인 자석", 3L, 1000, "일정 시간 코인 자동 획득", "Panel_Shop/ShopCardsContainer/Item_Magnet", "Btn_Buy_Magnet");
    }

    private void RegisterShopCard(
        Transform shopRoot,
        string itemKey,
        string displayName,
        long fallbackItemId,
        int fallbackPrice,
        string fallbackDescription,
        string cardPath,
        string buttonName)
    {
        Transform cardRoot = shopRoot != null ? shopRoot.Find(cardPath) : null;
        if (cardRoot == null)
        {
            return;
        }

        Text descriptionText = FindComponent<Text>(cardRoot, "CardOverlay/DescriptionText");

        _shopCards[itemKey] = new ShopItemCard
        {
            Root = cardRoot.gameObject,
            ItemKey = itemKey,
            DisplayName = displayName,
            ItemId = fallbackItemId,
            Price = fallbackPrice,
            Description = string.IsNullOrWhiteSpace(fallbackDescription)
                ? (descriptionText != null ? descriptionText.text : string.Empty)
                : fallbackDescription,
            TagText = FindComponent<Text>(cardRoot, "CardOverlay/Tag/Label"),
            TitleText = FindComponent<Text>(cardRoot, "CardOverlay/TitleText"),
            SubtitleText = FindComponent<Text>(cardRoot, "CardOverlay/SubtitleText"),
            DescriptionText = descriptionText,
            PriceText = FindComponent<Text>(cardRoot, "CardOverlay/PriceChip/PriceText"),
            OwnedCountText = EnsureOwnedCountText(cardRoot),
            BuyButton = FindComponent<Button>(cardRoot, $"CardOverlay/{buttonName}"),
            BuyButtonLabel = FindComponent<Text>(cardRoot, $"CardOverlay/{buttonName}/Label"),
            BuyButtonImage = FindComponent<Image>(cardRoot, $"CardOverlay/{buttonName}"),
        };
    }

    private Text EnsureOwnedCountText(Transform cardRoot)
    {
        if (cardRoot == null)
        {
            return null;
        }

        Text existing = FindComponent<Text>(cardRoot, "CardOverlay/OwnedCountText");
        Transform overlay = cardRoot.Find("CardOverlay");
        if (overlay == null)
        {
            return null;
        }

        Text styleSource = FindComponent<Text>(cardRoot, "CardOverlay/PriceChip/PriceText")
            ?? FindComponent<Text>(cardRoot, "CardOverlay/Tag/Label")
            ?? FindComponent<Text>(cardRoot, "CardOverlay/SubtitleText");

        Text text = existing;
        if (text == null)
        {
            GameObject textGo = new GameObject("OwnedCountText", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textGo.transform.SetParent(overlay, false);
            text = textGo.GetComponent<Text>();
        }

        RectTransform rect = text.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0f, 166f);
        rect.sizeDelta = new Vector2(220f, 24f);

        if (styleSource != null)
        {
            text.font = styleSource.font;
            text.fontStyle = styleSource.fontStyle;
            text.material = styleSource.material;
            text.fontSize = Mathf.Max(16, styleSource.fontSize - 4);
        }
        else
        {
            text.fontSize = 16;
        }

        text.alignment = TextAnchor.MiddleCenter;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.color = new Color(0.20f, 0.24f, 0.32f, 0.92f);
        text.text = "보유 x0";
        return text;
    }

    private void BindShopButtons()
    {
        foreach (KeyValuePair<string, ShopItemCard> entry in _shopCards)
        {
            string itemKey = entry.Key;
            ShopItemCard card = entry.Value;
            if (card?.BuyButton == null)
            {
                continue;
            }

            card.BuyButton.onClick.RemoveAllListeners();
            card.BuyButton.onClick.AddListener(() => TryPurchaseShopItem(itemKey));
        }
    }

    private void RefreshShopUi(string actionMessage = null, bool isError = false)
    {
        bool hasSession = HasAuthenticatedSession();

        foreach (ShopItemCard card in _shopCards.Values)
        {
            if (card == null)
            {
                continue;
            }

            if (card.Root != null)
            {
                card.Root.SetActive(true);
            }

            if (card.TagText != null)
            {
                card.TagText.text = GetShopTagText(card.ItemKey);
            }

            if (card.TitleText != null)
            {
                card.TitleText.text = card.DisplayName;
            }

            if (card.SubtitleText != null)
            {
                card.SubtitleText.text = GetShopSubtitleText(card.ItemKey);
            }

            if (card.DescriptionText != null && !string.IsNullOrWhiteSpace(card.Description))
            {
                card.DescriptionText.text = card.Description;
            }

            if (card.PriceText != null)
            {
                card.PriceText.text = card.Price > 0 ? $"{card.Price:N0} 코인" : "--";
            }

            if (card.PriceText != null && card.Price > 0)
            {
                card.PriceText.text = $"{card.Price:N0}";
            }

            if (card.OwnedCountText != null)
            {
                card.OwnedCountText.text = $"보유 x{GetOwnedItemCount(card.ItemKey)}";
            }

            bool canAfford = card.Price <= 0 || playerCoins >= card.Price;
            if (card.BuyButtonLabel != null)
            {
                card.BuyButtonLabel.text = _shopBusy
                    ? "처리 중"
                    : (!card.Purchasable
                        ? "잠김"
                        : (canAfford ? "구매" : "코인 부족"));
            }

            bool canBuy = hasSession && !_shopBusy && card.Purchasable && card.ItemId > 0 && canAfford;
            if (card.BuyButton != null)
            {
                card.BuyButton.interactable = canBuy;
            }

            if (card.BuyButtonImage != null)
            {
                card.BuyButtonImage.color = canBuy
                    ? GetShopButtonColor(card.ItemKey)
                    : new Color(0.27f, 0.27f, 0.30f, 0.86f);
            }
        }

        if (_shopHintText == null)
        {
            return;
        }

        string defaultMessage = hasSession
            ? "BE 연동 품목 3종을 확인하세요."
            : "로그인 후 상점을 이용할 수 있습니다.";

        _shopHintText.text = string.IsNullOrWhiteSpace(actionMessage)
            ? defaultMessage
            : actionMessage;
        _shopHintText.color = isError
            ? new Color(0.72f, 0.24f, 0.24f, 0.98f)
            : new Color(0.18f, 0.22f, 0.29f, 0.96f);
    }

    private IEnumerator SyncShopCatalogFlow(bool silent)
    {
        if (_shopBusy)
        {
            yield break;
        }

        if (!HasAuthenticatedSession())
        {
            if (!silent)
            {
                RefreshShopUi("로그인 후 상점을 이용할 수 있습니다.", true);
            }

            yield break;
        }

        _shopBusy = true;
        RefreshShopUi("BE 상점 메뉴를 불러오는 중...", false);

        LobbyAuthApi.GameShopData shopData = null;
        LobbyAuthApi.ApiError shopError = null;

        yield return StartCoroutine(_authApi.GetShopItems(
            data => shopData = data,
            error => shopError = error));

        _shopBusy = false;

        if (shopError != null)
        {
            RefreshShopUi(shopError.Message, true);
            yield break;
        }

        if (shopData?.Items == null)
        {
            RefreshShopUi("상점 정보가 비어 있습니다.", true);
            yield break;
        }

        ApplyShopCatalog(shopData);
        RefreshShopUi("BE 상점 메뉴를 불러왔습니다.", false);
    }

    private string GetShopTagText(string itemKey)
    {
        switch (itemKey)
        {
            case "shield":
                return "DEFENSE";
            case "booster":
                return "FEATURED";
            case "coin_magnet":
                return "COLLECT";
            default:
                return "ITEM";
        }
    }

    private string GetShopSubtitleText(string itemKey)
    {
        switch (itemKey)
        {
            case "shield":
                return "1회 방어 소모품";
            case "booster":
                return "일시 속도 강화";
            case "coin_magnet":
                return "자동 회수 보조";
            default:
                return itemKey;
        }
    }

    private Color GetShopButtonColor(string itemKey)
    {
        return new Color(0.18f, 0.20f, 0.25f, 0.96f);
    }

    private void ApplyShopCatalog(LobbyAuthApi.GameShopData shopData)
    {
        if (shopData?.Items == null)
        {
            return;
        }

        foreach (ShopItemCard localCard in _shopCards.Values)
        {
            if (localCard?.Root != null)
            {
                localCard.Root.SetActive(false);
            }
        }

        foreach (LobbyAuthApi.ShopItemData item in shopData.Items)
        {
            if (item == null || !TryGetShopCard(item.ItemName, out ShopItemCard card))
            {
                continue;
            }

            if (card.Root != null)
            {
                card.Root.SetActive(true);
            }

            card.ItemId = item.ItemId;
            card.Price = item.Price;
            card.Purchasable = item.Purchasable;

            if (!string.IsNullOrWhiteSpace(item.Description))
            {
                card.Description = item.Description;
            }
        }
    }

    private IEnumerator SyncInventoryFlow(bool silent)
    {
        if (!HasAuthenticatedSession())
        {
            _ownedItemCounts.Clear();
            RefreshShopUi(silent ? null : "로그인 후 상점을 이용할 수 있습니다.", !silent);
            yield break;
        }

        LobbyAuthApi.GameInventoryData inventoryData = null;
        LobbyAuthApi.ApiError inventoryError = null;

        yield return StartCoroutine(_authApi.GetInventory(
            data => inventoryData = data,
            error => inventoryError = error));

        if (inventoryError != null)
        {
            if (!silent)
            {
                RefreshShopUi(inventoryError.Message, true);
            }

            yield break;
        }

        ApplyInventory(inventoryData);
        RefreshShopUi(silent ? null : "보유 아이템 수량을 동기화했습니다.", false);
    }

    private void ApplyInventory(LobbyAuthApi.GameInventoryData inventoryData)
    {
        _ownedItemCounts.Clear();

        if (inventoryData?.Items == null)
        {
            return;
        }

        foreach (LobbyAuthApi.InventoryItemData item in inventoryData.Items)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.ItemName))
            {
                continue;
            }

            string normalizedName = NormalizeShopItemKey(item.ItemName);
            if (string.IsNullOrWhiteSpace(normalizedName))
            {
                continue;
            }

            _ownedItemCounts[normalizedName] = Mathf.Max(0, item.Quantity);
        }
    }

    private int GetOwnedItemCount(string itemKey)
    {
        if (string.IsNullOrWhiteSpace(itemKey))
        {
            return 0;
        }

        return _ownedItemCounts.TryGetValue(NormalizeShopItemKey(itemKey), out int count)
            ? count
            : 0;
    }

    private static string NormalizeShopItemKey(string itemKey)
    {
        if (string.IsNullOrWhiteSpace(itemKey))
        {
            return string.Empty;
        }

        string normalized = itemKey.Trim().ToLowerInvariant();
        return normalized == "magnet" ? "coin_magnet" : normalized;
    }

    private bool TryGetShopCard(string backendItemName, out ShopItemCard card)
    {
        card = null;
        if (string.IsNullOrWhiteSpace(backendItemName))
        {
            return false;
        }

        string normalizedName = NormalizeShopItemKey(backendItemName);
        switch (normalizedName)
        {
            case "shield":
            case "booster":
            case "coin_magnet":
                return _shopCards.TryGetValue(normalizedName, out card);
            default:
                return _shopCards.TryGetValue(normalizedName, out card);
        }
    }

    private void TryPurchaseShopItem(string itemKey)
    {
        if (_shopBusy)
        {
            return;
        }

        if (!HasAuthenticatedSession())
        {
            RefreshShopUi("로그인 후 상점을 이용할 수 있습니다.", true);
            return;
        }

        if (!_shopCards.TryGetValue(itemKey, out ShopItemCard card) || card == null)
        {
            RefreshShopUi("상점 항목을 찾을 수 없습니다.", true);
            return;
        }

        if (card.ItemId <= 0)
        {
            RefreshShopUi("BE 상품 매핑이 누락되었습니다.", true);
            return;
        }

        if (!card.Purchasable)
        {
            RefreshShopUi($"{card.DisplayName} 상품은 현재 구매할 수 없습니다.", true);
            return;
        }

        StartCoroutine(PurchaseShopItemFlow(card));
    }

    private IEnumerator PurchaseShopItemFlow(ShopItemCard card)
    {
        _shopBusy = true;
        RefreshShopUi($"{card.DisplayName} 구매 중...", false);

        LobbyAuthApi.ShopPurchaseData purchaseData = null;
        LobbyAuthApi.ApiError purchaseError = null;

        yield return StartCoroutine(_authApi.PurchaseShopItem(
            card.ItemId,
            1,
            data => purchaseData = data,
            error => purchaseError = error));

        _shopBusy = false;

        if (purchaseError != null)
        {
            RefreshShopUi(purchaseError.Message, true);
            yield break;
        }

        if (purchaseData == null)
        {
            RefreshShopUi("구매 응답이 비어 있습니다.", true);
            yield break;
        }

        playerCoins = purchaseData.RemainingCoin;
        UpdateCoinLabels(playerCoins);
        StoreLastKnownCoin(playerCoins);
        _ownedItemCounts[NormalizeShopItemKey(card.ItemKey)] = GetOwnedItemCount(card.ItemKey) + Mathf.Max(1, purchaseData.PurchasedQuantity);
        RefreshGarageUi();
        StartCoroutine(SyncInventoryFlow(true));
        RefreshShopUi($"{card.DisplayName} 구매 완료 · 남은 코인 {playerCoins:N0}", false);
    }

    private bool HasAuthenticatedSession()
    {
        if (_authApi != null && _authApi.HasSession)
        {
            return true;
        }

        return TryGetKnownAuthenticatedUser(out _, out _, out _, out _);
    }

    private void RestoreAuthStateFromKnownUser()
    {
        if (_authApi != null && _authApi.HasSession)
        {
            return;
        }

        if (!TryGetKnownAuthenticatedUser(
                out string loginId,
                out string nickname,
                out long userId,
                out int coinBalance))
        {
            return;
        }

        _currentLoginId = loginId;
        _currentNickname = nickname;
        _currentUserId = userId;
        playerCoins = coinBalance;

        string restoredBaseUrl = !string.IsNullOrWhiteSpace(BackendRuntimeSession.BaseUrl)
            ? BackendRuntimeSession.BaseUrl
            : backendBaseUrl;
        string restoredCookie = _authApi != null && !string.IsNullOrWhiteSpace(_authApi.SessionCookie)
            ? _authApi.SessionCookie
            : BackendRuntimeSession.SessionCookie;

        _authApi?.RestoreSession(restoredCookie, _currentUserId, _currentLoginId, _currentNickname);
        BackendRuntimeSession.Configure(
            restoredBaseUrl,
            _currentUserId,
            restoredCookie,
            _currentLoginId,
            _currentNickname);

        if (UserDataManager.Instance != null)
        {
            UserDataManager.Instance.SetUser(_currentLoginId, _currentNickname, playerCoins, _currentUserId);
        }

        _garageState.coinBalance = playerCoins;
        StoreLastKnownCoin(playerCoins);
        UpdateCoinLabels(playerCoins);

        if (!_hasCompletedLogin)
        {
            ApplyLobbyMainState();
            SetStatus(string.Empty, false);
            _hasCompletedLogin = true;
        }
    }

    private bool TryGetKnownAuthenticatedUser(
        out string loginId,
        out string nickname,
        out long userId,
        out int coinBalance)
    {
        UserData currentUser = UserDataManager.Instance != null ? UserDataManager.Instance.CurrentUser : null;

        loginId = !string.IsNullOrWhiteSpace(_currentLoginId)
            ? _currentLoginId.Trim()
            : (!string.IsNullOrWhiteSpace(BackendRuntimeSession.LoginId)
                ? BackendRuntimeSession.LoginId.Trim()
                : string.Empty);

        nickname = !string.IsNullOrWhiteSpace(_currentNickname)
            ? _currentNickname.Trim()
            : (!string.IsNullOrWhiteSpace(BackendRuntimeSession.Nickname)
                ? BackendRuntimeSession.Nickname.Trim()
                : (currentUser != null &&
                   string.Equals(currentUser.userId, loginId, StringComparison.Ordinal)
                    ? currentUser.nickname
                    : string.Empty));

        userId = _currentUserId > 0
            ? _currentUserId
            : (BackendRuntimeSession.UserId > 0
                ? BackendRuntimeSession.UserId
                : -1);

        coinBalance = currentUser != null &&
            string.Equals(currentUser.userId, loginId, StringComparison.Ordinal)
            ? Mathf.Max(currentUser.coin, 0)
            : Mathf.Max(playerCoins, 0);

        if (string.IsNullOrWhiteSpace(loginId) ||
            string.Equals(loginId, "guest", StringComparison.OrdinalIgnoreCase) ||
            userId <= 0)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(nickname))
        {
            nickname = loginId;
        }

        return true;
    }

    private void EnsureGeneratedGarageUi()
    {
        if (canvasGarage == null)
        {
            return;
        }

        Transform garageRoot = canvasGarage.transform;
        Transform panel = garageRoot.Find("UserIdPanel");
        if (panel == null)
        {
            GameObject panelGo = new GameObject("UserIdPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panelGo.transform.SetParent(garageRoot, false);
            RectTransform panelRect = panelGo.GetComponent<RectTransform>();
            SetAnchors(panelRect, 0.24f, 0.47f, 0.76f, 0.56f);

            Image panelImage = panelGo.GetComponent<Image>();
            panelImage.color = new Color(0.05f, 0.08f, 0.13f, 0.58f);

            if (_inputId != null)
            {
                GameObject inputGo = Instantiate(_inputId.gameObject, panelGo.transform, false);
                inputGo.name = "Input_UserId";
                RectTransform inputRect = inputGo.GetComponent<RectTransform>();
                SetAnchors(inputRect, 0.03f, 0.36f, 0.66f, 0.88f);

                InputField inputField = inputGo.GetComponent<InputField>();
                if (inputField != null)
                {
                    inputField.contentType = InputField.ContentType.IntegerNumber;
                    inputField.lineType = InputField.LineType.SingleLine;
                    inputField.characterLimit = 12;
                    inputField.text = string.Empty;
                    inputField.ForceLabelUpdate();
                }

                Text inputText = inputGo.transform.Find("Text")?.GetComponent<Text>();
                if (inputText != null)
                {
                    inputText.text = string.Empty;
                    inputText.alignment = TextAnchor.MiddleLeft;
                }

                Text placeholder = inputGo.transform.Find("Placeholder")?.GetComponent<Text>();
                if (placeholder != null)
                {
                    placeholder.text = "repair userId";
                    placeholder.alignment = TextAnchor.MiddleLeft;
                }
            }

            Button buttonTemplate = _btnRegister != null ? _btnRegister : btnLogin;
            if (buttonTemplate != null)
            {
                GameObject buttonGo = Instantiate(buttonTemplate.gameObject, panelGo.transform, false);
                buttonGo.name = "Btn_ApplyUserId";
                RectTransform buttonRect = buttonGo.GetComponent<RectTransform>();
                SetAnchors(buttonRect, 0.69f, 0.36f, 0.97f, 0.88f);

                Text buttonLabel = buttonGo.transform.Find("Text")?.GetComponent<Text>();
                if (buttonLabel != null)
                {
                    buttonLabel.text = "연결";
                    buttonLabel.fontSize = 22;
                }
            }

            GameObject hintGo = new GameObject("Text_Hint", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            hintGo.transform.SetParent(panelGo.transform, false);
            RectTransform hintRect = hintGo.GetComponent<RectTransform>();
            SetAnchors(hintRect, 0.04f, 0.06f, 0.96f, 0.28f);

            Text hintText = hintGo.GetComponent<Text>();
            Text styleSource = _statusText ?? _subtitleText ?? _btnRegisterText;
            if (styleSource != null)
            {
                hintText.font = styleSource.font;
                hintText.fontStyle = styleSource.fontStyle;
                hintText.material = styleSource.material;
            }

            hintText.fontSize = 16;
            hintText.alignment = TextAnchor.MiddleCenter;
            hintText.horizontalOverflow = HorizontalWrapMode.Wrap;
            hintText.verticalOverflow = VerticalWrapMode.Overflow;
            hintText.color = new Color(0.90f, 0.94f, 0.98f, 0.92f);
            hintText.text = "로그인 계정의 repair userId를 한 번만 입력하면 저장됩니다";

            panel = panelGo.transform;
        }

        _garageUserIdPanel = panel.gameObject;
        _garageUserIdInput = FindComponent<InputField>(panel, "Input_UserId");
        _garageUserIdApplyButton = FindComponent<Button>(panel, "Btn_ApplyUserId");
        _garageUserIdApplyLabel = FindComponent<Text>(panel, "Btn_ApplyUserId/Text");
        _garageUserIdHintText = FindComponent<Text>(panel, "Text_Hint");
    }

    private void ResolveGarageUiReferences()
    {
        _garageCards.Clear();

        if (canvasGarage == null)
        {
            return;
        }

        Transform garageRoot = canvasGarage.transform;
        _garageProgressLabel = FindComponent<Text>(garageRoot, "ProgressBar/Label");
        _garageProgressFill = FindComponent<Image>(garageRoot, "ProgressBar/Fill");
        _garageBulkUpgradeButton = FindComponent<Button>(garageRoot, "Footer/Btn_RepairAll");
        _garageBulkUpgradeLabel = FindComponent<Text>(garageRoot, "Footer/Btn_RepairAll/Label");

        RegisterGarageCard(garageRoot, GaragePart.Engine, "ENGINE", "엔진", 1000, "부스터 지속시간 증가", "GarageContent/CardsContainer/Card_Engine", "Btn_Upgrade_Engine");
        RegisterGarageCard(garageRoot, GaragePart.Wheel, "WHEEL", "바퀴", 1500, "기본 속도 증가", "GarageContent/CardsContainer/Card_Wheel", "Btn_Upgrade_Wheel");
        RegisterGarageCard(garageRoot, GaragePart.Armor, "ARMOR", "장갑판", 2000, "최대 HP 증가", "GarageContent/CardsContainer/Card_Armor", "Btn_Upgrade_Armor");

        Transform magnetCard = garageRoot.Find("GarageContent/CardsContainer/Card_Magnet");
        if (magnetCard != null)
        {
            magnetCard.gameObject.SetActive(false);
        }

        if (_garageBulkUpgradeButton != null)
        {
            _garageBulkUpgradeButton.gameObject.SetActive(false);
        }
    }

    private void RegisterGarageCard(
        Transform garageRoot,
        GaragePart part,
        string partName,
        string displayName,
        int repairCost,
        string effectText,
        string cardPath,
        string buttonName)
    {
        Transform cardRoot = garageRoot != null ? garageRoot.Find(cardPath) : null;
        if (cardRoot == null)
        {
            return;
        }

        _garageCards[part] = new GarageUpgradeCard
        {
            Root = cardRoot.gameObject,
            Part = part,
            PartName = partName,
            DisplayName = displayName,
            RepairCost = repairCost,
            EffectText = effectText,
            TitleText = FindComponent<Text>(cardRoot, "TitleText"),
            DescriptionText = FindComponent<Text>(cardRoot, "DescriptionText"),
            LevelText = FindComponent<Text>(cardRoot, "LevelText"),
            PriceText = FindComponent<Text>(cardRoot, "PriceRow/PriceText"),
            UpgradeButton = FindComponent<Button>(cardRoot, buttonName),
            UpgradeButtonLabel = FindComponent<Text>(cardRoot, $"{buttonName}/Label"),
            UpgradeButtonImage = FindComponent<Image>(cardRoot, buttonName),
        };
    }

    private void BindGarageButtons()
    {
        foreach (KeyValuePair<GaragePart, GarageUpgradeCard> entry in _garageCards)
        {
            GaragePart part = entry.Key;
            GarageUpgradeCard card = entry.Value;
            if (card?.UpgradeButton == null)
            {
                continue;
            }

            card.UpgradeButton.onClick.RemoveAllListeners();
            card.UpgradeButton.onClick.AddListener(() => TryUpgradeGaragePart(part));
        }

        if (_garageUserIdApplyButton != null)
        {
            _garageUserIdApplyButton.onClick.RemoveAllListeners();
            _garageUserIdApplyButton.onClick.AddListener(HandleApplyGarageUserId);
        }

        if (_garageBulkUpgradeButton != null)
        {
            _garageBulkUpgradeButton.onClick.RemoveAllListeners();
            _garageBulkUpgradeButton.gameObject.SetActive(false);
        }
    }

    private void RefreshGarageUi(string actionMessage = null, bool isError = false)
    {
        bool hasRepairSession = HasAuthenticatedSession();

        foreach (GarageUpgradeCard card in _garageCards.Values)
        {
            if (card == null || card.Root == null)
            {
                continue;
            }

            card.Root.SetActive(true);
            int currentLevel = GetGarageLevel(card.Part);

            if (card.TitleText != null)
            {
                card.TitleText.text = $"{card.DisplayName} ({card.PartName})";
            }

            if (card.DescriptionText != null)
            {
                card.DescriptionText.text = GetGarageDescription(card.Part, currentLevel);
            }

            if (card.LevelText != null)
            {
                card.LevelText.text = $"Lv {currentLevel}";
            }

            if (card.PriceText != null)
            {
                card.PriceText.text = $"{card.RepairCost:N0} 코인";
            }

            bool canRepair = !_garageBusy && hasRepairSession && playerCoins >= card.RepairCost;

            if (card.UpgradeButtonLabel != null)
            {
                card.UpgradeButtonLabel.text = _garageBusy
                    ? "처리 중"
                    : (playerCoins >= card.RepairCost ? "수리" : "코인 부족");
            }

            if (card.UpgradeButton != null)
            {
                card.UpgradeButton.interactable = canRepair;
            }

            if (card.UpgradeButtonImage != null)
            {
                card.UpgradeButtonImage.color = canRepair
                    ? new Color(0.18f, 0.15f, 0.18f, 0.96f)
                    : new Color(0.27f, 0.27f, 0.30f, 0.86f);
            }
        }

        if (_garageUserIdPanel != null)
        {
            _garageUserIdPanel.SetActive(false);
        }

        if (_garageUserIdInput != null)
        {
            if (hasRepairSession && string.IsNullOrWhiteSpace(_garageUserIdInput.text))
            {
                _garageUserIdInput.text = string.Empty;
            }

            _garageUserIdInput.interactable = false;
        }

        if (_garageUserIdApplyButton != null)
        {
            _garageUserIdApplyButton.interactable = false;
        }

        if (_garageUserIdApplyLabel != null)
        {
            _garageUserIdApplyLabel.text = _garageBusy ? "처리 중" : "연결";
        }

        if (_garageUserIdHintText != null)
        {
            _garageUserIdHintText.text = hasRepairSession
                ? "현재 로그인 세션으로 정비소를 이용합니다."
                : "로그인 후 정비소를 이용할 수 있습니다.";
        }

        UpdateGarageProgress(actionMessage, isError);
    }

    private void UpdateGarageProgress(string actionMessage, bool isError = false)
    {
        if (_garageProgressFill != null)
        {
            RectTransform fillRect = _garageProgressFill.rectTransform;
            fillRect.anchorMax = new Vector2(1f, fillRect.anchorMax.y);
            _garageProgressFill.color = isError
                ? new Color(0.72f, 0.28f, 0.24f, 0.92f)
                : new Color(0.94f, 0.76f, 0.28f, 0.92f);
        }

        if (_garageProgressLabel == null)
        {
            return;
        }

        string defaultMessage = HasAuthenticatedSession()
            ? "BE 정비 메뉴를 확인하세요."
            : "로그인 후 정비소를 이용할 수 있습니다.";

        _garageProgressLabel.text = string.IsNullOrWhiteSpace(actionMessage)
            ? defaultMessage
            : actionMessage;
        _garageProgressLabel.color = isError
            ? new Color(0.28f, 0.12f, 0.12f, 1f)
            : new Color(0.20f, 0.22f, 0.27f, 1f);
    }

    private void TryUpgradeGaragePart(GaragePart part)
    {
        if (_garageBusy)
        {
            return;
        }

        if (!_garageCards.TryGetValue(part, out GarageUpgradeCard card))
        {
            return;
        }

        if (!HasAuthenticatedSession())
        {
            RefreshGarageUi("로그인 후 정비소를 이용할 수 있습니다.", true);
            return;
        }

        StartCoroutine(RepairGaragePartFlow(card));
    }

    private void HandleApplyGarageUserId()
    {
        RefreshGarageUi("정비소는 현재 로그인 세션으로 연결됩니다.", false);
    }

    private IEnumerator RepairGaragePartFlow(GarageUpgradeCard card)
    {
        _garageBusy = true;
        RefreshGarageUi($"{card.DisplayName} 수리 요청 중...", false);

        LobbyAuthApi.UfoRepairData repairData = null;
        LobbyAuthApi.ApiError repairError = null;

        yield return StartCoroutine(_authApi.RepairUfo(
            card.PartName,
            data => repairData = data,
            error => repairError = error));

        _garageBusy = false;

        if (repairError != null)
        {
            RefreshGarageUi(repairError.Message, true);
            yield break;
        }

        if (repairData == null)
        {
            RefreshGarageUi("수리 응답이 비어 있습니다.", true);
            yield break;
        }

        playerCoins = repairData.RemainingCoin;
        UpdateCoinLabels(playerCoins);
        SetGarageLevel(card.Part, GetGarageLevel(card.Part) + 1);
        StoreLastKnownCoin(playerCoins);
        RefreshGarageUi($"{card.DisplayName} 수리 완료 · {repairData.Effect}");
    }

    private bool TryResolveRepairUserId(out long userId)
    {
        if (_currentUserId > 0)
        {
            userId = _currentUserId;
            return true;
        }

        if (repairUserIdOverride > 0)
        {
            userId = repairUserIdOverride;
            return true;
        }

        userId = -1;
        return false;
    }

    private void LoadPlayerState(int fallbackCoin)
    {
        playerCoins = fallbackCoin;
        _garageState = GarageUpgradeState.CreateDefault(fallbackCoin);
        _garageState.coinBalance = fallbackCoin;
        ClampGarageState();
        UpdateCoinLabels(playerCoins);
    }

    private void PersistPlayerState()
    {
        if (_garageState == null)
        {
            _garageState = GarageUpgradeState.CreateDefault(playerCoins);
        }

        _garageState.coinBalance = playerCoins;
        ClampGarageState();
    }

    private void RegisterGarageCard(Transform garageRoot, GaragePart part, string displayName, string cardPath, string buttonName)
    {
    }

    private string BuildPlayerStateKey()
    {
        string normalizedLoginId = string.IsNullOrWhiteSpace(_currentLoginId)
            ? GuestPlayerId
            : _currentLoginId.Trim().ToLowerInvariant();

        return PlayerStatePrefsPrefix + normalizedLoginId;
    }

    private void ClampGarageState()
    {
        if (_garageState == null)
        {
            return;
        }

        _garageState.engineLevel = Mathf.Clamp(_garageState.engineLevel, 1, GarageMaxLevel);
        _garageState.wheelLevel = Mathf.Clamp(_garageState.wheelLevel, 1, GarageMaxLevel);
        _garageState.armorLevel = Mathf.Clamp(_garageState.armorLevel, 1, GarageMaxLevel);
        _garageState.magnetLevel = Mathf.Clamp(_garageState.magnetLevel, 1, GarageMaxLevel);
    }

    private bool HasUpgradeableGaragePart()
    {
        foreach (GaragePart part in GetGarageUpgradeOrder())
        {
            if (_garageCards.TryGetValue(part, out GarageUpgradeCard card) && card != null && playerCoins >= card.RepairCost)
            {
                return true;
            }
        }

        return false;
    }

    private void TryBulkUpgradeGarage()
    {
        foreach (GaragePart part in GetGarageUpgradeOrder())
        {
            if (_garageCards.TryGetValue(part, out GarageUpgradeCard card) && card != null && playerCoins >= card.RepairCost)
            {
                TryUpgradeGaragePart(part);
                return;
            }
        }
    }

    private static GaragePart[] GetGarageUpgradeOrder()
    {
        return new[]
        {
            GaragePart.Engine,
            GaragePart.Wheel,
            GaragePart.Armor,
        };
    }

    private int GetGarageLevel(GaragePart part)
    {
        return part switch
        {
            GaragePart.Engine => _garageState != null ? _garageState.engineLevel : 1,
            GaragePart.Wheel => _garageState != null ? _garageState.wheelLevel : 1,
            GaragePart.Armor => _garageState != null ? _garageState.armorLevel : 1,
            GaragePart.Magnet => _garageState != null ? _garageState.magnetLevel : 1,
            _ => 1,
        };
    }

    private void SetGarageLevel(GaragePart part, int level)
    {
        if (_garageState == null)
        {
            _garageState = GarageUpgradeState.CreateDefault(playerCoins);
        }

        int clampedLevel = Mathf.Clamp(level, 1, GarageMaxLevel);
        switch (part)
        {
            case GaragePart.Engine:
                _garageState.engineLevel = clampedLevel;
                break;
            case GaragePart.Wheel:
                _garageState.wheelLevel = clampedLevel;
                break;
            case GaragePart.Armor:
                _garageState.armorLevel = clampedLevel;
                break;
            case GaragePart.Magnet:
                _garageState.magnetLevel = clampedLevel;
                break;
        }
    }

    private static int GetGarageUpgradePrice(int currentLevel)
    {
        return currentLevel switch
        {
            <= 1 => 1000,
            2 => 1500,
            _ => 2000,
        };
    }

    private static string GetGarageDescription(GaragePart part, int level)
    {
        int appliedLevel = Mathf.Max(level - 1, 0);

        return part switch
        {
            GaragePart.Engine => appliedLevel > 0
                ? $"부스터 지속시간 +{appliedLevel * 2}초"
                : "부스터 지속시간 증가",
            GaragePart.Wheel => appliedLevel > 0
                ? $"기본 속도 +{appliedLevel * 1f:0.0} 증가"
                : "기본 속도 증가",
            GaragePart.Armor => appliedLevel > 0
                ? $"최대 HP +{appliedLevel * 20} 증가"
                : "최대 HP 증가",
            GaragePart.Magnet => "보유 효과 없음",
            _ => string.Empty,
        };
    }

    private void ResolveGarageUiReferencesLegacy()
    {
        _garageCards.Clear();

        if (canvasGarage == null)
        {
            return;
        }

        Transform garageRoot = canvasGarage.transform;
        _garageProgressLabel = FindComponent<Text>(garageRoot, "ProgressBar/Label");
        _garageProgressFill = FindComponent<Image>(garageRoot, "ProgressBar/Fill");
        _garageBulkUpgradeButton = FindComponent<Button>(garageRoot, "Footer/Btn_RepairAll");
        _garageBulkUpgradeLabel = FindComponent<Text>(garageRoot, "Footer/Btn_RepairAll/Label");

        RegisterGarageCard(garageRoot, GaragePart.Engine, "엔진", "GarageContent/CardsContainer/Card_Engine", "Btn_Upgrade_Engine");
        RegisterGarageCard(garageRoot, GaragePart.Wheel, "바퀴", "GarageContent/CardsContainer/Card_Wheel", "Btn_Upgrade_Wheel");
        RegisterGarageCard(garageRoot, GaragePart.Armor, "장갑판", "GarageContent/CardsContainer/Card_Armor", "Btn_Upgrade_Armor");
        RegisterGarageCard(garageRoot, GaragePart.Magnet, "코인 자석", "GarageContent/CardsContainer/Card_Magnet", "Btn_Upgrade_Magnet");
    }

    private void RegisterGarageCardLegacy(Transform garageRoot, GaragePart part, string displayName, string cardPath, string buttonName)
    {
        Transform cardRoot = garageRoot != null ? garageRoot.Find(cardPath) : null;
        if (cardRoot == null)
        {
            return;
        }

        _garageCards[part] = new GarageUpgradeCard
        {
            Part = part,
            DisplayName = displayName,
            DescriptionText = FindComponent<Text>(cardRoot, "DescriptionText"),
            LevelText = FindComponent<Text>(cardRoot, "LevelText"),
            PriceText = FindComponent<Text>(cardRoot, "PriceRow/PriceText"),
            UpgradeButton = FindComponent<Button>(cardRoot, buttonName),
            UpgradeButtonLabel = FindComponent<Text>(cardRoot, $"{buttonName}/Label"),
            UpgradeButtonImage = FindComponent<Image>(cardRoot, buttonName),
        };
    }

    private void SetupInitialState()
    {
        SetActiveSafe(bgPlain, true);
        SetActiveSafe(bgCrashed, false);
        SetActiveSafe(ufoFlying, false);
        SetActiveSafe(ufoCrashed, false);
        SetParticleActive(fxImpactDust, false);
        SetParticleActive(fxSmokeA, false);
        SetParticleActive(fxSmokeB, false);

        if (canvasLogin != null)
        {
            canvasLogin.SetActive(true);
        }

        if (_loginCanvasGroup != null)
        {
            _loginCanvasGroup.alpha = 1f;
            _loginCanvasGroup.interactable = true;
            _loginCanvasGroup.blocksRaycasts = true;
        }

        if (canvasLobby != null)
        {
            canvasLobby.SetActive(false);
        }

        if (_lobbyCanvasGroup != null)
        {
            _lobbyCanvasGroup.alpha = 0f;
            _lobbyCanvasGroup.interactable = false;
            _lobbyCanvasGroup.blocksRaycasts = false;
        }

        HideCanvas(canvasShop);
        HideCanvas(canvasGarage);

        if (_inputId != null)
        {
            _inputId.text = string.Empty;
        }

        if (_inputPassword != null)
        {
            _inputPassword.text = string.Empty;
        }

        if (_inputNickname != null)
        {
            _inputNickname.text = string.Empty;
        }

        SetStatus(string.Empty, false);
    }

    private void ApplyLobbyMainState()
    {
        SetActiveSafe(bgPlain, false);
        SetActiveSafe(bgCrashed, true);
        SetActiveSafe(ufoFlying, false);
        SetActiveSafe(ufoCrashed, false);
        SetParticleActive(fxImpactDust, false);

        if (bgCrashedRenderer != null)
        {
            bgCrashedRenderer.color = Color.white;
        }

        if (fxSmokeA != null)
        {
            fxSmokeA.gameObject.SetActive(true);
            if (!fxSmokeA.isPlaying)
            {
                fxSmokeA.Play();
            }
        }

        if (fxSmokeB != null)
        {
            fxSmokeB.gameObject.SetActive(true);
            if (!fxSmokeB.isPlaying)
            {
                fxSmokeB.Play();
            }
        }

        if (canvasLogin != null)
        {
            canvasLogin.SetActive(false);
        }

        if (_loginCanvasGroup != null)
        {
            _loginCanvasGroup.alpha = 0f;
            _loginCanvasGroup.interactable = false;
            _loginCanvasGroup.blocksRaycasts = false;
        }

        if (canvasLobby != null)
        {
            canvasLobby.SetActive(true);
        }

        if (_lobbyCanvasGroup != null)
        {
            _lobbyCanvasGroup.alpha = 1f;
            _lobbyCanvasGroup.interactable = true;
            _lobbyCanvasGroup.blocksRaycasts = true;
        }

        HideCanvas(canvasShop);
        HideCanvas(canvasGarage);
        BindNavigationButtons();
    }

    private void BindNavigationButtons()
    {
        if (_btnGameStart == null && canvasLobby != null)
        {
            _btnGameStart = FindComponent<Button>(canvasLobby.transform, "Btn_GameStart");
        }

        RebindButton(btnShopOpen, OpenShop);
        RebindButton(btnGarageOpen, OpenGarage);
        RebindButton(btnShopClose, CloseShop);
        RebindButton(btnGarageClose, CloseGarage);
        RebindButton(_btnGameStart, OpenStageSelect);
    }

    private void BindAuthButtons()
    {
        if (btnLogin != null)
        {
            btnLogin.onClick.RemoveAllListeners();
            if (allowDirectLoginButton)
            {
                btnLogin.onClick.AddListener(OnLoginComplete);
            }
            else
            {
                btnLogin.onClick.AddListener(HandlePrimaryAuthAction);
            }
        }

        if (_btnRegister != null)
        {
            _btnRegister.onClick.RemoveAllListeners();
            _btnRegister.onClick.AddListener(HandleSecondaryAuthAction);
        }
    }

    private void BindGarageButtonsLegacy()
    {
        foreach (KeyValuePair<GaragePart, GarageUpgradeCard> entry in _garageCards)
        {
            GaragePart part = entry.Key;
            GarageUpgradeCard card = entry.Value;
            if (card?.UpgradeButton == null)
            {
                continue;
            }

            card.UpgradeButton.onClick.RemoveAllListeners();
            card.UpgradeButton.onClick.AddListener(() => TryUpgradeGaragePart(part));
        }

        if (_garageBulkUpgradeButton != null)
        {
            _garageBulkUpgradeButton.onClick.RemoveAllListeners();
            _garageBulkUpgradeButton.onClick.AddListener(TryBulkUpgradeGarage);
        }
    }

    private void HandlePrimaryAuthAction()
    {
        if (_authBusy || _hasCompletedLogin)
        {
            return;
        }

        if (_authMode == AuthMode.Login)
        {
            string loginId = _inputId != null ? _inputId.text.Trim() : string.Empty;
            string password = _inputPassword != null ? _inputPassword.text : string.Empty;
            if (!ValidateLoginForm(loginId, password))
            {
                return;
            }

            StartCoroutine(LoginFlow(loginId, password, null));
            return;
        }

        string signUpId = _inputId != null ? _inputId.text.Trim() : string.Empty;
        string signUpPassword = _inputPassword != null ? _inputPassword.text : string.Empty;
        string nickname = _inputNickname != null ? _inputNickname.text.Trim() : string.Empty;
        if (!ValidateSignUpForm(signUpId, signUpPassword, nickname))
        {
            return;
        }

        StartCoroutine(SignUpFlow(signUpId, signUpPassword, nickname));
    }

    private void HandleSecondaryAuthAction()
    {
        if (_authBusy || _hasCompletedLogin)
        {
            return;
        }

        ApplyAuthMode(_authMode == AuthMode.Login ? AuthMode.SignUp : AuthMode.Login);
    }

    private void ApplyAuthMode(AuthMode mode)
    {
        _authMode = mode;
        bool signUpMode = mode == AuthMode.SignUp;

        if (_subtitleText != null)
        {
            _subtitleText.text = signUpMode ? "\ud68c\uc6d0\uac00\uc785" : "\ub85c\uadf8\uc778";
        }

        if (_btnLoginText != null)
        {
            _btnLoginText.text = signUpMode ? "\uac00\uc785\ud558\uae30" : "\ub85c\uadf8\uc778";
        }

        if (_btnRegisterText != null)
        {
            _btnRegisterText.text = signUpMode ? "\ub85c\uadf8\uc778\uc73c\ub85c" : "\ud68c\uc6d0\uac00\uc785";
        }

        if (_inputNickname != null)
        {
            _inputNickname.gameObject.SetActive(signUpMode);
            if (!signUpMode)
            {
                _inputNickname.text = string.Empty;
            }
        }

        ApplyAuthLayout(signUpMode);
        SetStatus(string.Empty, false);
    }

    private void ApplyAuthLayout(bool signUpMode)
    {
        if (_inputId != null)
        {
            SetAnchors(_inputId.GetComponent<RectTransform>(), 0.10f, signUpMode ? 0.60f : 0.52f, 0.90f, signUpMode ? 0.72f : 0.65f);
        }

        if (_inputPassword != null)
        {
            SetAnchors(_inputPassword.GetComponent<RectTransform>(), 0.10f, signUpMode ? 0.44f : 0.36f, 0.90f, signUpMode ? 0.56f : 0.49f);
        }

        if (_inputNickname != null)
        {
            SetAnchors(_inputNickname.GetComponent<RectTransform>(), 0.10f, 0.28f, 0.90f, 0.40f);
        }

        if (_statusText != null)
        {
            SetAnchors(_statusText.GetComponent<RectTransform>(), 0.10f, signUpMode ? 0.19f : 0.26f, 0.90f, signUpMode ? 0.25f : 0.32f);
        }

        if (btnLogin != null)
        {
            SetAnchors(btnLogin.GetComponent<RectTransform>(), 0.10f, signUpMode ? 0.10f : 0.18f, 0.90f, signUpMode ? 0.19f : 0.31f);
        }

        if (_btnRegister != null)
        {
            SetAnchors(_btnRegister.GetComponent<RectTransform>(), 0.10f, signUpMode ? 0.02f : 0.05f, 0.90f, signUpMode ? 0.09f : 0.15f);
        }
    }

    private bool ValidateLoginForm(string loginId, string password)
    {
        if (string.IsNullOrWhiteSpace(loginId))
        {
            SetStatus("\uc544\uc774\ub514\ub97c \uc785\ub825\ud574 \uc8fc\uc138\uc694.", true);
            return false;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            SetStatus("\ube44\ubc00\ubc88\ud638\ub97c \uc785\ub825\ud574 \uc8fc\uc138\uc694.", true);
            return false;
        }

        return true;
    }

    private bool ValidateSignUpForm(string loginId, string password, string nickname)
    {
        if (!ValidateLoginForm(loginId, password))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(nickname))
        {
            SetStatus("\ub2c9\ub124\uc784\uc744 \uc785\ub825\ud574 \uc8fc\uc138\uc694.", true);
            return false;
        }

        return true;
    }

    private IEnumerator SignUpFlow(string loginId, string password, string nickname)
    {
        SetAuthBusy(true, "\ud68c\uc6d0\uac00\uc785 \uc911...", false);

        LobbyAuthApi.ApiMessage signUpMessage = null;
        LobbyAuthApi.ApiError signUpError = null;

        yield return StartCoroutine(_authApi.SignUp(
            loginId,
            password,
            nickname,
            message => signUpMessage = message,
            error => signUpError = error));

        if (signUpError != null)
        {
            SetAuthBusy(false, signUpError.Message, true);
            yield break;
        }

        if (!autoLoginAfterSignup)
        {
            if (_inputPassword != null)
            {
                _inputPassword.text = string.Empty;
            }

            ApplyAuthMode(AuthMode.Login);
            SetAuthBusy(false, signUpMessage != null ? signUpMessage.Message : "\ud68c\uc6d0\uac00\uc785\uc774 \uc644\ub8cc\ub418\uc5c8\uc2b5\ub2c8\ub2e4.", false);
            yield break;
        }

        SetAuthBusy(false, "\ud68c\uc6d0\uac00\uc785 \uc644\ub8cc. \ub85c\uadf8\uc778 \uc911...", false);
        yield return StartCoroutine(LoginFlow(
            loginId,
            password,
            "\ud68c\uc6d0\uac00\uc785\uacfc \ub85c\uadf8\uc778\uc774 \uc644\ub8cc\ub418\uc5c8\uc2b5\ub2c8\ub2e4."));
    }

    private IEnumerator LoginFlow(string loginId, string password, string successMessageOverride)
    {
        SetAuthBusy(true, "\ub85c\uadf8\uc778 \uc911...", false);

        LobbyAuthApi.ApiMessage loginMessage = null;
        LobbyAuthApi.ApiError loginError = null;

        yield return StartCoroutine(_authApi.Login(
            loginId,
            password,
            message => loginMessage = message,
            error => loginError = error));

        if (loginError != null)
        {
            if (_inputPassword != null)
            {
                _inputPassword.text = string.Empty;
            }

            SetAuthBusy(false, loginError.Message, true);
            yield break;
        }

        _currentLoginId = loginId;
        _currentNickname = string.Empty;
        _currentUserId = -1;

        if (_garageUserIdInput != null)
        {
            _garageUserIdInput.text = string.Empty;
        }

        LobbyAuthApi.GameProfileData profile = null;
        LobbyAuthApi.ApiError profileError = null;

        yield return StartCoroutine(_authApi.GetProfile(
            data => profile = data,
            error => profileError = error));

        if (profile != null)
        {
            ApplyProfile(profile);
        }
        else if (profileError != null)
        {
            if (profileError.StatusCode == 401 || profileError.StatusCode == 403)
            {
                SetAuthBusy(false, "로그인 세션을 확인하지 못했습니다. 다시 시도해 주세요.", true);
                yield break;
            }

            Debug.LogWarning($"Profile sync failed: {profileError.Message}");
        }

        if (HasAuthenticatedSession())
        {
            StartCoroutine(SyncShopCatalogFlow(true));
            StartCoroutine(SyncInventoryFlow(true));
        }

        SetAuthBusy(false, successMessageOverride ?? loginMessage?.Message ?? "\ub85c\uadf8\uc778\uc774 \uc644\ub8cc\ub418\uc5c8\uc2b5\ub2c8\ub2e4.", false);
        OnLoginComplete();
    }

    private void ApplyProfile(LobbyAuthApi.GameProfileData profile)
    {
        if (profile != null && profile.UserId > 0)
        {
            _currentUserId = profile.UserId;
        }

        if (profile != null && !string.IsNullOrWhiteSpace(profile.LoginId))
        {
            _currentLoginId = profile.LoginId.Trim();
        }

        if (profile != null && !string.IsNullOrWhiteSpace(profile.Nickname))
        {
            _currentNickname = profile.Nickname.Trim();
        }

        if (profile != null)
        {
            _currentBaseHp = Mathf.Max(100, profile.Hp);
            _currentBaseSpeed = Mathf.Max(2f, profile.BaseSpeed);
            _currentBoosterBonusSec = Mathf.Max(0, profile.BoosterBonusSec);
            playerCoins = profile.Coin;
            _garageState.coinBalance = playerCoins;
            _garageState.engineLevel = 1 + Mathf.Max(0, _currentBoosterBonusSec / 2);
            _garageState.wheelLevel = 1 + Mathf.Max(0, Mathf.FloorToInt((_currentBaseSpeed - 2f) + 0.01f));
            _garageState.armorLevel = 1 + Mathf.Max(0, Mathf.FloorToInt(((_currentBaseHp - 100f) / 20f) + 0.01f));
            _garageState.magnetLevel = 1;
            ClampGarageState();
            UpdateCoinLabels(playerCoins);
            StoreLastKnownCoin(playerCoins);
        }

        BackendRuntimeSession.Configure(
            backendBaseUrl,
            _currentUserId,
            _authApi != null ? _authApi.SessionCookie : null,
            _currentLoginId,
            _currentNickname);

        RefreshGarageUi();
        RefreshShopUi();
    }

    private IEnumerator ResumeLobbyMainFromPendingReturnNextFrame()
    {
        yield return null;
        TryResumeLobbyMainFromPendingReturn();
        RestoreAuthStateFromKnownUser();
    }

    private void TryResumeLobbyMainFromPendingReturn()
    {
        if (!LobbyReturnFlowBridge.TryConsumePendingReturn(
                out string pendingBaseUrl,
                out long pendingUserId,
                out string pendingSessionCookie,
                out string pendingLoginId,
                out string pendingNickname,
                out int pendingCoin))
        {
            return;
        }

        UserData currentUser = UserDataManager.Instance != null ? UserDataManager.Instance.CurrentUser : null;

        _currentLoginId = !string.IsNullOrWhiteSpace(pendingLoginId)
            ? pendingLoginId.Trim()
            : (currentUser != null ? currentUser.userId : string.Empty);
        _currentNickname = !string.IsNullOrWhiteSpace(pendingNickname)
            ? pendingNickname.Trim()
            : (currentUser != null ? currentUser.nickname : string.Empty);
        _currentUserId = pendingUserId > 0
            ? pendingUserId
            : (currentUser != null ? currentUser.backendUserId : -1);
        playerCoins = pendingCoin >= 0
            ? pendingCoin
            : (currentUser != null ? Mathf.Max(currentUser.coin, 0) : playerCoins);

        if (!string.IsNullOrWhiteSpace(_currentLoginId) && UserDataManager.Instance != null)
        {
            UserDataManager.Instance.SetUser(_currentLoginId, _currentNickname, playerCoins, _currentUserId);
        }

        string runtimeBaseUrl = !string.IsNullOrWhiteSpace(pendingBaseUrl)
            ? pendingBaseUrl
            : BackendRuntimeSession.BaseUrl;

        if (_authApi != null)
        {
            _authApi.RestoreSession(pendingSessionCookie, _currentUserId, _currentLoginId, _currentNickname);
        }

        BackendRuntimeSession.Configure(
            runtimeBaseUrl,
            _currentUserId,
            pendingSessionCookie,
            _currentLoginId,
            _currentNickname);

        _garageState.coinBalance = playerCoins;
        StoreLastKnownCoin(playerCoins);
        ApplyLobbyMainState();
        SetStatus(string.Empty, false);
        _hasCompletedLogin = true;

        if (HasAuthenticatedSession())
        {
            StartCoroutine(SyncShopCatalogFlow(true));
            StartCoroutine(SyncInventoryFlow(true));
        }
    }

    private void SetAuthBusy(bool busy, string message, bool isError)
    {
        _authBusy = busy;

        if (btnLogin != null)
        {
            btnLogin.interactable = !busy;
        }

        if (_btnRegister != null)
        {
            _btnRegister.interactable = !busy;
        }

        if (_inputId != null)
        {
            _inputId.interactable = !busy;
        }

        if (_inputPassword != null)
        {
            _inputPassword.interactable = !busy;
        }

        if (_inputNickname != null)
        {
            _inputNickname.interactable = !busy;
        }

        SetStatus(message, isError);
    }

    private void SetStatus(string message, bool isError)
    {
        if (_statusText == null)
        {
            return;
        }

        _statusText.text = message ?? string.Empty;
        _statusText.color = isError
            ? new Color(0.83f, 0.21f, 0.25f)
            : new Color(0.20f, 0.65f, 0.42f);
    }

    private void LoadPlayerStateLegacy(int fallbackCoin)
    {
        string key = BuildPlayerStateKey();
        GarageUpgradeState loadedState = null;

        if (PlayerPrefs.HasKey(key))
        {
            string json = PlayerPrefs.GetString(key, string.Empty);
            if (!string.IsNullOrWhiteSpace(json))
            {
                loadedState = JsonUtility.FromJson<GarageUpgradeState>(json);
            }
        }

        _garageState = loadedState ?? GarageUpgradeState.CreateDefault(fallbackCoin);
        _garageState.coinBalance = _garageState.coinBalance >= 0 ? _garageState.coinBalance : fallbackCoin;
        ClampGarageState();

        playerCoins = _garageState.coinBalance;
        UpdateCoinLabels(playerCoins);
        RefreshGarageUi();
        PersistPlayerState();
    }

    private void PersistPlayerStateLegacy()
    {
        if (_garageState == null)
        {
            _garageState = GarageUpgradeState.CreateDefault(playerCoins);
        }

        _garageState.coinBalance = playerCoins;
        ClampGarageState();

        PlayerPrefs.SetString(BuildPlayerStateKey(), JsonUtility.ToJson(_garageState));
        PlayerPrefs.Save();
    }

    private string BuildPlayerStateKeyLegacy()
    {
        string normalizedLoginId = string.IsNullOrWhiteSpace(_currentLoginId)
            ? GuestPlayerId
            : _currentLoginId.Trim().ToLowerInvariant();

        return PlayerStatePrefsPrefix + normalizedLoginId;
    }

    private void ClampGarageStateLegacy()
    {
        if (_garageState == null)
        {
            return;
        }

        _garageState.engineLevel = Mathf.Clamp(_garageState.engineLevel, 1, GarageMaxLevel);
        _garageState.wheelLevel = Mathf.Clamp(_garageState.wheelLevel, 1, GarageMaxLevel);
        _garageState.armorLevel = Mathf.Clamp(_garageState.armorLevel, 1, GarageMaxLevel);
        _garageState.magnetLevel = Mathf.Clamp(_garageState.magnetLevel, 1, GarageMaxLevel);
    }

    private void RefreshGarageUiLegacy(string actionMessage = null)
    {
        foreach (GarageUpgradeCard card in _garageCards.Values)
        {
            if (card == null)
            {
                continue;
            }

            int level = GetGarageLevel(card.Part);
            bool isMaxLevel = level >= GarageMaxLevel;
            int nextPrice = GetGarageUpgradePrice(level);

            if (card.DescriptionText != null)
            {
                card.DescriptionText.text = GetGarageDescription(card.Part, level);
            }

            if (card.LevelText != null)
            {
                card.LevelText.text = isMaxLevel
                    ? $"Lv {level} / MAX"
                    : $"Lv {level} -> Lv {level + 1}";
            }

            if (card.PriceText != null)
            {
                card.PriceText.text = isMaxLevel ? "MAX" : nextPrice.ToString();
            }

            if (card.UpgradeButtonLabel != null)
            {
                card.UpgradeButtonLabel.text = isMaxLevel ? "완료" : "업그레이드";
            }

            if (card.UpgradeButton != null)
            {
                card.UpgradeButton.interactable = !isMaxLevel && playerCoins >= nextPrice;
            }

            if (card.UpgradeButtonImage != null)
            {
                card.UpgradeButtonImage.color = isMaxLevel
                    ? new Color(0.18f, 0.28f, 0.21f, 0.92f)
                    : new Color(0.18f, 0.15f, 0.18f, 0.96f);
            }
        }

        if (_garageBulkUpgradeLabel != null)
        {
            _garageBulkUpgradeLabel.text = "일괄 업그레이드";
        }

        if (_garageBulkUpgradeButton != null)
        {
            _garageBulkUpgradeButton.interactable = HasUpgradeableGaragePart();
        }

        UpdateGarageProgress(actionMessage);
    }

    private void UpdateGarageProgressLegacy(string actionMessage)
    {
        int totalLevels = 0;
        foreach (GaragePart part in _garageCards.Keys)
        {
            totalLevels += GetGarageLevel(part);
        }

        float progress = _garageCards.Count == 0
            ? 0f
            : totalLevels / (float)(_garageCards.Count * GarageMaxLevel);

        if (_garageProgressFill != null)
        {
            RectTransform fillRect = _garageProgressFill.rectTransform;
            fillRect.anchorMax = new Vector2(progress, fillRect.anchorMax.y);
        }

        if (_garageProgressLabel != null)
        {
            string progressText = $"정비 진행도 {(progress * 100f):0}%";
            _garageProgressLabel.text = string.IsNullOrWhiteSpace(actionMessage)
                ? progressText
                : $"{actionMessage} · {progressText}";
        }
    }

    private bool HasUpgradeableGaragePartLegacy()
    {
        foreach (GaragePart part in _garageCards.Keys)
        {
            int level = GetGarageLevel(part);
            if (level < GarageMaxLevel && playerCoins >= GetGarageUpgradePrice(level))
            {
                return true;
            }
        }

        return false;
    }

    private void TryUpgradeGaragePartLegacy(GaragePart part)
    {
        if (!_garageCards.TryGetValue(part, out GarageUpgradeCard card))
        {
            return;
        }

        int currentLevel = GetGarageLevel(part);
        if (currentLevel >= GarageMaxLevel)
        {
            RefreshGarageUi($"{card.DisplayName} 최대 단계");
            return;
        }

        int price = GetGarageUpgradePrice(currentLevel);
        if (!TrySpendCoin(price, $"{card.DisplayName} 업그레이드"))
        {
            RefreshGarageUi($"코인이 부족합니다 ({price} 필요)");
            return;
        }

        SetGarageLevel(part, currentLevel + 1);
        PersistPlayerState();
        RefreshGarageUi($"{card.DisplayName} Lv {currentLevel} -> Lv {currentLevel + 1}");
    }

    private void TryBulkUpgradeGarageLegacy()
    {
        int upgradedCount = 0;

        foreach (GaragePart part in GetGarageUpgradeOrder())
        {
            int currentLevel = GetGarageLevel(part);
            int price = GetGarageUpgradePrice(currentLevel);

            if (currentLevel >= GarageMaxLevel || playerCoins < price)
            {
                continue;
            }

            if (!TrySpendCoin(price, $"{part} 일괄 업그레이드"))
            {
                continue;
            }

            SetGarageLevel(part, currentLevel + 1);
            upgradedCount++;
        }

        PersistPlayerState();

        if (upgradedCount <= 0)
        {
            RefreshGarageUi("업그레이드할 항목이 없습니다");
            return;
        }

        RefreshGarageUi($"{upgradedCount}개 항목 업그레이드");
    }

    private static GaragePart[] GetGarageUpgradeOrderLegacy()
    {
        return new[]
        {
            GaragePart.Engine,
            GaragePart.Wheel,
            GaragePart.Armor,
            GaragePart.Magnet,
        };
    }

    private int GetGarageLevelLegacy(GaragePart part)
    {
        switch (part)
        {
            case GaragePart.Engine:
                return _garageState != null ? _garageState.engineLevel : 1;
            case GaragePart.Wheel:
                return _garageState != null ? _garageState.wheelLevel : 1;
            case GaragePart.Armor:
                return _garageState != null ? _garageState.armorLevel : 1;
            case GaragePart.Magnet:
                return _garageState != null ? _garageState.magnetLevel : 1;
            default:
                return 1;
        }
    }

    private void SetGarageLevelLegacy(GaragePart part, int level)
    {
        if (_garageState == null)
        {
            _garageState = GarageUpgradeState.CreateDefault(playerCoins);
        }

        int clampedLevel = Mathf.Clamp(level, 1, GarageMaxLevel);
        switch (part)
        {
            case GaragePart.Engine:
                _garageState.engineLevel = clampedLevel;
                break;
            case GaragePart.Wheel:
                _garageState.wheelLevel = clampedLevel;
                break;
            case GaragePart.Armor:
                _garageState.armorLevel = clampedLevel;
                break;
            case GaragePart.Magnet:
                _garageState.magnetLevel = clampedLevel;
                break;
        }
    }

    private static int GetGarageUpgradePriceLegacy(int currentLevel)
    {
        return Mathf.Max(50, currentLevel * 50);
    }

    private static string GetGarageDescriptionLegacy(GaragePart part, int level)
    {
        switch (part)
        {
            case GaragePart.Engine:
                return $"퀴즈 정답 시 부스터 지속 +{level + 1}초";
            case GaragePart.Wheel:
                return $"기본 이동 속도 +{(level - 1) * 5}%";
            case GaragePart.Armor:
                return $"최대 HP +{(level - 1) * 20}";
            case GaragePart.Magnet:
                return $"코인 자석 범위 +{level * 0.5f:0.0}m";
            default:
                return string.Empty;
        }
    }

    private void UpdateCoinLabels(int value)
    {
        string coinText = value.ToString();

        if (_lobbyCoinText != null)
        {
            _lobbyCoinText.text = coinText;
        }

        if (_shopCoinText != null)
        {
            _shopCoinText.text = coinText;
        }

        if (_garageCoinText != null)
        {
            _garageCoinText.text = coinText;
        }
    }

    private IEnumerator CrashSequence()
    {
        if (_loginCanvasGroup != null)
        {
            yield return StartCoroutine(FadeCanvasGroup(_loginCanvasGroup, 1f, 0f, loginFadeOutDuration));
        }

        if (canvasLogin != null)
        {
            canvasLogin.SetActive(false);
        }

        yield return new WaitForSeconds(0.3f);

        if (ufoFlying != null)
        {
            ufoFlying.SetActive(true);

            Vector3 startPos = ufoFlying.transform.position + new Vector3(8f, 10f, 0f);
            Vector3 endPos = ufoFlying.transform.position;
            ufoFlying.transform.position = startPos;

            float elapsed = 0f;
            const float fallDuration = 0.85f;

            while (elapsed < fallDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fallDuration;
                t *= t;
                ufoFlying.transform.position = Vector3.Lerp(startPos, endPos, t);
                yield return null;
            }

            ufoFlying.transform.position = endPos;
            ufoFlying.SetActive(false);
        }

        SetActiveSafe(bgPlain, false);
        SetActiveSafe(bgCrashed, true);

        if (bgCrashedRenderer != null)
        {
            bgCrashedRenderer.color = Color.white;
        }

        if (fxImpactDust != null)
        {
            fxImpactDust.gameObject.SetActive(true);
            fxImpactDust.Play();
        }

        StartCoroutine(CameraShake(0.6f, 0.35f));

        if (fxSmokeA != null)
        {
            fxSmokeA.gameObject.SetActive(true);
            fxSmokeA.Play();
        }

        if (fxSmokeB != null)
        {
            fxSmokeB.gameObject.SetActive(true);
            fxSmokeB.Play();
        }

        yield return new WaitForSeconds(postCrashDelay);
        ShowLobby();
    }

    private void ShowLobby()
    {
        if (canvasLobby != null)
        {
            canvasLobby.SetActive(true);
        }

        if (_lobbyCanvasGroup != null)
        {
            StartCoroutine(FadeCanvasGroup(_lobbyCanvasGroup, 0f, 1f, lobbyFadeInDuration));
        }
    }

    private IEnumerator TransitionScreen(GameObject fromGo, GameObject toGo, bool returningToLobby)
    {
        _isTransitioning = true;

        CanvasGroup fromCg = EnsureCanvasGroup(fromGo);
        CanvasGroup toCg = EnsureCanvasGroup(toGo);

        if (toGo != null)
        {
            toGo.SetActive(true);
        }

        if (toCg != null)
        {
            toCg.alpha = 0f;
            toCg.interactable = false;
            toCg.blocksRaycasts = false;
        }

        if (fromCg != null)
        {
            yield return StartCoroutine(FadeCanvasGroup(fromCg, 1f, 0f, 0.25f));
        }

        if (fromGo != null)
        {
            fromGo.SetActive(false);
        }

        SetActiveSafe(bgCrashed, returningToLobby);
        if (fxSmokeA != null)
        {
            fxSmokeA.gameObject.SetActive(returningToLobby);
            if (returningToLobby && !fxSmokeA.isPlaying)
            {
                fxSmokeA.Play();
            }
        }

        if (fxSmokeB != null)
        {
            fxSmokeB.gameObject.SetActive(returningToLobby);
            if (returningToLobby && !fxSmokeB.isPlaying)
            {
                fxSmokeB.Play();
            }
        }

        if (toCg != null)
        {
            yield return StartCoroutine(FadeCanvasGroup(toCg, 0f, 1f, 0.25f));
        }

        _isTransitioning = false;
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float from, float to, float duration)
    {
        if (canvasGroup == null)
        {
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        canvasGroup.alpha = to;
        canvasGroup.interactable = to > 0.5f;
        canvasGroup.blocksRaycasts = to > 0.5f;
    }

    private IEnumerator CameraShake(float duration, float magnitude)
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            yield break;
        }

        Vector3 origin = mainCamera.transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float dampening = 1f - (elapsed / duration);
            float x = UnityEngine.Random.Range(-1f, 1f) * magnitude * dampening;
            float y = UnityEngine.Random.Range(-1f, 1f) * magnitude * dampening;
            mainCamera.transform.position = origin + new Vector3(x, y, 0f);
            yield return null;
        }

        mainCamera.transform.position = origin;
    }

    private static void RebindButton(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null || action == null)
        {
            return;
        }

        button.onClick.RemoveListener(action);

        if (HasPersistentListener(button, action.Method.Name))
        {
            return;
        }

        button.onClick.AddListener(action);
    }

    private static bool HasPersistentListener(Button button, string methodName)
    {
        if (button == null || string.IsNullOrWhiteSpace(methodName))
        {
            return false;
        }

        int persistentCount = button.onClick.GetPersistentEventCount();
        for (int index = 0; index < persistentCount; index++)
        {
            if (string.Equals(
                    button.onClick.GetPersistentMethodName(index),
                    methodName,
                    StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static void SetAnchors(RectTransform rectTransform, float minX, float minY, float maxX, float maxY)
    {
        if (rectTransform == null)
        {
            return;
        }

        rectTransform.anchorMin = new Vector2(minX, minY);
        rectTransform.anchorMax = new Vector2(maxX, maxY);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }

    private static void SetActiveSafe(GameObject target, bool active)
    {
        if (target != null)
        {
            target.SetActive(active);
        }
    }

    private static void SetParticleActive(ParticleSystem particle, bool active)
    {
        if (particle != null)
        {
            particle.gameObject.SetActive(active);
        }
    }

    private static T FindComponent<T>(Transform root, string relativePath) where T : Component
    {
        if (root == null)
        {
            return null;
        }

        Transform found = root.Find(relativePath);
        return found != null ? found.GetComponent<T>() : null;
    }

    private static CanvasGroup EnsureCanvasGroup(GameObject target)
    {
        if (target == null)
        {
            return null;
        }

        CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = target.AddComponent<CanvasGroup>();
        }

        return canvasGroup;
    }

    private static void HideCanvas(GameObject canvasObject)
    {
        if (canvasObject == null)
        {
            return;
        }

        CanvasGroup canvasGroup = EnsureCanvasGroup(canvasObject);
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        canvasObject.SetActive(false);
    }

    private long LoadStoredRepairUserId()
    {
        string key = BuildRepairUserIdKey();
        if (string.IsNullOrWhiteSpace(key) || !PlayerPrefs.HasKey(key))
        {
            return -1;
        }

        string rawValue = PlayerPrefs.GetString(key, string.Empty);
        return long.TryParse(rawValue, out long userId) && userId > 0
            ? userId
            : -1;
    }

    private void StoreRepairUserId(long userId)
    {
        string key = BuildRepairUserIdKey();
        if (string.IsNullOrWhiteSpace(key) || userId <= 0)
        {
            return;
        }

        PlayerPrefs.SetString(key, userId.ToString());
        PlayerPrefs.Save();
    }

    private string BuildRepairUserIdKey()
    {
        if (string.IsNullOrWhiteSpace(_currentLoginId))
        {
            return string.Empty;
        }

        return RepairUserIdPrefsPrefix + _currentLoginId.Trim().ToLowerInvariant();
    }

    private bool TryLoadLastKnownCoin(out int coin)
    {
        string key = BuildLastKnownCoinKey();
        if (string.IsNullOrWhiteSpace(key) || !PlayerPrefs.HasKey(key))
        {
            coin = 0;
            return false;
        }

        coin = PlayerPrefs.GetInt(key, playerCoins);
        return true;
    }

    private void StoreLastKnownCoin(int coin)
    {
        string key = BuildLastKnownCoinKey();
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        PlayerPrefs.SetInt(key, coin);
        PlayerPrefs.Save();
    }

    private string BuildLastKnownCoinKey()
    {
        if (string.IsNullOrWhiteSpace(_currentLoginId))
        {
            return string.Empty;
        }

        return LastKnownCoinPrefsPrefix + _currentLoginId.Trim().ToLowerInvariant();
    }
}
