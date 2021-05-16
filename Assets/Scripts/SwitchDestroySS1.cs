using UnityEngine;

public class SwitchDestroySS1 : MonoBehaviour
{
    // Attached components.
    public MySceneManager mySceneManagerRef;
    public GameObject door;
    public AudioClip audioDrag;
    public AudioClip audioShut;

    // Switch control.
    public bool isTriggered { get; set; } = false;


    // Attached components.
    private AudioSource audioSource;


    private void Awake()
    {
        // Init the AudioSource.
        audioSource = this.GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "player" && isTriggered == false)
        {
            // Disable second-time triggering.
            isTriggered = true;

            // Display the locked door.
            door.SetActive(true);

            // Play the door shutting audio.
            audioSource.PlayOneShot(audioDrag);
            Invoke("AudioPlayDoorShut", 1.0f);

            // Destroy sub-scene.
            mySceneManagerRef.DeleteSubScene(1);
            // Enable sub-scene.
            mySceneManagerRef.EnableSubScene(3);

            // Destroy the switch itself.
            Destroy(this.gameObject, 3.0f);
        }
    }

    // Play the door shutting audio.
    private void AudioPlayDoorShut()
    {
        audioSource.PlayOneShot(audioShut);
    }
}
