using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FinanceChoiceCardUI : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public FinanceSelectManager manager;
    public FinanceChoiceType choiceType;

    [Header("UI")]
    [SerializeField] private Button button;
    [SerializeField] private Image background;
    [SerializeField] private Image frame;

    [SerializeField] private Image icon;

    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text clickText;
    [SerializeField] private RectTransform root;

    [Header("Colors")]
    [SerializeField] private Color normalBackgroundColor = new Color(1f, 1f, 1f, 0f);
    [SerializeField] private Color hoverBackgroundColor = Color.white;
    [SerializeField] private Color normalFrameColor = Color.white;
    [SerializeField] private Color hoverFrameColor = Color.yellow;
    [SerializeField] private Color normalTextColor = Color.white;
    [SerializeField] private Color hoverTextColor = Color.black;

    [Header("Scale")]
    [SerializeField] private Vector3 normalScale = Vector3.one;
    [SerializeField] private Vector3 hoverScale = new Vector3(1.05f, 1.05f, 1f);

    private bool isInteractable = true;
    private bool isSelected;

    public GameObject SelectableTarget => button != null ? button.gameObject : gameObject;

    private void Awake()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (root == null)
        {
            root = GetComponent<RectTransform>();
        }
    }

    private void Start()
    {
        RefreshVisual();
    }

    public void SetInteractable(bool value)
    {
        isInteractable = value;
        RefreshVisual();
    }

    public void SetSelected(bool value)
    {
        isSelected = value;
        RefreshVisual();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isInteractable)
        {
            return;
        }

        manager?.RequestSelect(this);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isInteractable || eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        OnClick();
    }

    public void OnClick()
    {
        if (!isInteractable || manager == null)
        {
            return;
        }

        if (manager.IsSelected(this))
        {
            manager.RequestConfirm(this);
            return;
        }

        manager.RequestSelect(this);
    }

    private void RefreshVisual()
    {
        bool isHighlighted = isSelected;

        if (background != null)
        {
            background.color = isHighlighted ? hoverBackgroundColor : normalBackgroundColor;
        }

        if (frame != null)
        {
            frame.color = isHighlighted ? hoverFrameColor : normalFrameColor;
        }

        if (titleText != null)
        {
            titleText.color = isHighlighted ? hoverTextColor : normalTextColor;
        }

        if (clickText != null)
        {
            clickText.color = isHighlighted ? hoverTextColor : normalTextColor;
        }

        if (root != null)
        {
            root.localScale = isHighlighted ? hoverScale : normalScale;
        }

        if (button != null)
        {
            button.interactable = isInteractable;
        }
    }
}
