using UnityEngine;

public class MySceneManager : MonoBehaviour
{
    // Attached sub-scene objects.
    public GameObject[] subScenes;
    public GameObject[] staticSubScenes;
    // Size of the sub-scenes array.
    public int size;


    private void Awake()
    {
        size = subScenes.Length;
    }

    // Enable a numbered sub-scene.
    public void EnableSubScene(int i)
    {
        subScenes[i].SetActive(true);
        staticSubScenes[i].SetActive(true);
    }

    // Disable a numbered sub-scene.
    public void DisableSubScene(int i)
    {
        subScenes[i].SetActive(false);
        staticSubScenes[i].SetActive(false);
    }

    // Delete a numbered sub-scene.
    public void DeleteSubScene(int i)
    {
        Destroy(subScenes[i]);
        Destroy(staticSubScenes[i]);
    }
}
