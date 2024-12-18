using UnityEngine;
using UnityEngine.SceneManagement;

public class GameControl : MonoBehaviour
{

    private bool isPaused = false;
    public GameObject pauseMenu; 

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            TogglePause();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartScene();
        }
    }
    void TogglePause()
    {
        isPaused = !isPaused; 

        if (isPaused)
        {
            Time.timeScale = 0;  
            AudioListener.pause = true;  
            if (pauseMenu != null)
            {
                pauseMenu.SetActive(true);  
            }
        }
        else
        {
            Time.timeScale = 1;  
            AudioListener.pause = false; 
            if (pauseMenu != null)
            {
                pauseMenu.SetActive(false); 
            }
        }

        // Remove this section as it's no longer needed - buttons handle their own animation mode
        // if (pauseMenu != null) {
        //     var menuButtons = pauseMenu.GetComponentsInChildren<MenuButton>(true);
        //     foreach (var button in menuButtons) {
        //         if (button.GetComponent<Animator>() != null) {
        //             button.GetComponent<Animator>().updateMode = AnimatorUpdateMode.UnscaledTime;
        //         }
        //     }
        // }
    }

    void RestartScene()
    {
        Time.timeScale = 1;  
        AudioListener.pause = false; 
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);  
    }
}
