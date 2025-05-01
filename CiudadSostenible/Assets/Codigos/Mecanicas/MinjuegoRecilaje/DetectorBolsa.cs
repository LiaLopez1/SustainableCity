using UnityEngine;
using UnityEngine.EventSystems;

public class DetectorBolsa : MonoBehaviour, IDropHandler
{
    public GameObject imagenBolsaAbierta;

    // Método REQUERIDO por IDropHandler
    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("¡Drop detectado!"); // Para verificar en consola

        if (eventData.pointerDrag != null &&
           eventData.pointerDrag.CompareTag("BolsaBasura"))
        {
            imagenBolsaAbierta.SetActive(true);
            Debug.Log("¡Bolsa correcta!");
        }
    }
}