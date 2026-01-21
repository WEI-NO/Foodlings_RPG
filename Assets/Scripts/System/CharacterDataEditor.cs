using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CharacterData))]
public class CharacterDataEditor : Editor
{
    SerializedProperty debug_ShowMaxLevelStatsProp;

    private void OnEnable()
    {
        debug_ShowMaxLevelStatsProp = serializedObject.FindProperty("debug_ShowMaxLevelStats");
    }

    public override void OnInspectorGUI()
    {
        // Draw the normal inspector (including the debug toggle)
        serializedObject.Update();
        DrawDefaultInspector();

        var data = (CharacterData)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Debug Tools", EditorStyles.boldLabel);

        // Toggle: Show Max Level Stats
        EditorGUILayout.PropertyField(debug_ShowMaxLevelStatsProp, new GUIContent("Show Max Level Stats"));

        // If toggled on, compute and display max-levelStone stats
        if (data.debug_ShowMaxLevelStats)
        {
            // Recalculate whenever inspector is drawn
#if UNITY_EDITOR
            data.UpdateDebugStats();
            EditorUtility.SetDirty(data);
#endif

            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Max Level Stats (Level {data.maxLevel})", EditorStyles.boldLabel);

            using (new EditorGUI.DisabledScope(true)) // make read-only
            {
                EditorGUILayout.FloatField("HP", data.Debug_HP);
                EditorGUILayout.FloatField("PAtk", data.Debug_PAtk);
                EditorGUILayout.FloatField("MAtk", data.Debug_MAtk);
                EditorGUILayout.FloatField("PDef", data.Debug_PDef);
                EditorGUILayout.FloatField("MDef", data.Debug_MDef);
                EditorGUILayout.FloatField("Atk Range", data.Debug_AtkRng);
                EditorGUILayout.FloatField("Atk Speed", data.Debug_AtkSpe);
                EditorGUILayout.FloatField("Move Speed", data.Debug_Spe);
                EditorGUILayout.FloatField("Cooldown", data.Debug_CD);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
