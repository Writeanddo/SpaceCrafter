using System.Collections;
using UnityEngine;

/// <summary>
/// Handles the shrink and grow transition effects played on level load/exit
/// </summary>
public class TileTransitionAnimations : MonoBehaviour
{
    [SerializeField]
    Transform m_spriteXForm;

    /// <summary>
    /// A reference to the starting scale before anything is changed
    /// </summary>
    Vector3 m_originalScale;

    /// <summary>
    /// True while playing the animation of being placed
    /// </summary>
    public bool IsShrinking { get; private set; }

    /// <summary>
    /// True while playing the animation being removed
    /// </summary>
    public bool IsGrowing { get; private set; }

    /// <summary>
    /// True when still adding or removing itself
    /// </summary>
    public bool IsTransitioning { get { return IsGrowing || IsShrinking; } }

    /// <summary>
    /// Set references
    /// </summary>
    protected virtual void Awake()
    {
        if (m_spriteXForm == null) {
            m_spriteXForm = transform;
        }

        m_originalScale = m_spriteXForm.localScale;
    }

    /// <summary>
    /// Triggers the object to shrink and be removed
    /// </summary>
    public void Shrink(float size, float time)
    {
        if(!IsShrinking)
            StartCoroutine(ShrinkRoutine(size, time));
    }

    /// <summary>
    /// Triggers the object to grow
    /// </summary>
    public void Grow(float size, float time)
    {
        if (!IsGrowing)
            StartCoroutine(GrowRoutine(size, time));
    }

    /// <summary>
    /// Triggers a change in scale to make the tile appear to be shriking
    /// </summary>
    /// <returns></returns>
    IEnumerator ShrinkRoutine(float size, float time)
    {
        IsShrinking = true;

        // Ensure the current scale is the starting one
        m_spriteXForm.localScale = m_originalScale;

        var targetScale = m_originalScale * size;
        yield return StartCoroutine(ChangeScaleRoutine(targetScale, time));

        IsShrinking = false;

        gameObject.SetActive(false);
    }

    /// <summary>
    /// Triggers a change in scale to make the tile appear to be growing
    /// </summary>
    /// <returns></returns>
    IEnumerator GrowRoutine(float startSize, float time)
    {
        IsGrowing = true;

        // Ensure the current scale is that of being shrunk
        m_spriteXForm.localScale = m_originalScale * startSize;

        yield return StartCoroutine(ChangeScaleRoutine(m_originalScale, time));

        IsGrowing = false;
    }

    /// <summary>
    /// Smoothly changes the scale of the tile to match the given target
    /// </summary>
    /// <param name="targetScale"></param>
    /// <returns></returns>
    IEnumerator ChangeScaleRoutine(Vector3 targetScale, float time)
    {
        var distance = Vector3.Distance(m_spriteXForm.localScale, targetScale);
        var speed = Mathf.Abs(distance) / time;

        // Do/While so that we can avoid the "yield" statement once the goal is reached
        // thus preventing the slight pause before the next action happens
        do {
            yield return new WaitForEndOfFrame();
            m_spriteXForm.localScale = Vector3.Lerp(
                m_spriteXForm.localScale,
                targetScale,
                speed * Time.deltaTime
            );
        } while (Vector3.Distance(m_spriteXForm.localScale, targetScale) > 0.1f);

        m_spriteXForm.localScale = targetScale;
    }
}
