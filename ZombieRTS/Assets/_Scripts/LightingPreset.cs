using UnityEngine;

[CreateAssetMenu(fileName = "New Lighting Preset", menuName = "Environment/Lighting Preset")]
public class LightingPreset : ScriptableObject
{
    public Gradient AmbientColor;
    public Gradient DirectionalColor;
    public Gradient FogColor;
}
