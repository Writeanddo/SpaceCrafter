using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;
using System;

/// <summary>
/// Not behaving well with the persistent flag in the Singelton class
/// Chaning to a standalone class that has singleton props
/// </summary>
public class GameManager : MonoBehaviour
{
    /// <summary>
    /// An instance of self
    /// </summary>
    public static GameManager Instance { get; set; }

    [SerializeField, Tooltip("How long to wait in between each timer cycle")]
    float m_timerDealy = 0.5f;

    /// <summary>
    /// The name of the scene for the main menu
    /// </summary>
    [SerializeField]
    string m_mainMenuSceneName = "MainMenu";

    /// <summary>
    /// The format for scene level names
    /// </summary>
    [SerializeField]
    string m_levelSceneNamePrefix = "Level_";

    /// <summary>
    /// The name of the scene for the end credits
    /// </summary>
    [SerializeField]
    string m_creditsSceneName = "Credits";

    /// <summary>
    /// Keeps track of the current level
    /// </summary>
    [SerializeField]
    int m_currentLevel = 0;
    public int CurrentLevel
    {
        get {
            if (m_currentLevel == 0) {
                string sceneNumber = Regex.Match(CurrentSceneName, @"\d+").Value;
                if (!string.IsNullOrEmpty(sceneNumber)) {
                    m_currentLevel = int.Parse(sceneNumber);
                }
            }
            return m_currentLevel;
        }
        private set { m_currentLevel = value; }
    }

    /// <summary>
    /// Returns the current loaded scene's name
    /// </summary>
    string CurrentSceneName
    {
        get {
            return SceneManager.GetActiveScene().name;
        }
    }

    /// <summary>
    /// Toggles between ON/OFF to indicate where in the current tick it is ON
    /// </summary>
    public bool CycleOn { get; private set; } = false;

    /// <summary>
    /// We will assume that any scene name with a number in it is a level
    /// </summary>
    bool IsCurrentSceneALevel
    {
        get {
            string sceneNumber = Regex.Match(CurrentSceneName, @"\d+").Value;
            return !string.IsNullOrEmpty(sceneNumber);
        }
    }

    /// <summary>
    /// Keeps track of anything the player has collected
    /// Ignores the ones consumed as this is to carry over
    /// anything collected previously
    /// </summary>
    public Dictionary<TileType, int> Inventory { get; set; }

    /// <summary>
    /// Keeps tack of the tile type for the inventory icon the player selected
    /// </summary>
    public TileType LastIconSelected { get; set; }

    /// <summary>
    /// Keeps track of the scrolling bg's offset so that the next level starts the same way
    /// </summary>
    public float BackgroundOffset { get; set; }

    /// <summary>
    /// True when there is no next level
    /// </summary>
    public bool IsLastLevel {
        get {
            int nextLevel = CurrentLevel + 1;
            string levelName = $"{m_levelSceneNamePrefix}{nextLevel}";
            return !Application.CanStreamedLevelBeLoaded(levelName);
        }
    }

    /// <summary>
    /// Creates the instance reff=
    /// </summary>
    public void Awake()
    {
        if (Instance == null) {
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        } else if (Instance != this) {
            DestroyImmediate(gameObject);
        }
    }

    /// <summary>
    /// Kicks off the global timer routine
    /// </summary>
    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneWasLoaded;
        StartCoroutine(GlobalTimer());

        Inventory = new Dictionary<TileType, int>();

        // If the level we start is a level one 
        // then the on scene loaded will not be picked up
        if (IsCurrentSceneALevel) {
            OnLevelLoaded();
        }
    }

    /// <summary>
    /// ESC to return to main menu when not already there
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (CurrentSceneName != m_mainMenuSceneName) {
                TransitionToMainMenu();
            }
        }
    }

    /// <summary>
    /// Toggles the current cycle
    /// </summary>
    /// <returns></returns>
    IEnumerator GlobalTimer()
    {
        while (true) {
            yield return new WaitForSeconds(m_timerDealy);
            CycleOn = !CycleOn;
        }
    }

    /// <summary>
    /// Registers when a scene was loaded
    /// This allows the GM to know when to invoke the LevelManager to transition the level
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="mode"></param>
    void OnSceneWasLoaded(Scene scene, LoadSceneMode mode)
    {
        if (IsCurrentSceneALevel) {
            OnLevelLoaded();
        }
    }

    /// <summary>
    /// Triggers the start of the level
    /// </summary>
    void OnLevelLoaded()
    {
        var manager = FindObjectOfType<LevelManager>();
        if(manager != null) {
            manager.LevelStarted();
        }
    }

    /// <summary>
    /// Triggers the level manager to end the level
    /// </summary>
    public void LevelCompleted()
    {
        var manager = FindObjectOfType<LevelManager>();
        if (manager != null) {
            manager.LevelCompleted(IsLastLevel);

        }
    }

    /// <summary>
    /// Triggers the game to start
    /// </summary>
    public void OnMainMenuPlayButtonPressed()
    {
        StartGame();
    }

    /// <summary>
    /// Takes you back to the main menu
    /// </summary>
    public void OnCreditsSceneMainMenuButtonPressed()
    {
        TransitionToMainMenu();
    }

    /// <summary>
    /// Resets the level counter to 1 and loads the level
    /// </summary>
    public void StartGame()
    {
        CurrentLevel = 1;
        Inventory.Clear();
        LoadCurrentLevel();
    }

    /// <summary>
    /// Increases the current count for the specified tile type
    /// </summary>
    /// <param name="type"></param>
    public void AddToInventory(TileType type)
    {
        if (Inventory.ContainsKey(type)) {
            Inventory[type]++;
        }
    }

    /// <summary>
    /// Triggers a scene change back to the main menu
    /// </summary>
    public void TransitionToMainMenu()
    {
        TransitionToScene(m_mainMenuSceneName);
    }

    /// <summary>
    /// Loads the credits scene
    /// </summary>
    public void TransitionToCredits()
    {
        TransitionToScene(m_creditsSceneName);
    }

    /// <summary>
    /// Increases the current level counter and triggers a transition to it
    /// When the level cannot be loaded it defaults to the end credits
    /// </summary>
    public void LoadNextLevel()
    {
        // Defaults action to credits screen
        Action transitionTo = TransitionToCredits;

        // Switches to loading the level if it can be loaded
        int nextLevel = CurrentLevel + 1;

        string levelName = $"{m_levelSceneNamePrefix}{nextLevel}";        
        if (Application.CanStreamedLevelBeLoaded(levelName)) {
            CurrentLevel = nextLevel;
            transitionTo = LoadCurrentLevel;
        }

        transitionTo?.Invoke();
    }

    /// <summary>
    /// Loads the current level
    /// </summary>
    public void LoadCurrentLevel()
    {
        string levelName = $"{m_levelSceneNamePrefix}{CurrentLevel}";
        TransitionToScene(levelName);
    }

    /// <summary>
    /// Loads the given scene if it can be loaded
    /// </summary>
    /// <param name="sceneName"></param>
    void TransitionToScene(string sceneName)
    {
        if (Application.CanStreamedLevelBeLoaded(sceneName)) {
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        } else {
            Debug.LogErrorFormat("Scene '{0}' cannot be loaded", sceneName);
            // Failsafe
            ReloadScene();
        }
    }

    /// <summary>
    /// Reloads the currently loaded scene
    /// </summary>
    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Terminates the application
    /// Todo: Remove when done debugging
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
    }
}
