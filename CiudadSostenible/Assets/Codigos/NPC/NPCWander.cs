using UnityEngine;
using UnityEngine.AI;

public class NPCWander : MonoBehaviour
{
    [Header("Wander aleatorio")]
    public float wanderRadius = 10f;
    public float wanderInterval = 3f;

    [Header("Control externo")]
    public bool canWander = true;   // Si es false, NO elige puntos nuevos, pero la animación sigue funcionando

    private NavMeshAgent agent;
    private Animator animator;
    private float timer;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    void OnEnable()
    {
        timer = 0f;
    }

    void Start()
    {
        if (canWander)
            ChooseNewDestination();
    }

    void Update()
    {
        // 🔹 1) Animación SIEMPRE (aunque esté trabajando o quieto)
        if (animator != null && agent != null)
        {
            float speed = agent.velocity.magnitude;   // velocidad del NavMesh
            animator.SetFloat("Speed", speed);        // tu blend tree usa este parámetro
        }

        // 🔹 2) Wander solo si está permitido
        if (!canWander || agent == null)
            return;

        timer += Time.deltaTime;

        bool reached = !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;

        if (reached && timer >= wanderInterval)
        {
            ChooseNewDestination();
            timer = 0f;
        }
    }

    public void ChooseNewDestination()
    {
        if (agent == null) return;

        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += transform.position;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
        {
            agent.isStopped = false;
            agent.SetDestination(hit.position);
        }
    }
}
