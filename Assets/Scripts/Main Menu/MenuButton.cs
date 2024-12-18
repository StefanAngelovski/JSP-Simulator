using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuButton : MonoBehaviour
{
    [SerializeField] private MenuButtonController menuButtonController;
    [SerializeField] private Animator animator;
    [SerializeField] private AnimatorFunctions animatorFunctions;
    [SerializeField] private int thisIndex;
    [SerializeField] private GameObject currentGameObject;
    [SerializeField] private GameObject nextGameObject;
    [SerializeField] private bool isQuitButton;
    [SerializeField] private bool isExtras;
    [SerializeField] private bool isPauseMenuButton = false;

    // Added from PlayGame
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip audioClip;
    [SerializeField] private float sceneLoadDelay = 0.5f;
    [SerializeField] private string sceneToLoad;

    private bool canPressButton = true;
    private Menu menu;

    private void Start()
    {
        menuButtonController = GetComponentInParent<MenuButtonController>();
        audioSource = GetComponent<AudioSource>();
        menu = GetComponent<Menu>(); 

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (audioClip != null)
        {
            audioSource.clip = audioClip;
        }

        if (animator != null)
        {
            if (isPauseMenuButton)
            {
                animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            }
            else
            {
                animator.SetBool("selected", false);
                animator.SetBool("pressed", false);
            }
        }
        else
        {
            Debug.LogWarning("No Animator component assigned for the button.");
        }
    }

    void Update()
    {
        if (menuButtonController.index == thisIndex && canPressButton)
        {
            if (!isPauseMenuButton)
            {
                animator.SetBool("selected", true);
            }

            if (Input.GetButtonDown("Submit"))  
            {
                if (!isPauseMenuButton)
                {
                    animator.SetBool("pressed", true);
                }

                if (isQuitButton)
                {
                    QuitGame();
                }
                else if (isExtras)
                {
                    if (menu != null && menu.isLastExtrasPage)
                    {
                        Debug.Log("Last extras page reached, not resetting the menu.");
                        StartCoroutine(DisableButtonPressForSeconds(2f));
                    }
                    else
                    {
                        currentGameObject.SetActive(false);
                        nextGameObject.SetActive(true);
                    }
                }
                else
                {
                    StartCoroutine(LoadSceneWithDelay(sceneLoadDelay));
                }
            }
            else if (!isPauseMenuButton && animator.GetBool("pressed"))
            {
                animator.SetBool("pressed", false);
                animatorFunctions.disableOnce = true;
            }
        }
        else if (!isPauseMenuButton)
        {
            animator.SetBool("selected", false);
        }
    }

    private IEnumerator LoadSceneWithDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        Time.timeScale = 1f;  // Unpause the game
        AudioListener.pause = false;  // Unpause audio
        
        SceneManager.LoadScene(sceneToLoad);
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    private IEnumerator DisableButtonPressForSeconds(float seconds)
    {
        canPressButton = false;
        yield return new WaitForSecondsRealtime(seconds);
        canPressButton = true;
    }

    public void PlaySound()
    {
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.PlayOneShot(audioSource.clip);
        }
    }
}