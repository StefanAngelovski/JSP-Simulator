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
        menu = GetComponent<Menu>(); // Get the Menu component attached to the button

        if (audioSource != null && audioClip != null)
        {
            audioSource.clip = audioClip;
            audioSource.Play();
        }
        else if (audioClip == null)
        {
            Debug.LogWarning("No AudioClip assigned to play.");
        }
        else
        {
            Debug.LogWarning("No AudioSource component found on this GameObject.");
        }

        if (animator != null)
        {
            animator.SetBool("selected", true);
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
            animator.SetBool("selected", true);

            if (Input.GetAxis("Submit") == 1)
            {
                animator.SetBool("pressed", true);

                if (isQuitButton)
                {
                    QuitGame();
                }
                else if (isExtras)
                {
                    if (menu != null && menu.isLastExtrasPage)
                    {
                        // Do not reset the menu if it's the last page
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
            else if (animator.GetBool("pressed"))
            {
                animator.SetBool("pressed", false);
                animatorFunctions.disableOnce = true;
            }
        }
        else
        {
            animator.SetBool("selected", false);
        }
    }

    private IEnumerator LoadSceneWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
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
        yield return new WaitForSeconds(seconds);
        canPressButton = true;
    }
}