using UnityEngine;
using UnityEngine.SceneManagement;

public class VolverAlMenuPrincipal : MonoBehaviour
{
    [Tooltip("Nombre de la escena del menú principal (debe estar en Build Settings)")]
    public string nombreEscenaMenu = "MenuPrincipal";

    public void IrAlMenu()
    {
        SceneManager.LoadScene(nombreEscenaMenu);
    }
}
