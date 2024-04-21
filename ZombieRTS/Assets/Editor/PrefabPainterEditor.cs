using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(PrefabPainter))]
public class PrefabPainterEditor : Editor
{
    private List<Vector3> placedPositions = new List<Vector3>();

    private Texture2D headerLogo;
    private Texture2D paintIcon;
    private Texture2D deleteIcon;


    void OnEnable()
    {
        headerLogo = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/__Assets/UI Images/EDITORLOGO.png");
        paintIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/__Assets/UI Images/paintBrush.png");
        deleteIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/__Assets/UI Images/removeBrush.png");
    }

    void OnSceneGUI()
    {
        PrefabPainter painter = (PrefabPainter)target;
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        Event e = Event.current;

        // Draw brush visualization based on mode
        DrawBrushCircle(painter, e);

        if ((e.type == EventType.MouseDown || (e.type == EventType.MouseDrag && painter.continuousMode)) && e.button == 0 && !e.alt)
        {
            if (painter.currentMode == PrefabPainter.PaintMode.Paint)
            {
                PaintPrefabs(e, painter);
            }
            else if (painter.currentMode == PrefabPainter.PaintMode.Delete)
            {
                DeletePrefabs(e, painter);
            }
            e.Use();
        }
    }

    private void DrawBrushCircle(PrefabPainter painter, Event e)
    {
        if (Event.current.type == EventType.Repaint)
        {
            Handles.color = Color.yellow;
            Vector3 mousePosition = Event.current.mousePosition;
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, painter.groundLayer))
            {
                Handles.DrawWireDisc(hit.point, Vector3.up, painter.brushSize);
            }
        }
    }

    private void DeletePrefabs(Event e, PrefabPainter painter)
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, painter.groundLayer))
        {
            // Define a sphere where the mouse is pointing to delete objects within that sphere
            Collider[] hitColliders = Physics.OverlapSphere(hit.point, painter.brushSize, ~0, QueryTriggerInteraction.Ignore);
            foreach (Collider col in hitColliders)
            {
                if (col.transform.parent == painter.transform) // Ensure we only delete objects managed by this painter
                {
                    Undo.DestroyObjectImmediate(col.gameObject); // Use Undo to allow revert
                }
            }
        }
    }

    private void PaintPrefabs(Event e, PrefabPainter painter)
    {
        if (painter.selectedGroupIndex >= 0 && painter.selectedGroupIndex < painter.prefabGroups.Count)
        {
            List<GameObject> prefabsToPaint = painter.prefabGroups[painter.selectedGroupIndex].prefabs;
            for (int i = 0; i < painter.treesPerBrush; i++)
            {
                Vector3 point = RandomPointInCircle(e.mousePosition, painter.brushSize);
                Ray ray = HandleUtility.GUIPointToWorldRay(point);
                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, painter.groundLayer))
                {
                    Vector3 position = hit.point + Vector3.up * Random.Range(painter.minYOffset, painter.maxYOffset);
                    Quaternion rotation = painter.randomRotation ? Quaternion.Euler(0, Random.Range(0, 360), 0) : Quaternion.identity;

                    if (IsPositionValid(position, painter.treePlacementRadius, painter))
                    {
                        GameObject prefab = prefabsToPaint[Random.Range(0, prefabsToPaint.Count)];
                        GameObject newObject = Instantiate(prefab, position, rotation);
                        newObject.transform.SetParent(painter.transform);
                        Undo.RegisterCreatedObjectUndo(newObject, "Place Prefab");
                        placedPositions.Add(position);
                    }
                }
            }
        }
    }

    private bool IsPositionValid(Vector3 position, float minimumDistance, PrefabPainter painter)
    {
        foreach (Vector3 placedPosition in placedPositions)
        {
            if (Vector3.Distance(position, placedPosition) < minimumDistance)
                return false;
        }
        return true;
    }

    private Vector3 RandomPointInCircle(Vector2 mousePosition, float radius)
    {
        Vector2 randomPoint = Random.insideUnitCircle * radius;
        return new Vector3(mousePosition.x + randomPoint.x, mousePosition.y + randomPoint.y, 0);
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (headerLogo != null)
        {
            GUILayout.Space(20);
            var rect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(145));
            GUI.DrawTexture(rect, headerLogo, ScaleMode.ScaleToFit);
            GUILayout.Space(20);
        }

        GUILayout.Label("INFECTED EDITOR 2.0");
        GUILayout.Label("Created by Wael A.E");
        GUILayout.Label("cuz terrain painter sucks.");

        PrefabPainter painter = (PrefabPainter)target;
        GUILayout.Space(20);

        // Define the size of the icons
        float iconSize = 100f;
        GUIStyle iconStyle = new GUIStyle(GUI.skin.button)
        {
            fixedWidth = iconSize,
            fixedHeight = iconSize,
            imagePosition = ImagePosition.ImageAbove,
            padding = new RectOffset(2, 2, 2, 2), // Minimal padding around the icon
            margin = new RectOffset(5, 5, 5, 5) // Minimal margin around elements
        };

        // Apply changes and refresh the editor window
        if (GUI.changed)
        {
            EditorUtility.SetDirty(painter);
            SceneView.RepaintAll(); // Refresh the Scene view if necessary
        }

        if (GUILayout.Button("Save Groups"))
        {
            SavePrefabPainter(painter);
        }


        // If there are any other properties that get modified via the Inspector, ensure they trigger a save too.
        EditorGUI.BeginChangeCheck();
        GUILayout.Label("Groups");
        GUILayout.Space(5);
        for (int i = 0; i < painter.prefabGroups.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            painter.prefabGroups[i].groupName = EditorGUILayout.TextField("Group Name", painter.prefabGroups[i].groupName);
            if (GUILayout.Button("Remove"))
            {
                painter.prefabGroups.RemoveAt(i);
                i--;
            }
            EditorGUILayout.EndHorizontal();
        }

        string[] groupNames = painter.prefabGroups.ConvertAll(g => g.groupName).ToArray();
        if (groupNames.Length > 0)
        {
            painter.selectedGroupIndex = EditorGUILayout.Popup("Select Paint Group", painter.selectedGroupIndex, groupNames);
        }

        if (GUILayout.Button("Add New Group"))
        {
            painter.prefabGroups.Add(new PrefabPainter.PrefabGroup { groupName = "New Group" });
        }

        GUILayout.Space(10);

        GUILayout.Label("Settings");

        painter.currentMode = (PrefabPainter.PaintMode)EditorGUILayout.EnumPopup("Painting Mode", painter.currentMode);
        GUILayout.Space(5);
        painter.brushSize = EditorGUILayout.Slider("Brush Size", painter.brushSize, 1f, 99f);
        painter.treesPerBrush = EditorGUILayout.IntSlider("Trees per Brush", painter.treesPerBrush, 1, 99);
        painter.treePlacementRadius = EditorGUILayout.Slider("Tree Placement Radius", painter.treePlacementRadius, 1f, 25f);

        // Display Y Offset Range with current value indicators
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(new GUIContent("Y Offset Range", "Adjust the vertical offset range for placements. (some shorter, some longer)"), GUILayout.Width(90));
        GUILayout.Label($"{painter.minYOffset:F2}", GUILayout.Width(40));  // Display the current minimum value
        EditorGUILayout.MinMaxSlider(ref painter.minYOffset, ref painter.maxYOffset, -1.0f, 0.1f);
        GUILayout.Label($"{painter.maxYOffset:F2}", GUILayout.Width(40));  // Display the current maximum value
        EditorGUILayout.EndHorizontal();

        painter.randomRotation = EditorGUILayout.Toggle("Random Rotation", painter.randomRotation);
        painter.continuousMode = EditorGUILayout.Toggle("Continuous Mode", painter.continuousMode);
        painter.avoidOverlap = EditorGUILayout.Toggle("Avoid Overlap", painter.avoidOverlap);

        // GUI for switchin modes
        GUILayout.Label("Mode:");
        painter.currentMode = GUILayout.Toolbar(painter.currentMode == PrefabPainter.PaintMode.Paint ? 0 : 1,
            new GUIContent[] {
            new GUIContent(" Paint", paintIcon, "Switch to Paint Mode"),
            new GUIContent(" Delete", deleteIcon, "Switch to Delete Mode")
            }, iconStyle, GUILayout.ExpandWidth(false)) == 0 ? PrefabPainter.PaintMode.Paint : PrefabPainter.PaintMode.Delete;


        if (GUI.changed)
        {
            placedPositions.Clear();
            EditorUtility.SetDirty(painter);
            SceneView.RepaintAll();
        }

        if (EditorGUI.EndChangeCheck()) // Check if any GUI element was modified
        {
            EditorUtility.SetDirty(painter); // Mark the PrefabPainter as dirty
            SavePrefabPainter(painter);
        }
    }

    private void SavePrefabPainter(PrefabPainter painter)
    {
        Undo.RecordObject(painter, "Modified PrefabPainter"); // Record changes for undo
        EditorUtility.SetDirty(painter); // Mark the painter as dirty to ensure it gets saved
    }

}
