using UnityEngine;

/// <summary>
/// Shrinks/Grows the object to make it look like it is hovering
/// </summary>
public class TopDownHoverEffect : MonoBehaviour
{
    [SerializeField, Tooltip("Minimum scale it can shrink")]
    float m_minSize = 0.75f;

    [SerializeField, Tooltip("Maximum scale it can shrink")]
    float m_maxSize = 1f;

    [SerializeField, Tooltip("How fast is cycles in growing and shrinking")]
    float m_growSpeed = 1f;

    /// <summary>
    /// Trigger the effet
    /// </summary>
    private void Update()
    {
        float range = m_maxSize - m_minSize;
        float scale = (float)((Mathf.Sin(Time.time * m_growSpeed) + 1.0) / 2.0 * range + m_minSize);
        transform.localScale = Vector2.one * scale;
    }
}
