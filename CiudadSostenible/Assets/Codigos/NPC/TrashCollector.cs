using UnityEngine;

public class TrashCollector : MonoBehaviour
{
    public string trashTag = "Basura";
    public NPCInteraction npcInteraction;   // referencia al script del NPC

    private void Start()
    {
        // Si no se asigna por Inspector, intentamos encontrarlo en el mismo objeto
        if (npcInteraction == null)
            npcInteraction = GetComponentInParent<NPCInteraction>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // ¿Es basura?
        if (!other.CompareTag(trashTag)) return;

        // ¿La misión de basura está activa?
        if (npcInteraction == null) return;
        if (!npcInteraction.isCollectingTrash) return;
        if (npcInteraction.trashQuestCompleted) return;

        // Recoger basura
        Destroy(other.gameObject);
        npcInteraction.currentTrashCount++;

        Debug.Log("NPC recogió basura. Lleva: " + npcInteraction.currentTrashCount);

        // ¿Ya completó la meta?
        if (npcInteraction.currentTrashCount >= npcInteraction.trashGoal)
        {
            npcInteraction.OnTrashQuestCompleted();
        }
    }
}
