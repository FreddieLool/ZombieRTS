using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the painting of prefabs in the scene.
/// Editor scripts are located in Assets/Editor.
/// Toggle Gizmo Mode in the editor to use the painting tools.
/// </summary>
/// 



// FEATURES TO DOO
// ERASE BRUSH THAT ONLY ERASES OBJECTS OF CERTAIN TYPE (GROUP), THAT ARE INTERSECTING WITH EACH OTHER, OR OTHER OBJECTS.


public class PrefabPainter : MonoBehaviour
{
    // Enums
    [HideInInspector] public enum PaintMode { Paint, Erase }
    [HideInInspector]
    public enum PatternType { Grid, Radial, Scattered }

    // Painting Modes..
    public PatternType patternMode = PatternType.Grid;
    public float patternSpacing = 5.0f;  // Distance between objects in grid mode


    // Serializable Classes
    [System.Serializable]
    public class PrefabGroup
    {
        public string groupName;
        public List<GameObject> prefabs = new List<GameObject>();
        public bool isExpanded = false;
    }

    // Serialized Fields
    [SerializeField, HideInInspector]
    private List<PrefabGroup> prefabGroups = new List<PrefabGroup>();

    // Public Properties
    public PaintMode currentMode = PaintMode.Paint;
    public LayerMask groundLayer;

    public List<PrefabGroup> PrefabGroups
    {
        get { return prefabGroups; }
        set { prefabGroups = value; }
    }

    // Non-serialized Public Fields (Editor-Only Settings)
    [NonSerialized] public int selectedGroupIndex = 0;
    [NonSerialized] public float brushSize = 10.0f;
    [NonSerialized] public int treesPerBrush = 5;
    [NonSerialized] public float minYOffset = -0.5f;
    [NonSerialized] public float maxYOffset = 0f;
    [NonSerialized] public bool randomRotation = true;
    [NonSerialized] public float treePlacementRadius = 1f;
    [NonSerialized] public bool continuousMode = true;
    [NonSerialized] public bool avoidOverlap = true;
    [NonSerialized] public bool enableRandomScale = false; // Toggle for enabling random scaling
    [NonSerialized] public Vector2 scaleRange = new Vector2(1f, 2f); // Min and Max scale range

    // Additional Behavior Controls
    [NonSerialized] public bool alignToTerrain = true;
    [NonSerialized] public float maxSlopeAngle = 45.0f; // Maximum angle of terrain slope on which prefabs can be painted
    [NonSerialized] public float minAlignAngle = 0.0f; // Minimum angle offset for aligning to terrain
    [NonSerialized] public float maxAlignAngle = 10.0f; // Maximum angle offset for aligning to terrain
}
