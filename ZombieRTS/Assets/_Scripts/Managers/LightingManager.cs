using UnityEngine.Rendering;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Events;

[ExecuteAlways]
public class LightingManager : MonoBehaviour
{
    public static LightingManager Instance { get; private set; }

    [SerializeField] private Light DirectionalLight;
    [SerializeField] private LightingPreset Preset;
    [SerializeField] private Volume globalVolume;
    private Vignette vignette;
    private int cycleCount = 0; // Tracks the number of complete cycles
    [SerializeField] float cycleSpeed = 1f;


    // Cycle manager
    [SerializeField, Range(0, 24)] private float TimeOfDay = 17f;
    public UnityEvent onNewCycle;


    // InGame
    private bool nightSoundPlayed = false;
    private float timeForNightSound;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }

        onNewCycle = new UnityEvent();
    }

    private void Start()
    {
        if (globalVolume.profile.TryGet<Vignette>(out var v) != false)
        {
            vignette = v;
        }
    }

    private void Update()
    {
        // --------------------

        if (Preset == null)
            return;

        float previousTime = TimeOfDay;
        TimeOfDay += Application.isPlaying ? Time.deltaTime * cycleSpeed : 0;
        TimeOfDay %= 24; // always between 0-24
        UpdateLighting(TimeOfDay / 24f);
        if (vignette != null)
            UpdateVignette(TimeOfDay);

        if (previousTime > TimeOfDay)  // Check if a new day has started
        {
            nightSoundPlayed = false;  // Reset the night sound flag each day
            SetRandomTimeForNightSound();
        }

        // Cycle counter
        if (previousTime > TimeOfDay) // wraps
        {
            cycleCount++;
            onNewCycle.Invoke();
        }

        UpdateLighting(TimeOfDay / 24f);
    }

    private bool IsNightTime()
    {
        return TimeOfDay >= 22 || TimeOfDay <= 5;
    }

    private void SetRandomTimeForNightSound()
    {
        float startNight = TimeOfDay >= 22 ? TimeOfDay : 22;
        float endNight = 5;
        timeForNightSound = Random.Range(startNight, 29) % 24;  // Generate a random time between 22 and 5
        if (timeForNightSound > 5)  // Ensure the time is within the night period
            timeForNightSound = 22 + (timeForNightSound - 22) % (24 - 22);
    }

    private void UpdateLighting(float timePercent)
    {
        RenderSettings.ambientLight = Preset.AmbientColor.Evaluate(timePercent);
        RenderSettings.fogColor = Preset.FogColor.Evaluate(timePercent);

        if (DirectionalLight != null)
        {
            DirectionalLight.color = Preset.DirectionalColor.Evaluate(timePercent);
            float angle = Mathf.Lerp(-12f, 201f, timePercent); // Clamped between -12 at 24:00 and 201 at 00:00
            DirectionalLight.transform.localRotation = Quaternion.Euler(new Vector3(angle, 170f, 0));
        }
    }

    private void UpdateVignette(float time)
    {
        // Handle the vignette intensity from 15:00 to 7:50 across midnight transition
        if (time >= 15 || time <= 7.5)
        {
            // Normalize time across the midnight boundary
            float normTime = 0;
            if (time >= 15)
            {
                // From 15:00 to 24:00
                normTime = (time - 15) / (24 - 15);
                vignette.intensity.value = Mathf.Lerp(0.15f, 0.45f, normTime);
            }
            else if (time <= 7.5)
            {
                // From 0:00 to 7:50
                normTime = time / 7.5f;
                vignette.intensity.value = Mathf.Lerp(0.45f, 0.15f, normTime);
            }
        }
        else
        {
            // Outside these hours, ensure it maintains the minimum default intensity
            vignette.intensity.value = 0.15f;
        }
    }

    public int GetCurrentCycle()
    {
        return cycleCount;
    }

    public float GetCurrentTime()
    {
        return TimeOfDay;
    }


    private void OnValidate()
    {
        if (DirectionalLight != null)
            return;

        if (RenderSettings.sun != null)
        {
            DirectionalLight = RenderSettings.sun;
        }
        else
        {
            foreach (Light light in GameObject.FindObjectsOfType<Light>())
            {
                if (light.type == LightType.Directional)
                {
                    DirectionalLight = light;
                    return;
                }
            }
        }
    }
}
