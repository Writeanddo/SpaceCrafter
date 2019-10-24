using UnityEngine;

/// <summary>
/// Handles the effects a laser beam has
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class LaserBeam : MonoBehaviour
{
    [SerializeField, Tooltip("Color for the ON tick cycle")]
    Color m_tickOnColor = new Color(255, 0, 0, 1f);

    [SerializeField, Tooltip("Color for the OFF tick cycle")]
    Color m_tickOffColor = new Color(200, 0, 0, 0.5f);

    [SerializeField, Tooltip("Minimum scale it can shrink")]
    float m_minSize = 0.75f;

    [SerializeField, Tooltip("Maximum scale it can shrink")]
    float m_maxSize = 1f;

    [SerializeField, Tooltip("How fast is cycles in growing and shrinking")]
    float m_growSpeed = 1f;

    LineRenderer m_lineRenderer;
    float m_startingWidth;

    private void Awake()
    {
        m_lineRenderer = GetComponent<LineRenderer>();
        m_startingWidth = m_lineRenderer.startWidth;
    }

    /// <summary>
    /// Toggles the current tile sprite being renderer based on the Global timer's current cycle
    /// </summary>
    private void LateUpdate()
    {
        float range = m_maxSize - m_minSize;
        float scale = (float)((Mathf.Sin(Time.time * m_growSpeed) + 1.0) / 2.0 * range + m_minSize);

        m_lineRenderer.startWidth = m_startingWidth * scale;
        m_lineRenderer.endWidth = m_startingWidth * scale;

        var color = m_tickOffColor;
        if (GameManager.Instance.CycleOn) {
            color = m_tickOnColor;
        }

        m_lineRenderer.startColor = color;
        m_lineRenderer.endColor = color;
    }
}
