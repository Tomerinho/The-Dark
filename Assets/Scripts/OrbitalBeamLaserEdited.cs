using UnityEngine;

public class OrbitalBeamLaserEdited : MonoBehaviour
{
    //public ParticleSystem LaserSparks,
    //LaserSmoke;

    public AudioSource LaserChargeAudio,
        LaserAudio,
        LaserStopAudio;

    public GameObject LaserEffects,
        LaserChargeBeam;
        //SmokeAndSparks;

    public float ChargeTime = 1.4f,
        FireTime = 2.6f,
        CooldownTime = 3.0f;

    //[HideInInspector]
    public bool LaserActive = false;

    // Laser hit points.
    public int hitpoint { get; set; } = 6;
    // Laser active length parameter (should be controlled according to distance from target).
    public int activeLength { get; set; } = 5;

    // The laser beam's collider and visual length.
    private Collider colliderBox;
    private Transform[] laserLength;

    // Charging control.
    private float LaserChargeLevel = 0f;
    private bool hasPlayedChargingEffects = false;

    void Start()
    {
        InitializeLaser();
    }

    void Update()
    {
        if (LaserActive)
        {
            UpdateLaserLength();

            if (LaserChargeLevel < ChargeTime && hasPlayedChargingEffects == false && !LaserChargeBeam.activeSelf)
            {
                ShowChargingEffects();
            }
            else if (LaserChargeLevel >= ChargeTime && !LaserEffects.activeSelf)
            {
                ShowFiringEffects();
            }
            LaserChargeLevel += Time.deltaTime;
        }
        else
        {
            if (LaserEffects.activeSelf || LaserChargeBeam.activeSelf)
            {
                StopEffects();
                // Added to reset the charge effect every fire shot.
                LaserChargeLevel = 0f;
            }

            if (LaserChargeLevel > 0f)
            {
                LaserChargeLevel -= Time.deltaTime * (FireTime + ChargeTime) / CooldownTime;

                if (LaserChargeLevel <= 0f)
                {
                    LaserChargeLevel = 0f;
                }
            }
            UpdateLaserLength();
        }
    }

    private void InitializeLaser()
    {
        // Init effects.
        LaserEffects.SetActive(false);
        LaserChargeBeam.SetActive(false);
        //SmokeAndSparks.SetActive(false);
        LaserChargeAudio.Stop();
        LaserAudio.Stop();
        LaserStopAudio.Stop();

        // Init collider.
        colliderBox = this.GetComponent<BoxCollider>();
        colliderBox.enabled = false;

        // Init laser length vector.
        laserLength = new Transform[7];

    }

    private void ShowChargingEffects()
    {
        LaserEffects.SetActive(false);
        LaserChargeBeam.SetActive(true);
        LaserChargeAudio.Play();
        hasPlayedChargingEffects = true;
    }

    private void ShowFiringEffects()
    {
        LaserEffects.SetActive(true);
        LaserChargeBeam.SetActive(false);
        //SmokeAndSparks.SetActive(true);
        //LaserSparks.Play();
        //LaserSmoke.Play();
        LaserAudio.Play();

        colliderBox.enabled = true;
    }

    private void StopEffects()
    {
        LaserEffects.SetActive(false);
        LaserChargeBeam.SetActive(false);
        //LaserSparks.Stop();
        //LaserSmoke.Stop();
        LaserAudio.Stop();
        LaserStopAudio.Play();
        hasPlayedChargingEffects = false;

        colliderBox.enabled = false;
    }

    private void UpdateLaserLength()
    {
        // Updated laser length according to the active length parameter (controlled from outside).
        int i = 0;
        for (; i < activeLength && i < laserLength.Length; i++)
        {
            laserLength[i] = this.transform.GetChild(0).GetChild(i);
            laserLength[i].gameObject.SetActive(true);
        }
        for (; i < laserLength.Length; i++)
        {
            laserLength[i] = this.transform.GetChild(0).GetChild(i);
            laserLength[i].gameObject.SetActive(false);
        }
    }
}