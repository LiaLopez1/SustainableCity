using UnityEngine;
using UnityEngine.AI;

public class NPCInteraction : MonoBehaviour
{
    [Header("Detección del jugador")]
    public string playerTag = "Player";

    [Header("Iconos / UI")]
    public GameObject exclamationIcon;   // Signo ! o ?
    public GameObject talkPromptUI;      // Texto "Pulsa E para hablar"
    public GameObject dialogueCanvas;    // Panel con las opciones de diálogo

    [Header("Movimiento del NPC")]
    public NavMeshAgent agent;
    public NPCWander wanderScript;       // Script que lo hace caminar por el mapa

    [Header("Cámara principal")]
    public CamaraJugador camaraJugador;  // Script de la cámara del jugador

    [Header("Misión de recoger basura")]
    public int trashGoal = 10;                // Cuánta basura debe recoger
    public bool isCollectingTrash = false;    // Misión en curso
    public bool trashQuestCompleted = false;  // Misión completada
    public int currentTrashCount = 0;         // Basura que lleva este NPC

    [Header("Inventario del jugador")]
    public InventorySystem playerInventory;   // Inventario del jugador
    public ItemData basuraItemData;          // ItemData de la basura que te va a entregar

    private bool playerInRange = false;
    private bool isTalking = false;
    private Transform playerTransform;

    void Start()
    {
        // Referencias automáticas si no se arrastran en el Inspector
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (wanderScript == null) wanderScript = GetComponent<NPCWander>();
        if (camaraJugador == null) camaraJugador = FindObjectOfType<CamaraJugador>();
        if (playerInventory == null) playerInventory = FindObjectOfType<InventorySystem>();

        if (exclamationIcon != null) exclamationIcon.SetActive(true);
        if (talkPromptUI != null) talkPromptUI.SetActive(false);
        if (dialogueCanvas != null) dialogueCanvas.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = true;
            playerTransform = other.transform;

            if (!isTalking && talkPromptUI != null)
                talkPromptUI.SetActive(true);
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
        // Salir de conversación con E SOLO si ya no hay panel abierto
        else if (isTalking && Input.GetKeyDown(KeyCode.E) && (dialogueCanvas == null || !dialogueCanvas.activeSelf))
        {
            EndConversation();
        }
    }

    void StartConversation()
    {
        isTalking = true;

        // 1) Detener al NPC
        if (wanderScript != null) wanderScript.enabled = false;

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

        // 3) Pausar la cámara del jugador (deja de seguirlo)
        if (camaraJugador != null)
            camaraJugador.enabled = false;

        // 4) Ocultar signo !/? y "Pulsa E"
        if (exclamationIcon != null)
            exclamationIcon.SetActive(false);

        if (talkPromptUI != null)
            talkPromptUI.SetActive(false);

        // 5) Mostrar panel de diálogo
        if (dialogueCanvas != null)
            dialogueCanvas.SetActive(true);

        Debug.Log("Comienza conversación con " + gameObject.name);
    }

    public void EndConversation()
    {
        isTalking = false;

        // 1) Reanudar movimiento
        if (agent != null)
            agent.isStopped = false;

        if (wanderScript != null)
            wanderScript.enabled = true;

        // 2) Volver a activar la cámara del jugador
        if (camaraJugador != null)
            camaraJugador.enabled = true;

        // 3) Ocultar panel
        if (dialogueCanvas != null)
            dialogueCanvas.SetActive(false);

        // 4) Mostrar el signo ! solo si no está en misión ni completada
        if (exclamationIcon != null && !isCollectingTrash && !trashQuestCompleted)
            exclamationIcon.SetActive(true);

        // 5) Si el jugador sigue cerca, vuelve "Pulsa E"
        if (playerInRange && talkPromptUI != null)
            talkPromptUI.SetActive(true);

        Debug.Log("Termina conversación con " + gameObject.name);
    }

    // ==========================
    // BOTONES DEL DIALOGO
    // ==========================

    // Opción: Iniciar misión de recoger basura
    public void OnClick_RecogerBasura()
    {
        isCollectingTrash = true;
        trashQuestCompleted = false;
        currentTrashCount = 0;

        Debug.Log("Misión de basura iniciada. Debe recoger " + trashGoal + " ítems.");

        // Cerramos diálogo para que el NPC salga a "trabajar"
        EndConversation();
    }

    // Opción: Cancelar / hablar después
    public void OnClick_Cancelar()
    {
        Debug.Log("Jugador canceló la misión de basura.");
        EndConversation();
    }

    // Llamado desde TrashCollector cuando llega a la meta
    public void OnTrashQuestCompleted()
    {
        trashQuestCompleted = true;
        isCollectingTrash = false;

        Debug.Log("NPC terminó de recoger basura. Total: " + currentTrashCount);

        // Volvemos a mostrar el signo para que el jugador sepa que ya puede ir
        if (exclamationIcon != null)
            exclamationIcon.SetActive(true);
    }

    // Opción: ENTREGAR BASURA al jugador (este es el que te faltaba)
    public void OnClick_EntregarBasura()
    {
        // Validaciones básicas
        if (!trashQuestCompleted)
        {
            Debug.Log("El NPC aún no ha completado la misión de basura.");
            return;
        }

        if (currentTrashCount <= 0)
        {
            Debug.Log("El NPC no tiene basura para entregar.");
            return;
        }

        if (playerInventory == null || basuraItemData == null)
        {
            Debug.LogWarning("Falta asignar playerInventory o basuraItemData en el NPC.");
            return;
        }

        // Agregar la basura al inventario del jugador
        bool added = playerInventory.AddItem(basuraItemData, currentTrashCount);

        if (added)
        {
            Debug.Log($"Se añadieron {currentTrashCount} unidades de basura al inventario.");
        }
        else
        {
            Debug.LogWarning("No se pudo agregar la basura al inventario (no hay espacio).");
        }

        // Resetear estado de la misión en el NPC
        currentTrashCount = 0;
        trashQuestCompleted = false;
        isCollectingTrash = false;

        // El signo puede volver a aparecer para ofrecer de nuevo la misión, si quieres
        if (exclamationIcon != null)
            exclamationIcon.SetActive(true);

        // Cerrar conversación
        EndConversation();
    }
}
