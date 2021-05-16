using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class SwitchFootsteps : MonoBehaviour
{
    // Attached components.
    public FirstPersonController FPControllerRef;


    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "player")
        {
            // Switch footsteps type indication.
            FPControllerRef.footStepType = 1;

            // Adjust the footsteps volume.
            FPControllerRef.m_AudioSource.volume = 0.25f;

            // Destroy the switch itself.
            Destroy(this.gameObject);
        }
    }
}
