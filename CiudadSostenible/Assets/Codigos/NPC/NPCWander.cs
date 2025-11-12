using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class NPCWander : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] float moveSpeed = 2.4f;
    [SerializeField] float turnSpeed = 240f;             // más rápido para escapar
    [SerializeField] float randomTurnEvery = 4f;

    [Header("Evitación (probes)")]
    [SerializeField] float probeDistance = 2.0f;         // más largo: ve la pared antes
    [SerializeField] float sideProbeDistance = 1.0f;     // probes laterales
    [SerializeField] float probeRadius = 0.28f;
    [SerializeField] LayerMask obstacleMask;

    [Header("Giros")]
    [SerializeField] float smallTurnMin = 15f;
    [SerializeField] float smallTurnMax = 35f;
    [SerializeField] float bigTurnBase = 140f;           // giro grande cuando choca o ve pared de frente
    [SerializeField] float bigTurnJitter = 25f;          // aleatorio +/- para evitar loops

    [Header("Anti-atasco")]
    [SerializeField] float backOffDistance = 1.2f;       // retroceso corto
    [SerializeField] float stuckCheckInterval = 1.2f;    // cada cuánto evaluamos si está atascado
    [SerializeField] float stuckMinAdvance = 0.15f;      // si avanzó menos que esto => atascado

    [Header("Estabilidad")]
    [SerializeField] float bumpCooldown = 0.4f;
    [SerializeField] float minForwardTimeAfterTurn = 0.35f;

    Rigidbody rb;
    float randomTimer, bumpTimer, forwardTimer;
    Vector3 lastStuckPos;
    float stuckTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        randomTimer = Random.Range(0f, randomTurnEvery);
        forwardTimer = 0f;
        lastStuckPos = transform.position;
        stuckTimer = stuckCheckInterval;
    }

    void Update()
    {
        randomTimer -= Time.deltaTime;
        bumpTimer -= Time.deltaTime;
        forwardTimer -= Time.deltaTime;
        stuckTimer -= Time.deltaTime;

        // ===== 1) Probes: frente + costados =====
        Vector3 origin = transform.position + Vector3.up * 0.5f;

        bool frontHit = Physics.SphereCast(origin, probeRadius, transform.forward,
            out RaycastHit frontInfo, probeDistance, obstacleMask, QueryTriggerInteraction.Ignore);

        // lateral derecho (perpendicular a forward)
        Vector3 rightDir = Quaternion.Euler(0f, 40f, 0f) * transform.forward;
        bool rightHit = Physics.SphereCast(origin, probeRadius, rightDir,
            out _, sideProbeDistance, obstacleMask, QueryTriggerInteraction.Ignore);

        // lateral izquierdo
        Vector3 leftDir = Quaternion.Euler(0f, -40f, 0f) * transform.forward;
        bool leftHit = Physics.SphereCast(origin, probeRadius, leftDir,
            out _, sideProbeDistance, obstacleMask, QueryTriggerInteraction.Ignore);

        // ===== 2) Decidir giro ante obstáculo =====
        if ((frontHit || rightHit || leftHit) && bumpTimer <= 0f)
        {
            // si ve pared al frente, decide lado por el que haya menos colisión
            float sign; // +1 derecha, -1 izquierda

            if (frontHit)
            {
                // preferimos girar hacia el lado que NO detecta obstáculo lateral
                if (leftHit && !rightHit) sign = +1f;           // izquierda ocupada, gira derecha
                else if (rightHit && !leftHit) sign = -1f;      // derecha ocupada, gira izquierda
                else sign = (Random.value < 0.5f) ? +1f : -1f;  // ambos libres/ocupados: aleatorio

                float big = bigTurnBase + Random.Range(-bigTurnJitter, bigTurnJitter);
                StartCoroutine(SmoothTurn(big * sign));
                // retroceso corto para despegarse de la pared
                StartCoroutine(BackOff(backOffDistance));
            }
            else
            {
                // no es pared frontal, solo un lateral => pequeño ajuste al lado contrario
                sign = (!rightHit && leftHit) ? +1f : (rightHit && !leftHit) ? -1f : (Random.value < 0.5f ? +1f : -1f);
                float small = Random.Range(smallTurnMin, smallTurnMax);
                StartCoroutine(SmoothTurn(small * sign));
            }

            bumpTimer = bumpCooldown;
            forwardTimer = minForwardTimeAfterTurn;
        }

        // ===== 3) Giro aleatorio suave para patrulla natural =====
        if (randomTimer <= 0f && forwardTimer <= 0f)
        {
            float sign = Random.value < 0.5f ? -1f : 1f;
            float angle = Random.Range(smallTurnMin, smallTurnMax) * sign;
            StartCoroutine(SmoothTurn(angle));
            randomTimer = randomTurnEvery + Random.Range(-1f, 1f);
            forwardTimer = 0.2f;
        }

        // ===== 4) Detección de atasco (no avanza lo suficiente) =====
        if (stuckTimer <= 0f)
        {
            float advanced = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z),
                                              new Vector3(lastStuckPos.x, 0, lastStuckPos.z));
            if (advanced < stuckMinAdvance)
            {
                // giro grande aleatorio + retroceso
                float sign = (Random.value < 0.5f) ? -1f : 1f;
                float big = bigTurnBase + Random.Range(-bigTurnJitter, bigTurnJitter);
                StartCoroutine(SmoothTurn(big * sign));
                StartCoroutine(BackOff(backOffDistance));
                forwardTimer = minForwardTimeAfterTurn;
            }
            lastStuckPos = transform.position;
            stuckTimer = stuckCheckInterval;
        }
    }

    void FixedUpdate()
    {
        Vector3 step = transform.forward * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + step);
    }

    System.Collections.IEnumerator SmoothTurn(float angle)
    {
        float remaining = Mathf.Abs(angle);
        float dir = Mathf.Sign(angle);
        while (remaining > 0f)
        {
            float delta = Mathf.Min(remaining, turnSpeed * Time.deltaTime);
            transform.Rotate(0f, delta * dir, 0f, Space.World);
            remaining -= delta;
            yield return null;
        }
    }

    System.Collections.IEnumerator BackOff(float distance)
    {
        float moved = 0f;
        while (moved < distance)
        {
            float step = moveSpeed * 0.6f * Time.deltaTime;  // retroceso un poco más lento
            rb.MovePosition(rb.position - transform.forward * step);
            moved += step;
            yield return null;
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (((1 << col.gameObject.layer) & obstacleMask) != 0 && bumpTimer <= 0f)
        {
            // cuando choca, forzamos giro grande con base en la normal
            Vector3 n = col.contacts[0].normal; n.y = 0f;
            float sign = Vector3.SignedAngle(transform.forward, n, Vector3.up) > 0 ? +1f : -1f;
            float big = bigTurnBase + Random.Range(-bigTurnJitter, bigTurnJitter);
            StartCoroutine(SmoothTurn(big * sign));
            StartCoroutine(BackOff(backOffDistance));

            bumpTimer = bumpCooldown;
            forwardTimer = minForwardTimeAfterTurn;
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 start = transform.position + Vector3.up * 0.5f;
        Gizmos.DrawWireSphere(start, probeRadius);

        // front
        Gizmos.color = Color.red;
        Gizmos.DrawLine(start, start + transform.forward * probeDistance);
        Gizmos.DrawWireSphere(start + transform.forward * probeDistance, probeRadius * 0.7f);

        // sides
        Gizmos.color = Color.cyan;
        Vector3 r = Quaternion.Euler(0f, 40f, 0f) * transform.forward;
        Gizmos.DrawLine(start, start + r * sideProbeDistance);
        Vector3 l = Quaternion.Euler(0f, -40f, 0f) * transform.forward;
        Gizmos.DrawLine(start, start + l * sideProbeDistance);
    }
#endif
}
