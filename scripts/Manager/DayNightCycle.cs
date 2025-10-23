using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Header("Sun/Moon")]
    public Light directionalLight; // Your main sun/moon light
    public Transform celestialBody; // Optional: visual sun/moon object

    [Header("Day Settings")]
    public Gradient dayNightColor; // Sky color gradient
    public AnimationCurve lightIntensityCurve; // Light intensity over time
    public Color dayAmbientLight = new Color(0.5f, 0.5f, 0.5f);
    public Color nightAmbientLight = new Color(0.1f, 0.1f, 0.15f);

    [Header("Sun Colors")]
    public Color sunriseColor = new Color(1f, 0.6f, 0.3f);
    public Color dayColor = new Color(1f, 0.95f, 0.8f);
    public Color sunsetColor = new Color(1f, 0.4f, 0.2f);
    public Color nightColor = new Color(0.5f, 0.5f, 0.8f);

    [Header("Fog Settings")]
    public bool useFog = true;
    public Color dayFogColor = new Color(0.5f, 0.6f, 0.7f);
    public Color nightFogColor = new Color(0.05f, 0.05f, 0.1f);

    private float timeOfDay = 0f; // 0 = midnight, 0.25 = 6am, 0.5 = noon, 0.75 = 6pm, 1 = midnight

    void Start()
    {
        // Create default directional light if none assigned
        if (directionalLight == null)
        {
            GameObject lightObj = new GameObject("Sun/Moon Light");
            directionalLight = lightObj.AddComponent<Light>();
            directionalLight.type = LightType.Directional;
            directionalLight.shadows = LightShadows.Soft;
        }

        // Setup default gradient if none exists
        SetupDefaultGradient();

        // Setup default intensity curve
        SetupDefaultIntensityCurve();

        // Enable fog
        if (useFog)
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 20f;
            RenderSettings.fogEndDistance = 100f;
        }

        // Initialize with current game time
        if (GameManager.Instance != null)
        {
            float normalizedTime = (GameManager.Instance.currentHour + GameManager.Instance.currentMinute / 60f) / 24f;
            SetTimeOfDay(normalizedTime);
        }
    }

    void Update()
    {
        // The GameManager now controls the time, we just update visuals
        UpdateLighting();
        UpdateSunPosition();
        UpdateSkybox();
        UpdateFog();
    }

    void UpdateLighting()
    {
        if (directionalLight == null) return;

        // Evaluate light intensity from curve
        float intensity = lightIntensityCurve.Evaluate(timeOfDay);
        directionalLight.intensity = intensity;

        // Change light color based on time
        Color lightColor;

        // Sunrise: 4:30-7:30 (0.1875 - 0.3125)
        // Day: 7:30-17:30 (0.3125 - 0.7292)
        // Sunset: 17:30-20:30 (0.7292 - 0.8542)
        // Night: 20:30-4:30 (0.8542 - 1.0 and 0.0 - 0.1875)

        if (timeOfDay < 0.1875f) // Early morning/night (midnight to 4:30am)
        {
            lightColor = nightColor;
        }
        else if (timeOfDay < 0.3125f) // Sunrise (4:30am to 7:30am)
        {
            float t = (timeOfDay - 0.1875f) / 0.125f;
            lightColor = Color.Lerp(nightColor, sunriseColor, t);
        }
        else if (timeOfDay < 0.5f) // Morning to Noon (7:30am to 12pm)
        {
            float t = (timeOfDay - 0.3125f) / 0.1875f;
            lightColor = Color.Lerp(sunriseColor, dayColor, t);
        }
        else if (timeOfDay < 0.7292f) // Afternoon (12pm to 5:30pm)
        {
            lightColor = dayColor;
        }
        else if (timeOfDay < 0.8542f) // Sunset (5:30pm to 8:30pm)
        {
            float t = (timeOfDay - 0.7292f) / 0.125f;
            lightColor = Color.Lerp(sunsetColor, nightColor, t);
        }
        else // Night (8:30pm to midnight)
        {
            lightColor = nightColor;
        }

        directionalLight.color = lightColor;

        // Update ambient light
        Color ambientColor = Color.Lerp(nightAmbientLight, dayAmbientLight, intensity);
        RenderSettings.ambientLight = ambientColor;
    }

    void UpdateSunPosition()
    {
        if (directionalLight == null) return;

        // Rotate the sun/moon around the world
        // 0 = midnight (moon overhead), 0.5 = noon (sun overhead)
        float angle = timeOfDay * 360f - 90f;
        directionalLight.transform.rotation = Quaternion.Euler(new Vector3(angle, 170f, 0));

        // Update celestial body if assigned
        if (celestialBody != null)
        {
            celestialBody.rotation = directionalLight.transform.rotation;
        }
    }

    void UpdateSkybox()
    {
        // Evaluate gradient for sky tint
        Color skyColor = dayNightColor.Evaluate(timeOfDay);
        RenderSettings.ambientSkyColor = skyColor;

        // If using a skybox material, you can tint it here
        if (RenderSettings.skybox != null)
        {
            RenderSettings.skybox.SetColor("_Tint", skyColor);
        }
    }

    void UpdateFog()
    {
        if (!useFog) return;

        // Interpolate fog color between day and night
        float dayNightBlend = lightIntensityCurve.Evaluate(timeOfDay);
        Color fogColor = Color.Lerp(nightFogColor, dayFogColor, dayNightBlend);
        RenderSettings.fogColor = fogColor;
    }

    void SetupDefaultGradient()
    {
        if (dayNightColor == null || dayNightColor.colorKeys.Length == 0)
        {
            dayNightColor = new Gradient();
            GradientColorKey[] colorKeys = new GradientColorKey[5];
            colorKeys[0] = new GradientColorKey(new Color(0.05f, 0.05f, 0.1f), 0f);    // Midnight - dark blue
            colorKeys[1] = new GradientColorKey(new Color(1f, 0.5f, 0.3f), 0.25f);     // Sunrise - orange
            colorKeys[2] = new GradientColorKey(new Color(0.5f, 0.7f, 1f), 0.5f);      // Noon - bright blue
            colorKeys[3] = new GradientColorKey(new Color(1f, 0.4f, 0.2f), 0.75f);     // Sunset - red/orange
            colorKeys[4] = new GradientColorKey(new Color(0.05f, 0.05f, 0.1f), 1f);    // Midnight - dark blue

            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
            alphaKeys[0] = new GradientAlphaKey(1f, 0f);
            alphaKeys[1] = new GradientAlphaKey(1f, 1f);

            dayNightColor.SetKeys(colorKeys, alphaKeys);
        }
    }

    void SetupDefaultIntensityCurve()
    {
        if (lightIntensityCurve == null || lightIntensityCurve.length == 0)
        {
            lightIntensityCurve = new AnimationCurve();

            // Adjusted to match realistic timing with darkness at 19:00 (7pm)
            lightIntensityCurve.AddKey(0f, 0.05f);      // Midnight - very dark
            lightIntensityCurve.AddKey(0.25f, 0.3f);    // 6am - dawn
            lightIntensityCurve.AddKey(0.375f, 1f);     // 9am - full daylight
            lightIntensityCurve.AddKey(0.5f, 1f);       // Noon - brightest
            lightIntensityCurve.AddKey(0.708f, 1f);     // 5pm - still bright
            lightIntensityCurve.AddKey(0.792f, 0.3f);   // 7pm (19:00) - getting dark
            lightIntensityCurve.AddKey(0.875f, 0.05f);  // 9pm - dark
            lightIntensityCurve.AddKey(1f, 0.05f);      // Midnight - very dark
        }
    }

    // Called from GameManager to sync visual time
    public void SetTimeOfDay(float normalizedTime)
    {
        timeOfDay = Mathf.Clamp01(normalizedTime);
    }

    // Helper to get current time as readable string
    public string GetTimeString()
    {
        int hour = Mathf.FloorToInt(timeOfDay * 24f);
        int minute = Mathf.FloorToInt((timeOfDay * 24f - hour) * 60f);
        return $"{hour:00}:{minute:00}";
    }

    // Get current timeOfDay value (useful for debugging)
    public float GetTimeOfDay()
    {
        return timeOfDay;
    }
}