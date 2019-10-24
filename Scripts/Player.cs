using System.Collections;
using UnityEngine;

/// <summary>
/// The player is the avatar controlled by the player which moves around in a grid pattern
/// The player can walk off the board, be burned, or reach the end goal to win the level
/// The player can also collect new tiles to place on the board as long as they are not:
///     - moving
///     - falling
///     - burned
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class Player : MonoBehaviour, ILaserTarget
{
    [SerializeField, Tooltip("Layer mask for objects the player cannot walk through")]
    LayerMask m_collisionMask;

    /// <summary>
    /// How fast the player moves
    /// </summary>
    [SerializeField, Tooltip("Movement speed")]
    float m_speed = 5f;

    /// <summary>
    /// How close to the movement destination before snapping into place
    /// </summary>
    [SerializeField, Tooltip("How close to the center of the tile before considering movement done")]
    float m_minDistance = .001f;

    [SerializeField, Tooltip("How long in seconds is the falling animation")]
    float m_fallAnimationLength = 0.98f;

    [SerializeField, Tooltip("How long in seconds is the burning animation")]
    float m_burningAnimationLength = 0.4f;

    /// <summary>
    /// Stores where the player needs to move to before control is returned
    /// </summary>
    [SerializeField]
    Vector2 m_startingPoint;

    /// <summary>
    /// Stores where player first spawned
    /// </summary>
    [SerializeField]
    Vector2 m_spawnPoint;

    /// <summary>
    /// Point of exist should be the spaceship's cockpit
    /// </summary>
    [SerializeField]
    Vector2 m_exitPoint;

    /// <summary>
    /// Keeps track of the previous input
    /// </summary>
    Vector2 m_previousInput;

    /// <summary>
    /// Keeps track of the current direction the player is moving
    /// </summary>
    Vector2 m_currentDirection;

    /// <summary>
    /// A ref to the rigid body component
    /// </summary>
    Rigidbody2D m_rigidbody;

    /// <summary>
    /// A ref to the animator component
    /// </summary>
    Animator m_animator;

    /// <summary>
    /// The GameObject the player is currently on
    /// </summary>
    GameObject m_onGameObject;

    /// <summary>
    /// True to prevent the player from moving
    /// </summary>
    public bool MovementDisabled { get; set; } = true;

    /// <summary>
    /// True while the player is moving
    /// </summary>
    public bool IsMoving { get; internal set; }

    /// <summary>
    /// True while the player is in the falling routine
    /// </summary>
    public bool IsFalling { get; private set; }

    /// <summary>
    /// True while the player is in the burned routine
    /// </summary>
    public bool IsBurned { get; private set; }

    /// <summary>
    /// True when the player in the win routine
    /// </summary>
    public bool HasWon { get; set; }

    /// <summary>
    /// True while the avatar is auto moving to the starting position
    /// Defaults to true since whenever a level is loaded the player 
    /// must transition to the starting point
    /// </summary>
    public bool IsSpawning { get; private set; } = false;

    /// <summary>
    /// True while the player is walking into the ship
    /// </summary>
    public bool IsExiting { get; private set; } = false;

    /// <summary>
    /// True when the request to end the level has been made
    /// </summary>
    bool m_hasWonTriggered = false;

    /// <summary>
    /// True while a laser is touching the player
    /// </summary>
    bool m_isTouchingLaser = false;

    /// <summary>
    /// True when any of the routine boolens is active
    /// </summary>
    public bool IsInARoutine
    {
        get {
            return IsSpawning || IsMoving || IsFalling || IsBurned || HasWon || IsExiting;
        }
    }

    /// <summary>
    /// Set referenceses
    /// </summary>
    private void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody2D>();
        m_animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Handles player input, movement, and effects
    /// While falling, burned, or in a winning state controls are disabled
    /// </summary>
    private void Update()
    {
        m_animator.SetBool("IsMoving", IsMoving);

        if (!m_hasWonTriggered && HasWon && !IsMoving) {
            m_hasWonTriggered = true;
            GameManager.Instance.LevelCompleted();
            return;
        }

        // Wait until the routine is over 
        if (IsInARoutine) {
            return;
        }

        // Player was burned while not moving
        // probably after removing a blocker or redirecting lasers
        if (m_isTouchingLaser && !IsBurned) {
            TriggerBurnedRoutine();
        }

        // Player might be idled so let's test that the tile they are on still exist
        if (IsPlayerFalling()) {
            TriggerFallRoutine();
            return; // falling
        }

        if (MovementDisabled) {
            return;
        }

        Movement();
        m_animator.SetBool("IsMoving", IsMoving);
    }

    /// <summary>
    /// Checks if the game object that the player is currently on
    /// does not exist, is an empty node, or is a tile being removed
    /// Returns true when any of those conditions are true
    /// </summary>
    bool IsPlayerFalling()
    {
        // Not falling if out of bounds as the players starts out of bounds
        if(transform.position.x < 0) {
            return false;
        }

        // Cannot tell at the moment so let's say they are not
        if (m_onGameObject == null) {
            return false;
        }

        var isFalling = false;

        if (m_onGameObject.CompareTag("NodeTile")) {
            isFalling = true;

            // Ensure the tile is not shrinking 
        } else if (m_onGameObject.CompareTag("GroundTile")) {
            var tile = m_onGameObject.GetComponent<Tile>();
            isFalling = tile == null || tile.IsShrinking;
        }

        return isFalling;
    }

    /// <summary>
    /// Decide direction player can move based on input
    /// Rotates the player to look at the desired direction
    /// Trigger the move routine if the desired destination is available
    /// </summary>
    void Movement()
    {
        Vector2 input = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );

        Vector2 direction = GetDirectionFromInput(input);

        // Not moving
        if (direction == Vector2.zero) {
            return;
        }

        Vector2 position = new Vector2(transform.position.x, transform.position.y);
        Vector2 destination = position + direction;

        // Always face the direction but only move when allowed
        FaceDirection(direction);

        if (CanMoveToDestination(destination)) {
            StartCoroutine(Move(destination));
        }
    }

    /// <summary>
    /// Returns the direction to move based on the previous and current input
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Vector3 GetDirectionFromInput(Vector2 input)
    {
        // Defaults to current direction
        Vector2 direction = m_currentDirection;

        // New Input received
        if (m_previousInput != input) {

            // Switch to opposite axis 
            // If moving horizonally then change to vertical and vice-versa
            if (m_previousInput.x != 0f && input.y != 0) {
                direction.Set(0f, input.y);

            } else if (m_previousInput.y != 0f && input.x != 0f) {
                direction.Set(input.x, 0f);

            } else {
                // Previously not moving
                // Or was holding two directions and now going back to one direction
                direction = input;

                // Always prioritize horizontal movement
                if (direction.x != 0f && direction.y != 0f) {
                    direction.y = 0f;
                }
            }

            // Direction changed
            m_currentDirection = direction;
        }

        // Save input
        // Should be exactly as input was recieved before modifications for direction
        m_previousInput = input;

        return direction;
    }

    /// <summary>
    /// Rotates the player to face the direction they are moving towards
    /// </summary>
    /// <param name="direction"></param>
    void FaceDirection(Vector2 direction)
    {
        if (direction != Vector2.zero) {
            float angle = Mathf.Atan2(direction.y, direction.x);
            Quaternion target = Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg);
            m_rigidbody.MoveRotation(target);
        }
    }

    /// <summary>
    /// Checks for collision with an impassible object at the given destination
    /// </summary>
    /// <param name="end"></param>
    /// <returns></returns>
    public bool CanMoveToDestination(Vector2 end)
    {
        var origin = m_rigidbody.position;

        // Avoid colliding with self
        LayerMask layerMask = gameObject.layer;
        gameObject.layer = LayerMask.NameToLayer("Default");

        var hit = Physics2D.Linecast(origin, end, m_collisionMask);

        // Reset layer
        gameObject.layer = layerMask;

        return hit.collider == null;
    }

    /// <summary>
    /// Triggers the player to spawn which has the avatar move from 
    /// the starting platform to the starting position
    /// </summary>
    public void TriggerSpawn()
    {
        if (!IsSpawning) {
            StartCoroutine(SpawnRoutine());
        }
    }

    /// <summary>
    /// Transitions the player from the starting platform to the
    /// starting point where they regain control
    /// </summary>
    /// <returns></returns>
    IEnumerator SpawnRoutine()
    {
        IsSpawning = true;

        m_rigidbody.position = m_spawnPoint;
        m_rigidbody.rotation = 0f;

        m_animator.SetBool("IsMoving", true);
        yield return StartCoroutine(Move(m_startingPoint, false));
        m_animator.SetBool("IsMoving", false);

        IsSpawning = false;
        MovementDisabled = false;
    }

    /// <summary>
    /// Moves the player to the given destination until it has reach the destination
    /// </summary>
    /// <param name="destination"></param>
    /// <returns></returns>
    IEnumerator Move(Vector2 destination, bool checkIsFalling = true)
    {
        IsMoving = true;

        // Though we have a rigidbody component we are not using the built in 
        // physics to detect collision hence us waiting for EndOfFrame and not FixedUpdate
        while (Vector2.Distance(m_rigidbody.position, destination) >= m_minDistance) {
            yield return null;

            Vector2 position = Vector2.MoveTowards(
                m_rigidbody.position, 
                destination, 
                m_speed * Time.fixedDeltaTime
            );

            m_rigidbody.MovePosition(position);
        }

        m_rigidbody.position = destination;

        // Check for falling once we are done moving
        if (checkIsFalling && IsPlayerFalling()) {
            TriggerFallRoutine();
        } else if (m_isTouchingLaser) {
            TriggerBurnedRoutine();
        }

        IsMoving = false;
    }

    /// <summary>
    /// Triggers the player falling and removes a reference to the game object 
    /// they might have been standing on
    /// </summary>
    void TriggerFallRoutine()
    {
        m_onGameObject = null;
        StartCoroutine(FallingRoutine());
    }

    /// <summary>
    /// Prevents movement while the player falls
    /// Resets position to the starting position
    /// </summary>
    /// <returns></returns>
    IEnumerator FallingRoutine()
    {
        IsFalling = true;
        MovementDisabled = true;

        m_animator.SetTrigger("Fall");
        AudioManager.Instance.Play2DSound(AudioClipName.PlayerFall);
        yield return new WaitForSeconds(m_fallAnimationLength);

        IsFalling = false;
        MovementDisabled = false;

        TriggerSpawn();
    }

    /// <summary>
    /// Triggers the player being burned by a laser
    /// </summary>
    void TriggerBurnedRoutine()
    {
        m_onGameObject = null;
        StartCoroutine(BurnedRoutine());
    }

    /// <summary>
    /// Prevents movement and shows the animation of the player burning
    /// </summary>
    /// <returns></returns>
    IEnumerator BurnedRoutine()
    {
        IsBurned = true;
        MovementDisabled = true;

        m_animator.SetTrigger("Burned");
        var source = AudioManager.Instance.Play2DSound(AudioClipName.PlayerBurned);
        yield return new WaitForSeconds(m_burningAnimationLength);

        IsBurned = false;
        MovementDisabled = false;

        TriggerSpawn();
    }

    /// <summary>
    /// Moves the player into the ship playing the victory sound
    /// </summary>
    /// <returns></returns>
    public IEnumerator ExitLevelRoutine()
    {
        IsExiting = true;

        AudioManager.Instance.Play2DSound(AudioClipName.LevelCompleted);

        // Open the stairs
        var stairs = FindObjectOfType<ShuttleStairs>();
        if(stairs != null) {
            stairs.Open();
            yield return new WaitForSeconds(0.5f);
        }

        // Make sure we are facing right
        m_rigidbody.rotation = 0f;

        m_animator.SetBool("IsMoving", true);
        yield return StartCoroutine(Move(m_exitPoint, false));
        m_animator.SetBool("IsMoving", false);
    }

    /// <summary>
    /// Store the object the player is currently on
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        m_onGameObject = collision.gameObject;
    }

    /// <summary>
    /// Player might have removed the tile underneath so remove this
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.gameObject == m_onGameObject) {
            m_onGameObject = null;
        }
    }

    /// <summary>
    /// Turns the flag on for laser is touching
    /// </summary>
    public void OnLaserEnter()
    {
        if(!IsFalling && !IsBurned) {
            m_isTouchingLaser = true;
        }
    }

    /// <summary>
    /// Turns the flag on for laser is touching
    /// </summary>
    public void OnLaserExit()
    {
        m_isTouchingLaser = false;
    }
}
