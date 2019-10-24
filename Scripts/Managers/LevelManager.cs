using System.Collections;
using UnityEngine;

/// <summary>
/// Handles the transitions for when the level starts and ends
/// </summary>
public class LevelManager : MonoBehaviour
{
    [SerializeField, Range(0.1f, 1f), Tooltip("Minimum perecent the tile shrinks when removed")]
    float m_shrinkSize = 0.25f;

    [SerializeField, Tooltip("How long it takes the tile to shrink")]
    float m_shrinkTime = .5f;

    [SerializeField, Tooltip("How long it takes the tile to grow")]
    float m_growTime = .25f;

    /// <summary>
    /// Triggers level started routine
    /// </summary>
    public void LevelStarted()
    {
        StartCoroutine(LevelStartedRoutine());
    }

    /// <summary>
    /// Forces all tiles with animations to grow
    /// Once they had some time to grow then spawns the player
    /// </summary>
    /// <returns></returns>
    IEnumerator LevelStartedRoutine()
    {
        // Skip on the first level
        if (GameManager.Instance.CurrentLevel != 1) {
            foreach (var tile in FindObjectsOfType<TileTransitionAnimations>()) {
                tile.Grow(m_shrinkSize, m_growTime);
            }
        }

        // Wait a bit for things to start growing then move the player
        yield return new WaitForSeconds(m_growTime);

        var player = FindObjectOfType<Player>();
        player.TriggerSpawn();
    }

    /// <summary>
    /// Triggers the level complete transition
    /// </summary>
    public void LevelCompleted(bool isFinalLevel = false)
    {
        StartCoroutine(LevelCompletedRoutine(isFinalLevel));
    }

    /// <summary>
    /// Forces all tiles with animations to shrink/be destroyed
    /// Moves the player towards the space ship
    /// </summary>
    /// <returns></returns>
    IEnumerator LevelCompletedRoutine(bool isFinalLevel)
    {
        var player = FindObjectOfType<Player>();

        // Wait until the player is done moving
        while (player.IsMoving) {
            yield return null;
        }

        // Making sure they cannot move
        player.IsMoving = false;
        player.MovementDisabled = true;

        // Makes everything dissappear
        foreach (var tile in FindObjectsOfType<TileTransitionAnimations>()) {
            tile.Shrink(m_shrinkSize, m_shrinkTime);
        }

        // Wait a bit for things to start growing then move the player
        yield return new WaitForSeconds(m_shrinkTime);
        yield return StartCoroutine(player.ExitLevelRoutine());

        // Wait a bit in case something else is triggering a remove
        yield return new WaitForSeconds(1f);

        // Fire off the Spaceship
        if (isFinalLevel) {
            player.gameObject.SetActive(false);
            var spaceship = FindObjectOfType<Spaceship>();
            if(spaceship != null) {
                spaceship.PlayLaunchAnimation();
                var source = AudioManager.Instance.Play2DSound(AudioClipName.Shuttle);

                var fader = FindObjectOfType<ScreenFader>();
                if(fader != null) {
                    fader.FadeIn();
                }

                yield return new WaitForSeconds(source.clip.length); // let it play
            }
        }

        GameManager.Instance.LoadNextLevel();
    }
}
