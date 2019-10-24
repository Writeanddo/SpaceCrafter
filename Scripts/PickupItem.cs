using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sprites in a bubble waiting for the player to pick them up
/// Once picked up they notifie the inventory manager to increase 
/// the specified tile type. 
/// 
/// Picksup are ignored if the player is in a transition
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class PickupItem : MonoBehaviour
{
    [SerializeField, Tooltip("The tile type this represents")]
    TileType type;

    /// <summary>
    /// A reference to the player
    /// </summary>
    Player m_player;

    /// <summary>
    /// True once picked up
    /// </summary>
    bool m_isCollected;

    /// <summary>
    /// Set refs
    /// </summary>
    private void Start()
    {
        m_player = FindObjectOfType<Player>();
    }

    /// <summary>
    /// Trigger pickup only when the player is not in transition
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) {
            return;
        }
        
        if (!m_isCollected && !m_player.IsInARoutine) {
            m_isCollected = true;
            InventoryManager.Instance.AddToInventory(type);
            InventoryManager.Instance.SetCurrentPrefab(type);
            GameManager.Instance.AddToInventory(type);
            var source = AudioManager.Instance.Play2DSound(AudioClipName.Bubble);
            Destroy(gameObject);
        }
    }
}
