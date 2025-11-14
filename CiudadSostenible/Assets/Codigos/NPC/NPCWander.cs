using UnityEngine;
using UnityEngine.AI;

public class NPCWander : MonoBehaviour
{
    public float wanderRadius = 10f;
    public float wanderInterval = 3f;

    private NavMeshAgent agent;
    private Animator animator;
    private float timer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();   // Asume que el Animator está en el mismo objeto

        ChooseNewDestination();
    }

    void Update()
    {
        timer += Time.deltaTime;

        // Actualizar parámetro de velocidad para las animaciones
        if (animator != null)
        {
            float speed = agent.velocity.magnitude;
            animator.SetFloat("Speed", speed);
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (timer >= wanderInterval)
            {
                ChooseNewDestination();
                timer = 0f;
            }
        }
    }

    void ChooseNewDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += transform.position;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }
}
