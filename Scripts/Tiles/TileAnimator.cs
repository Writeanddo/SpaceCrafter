using UnityEngine;

/// <summary>
/// Handls swapping tile sprites to give the illusion of being animated
/// based on whether it is active or not and the current global timer cycle
/// </summary>
public class TileAnimator : MonoBehaviour
{
    [SerializeField, Tooltip("The renderer to animate")]
    SpriteRenderer m_renderer;

    [SerializeField, Tooltip("Sprite to use when the cycle is ON")]
    Sprite m_cycleOnSprite;

    [SerializeField, Tooltip("Sprite to use when the cycle is OFF")]
    Sprite m_cycleOffSprite;

    /// <summary>
    /// Defaults to being true
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Set references
    /// </summary>
    private void Awake()
    {
        if (m_renderer == null) {
            m_renderer = GetComponent<SpriteRenderer>();
        }

        if (m_renderer == null) {
            m_renderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (m_renderer == null) {
            Debug.LogError($"{name} is missing a sprite renderer");
        }
    }
    
    /// <summary>
    /// Toggles the current tile sprite being renderer based on the Global timer's current cycle
    /// </summary>
    private void LateUpdate()
    {
        if (IsActive) {
            m_renderer.sprite = GameManager.Instance.CycleOn? m_cycleOnSprite : m_cycleOffSprite;
        }
    }
}
