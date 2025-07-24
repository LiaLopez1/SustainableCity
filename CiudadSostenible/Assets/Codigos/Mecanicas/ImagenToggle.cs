using UnityEngine;
using UnityEngine.UI;

public class ImagenToggle : MonoBehaviour
{
    public Image imagenUI;

    private void Start()
    {
        ImagenToggleManager.Instance?.Registrar(this);
    }

    public void ToggleImagen()
    {
        if (imagenUI != null)
        {
            ImagenToggleManager.Instance?.ToggleImagen(this);
        }
    }

    public void ActivarImagen()
    {
        if (imagenUI != null)
            imagenUI.gameObject.SetActive(true);
    }

    public void DesactivarImagen()
    {
        if (imagenUI != null)
            imagenUI.gameObject.SetActive(false);
    }
}
