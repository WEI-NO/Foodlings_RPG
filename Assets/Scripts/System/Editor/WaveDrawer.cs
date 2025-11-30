using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Wave))]
public class WaveDrawer : PropertyDrawer
{
    private bool showBurst;
    private bool showRepeater;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // Default drawer height calculation
        return EditorGUI.GetPropertyHeight(property, true) - 6f;
        // ^ reduce height slightly to remove unwanted gap
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // --- Reduce the gap before the box ---
        GUILayout.Space(-6f); // perfect sweet spot (adjust to taste: -4f to -8f)

        SerializedProperty startSeconds = property.FindPropertyRelative("startSeconds");
        SerializedProperty characterIndex = property.FindPropertyRelative("characterIndex");

        SerializedProperty single = property.FindPropertyRelative("single");
        SerializedProperty burst = property.FindPropertyRelative("burst");
        SerializedProperty repeater = property.FindPropertyRelative("repeater");

        SerializedProperty burstOptions = property.FindPropertyRelative("burstOptions");
        SerializedProperty repeaterOptions = property.FindPropertyRelative("repeaterOptions");

        // Draw box for readability
        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.LabelField("Wave Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(startSeconds);
        EditorGUILayout.PropertyField(characterIndex);

        EditorGUILayout.Space(4);

        EditorGUILayout.LabelField("Spawn Mode", EditorStyles.boldLabel);

        DrawModeToggle(single, burst, repeater, "Single");
        DrawModeToggle(burst, single, repeater, "Burst");
        DrawModeToggle(repeater, single, burst, "Repeater");

        EditorGUILayout.Space(4);

        if (single.boolValue)
        {
            EditorGUILayout.HelpBox("Spawns a single entity at the start time.", MessageType.Info);
        }

        if (burst.boolValue)
        {
            showBurst = EditorGUILayout.Foldout(showBurst, "Burst Settings");

            if (showBurst)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(burstOptions.FindPropertyRelative("amount"), new GUIContent("Burst Amount"));
                EditorGUILayout.PropertyField(burstOptions.FindPropertyRelative("delay"), new GUIContent("Burst Delay"));
                EditorGUI.indentLevel--;
            }
        }

        if (repeater.boolValue)
        {
            showRepeater = EditorGUILayout.Foldout(showRepeater, "Repeater Settings");

            if (showRepeater)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(repeaterOptions.FindPropertyRelative("spawnOnStart"));
                EditorGUILayout.PropertyField(repeaterOptions.FindPropertyRelative("interval"));
                EditorGUILayout.PropertyField(repeaterOptions.FindPropertyRelative("duration"));
                EditorGUILayout.PropertyField(repeaterOptions.FindPropertyRelative("useRandomInterval"));
                EditorGUILayout.PropertyField(repeaterOptions.FindPropertyRelative("randomInterval"));
                EditorGUI.indentLevel--;
            }
        }

        EditorGUILayout.EndVertical();
        EditorGUI.EndProperty();
    }

    private void DrawModeToggle(SerializedProperty current, SerializedProperty other1, SerializedProperty other2, string label)
    {
        bool newValue = EditorGUILayout.ToggleLeft(label, current.boolValue);

        if (newValue)
        {
            current.boolValue = true;
            other1.boolValue = false;
            other2.boolValue = false;
        }
        else
        {
            current.boolValue = false;
        }
    }
}
