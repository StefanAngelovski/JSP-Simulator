using UnityEngine;
using UnityEngine.SceneManagement;

public class GameControl : MonoBehaviour
{
    // Boolean to track whether the game is paused
    private bool isPaused = false;

    // UI element to show the pause menu (if needed)
    public GameObject pauseMenu; 

    void Update()
    {
        // Check if the player presses 'P' to toggle pause
        if (Input.GetKeyDown(KeyCode.P))
        {
            TogglePause();
        }

        // Check if the player presses 'R' to restart the scene
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartScene();
        }
    }

    // Function to toggle the pause state
    void TogglePause()
    {
        isPaused = !isPaused;  // Toggle the paused state

        if (isPaused)
        {
            Time.timeScale = 0;  // Pause the game
            AudioListener.pause = true;  // Optionally pause the audio
            if (pauseMenu != null)
            {
                pauseMenu.SetActive(true);  // Show the pause menu (if available)
            }
        }
        else
        {
            Time.timeScale = 1;  // Resume the game
            AudioListener.pause = false;  // Resume the audio
            if (pauseMenu != null)
            {
                pauseMenu.SetActive(false); // Hide the pause menu
            }
        }
    }

    // Function to restart the current scene
    void RestartScene()
    {
        Time.timeScale = 1;  // Ensure the game is not paused before restarting
        AudioListener.pause = false;  // Ensure audio is playing
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);  // Restart the scene
    }
}
