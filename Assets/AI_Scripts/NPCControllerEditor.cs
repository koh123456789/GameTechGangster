//using UnityEngine;
//using UnityEditor;
//using UnityEditorInternal; // Needed for the LayerMask helper

//[CustomEditor(typeof(NPCController))]
//[CanEditMultipleObjects]
//public class NPCControllerEditor : Editor
//{
//    public override void OnInspectorGUI()
//    {
//        NPCController script = (NPCController)target;
//        serializedObject.Update();

//        // 1. Core Toggle
//        script.isBoss = EditorGUILayout.ToggleLeft(" Is this NPC a Boss?", script.isBoss, EditorStyles.boldLabel);
//        EditorGUILayout.Space(10);

//        // --- SURVIVAL SECTION ---
//        EditorGUILayout.BeginVertical("helpbox"); // "helpbox" gives a nice subtle border
//        EditorGUILayout.LabelField("SURVIVAL", EditorStyles.whiteLargeLabel);
//        script.NpcHealth = EditorGUILayout.FloatField("Health", script.NpcHealth);

//        if (script.isBoss)
//        {
//            script.lowHealthThreshold = EditorGUILayout.FloatField("Low Health Threshold", script.lowHealthThreshold);
//            script.safeArea = (Transform)EditorGUILayout.ObjectField("Safe Area", script.safeArea, typeof(Transform), true);
//        }
//        EditorGUILayout.EndVertical();

//        // --- MONEY SECTION (Only for Boss) ---
//        if (script.isBoss)
//        {
//            EditorGUILayout.Space(10);
//            EditorGUILayout.BeginVertical("helpbox");
//            EditorGUILayout.LabelField("ECONOMY", EditorStyles.whiteLargeLabel);
//            script.NpcCash = EditorGUILayout.FloatField("Current Cash", script.NpcCash);
//            script.cashTarget = EditorGUILayout.FloatField("Cash Target", script.cashTarget);

//            // Simplified LayerMask field
//            SerializedProperty layerProp = serializedObject.FindProperty("moneyLayer");
//            EditorGUILayout.PropertyField(layerProp);
//            EditorGUILayout.EndVertical();
//        }

//        // --- COMBAT SECTION ---
//        EditorGUILayout.Space(10);
//        EditorGUILayout.BeginVertical("helpbox");
//        EditorGUILayout.LabelField("COMBAT", EditorStyles.whiteLargeLabel);
//        script.meleeWeaponRange = EditorGUILayout.FloatField("Melee Range", script.meleeWeaponRange);
//        script.rangedWeaponRange = EditorGUILayout.FloatField("Ranged Range", script.rangedWeaponRange);

//        script.meleeWeapon = (GameObject)EditorGUILayout.ObjectField("Melee Model", script.meleeWeapon, typeof(GameObject), true);
//        script.rangedWeapon = (GameObject)EditorGUILayout.ObjectField("Ranged Model", script.rangedWeapon, typeof(GameObject), true);

//        if (script.isBoss)
//        {
//            EditorGUILayout.Space(5);
//            EditorGUILayout.LabelField("Boss Special Abilities", EditorStyles.miniBoldLabel);
//            script.projectilePrefab = (GameObject)EditorGUILayout.ObjectField("Projectile", script.projectilePrefab, typeof(GameObject), true);
//            script.firePoint = (Transform)EditorGUILayout.ObjectField("Fire Point", script.firePoint, typeof(Transform), true);

//            SerializedProperty spawnPointsProp = serializedObject.FindProperty("spawnPoints");
//            EditorGUILayout.PropertyField(spawnPointsProp, true);
//        }
//        EditorGUILayout.EndVertical();

//        // --- DEBUG INFO ---
//        EditorGUILayout.Space(10);
//        GUI.enabled = false; // Makes these fields read-only
//        EditorGUILayout.TextField("Active State", script.currentState);
//        EditorGUILayout.EnumPopup("Active Action", script.currentAction);
//        GUI.enabled = true;

//        if (GUI.changed)
//        {
//            EditorUtility.SetDirty(script);
//        }
//        serializedObject.ApplyModifiedProperties();
//    }
//}