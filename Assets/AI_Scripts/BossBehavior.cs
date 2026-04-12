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
            new ConditionNode(() => npc.bossNpcHealth <= npc.lowHealthThreshold && !npc.beenToSafeArea),
            new ActionNode(() => {
                npc.currentState = "Survival";
                float distToSafe = Vector3.Distance(transform.position, npc.safeAreaPosition);
                if (distToSafe > 2.5f) {
                    npc.currentAction = "Retreat";
                    steering.Seek(npc.safeAreaPosition);
                    return NodeStatus.RUNNING;
                }
                npc.currentAction = "Upgrade & Buff";
                npc.PerformUpgrade();
                return NodeStatus.SUCCESS;
            })
        }),

        // 2. MONEY BRANCH (Priority 2)
        new Sequence(new List<Node> {
            new ConditionNode(() => npc.moneyVisible),
            new ActionNode(() => {
                npc.currentState = "Interact";
                float dist = Vector3.Distance(transform.position, npc.currentMoneyPos);
                if (dist < 1.2f) {
                    npc.currentAction = "Collect Cash";
                    npc.CollectMoney();
                    return NodeStatus.SUCCESS;
                } else {
                    npc.currentAction = "Track Cash";
                    steering.Seek(npc.currentMoneyPos);
                    return NodeStatus.RUNNING;
                }
            })
        }),

        // 3. COMBAT BRANCH (Priority 3)
        new Selector(new List<Node> {
            // --- SUB-BRANCH A: PLAYER VISIBLE ---
            new Sequence(new List<Node> {
                new ConditionNode(() => npc.playerVisible),

                new ActionNode(() => {
                    npc.currentState = "Combat";
                    return NodeStatus.SUCCESS; // Success lets the Sequence move to the next child
                }),
                new Selector(new List<Node> {
                    
                    // PRIORITY 1: ATTACK (The "Close" Zone)
                    new Sequence(new List<Node> {
                        new ConditionNode(() => npc.playerDistance <= npc.attackRange),
                        new Selector(new List<Node>{
                            // Action A: Do damage if cooldown is ready
                            new Cooldown(new ActionNode(() => {
                                npc.currentAction = "Attack";
                                npc.DealDamageToPlayer();
                                return NodeStatus.SUCCESS;
                            }), 2.0f),
                            // Action B: If cooldown is NOT ready, stay in Attack state
                            new ActionNode(() => {
                                npc.currentAction = "Attack";
                                steering.Stop();
                                return NodeStatus.RUNNING;
                            })
                        })
                    }),
                
                    // PRIORITY 2: ENTER RANGE (The "Middle" Zone)
                    new Sequence(new List<Node> {
                        // Only run this if we are within a reasonable 'approach' distance
                        new ConditionNode(() => npc.playerDistance <= (npc.attackRange + 4f)),
                        new ActionNode(() => {
                            npc.currentAction = "Enter Attack Range";
                            steering.Arrive(playerTransform.position);
                            return NodeStatus.RUNNING;
                        })
                    }),
                
                    // PRIORITY 3: CHASE (The "Far" Zone)
                    new ActionNode(() => {
                        npc.currentAction = "Chase";
                        steering.Seek(playerTransform.position);
                        return NodeStatus.RUNNING;
                    })
                })
            }),
            // --- SUB-BRANCH B: SEARCH IF DAMAGED ---
            new Sequence(new List<Node> {
                new ConditionNode(() => npc.damageReceived),
                new ActionNode(() => {
                    npc.currentState = "Combat";
                    npc.currentAction = "Search";
                    // steering.Seek(lastKnownPos);
                    return NodeStatus.SUCCESS;
                })
            })
        }),

        // 4. PATROL BRANCH (Priority 4 - Default)
        // No conditions needed here because the Selector only gets here 
        // if Survival, Money, and Combat ALL failed.
        new ActionNode(() => {
            npc.currentState = "Idle/Patrol";
            npc.currentAction = "Wandering";
            steering.Wander();
            return NodeStatus.SUCCESS;
        })
        });
    }

    void Update()
    {
        if (rootNode != null) rootNode.Tick();
    }


}