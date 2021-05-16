using UnityEngine;

public class SwitchWeaponPickup : MonoBehaviour
{
    // Attached components.
    public AudioManager audioManager;
    public GameObject weapon;
    public GameObject weaponInHand;
    public GameObject hud;
    public GameObject crosshair;
    public AudioClip audioWeaponPickup;
    public AudioClip audioWeaponRecharge;
    public Ammobar2 ammobarScriptRef;
    
    // Switch control.
    public bool isTriggered { get; set; } = false;


    // Attached components.
    private AudioSource audioSource;

    // Audio control.
    private float weaponSoundVolume = 4.0f;


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

            // Destroy the picked-up object.
            Destroy(weapon.gameObject);
            // Display the object in hand.
            weaponInHand.SetActive(true);
            // Display HUD.
            hud.SetActive(true);
            // Display the crosshair.
            crosshair.SetActive(true);

            ammobarScriptRef.ammo = 100;

            // Play the weapon pick-up sound.
            audioSource.PlayOneShot(audioWeaponPickup);
            audioSource.PlayOneShot(audioWeaponRecharge, weaponSoundVolume);

            // Play the appropriate background track.
            audioManager.PlayTrack(0);

            // Destroy the switch itself.
            Destroy(this.gameObject, 3.0f);
        }
    }
}
