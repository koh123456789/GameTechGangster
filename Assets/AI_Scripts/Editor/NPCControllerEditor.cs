using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NPCController))]
[CanEditMultipleObjects]
public class NPCControllerEditor : Editor
{
    // --- OPTIMIZATION: Variables must be here, outside the function ---
    private bool showDebug = false;
    private SerializedObject serializedAgent;

    public override void OnInspectorGUI()
    {
        NPCController script = (NPCController)target;
        serializedObject.Update();

        EditorGUILayout.Space(5);
        script.isBoss = EditorGUILayout.ToggleLeft(" Is this NPC the boss", script.isBoss, EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        // --- 1. SURVIVAL BRANCH ---
        EditorGUILayout.LabelField("--- 1. SURVIVAL BRANCH ---", EditorStyles.boldLabel);
        script.NpcHealth = EditorGUILayout.FloatField("Npc Health", script.NpcHealth);

        if (script.isBoss)
        {
            script.lowHealthThreshold = EditorGUILayout.FloatField("Low Health Threshold", script.lowHealthThreshold);
            script.safeArea = (Transform)EditorGUILayout.ObjectField("Safe Area", script.safeArea, typeof(Transform), true);
        }
        EditorGUILayout.Space(10);

        // --- 2. MONEY BRANCH ---
        EditorGUILayout.LabelField("--- 2. MONEY BRANCH ---", EditorStyles.boldLabel);
        script.NpcCash = EditorGUILayout.FloatField("Npc Cash", script.NpcCash);
        script.cashTarget = EditorGUILayout.FloatField("Cash Target", script.cashTarget);

        GUI.enabled = false;
        script.moneyVisible = EditorGUILayout.Toggle("Money Visible", script.moneyVisible);
        GUI.enabled = true;

        SerializedProperty moneyLayerProp = serializedObject.FindProperty("moneyLayer");
        EditorGUILayout.PropertyField(moneyLayerProp);
        EditorGUILayout.Space(10);

        // --- 3. COMBAT BRANCH ---
        EditorGUILayout.LabelField("--- 3. COMBAT BRANCH ---", EditorStyles.boldLabel);
        GUI.enabled = false;
        script.playerVisible = EditorGUILayout.Toggle("Player Visible", script.playerVisible);
        GUI.enabled = true;

        script.damageReceived = EditorGUILayout.Toggle("Damage Received", script.damageReceived);
        script.meleeWeaponRange = EditorGUILayout.FloatField("Melee Weapon Range", script.meleeWeaponRange);

        if (script.isBoss)
        {
            script.rangedWeaponRange = EditorGUILayout.FloatField("Ranged Weapon Range", script.rangedWeaponRange);
            script.attackCooldown = EditorGUILayout.FloatField("Attack Cooldown", script.attackCooldown);
            script.muzzleFlash = (ParticleSystem)EditorGUILayout.ObjectField("Muzzle Flash", script.muzzleFlash, typeof(ParticleSystem), true);

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Weapons & Projectiles", EditorStyles.miniBoldLabel);
            script.meleeWeapon = (GameObject)EditorGUILayout.ObjectField("Melee Weapon", script.meleeWeapon, typeof(GameObject), true);
            script.rangedWeapon = (GameObject)EditorGUILayout.ObjectField("Ranged Weapon", script.rangedWeapon, typeof(GameObject), true);
            script.projectilePrefab = (GameObject)EditorGUILayout.ObjectField("Projectile Prefab", script.projectilePrefab, typeof(GameObject), true);
            script.firePoint = (Transform)EditorGUILayout.ObjectField("Fire Point", script.firePoint, typeof(Transform), true);

            script.meleeWeaponDamage = EditorGUILayout.FloatField("Melee Weapon Damage", script.meleeWeaponDamage);
            script.rangedWeaponDamage = EditorGUILayout.FloatField("Ranged Weapon Damage", script.rangedWeaponDamage);
        }
        else
        {
            script.meleeWeapon = (GameObject)EditorGUILayout.ObjectField("Melee Weapon", script.meleeWeapon, typeof(GameObject), true);
            script.meleeWeaponDamage = EditorGUILayout.FloatField("Melee Damage", script.meleeWeaponDamage);
        }
        EditorGUILayout.Space(10);

        // --- 4. SENSING SETTINGS ---
        EditorGUILayout.LabelField("--- SENSING SETTINGS ---", EditorStyles.boldLabel);
        script.viewDistance = EditorGUILayout.FloatField("View Distance", script.viewDistance);
        script.viewAngle = EditorGUILayout.FloatField("View Angle", script.viewAngle);

        SerializedProperty obstructionProp = serializedObject.FindProperty("obstructionMask");
        EditorGUILayout.PropertyField(obstructionProp);
        script.searchDuration = EditorGUILayout.FloatField("Search Duration", script.searchDuration);
        EditorGUILayout.Space(10);

        // --- 5. MINIONS SETTING ---
        if (script.isBoss)
        {
            EditorGUILayout.LabelField("--- MINIONS SETTING ---", EditorStyles.boldLabel);
            script.followDistance = EditorGUILayout.FloatField("Follow Distance", script.followDistance);
            script.bossTransform = (Transform)EditorGUILayout.ObjectField("Boss Transform", script.bossTransform, typeof(Transform), true);
            script.minionPrefab = (GameObject)EditorGUILayout.ObjectField("Minion Prefab", script.minionPrefab, typeof(GameObject), true);

            SerializedProperty spawnPointsProp = serializedObject.FindProperty("spawnPoints");
            EditorGUILayout.PropertyField(spawnPointsProp, true);
        }
        EditorGUILayout.Space(10);

        // --- 6. PATROL & MOVEMENT (OPTIMIZED) ---
        EditorGUILayout.LabelField("--- 6. PATROL & MOVEMENT ---", EditorStyles.boldLabel);
        SteeringAgent agent = script.GetComponent<SteeringAgent>();
        if (agent != null)
        {
            if (serializedAgent == null || serializedAgent.targetObject != agent)
                serializedAgent = new SerializedObject(agent);

            serializedAgent.Update();
            agent.maxSpeed = EditorGUILayout.FloatField("Movement Speed", agent.maxSpeed);
            agent.wanderRadius = EditorGUILayout.FloatField("Wander Radius", agent.wanderRadius);
            agent.minWaitTime = EditorGUILayout.FloatField("Min Idle Time", agent.minWaitTime);
            agent.maxWaitTime = EditorGUILayout.FloatField("Max Idle Time", agent.maxWaitTime);
            serializedAgent.ApplyModifiedProperties();
        }
        else
        {
            EditorGUILayout.HelpBox("SteeringAgent missing!", MessageType.Warning);
        }

        EditorGUILayout.Space(10);

        // --- 7. INTERNAL STATES (DEBUG WITH FOLDOUT) ---
        showDebug = EditorGUILayout.BeginFoldoutHeaderGroup(showDebug, "--- 7. INTERNAL STATES (DEBUG) ---");

        if (showDebug)
        {
            if (GUILayout.Button("FULL RESET TO PHASE 1"))
            {
                script.ResetToPhase1();
                EditorUtility.SetDirty(script);
            }

            EditorGUILayout.Space(5);

            if (GUILayout.Button("Simulate Player Hit (Trigger Search)"))
            {
                script.TakeDamage(0f);
            }

            EditorGUILayout.Space(5);

            GUI.enabled = false;
            EditorGUILayout.Toggle("Damage Received", script.damageReceived);
            EditorGUILayout.Toggle("Player Visible", script.playerVisible);
            EditorGUILayout.Toggle("Money Visible", script.moneyVisible);

            if (script.isBoss)
            {
                EditorGUILayout.Toggle("Is Upgraded", script.isUpgraded);
                EditorGUILayout.Toggle("Been To Safe Area", script.beenToSafeArea);
                EditorGUILayout.Toggle("Has Called Backup", script.hasCalledBackup);
            }

            EditorGUILayout.Vector3Field("Internal: Last Known Pos", script.lastKnownPlayerPos);
            EditorGUILayout.FloatField("Internal: Player Distance", script.playerDistance);
            EditorGUILayout.Toggle("Internal: Was Retreating", script.wasRetreating);
            GUI.enabled = true;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(script);
        }
    }

    // --- OPTIMIZATION: Prevents laggy repaints every frame ---
    public override bool RequiresConstantRepaint()
    {
        return showDebug && Application.isPlaying;
    }
}