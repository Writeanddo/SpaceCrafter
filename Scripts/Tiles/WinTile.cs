using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// The tile the player must reach, when it is ON, to complete the level
/// </summary>
[RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
public class WinTile : MonoBehaviour
{
    [SerializeField, Tooltip("Sprite for when the tile is not active")]
    Sprite m_offSprite;

    [SerializeField, Tooltip("All the sprites to show when it is active")]
    Sprite[] m_activeSprites;

    //[SerializeField, Tooltip("How fast to play the active animation")]
    //float m_activeSpeed = .025f;

    SpriteRenderer m_renderer;

    bool m_isActive = false;

    bool m_hasWonTriggered = false;

    Player m_player;

    [SerializeField]
    Vector2 m_point;

    /// <summary>
    /// Queues up the sprites to cycle through
    /// </summary>
    Queue<Sprite> m_sprites;

    /// <summary>
    /// As a safety, sometimes the two positions are not matching
    /// so we use a collision detection as a backup
    /// </summary>
    bool m_isCollidingWithPlayer = false;

    /// <summary>
    /// Sets the tile to active/inactive
    /// </summary>
    public bool IsActive
    {
        get { return m_isActive; }

        set {
            m_isActive = value;

            // Active is handled in the routine
            if (!m_isActive) {
                m_renderer.sprite = m_offSprite;
            }
        }
    }

    /// <summary>
    /// Set refs 
    /// Starts active as long as there are no power tiles
    /// </summary>
    private void Awake()
    {
        m_renderer = GetComponent<SpriteRenderer>();

        if (m_activeSprites == null || m_activeSprites.Length == 0 || m_offSprite == null) {
            Debug.LogError($"{name} is missing either the ON or OFF sprite");
            return;
        }

        IsActive = FindObjectOfType<PowerTile>() == null;
        m_point = new Vector2(transform.position.x, transform.position.y);
        m_sprites = new Queue<Sprite>(m_activeSprites);
    }

    /// <summary>
    /// Initialize
    /// </summary>
    private void Start()
    {
        m_player = FindObjectOfType<Player>();
        // StartCoroutine(ActiveSpriteAnimationRoutine());
    }

    /// <summary>
    /// If the player's current position is that of the win tile 
    /// Then notifies the player that they are in a win condition
    /// </summary>
    private void LateUpdate()
    {
        if (m_player == null) {
            return;
        }

        var origin = new Vector2(m_player.transform.position.x, m_player.transform.position.y);

        // Play the active sprites
        if (IsActive) {
            var sprite = m_sprites.Dequeue();
            m_renderer.sprite = sprite;
            m_sprites.Enqueue(sprite);
        }

        if ((origin == m_point || m_isCollidingWithPlayer) && IsActive) {
            if (!m_player.IsInARoutine) {
                m_hasWonTriggered = true;
                m_player.MovementDisabled = true;
                m_player.HasWon = true;
            }
        }
    }

    /// <summary>
    /// Cycles through the sprites that represents the tile is active
    /// </summary>
    /// <returns></returns>
    IEnumerator ActiveSpriteAnimationRoutine()
    {
        var sprites = new Queue<Sprite>(m_activeSprites);
        
        do {
            // We want this to run after the frame calculations where made
            // as that will indicate whether it is still active or not
            // we wait one more frame to play every other frame
            yield return new WaitForEndOfFrame();
            Debug.Log($"IsActive {IsActive}");

            if (IsActive) {
                var sprite = sprites.Dequeue();
                m_renderer.sprite = sprite;
                sprites.Enqueue(sprite);
            }

            yield return new WaitForEndOfFrame();
        } while (sprites.Count > 0);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        m_isCollidingWithPlayer = collision.CompareTag("Player");
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        m_isCollidingWithPlayer = false;
    }
}
