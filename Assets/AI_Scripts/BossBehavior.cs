using UnityEngine;
using System.Collections.Generic;

public class BossBehavior : MonoBehaviour
{
    private NPCController npc;
    private SteeringAgent steering;
    private Node rootNode;
    private Transform playerTransform;

    public enum TreeComplexityMode
    {
        BaselineFlat, 
        OriginalLogic,
        ComplexDeep  
    }

    [Header("Tree Complexity Testing")]
    public TreeComplexityMode complexityMode = TreeComplexityMode.OriginalLogic;

    [Tooltip("Artificial depth layers for ComplexDeep mode.")]
    public int artificialDepth = 50;

    [Header("Tick Rate Testing")]
    public float tickInterval = 0f;
    private float timeSinceLastTick = 0f;


    void Start()
    {
        npc = GetComponent<NPCController>();
        steering = GetComponent<SteeringAgent>();

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) playerTransform = playerObj.transform;

        // Switch statement to build the tree
        switch (complexityMode)
        {
            case TreeComplexityMode.BaselineFlat:
                rootNode = BuildBaselineTree();
                break;
            case TreeComplexityMode.ComplexDeep:
                rootNode = BuildComplexTree();
                break;
            case TreeComplexityMode.OriginalLogic:
            default:
                rootNode = GetActualBossLogic();
                break;
        }
    }

    void Update()
    {
        if (npc != null && npc.isDead)
        {
            npc.currentState = "DEFEATED";
            npc.currentAction = BossAction.Dead;
            return;
        }

        if (rootNode != null)
        {
            if (tickInterval <= 0f)
            {
                rootNode.Tick();
            }
            else
            {
                timeSinceLastTick += Time.deltaTime;
                if (timeSinceLastTick >= tickInterval)
                {
                    rootNode.Tick();
                    timeSinceLastTick = 0f; 
                }
            }
        }
    }

    private Node BuildBaselineTree()
    {
        //  3 nodes only   
        return new Selector(new List<Node> {
            new Sequence(new List<Node> {
                new ConditionNode(() => npc.playerVisible),
                new ActionNode(() => {
                    steering.Seek(playerTransform.position);
                    return NodeStatus.RUNNING;
                })
            }),
            new ActionNode(() => {
                steering.Wander();
                return NodeStatus.RUNNING;
            })
        });
    }

    private Node BuildComplexTree()
    {
        //Get the initial game logic
        Node coreGameLogic = GetActualBossLogic();

        // forces the CPU to recursively travel down many many layers every single frame?
        Node deepTree = coreGameLogic;
        for (int i = 0; i < artificialDepth; i++)
        {
            deepTree = new Sequence(new List<Node> {
                // dummy condition
                new ConditionNode(() => true),
                new Repeater(deepTree, 1)
            });
        }

        return deepTree;
    }

    // =========================================
    // ORIGINAL BOSS LOGIC
    // ==========================================
    private Node GetActualBossLogic()
    {
        return new Selector(new List<Node>
        {
            // 1. SURVIVAL BRANCH
            new Sequence(new List<Node> {
                new ConditionNode(() => npc.isBoss && npc.NpcHealth <= npc.lowHealthThreshold && !npc.beenToSafeArea),
                new Selector(new List<Node> {
                    new Sequence(new List<Node> {
                        new ConditionNode(() => Vector3.Distance(transform.position, npc.safeAreaPosition) > 2.5f),
                        new ActionNode(() => {
                            npc.currentState = "Survival";
                            npc.currentAction = BossAction.Retreat;
                            steering.Seek(npc.safeAreaPosition);
                            return NodeStatus.RUNNING;
                        })
                    }),
                    new Sequence(new List<Node> {
                        new ActionNode(() => {
                            npc.CallBackup();
                            return NodeStatus.SUCCESS;
                        }),
                        new ActionNode(() => {
                            npc.currentState = "Survival";
                            npc.currentAction = BossAction.Upgrade;
                            steering.Stop();
                            if (!npc.isUpgraded) npc.PerformUpgrade();
                            return NodeStatus.RUNNING;
                        }),
                    }),
                    new ActionNode(() => {
                        npc.currentState = "Survival";
                        npc.currentAction = BossAction.Upgrade;
                        steering.Stop();
                        if (!npc.isUpgraded) npc.PerformUpgrade();
                        return NodeStatus.RUNNING;
                    }),
                })
            }),

            // 2. MONEY BRANCH
            new Sequence(new List<Node> {
                new ConditionNode(() => npc.moneyVisible && npc.NpcCash < npc.cashTarget),
                new ActionNode(() => {
                    npc.currentState = "Money";
                    npc.currentAction = BossAction.TrackMoney;
                    npc.TrackMoney();
                    return NodeStatus.RUNNING;
                })
            }),

            // 3. COMBAT BRANCH
            new Selector(new List<Node> {
                new Sequence(new List<Node> {
                    new ConditionNode(() => npc.playerVisible),
                    new ActionNode(() => { npc.currentState = "Combat"; return NodeStatus.SUCCESS; }),
                    new Selector(new List<Node> {
                        new Sequence(new List<Node> {
                            new ConditionNode(() => npc.playerDistance <= npc.meleeWeaponRange),
                            new ActionNode(() => {
                                npc.HandleMovementLogic();
                                return NodeStatus.SUCCESS;
                            }),
                            new Selector(new List<Node> {
                                new Cooldown(new ActionNode(() => {
                                    npc.currentAction = BossAction.Attack;
                                    npc.DealDamageToPlayer();
                                    return NodeStatus.SUCCESS;
                                }), npc.attackCooldown),
                                new ActionNode(() => NodeStatus.SUCCESS)
                            })
                        }),
                        new ActionNode(() => {
                            npc.currentAction = BossAction.Chase;
                            steering.Seek(playerTransform.position);
                            return NodeStatus.RUNNING;
                        })
                    })
                }),

                new Sequence(new List<Node> {
                    new ConditionNode(() => npc.damageReceived && npc.lastKnownPlayerPos != Vector3.zero),
                    new Selector(new List<Node> {
                        new Timeout(new ActionNode(() => {
                            npc.currentAction = BossAction.Search;
                            npc.currentState = "Combat";
                            npc.Search();
                            return NodeStatus.RUNNING;
                        }), npc.searchDuration),
                        new ActionNode(() => {
                            npc.damageReceived = false;
                            return NodeStatus.SUCCESS;
                        })
                    })
                })
            }), 
            
            // 4. PATROL / FOLLOW BRANCH
            new Sequence(new List<Node> {
                new Repeater(new ActionNode(() => {
                if (!npc.isBoss && npc.bossTransform != null)
                {
                    npc.currentState = "Following Boss";
                    npc.currentAction = BossAction.Wandering;
                    float distToBoss = Vector3.Distance(transform.position, npc.bossTransform.position);

                    if (distToBoss > npc.followDistance)
                    {
                        steering.Seek(npc.bossTransform.position);
                    }
                    else
                    {
                        steering.Stop();
                    }
                    return NodeStatus.RUNNING;
                }
                else
                {
                    npc.currentState = "Patrol";
                    npc.currentAction = BossAction.Wandering;
                    steering.Wander();
                    return NodeStatus.RUNNING;
                }
                }), -1)
            })
        });
    }
}

/*using UnityEngine;
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
            // This condition keeps the branch ALIVE
            new ConditionNode(() => npc.isBoss && npc.NpcHealth <= npc.lowHealthThreshold && !npc.beenToSafeArea),
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
                    new ActionNode(() => {
                        npc.currentState = "Survival";
                        npc.currentAction = BossAction.Upgrade;
                        steering.Stop();
                        if (!npc.isUpgraded) npc.PerformUpgrade();
                        return NodeStatus.RUNNING;
                    }), 
                }),

                // UPGRADE
                new ActionNode(() => {
                    // FORCE the text to stay visible during the timeout
                    npc.currentState = "Survival";
                    npc.currentAction = BossAction.Upgrade;
                    steering.Stop();

                    if (!npc.isUpgraded) {
                        npc.PerformUpgrade();
                    }
        
                    // Return RUNNING so the Timeout doesn't end early
                    return NodeStatus.RUNNING;
                }),
            })
        }),

        // 2. MONEY BRANCH
        new Sequence(new List<Node> {
            // Check if he sees money AND doesn't have enough yet
            new ConditionNode(() => npc.moneyVisible && npc.NpcCash < npc.cashTarget),
        
            new ActionNode(() => {
                // Set visual states for the Tally/Graph
                npc.currentState = "Money";
                npc.currentAction = BossAction.TrackMoney; 
                
                // Tells steering to Seek the currentMoneyPos
                npc.TrackMoney(); 
                
                // IMPORTANT: Return RUNNING so the tree stays on this node 
                // until the Boss actually touches the coin.
                return NodeStatus.RUNNING;
            })
        }),

       // 3. COMBAT BRANCH
        new Selector(new List<Node> {
            new Sequence(new List<Node> {
                new ConditionNode(() => npc.playerVisible),
                new ActionNode(() => { npc.currentState = "Combat"; return NodeStatus.SUCCESS; }),

                new Selector(new List<Node> {
                    // ATTACK & MOVE Sequence
                    new Sequence(new List<Node> {
                        new ConditionNode(() => npc.playerDistance <= npc.meleeWeaponRange),
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
                new ConditionNode(() => npc.damageReceived && npc.lastKnownPlayerPos != Vector3.zero),
                new Selector(new List<Node> {
                    new Timeout(new ActionNode(() => {
                        npc.currentAction = BossAction.Search;
                        npc.currentState = "Combat";
                        npc.Search();
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
*/