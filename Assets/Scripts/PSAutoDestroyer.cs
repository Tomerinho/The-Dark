using UnityEngine;

public class PSAutoDestroyer : MonoBehaviour
{
    public float destroyTimer = 0.0f;
    public float destroyTimeDone = 2.4f;


    private ParticleSystem ps;


    public void Start()
    {
        // Get the attached PS.
        ps = GetComponent<ParticleSystem>();
    }

    public void Update()
    {
        // Destroy the PS after timer is done or the PS has finished.
        destroyTimer += Time.deltaTime;
        if (destroyTimer > destroyTimeDone || (ps && !ps.IsAlive()))
        {
            //Destroy(gameObject);
            destroyTimer = 0.0f;
            gameObject.SetActive(false);
        }
    }
}