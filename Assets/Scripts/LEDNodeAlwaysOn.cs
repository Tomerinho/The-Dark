using UnityEngine;

// A LED node emitting light based on start/stop signals from the previous node in the chain.
public class LEDNodeAlwaysOn : MonoBehaviour
{
    // Emitted light intensity parameters.
    public float intensity = 0.0f;
    public float minIntensity = 0.0f;
    public float maxIntensity = 1.0f;


    private void Awake()
    {
        // Init light intensity.
        intensity = minIntensity;
    }

    private void Update()
    {
        // Update the calculated LED color and intensity level.
        UpdateColor();
    }

    // Update the calculated LED color and intensity level.
    private void UpdateColor()
    {
        Renderer renderer = GetComponent<Renderer>();
        Material mat = renderer.material;
        Color baseColor = mat.color;

        // Calculate the resulting color based on the intensity.
        Color finalColor = baseColor * Mathf.LinearToGammaSpace(intensity);
        mat.SetColor("_EmissionColor", finalColor);
    }
}
