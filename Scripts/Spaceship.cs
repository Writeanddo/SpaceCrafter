using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(Animator))]
public class Spaceship : MonoBehaviour
{
    [SerializeField, Tooltip("The sprite to use when playing the launching animation")]
    Sprite m_launchSprite;

    SpriteRenderer m_renderer;
    Animator m_animator;

    private void Awake()
    {
        m_renderer = GetComponent<SpriteRenderer>();
        m_animator = GetComponent<Animator>();
    }

    public void PlayLaunchAnimation()
    {
        m_renderer.sprite = m_launchSprite;
        m_animator.SetTrigger("Launch");
    }
}
