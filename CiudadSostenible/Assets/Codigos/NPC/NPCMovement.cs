using UnityEngine;
using UnityEngine.AI;

public class NPCWMOvement : MonoBehaviour
{
    public float wanderRadius = 10f;
    public float wanderInterval = 3f;

    private NavMeshAgent agent;
    private float timer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        timer = wanderInterval;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= wanderInterval)
        {
            Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, -1);
            agent.SetDestination(newPos);
            timer = 0;
        }
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;

        NavMeshHit navHit;
        NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);

        return navHit.position;
    }
}
