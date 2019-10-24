using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Keeps track of the tiles available to the player and quanity for each one
/// </summary>
public class InventoryManager : Singleton<InventoryManager>
{
    /// <summary>
    /// All the tiles the player could use
    /// </summary>
    [SerializeField, Tooltip("Collection of all the different types available to the player")]
    Tile[] m_prefabInventory;

    /// <summary>
    /// Keeps track of the currently selected inventory icon
    /// </summary>
    InventoryIcon m_icon;

    /// <summary>
    /// Mapps the icons so that we switch icons based on tile type
    /// </summary>
    Dictionary<TileType, InventoryIcon> m_iconMapping;

    /// <summary>
    /// A collection of all the unique prefabs in the inventory
    /// </summary>
    public Tile[] UniquePrefabs { get { return m_prefabInventory.Distinct().ToArray(); } } 

    /// <summary>
    /// Keeps track of the quanity left for a given tile type
    /// </summary>
    Dictionary<TileType, int> m_inventory;

    /// <summary>
    /// Maps the tile type with a prefab
    /// </summary>
    Dictionary<TileType, Tile> m_prefabMapping;    

    /// <summary>
    /// Initialize
    /// Inventory is first retrieved from GameManager
    /// and initialize when it is empty
    private void Start()
    {
        // Clone it less we duplicate values
        m_inventory = new Dictionary<TileType, int>(GameManager.Instance.Inventory);
        m_prefabMapping = new Dictionary<TileType, Tile>();

        // Nothing has been collected yet so we need to initialize as zero
        var initInventory = m_inventory.Count == 0;

        foreach (var prefab in UniquePrefabs) {
            if (prefab == null)
                continue;

            AddToTileMapping(prefab);

            if (initInventory && !m_inventory.ContainsKey(prefab.Type)) {
                m_inventory.Add(prefab.Type, 0);
            }
        }

        if (initInventory) {
            GameManager.Instance.Inventory = new Dictionary<TileType, int>(m_inventory);
        }

        // A hack to prevent softlocking due to being able to miss a tile
        if(GameManager.Instance.CurrentLevel == 4 && m_inventory[TileType.Ground] < 6) {
            while(m_inventory[TileType.Ground] < 6) {
                AddToInventory(TileType.Ground, false);
                GameManager.Instance.AddToInventory(TileType.Ground);
            }
        }

        // Map icons
        m_iconMapping = new Dictionary<TileType, InventoryIcon>();
        foreach (var icon in FindObjectsOfType<InventoryIcon>()) {
            if (!m_iconMapping.ContainsKey(icon.type)) {
                m_iconMapping.Add(icon.type, icon);
            }
        }

        // Player had selected one on the previous level so lets keep that one
        if(GameManager.Instance.LastIconSelected != TileType.Normal) {
            m_icon = m_iconMapping[GameManager.Instance.LastIconSelected];
            m_icon.IsSelected = true;
        }
    }

    /// <summary>
    /// Adds the new tile into the prefab mapping
    /// </summary>
    /// <param name="tile"></param>
    void AddToTileMapping(Tile tile)
    {
        if(tile != null && !m_prefabMapping.ContainsKey(tile.Type)) {
            m_prefabMapping.Add(tile.Type, tile);
        }
    }

    /// <summary>
    /// Increases the current count for the given tile type
    /// Makes the icon be marked as "selected"
    /// </summary>
    /// <param name="type"></param>
    public void AddToInventory(TileType type, bool setAsSelected = true)
    {
        if (m_inventory.ContainsKey(type)) {
            m_inventory[type]++;

            // Make sure it is enabled if it is currently not
            if (m_iconMapping != null && !m_iconMapping[type].IsEnabled) {
                m_iconMapping[type].EnableIcon(true);
            }

            // Only mark as selected if this is not the current type already selected
            if(m_icon != null && m_icon.type != type) {
                m_iconMapping[type].IsSelected = setAsSelected;
            }   
        }
    }

    /// <summary>
    /// Returns the current prefab to spawn if there are any left
    /// If this is the last one being placed then we set the inventory icon 
    /// to disabled
    /// </summary>
    /// <param name="dni">Do not inventory means do not remove from the inventory</param>
    /// <returns></returns>
    public Tile GetCurrentPrefabToSpawn(bool dni = false)
    {
        Tile prefab = null;

        if(m_icon != null && m_inventory.ContainsKey(m_icon.type)) {

            if(m_inventory[m_icon.type] > 0) {
                prefab = m_prefabMapping[m_icon.type];

                // Can inventory
                if(!dni) {
                    m_inventory[m_icon.type]--;
                }

                // Disable the icon
                if (m_inventory[m_icon.type] < 1) {
                    m_iconMapping[m_icon.type].EnableIcon(false);
                    SwitchToNextAvailablePrefab();
                }
            }
        }

        return prefab;
    }

    /// <summary>
    /// When the player runs out of the currently selected tiles
    /// We will auto switch to the next available set
    /// </summary>
    private void SwitchToNextAvailablePrefab()
    {
        foreach (var inventory in m_inventory) {
            if(inventory.Value > 0) {
                m_icon = m_iconMapping[inventory.Key];
                m_icon.IsSelected = true;
                break;
            }
        }
    }

    /// <summary>
    /// Changes the current prefab to the given type
    /// </summary>
    /// <param name="icon"></param>
    public void SetCurrentPrefab(InventoryIcon icon)
    {
        if(m_icon == icon) {
            return;
        }

        if(m_icon != null) {
            m_icon.IsSelected = false;
        }

        m_icon = icon;
        GameManager.Instance.LastIconSelected = icon.type;
    }

    /// <summary>
    /// Uses tile type to set the currently selected prefab
    /// </summary>
    /// <param name="type"></param>
    public void SetCurrentPrefab(TileType type)
    {
        if (m_icon != null && m_icon.type == type) {
            return;
        }

        if (m_icon != null) {
            m_icon.IsSelected = false;
        }

        m_icon = m_iconMapping[type];

        m_icon.IsSelected = true;
        GameManager.Instance.LastIconSelected = type;
    }

    /// <summary>
    /// Returns the current inventory for the given tile type
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public int TotalTileType(TileType type)
    {
        int total = 0;
        
        if (m_inventory.ContainsKey(type)) {
            total = m_inventory[type];
        }

        return total;
    }
}
