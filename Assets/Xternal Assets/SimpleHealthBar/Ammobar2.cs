using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Only allow this script to be attached to the object with the ammobar slider:
[RequireComponent(typeof(Slider))]
public class Ammobar2 : MonoBehaviour {
    //External Game Manager.
    public GameManager GM;

    // Visible ammo bar ui:
    private Slider ammobarDisplay;

    [Header("Main Variables:")]
    // Ammo variable: (default range: 0-100)
    [Tooltip("Ammo variable: (default range: 0-100)")] public float ammo = 100;

    // Percentage of how full your ammo is: (0-100, no decimals)
    private int ammoPercentage = 100;

    // Minimum possible heath:
    [Tooltip("Minimum possible heath: (default is 0)")] public float minimumAmmo = 0;

    // Maximum possible ammo:
    [Tooltip("Maximum possible heath: (default is 100)")] public float maximumAmmo = 100;

    // If the character has this ammo or less, consider them having low ammo:
    [Tooltip("Low ammo is less than or equal to this:")] public int lowAmmo = 33;

    // If the character has between this ammo and "low ammo", consider them having medium ammo:
    // If they have more than this ammo, consider them having highAmmo:
    [Tooltip("High ammo is greater than or equal to this:")] public int highAmmo = 66;

    [Space]

    [Header("Regeneration:")]    
    // If 'regenerateAmmo' is checked, character will regenerate ammo/sec at the rate of 'ammoPerSecond':
    public bool regenerateAmmo;
    public float ammoPerSecond;

    [Space]

    [Header("Ammobar Colors:")]
    public Color highAmmoColor = new Color(0.35f, 1f, 0.35f);
    public Color mediumAmmoColor = new Color(0.9450285f, 1f, 0.4481132f);
    public Color lowAmmoColor = new Color(1f, 0.259434f, 0.259434f);

    // Attached fill object for controlling color changes.
    public GameObject objFill;

    private void Start()
    {
        // If the ammobar hasn't already been assigned, then automatically assign it.
        if (ammobarDisplay == null)
        {
            ammobarDisplay = GetComponent<Slider>();
        }

        // Set the minimum and maximum ammo on the ammobar to be equal to the 'minimumAmmo' and 'maximumAmmo' variables:
        ammobarDisplay.minValue = minimumAmmo;
        ammobarDisplay.maxValue = maximumAmmo;

        // Change the starting visible ammo to be equal to the variable:
        UpdateAmmo();
    }

    // Every frame:
    private void Update()
    {
        if (GM.isPaused == false)
        {
            ammoPercentage = int.Parse((Mathf.Round(maximumAmmo * (ammo / 1000f))).ToString());

            // If the player's ammo is below the minimum ammo, then set it to the minimum ammo:
            if (ammo < minimumAmmo)
            {
                ammo = minimumAmmo;
            }

            // If the player's ammo is above the maximum ammo, then set it to the maximum ammo:
            if (ammo > maximumAmmo)
            {
                ammo = maximumAmmo;
            }

            // If the character's ammo is not full and the ammo regeneration button is ticked, regenerate ammo/sec at the rate of 'ammoPerSecond':
            if (ammo < maximumAmmo && regenerateAmmo)
            {
                ammo += ammoPerSecond * Time.deltaTime;
            }

            // Each time the ammo is changed, update it visibly:
            UpdateAmmo();
        }
    }

    // Set the ammo bar to display the same ammo value as the ammo variable:
    public void UpdateAmmo()
    {
        // Change the ammo bar color acording to how much ammo the player has:
        if (ammoPercentage <= lowAmmo && ammo >= minimumAmmo && objFill.GetComponent<Image>().color != lowAmmoColor)
        {
            ChangeAmmobarColor(lowAmmoColor);
        }
        else if (ammoPercentage <= highAmmo && ammo > lowAmmo)
        {
            float lerpedColorValue = (float.Parse(ammoPercentage.ToString()) - 250) / 410;
            ChangeAmmobarColor(Color.Lerp(lowAmmoColor, mediumAmmoColor, lerpedColorValue));
        }
        else if (ammoPercentage > highAmmo && ammo <= maximumAmmo)
        {
            float lerpedColorValue = (float.Parse(ammoPercentage.ToString()) - 670) / 330;
            ChangeAmmobarColor(Color.Lerp(mediumAmmoColor, highAmmoColor, lerpedColorValue));
        }

        ammobarDisplay.value = ammo;
    }

    public void GainAmmo(float amount)
    {
        // Add 'amount' hitpoints, then update the characters ammo:
        ammo += amount;
        UpdateAmmo();
    }

    public void TakeDamage(int amount)
    {
        // Remove 'amount' hitpoints, then update the characters ammo:
        ammo -= float.Parse(amount.ToString());
        UpdateAmmo();
    }

    public void ChangeAmmobarColor(Color colorToChangeTo)
    {
        objFill.GetComponent<Image>().color = colorToChangeTo;
    }

    public void ToggleRegeneration()
    {
        regenerateAmmo = !regenerateAmmo;
    }

    public void SetAmmo(float value)
    {
        ammo = value;
        UpdateAmmo();
    }
}