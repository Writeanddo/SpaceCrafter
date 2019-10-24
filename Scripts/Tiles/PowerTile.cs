using UnityEngine;

/// <summary>
/// Requires to be powered ON by a laser to active the win tile
/// </summary>
[RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
public class PowerTile : MonoBehaviour, ILaserTarget
{
    [SerializeField, Tooltip("Sprite for when the tile is active")]
    Sprite m_onSprite;

    [SerializeField, Tooltip("Sprite for when the tile is not active")]
    Sprite m_offSprite;

    SpriteRenderer m_renderer;

    WinTile m_winTile;

    bool m_isActive = false;
    bool m_previousState = false;
    bool m_isFirstFrame = true;

    /// <summary>
    /// Sets the tile to active/inactive
    /// </summary>
    public bool IsActive
    {
        get { return m_isActive; }

        set {
            m_isActive = value;
            m_winTile.IsActive = value;
            m_renderer.sprite = m_isActive ? m_onSprite : m_offSprite;
        }
    }

    /// <summary>
    /// Set references
    /// </summary>
    private void Awake()
    {
        m_renderer = GetComponent<SpriteRenderer>();

        if (m_onSprite == null || m_offSprite == null) {
            Debug.LogError($"{name} is missing either the ON or OFF sprite");
            return;
        }
    }

    /// <summary>
    /// Initialize
    /// </summary>
    private void Start()
    {
        m_winTile = FindObjectOfType<WinTile>();
        if(m_winTile == null) {
            Debug.LogError($"{name} could not find a reference to the Win Tile");
        }
    }

    /// <summary>
    /// Since on Update this power tile is being notified of being turned On/Off
    /// we want to play the sound on LateUpdate after a decision has been made of 
    /// which state it is currently in
    /// </summary>
    void LateUpdate()
    {
        var playSound = m_previousState != m_isActive;
        m_previousState = m_isActive;

        // Using m_isFirstFrame to see if the power was turned on the first frame
        // of the object being loaded to avoid playing the sound
        if (playSound && !m_isFirstFrame) {
            var clip = m_isActive ? AudioClipName.PowerOn : AudioClipName.PowerOff;
            AudioManager.Instance.Play2DSound(clip);
        }

        m_isFirstFrame = false;
    }

    /// <summary>
    /// Turns on the win tile
    /// </summary>
    /// <param name="source"></param>
    public void OnLaserEnter()
    {
        IsActive = true;
    }

    /// <summary>
    /// Turns off the win tile
    /// </summary>
    public void OnLaserExit()
    {
        IsActive = false;
    }
}
