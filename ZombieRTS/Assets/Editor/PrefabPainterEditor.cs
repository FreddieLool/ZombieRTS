using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using static PrefabPainterWindow;
using static PrefabPainter;

[CustomEditor(typeof(PrefabPainter))]

public class PrefabPainterEditor : Editor
{
    private List<Vector3> placedPositions = new List<Vector3>();
    private GameObject currentParentGroup;
    private Texture2D headerLogo;
    private Texture2D paintIcon;
    private Texture2D deleteIcon;

    SerializedProperty prefabGroupsProperty;

    void OnEnable()
    {
        headerLogo = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/__Assets/UI Images/EDITORLOGO.png");
        paintIcon = ResizeTexture(AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/__Assets/UI Images/paintBrush.png"), 24, 24);
        deleteIcon = ResizeTexture(AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/__Assets/UI Images/removeBrush.png"), 24, 24);
        prefabGroupsProperty = serializedObject.FindProperty("prefabGroups");
    }



    //
    //
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
            else if (painter.currentMode == PrefabPainter.PaintMode.Erase)
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
            Vector3 mousePosition = Event.current.mousePosition;
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, painter.groundLayer))
            {
                // Determine the slope based on the angle between the normal and the up vector
                float angle = Vector3.Angle(Vector3.up, hit.normal);
                bool isSloped = angle > 20f; // Consider it sloped if the angle is greater than a small threshold

                // Change color based on mode and slope
                if (painter.currentMode == PrefabPainter.PaintMode.Erase)
                {
                    Handles.color = Color.red; // Use red color when in Erase mode
                }
                else
                {
                    Handles.color = isSloped ? Color.green : Color.yellow; // Use green for sloped terrain in Paint mode, yellow otherwise
                }

                // Set line thickness
                Handles.DrawWireDisc(hit.point, hit.normal, painter.brushSize, isSloped ? 3f : 2f); // Thicker line when on slope

                // Optional: Draw a line from the hit point along the normal for better visual understanding
                Handles.DrawLine(hit.point, hit.point + hit.normal * 2); // Extend the normal line 2 units upwards
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
                if (col.gameObject.CompareTag("PaintedPrefab")) // Ensure only objects with the "PaintedPrefab" tag are deleted
                {
                    Undo.DestroyObjectImmediate(col.gameObject); // Use Undo to allow revert
                }
            }
        }
    }


    private void PaintPrefabs(Event e, PrefabPainter painter)
    {
        if (painter.selectedGroupIndex < 0 || painter.selectedGroupIndex >= painter.PrefabGroups.Count)
            return;

        if (!currentParentGroup)
            CreateNewParentGroup(painter);

        List<GameObject> prefabsToPaint = painter.PrefabGroups[painter.selectedGroupIndex].prefabs;
        if (prefabsToPaint.Count == 0)
            return;

        Vector3 startPoint = RandomPointInCircle(e.mousePosition, painter.brushSize);
        Ray startRay = HandleUtility.GUIPointToWorldRay(startPoint);
        if (!Physics.Raycast(startRay, out RaycastHit startHit, Mathf.Infinity, painter.groundLayer))
            return;

        for (int x = 0; x < painter.treesPerBrush; x++)
        {
            for (int y = 0; y < painter.treesPerBrush; y++)
            {
                Vector3 gridPoint = new Vector3(startPoint.x + x * painter.patternSpacing, startPoint.y, startPoint.z + y * painter.patternSpacing);
                Ray ray = HandleUtility.GUIPointToWorldRay(gridPoint);
                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, painter.groundLayer))
                {
                    Vector3 position = hit.point + Vector3.up * Random.Range(painter.minYOffset, painter.maxYOffset);
                    Quaternion rotation = GetRandomOrAlignedRotation(painter, hit.normal);
                    Vector3 scale = GetRandomScale(painter);

                    if (IsPositionValid(position, hit.normal, painter))
                    {
                        GameObject prefab = prefabsToPaint[Random.Range(0, prefabsToPaint.Count)];
                        GameObject newObject = Instantiate(prefab, position, rotation);
                        newObject.transform.localScale = scale;
                        newObject.transform.SetParent(currentParentGroup.transform);
                        Undo.RegisterCreatedObjectUndo(newObject, "Place Prefab");
                        placedPositions.Add(position);
                    }
                }
            }
        }
    }



    private void CreateNewParentGroup(PrefabPainter painter)
    {
        currentParentGroup = new GameObject("PaintedPrefabs_Group");
        currentParentGroup.transform.SetParent(painter.transform);
    }


    private Quaternion GetRandomOrAlignedRotation(PrefabPainter painter, Vector3 normal)
    {
        Quaternion rotation = Quaternion.identity;
        if (painter.randomRotation)
            rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

        if (painter.alignToTerrain)
        {
            float angleOffset = Random.Range(painter.minAlignAngle, painter.maxAlignAngle);
            Quaternion terrainAlignment = Quaternion.FromToRotation(Vector3.up, normal);
            rotation = Quaternion.Slerp(Quaternion.identity, terrainAlignment, angleOffset / 90.0f) * rotation;
        }

        return rotation;
    }


    private Vector3 GetRandomScale(PrefabPainter painter)
    {
        if (!painter.enableRandomScale)
            return Vector3.one;

        float randomScale = Random.Range(painter.scaleRange.x, painter.scaleRange.y);
        return new Vector3(randomScale, randomScale, randomScale);
    }


    private bool IsPositionValid(Vector3 position, Vector3 normal, PrefabPainter painter)
    {
        if (Vector3.Angle(Vector3.up, normal) > painter.maxSlopeAngle)
            return false;

        foreach (Vector3 placedPosition in placedPositions)
        {
            if (Vector3.Distance(position, placedPosition) < painter.treePlacementRadius)
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
        PrefabPainter painter = (PrefabPainter)target;
        serializedObject.Update();  // Load the real values into the serialized object

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

        // If there are any other properties that get modified via the Inspector, ensure they trigger a save too.
        EditorGUI.BeginChangeCheck();

        // GUI for switching modes with compact buttons and scaled icons
        EditorGUILayout.LabelField("Painting Mode", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal(); // Start a horizontal group for the buttons

        // Style for the buttons to control icon size and alignment
        GUIStyle btnStyle = new GUIStyle(GUI.skin.button);
        btnStyle.imagePosition = ImagePosition.ImageAbove;
        btnStyle.fixedHeight = 50;
        btnStyle.fixedWidth = 80;

        // Paint Mode Button
        if (GUILayout.Button(new GUIContent(" Paint", paintIcon, "Switch to Paint Mode"), btnStyle))
        {
            painter.currentMode = PrefabPainter.PaintMode.Paint;
        }

        // Erase Mode Button
        if (GUILayout.Button(new GUIContent(" Erase", deleteIcon, "Switch to Erase Mode"), btnStyle))
        {
            painter.currentMode = PrefabPainter.PaintMode.Erase;
        }

        EditorGUILayout.EndHorizontal(); // End the horizontal group

        // Pattern Painting Stuff
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Pattern Settings", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Pattern Mode: Select a layout style—Grid, Radial, or Scattered—for organizing prefabs. Grid Spacing: Adjusts the distance between prefabs in Grid mode, perfect for systematic designs like orchards or urban layouts.", EditorStyles.helpBox);

        painter.patternMode = (PatternType)EditorGUILayout.EnumPopup("Pattern Mode", painter.patternMode);
        if (painter.patternMode == PatternType.Grid)
        {
            painter.patternSpacing = EditorGUILayout.FloatField("Grid Spacing", painter.patternSpacing);
        }

        EditorGUILayout.Space();

        string[] groupNames = painter.PrefabGroups.ConvertAll(g => g.groupName).ToArray();
        if (groupNames.Length > 0)
        {
            painter.selectedGroupIndex = EditorGUILayout.Popup("Select Paint Group", painter.selectedGroupIndex, groupNames);
        }
        EditorGUILayout.Space();

        // Custom UI for Prefab Groups
        EditorGUILayout.LabelField("Prefab Groups", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Manage your prefab collections. Groups list all available prefab groups for quick access and management.", EditorStyles.helpBox);

        if (prefabGroupsProperty != null)
        {
            for (int i = 0; i < prefabGroupsProperty.arraySize; i++)
            {
                SerializedProperty groupProperty = prefabGroupsProperty.GetArrayElementAtIndex(i);
                SerializedProperty groupNameProperty = groupProperty.FindPropertyRelative("groupName");
                SerializedProperty prefabsProperty = groupProperty.FindPropertyRelative("prefabs");
                SerializedProperty isExpandedProperty = groupProperty.FindPropertyRelative("isExpanded");

                bool isExpanded = EditorGUILayout.Foldout(isExpandedProperty.boolValue, groupNameProperty.stringValue);
                isExpandedProperty.boolValue = isExpanded;

                if (isExpanded)
                {
                    groupNameProperty.stringValue = EditorGUILayout.TextField("Group Name", groupNameProperty.stringValue);
                    EditorGUILayout.PropertyField(prefabsProperty, new GUIContent("Prefabs"), true);
                    if (GUILayout.Button("Remove Group"))
                    {
                        prefabGroupsProperty.DeleteArrayElementAtIndex(i);
                    }
                }
            }
        }

        if (GUILayout.Button("Add New Group"))
        {
            prefabGroupsProperty.arraySize++;
            SerializedProperty newGroupProperty = prefabGroupsProperty.GetArrayElementAtIndex(prefabGroupsProperty.arraySize - 1);
            newGroupProperty.FindPropertyRelative("groupName").stringValue = "New Group";
        }

        serializedObject.ApplyModifiedProperties(); // Apply changes to the serialized object



        if (GUI.changed)
        {
            EditorUtility.SetDirty(painter);
            SceneView.RepaintAll();
        }

        GUILayout.Space(10);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Paint Grouping", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Manage how painted objects are grouped in the scene hierarchy. Click to add a new group parent for the next paint.", EditorStyles.helpBox);
        EditorGUILayout.Space();
        if (GUILayout.Button("Create New Parent Group"))
        {
            CreateNewParentGroup(painter);
        }
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Brush Settings", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Configure the brush properties: Brush Size determines the radius for painting or deleting; Trees per Brush sets the number of prefabs placed per stroke; Tree Placement Radius specifies the spacing between prefabs to avoid overlaps; Y Offset Range adjusts the vertical position of prefabs relative to the surface level, allowing for variations in height.", EditorStyles.helpBox);
        EditorGUILayout.Space();


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

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Placement Settings", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Adjust how prefabs are placed: Random Rotation randomizes the prefab orientation; Continuous Mode allows continuous placement as you drag the mouse; Avoid Overlap ensures prefabs do not intersect with each other (needs fixing); Align to Terrain conforms prefab orientation to the terrain slope, with additional customization available through slope and alignment settings below.", EditorStyles.helpBox);
        EditorGUILayout.Space();

        painter.randomRotation = EditorGUILayout.Toggle("Random Rotation", painter.randomRotation);
        painter.continuousMode = EditorGUILayout.Toggle("Continuous Mode", painter.continuousMode);
        painter.avoidOverlap = EditorGUILayout.Toggle("Avoid Overlap", painter.avoidOverlap);
        // Toggle for aligning prefabs to terrain slope
        painter.alignToTerrain = EditorGUILayout.Toggle("Align to Terrain", painter.alignToTerrain);
        // Slope and alignment controls

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Slope and Alignment Settings", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Fine-tune placement on sloped terrains: Max Slope Angle restricts prefab placement to slopes below this angle; Min Alignment Angle and Max Alignment Angle control the range of angles prefabs can rotate to align with the terrain's slope, providing a natural variance in orientation.", EditorStyles.helpBox);
        EditorGUILayout.Space();

        painter.maxSlopeAngle = EditorGUILayout.Slider("Max Slope Angle", painter.maxSlopeAngle, 0f, 90f);
        painter.minAlignAngle = EditorGUILayout.Slider("Min Alignment Angle", painter.minAlignAngle, 0f, 90f);
        painter.maxAlignAngle = EditorGUILayout.Slider("Max Alignment Angle", painter.maxAlignAngle, 0f, 360f);


        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Scale Settings", EditorStyles.boldLabel);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Adjust the random scaling of prefabs during placement. Random Scale enables variability in size, with minimum and maximum scale values defining the range of potential sizes for more diverse and dynamic scenes.", EditorStyles.helpBox);
        EditorGUILayout.Space();
        // Toggle for random scaling
        painter.enableRandomScale = EditorGUILayout.Toggle("Enable Random Scale", painter.enableRandomScale);

        // Scale range sliders
        if (painter.enableRandomScale)
        {
            EditorGUILayout.LabelField("Scale Range");
            painter.scaleRange.x = EditorGUILayout.FloatField("Min Scale", painter.scaleRange.x);
            painter.scaleRange.y = EditorGUILayout.FloatField("Max Scale", painter.scaleRange.y);
        }

        // Ensure the minimum scale is never greater than the maximum scale
        painter.scaleRange.x = Mathf.Min(painter.scaleRange.x, painter.scaleRange.y);
        painter.scaleRange.y = Mathf.Max(painter.scaleRange.x, painter.scaleRange.y);


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

    Texture2D ResizeTexture(Texture2D source, int newWidth, int newHeight)
    {
        source.filterMode = FilterMode.Bilinear;
        RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
        rt.filterMode = FilterMode.Bilinear;
        RenderTexture.active = rt;
        Graphics.Blit(source, rt);
        Texture2D nTex = new Texture2D(newWidth, newHeight);
        nTex.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
        nTex.Apply();
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);
        return nTex;
    }
}
