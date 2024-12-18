using UnityEngine;

public class MenuButtonController : MonoBehaviour {

    public int index;
    [SerializeField] private int maxIndex;
    public AudioSource audioSource;

    void Start () {
        audioSource = GetComponent<AudioSource>();
    }
    
    void Update () {
        HandleInput();
    }

    private void HandleInput() {
        // Changed to GetKeyDown which works during pause
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) {
            if (index < maxIndex)
                index++;
            else
                index = 0;
            audioSource.Play();
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) {
            if (index > 0)
                index--;
            else
                index = maxIndex;
            audioSource.Play();
        }
    }
}
