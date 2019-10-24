using UnityEngine;

/// <summary>
/// Mediator for main menu ui buttons with game manager
/// which might be a different instance each time
/// </summary>
public class MainMenuController : MonoBehaviour
{
    public void StartGame()
    {
        GameManager.Instance.OnMainMenuPlayButtonPressed();
    }

    public void QuitGame()
    {
        GameManager.Instance.QuitGame();
    }
}
