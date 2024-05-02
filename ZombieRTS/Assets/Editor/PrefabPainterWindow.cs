using UnityEditor;
using UnityEngine;

public class PrefabPainterWindow : EditorWindow
{
    private int selectedTabIndex = 0;
    private string[] tabTitles = new string[] { "Tab 1", "Tab 2", "Tab 3" };

    [MenuItem("Window/Prefab Painter")]
    public static void ShowWindow()
    {
        GetWindow<PrefabPainterWindow>("Prefab Painter");
    }

    void OnGUI()
    {
        // Tab bar
        selectedTabIndex = GUILayout.Toolbar(selectedTabIndex, tabTitles);

        switch (selectedTabIndex)
        {
            case 0:
                DrawTab1();
                break;
            case 1:
                DrawTab2();
                break;
            case 2:
                DrawTab3();
                break;
        }
    }

    void DrawTab1()
    {
        GUILayout.Label("This is Tab 1", EditorStyles.boldLabel);
        // Additional GUI elements for Tab 1
    }

    void DrawTab2()
    {
        GUILayout.Label("This is Tab 2", EditorStyles.boldLabel);
        // Additional GUI elements for Tab 2
    }

    void DrawTab3()
    {
        GUILayout.Label("This is Tab 3", EditorStyles.boldLabel);
        // Additional GUI elements for Tab 3
    }
}
