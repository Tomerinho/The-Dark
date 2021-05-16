using UnityEngine;

public class SwitchDestroySS2 : MonoBehaviour
{
    // Attached components.
    public GameManager gameManager;
    public MySceneManager mySceneManagerRef;
    public AudioManager audioManager;
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

            // Dispaly the locked door.
            door.SetActive(true);

            // Play the door shutting audio.
            audioSource.PlayOneShot(audioDrag);
            Invoke("AudioPlayDoorShut", 1.0f);

            // Destroy sub-scene.
            mySceneManagerRef.DeleteSubScene(2);

            // Indicate the checkpoint has been reached..
            GameManager.isCheckpointReached = true;

            // Save the game state pre-battle.
            SavePreBattle();

            // Destroy the switch itself.
            Destroy(this.gameObject, 3.0f);
        }
    }

    // Save the game state pre-battle.
    private void SavePreBattle()
    {
        gameManager.SaveGame();
    }

    // Play the door shutting audio.
    private void AudioPlayDoorShut()
    {
        audioSource.PlayOneShot(audioShut);
    }
}
