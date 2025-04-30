using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ZonaDropBolsa : MonoBehaviour, IDropHandler
{
    public GameObject imagenBolsaAbierta; // Arrastra la ImagenBolsaAbierta aquí
    public GameObject[] prefabsItems;     // Prefabs de los items aleatorios (arrástralos en el Inspector)

    public void OnDrop(PointerEventData eventData)
    {
        // Verificar si el objeto arrastrado es la bolsa (tag "BolsaBasura")
        if (eventData.pointerDrag.CompareTag("BolsaBasura"))
        {
            // Mostrar la imagen de la bolsa abierta
            imagenBolsaAbierta.SetActive(true);

            // Generar items aleatorios en los slots
            GenerarItemsAleatorios();
        }
    }

    void GenerarItemsAleatorios()
    {
        Transform[] slots = new Transform[imagenBolsaAbierta.transform.childCount];
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i] = imagenBolsaAbierta.transform.GetChild(i);

            // Limpiar el slot si ya tiene un item
            if (slots[i].childCount > 0)
            {
                Destroy(slots[i].GetChild(0).gameObject);
            }

            // Generar item aleatorio (solo si hay prefabs)
            if (prefabsItems.Length > 0)
            {
                int indexAleatorio = Random.Range(0, prefabsItems.Length);
                GameObject item = Instantiate(prefabsItems[indexAleatorio], slots[i].position, Quaternion.identity);
                item.transform.SetParent(slots[i]); // Hacerlo hijo del slot

                // Opcional: Actualizar contador (ej: si el item es stackable)
                Text contador = slots[i].GetComponentInChildren<Text>();
                if (contador != null)
                {
                    contador.text = "x" + Random.Range(1, 5).ToString(); // Número aleatorio
                }
            }
        }
    }
}