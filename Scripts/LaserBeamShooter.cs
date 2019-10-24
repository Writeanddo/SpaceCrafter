using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The laser beam shooter is given direction at which to fire laser
/// Each laser checks for collision with an object it can interact with
/// Objects interacted with are notified of being shot by this beam shooter
/// </summary>
[RequireComponent(typeof(Collider2D), typeof(ILaserSource))]
public class LaserBeamShooter : MonoBehaviour
{
    [SerializeField, Tooltip("Layer mask for objects the lasers can hit")]
    LayerMask m_laserTargetMask;

    [SerializeField, Tooltip("The prefab for the laser beam")]
    GameObject m_laserBeamPrefab;

    [SerializeField, Range(0, 7), Tooltip("Maximum number of tiles the laser fires at")]
    int m_laserDistance = 5;

    Collider2D m_collider;

    /// <summary>
    /// Keeps track off line renderers spawned
    /// </summary>
    List<LineRenderer> m_lineRenderers;

    /// <summary>
    /// Stores the targets this laser shooter hit
    /// </summary>
    public List<ILaserTarget> Targets { get; private set; }

    /// <summary>
    /// Holds the positions for each line renderer 
    /// </summary>
    List<Vector3[]> m_positions;

    /// <summary>
    /// References
    /// </summary>
    private void Awake()
    {
        m_lineRenderers = new List<LineRenderer>();
        Targets = new List<ILaserTarget>();
        m_collider = GetComponent<Collider2D>();
        m_positions = new List<Vector3[]>();
    }

    /// <summary>
    /// Always reset at the beginning of each frame
    /// </summary>
    private void Update()
    {
        ClearLaser();
    }

    /// <summary>
    /// Resets both the lasers and the targets
    /// </summary>
    public void ClearLaser()
    {
        ResetLasers();
        ResetTargets();
    }

    /// <summary>
    /// Resets all line renderer's count to not renderer the lines
    /// </summary>
    public void ResetLasers()
    {
        foreach (var lineRenderer in m_lineRenderers) {
            if (lineRenderer != null) {
                lineRenderer.positionCount = 0;
            }
        }
    }

    /// <summary>
    /// Notifies the all objcts this shooter has hit that the laser is no longer hitting it
    /// </summary>
    public void ResetTargets()
    {
        Targets.Clear();
    }

    /// <summary>
    /// Draws a laser for each direction we have
    /// </summary>
    public void Shoot(Vector2[] directions)
    {
        m_positions.Clear();
        Targets.Clear();

        for (int i = 0; i < directions.Length; i++) {            
            var direction = directions[i];
            var lineRenderer = GetLineRendererAtPosition(i);
            var hit = GetRaycastHitInDirection(direction);

            var origin = GetLineOrigin(direction);
            var end = GetLineEnd(direction, origin);

            // Ensure the first point is always at the origin
            var positions = new List<Vector3>()
            {
                new Vector3(origin.x, origin.y, LaserManager.Instance.LineZPos)
            };

            // Nothing was hit so fire until the end
            if (hit.collider == null) {
                positions.Add(new Vector3(end.x, end.y, LaserManager.Instance.LineZPos));

            } else {
                // Notify of interaction if the hit cares about it
                var interactible = hit.collider.GetComponent<ILaserTarget>();

                if (interactible != null) {
                    Targets.Add(interactible);
                }

                // Force Z to be 1 to appear infront of things                
                positions.Add( new Vector3(hit.point.x, hit.point.y, LaserManager.Instance.LineZPos));
            }

            m_positions.Insert(i, positions.ToArray());            
        }
    }

    /// <summary>
    /// Draws the locations of the lasers
    /// Each position is a collection of positions for a one line renderer
    /// </summary>
    private void LateUpdate()
    {
        for (int i = 0; i < m_positions.Count; i++) {
            var positions = m_positions[i];
            var lineRenderer = GetLineRendererAtPosition(i);

            lineRenderer.positionCount = positions.Length;
            lineRenderer.SetPositions(positions);
        }

        // Reset for next frame
        m_positions.Clear();
    }

    /// <summary>
    /// Returns the first available hit in the given direction
    /// that this laser will make contact with
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    public RaycastHit2D GetRaycastHitInDirection(Vector2 direction)
    {
        LayerMask layerMask = gameObject.layer;
        gameObject.layer = LayerMask.NameToLayer("Default");

        Vector2 origin = GetLineOrigin(direction);
        Vector2 end = GetLineEnd(direction, origin);

        var hit = Physics2D.Linecast(origin, end, m_laserTargetMask);

        // Reset layer
        gameObject.layer = layerMask;

        return hit;
    }

    /// <summary>
    /// Get's or Instantiates a line render at the given position
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    private LineRenderer GetLineRendererAtPosition(int i)
    {
        LineRenderer lineRenderer = null;

        if (i > m_lineRenderers.Count - 1 || m_lineRenderers[i] == null) {
            lineRenderer = Instantiate(m_laserBeamPrefab, transform).GetComponent<LineRenderer>();
            lineRenderer.name = $"LineRenderer_{i}";
            m_lineRenderers.Insert(i, lineRenderer);
        } else {
            lineRenderer = m_lineRenderers[i];
        }

        return lineRenderer;
    }

    /// <summary>
    /// Calculate origin based on direction
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    private Vector2 GetLineOrigin(Vector2 direction)
    {
        var origin = m_collider.bounds.center;

        if (direction == Vector2.up)
            origin = new Vector2(m_collider.bounds.center.x, m_collider.bounds.max.y);

        else if (direction == Vector2.left)
            origin = new Vector2(m_collider.bounds.min.x, m_collider.bounds.center.y);

        else if (direction == Vector2.down)
            origin = new Vector2(m_collider.bounds.center.x, m_collider.bounds.min.y);

        else if (direction == Vector2.right)
            origin = new Vector2(m_collider.bounds.max.x, m_collider.bounds.center.y);

        return origin;
    }

    /// <summary>
    /// Calculate end based on direction
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="origin"></param>
    /// <returns></returns>
    private Vector3 GetLineEnd(Vector2 direction, Vector3 origin)
    {
        var end = origin * m_laserDistance;
        if (direction == Vector2.up || direction == Vector2.down)
            end = new Vector2(origin.x, origin.y + (m_laserDistance * direction.y));

        else if (direction == Vector2.left || direction == Vector2.right)
            end = new Vector2(origin.x + (m_laserDistance * direction.x), origin.y);
        return end;
    }

    /// <summary>
    /// True when the given target has been hit by this laser
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public bool HasTarget(ILaserTarget target)
    {
        return Targets.Contains(target);
    }
}
