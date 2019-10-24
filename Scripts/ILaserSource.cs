using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A source where a beam of laser was fired from
/// </summary>
public interface ILaserSource
{
    GameObject gameObject { get; }
    bool HasHitTarget(ILaserTarget target);
    List<ILaserTarget> CalculateTargets();
}
