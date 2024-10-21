using UnityEngine;

public class SpawnHuman : MonoBehaviour
{
    public GameObject[] array;
    public Camera mainCamera; 
    private GameObject currentObject; 

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpawnRandomObject();
        }
    }

    void Start()
    {
        SpawnRandomObject();
    }

    void SpawnRandomObject()
    {
        if (currentObject != null)
        {
            Destroy(currentObject);
        }

        int randomIndex = Random.Range(0, array.Length);
        GameObject randomObject = array[randomIndex];

        Vector3 centerPosition = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, mainCamera.nearClipPlane));
        centerPosition.z = -1;

        currentObject = Instantiate(randomObject, centerPosition, Quaternion.identity);
    }
}
