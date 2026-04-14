using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BT_API_Manager : MonoBehaviour
{
    [Header("Target NPC")]
    public BossController boss;

    [Header("UI Input Fields - Boss Logic")]
    public TMP_InputField retreatInput;
    public TMP_InputField viewDistInput;
    public TMP_InputField viewAngleInput;
    public TMP_InputField moneyDistInput;

    [Header("UI Input Fields - Boss Combat")]
    public TMP_InputField closeDmgInput;
    public TMP_InputField longDmgInput;
    public TMP_InputField attackRateInput;
    public TMP_InputField speedInput;

    [Header("UI Input Fields - Minions")]
    public TMP_InputField minionSpeedInput;
    public TMP_InputField minionDmgInput;
    public TMP_InputField minionFollowDistInput; // Added this back to the Minion group

    [Header("Feedback UI")]
    public TMP_Text statusLabel;
    public TMP_Text errorLabel;

    private void Start()
    {
        if (boss != null) RefreshUI();
        if (errorLabel != null) errorLabel.text = "System Ready";
    }

    private void Update()
    {
        if (boss != null && statusLabel != null)
        {
            statusLabel.text = "Active BT Branch: " + boss.currentState + "\nAction: " + boss.currentAction;
        }
    }

    public void RefreshUI()
    {
        if (boss == null) return;

        // Sync Boss
        retreatInput.text = boss.lowHealthThreshold.ToString();
        viewDistInput.text = boss.viewDistance.ToString();
        viewAngleInput.text = boss.viewAngle.ToString();
        moneyDistInput.text = boss.moneyDetectionRadius.ToString();
        closeDmgInput.text = boss.closeRangeDamage.ToString();
        longDmgInput.text = boss.longRangeDamage.ToString();
        attackRateInput.text = boss.attackCooldown.ToString();

        SteeringAgent bossSteering = boss.GetComponent<SteeringAgent>();
        if (bossSteering != null) speedInput.text = bossSteering.maxSpeed.ToString();

        // Sync Minion (Find the first minion to show current baseline)
        BossController[] allNPCs = FindObjectsOfType<BossController>();
        foreach (var npc in allNPCs)
        {
            if (!npc.isBoss)
            {
                minionSpeedInput.text = npc.GetComponent<SteeringAgent>().maxSpeed.ToString();
                minionDmgInput.text = npc.closeRangeDamage.ToString();
                minionFollowDistInput.text = npc.followDistance.ToString(); // Sync this too
                break;
            }
        }
    }

    public void ApplyAllSettings()
    {
        if (boss == null)
        {
            ShowError("Error: Target Boss no longer exists!");
            return;
        }

        // 1. Boss Logic
        ValidateAndSet(retreatInput.text, "Retreat", 0, 100, (val) => boss.lowHealthThreshold = val);
        ValidateAndSet(viewDistInput.text, "View Dist", 1, 100, (val) => boss.viewDistance = val);
        ValidateAndSet(viewAngleInput.text, "View Angle", 10, 360, (val) => boss.viewAngle = val);
        ValidateAndSet(moneyDistInput.text, "Money Dist", 1, 50, (val) => boss.moneyDetectionRadius = val);

        // 2. Boss Combat
        ValidateAndSet(closeDmgInput.text, "Close Dmg", 0, 100, (val) => boss.closeRangeDamage = val);
        ValidateAndSet(longDmgInput.text, "Long Dmg", 0, 100, (val) => boss.longRangeDamage = val);
        ValidateAndSet(attackRateInput.text, "Attack Rate", 0.1f, 10f, (val) => boss.attackCooldown = val);
        ValidateAndSet(speedInput.text, "Speed", 0, 20, (val) => boss.GetComponent<SteeringAgent>().maxSpeed = val);

        // 3. Minion Swarm (Combined all minion updates here)
        ValidateAndSet(minionSpeedInput.text, "Minion Speed", 0, 20, (val) => UpdateAllMinions(val, -1, -1));
        ValidateAndSet(minionDmgInput.text, "Minion Dmg", 0, 50, (val) => UpdateAllMinions(-1, val, -1));
        ValidateAndSet(minionFollowDistInput.text, "Follow Dist", 1, 30, (val) => UpdateAllMinions(-1, -1, val));

        ShowSuccess("API Configuration Synchronized!");
    }

    private void UpdateAllMinions(float newSpeed, float newDmg, float newFollow)
    {
        BossController[] allNPCs = FindObjectsOfType<BossController>();

        // 1. Check if we actually found any NPCs at all
        if (allNPCs.Length == 0) return;

        foreach (var npc in allNPCs)
        {
            // 2. Ensure we only talk to Minions that aren't null
            if (npc != null && !npc.isBoss)
            {
                // 3. Check for the SteeringAgent before applying speed
                if (newSpeed != -1)
                {
                    SteeringAgent sa = npc.GetComponent<SteeringAgent>();
                    if (sa != null) sa.maxSpeed = newSpeed;
                }

                // 4. Apply Damage directly (assuming it's a variable in BossController)
                if (newDmg != -1) npc.closeRangeDamage = newDmg;

                // 5. Apply Follow Distance
                if (newFollow != -1) npc.followDistance = newFollow;
            }
        }
    }
    private void ValidateAndSet(string input, string fieldName, float min, float max, System.Action<float> successAction)
    {
        // If the box is empty, show a specific error
        if (string.IsNullOrWhiteSpace(input))
        {
            ShowError($"Empty Field: {fieldName} is required!");
            return;
        }

        if (float.TryParse(input, out float result))
        {
            if (result >= min && result <= max)
            {
                successAction(result);
            }
            else
            {
                ShowError($"{fieldName} out of range! ({min}-{max})");
            }
        }
        else
        {
            ShowError($"Invalid format in {fieldName}!");
        }
    }

    private void ShowError(string msg) { errorLabel.text = msg; errorLabel.color = Color.red; }
    private void ShowSuccess(string msg) { errorLabel.text = msg; errorLabel.color = Color.green; }
}