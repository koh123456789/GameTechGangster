using UnityEngine;
using UnityEngine.AI;

public class SteeringAgent : MonoBehaviour
{
    [Header("Movement Settings")]
    public float maxSpeed = 5f;
    public float maxForce = 10f;
    public float slowingDistance = 2f;
    public float stoppingDistance = 0f;

    [Header("Wander Settings (API)")]
    public float wanderRadius = 8f;
    public float wanderInterval = 5f; // How many seconds before picking a new spot
    public float arrivalDistance = 1.5f; // How close to get before stopping

    [Header("Idle Settings")]
    public float minWaitTime = 2f;
    public float maxWaitTime = 5f;
    private float waitTimer = 0f;
    private bool isWaiting = false;

    // You can use this in your Behavior Tree to check if the Boss is "thinking"
    public bool IsIdle => isWaiting;

    private Vector3 velocity;
    private NavMeshAgent agent;
    private Vector3 currentWanderTarget;
    private float wanderTimer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 2.0f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
        }

        agent.updatePosition = false;
        agent.updateRotation = false;

        // Pick initial target
        PickNewWanderTarget();
    }

    private void ApplyForce(Vector3 force)
    {
        if (!this.enabled) return;

        force = Vector3.ClampMagnitude(force, maxForce);
        velocity = Vector3.ClampMagnitude(velocity + force * Time.deltaTime, maxSpeed);
        velocity.y = 0;

        if (agent.isOnNavMesh)
        {
            agent.Move(velocity * Time.deltaTime);
            transform.position = agent.nextPosition;
        }

        agent.nextPosition = transform.position;

        if (velocity.magnitude > 0.1f)
            transform.forward = Vector3.Slerp(transform.forward, velocity.normalized, Time.deltaTime * 5f);

    }

    public void Seek(Vector3 target)
    {
        Vector3 desired = (target - transform.position).normalized * maxSpeed;
        ApplyForce(desired - velocity);
    }

    public void Arrive(Vector3 target)
    {
        Vector3 desired = target - transform.position;
        float distance = desired.magnitude;

        if (distance < slowingDistance)
            desired = desired.normalized * maxSpeed * (distance / slowingDistance);
        else
            desired = desired.normalized * maxSpeed;

        ApplyForce(desired - velocity);
    }

    public void Wander()
    {
        // 1. If we are waiting, count down the timer and stop moving
        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0)
            {
                isWaiting = false;
                PickNewWanderTarget(); // Pick a new spot once rested
            }

            // Apply "Zero" force to bring him to a smooth stop
            ApplyForce(Vector3.zero - velocity);
            return;
        }

        // 2. Check if we have reached the current target
        float distanceToTarget = Vector3.Distance(transform.position, currentWanderTarget);

        if (distanceToTarget < arrivalDistance || currentWanderTarget == Vector3.zero)
        {
            // Start the waiting period
            isWaiting = true;
            waitTimer = Random.Range(minWaitTime, maxWaitTime);
            return;
        }

        // 3. If not waiting and not there yet, keep walking
        Arrive(currentWanderTarget);
    }

    private void PickNewWanderTarget()
    {
        Vector2 randomCircle = Random.insideUnitCircle * wanderRadius;
        Vector3 randomTarget = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomTarget, out hit, wanderRadius, NavMesh.AllAreas))
        {
            currentWanderTarget = hit.position;
            wanderTimer = 0f;
        }
    }

    public void Leave(Vector3 target)
    {
        // Calculate direction away from target
        Vector3 awayDirection = (transform.position - target).normalized * 5f;
        Vector3 fleeTarget = transform.position + awayDirection;

        // Ensure the flee target is actually a walkable spot
        NavMeshHit hit;
        if (NavMesh.SamplePosition(fleeTarget, out hit, 5f, NavMesh.AllAreas))
        {
            Seek(hit.position);
        }
        else
        {
            // If no spot found behind, just push away
            Vector3 desired = awayDirection.normalized * maxSpeed;
            ApplyForce(desired - velocity);
        }
    }

    public void Stop()
    {
        velocity = Vector3.zero;
        if (agent != null && agent.isOnNavMesh)
        {
            agent.velocity = Vector3.zero;
            // Keep updatePosition false so our steering stays in control
            agent.updatePosition = false;
            agent.updateRotation = false;
        }
    }

    // Add this so other scripts can check how fast the agent is moving
    public Vector3 GetVelocity()
    {
        return velocity;
    }

    // Visual Aid: Shows the wander target as a blue sphere in Scene View
    private void OnDrawGizmos()
    {
        if (currentWanderTarget != Vector3.zero)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(currentWanderTarget, 0.5f);
            Gizmos.DrawLine(transform.position, currentWanderTarget);
        }
    }
}