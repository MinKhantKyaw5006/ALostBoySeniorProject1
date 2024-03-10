/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public class GameData
{
    public Vector2 playerPosition;
    public GameData()
    {
        playerPosition = Vector2.zero; // Default player position
    }
}

*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public Vector2 playerPosition;
    public long lastUpdated; //timestamp storage
    public bool shouldReset = false;

    public List<int> checkpointIDs = new List<int>();

    public List<int> allCheckpointIDsInScene = new List<int>();  // All checkpoint IDs available in the current scene

    public GameData()
    {
        playerPosition = Vector2.zero; // Default player position
        
    }
}



