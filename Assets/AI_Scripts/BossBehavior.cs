using UnityEngine;
using System.Collections.Generic;

public class BossBehavior : MonoBehaviour
{
    private NPCController npc;
    private SteeringAgent steering;
    private Node rootNode; // Line 15 - Reference declared

    void Start()
    {
        // 1. Get the components
        npc = GetComponent<NPCController>();
        steering = GetComponent<SteeringAgent>();

        // 2. Build the Priority-Based Behavior Tree
        // The "=" here is crucial to assign the new Selector to our rootNode variable
        rootNode = new Selector(new List<Node>
        {
            // PRIORITY 1: SURVIVAL
            new Sequence(new List<Node> {
                new ConditionNode(() => npc.bossNpcHealth <= npc.lowHealthThreshold),
                new ActionNode(() => {
                    npc.currentState = "Survival";
                    npc.currentAction = "Retreating";
                    // Using "Leave" steering to move away from player
                    steering.Leave(GameObject.FindWithTag("Player").transform.position);
                })
            }),

            // PRIORITY 2: COMBAT
            new Sequence(new List<Node> {
                new ConditionNode(() => npc.playerVisible),
                new ActionNode(() => {
                    npc.currentState = "Combat";
                    if (npc.playerDistance <= npc.attackRange) {
                        npc.currentAction = "Attacking";
                        Debug.Log("I am in range! Swiping at player!");
                        // Insert Attack Animation/Logic here
                    } else {
                        npc.currentAction = "Chasing";
                        steering.Seek(GameObject.FindWithTag("Player").transform.position);
                    }
                })
            }),

            // PRIORITY 3: WANDER (DEFAULT)
            new ActionNode(() => {
                npc.currentState = "Idle/Patrol";
                npc.currentAction = "Wandering";
                steering.Wander();
            })
        });
    }

    void Update()
    {
        // Check if rootNode exists before Ticking to avoid NullReferenceException
        if (rootNode != null)
        {
            rootNode.Tick();
        }
    }
}