using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A tile node is where the player clicks on to add a tile
/// </summary>
[RequireComponent(typeof(MouseClickHandler), typeof(Collider2D))]
public class TileNode : MonoBehaviour
{
    MouseClickHandler m_mouseClickHandler;

    Collider2D m_collider;

    /// <summary>
    /// Displays the currently selected tile and whether it can be placed or not
    /// </summary>
    [SerializeField]
    SpriteRenderer m_previewTileRenderer;

    [SerializeField, Tooltip("Color to use when the tile cannot be placed")]
    Color m_unavailableColor;

    /// <summary>
    /// Changes the status of the collider to prevent further interaction with the node
    /// </summary>
    /// <param name="enabled"></param>
    public bool IsActive
    {
        set{ m_collider.enabled = value; }
    }

    /// <summary>
    /// Set references
    /// </summary>
    private void Awake()
    {
        m_mouseClickHandler = GetComponent<MouseClickHandler>();
        m_collider = GetComponent<Collider2D>();

        if (m_previewTileRenderer == null)
            m_previewTileRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    /// <summary>
    /// Initialize
    /// </summary>
    private void Start()
    {
        m_mouseClickHandler.OnLeftClickEvent = OnLeftClick;
        m_mouseClickHandler.OnMouseEnterEvent = OnMouseEnterEvent;
        m_mouseClickHandler.OnMouseExitEvent = OnMouseExitEvent;
    }

    /// <summary>
    /// Shows the preview sprite
    /// </summary>
    private void OnMouseEnterEvent()
    {
        if (m_previewTileRenderer == null)
            return;

        Tile tile = InventoryManager.Instance.GetCurrentPrefabToSpawn(true);
        if (tile == null)
            return;
        
        m_previewTileRenderer.sprite = tile.TileSprite;

        Color color = new Color(255, 255, 255, m_previewTileRenderer.color.a);
        if (!TileNodeManager.Instance.NodeIsAvailableForTilePlacement(this)) {
            color = m_unavailableColor;
        }

        m_previewTileRenderer.color = color;
    }

    /// <summary>
    /// Removes the preview sprite
    /// </summary>
    private void OnMouseExitEvent()
    {
        if(m_previewTileRenderer != null) {
            m_previewTileRenderer.sprite = null;
        }
    }

    /// <summary>
    /// Requests the placement of a tile
    /// </summary>
    private void OnLeftClick()
    {
        TileNodeManager.Instance.OnNodeClicked(this);
    }
}
