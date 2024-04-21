using System;
using System.Collections.Generic;
using UnityEngine;


// Editor scripts are in Assets/Editor
// In order to paint, Toggle Gizmo Mode in editor
public class PrefabPainter : MonoBehaviour
{
    [System.Serializable]
    public class PrefabGroup
    {
        public string groupName;
        public List<GameObject> prefabs = new List<GameObject>();
    }

    public enum PaintMode { Paint, Delete }
    public PaintMode currentMode = PaintMode.Paint;
    public LayerMask groundLayer;
    public List<PrefabGroup> prefabGroups = new List<PrefabGroup>();
    [NonSerialized] public int selectedGroupIndex = 0;
    [NonSerialized] public float brushSize = 10.0f;
    [NonSerialized] public int treesPerBrush = 5;
    [NonSerialized] public float minYOffset = -0.5f;
    [NonSerialized] public float maxYOffset = 0f;
    [NonSerialized] public bool randomRotation = true;
    [NonSerialized] public float treePlacementRadius = 1f;
    [NonSerialized] public bool continuousMode = true;
    [NonSerialized] public bool avoidOverlap = true;
}
