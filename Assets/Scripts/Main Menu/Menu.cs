using UnityEngine;
using System.Collections;

public class Menu : MonoBehaviour
{
    [SerializeField] private Animator buttonAnimator;
    [SerializeField] private GameObject current;
    [SerializeField] private GameObject next;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] public bool isLastExtrasPage;
    [SerializeField] private float transitionDelay = 1f;
    [SerializeField] private float buttonCooldown = 0.5f; // Cooldown duration in seconds

    private bool lastPageClicked = false;
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
        if (mainMenu == null && isLastExtrasPage) Debug.LogError("Main Menu GameObject is not assigned");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && buttonAnimator != null)
        {
            if (Time.time - lastButtonPressTime < buttonCooldown){
                return;
            }

            lastButtonPressTime = Time.time;


            if (isLastExtrasPage)
            {
                if (!lastPageClicked)
                {
                    lastPageClicked = true;
                }
                else
                {
                    StartCoroutine(TransitionToMainMenu());
                }
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

    private IEnumerator TransitionToMainMenu()
    {
        current.SetActive(false);
        mainMenu.SetActive(true);
        yield return new WaitForSeconds(transitionDelay);
    }

    private void RemoveCurrentPage()
    {
        current.SetActive(false);
        mainMenu.SetActive(true);
    }
}