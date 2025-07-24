using System.Collections.Generic;
using UnityEngine;

public class ImagenToggleManager : MonoBehaviour
{
    public static ImagenToggleManager Instance { get; private set; }

    private List<ImagenToggle> imagenToggles = new List<ImagenToggle>();
    private ImagenToggle imagenActiva = null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void Registrar(ImagenToggle toggle)
    {
        if (!imagenToggles.Contains(toggle))
        {
            imagenToggles.Add(toggle);
        }
    }

    public void ToggleImagen(ImagenToggle toggle)
    {
        // Si el mismo botón intenta cerrar su imagen
        if (imagenActiva == toggle)
        {
            toggle.DesactivarImagen();
            imagenActiva = null;
            return;
        }

        // Desactivar la anterior
        if (imagenActiva != null)
        {
            imagenActiva.DesactivarImagen();
        }

        // Activar la nueva
        toggle.ActivarImagen();
        imagenActiva = toggle;
    }

    public void CerrarTodo()
    {
        foreach (var toggle in imagenToggles)
        {
            toggle.DesactivarImagen();
        }
        imagenActiva = null;
    }
}
