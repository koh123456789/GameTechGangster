//using UnityEngine;
//using UnityEngine.UI;
//using TMPro;

//public class BT_API_Manager : MonoBehaviour
//{
//    [Header("Target NPC")]
//    public NPCController boss;

//    [Header("UI Input Fields - Boss Logic")]
//    public TMP_InputField retreatInput;
//    public TMP_InputField viewDistInput;
//    public TMP_InputField viewAngleInput;
//    public TMP_InputField moneyDistInput;

//    [Header("UI Input Fields - Boss Combat")]
//    public TMP_InputField closeDmgInput;
//    public TMP_InputField longDmgInput;
//    public TMP_InputField attackRateInput;
//    public TMP_InputField speedInput;

//    [Header("UI Input Fields - Minions")]
//    public TMP_InputField minionSpeedInput;
//    public TMP_InputField minionDmgInput;
//    public TMP_InputField minionFollowDistInput; // Added this back to the Minion group

//    [Header("Feedback UI")]
//    public TMP_Text statusLabel;
//    public TMP_Text errorLabel;

//    private void Start()
//    {
//        if (boss != null) RefreshUI();
//        if (errorLabel != null) errorLabel.text = "System Ready";
//    }

//    private void Update()
//    {
//        if (boss != null && statusLabel != null)
//        {
//            statusLabel.text = "Active BT Branch: " + boss.currentState + "\nAction: " + boss.currentAction;
//        }
//    }

//    public void RefreshUI()
//    {
//        if (boss == null) return;

//        // Sync Boss
//        retreatInput.text = boss.lowHealthThreshold.ToString();
//        viewDistInput.text = boss.viewDistance.ToString();
//        viewAngleInput.text = boss.viewAngle.ToString();
//        moneyDistInput.text = boss.moneyDetectionRadius.ToString();
//        closeDmgInput.text = boss.closeRangeDamage.ToString();
//        longDmgInput.text = boss.longRangeDamage.ToString();
//        attackRateInput.text = boss.attackCooldown.ToString();

//        SteeringAgent bossSteering = boss.GetComponent<SteeringAgent>();
//        if (bossSteering != null) speedInput.text = bossSteering.maxSpeed.ToString();

//        // Sync Minion (Find the first minion to show current baseline)
//        BossController[] allNPCs = FindObjectsOfType<BossController>();
//        foreach (var npc in allNPCs)
//        {
//            if (!npc.isBoss)
//            {
//                minionSpeedInput.text = npc.GetComponent<SteeringAgent>().maxSpeed.ToString();
//                minionDmgInput.text = npc.closeRangeDamage.ToString();
//                minionFollowDistInput.text = npc.followDistance.ToString(); // Sync this too
//                break;
//            }
//        }
//    }

//    public void ApplyAllSettings()
//    {
//        if (boss == null) return;
//        bool allValid = true;

//        // --- BOSS LOGIC (Limits matching BossController) ---
//        allValid &= ValidateAndSet(retreatInput.text, retreatInput, "Retreat HP", 0, boss.maxHealth, (val) => boss.lowHealthThreshold = val);
//        allValid &= ValidateAndSet(viewDistInput.text, viewDistInput, "View Dist", 1, 100, (val) => boss.viewDistance = val);
//        allValid &= ValidateAndSet(viewAngleInput.text, viewAngleInput, "View Angle", 10, 360, (val) => boss.viewAngle = val);
//        allValid &= ValidateAndSet(moneyDistInput.text, moneyDistInput, "Money Dist", 1, 50, (val) => boss.moneyDetectionRadius = val);

//        // --- BOSS COMBAT ---
//        allValid &= ValidateAndSet(closeDmgInput.text, closeDmgInput, "Close Dmg", 0, 200, (val) => boss.closeRangeDamage = val);
//        allValid &= ValidateAndSet(longDmgInput.text, longDmgInput, "Long Range", 1, 100, (val) => {
//            boss.phase2AttackRange = val;
//            if (boss.isUpgraded) boss.attackRange = val;
//        });
//        allValid &= ValidateAndSet(attackRateInput.text, attackRateInput, "Attack Rate", 0.1f, 10f, (val) => boss.attackCooldown = val);
//        allValid &= ValidateAndSet(speedInput.text, speedInput, "Speed", 0, 20, (val) => boss.GetComponent<SteeringAgent>().maxSpeed = val);

//        // --- MINION SWARM ---
//        allValid &= ValidateAndSet(minionSpeedInput.text, minionSpeedInput, "Minion Speed", 0, 20, (val) => UpdateAllMinions(val, -1, -1));
//        allValid &= ValidateAndSet(minionDmgInput.text, minionDmgInput, "Minion Dmg", 0, 100, (val) => UpdateAllMinions(-1, val, -1));

//        if (allValid) ShowSuccess("System Synchronized");
//    }

//    // Update this to return a bool (True = Success, False = Error)
//    private bool ValidateAndSet(string input, TMP_InputField field, string fieldName, float min, float max, System.Action<float> successAction)
//{
//    // 1. Silent check for empty/middle-of-typing states
//    if (string.IsNullOrWhiteSpace(input) || input == "-") return false;

//    if (float.TryParse(input, out float result))
//    {
//        // 2. THE SOFT CLAMP
//        float clampedValue = Mathf.Clamp(result, min, max);

//        // 3. UI SYNC: If the user typed something out of range, snap the UI text to the limit
//        // We check !field.isFocused so we don't move their cursor while they are typing
//        if (result != clampedValue && !field.isFocused)
//        {
//            field.text = clampedValue.ToString("F1"); 
//        }

//        // 4. Execution
//        successAction(clampedValue);
//        return true;
//    }

//    // 5. Format Error (Letters instead of numbers)
//    ShowError($"{fieldName} must be a number!");
//    return false;
//}

//    private void UpdateAllMinions(float newSpeed, float newDmg, float newFollow)
//    {
//        BossController[] allNPCs = FindObjectsOfType<BossController>();

//        // 1. Check if we actually found any NPCs at all
//        if (allNPCs.Length == 0) return;

//        foreach (var npc in allNPCs)
//        {
//            // 2. Ensure we only talk to Minions that aren't null
//            if (npc != null && !npc.isBoss)
//            {
//                // 3. Check for the SteeringAgent before applying speed
//                if (newSpeed != -1)
//                {
//                    SteeringAgent sa = npc.GetComponent<SteeringAgent>();
//                    if (sa != null) sa.maxSpeed = newSpeed;
//                }

//                // 4. Apply Damage directly (assuming it's a variable in BossController)
//                if (newDmg != -1) npc.closeRangeDamage = newDmg;

//                // 5. Apply Follow Distance
//                if (newFollow != -1) npc.followDistance = newFollow;
//            }
//        }
//    }

//    private void ShowError(string msg) { errorLabel.text = msg; errorLabel.color = Color.red; }
//    private void ShowSuccess(string msg) { errorLabel.text = msg; errorLabel.color = Color.green; }
//}