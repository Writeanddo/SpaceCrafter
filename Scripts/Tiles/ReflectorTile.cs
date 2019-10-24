using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Reflects laser beams in all directions
/// </summary>
[RequireComponent(typeof(LaserBeamShooter))]
public class ReflectorTile : Tile, ILaserTarget, ILaserSource
{
    LaserBeamShooter m_laser;

    /// <summary>
    /// True when we need to calculate where lasers can be reflected to
    /// </summary>
    bool ReflectLaser { get; set; } = false;

    /// <summary>
    /// Sets the reference to the laser
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        m_laser = GetComponent<LaserBeamShooter>();
    }

    public void ClearLasers()
    {
        m_laser.ClearLaser();
    }

    /// <summary>
    /// Fire the laser or destroy itself
    /// Using late update to ensure that the lasers have been reset
    /// in the laser shooter's update 
    /// </summary>
    public List<ILaserTarget> CalculateTargets()
    {
        m_laser.Shoot(GetReflectedDiretions());
        return m_laser.Targets;
    }

    /// <summary>
    /// Overrides the request to remove the tiles to clear any lasers this may be reflecting
    /// </summary>
    protected override void OnRightClick()
    {
        ReflectLaser = false;
        m_laser.ClearLaser();
        base.OnRightClick();
    }

    /// <summary>
    /// Redirects the laser to the remaining cardinal directions based on where the source is located at
    /// </summary>
    /// <param name="source"></param>
    /// <param name="direction"></param>
    public void OnLaserEnter()
    {
        // now handle by the laser manager
        // Will acknowledge the hit so long as it is not in mid transition
        //if (!IsTransitioning) {
        //    ReflectLaser = true;
        //}
    }

    /// <summary>
    /// No action
    /// </summary>
    public void OnLaserExit()
    {
    }

    /// <summary>
    /// Returns a collection of directions the laser can be reflected to
    /// Any direction that contains a laser tile that has hit this tile
    /// will be ignored since the laser is supposed to be firing at this tile
    /// </summary>
    /// <returns></returns>
    Vector2[] GetReflectedDiretions()
    {
        List<Vector2> directions = Utilities.CaridanalPoints;

        var tempDirections = new List<Vector2>(directions);

        foreach (var direction in tempDirections) {
            var hit = m_laser.GetRaycastHitInDirection(direction);

            // Nothing hit, move on
            if (hit.collider == null) {
                continue;
            }

            // If we are hitting another laser source that might has hitten us first
            // then we don't want to fire back at them
            var source = hit.collider.GetComponent<ILaserSource>();
            if (source != null && source.HasHitTarget(this)) {
                directions.Remove(direction);
            }
        }

        return directions.ToArray();
    }

    /// <summary>
    /// True when this laser tile has hit the given target
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public bool HasHitTarget(ILaserTarget target)
    {
        return m_laser.HasTarget(target);
    }
}
