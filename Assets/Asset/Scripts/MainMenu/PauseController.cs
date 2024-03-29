using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;


public class PauseController : MonoBehaviour
{
    public GameObject pauseMenuUI; // Assign this in the Inspector
    private AudioSource[] allAudioSources; // Array to hold all audio sources in the scene
    public AudioSource themeMusic; // Assign your theme music audio source in the Inspector
    public AudioSource bgmusic;
    private bool isPaused = false; // Track if the game is paused

    //private PlayerControls playerControls; // Reference to the generated PlayerControls class
    private PlayerInput playerInput;
    public GameObject tutorialPanel; // Assign this in the Inspector

    [SerializeField] private GameObject loadingScreen; // Reference to your loading screen object





    void Awake()
    {
        playerInput = FindObjectOfType<PlayerInput>(); // Find the PlayerInput component in the scene
        // Subscribe to the pause action
        playerInput.actions["PauseOpen"].performed += OnPausePerformed;
    }



    private void OnDestroy()
    {
        // Unsubscribe from the pause action to clean up
        if (playerInput != null)
        {
            playerInput.actions["PauseOpen"].performed -= OnPausePerformed;
        }
    }


    void Update()
    {
        
        // Check if the pause button is pressed
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            if (!isPaused)
            {
                Pause();
            }
            else
            {
                Resume();
            }
        }
        

    }
    void Start()
    {
        // Get all audio sources in the scene
        allAudioSources = FindObjectsOfType<AudioSource>();
    }



    void OnPausePerformed(InputAction.CallbackContext context)
    {
        Debug.Log("Pause button clicked");
        TogglePause();
    }

    public void TogglePause()
    {
        if (!isPaused)
        {
            Pause();
        }
        else
        {
            Resume();
        }
    }




    public void Pause()
    {
        isPaused = true;
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f; // Pause the game
        ControlAudioSources(true); // Mute or pause all audio sources
                                   // Use InputManager to switch to UI action map
        


    }


    public void Resume()
    {
        isPaused = false;
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f; // Resume game time
        ControlAudioSources(false); // Unpause all audio sources
                                    // Use InputManager to switch back to Player action map
        
    }

    public void RestartChapter()
    {
        // Ensure DataPersistenceManager instance is available
        if (DataPersistenceManager.instance != null)
        {
            // Indicate that a reset is requested
            DataPersistenceManager.instance.shouldReset = true;

            // Save the game to apply the reset immediately
            DataPersistenceManager.instance.SaveGame();

            // Ensure game time is resumed before reloading
            Time.timeScale = 1f;

            if (loadingScreen != null)
            {
                loadingScreen.SetActive(true); // Activate the loading screen
            }


            // Reload the current scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            Debug.LogError("DataPersistenceManager instance not found. Cannot restart chapter.");
        }
    }


    public void RestartFromLastCheckpoint()
    {
        // Check if DataPersistenceManager instance is available
        if (DataPersistenceManager.instance != null)
        {
            if (loadingScreen != null)
            {
                loadingScreen.SetActive(true); // Activate the loading screen
            }

            // Reload the current scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);

            // Load the most recent game state, which should be the last checkpoint
            DataPersistenceManager.instance.LoadGame();

            Debug.Log("Restarted from last checkpoint");
            pauseMenuUI.SetActive(false);
            Time.timeScale = 1f;


        }
        else
        {
            Debug.LogError("DataPersistenceManager instance not found. Cannot restart from last checkpoint.");
        }
    }





    //original

    public void LoadMainMenu()
    {
        // Check if DataPersistenceManager and SceneDataHandler instances are available
        if (DataPersistenceManager.instance != null && DataPersistenceManager.instance.sceneDataHandler != null)
        {
            // Load current scene data to update it for transitioning back to the main menu
            SceneData currentSceneData = DataPersistenceManager.instance.sceneDataHandler.LoadSceneData();

            // Set the previous scene to the current game scene before transitioning to the main menu
            if (currentSceneData != null)
            {
                currentSceneData.previousScene = currentSceneData.currentScene;
                currentSceneData.currentScene = "GameMainMenu";
                DataPersistenceManager.instance.sceneDataHandler.SaveSceneData(currentSceneData);
                Debug.Log($"Updated scene data for transition to main menu: Previous - {currentSceneData.previousScene}, Current - {currentSceneData.currentScene}");
            }



            // Ensure game time is resumed
            Time.timeScale = 1f;

            // Load the main menu scene
            SceneManager.LoadScene("GameMainMenu");
        }
        else
        {
            Debug.LogError("DataPersistenceManager or SceneDataHandler instance not found. Game not saved.");
        }
    }



    // Method to control audio sources (mute or pause)
    private void ControlAudioSources(bool mute)
    {
        foreach (AudioSource audioSource in allAudioSources)
        {
            // Skip the theme music audio source
            if (audioSource == themeMusic || audioSource == bgmusic)
            {
                continue;
            }

            if (mute)
            {
                audioSource.Pause(); // Pause the audio source
            }
            else
            {
                audioSource.UnPause(); // Resume the audio source
            }
        }
    }


    public void ShowTutorial()
    {
        tutorialPanel.SetActive(true); // Show the tutorial panel
        pauseMenuUI.SetActive(false); // Optionally, hide the pause menu to only show the tutorial instructions
    }

    public void HideTutorial()
    {
        tutorialPanel.SetActive(false); // Hide the tutorial panel
                                        // If you hide the pause menu when showing the tutorial, you might want to re-show the pause menu when the tutorial is closed
        pauseMenuUI.SetActive(true);
    }



}

