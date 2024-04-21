using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Enemy))]
public class EnemyEditor : Editor
{
    public void OnSceneGUI()
    {
        Enemy enemy = target as Enemy;
        if (enemy == null) return;

        switch (enemy.mode)
        {
            case EnemyMode.BoxPatrol:
                DrawBoxPatrol(enemy);
                break;
            case EnemyMode.SphereRoam:
                DrawSphereRoam(enemy);
                break;
        }
    }

    private void DrawBoxPatrol(Enemy enemy)
    {
        Handles.color = Color.blue;
        Vector3 size = new Vector3(enemy.boxSize.x, 1, enemy.boxSize.z);
        Vector3 center = enemy.transform.position + Vector3.up * 0.5f; // Adjust based on enemy's height
        Handles.DrawWireCube(center, size);
    }

    private void DrawSphereRoam(Enemy enemy)
    {
        Handles.color = Color.green;
        Handles.DrawWireDisc(enemy.transform.position, Vector3.up, enemy.roamRadius);
    }
}
