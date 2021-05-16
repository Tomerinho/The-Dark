using System.Collections;
using UnityEngine;

public class LaserGun : MonoBehaviour
{
    // External Game Manager.
    public GameManager GM;
    // Attached weapon effects prefabs.
    public ParticleSystem effects = null;
    public ParticleSystem effects2 = null;
    // Attached weapon socket prefab.
    public Transform weaponSocket = null;
    // Attached recoil empty object.
    public Transform recoilMod;
    // Attached audio clips.
    public AudioClip audioRecharge = null;
    public AudioClip audioNoAmmo = null;
    public AudioClip audioCooldown = null;


    // Weapon ammo bar script.
    public Ammobar2 ammobarScriptRef;

    // Weapon's ammo.
    public float weaponAmmo { get; set; }


    // Recoil parameters.
    private bool doRecoil = false;
    private float maxRecoil_x = -20.0f;
    private float recoilSpeed = 10.0f;
    private float recoil = 0.0f;

    // Bob parameters.
    private Vector3 midPoint;
    private float horizontal, vertical, timer, waveSlice;
    private float bobSpeed = 0.1f;
    private float bobDistance = 80.0f;

    // Weapon positioning according to monitor.
    private Transform weaponTrnsfrm;
    private Camera cam;
    private float offsetFromBottom = 60.4f;
    private float offsetFromRight = 102.9f;
    private float distanceFromCam = 0.16f;

    // Weapon parameters.
    private float bulletsPerSecond = 3.0f;
    private float fireRate = 0.0f;
    private float ammoCost;
    private float shotTimer = 0.0f;
    private float effectsTimer = 0.0f;
    private float coolDownTime = 2.0f;
    private float coolDownTimer = 0.0f;
    private bool isCoolingDown = false;
    private float rechargeTime = 0.7f;
    private float rechargeTimer = 0.0f;
    private bool isWeaponRechargeCR = false;

    // Audio components.
    private AudioSource audioSource = null;
    private float weaponSoundVolume = 4.0f;


    private void Awake()
    {
        // Init weapon's ammo.
        weaponAmmo = ammobarScriptRef.ammo;
        ammoCost = LaserBullet.ammoCost;

        // Disable gun special effects.
        effects.Stop();
        effects2.Stop();

        // Init audio component.
        audioSource = this.GetComponent<AudioSource>();
    }

    private void Start()
    {
        // Init weapon parameters.
        fireRate = 1 / bulletsPerSecond;

        // Init weapon position based on the monitor.
        cam = Camera.main;
        weaponTrnsfrm = this.transform;
        weaponTrnsfrm.localPosition = Vector3.zero;
        weaponTrnsfrm.position = cam.ScreenToWorldPoint(new Vector3(Screen.width + offsetFromRight, 0 + offsetFromBottom, distanceFromCam));

        // Init bob parameters.
        midPoint = weaponTrnsfrm.position;
    }

    private void Update()
    {
        if (GM.isPaused == false && GM.isCreditsOn == false)
        {
            // Weapon shooting.
            Shoot();

            // Weapon recoiling.
            Recoil();

            // Weapon bobbing.
            Bob();
        }

        // Update weapon position based on the monitor.
        weaponTrnsfrm.position = cam.ScreenToWorldPoint(new Vector3(Screen.width + offsetFromRight + 100.0f, 0 + offsetFromBottom, distanceFromCam));

        // Update weapon's ammo.
        weaponAmmo = ammobarScriptRef.ammo;
    }

    // Responsible for weapon shooting.
    private void Shoot()
    {
        // Timer counting until the next shot is allowed.
        shotTimer += Time.deltaTime;

        // If the weapon isn't in cool-down mode.
        if (isCoolingDown == false)
        {
            // When user holds the fire button down.
            if (Input.GetButton("Fire1"))
            {
                // If shot timer has elapsed.
                if (shotTimer > fireRate)
                {
                    // Shoot.
                    //GameObject obj1 = Instantiate(bullet, weaponSocket.position, weaponSocket.rotation) as GameObject;
                    GameObject obj1 = ObjectPool.SharedInstance.GetPooledObject();
                    if (obj1 != null)
                    {
                        obj1.name = "bulletBlue";
                        obj1.transform.position = weaponSocket.position;
                        obj1.transform.rotation = weaponSocket.rotation;
                        obj1.SetActive(true);
                    }
                    shotTimer = 0;

                    // Reduce the appropriate ammo amount.
                    AmmoUpdate(ammoCost);

                    // Start special effects.
                    effects.Play();
                    effects2.Play();
                    effectsTimer = 0;

                    // Indicate recoil.
                    doRecoil = true;
                }
            }
        }
        // else, the weapon is in cool-down mode.
        else
        {
            // If the cool-down timer hasn't finished, increase the cool-down timer.
            if (coolDownTimer < coolDownTime)
            {
                coolDownTimer += Time.deltaTime;

                // If the user tries to shoot at this state, play the no-ammo audio.
                if (Input.GetButton("Fire1"))
                {
                    // If shot timer has elapsed.
                    if (shotTimer > fireRate)
                    {
                        shotTimer = 0;
                        audioSource.PlayOneShot(audioNoAmmo, weaponSoundVolume);
                    }
                }
            }
            // else, exit cool-down.
            else
            {
                isCoolingDown = false;
                coolDownTimer = 0;
                ammobarScriptRef.regenerateAmmo = true;
                audioSource.PlayOneShot(audioRecharge, weaponSoundVolume);
            }
        }

        // If the special effects are over.
        if (effectsTimer > 0.2f)
        {
            effects.Stop();
        }
        else
        {
            effectsTimer += Time.deltaTime;
        }
    }

    // Responsible for weapon bobbing while walking.
    private void Bob()
    {
        // Calculate weapon bob based on horizontal and vertical axes.
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        if (Mathf.Abs(horizontal) == 0 && Mathf.Abs(vertical) == 0)
        {
            timer = 0.0f;
        }
        else
        {
            waveSlice = Mathf.Sin(timer);
            timer = timer + bobSpeed;
            if (timer > Mathf.PI * 2)
            {
                timer = timer - (Mathf.PI * 2);
            }
        }
        if (waveSlice != 0)
        {
            float translateChange = waveSlice * bobDistance;
            float totalAxes = Mathf.Abs(horizontal) + Mathf.Abs(vertical);
            totalAxes = Mathf.Clamp(totalAxes, 0.0f, 1.0f);
            translateChange = totalAxes * translateChange;
            offsetFromBottom = midPoint.y + translateChange;
            offsetFromRight = midPoint.x + translateChange * 2;
        }
        else
        {
            offsetFromBottom = midPoint.y;
            offsetFromRight = midPoint.x;
        }
    }

    // Responsible for weapon recoiling when shooting.
    private void Recoil()
    {
        if (doRecoil == true)
        {
            doRecoil = false;
            //every time you fire a bullet, add to the recoil.. of course you can probably set a max recoil etc..
            recoil += 0.1f;
        }

        if (recoil > 0)
        {
            var maxRecoil = Quaternion.Euler(maxRecoil_x, 0, 0);
            // Dampen towards the target rotation
            recoilMod.rotation = Quaternion.Slerp(recoilMod.rotation, maxRecoil, Time.deltaTime * recoilSpeed);
            weaponTrnsfrm.localEulerAngles = new Vector3(-recoilMod.localEulerAngles.x, weaponTrnsfrm.localEulerAngles.y, weaponTrnsfrm.localEulerAngles.z);
            recoil -= Time.deltaTime;
        }
        else
        {
            recoil = 0;
            var minRecoil = Quaternion.Euler(0, 0, 0);
            // Dampen towards the target rotation
            recoilMod.rotation = Quaternion.Slerp(recoilMod.rotation, minRecoil, Time.deltaTime * recoilSpeed / 2);
            weaponTrnsfrm.localEulerAngles = new Vector3(-recoilMod.localEulerAngles.x, weaponTrnsfrm.localEulerAngles.y, weaponTrnsfrm.localEulerAngles.z);
        }
    }

    // Update the weapon's ammo when shooting.
    private void AmmoUpdate(float ammo)
    {
        // If the weapon recharge co-routine is running, stop it.
        if (isWeaponRechargeCR == true)
        {
            StopCoroutine("WeaponRecharge");
            // Indicate the co-routine has stopped.
            isWeaponRechargeCR = false;
            // Init the recharge timer.
            rechargeTimer = 0.0f;
        }

        // Reduce the ammo cost from the weapon's ammo amount.
        ammobarScriptRef.ammo -= ammo;
        // If ammo amount is under the minimum threshold, enter cool-down mode.
        if (ammobarScriptRef.ammo < 100)
        {

            ammobarScriptRef.ammo = 100;
            ammobarScriptRef.regenerateAmmo = false;
            isCoolingDown = true;
            audioSource.PlayOneShot(audioCooldown, weaponSoundVolume * 0.7f);
        }
        else
        {
            // Start the weapon recharge co-routine.
            StartCoroutine("WeaponRecharge");
        }
    }

    // Play the weapon recharge audio after a certain amount of time.
    private IEnumerator WeaponRecharge()
    {
        // Indicate the co-routine has started.
        isWeaponRechargeCR = true;

        // Stop regenerating ammo.
        ammobarScriptRef.regenerateAmmo = false;

        // While the recharge timer hasn't finished.
        while (rechargeTimer < rechargeTime)
        {
            // Increase the recharge timer.
            rechargeTimer += Time.deltaTime;

            yield return null;
        }

        // Continue regenerating ammo.
        ammobarScriptRef.regenerateAmmo = true;
        // Play the recharge audio.
        audioSource.PlayOneShot(audioRecharge, weaponSoundVolume * 2.0f);
        // Init the recharge timer.
        rechargeTimer = 0.0f;

        // Indicate the co-routine has stopped.
        isWeaponRechargeCR = false;
    }
}
