using UnityEngine;
using System.Collections.Generic;

public class BossBehavior : MonoBehaviour
{
    private BossController npc;
    private SteeringAgent steering;
    private Node rootNode;
    private Transform playerTransform;

    void Start()
    {
        npc = GetComponent<BossController>();
        steering = GetComponent<SteeringAgent>();

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) playerTransform = playerObj.transform;

        rootNode = new Selector(new List<Node>
        {
       // 1. SURVIVAL BRANCH
        new Sequence(new List<Node> {
            // This condition keeps the branch ALIVE
            new ConditionNode(() => npc.isBoss && npc.bossNpcHealth <= npc.lowHealthThreshold && !npc.beenToSafeArea),
            new Selector(new List<Node> {
                // RETREAT
                new Sequence(new List<Node> {
                    new ConditionNode(() => Vector3.Distance(transform.position, npc.safeAreaPosition) > 2.5f),
                    new ActionNode(() => {
                        npc.currentState = "Survival"; // Category
                        npc.currentAction = BossAction.Retreat; // Specific Icon/Text
                        steering.Seek(npc.safeAreaPosition);
                        return NodeStatus.RUNNING;
                    })
                }),

                // STEP B: CALL BACKUP (Runs once he is at the safe area)
                new Sequence(new List<Node> {
                    new ActionNode(() => {
                        npc.CallBackup();
                        return NodeStatus.SUCCESS;
                    }),
                    new Timeout(new ActionNode(() => {
                        npc.currentState = "Survival";
                        npc.currentAction = BossAction.Upgrade;
                        steering.Stop();
                        if (!npc.isUpgraded) npc.PerformUpgrade();
                        return NodeStatus.RUNNING;
                    }), 10.0f)
                }),

                // UPGRADE
                new Timeout(new ActionNode(() => {
                    // FORCE the text to stay visible during the timeout
                    npc.currentState = "Survival";
                    npc.currentAction = BossAction.Upgrade;
                    steering.Stop();
        
                    if (!npc.isUpgraded) {
                        npc.PerformUpgrade();
                    }
        
                    // Return RUNNING so the Timeout doesn't end early
                    return NodeStatus.RUNNING;
                }), 10.0f)
            })
        }),

        // 2. MONEY BRANCH
        new Sequence(new List<Node> {
        // 1. Check if money is there to start the process
        new ConditionNode(() => npc.moneyVisible),
        new Selector(new List<Node> {
            // 2. If far away, Seek
            new Sequence(new List<Node> {
                new ConditionNode(() => Vector3.Distance(transform.position, npc.currentMoneyPos) > 1.2f),
                new ActionNode(() => {
                    npc.currentState = "Money";
                    npc.currentAction = BossAction.TrackMoney;
                    steering.Seek(npc.currentMoneyPos);
                    return NodeStatus.RUNNING;
                })
            }),
            // 3. If close, "Snap" the collection and hold for 0.5s
            new Timeout(new ActionNode(() => {
                npc.currentState = "Money";
                npc.currentAction = BossAction.CollectMoney;
                steering.Stop();
                
                // We collect it immediately on the first frame of the timeout
                if(npc.moneyVisible) npc.CollectMoney(); 
    
                // We return RUNNING so the node STAYS active for 0.5s 
                // even though moneyVisible is now false!
                return NodeStatus.RUNNING;
            }), 10.0f)
        })
    }),

        // 3. COMBAT BRANCH
       // 3. COMBAT BRANCH
        new Selector(new List<Node> {
            new Sequence(new List<Node> {
                new ConditionNode(() => npc.playerVisible),
                new ActionNode(() => { npc.currentState = "Combat"; return NodeStatus.SUCCESS; }),

                new Selector(new List<Node> {
                    // ATTACK & MOVE Sequence
                    new Sequence(new List<Node> {
                        new ConditionNode(() => npc.playerDistance <= npc.attackRange),
                        // Movement Logic (Always Move)
                        new ActionNode(() => {
                            npc.HandleMovementLogic();
                            return NodeStatus.SUCCESS;
                        }),
                        // Shooting Logic (Cooldown)
                        new Selector(new List<Node> {
                            new Cooldown(new ActionNode(() => {
                                npc.currentAction = BossAction.Attack;
                                npc.DealDamageToPlayer();
                                return NodeStatus.SUCCESS;
                            }), npc.attackCooldown),
                            new ActionNode(() => NodeStatus.SUCCESS)
                        })
                    }),

                    // CHASE (Runs if PlayerDistance > AttackRange)
                    new ActionNode(() => {
                        npc.currentAction = BossAction.Chase;
                        steering.Seek(playerTransform.position);
                        return NodeStatus.RUNNING;
                    })
                }) // End of Inner Selector
            }), // End of PlayerVisible Sequence

            // 2. SEARCH logic
            new Sequence(new List<Node> {
                new ConditionNode(() => npc.damageReceived),
                new Selector(new List<Node> {
                    new Timeout(new ActionNode(() => {
                        npc.currentAction = BossAction.Search;
                        npc.currentState = "Combat";
                        steering.Wander();
                        return NodeStatus.RUNNING;
                    }), npc.searchDuration),

                    new ActionNode(() => {
                        npc.damageReceived = false;
                        return NodeStatus.SUCCESS;
                    })
                })
            })
        }), // End of Combat Branch Selector
        
        // 4. PATROL / FOLLOW BRANCH (Lowest Priority)
        new Sequence(new List<Node> {
            new Repeater(new ActionNode(() => {
            // --- MINION LOGIC ---
            if (!npc.isBoss && npc.bossTransform != null)
            {
                npc.currentState = "Following Boss";
                npc.currentAction = BossAction.Wandering;
            
                float distToBoss = Vector3.Distance(transform.position, npc.bossTransform.position);
            
                // CHANGE THIS LINE: Use the API variable instead of 4f
                if (distToBoss > npc.followDistance)
                {
                    // Use the minion's individual maxSpeed (which the API also controls)
                    steering.Seek(npc.bossTransform.position);
                }
                else
                {
                    steering.Stop();
                }
                return NodeStatus.RUNNING;
            }
                
                // --- BOSS LOGIC (OR if boss is dead) ---
                else
                {
                    npc.currentState = "Patrol";
                    npc.currentAction = BossAction.Wandering;
                    steering.Wander();
                    return NodeStatus.RUNNING; // Keep repeating the wander logic
                }
            }), -1) // -1 ensures it never stops until a higher priority branch interrupts
        })

        });
    }

    void Update()
    {
        if (npc != null && npc.isDead)
        {
            // 1. Force the technical state to something neutral
            npc.currentState = "DEFEATED";

            // 2. Set the Action to something that has NO icon
            npc.currentAction = BossAction.Dead;

            // 3. STOP TICKING
            // By not calling rootNode.Tick(), the green highlights 
            // will naturally disappear because they aren't being updated.
            return;
        }

        if (rootNode != null) rootNode.Tick();
    }

}