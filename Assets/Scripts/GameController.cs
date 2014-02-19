﻿using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {

	#region Public
	// Number of lives the player can start out with
	public int startingLives = 3;

	// Intended for development. Sets the starting level.
	public int startingLevel = 0;
	#endregion

	#region Handlers to other GameObjects
	// Container where the life icons will go
	public GameObject uiLivesContainer;

	// UI Text displaying score
	public GameObject uiScore;

	// UI Text to to display level on intro
	public GameObject uiIntroLevel;

	// Level config container
	public GameObject levelConfigContainer;

	// Reference to the player
	public GameObject player;

	// The player's starting position
	public Vector3 playerStartingPosition;

	// Fire shower spawn object
	public GameObject fireSpawner;
	#endregion

	#region Private
	// Current number of lives the player has remaining
	private int currentLives;

	private int currentLevel = 0;

	// Total points accumulated
	private int score;

	// Total # of coins collected
	private int coinsCollected;

	// Number of coins needed to add life
	private int coinsFor1Up = 10;

	private LevelConfig levelConfig;
	private SpawnController leftSpawn;
	private SpawnController rightSpawn;

	// Controller to the fire shower spawner
	private FireShowerController fireController;

	// Handle to LivesUI script
	private LivesUI livesUI;
	#endregion

	void Awake() {
		// Level config
		levelConfig = levelConfigContainer.GetComponent<LevelConfig>();

		// Spawn points
		GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
		foreach(GameObject spawnPoint in spawnPoints) {
			if (spawnPoint.name == "leftSpawn") {
				leftSpawn = spawnPoint.GetComponent<SpawnController>();
				leftSpawn.enabled = false;
			}
			else if (spawnPoint.name == "rightSpawn") {
				rightSpawn = spawnPoint.GetComponent<SpawnController>();
				rightSpawn.enabled = false;
			}
		}

		// Fire shower controller
		fireController = fireSpawner.GetComponent<FireShowerController>();

		// Controls lives icon
		livesUI = uiLivesContainer.GetComponent<LivesUI>();

		// Starting # of lives
		currentLives = startingLives;

		// Starting level
		currentLevel = startingLevel;

		// Starting score
		score = 0;

		// Starting coins
		coinsCollected = 0;

		ResetGameState();
	}

	/**
	 * Reset the state of the level. Includes clearing the enemies off the screen and resetting the UI.
	 */
	public void ResetGameState() {
		// Clear objects off the scene
		GameObject[] objects = FindObjectsOfType(typeof(GameObject)) as GameObject[];
		foreach(GameObject obj in objects) {
			// 9 = "Enemy" layer. 11 = "Pickup" layer. 12 = "Fire"
			if (obj.layer == 9 || obj.layer == 11 || obj.layer == 12) {
				Destroy(obj);
			}
		}
		
		// Setup UI
		UpdateGUI();

		// Show intro level UI
		uiIntroLevel.SetActive(true);

		// +1 since it starts at 0
		uiIntroLevel.guiText.text = "Level " + (currentLevel + 1);

		// Move player to beginning state
		player.transform.position = playerStartingPosition;

		// Suspend any fire showers until level starts
		fireController.Suspend();

		// Start level
		Invoke("StartLevel", 2);
	}

	private void StartLevel() {
		SetupSpawnPoints();
		SetupFireShowers();

		leftSpawn.enabled = true;
		rightSpawn.enabled = true;

		// Hide intro level UI
		uiIntroLevel.SetActive(false);
	}

	/**
	 * Setup for the fire shower spawner.
	 */
	private void SetupFireShowers() {
		Level level = levelConfig.GetLevel(currentLevel);
		if (level.fireShowers.Length > 0) {
			fireController.Setup(level.fireShowers);
		}
	}

	/**
	 * Sets up the enemy spawners.
	 */
	private	void SetupSpawnPoints() {
		Level level = levelConfig.GetLevel(currentLevel);

		// Setup configs on the left spawn
		leftSpawn.Setup(level.leftSpawnConfigs, level.respawnDelay, level.endlessMode);

		// Setup configs on the right spawn
		rightSpawn.Setup(level.rightSpawnConfigs, level.respawnDelay, level.endlessMode);
	}

	private void RestartGame() {
		currentLevel = startingLevel;
		currentLives = startingLives;
		score = 0;
		ResetGameState();
	}

	private void UpdateGUI() {
		if (livesUI) {
			livesUI.UpdateRemainingLives(currentLives);
		}

		if (uiScore && uiScore.guiText) {
			uiScore.guiText.text = "Score: " + score;
		}
	}

	private void TriggerLevelComplete() {
		Debug.Log("level complete. start next level.");

		currentLevel++;
		ResetGameState();
	}

	public void CheckIfLevelCompleted() {
		// Checking if any pending enemies to be spawned or if this is an endless mode level
		Level level = levelConfig.GetLevel(currentLevel);
		if (level.endlessMode || leftSpawn.GetNumPendingEnemies() > 0 || rightSpawn.GetNumPendingEnemies() > 0) {
			return;
		}
		
		// Check if any living enemies still on the scene
		GameObject[] objects = FindObjectsOfType(typeof(GameObject)) as GameObject[];
		foreach(GameObject obj in objects) {
			if (obj.layer == 9) { // "Enemy" layer
				EnemyController enemyController = obj.GetComponent<EnemyController>();
				if (enemyController.CurrState != EnemyController.EnemyState.DEAD) {
					return;
				}
			}
		}
		
		TriggerLevelComplete();
	}

	public int GetCurrentLives() {
		return currentLives;
	}

	public void DecrementCurrentLives() {
		currentLives--;
		UpdateGUI();

		if (currentLives < 0) {
			RestartGame();
		}
	}

	public void AddToScore(int addPoints) {
		score += addPoints;
		UpdateGUI();
	}

	public void AddCoinCollected() {
		coinsCollected++;
	}
}
