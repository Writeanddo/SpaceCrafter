using UnityEngine;

/// <summary>
/// Disable the node this is bellow this object so as to prevent
/// interactions with it since there's an object on top
/// </summary>
public class TileNodeDisabler : MonoBehaviour
{
    [SerializeField, Tooltip("The layermask for the Node Tile")]
    LayerMask m_nodeMask;

    /// <summary>
    /// Stores a reference to reacivate when this is destroyed
    /// </summary>
    TileNode m_node;

    /// <summary>
    /// Peforms a raycast on the center of this object to see if there's node
    /// underneath to remove it
    /// </summary>
    private void Start()
    {
        var origin = new Vector2(transform.position.x, transform.position.y);
        var end = new Vector2(origin.x, origin.y + .25f);
        var hit = Physics2D.Linecast(origin, end, m_nodeMask);

        if (hit.collider != null) {
            m_node = hit.collider.GetComponent<TileNode>();
            m_node.IsActive = false;
        }
    }

    /// <summary>
    /// Re-activate the node since the object is no longer on top of it
    /// </summary>
    private void OnDestroy()
    {
        if (m_node != null) {
            m_node.IsActive = true;
        }
    }
}
