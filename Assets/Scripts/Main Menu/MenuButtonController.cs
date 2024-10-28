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
        if (Input.GetKeyDown(KeyCode.DownArrow)) {
            index = (index < maxIndex) ? index + 1 : 0;
        	audioSource.Play();
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow)) {
            index = (index > 0) ? index - 1 : maxIndex;
        	audioSource.Play();
        }
    }
}
