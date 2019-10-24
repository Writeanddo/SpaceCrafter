using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A laser tile is the source from which a laser is fired from
/// </summary>
[RequireComponent(typeof(LaserBeamShooter), typeof(Animator))]
public class LaserTile : MonoBehaviour, ILaserTarget, ILaserSource
{
    [SerializeField, Tooltip("How long in seconds to play the animation before removing this")]
    float m_explosionTime = .5f;

    /// <summary>
    /// The laser shooter receives an array of directions to fire the laser at
    /// However, a laser tile only every shoots out of its barrel which is pointing down
    /// which we will assigned during Awake as we need the transform to be available
    /// </summary>
    /// 
    Vector2[] m_directions = new Vector2[1];

    LaserBeamShooter m_laser;

    Animator m_animator;

    bool IsDestroyed { get; set; } = false;
    bool BeingDestroyed { get; set; } = false;

    /// <summary>
    /// A reference to the laser source that shot at this tile
    /// </summary>
    ILaserSource m_laserSource;

    /// <summary>
    /// Set references
    /// </summary>
    private void Awake()
    {
        m_laser = GetComponent<LaserBeamShooter>();
        m_animator = GetComponentInChildren<Animator>();

        // By using the inverse of the transform's up we ensure the laser comes out from the barrel
        // regardless of orientation
        m_directions[0] = -transform.up;
    }

    /// <summary>
    /// Fire the laser or destroy itself
    /// Using late update to ensure that the lasers have been reset
    /// in the laser shooter's update 
    /// </summary>
    public List<ILaserTarget> CalculateTargets()
    {
        m_laser.Shoot(m_directions);
        return m_laser.Targets;
    }

    /// <summary>
    /// Wait until the lasers have updated to see if this was hit 
    /// and destroys iteslf
    /// </summary>
    private void LateUpdate()
    {
        if (IsDestroyed && !BeingDestroyed) {
            BeingDestroyed = true;
            m_animator.SetTrigger("Explode");
            AudioManager.Instance.Play2DSound(AudioClipName.Explode);
            Destroy(gameObject, m_explosionTime);
        }
    }

    /// <summary>
    /// Triggered when a laser has hit this tile
    /// However, we will delay the check until after the frame update
    /// as we might still be calculating how many targets where hit
    /// which means we might be ignoring this request to destroy the laser
    /// </summary>
    /// <param name="source"></param>
    /// <param name="direction"></param>
    public void OnLaserEnter()
    {
        // Setup to destroy on the next frame
        if (!IsDestroyed) {
            IsDestroyed = true;
        }
    }

    /// <summary>
    /// Nothing since it would've been destroyed but it is required by the interface
    /// </summary>
    public void OnLaserExit()
    {

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
