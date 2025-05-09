using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ImagenTemporizador : MonoBehaviour
{
    public Image imagenUI;

    void Start()
    {
        StartCoroutine(MostrarImagenTemporal());
    }

    IEnumerator MostrarImagenTemporal()
    {
        imagenUI.gameObject.SetActive(true);        
        yield return new WaitForSeconds(12f);       
        imagenUI.gameObject.SetActive(false);      
    }
}
