﻿using UnityEngine;
using System.Collections;

public class LevelConfig : MonoBehaviour {

    // Array of level definitions
    public Level[] levels;

    public Level GetLevel(int levelIndex) {
        if (levelIndex < levels.Length) {
            return levels[levelIndex];
        }
        else {
            return null;
        }
    }
}

[System.Serializable]
public class Level {
    public SpawnConfig[] leftSpawnConfigs;
    public SpawnConfig[] rightSpawnConfigs;
    public float respawnDelay;
    public bool endlessMode;
    public FireShower[] fireShowers;
}

[System.Serializable]
public class SpawnConfig {
    public GameObject spawnObject;
    public float spawnDelay;
    public float speedModifier = 1;
}

[System.Serializable]
public class FireShower {
    public int numFireballs;
    public float startDelay;
    public float speed;
}