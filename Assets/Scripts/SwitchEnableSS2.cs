using UnityEngine;

public class SwitchEnableSS2 : MonoBehaviour
{
    // Attached components.
    public MySceneManager mySceneManagerRef;

    // Switch control.
    public bool isTriggered { get; set; } = false;


    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "player" && isTriggered == false)
        {
            // Disable second-time triggering.
            isTriggered = true;

            // Enable sub-scene.
            mySceneManagerRef.EnableSubScene(2);

            // Destroy the switch itself.
            Destroy(this.gameObject);
        }
    }
}
