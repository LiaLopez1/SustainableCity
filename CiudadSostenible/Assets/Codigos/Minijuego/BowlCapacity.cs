using UnityEngine;
using TMPro;

public class BowlCapacity : MonoBehaviour
{
    [Header("Configuración")]
    public int maxCapacity = 5;
    public TextMeshProUGUI fullMessageText;

    private int currentCount = 0;

    // Método renombrado a TryAddSphere para consistencia
    public bool TryAddSphere()
    {
        if (currentCount >= maxCapacity)
            return false;

        currentCount++;

        // Mostrar mensaje solo al llegar a 5
        if (currentCount == maxCapacity && fullMessageText != null)
        {
            fullMessageText.gameObject.SetActive(true);
            Invoke("HideMessage", 2f);
        }

        return true;
    }

    private void OnCollisionExit(Collision collision)
    {
        currentCount = Mathf.Max(0, currentCount - 1);
    }

    private void HideMessage()
    {
        if (fullMessageText != null)
            fullMessageText.gameObject.SetActive(false);
    }
}