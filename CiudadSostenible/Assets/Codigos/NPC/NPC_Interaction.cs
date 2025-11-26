using UnityEngine;
using UnityEngine.AI;

public class NPCInteraction : MonoBehaviour
{
    [Header("Detección del jugador")]
    public string playerTag = "Player";

    [Header("Iconos / UI")]
    public GameObject exclamationIcon;   // Signo ! o ?
    public GameObject talkPromptUI;      // Texto "Pulsa E para hablar"
    public GameObject dialogueCanvas;    // Panel con las opciones de diálogo (principal)

    [Header("UI de materiales")]
    public GameObject materialesPanel;   // Panel con botones: Papel / Plástico / Orgánico

    [Header("Movimiento del NPC")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private NPCWander wanderScript;   // Script que lo hace caminar por el mapa

    [Header("Cámara principal")]
    [SerializeField] private CamaraJugador camaraJugador;  // Script de la cámara del jugador

    [Header("Misión de recoger basura")]
    public int trashGoal = 10;                // Cuánta basura debe recoger
    public bool isCollectingTrash = false;    // Misión en curso
    public bool trashQuestCompleted = false;  // Misión completada
    public int currentTrashCount = 0;         // Basura que lleva este NPC

    [Header("Tipos de basura disponibles")]
    public ItemData papelItemData;
    public ItemData plasticoItemData;
    public ItemData organicoItemData;

    [Tooltip("Tipo de basura que este NPC está recogiendo actualmente.")]
    public ItemData basuraObjetivoItemData;   // se setea al contratar

    [Header("Búsqueda de basura (radio)")]
    public float searchRadius = 10f;      // radio de búsqueda
    public float searchInterval = 0.25f;  // cada cuántos segundos vuelve a buscar

    [Header("Inventario del jugador")]
    [SerializeField] private InventorySystem playerInventory;   // Inventario del jugador

    private bool playerInRange = false;
    private bool isTalking = false;
    private Transform playerTransform;

    // estado interno para la búsqueda
    private float searchTimer = 0f;
    private ItemRecogible currentTargetItem;

    void Start()
    {
        if (!agent) agent = GetComponent<NavMeshAgent>();
        if (!wanderScript) wanderScript = GetComponent<NPCWander>();
        if (!camaraJugador) camaraJugador = FindFirstObjectByType<CamaraJugador>();
        if (!playerInventory) playerInventory = FindFirstObjectByType<InventorySystem>();

        if (exclamationIcon != null) exclamationIcon.SetActive(true);
        if (talkPromptUI != null) talkPromptUI.SetActive(false);
        if (dialogueCanvas != null) dialogueCanvas.SetActive(false);
        if (materialesPanel != null) materialesPanel.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        // 1) detección del jugador
        if (other.CompareTag(playerTag))
        {
            playerInRange = true;
            playerTransform = other.transform;

            if (!isTalking && talkPromptUI != null)
                talkPromptUI.SetActive(true);
        }

        // 2) detección de basura para "recogerla"
        var itemRec = other.GetComponent<ItemRecogible>();
        if (itemRec != null)
        {
            TryCollectTrashItem(itemRec);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = false;
            playerTransform = null;

            if (talkPromptUI != null)
                talkPromptUI.SetActive(false);

            if (isTalking)
                EndConversation();
        }
    }

    void Update()
    {
        // Entrar a conversación
        if (playerInRange && !isTalking && Input.GetKeyDown(KeyCode.E))
        {
            StartConversation();
        }
        // Salir de conversación con E SOLO si no hay panel abierto
        else if (isTalking
                 && Input.GetKeyDown(KeyCode.E)
                 && ((dialogueCanvas == null || !dialogueCanvas.activeSelf)
                     && (materialesPanel == null || !materialesPanel.activeSelf)))
        {
            EndConversation();
        }

        // Búsqueda de basura mientras está trabajando
        if (isCollectingTrash && !trashQuestCompleted)
        {
            HandleTrashSearch();
        }
    }

    // ==========================
    // CONVERSACIÓN
    // ==========================

    void StartConversation()
    {
        isTalking = true;

        // 1) Detener wander, pero NO el script (sólo apagar movimiento aleatorio)
        if (wanderScript != null)
            wanderScript.canWander = false;

        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        // 2) Girar hacia el jugador
        if (playerTransform != null)
        {
            Vector3 dir = playerTransform.position - transform.position;
            dir.y = 0f;

            if (dir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);
                transform.rotation = targetRot;
            }
        }

        // 3) Pausar la cámara del jugador
        if (camaraJugador != null)
            camaraJugador.enabled = false;

        // 4) Ocultar signo !/? y "Pulsa E"
        if (exclamationIcon != null)
            exclamationIcon.SetActive(false);

        if (talkPromptUI != null)
            talkPromptUI.SetActive(false);

        // 5) Mostrar panel principal
        if (dialogueCanvas != null)
            dialogueCanvas.SetActive(true);

        if (materialesPanel != null)
            materialesPanel.SetActive(false);

        Debug.Log("Comienza conversación con " + gameObject.name);
    }

    public void EndConversation()
    {
        isTalking = false;

        // 1) Reactivar NavMesh (el tipo de movimiento lo decide el estado: wander o misión)
        if (agent != null)
            agent.isStopped = false;

        // 2) Si NO está en misión, vuelve a vagar solo
        if (wanderScript != null && !isCollectingTrash)
            wanderScript.canWander = true;

        // 3) Reactivar cámara
        if (camaraJugador != null)
            camaraJugador.enabled = true;

        // 4) Ocultar paneles
        if (dialogueCanvas != null)
            dialogueCanvas.SetActive(false);

        if (materialesPanel != null)
            materialesPanel.SetActive(false);

        // 5) Mostrar signo ! si no está en misión ni completada
        if (exclamationIcon != null && !isCollectingTrash && !trashQuestCompleted)
            exclamationIcon.SetActive(true);

        // 6) Si el jugador sigue cerca, mostrar "Pulsa E"
        if (playerInRange && talkPromptUI != null)
            talkPromptUI.SetActive(true);

        Debug.Log("Termina conversación con " + gameObject.name);
    }

    // ==========================
    // BOTONES DEL DIALOGO
    // ==========================

    public void OnClick_Contratar()
    {

        if (materialesPanel != null)
            materialesPanel.SetActive(true);

        Debug.Log("Abriendo menú de selección de material.");
    }

    public void OnClick_Cancelar()
    {
        Debug.Log("Jugador canceló la conversación.");
        EndConversation();
    }

    // ==========================
    // Selección de MATERIAL
    // ==========================

    public void OnClick_ContratarPapel()
    {
        IniciarMisionBasura(papelItemData);
    }

    public void OnClick_ContratarPlastico()
    {
        IniciarMisionBasura(plasticoItemData);
    }

    public void OnClick_ContratarOrganico()
    {
        IniciarMisionBasura(organicoItemData);
    }

    void IniciarMisionBasura(ItemData tipoBasura)
    {
        if (tipoBasura == null)
        {
            Debug.LogWarning("No se asignó ItemData para este tipo de basura.");
            return;
        }

        basuraObjetivoItemData = tipoBasura;
        isCollectingTrash = true;
        trashQuestCompleted = false;
        currentTrashCount = 0;
        currentTargetItem = null;

        Debug.Log($"Misión iniciada: {trashGoal} de {tipoBasura.itemName}.");

        // mientras está en misión, el wander aleatorio se apaga,
        // pero el script sigue actualizando animación
        if (wanderScript != null)
            wanderScript.canWander = false;

        // 🔹 que NO se quede parado: reactivamos el agente
        if (agent != null)
            agent.isStopped = false;

        // 🔹 buscar de inmediato el primer objetivo
        searchTimer = searchInterval; // fuerza a que HandleTrashSearch busque ya
        HandleTrashSearch();

        EndConversation();
    }

    // ==========================
    // BÚSQUEDA Y RECOLECCIÓN
    // ==========================

    void HandleTrashSearch()
    {
        if (basuraObjetivoItemData == null) return;

        // si ya tiene objetivo, deja que el NavMesh se encargue
        if (currentTargetItem != null) return;

        searchTimer += Time.deltaTime;
        if (searchTimer < searchInterval)
            return;

        searchTimer = 0f;

        Collider[] hits = Physics.OverlapSphere(transform.position, searchRadius);
        ItemRecogible mejorObjetivo = null;
        float mejorDistancia = Mathf.Infinity;

        foreach (var hit in hits)
        {
            var itemRec = hit.GetComponent<ItemRecogible>();
            if (itemRec == null) continue;

            if (itemRec.itemData != basuraObjetivoItemData)
                continue;

            float dist = Vector3.Distance(transform.position, itemRec.transform.position);
            if (dist < mejorDistancia)
            {
                mejorDistancia = dist;
                mejorObjetivo = itemRec;
            }
        }

        if (mejorObjetivo != null && agent != null)
        {
            currentTargetItem = mejorObjetivo;
            agent.isStopped = false;
            agent.SetDestination(currentTargetItem.transform.position);
        }
    }

    void TryCollectTrashItem(ItemRecogible itemRec)
    {
        if (!isCollectingTrash) return;
        if (trashQuestCompleted) return;
        if (basuraObjetivoItemData == null) return;
        if (itemRec.itemData != basuraObjetivoItemData) return;

        currentTrashCount++;

        if (currentTargetItem == itemRec)
            currentTargetItem = null;

        BasuraSpawner spawner = FindFirstObjectByType<BasuraSpawner>();
        if (spawner != null)
        {
            spawner.RecogerBasura(itemRec.itemData.itemName);
        }

        Destroy(itemRec.gameObject);

        Debug.Log($"NPC recogió {itemRec.itemData.itemName}. Lleva: {currentTrashCount}");

        if (currentTrashCount >= trashGoal)
        {
            OnTrashQuestCompleted();
        }
    }

    public void OnTrashQuestCompleted()
    {
        trashQuestCompleted = true;
        isCollectingTrash = false;
        currentTargetItem = null;

        Debug.Log("NPC terminó de recoger basura. Total: " + currentTrashCount);

        // Vuelve al wander aleatorio
        if (wanderScript != null)
        {
            wanderScript.canWander = true;
            wanderScript.ChooseNewDestination();
        }

        if (exclamationIcon != null)
            exclamationIcon.SetActive(true);
    }

    // ==========================
    // ENTREGAR AL INVENTARIO
    // ==========================

    public void OnClick_EntregarBasura()
    {
        if (!trashQuestCompleted)
        {
            Debug.Log("El NPC aún no ha completado la misión.");
            return;
        }

        if (currentTrashCount <= 0)
        {
            Debug.Log("El NPC no tiene basura para entregar.");
            return;
        }

        if (playerInventory == null || basuraObjetivoItemData == null)
        {
            Debug.LogWarning("Falta asignar playerInventory o basuraObjetivoItemData.");
            return;
        }

        bool added = playerInventory.AddItem(basuraObjetivoItemData, currentTrashCount);

        if (added)
        {
            Debug.Log($"Se añadieron {currentTrashCount} de {basuraObjetivoItemData.itemName} al inventario.");
        }
        else
        {
            Debug.LogWarning("No se pudo agregar al inventario (sin espacio).");
        }

        currentTrashCount = 0;
        trashQuestCompleted = false;
        isCollectingTrash = false;
        currentTargetItem = null;

        if (exclamationIcon != null)
            exclamationIcon.SetActive(true);

        EndConversation();
    }
}
