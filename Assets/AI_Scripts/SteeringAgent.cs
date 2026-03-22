using UnityEngine;
using UnityEngine.AI; // Required for NavMesh

public class SteeringAgent : MonoBehaviour
{
    public float maxSpeed = 5f;
    public float maxForce = 10f;
    private Vector3 velocity;
    private float slowingDistance = 2f;
    private NavMeshAgent agent; // Reference to the agent

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Force the agent to snap to the closest blue point on the map
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 2.0f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
        }

        // This must be false for our manual steering to work!
        agent.updatePosition = false;
        agent.updateRotation = false;
    }

    private void ApplyForce(Vector3 force)
    {
        force = Vector3.ClampMagnitude(force, maxForce);
        velocity = Vector3.ClampMagnitude(velocity + force * Time.deltaTime, maxSpeed);
        velocity.y = 0;

        if (agent.isOnNavMesh)
        {
            // 1. Move the GHOST Agent
            agent.Move(velocity * Time.deltaTime);

            // 2. THE FIX: Pull the Boss's body to where the Agent just moved
            transform.position = agent.nextPosition;
        }
        else
        {
            // Fallback if off-mesh
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 1.0f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
            }
        }

        // This keeps the agent's internal logic from drifting away
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
        {
            desired = desired.normalized * maxSpeed * (distance / slowingDistance);
        }
        else
        {
            desired = desired.normalized * maxSpeed;
        }
        ApplyForce(desired - velocity);
    }

    public void Leave(Vector3 target)
    { // Also known as Flee
        Vector3 desired = (transform.position - target).normalized * maxSpeed;
        ApplyForce(desired - velocity);
    }

    public void Wander()
    {
        Vector3 circleCenter = transform.position + transform.forward * 5f;
        Vector3 randomOffset = Random.insideUnitSphere * 4f;
        Vector3 target = circleCenter + randomOffset;

        // Use SamplePosition to find the nearest legal spot on the blue mesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(target, out hit, 5.0f, NavMesh.AllAreas))
        {
            Seek(hit.position);
        }
    }
    void Update()
    {
        // Draws a red line in the Scene view to the Boss's forward target
        Debug.DrawRay(transform.position, transform.forward * 5f, Color.red);
        Debug.Log("Current Velocity: " + velocity.magnitude);
    }
}