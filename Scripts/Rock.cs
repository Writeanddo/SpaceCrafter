using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Rock : MonoBehaviour, ILaserTarget
{
    [SerializeField, Tooltip("Minimum speed to rotate at")]
    int m_minSpeed = 1;

    [SerializeField, Tooltip("Maximum speed to rotate at")]
    int m_maxSpeed = 18;

    [SerializeField, Tooltip("Direction at which to rotate at")]
    int m_rotationDir = -1;

    [SerializeField, Tooltip("Different sprites a rock could be")]
    Sprite[] m_sprites;

    [SerializeField, Tooltip("How long in seconds to play the animation before removing this")]
    float m_explosionTime = .5f;

    [SerializeField]
    SpriteRenderer m_renderer;

    [SerializeField]
    Transform m_childXform;

    float m_rotationSpeed = 20f;

    bool IsDestroyed { get; set; }

    bool BeingDestroyed { get; set; }

    Animator m_animator;

    /// <summary>
    /// Sets references
    /// Calculate speed
    /// </summary>
    private void Start()
    {
        if(m_renderer == null) {
            m_renderer = GetComponentInChildren<SpriteRenderer>();
        }

        if(m_childXform == null) {
            m_childXform = transform.GetChild(0);
        }

        if(m_sprites != null) {
            int rand = Random.Range(0, m_sprites.Length);
            m_renderer.sprite = m_sprites[rand];
        }

        m_animator = GetComponentInChildren<Animator>();
        m_rotationSpeed = Random.Range(m_minSpeed, m_maxSpeed) * m_rotationDir;
    }

    /// <summary>
    /// Triggers the explosion when being destroyed
    /// </summary>
    private void Update()
    {
        if(IsDestroyed && !BeingDestroyed) {
            BeingDestroyed = true;
            m_animator.SetTrigger("Explode");
            AudioManager.Instance.Play2DSound(AudioClipName.Explode);
            Destroy(gameObject, m_explosionTime);
        }
    }

    /// <summary>
    /// Rotate
    /// </summary>
    private void LateUpdate()
    {
        if (!BeingDestroyed) {
            m_childXform.Rotate(new Vector3(0f, 0f, 1f) * m_rotationSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Trigger destruction
    /// </summary>
    /// <param name="source"></param>
    public void OnLaserEnter()
    {
        if (!IsDestroyed) {
            IsDestroyed = true;
        }
    }

    /// <summary>
    /// Would be destroyed so ignore
    /// </summary>
    public void OnLaserExit()
    {
        // throw new System.NotImplementedException();
    }    
}
