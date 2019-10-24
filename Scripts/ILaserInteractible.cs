using UnityEngine;

public interface ILaserTarget
{
    GameObject gameObject { get; }

    /// <summary>
    /// Triggered when laser has collided with the object
    /// </summary>
    /// <param name="source"></param>
    void OnLaserEnter();

    /// <summary>
    /// Triggered when the laser is no longer hitting it
    /// </summary>
    void OnLaserExit();
}
