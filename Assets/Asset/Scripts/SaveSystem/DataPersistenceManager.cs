using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using System;
using System.IO;



public class DataPersistenceManager : MonoBehaviour
{
    [Header("Debugging")]
    [SerializeField] private bool disableDataPersistence = false;
    [SerializeField] private bool initializeDataIfNull = false;
    [SerializeField] private bool overrideSelectedProfileId = false;
    [SerializeField] private string testSelectedProfileId = "test";

   

    [Header("File Storage Config")]
    [SerializeField] private string fileName = "MyGame.json";
    [SerializeField] private string checkpointFileName = "CheckpointData.json"; // File name for checkpoint data
    [SerializeField] private string sceneDataFileName = "sceneData.json"; // Different file for scene data
    public bool shouldReset = false; // Add this field to DataPersistenceManager


    private SceneData sceneData;
    private GameData gameData;
    private List<IDataPersistence> dataPersistenceObjects;
    private FileDataHandler dataHandler;
    public SceneDataHandler sceneDataHandler;
    private string selectedProfileId = "";
    // New field to indicate a reset is requested
    public bool resetGameOnLoad = false;
    public static DataPersistenceManager instance { get; private set; }

    // Define the IsNewGame property here
    public bool IsNewGame { get; private set; } = false;

    public GameData GameData
    {
        get { return gameData; }
    }


    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Found more than  one Data Peristence Manager in the scene.");
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this.gameObject);

        if (disableDataPersistence)
        {
            Debug.LogWarning("Data Persistence is currently disabled!");
        }

        this.dataHandler = new FileDataHandler(Application.persistentDataPath, fileName);
        this.selectedProfileId = dataHandler.GetMostRecentlyUpdatedProfile();
        this.gameData = new GameData(); // Make sure this line is executed

        sceneDataHandler = new SceneDataHandler(Application.persistentDataPath, sceneDataFileName);
        sceneData = sceneDataHandler.LoadSceneData(); // Load scene data at start

        if (overrideSelectedProfileId)
        {
            this.selectedProfileId = testSelectedProfileId;
            Debug.LogWarning("Overrode selected profile id with test id" + testSelectedProfileId);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        //SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        //SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }


    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene {scene.name} loaded, checking if game data reset is requested");
        


        string sceneName = scene.name;
        SceneData existingSceneData = sceneDataHandler.LoadSceneData();

        if (existingSceneData != null)
        {
            if (sceneName == "GameMainMenu" && !string.IsNullOrEmpty(existingSceneData.previousScene) && existingSceneData.previousScene != "GameMainMenu")
            {
                // Skip updating scene data for "GameMainMenu" if coming from another scene
            }
            else
            {
                sceneData.previousScene = existingSceneData.currentScene;
                sceneData.currentScene = sceneName;
                Debug.Log($"Scene transition: Previous - {sceneData.previousScene}, Current - {sceneData.currentScene}");
                sceneDataHandler.SaveSceneData(sceneData);
            }
        }
        else
        {
            sceneData.previousScene = "";
            sceneData.currentScene = sceneName;
            Debug.Log($"First scene load, setting scene data: Current - {sceneData.currentScene}");
            sceneDataHandler.SaveSceneData(sceneData);
        }

        // New logic to handle SaveSlot.json
        // Check if the scene is not "GameMainMenu" before saving the scene name to SaveSlot.json
        if (sceneName != "GameMainMenu" && !string.IsNullOrEmpty(selectedProfileId))
        {
            SaveSceneNameToSaveSlotJson(selectedProfileId, sceneName);
        }

        this.dataPersistenceObjects = FindAllDataPersistenceObjects();

        //if not work delete
        // Initialize or update checkpoint data
        if (SceneManager.GetActiveScene().name != "GameMainMenu")
        {
           

        }

        LoadGame(); // Continue to load game data
        
    }

    private void SaveSceneNameToSaveSlotJson(string profileId, string sceneName)
    {
        // Assuming fileDataHandler is an instance of FileDataHandler already defined in your class.
        SaveSlotData saveSlotData = new SaveSlotData { lastPlayedScene = sceneName };
        dataHandler.SaveSaveSlotData(profileId, saveSlotData); // Use the instance to call the method
        Debug.Log($"Saved '{sceneName}' to SaveSlot.json for profile {profileId}");
    }


    public void NewGame()
    {
        Debug.Log("New game started, resetting game data including player position.");
        gameData = new GameData();
        shouldReset = true; // Indicate that we've started a new game
      
    }


    public void LoadGame()
    {


        if (disableDataPersistence) return;

        // Check if we should reset the game data based on the flag
        if (shouldReset)
        {
            Debug.Log("Resetting game data as requested.");
            NewGame();
            shouldReset = false; // Reset the flag after handling
            SaveGame(); // Optionally save immediately to persist the reset state
        }
        else
        {
            // Proceed with loading saved data
            gameData = dataHandler.Load(selectedProfileId);

            if (gameData == null)
            {
                Debug.Log("No saved data was found. Starting a new game.");
                if (initializeDataIfNull)
                {
                    NewGame(); // Optionally start a new game if configured to do so
                }
                else
                {
                    // Optionally, handle disabling continue/load game in main menu here
                    // This might involve setting a flag or calling a method on your menu controller
                }
            }
            else
            {
                Debug.Log("Loading game data.");
                foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
                {
                    dataPersistenceObj.LoadData(gameData);
                }
            }
        }
    }



    public void SaveGame()
    {
        //return right away if data  persistence is disabled
        if (disableDataPersistence)
        {
            return;
        }

        //if we dont have any data to save, log a warning here
        if (this.gameData == null)
        {
            Debug.LogWarning("No data was found. A new game needs to be started before data can be saved.");
            return;
        }
        // pass the data to other scripts so they can update it
        foreach (IDataPersistence dataPersistenceObj in dataPersistenceObjects)
        {
            dataPersistenceObj.SaveData(ref gameData);
        }

        // Log the player's position at the time of saving (assuming player's data is part of gameData).
        //Debug.Log($"Player position saved: {gameData.playerPosition}");

        // save that data to a file using file data handler
        dataHandler.Save(gameData, selectedProfileId);

        //timestamp the data so we know when it was last saved
        gameData.lastUpdated = System.DateTime.Now.ToBinary();

        //IMPORTANT TO NOTE!!!!!!
        // Known issue: MissingReferenceException occurs when transitioning back to the main menu because
        // objects implementing IDataPersistence (e.g., PlayerController) are destroyed before the DataPersistenceManager
        // completes the save operation. Currently, this does not affect game functionality and is considered a low-priority issue.

    }





    private List<IDataPersistence> FindAllDataPersistenceObjects()
    {
        IEnumerable<IDataPersistence> dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>().OfType<IDataPersistence>();

        return new List<IDataPersistence>(dataPersistenceObjects);
    }

    public void OnApplicationQuit()
    {
        
        
    }

    public bool HasGameData()
    {
        return gameData != null;
    }



    public string DetermineSceneToLoad()
    {
        Debug.Log($"[DetermineSceneToLoad] Current Scene: {sceneData.currentScene}, Previous Scene: {sceneData.previousScene}");

        // If the current scene is "MainMenu" and there's a valid previous scene that is not "MainMenu", prefer the previous scene.
        if (sceneData.currentScene == "GameMainMenu" && !string.IsNullOrEmpty(sceneData.previousScene) && sceneData.previousScene != "GameMainMenu")
        {
            Debug.Log($"[DetermineSceneToLoad] Decided to load (Previous Scene): {sceneData.previousScene}");
            return sceneData.previousScene;
        }
        // If neither the current scene nor the previous scene is "MainMenu", prefer the current scene.
        else if (!string.IsNullOrEmpty(sceneData.currentScene) && sceneData.currentScene != "GameMainMenu" && !string.IsNullOrEmpty(sceneData.previousScene) && sceneData.previousScene != "GameMainMenu")
        {
            Debug.Log($"[DetermineSceneToLoad] Decided to load (Current Scene): {sceneData.currentScene}");
            return sceneData.currentScene;
        }
        // Otherwise, if the current scene is not "MainMenu" and there's no valid previous scene, load the current scene.
        else if (!string.IsNullOrEmpty(sceneData.currentScene) && sceneData.currentScene != "GameMainMenu")
        {
            Debug.Log($"[DetermineSceneToLoad] Decided to load (Current Scene): {sceneData.currentScene}");
            return sceneData.currentScene;
        }
        // Default to a specific scene if none match your criteria or both are "MainMenu"
        Debug.Log("[DetermineSceneToLoad] Defaulting to 'Chapter1'");
        return "Chapter1"; // Adjust this to your game's default or initial scene
    }

    //get data for profileid all at once from other scripts method to get all profile game data

    public Dictionary<string, GameData> GetAllProfilesGameData()
    {
        return dataHandler.LoadAllProfiles();
    }

    public void ChangeSelectedProfileId(string newProfileId)
    {
        //update the profile to  use for saving and loading
        this.selectedProfileId = newProfileId;
        //load the game which will use that profile, updating our game data accordingly
        LoadGame();
    }


    // Method to load SaveSlotData
    public SaveSlotData LoadSaveSlotData(string profileId)
    {
        return dataHandler.LoadSaveSlotData(profileId); // Utilize existing dataHandler
    }

    public void DeleteGameDataFile()
    {
        string profilePath = Path.Combine(dataHandler.DataDirPath, selectedProfileId);
        string filePath = Path.Combine(profilePath, fileName);

        if (File.Exists(filePath))
        {
            Debug.Log($"Deleting game data file: {filePath}");
            File.Delete(filePath);
        }
    }

    //original
    /*
    private void InitializeCheckpoints()
    {
        // Find all GameObjects with the "Checkpoint" tag
        GameObject[] checkpointObjects = GameObject.FindGameObjectsWithTag("CheckPoint");

        // Convert the GameObject array to a list of Checkpoint components
        List<CheckPoint> checkpoints = checkpointObjects.Select(obj => obj.GetComponent<CheckPoint>()).ToList();

        // Sort the checkpoints by their ID
        checkpoints.Sort((a, b) => a.checkpointID.CompareTo(b.checkpointID));

        // Optionally: Store the sorted checkpoint IDs in the GameData (assuming GameData has a list for this)
        // Clear existing IDs to avoid duplicates
        gameData.checkpointIDs.Clear();

        foreach (var checkpoint in checkpoints)
        {
            gameData.checkpointIDs.Add(checkpoint.checkpointID);
        }

        // Debug: Print the ordered list of checkpoint IDs
        Debug.Log("Ordered Checkpoint IDs: " + string.Join(", ", gameData.checkpointIDs));

        SaveCheckpointsOnly();
    }

    
    //current working codes
    private void SaveCheckpointsOnly()
    {
        // Avoid saving checkpoint data when in the main menu scene
        if (SceneManager.GetActiveScene().name == "GameMainMenu") // Adjust the scene name as per your main menu's scene name
        {
            Debug.Log("Currently in the Main Menu. Skipping checkpoint save.");
            return;
        }

        if (gameData == null)
        {
            Debug.LogError("GameData is null. Cannot save checkpoints.");
            return;
        }

        // Ensure we have a valid profile ID selected
        if (string.IsNullOrEmpty(selectedProfileId))
        {
            Debug.LogError("No selected profile ID. Cannot save checkpoints.");
            return;
        }

        // Extract only checkpoint data from gameData
        var checkpointData = new CheckpointData
        {
            checkpointIDs = gameData.checkpointIDs
            // Add any other checkpoint-specific data you might have
        };

        // Serialize the checkpoint data object to JSON
        string jsonData = JsonUtility.ToJson(checkpointData, true);

        // Define the path for the save file within the selected profile's directory
        string profilePath = Path.Combine(Application.persistentDataPath, selectedProfileId);
        if (!Directory.Exists(profilePath))
        {
            Directory.CreateDirectory(profilePath);
        }
        // Use a specific file name for checkpoint data
        string filePath = Path.Combine(profilePath, "CheckpointData.json"); // Consider using a constant or a setting for file names

        // Write the JSON string to the file
        File.WriteAllText(filePath, jsonData);

        Debug.Log($"Checkpoint data saved successfully for profile {selectedProfileId}.");
    }


    [System.Serializable]
    public class CheckpointData
    {
        public List<int> checkpointIDs;
        // Include other checkpoint-specific fields as needed

        
    }
    */
}
