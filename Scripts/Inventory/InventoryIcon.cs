using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image), typeof(Text))]
public class InventoryIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField, Tooltip("Tracks total available tiles")]
    Text m_quanity;
    public int Quanity {
        get { return int.Parse(m_quanity.text); }
        set { m_quanity.text = value.ToString(); }
    }

    /// <summary>
    /// The tile type this icon represents
    /// </summary>
    public TileType type;

    [SerializeField]
    Sprite m_emptySprite;

    [SerializeField]
    Sprite m_uiSprite;

    [SerializeField]
    Sprite m_selectedSprite;

    /// <summary>
    /// Image component
    /// </summary>
    Image m_image;

    /// <summary>
    /// True when it is the currently selected tile
    /// </summary>
    bool m_isSelected = false;
    public bool IsSelected
    {
        get { return m_isSelected; }
        set {
            m_isSelected = value;
            if (m_isSelected) {
                SetSprite(m_selectedSprite);
            } else {
                SetSprite(m_uiSprite);
            }
        }
    }

    /// <summary>
    /// True when it is not grayed out
    /// </summary>
    public bool IsEnabled { get { return m_image.sprite != m_emptySprite; } }

    /// <summary>
    /// Initialize
    /// </summary>
    private void Awake()
    {
        if(m_quanity == null) {
            m_quanity = GetComponentInChildren<Text>();
        }

        m_image = GetComponent<Image>();
    }

    /// <summary>
    /// Toggles the border that indicates whether a tile is selected or not
    /// </summary>
    private void Start()
    {
        var prefab = InventoryManager.Instance.GetCurrentPrefabToSpawn(true);
        IsSelected = prefab != null && prefab.Type == type;
        
        Quanity = InventoryManager.Instance.TotalTileType(type);
        EnableIcon(Quanity > 0);
    }

    /// <summary>
    /// Updates the quanity counter text
    /// Also marks it as available if it was once at zero
    /// </summary>
    private void Update()
    {
        Quanity = InventoryManager.Instance.TotalTileType(type);
    }

    /// <summary>
    /// Sets the status of the border
    /// </summary>
    /// <param name="enabled"></param>
    public void EnableIcon(bool enabled)
    {
        if (enabled) {
            SetSprite(m_uiSprite);
        } else {
            SetSprite(m_emptySprite);
        }
    }

    /// <summary>
    /// Notifies the inventory manager of a prefab change
    /// </summary>
    private void SetSprite(Sprite sprite)
    {
        if(m_image != null) {
            m_image.sprite = sprite;
        }
    }

    /// <summary>
    /// Highlight the sprite
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!IsSelected && Quanity > 0) {
            SetSprite(m_selectedSprite);
            AudioManager.Instance.Play2DSound(AudioClipName.InventoryHover);
        }
    }

    /// <summary>
    /// Reset to normal sprite
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!IsSelected && Quanity > 0) {
            SetSprite(m_uiSprite);
        }
    }

    /// <summary>
    /// Mark as selected
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!IsSelected && Quanity > 0) {
            IsSelected = true;
            InventoryManager.Instance.SetCurrentPrefab(this);
            AudioManager.Instance.Play2DSound(AudioClipName.InventorySelect);
        }
    }
}
