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
                new ConditionNode(() => npc.bossNpcHealth <= npc.lowHealthThreshold),
                new ActionNode(() => {
                    Debug.Log("<color=red>BT Logic: Health Low! Checking Safe Area.</color>");
                    if (!npc.beenToSafeArea) {
                        npc.currentState = "Survival";
                        npc.currentAction = "Retreating";
                        steering.Seek(npc.safeAreaPosition);
                        
                        // If we haven't reached the point yet, keep running
                        if (Vector3.Distance(transform.position, npc.safeAreaPosition) > 2f) {
                            return NodeStatus.RUNNING;
                        }
                
                        // We arrived!
                        npc.beenToSafeArea = true;
                        npc.bossNpcHealth = 100f;
                        npc.SwitchToLongRange();
                        return NodeStatus.SUCCESS;
                    }
                    return NodeStatus.SUCCESS; // Already been there, move to next task
                })
            }),
                // 2. MONEY BRANCH
                new Sequence(new List<Node> {
                    new ConditionNode(() => npc.moneyVisible),
                    new ActionNode(() => {
                        npc.currentState = "Interact";
                        npc.currentAction = "Collecting Money";
                
                        steering.Seek(npc.currentMoneyPos); 
                        
                        // Only call the interaction function if we are close enough
                        if (Vector3.Distance(transform.position, npc.currentMoneyPos) < 1.2f) {
                            npc.CollectMoney(); // <--- Clean API call
                            return NodeStatus.SUCCESS;
                        }
                        return NodeStatus.RUNNING;
                    })

                }),
            // 3. COMBAT BRANCH
            new Selector(new List<Node> {
                // SUB-BRANCH A: PLAYER IS IN SIGHT
                new Sequence(new List<Node> {
                    new ConditionNode(() => npc.playerVisible), // GATEKEEPER
                    new Selector(new List<Node> {
                        // OPTION 1: Close enough to Attack
                        new Sequence(new List<Node> {
                            new ConditionNode(() => npc.playerDistance <= npc.attackRange),
                            new Parallel(new List<Node> {
                                // Branch 1 of Parallel: Stay at Player position
                                new ActionNode(() => {

                                if (npc.playerDistance > steering.arrivalDistance) {
                                    steering.Arrive(playerTransform.position);
                                    return NodeStatus.RUNNING; // Keep moving
                                } else {
                                    steering.Stop();
                                    return NodeStatus.SUCCESS; // Arrived
                                }

                                }),

                                // Branch 2 of Parallel: The Cooldown Diamond from your graph
                                new Cooldown(
                                    new ActionNode(() => {
                                        npc.currentAction = "Attacking";
                                        npc.DealDamageToPlayer(10f); // This handles the health logic
                                        return NodeStatus.SUCCESS;
                                    }),
                                    2.0f // The 2 second duration from your graph
                                )
                            })
                        }),
                        // OPTION 2: Too far to attack, so Chase
                        new ActionNode(() => {
                            Debug.Log("<color=yellow>Action: Chasing Visible Player</color>");
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