using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// A laser manager handles triggering all laser sources to fire a laser
/// and for each target they hit, those will be notified of collision
/// Since we have sources that can redirect the laser we need a way to 
/// pre-calculate everything a laser will touch only notifying the targets
/// that they were hit so that during LateUpdate everythying can be drawn
/// Thus all necessary calculations are done before hand
/// </summary>
public class LaserManager : Singleton<LaserManager>
{
    [SerializeField, Tooltip("A quick way to change/test the line rendere's Z axis")]
    float m_lineZPos = 5f;
    public float LineZPos { get { return m_lineZPos; } }

    List<LaserTile> m_laserTiles;
    List<ReflectorTile> m_reflectors;
    List<ILaserTarget> m_targets;

    private void Start()
    {
        m_laserTiles = FindObjectsOfType<LaserTile>().ToList();
        m_reflectors = new List<ReflectorTile>();
        m_targets = new List<ILaserTarget>();
    }

    private void Update()
    {
        // Ensure we are not working with nulls
        m_laserTiles = m_laserTiles.Where(x => x != null).ToList(); // can be destroyed once
        m_reflectors = FindObjectsOfType<ReflectorTile>().ToList(); // controlled by player

        // Makes sure targets and reflectors are all cleared
        // lasers tiles always shoot in the same direction so it does not matter
        m_reflectors.ForEach(r => r.ClearLasers());
        m_targets.ForEach(t => t.OnLaserExit()); // but first let them know they are not being lasered
        m_targets.Clear(); 

        // Calculate what all the laser will hits removing duplicates from the list
        foreach (var tile in m_laserTiles) {
            m_targets.AddRange(tile.CalculateTargets());
            m_targets.Distinct().ToList();
        }

        // Calculte the same for the reflector
        // Except that reflectors need a source (laser tile) to do so
        // therefore we if they are in the pre-extisting list of targets
        // then they have been hit

        // PROBLEM:
        // The order is not consistent an a reflector that was not a target before
        // might be a target now...there for we keep track of those skipped to see
        // if they become targets later and have them calculate their targets

        var skipped = new List<ILaserTarget>();
        foreach (var reflector in m_reflectors) {
            var target = reflector.GetComponent<ILaserTarget>();

            if (!m_targets.Contains(target)) {
                skipped.Add(target);
                continue;
            }

            // Get new targets
            m_targets.AddRange(reflector.CalculateTargets());
            m_targets.Distinct().ToList();

            // Now skipped ones might have been targeted
            var skippedTargets = new List<ILaserTarget>(skipped);

            foreach (var skippedTarget in skippedTargets) {

                // Still not targetted
                if (!m_targets.Contains(skippedTarget)) {
                    continue;
                }

                // Was hit
                skipped.Remove(skippedTarget);

                // Since we are working with reflectors we need to 
                // calculate where the beam will be redirected to
                var skippedReflector = skippedTarget.gameObject.GetComponent<ReflectorTile>();

                // Should never be null but to be safe
                if(skippedReflector != null) {
                    m_targets.AddRange(skippedReflector.CalculateTargets());
                    m_targets.Distinct().ToList();
                }
            }
        }

        // Now that we know absolutely all avaiable targets
        // we can notify those targets that they were hit
        m_targets.ForEach(t => t.OnLaserEnter());
    }
}
