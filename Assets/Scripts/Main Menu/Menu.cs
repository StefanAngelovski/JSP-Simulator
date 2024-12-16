using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement; // Import the SceneManager namespace

public class Menu : MonoBehaviour
{
    [SerializeField] private Animator buttonAnimator;
    [SerializeField] private GameObject current;
    [SerializeField] private GameObject next;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] public bool isLastExtrasPage;
    [SerializeField] private float transitionDelay = 1f;
    [SerializeField] private float buttonCooldown = 0.5f;
    [SerializeField] private string targetSceneName; // Name of the scene to load

    private float lastButtonPressTime = 0f;

    private void Start()
    {
        if (buttonAnimator != null)
        {
            buttonAnimator.SetBool("selected", true);
        }
        else
        {
            Debug.LogWarning("No Animator component assigned for the button.");
        }

        ValidateReferences();
    }

    private void ValidateReferences()
    {
        if (current == null) Debug.LogError("Current GameObject is not assigned");
        if (next == null && !isLastExtrasPage) Debug.LogError("Next GameObject is not assigned");
        if (mainMenu == null && isLastExtrasPage && string.IsNullOrEmpty(targetSceneName)) 
            Debug.LogError("Main Menu or Target Scene Name is not assigned");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && buttonAnimator != null)
        {
            if (Time.time - lastButtonPressTime < buttonCooldown)
            {
                return;
            }

            lastButtonPressTime = Time.time;

            if (isLastExtrasPage)
                {
                    StartCoroutine(TransitionToAnotherScene());
                }
            else
            {
                current.SetActive(false);
                next.SetActive(true);
            }
        }

        if (Input.GetKeyUp(KeyCode.Return) && buttonAnimator != null)
        {
            buttonAnimator.SetBool("pressed", false);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            RemoveCurrentPage();
        }
    }

    private IEnumerator TransitionToAnotherScene()
    {
        // Play transition delay (if needed)
        yield return new WaitForSeconds(transitionDelay);

        // Load the target scene
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            Debug.LogError("Target scene name is not set!");
        }
    }

    private void RemoveCurrentPage()
    {
        current.SetActive(false);
        mainMenu.SetActive(true);
    }
}
