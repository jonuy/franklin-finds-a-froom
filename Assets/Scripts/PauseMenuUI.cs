﻿using UnityEngine;
using System.Collections;

public class PauseMenuUI : MonoBehaviour {

    private static string SOUND_PREFS_KEY = "SOUND_PREFS";

    #region Reference to other game objects
    // Home screen controller
    public HomeScreenUI homeScreenUI;

    // Game controller
    public GameController gameController;
    #endregion

    #region UI elements to hide
    public GUITexture pauseTouchButton;

    public GameObject leftTouchButton;
    public GameObject rightTouchButton;
    public GameObject jumpTouchButton;
    public GameObject uiLevelIntro;
    #endregion

    #region Pause UI elements to show
    public GUITexture pauseMenuBg;
    public GUITexture exitTouchButton;
    public GUITexture resumeTouchButton;
    public GUITexture soundOffTouchButton;
    public GUITexture soundOnTouchButton;
    #endregion

    // Flag tracking whether or not game is paused.
    private bool isGamePaused;

    private float savedTimeScale;

    void Awake() {
        isGamePaused = false;

        // Set the width and height of the pause menu bg to match the screen size
        if (pauseMenuBg != null) {
            pauseMenuBg.guiTexture.pixelInset = new Rect(0, 0, Screen.width, Screen.height);
        }
    }

    // Update is called once per frame
    void Update () {
        // Only allow for Puase menu controls to work if gameplay is in progress
        if (gameController.GameState != GameController.FFGameState.InProgress) {
            return;
        }

        if (Input.GetButtonUp("Back")) {
            // Pause the game
            if (!isGamePaused) {
                this.Pause();
            }
            // Unpause the game
            else {
                this.Unpause();
            }
        }

        // Check touches to the on-screen pause button
        if (!isGamePaused) {
            foreach (Touch touch in Input.touches) {
                if (touch.phase == TouchPhase.Ended && pauseTouchButton.HitTest(touch.position)) {
                    this.Pause();
                }
            }
        }
        // Check touches to the pause menu items
        else {
            bool endGame = false;
            bool unpause = false;
            bool unmute = false;
            bool mute = false;

            foreach (Touch touch in Input.touches) {
                if (touch.phase == TouchPhase.Ended) {
                    if (exitTouchButton.HitTest(touch.position)) {
                        endGame = true;
                        break;
                    }
                    else if (resumeTouchButton.HitTest(touch.position)) {
                        unpause = true;
                        break;
                    }
                    else if (soundOffTouchButton.guiTexture.enabled && soundOffTouchButton.HitTest(touch.position)) {
                        unmute = true;
                        break;
                    }
                    else if (soundOnTouchButton.guiTexture.enabled && soundOnTouchButton.HitTest(touch.position)) {
                        mute = true;
                        break;
                    }
                }
            }

            // If no touch events are detected, check for keyboard input.
            if (!endGame && !unpause && !unmute && !mute) {
                if (Input.GetButtonUp("End Game Exit")) {
                    endGame = true;
                }
                else if (Input.GetButtonUp("Pause Menu Sound")) {
                    if (soundOffTouchButton.guiTexture.enabled) {
                        unmute = true;
                    }
                    else if (soundOnTouchButton.guiTexture.enabled) {
                        mute = true;
                    }
                }
            }

            if (endGame) {
                // End game and return to home screen.
                this.EndGame();
            }
            else if (unpause) {
                // Resume gameplay.
                this.Unpause();
            }
            else if (unmute) {
                // Turn sound back on.
                this.UnmuteSound();
            }
            else if (mute) {
                // Turn sound off.
                this.MuteSound();
            }
        }
    }

    /**
     * Pause the game.
     */
    private void Pause() {
        isGamePaused = true;
        
        // Setting time scale to 0 for any components that rely on time to execute.
        savedTimeScale = Time.timeScale;
        Time.timeScale = 0;
        
        // Pause audio
        AudioListener.pause = true;
        
        // Show the Pause Menu elements
        this.ShowMenuUI();
        
        // Hide the UI controls
        this.HideGameUI();
    }

    /**
     * Unpause the game and resume gameplay.
     */
    private void Unpause() {
        isGamePaused = false;
        
        // Restore time scale
        Time.timeScale = savedTimeScale;
        
        // Unpause audio
        AudioListener.pause = false;
        
        // Hide the Pause Menu elements
        this.HideMenuUI();
        
        // Show the UI controls
        this.ShowGameUI();
    }

    /**
     * Hide the UI for the game controls and other in-game elements.
     */
    private void HideGameUI() {
        leftTouchButton.SetActive(false);
        rightTouchButton.SetActive(false);
        jumpTouchButton.SetActive(false);

        uiLevelIntro.SetActive(false);

        pauseTouchButton.enabled = false;
    }

    /**
     * Show the UI for the game controls and other in-game elements.
     */
    private void ShowGameUI() {
        leftTouchButton.SetActive(true);
        rightTouchButton.SetActive(true);
        jumpTouchButton.SetActive(true);

        pauseTouchButton.enabled = true;
    }

    /**
     * Hide the Pause Menu UI elements.
     */
    private void HideMenuUI() {
        pauseMenuBg.guiTexture.enabled = false;
        exitTouchButton.guiTexture.enabled = false;
        resumeTouchButton.guiTexture.enabled = false;
        soundOffTouchButton.guiTexture.enabled = false;
        soundOnTouchButton.guiTexture.enabled = false;
    }
    
    /**
     * Show the Pause Menu UI elements.
     */
    private void ShowMenuUI() {
        pauseMenuBg.guiTexture.enabled = true;
        exitTouchButton.guiTexture.enabled = true;
        resumeTouchButton.guiTexture.enabled = true;
        if (AudioListener.volume == 0) {
            soundOffTouchButton.guiTexture.enabled = true;
            soundOnTouchButton.guiTexture.enabled = false;
        }
        else {
            soundOnTouchButton.guiTexture.enabled = true;
            soundOffTouchButton.guiTexture.enabled = false;
        }
    }

    /**
     *  Mute the sound and change UI if pause menu is open.
     */
    private void MuteSound() {
        AudioListener.volume = 0;

        if (isGamePaused) {
            soundOffTouchButton.guiTexture.enabled = true;
            soundOnTouchButton.guiTexture.enabled = false;
        }

        // Save to PlayerPrefs
        PlayerPrefs.SetInt(SOUND_PREFS_KEY, 0);

        // GameAnalytics tracking
        GA.API.Design.NewEvent("Preferences:MuteSound");
    }

    /**
     * Unmute the sound and change UI if pause menu is open.
     */
    private void UnmuteSound() {
        AudioListener.volume = 1;

        if (isGamePaused) {
            soundOffTouchButton.guiTexture.enabled = false;
            soundOnTouchButton.guiTexture.enabled = true;
        }

        // Save to PlayerPrefs
        PlayerPrefs.SetInt(SOUND_PREFS_KEY, 1);

        // GameAnalytics tracking
        GA.API.Design.NewEvent("Preferences:UnmuteSound");
    }

    /**
     * End the game and return to the home screen.
     */
    private void EndGame() {
        // Unpause the game state
        this.Unpause();

        // Hide gameplay UI elements
        this.HideGameUI();
        this.HideMenuUI();

        // Show home screen
        homeScreenUI.ShowUI();

        // Allow GameController to clean up whatever it needs to clean up.
        gameController.EndGame();
    }

}
