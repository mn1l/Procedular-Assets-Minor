using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RecursiveBacktrackingMaze))]
public class RecursiveBacktrackingMazeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Update serialized object for custom field handling
        serializedObject.Update();

        // Draw "Maze Settings" fields
        EditorGUILayout.LabelField("Maze Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("width"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("height"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("seed"));

        // Add buttons between Maze Settings and Tilemap settings
        EditorGUILayout.Space(); // Add some spacing for better visual separation
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Generate Maze"))
        {
            ((RecursiveBacktrackingMaze)target).GenerateMaze();
        }
        if (GUILayout.Button("Clear Maze"))
        {
            ((RecursiveBacktrackingMaze)target).ClearMaze();
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(); // Add spacing below the buttons

        // Draw "Tilemap and Tiles" fields
        EditorGUILayout.LabelField("Tilemap and Tiles", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("tilemap"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("pathTile"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("wallTile"));

        // Apply changes to serialized properties
        serializedObject.ApplyModifiedProperties();
    }
}