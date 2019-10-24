using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ShuttleStairs : MonoBehaviour
{
    bool m_isOpened;
    Animator m_animator;

    private void Awake()
    {
        m_animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Opens the stairs
    /// </summary>
    public void Open()
    {
        if (!m_isOpened) {
            m_isOpened = true;
            m_animator.SetTrigger("Open");
        }
    }
}
