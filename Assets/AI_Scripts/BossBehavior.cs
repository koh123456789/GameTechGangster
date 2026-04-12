using UnityEngine;
using System.Collections.Generic;

public class BossBehavior : MonoBehaviour
{
    private NPCController npc;
    private SteeringAgent steering;
    private Node rootNode;
    private Transform playerTransform;

    void Start()
    {
        npc = GetComponent<NPCController>();
        steering = GetComponent<SteeringAgent>();

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) playerTransform = playerObj.transform;

        // --- BUILDING THE TREE ACCORDING TO YOUR GRAPH ---
        rootNode = new Selector(new List<Node>
        {
                // 1. SURVIVAL BRANCH (Priority 1)
                new Sequence(new List<Node> {
                    // Only enter this branch if health is low AND we haven't retreated yet
                    new ConditionNode(() => npc.bossNpcHealth <= npc.lowHealthThreshold && !npc.beenToSafeArea),
                // --- INSIDE SURVIVAL BRANCH ---
                new ActionNode(() => {
                    if (!npc.beenToSafeArea) {
                        npc.currentState = "Survival";
                        float distToSafe = Vector3.Distance(transform.position, npc.safeAreaPosition);
                
                        if (distToSafe > 2.5f) {
                            npc.currentAction = "Retreat"; // UI HIGHLIGHT
                            steering.Seek(npc.safeAreaPosition);
                            return NodeStatus.RUNNING;
                        }
                
                        // Arrived at Safe Area
                        npc.currentAction = "Upgrade & Buff"; // UI HIGHLIGHT
                        npc.beenToSafeArea = true;
                        npc.bossNpcHealth = 100f;
                        npc.SwitchToLongRange();
                
                        return NodeStatus.SUCCESS;
                    }
                    return NodeStatus.SUCCESS;
                })
                }),
                // --- INSIDE MONEY BRANCH ---
                new Sequence(new List<Node> {
                    new ConditionNode(() => npc.moneyVisible),
                    new ActionNode(() => {
                        npc.currentState = "Interact";
                
                        float dist = Vector3.Distance(transform.position, npc.currentMoneyPos);
                        if (dist < 1.2f) {
                            npc.currentAction = "Collect Cash"; // UI HIGHLIGHT
                            npc.CollectMoney();
                            return NodeStatus.SUCCESS;
                        } else {
                            npc.currentAction = "Track Cash"; // UI HIGHLIGHT
                            steering.Seek(npc.currentMoneyPos);
                            return NodeStatus.RUNNING;
                        }
                    })
                }),
            // 3. COMBAT BRANCH
            new Selector(new List<Node> {
            new Sequence(new List<Node> {
                new ConditionNode(() => npc.playerVisible),
                new Selector(new List<Node> {
                    // OPTION 1: Close enough to Attack
                    new Sequence(new List<Node> {
                        new ConditionNode(() => npc.playerDistance <= npc.attackRange),
                        new Parallel(new List<Node> {
                            new ActionNode(() => {
                                npc.currentState = "Combat"; // Ensure state is set
                                if (npc.playerDistance > steering.arrivalDistance) {
                                    npc.currentAction = "Enter Attack Range"; // UI HIGHLIGHT
                                    steering.Arrive(playerTransform.position);
                                    return NodeStatus.RUNNING;
                                } else {
                                    steering.Stop();
                                    return NodeStatus.SUCCESS;
                                }
                            }),
                            new Cooldown(
                                new ActionNode(() => {
                                    npc.currentAction = "Attack"; // UI HIGHLIGHT
                                    npc.DealDamageToPlayer();
                                    return NodeStatus.SUCCESS;
                                }),
                                2.0f
                            )
                        })
                    }),
                    // OPTION 2: Chase
                    new ActionNode(() => {
                        npc.currentState = "Combat";
                        npc.currentAction = "Chase"; // UI HIGHLIGHT
                        steering.Seek(playerTransform.position);
                        return NodeStatus.RUNNING;
                    })
                })
            }),
            
                // SUB-BRANCH B: PLAYER HIDDEN BUT WE WERE JUST HIT
                new Sequence(new List<Node> {
                    new ConditionNode(() => npc.damageReceived),
                    new ActionNode(() => {
                        Debug.Log("<color=magenta>Action: Searching Last Known Location</color>");
                        npc.currentState = "Combat";
                        npc.currentAction = "Search"; // UI HIGHLIGHT
                        return NodeStatus.SUCCESS;
                        // steering.Seek(lastKnownPos);
                    })
                })
            }),

            // 4. PATROL BRANCH (Default)
            new Sequence(new List<Node> {
                new ConditionNode(() => !npc.playerVisible),
                new ConditionNode(() => !npc.moneyVisible),
               // new Inverter(new ConditionNode(() => npc.playerVisible)),
                //new Inverter(new ConditionNode(() => npc.moneyVisible)),
                new ActionNode(() => {
                    // We only log this once in a while to prevent spamming the console
                    if (Time.frameCount % 100 == 0) Debug.Log("<color=cyan>BT Logic: No Targets - Wandering.</color>");
                    npc.currentState = "Idle/Patrol";
                    npc.currentAction = "Wandering";
                    steering.Wander();
                    return NodeStatus.SUCCESS;
                })
            })
        });
    }

    void Update()
    {
        if (rootNode != null) rootNode.Tick();
    }


}