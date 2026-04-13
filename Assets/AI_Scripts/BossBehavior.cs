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

        rootNode = new Selector(new List<Node>
        {
        // 1. SURVIVAL BRANCH
        new Sequence(new List<Node> {
            new ConditionNode(() => npc.bossNpcHealth <= npc.lowHealthThreshold && !npc.beenToSafeArea),
            new Selector(new List<Node> {
                // RETREAT
                new Sequence(new List<Node> {
                    new ConditionNode(() => Vector3.Distance(transform.position, npc.safeAreaPosition) > 2.5f),
                    new ActionNode(() => {
                        npc.currentState = "Survival";
                        npc.currentAction = BossAction.Retreat; // SET ACTION
                        steering.Seek(npc.safeAreaPosition);
                        return NodeStatus.RUNNING;
                    })
                }),
                // UPGRADE & BUFF
                new ActionNode(() => {
                    npc.currentState = "Survival";
                    npc.currentAction = BossAction.Upgrade; // SET ACTION
                    npc.PerformUpgrade();
                    return NodeStatus.SUCCESS;
                })
            })
        }),

        // 2. MONEY BRANCH
        new Sequence(new List<Node> {
            new ConditionNode(() => npc.moneyVisible),
            new ActionNode(() => {
                npc.currentState = "Money";
                float dist = Vector3.Distance(transform.position, npc.currentMoneyPos);
                if (dist < 1.2f) {
                    npc.currentAction = BossAction.CollectMoney; // SET ACTION
                    npc.CollectMoney();
                    return NodeStatus.SUCCESS;
                } else {
                    npc.currentAction = BossAction.TrackMoney; // SET ACTION
                    steering.Seek(npc.currentMoneyPos);
                    return NodeStatus.RUNNING;
                }
            })
        }),

        // 3. COMBAT BRANCH
        new Selector(new List<Node> {
            new Sequence(new List<Node> {
                new ConditionNode(() => npc.playerVisible),
                new ActionNode(() => { npc.currentState = "Combat"; return NodeStatus.SUCCESS; }),
                new Selector(new List<Node> {
                    // ATTACK
                    new Sequence(new List<Node> {
                        new ConditionNode(() => npc.playerDistance <= npc.attackRange),
                        new Selector(new List<Node>{
                            new Cooldown(new ActionNode(() => {
                                npc.currentAction = BossAction.Attack; // SET ACTION
                                npc.DealDamageToPlayer();
                                return NodeStatus.SUCCESS;
                            }), 2.0f),
                            new ActionNode(() => {
                                npc.currentAction = BossAction.Attack;
                                steering.Stop();
                                return NodeStatus.RUNNING;
                            })
                        })
                    }),
                    // CHASE
                    new ActionNode(() => {
                        npc.currentAction = BossAction.Chase; // SET ACTION
                        steering.Seek(playerTransform.position);
                        return NodeStatus.RUNNING;
                    })
                })
            }),

            // 2. SEARCH logic (The logic fix)
            new Sequence(new List<Node> {
                new ConditionNode(() => npc.damageReceived),
                // Wrap the Timeout in an Inverter or use a Selector to ensure we reset the flag
                new Selector(new List<Node> {
                    new Timeout(new ActionNode(() => {
                        npc.currentAction = BossAction.Search;
                        npc.currentState = "Combat"; // Added this for the UI
                        steering.Wander();
                        return NodeStatus.RUNNING;
                    }), 7.0f), 
                    
                    // This ActionNode runs ONLY when the Timeout returns FAILURE (after 7s)
                    new ActionNode(() => {
                        npc.damageReceived = false;
                        Debug.Log("Search timed out. Resetting flag.");
                        return NodeStatus.SUCCESS;
                    })
                })
            })


        }),

        
            // 4. PATROL BRANCH (Lowest Priority)
            new Sequence(new List<Node> {
                new Repeater(new ActionNode(() => {
                    npc.currentState = "Patrol";
                    npc.currentAction = BossAction.Wandering;
                    steering.Wander();
                    return NodeStatus.RUNNING;
                }), -1) // -1 makes the Boss wander forever until interrupted by Combat/Survival
            })

        });
    }

    void Update()
    {
        if (rootNode != null) rootNode.Tick();
    }


}