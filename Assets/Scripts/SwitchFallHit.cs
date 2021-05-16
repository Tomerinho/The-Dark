using UnityEngine;

public class SwitchFallHit : MonoBehaviour
{
    // Attached components.
    public Hero heroScriptRef;

    // Switch control.
    public bool isTriggered { get; set; } = false;


    // Damage hitpoints from falling.
    private int fallHitPoint = 30;


    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "player" && isTriggered == false)
        {
            // Disable second-time triggering.
            isTriggered = true;

            // Indicate the damage done.
            heroScriptRef.OnDamage(fallHitPoint);

            // Destroy the switch itself.
            Destroy(this.gameObject);
        }
    }
}
