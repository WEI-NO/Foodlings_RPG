//using UnityEditor;
//using UnityEngine;
//using System;
//using System.Linq;

//[CustomPropertyDrawer(typeof(Wave))]
//public class WaveDrawer : PropertyDrawer
//{
//    Type[] spawnModeTypes;

//    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//    {
//        Debug.Log("WaveDrawer OnGUI called");

//        EditorGUI.BeginProperty(position, label, property);

//        position.height = EditorGUIUtility.singleLineHeight;

//        var startTimeProp = property.FindPropertyRelative("startTime");
//        var characterIndexProp = property.FindPropertyRelative("characterIndex");
//        var spawnModeProp = property.FindPropertyRelative("spawnMode");

//        EditorGUI.PropertyField(position, startTimeProp);
//        position.y += position.height + 2;

//        EditorGUI.PropertyField(position, characterIndexProp);
//        position.y += position.height + 4;

//        EnsureSpawnModeTypes();

//        string[] options = spawnModeTypes.Select(t => t.Name).ToArray();

//        int currentIndex = -1;
//        if (spawnModeProp.managedReferenceValue != null)
//        {
//            Type currentType = spawnModeProp.managedReferenceValue.GetType();
//            currentIndex = Array.IndexOf(spawnModeTypes, currentType);
//        }

//        int selected = EditorGUI.Popup(position, "Spawn Mode", currentIndex, options);

//        if (selected != currentIndex && selected >= 0)
//        {
//            spawnModeProp.managedReferenceValue =
//                Activator.CreateInstance(spawnModeTypes[selected]);
//        }

//        position.y += position.height + 4;

//        if (spawnModeProp.managedReferenceValue != null)
//        {
//            EditorGUI.indentLevel++;
//            float spawnModeHeight = EditorGUI.GetPropertyHeight(spawnModeProp, true);
//            Rect spawnRect = new Rect(position.x, position.y, position.width, spawnModeHeight);
//            EditorGUI.PropertyField(spawnRect, spawnModeProp, true);
//            EditorGUI.indentLevel--;
//        }

//        EditorGUI.EndProperty();
//    }

//    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
//    {
//        float height = 0f;
//        height += EditorGUIUtility.singleLineHeight * 3;

//        var spawnModeProp = property.FindPropertyRelative("spawnMode");
//        if (spawnModeProp.managedReferenceValue != null)
//        {
//            height += EditorGUI.GetPropertyHeight(spawnModeProp, true) + 4;
//        }

//        return height;
//    }

//    void EnsureSpawnModeTypes()
//    {
//        if (spawnModeTypes != null)
//            return;

//        spawnModeTypes = TypeCache
//            .GetTypesDerivedFrom<SpawnMode>()
//            .Where(t => !t.IsAbstract)
//            .ToArray();
//    }
//}
