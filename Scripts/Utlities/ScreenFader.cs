using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ScreenFader : MonoBehaviour
{
    /// <summary>
    /// How quickly to fade the in/out
    /// </summary>
    [SerializeField, Range(0f, 30f)]
    float m_fadeTime = 1f;

    /// <summary>
    /// A reference to the fade routine to know it is running
    /// </summary>
    IEnumerator m_fadeRoutine;

    /// <summary>
    /// A reference to the sprite renderer to fade in/out
    /// </summary>
    Image m_image;

    /// <summary>
    /// Set reference
    /// </summary>
    void Awake()
    {
        m_image = GetComponent<Image>();
    }

    /// <summary>
    /// Triggers fade in to 100% alpha
    /// </summary>
    public void FadeIn()
    {
        TriggerFadeRoutine(0f, 1f);
    }

    /// <summary>
    /// Triggers a fade out to 0% alpha
    /// </summary>
    public void FadeOut()
    {
        TriggerFadeRoutine(1f, 0f);
    }

    /// <summary>
    /// Stops the current routine if running and starts a new one wiht the given values
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    void TriggerFadeRoutine(float start, float end)
    {
        if (m_fadeRoutine != null) {
            StopCoroutine(m_fadeRoutine);
        }

        m_fadeRoutine = FadeRoutine(start, end, m_fadeTime);
        StartCoroutine(m_fadeRoutine);
    }

    /// <summary>
    /// Fades the current renderer's alpha from the start to the end values
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="speed"></param>
    /// <returns></returns>
    IEnumerator FadeRoutine(float start, float end, float speed = 1f)
    {
        Color color = m_image.color;
        Color cColor = new Color(color.r, color.g, color.b, start);
        Color nColor = new Color(color.r, color.g, color.b, end);

        // Ensure the current alpha is the "start" alpha
        m_image.color = cColor;

        float t = 0f;
        float i = Time.deltaTime / speed;

        while (t < 1) {
            m_image.color = Color.Lerp(cColor, nColor, t);
            t += i;
            yield return new WaitForEndOfFrame();
        }

    }
}
