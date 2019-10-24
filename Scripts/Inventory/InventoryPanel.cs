using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Creates the inventory panel with the currently unlocked tiles
/// as well as the current quantity
/// </summary>
public class InventoryPanel : MonoBehaviour
{
    //[SerializeField, Tooltip("Prefab for a generic inventory icon")]
    //InventoryIcon m_iconPrefab;

    ///// <summary>
    ///// Builds the panel with all available tile prefabs
    ///// </summary>
    //private void Start()
    //{
    //    if(m_iconPrefab == null) {
    //        Debug.LogErrorFormat($"{name} does not have an icon prefab assigned");
    //        return;
    //    }

    //    foreach(var prefab in InventoryManager.Instance.UniquePrefabs) {
    //        var icon = Instantiate(m_iconPrefab, transform).GetComponent<InventoryIcon>();
    //        icon.Setup(prefab);
    //    }
    //}
}
