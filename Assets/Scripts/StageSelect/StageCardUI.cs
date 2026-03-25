using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StageCardUI : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("Stage Data")]
    [SerializeField] private StageInfo stageInfo;
    [SerializeField] private string sceneName;

    [Header("References")]
    [SerializeField] private Button button;
    [SerializeField] private Transform visualRoot;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Graphic outlineTargetGraphic;
    [SerializeField] private Image thumbnailImage;
    [SerializeField] private Image lockIcon;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text stateText;

    [Header("Selection Visuals")]
    [SerializeField] private float selectedScale = 1.08f;
    [SerializeField] private float unselectedScale = 1f;
    [SerializeField] [Range(0f, 1f)] private float selectedAlpha = 1f;
    [SerializeField] [Range(0f, 1f)] private float unselectedAlpha = 0.5f;
    [SerializeField] private Color selectedOutlineColor = new Color(0.35f, 0.9f, 0.8f, 1f);
    [SerializeField] private Vector2 selectedOutlineDistance = new Vector2(6f, 6f);
    [SerializeField] private Color unlockedThumbnailColor = Color.white;
    [SerializeField] private Color lockedThumbnailColor = new Color(0.35f, 0.35f, 0.35f, 1f);
    [SerializeField] private Color lockedStateColor = new Color(1f, 0.75f, 0.35f, 1f);
    [SerializeField] private Color playableStateColor = Color.white;
    [SerializeField] private Color clearedStateColor = new Color(0.45f, 1f, 0.7f, 1f);

    private StageSelectManager manager;
    private Outline selectionOutline;
    private bool autoBindLogged;

    public StageInfo StageInfo => stageInfo;
    public GameObject SelectableTarget => button != null ? button.gameObject : gameObject;
    public string SceneName => !string.IsNullOrWhiteSpace(sceneName)
        ? sceneName
        : stageInfo != null ? stageInfo.SceneName : string.Empty;

    private void Awake()
    {
        AutoBind();
    }

    private void OnEnable()
    {
        RefreshContent();
    }

    public void Initialize(StageSelectManager stageSelectManager)
    {
        manager = stageSelectManager;
    }

    public void SetStageInfo(StageInfo data)
    {
        stageInfo = data;
        RefreshContent();
    }

    public void RefreshContent()
    {
        if (stageInfo == null)
        {
            Debug.LogWarning($"StageCardUI.RefreshContent: '{name}' has no StageInfo assigned. Keeping scene defaults.", this);
            return;
        }

        bool isUnlocked = IsStageUnlocked();
        bool isCleared = UserDataManager.Instance != null && UserDataManager.Instance.IsStageCleared(stageInfo.StageId);

        if (titleText != null)
        {
            titleText.text = stageInfo.StageName;
        }

        if (descriptionText != null)
        {
            descriptionText.text = stageInfo.Description;
        }

        if (thumbnailImage != null)
        {
            thumbnailImage.sprite = stageInfo.Thumbnail;
            thumbnailImage.enabled = thumbnailImage.sprite != null;
            thumbnailImage.color = isUnlocked ? unlockedThumbnailColor : lockedThumbnailColor;
        }

        if (lockIcon != null)
        {
            lockIcon.enabled = !isUnlocked;
        }

        if (stateText != null)
        {
            stateText.text = BuildStateText(isUnlocked, isCleared);
            stateText.color = BuildStateColor(isUnlocked, isCleared);
        }
    }

    public void SetSelected(bool isSelected)
    {
        if (visualRoot != null)
        {
            visualRoot.localScale = Vector3.one * (isSelected ? selectedScale : unselectedScale);
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = isSelected ? selectedAlpha : unselectedAlpha;
        }

        if (selectionOutline != null)
        {
            selectionOutline.effectColor = selectedOutlineColor;
            selectionOutline.effectDistance = selectedOutlineDistance;
            selectionOutline.enabled = isSelected;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        manager?.RequestSelect(this);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnClick();
        }
    }

    public void OnClick()
    {
        if (manager == null)
        {
            return;
        }

        if (stageInfo == null)
        {
            Debug.LogWarning($"StageCardUI.OnClick: '{name}' has no StageInfo assigned.", this);
            return;
        }

        if (manager.IsSelected(this))
        {
            if (!IsStageUnlocked())
            {
                Debug.Log($"StageCardUI.OnClick: '{name}' is locked.", this);
                return;
            }

            manager.RequestStart(this);
            return;
        }

        manager.RequestSelect(this);
    }

    private bool IsStageUnlocked()
    {
        if (stageInfo == null)
        {
            return false;
        }

        return stageInfo.DefaultUnlocked
            || (UserDataManager.Instance != null && UserDataManager.Instance.IsStageUnlocked(stageInfo.StageId));
    }

    private void AutoBind()
    {
        bool autoBoundAny = false;

        if (button == null)
        {
            button = GetComponent<Button>();
            autoBoundAny |= button != null;
        }

        visualRoot ??= transform;

        if (outlineTargetGraphic == null)
        {
            outlineTargetGraphic = GetComponent<Graphic>();
            autoBoundAny |= outlineTargetGraphic != null;
        }

        if (thumbnailImage == null)
        {
            thumbnailImage = FindImage("Thumbnail");
            autoBoundAny |= thumbnailImage != null;
        }

        if (lockIcon == null)
        {
            lockIcon = FindImage("LockIcon");
            autoBoundAny |= lockIcon != null;
        }

        if (titleText == null)
        {
            titleText = FindText("TitleText");
            autoBoundAny |= titleText != null;
        }

        if (descriptionText == null)
        {
            descriptionText = FindText("DescriptionText");
            autoBoundAny |= descriptionText != null;
        }

        if (stateText == null)
        {
            stateText = FindText("StateText");
            autoBoundAny |= stateText != null;
        }

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
            autoBoundAny = true;
        }

        if (outlineTargetGraphic != null)
        {
            selectionOutline = outlineTargetGraphic.GetComponent<Outline>() ?? outlineTargetGraphic.gameObject.AddComponent<Outline>();
            selectionOutline.effectColor = selectedOutlineColor;
            selectionOutline.effectDistance = selectedOutlineDistance;
            selectionOutline.useGraphicAlpha = false;
            selectionOutline.enabled = false;
        }

        if (!autoBindLogged && autoBoundAny)
        {
            Debug.LogWarning($"StageCardUI.Awake: Auto-bound references on '{name}'. Inspector assignment is recommended.", this);
            autoBindLogged = true;
        }
    }

    private string BuildStateText(bool isUnlocked, bool isCleared)
    {
        if (!isUnlocked)
        {
            return "LOCK";
        }

        return isCleared ? "CLEAR" : "START";
    }

    private Color BuildStateColor(bool isUnlocked, bool isCleared)
    {
        if (!isUnlocked)
        {
            return lockedStateColor;
        }

        return isCleared ? clearedStateColor : playableStateColor;
    }

    private Image FindImage(string objectName)
    {
        foreach (Image image in GetComponentsInChildren<Image>(true))
        {
            if (image.name == objectName)
            {
                return image;
            }
        }

        return null;
    }

    private TMP_Text FindText(string objectName)
    {
        foreach (TMP_Text text in GetComponentsInChildren<TMP_Text>(true))
        {
            if (text.name == objectName)
            {
                return text;
            }
        }

        return null;
    }
}
