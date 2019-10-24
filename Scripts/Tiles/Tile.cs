using System.Collections;
using UnityEngine;

public enum TileType
{
    Normal,
    Ground,
    Blocker,
    Reflector,
}

/// <summary>
/// Base class for all tiles that the player can directly interact with by clicking on them
/// </summary>
[RequireComponent(typeof(MouseClickHandler), typeof(TileAnimator), typeof(Collider2D))]
public class Tile : MonoBehaviour
{
    /// <summary>
    /// The type of tile this is
    /// </summary>
    [SerializeField]
    TileType m_type = TileType.Normal;
    public TileType Type { get { return m_type; } }

    [SerializeField, Range(0.1f, 1f), Tooltip("Minimum perecent the tile shrinks when removed")]
    float m_shrinkSize = 0.25f;

    [SerializeField, Tooltip("How long it takes the tile to shrink")]
    float m_shrinkTime = .5f;

    [SerializeField, Tooltip("How long it takes the tile to grow")]
    float m_growTime = .25f;

    [SerializeField]
    Transform m_spriteXForm;

    [SerializeField, Tooltip("The renderer that contains the unique sprite image for this tile")]
    protected SpriteRenderer m_renderer;

    [SerializeField, Tooltip("The renderer that contains the tile's background sprite image")]
    SpriteRenderer m_bgRenderer;

    [SerializeField, Tooltip("BG Sprite for when not selected")]
    Sprite m_bgNormalSprite;

    [SerializeField, Tooltip("BG Sprite for when the tile is selected")]
    Sprite m_bgSelectedSprite;

    protected Collider2D m_collider;

    /// <summary>
    /// Returns the sprite associated with the tile
    /// </summary>
    public Sprite TileSprite
    {
        get {
            Sprite sprite = null;

            if (m_renderer != null) {
                sprite = m_renderer.sprite;
            }

            return sprite;
        }
    }

    MouseClickHandler m_mouseClickHandler;

    Vector3 m_originalScale;

    /// <summary>
    /// True while playing the animation of being placed
    /// </summary>
    public bool IsShrinking { get; private set; }

    /// <summary>
    /// True while playing the animation being removed
    /// </summary>
    public bool IsGrowing { get; private set; }

    /// <summary>
    /// True when still adding or removing itself
    /// </summary>
    public bool IsTransitioning { get { return IsGrowing || IsShrinking; } }

    /// <summary>
    /// Set references
    /// </summary>
    protected virtual void Awake()
    {
        if(m_spriteXForm == null) {
            m_spriteXForm = transform.GetChild(0);
        }

        if (m_renderer == null || m_bgRenderer == null) {
            Debug.LogError($"{name} is missing one or more renderer references");
        }

        m_mouseClickHandler = GetComponent<MouseClickHandler>();
        m_collider = GetComponent<Collider2D>();
        m_originalScale = m_spriteXForm.localScale;
    }

    /// <summary>
    /// Initialize
    /// </summary>
    protected virtual void Start()
    {
        m_mouseClickHandler.OnRightClickEvent = OnRightClick;
        m_mouseClickHandler.OnMouseEnterEvent = OnMouseEnterEvent;
        m_mouseClickHandler.OnMouseExitEvent = OnMouseExitEvent;
        StartCoroutine(GrowRoutine());
    }

    /// <summary>
    /// Removes the tile
    /// </summary>
    protected virtual void OnRightClick()
    {
        TriggerRemove();
    }

    /// <summary>
    /// Triggers the tile to be removed
    /// </summary>
    public void TriggerRemove(bool setAsSelected = true)
    {
        if (!IsTransitioning) {
            StartCoroutine(ShrinkRoutine(setAsSelected));
        }
    }

    /// <summary>
    /// Highlights the tile
    /// </summary>
    protected virtual void OnMouseEnterEvent()
    {
        // SetBackgroundSpriteColor(Color.cyan);
        if (m_bgRenderer != null && m_bgSelectedSprite != null) {
            m_bgRenderer.sprite = m_bgSelectedSprite;
        }
    }

    /// <summary>
    /// Removes the tile's highlight
    /// </summary>
    protected virtual void OnMouseExitEvent()
    {
        // SetBackgroundSpriteColor(Color.white);
        if (m_bgRenderer != null && m_bgNormalSprite != null) {
            m_bgRenderer.sprite = m_bgNormalSprite;
        }
    }

    /// <summary>
    /// Updates the color of the background sprite to the given one
    /// </summary>
    /// <param name="color"></param>
    protected void SetBackgroundSpriteColor(Color color)
    {
        if(m_bgRenderer != null) {
            m_bgRenderer.color = color;
        }
    }

    /// <summary>
    /// Changes the active status of the sprite container
    /// </summary>
    /// <param name="enabled"></param>
    protected void SetSpriteState(bool enabled)
    {
        m_spriteXForm.gameObject.SetActive(enabled);
    }

    /// <summary>
    /// Triggers a change in scale to make the tile appear to be shriking
    /// </summary>
    /// <returns></returns>
    IEnumerator ShrinkRoutine(bool setAsSelected = false)
    {
        IsShrinking = true;

        //Ensure the current scale is the starting one
        m_spriteXForm.localScale = m_originalScale;
        SetSpriteState(true);

        var targetScale = m_originalScale * m_shrinkSize;
        yield return StartCoroutine(ChangeScaleRoutine(targetScale, m_shrinkTime));

        SetSpriteState(false);

        // Add it back to the inventory and make it be the newly selected one
        InventoryManager.Instance.AddToInventory(Type, setAsSelected);
        TileNodeManager.Instance.OnTileRemoved(this);

        if (setAsSelected)
            InventoryManager.Instance.SetCurrentPrefab(Type);        

        Destroy(gameObject);
    }

    /// <summary>
    /// Triggers a change in scale to make the tile appear to be growing
    /// </summary>
    /// <returns></returns>
    IEnumerator GrowRoutine()
    {
        IsGrowing = true;

        // Ensure the current scale is that of being shrunk
        m_spriteXForm.localScale = m_originalScale * m_shrinkSize;
        SetSpriteState(true);

        yield return StartCoroutine(ChangeScaleRoutine(m_originalScale, m_growTime));
        
        IsGrowing = false;
    }

    /// <summary>
    /// Smoothly changes the scale of the tile to match the given target
    /// </summary>
    /// <param name="targetScale"></param>
    /// <returns></returns>
    IEnumerator ChangeScaleRoutine(Vector3 targetScale, float time)
    {
        var distance = Vector3.Distance(m_spriteXForm.localScale, targetScale);
        var speed = Mathf.Abs(distance) / time;

        // Do/While so that we can avoid the "yield" statement once the goal is reached
        // thus preventing the slight pause before the next action happens
        do {
            yield return new WaitForEndOfFrame();
            m_spriteXForm.localScale = Vector3.Lerp(
                m_spriteXForm.localScale,
                targetScale,
                speed * Time.deltaTime
            );
        } while (Vector3.Distance(m_spriteXForm.localScale, targetScale) > 0.1f);

        m_spriteXForm.localScale = targetScale;
    }
}