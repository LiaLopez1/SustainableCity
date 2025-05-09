using UnityEngine;
using UnityEngine.UI;

// Toggle button help
public class ImagenToggle : MonoBehaviour
{
    public Image imagenUI;

    public void ToggleImagen()
    {
        if (imagenUI != null)
        {
            imagenUI.gameObject.SetActive(!imagenUI.gameObject.activeSelf);
        }
    }
}
