using UnityEngine;

public class BottleClickHandler : MonoBehaviour
{
    [Header("Spawn Points")]
    public Transform tapaSpawnPoint;            
    public Transform bottleFinalSpawnPoint;     

    private bool tapaSeparada = false;
    private bool botellaMovida = false;

    private void OnMouseDown()
    {
        if (!tapaSeparada)
        {
            Transform cap = transform.Find("BottleCap");
            if (cap != null && tapaSpawnPoint != null)
            {
                cap.position = tapaSpawnPoint.position;
                tapaSeparada = true;
                Debug.Log("🧴 Tapa movida al segundo spawn.");
            }
            else
            {
                Debug.LogWarning("❌ No se encontró 'BottleCap' o falta el spawn.");
            }
        }
        else if (!botellaMovida)
        {
            Transform cuerpo = transform.Find("Bottle");
            if (cuerpo != null && bottleFinalSpawnPoint != null)
            {
                cuerpo.position = bottleFinalSpawnPoint.position;
                botellaMovida = true;
                Debug.Log("📦 Cuerpo de botella movido al tercer spawn.");
            }
            else
            {
                Debug.LogWarning("❌ No se encontró 'Bottle' o falta el tercer spawn.");
            }
        }
    }
}
