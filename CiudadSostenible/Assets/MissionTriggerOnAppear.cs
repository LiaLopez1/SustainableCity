using UnityEngine;

public class MissionTriggerOnAppear : MonoBehaviour
{
    [Tooltip("Clave que representa este objeto en el sistema de misiones")]
    public string missionKey; // Ejemplo: "arbol_instanciado"

    private bool yaNotificado = false;

    void OnEnable()
    {
        if (yaNotificado) return;

        MissionManager manager = FindObjectOfType<MissionManager>();
        if (manager != null)
        {
            manager.AgregarProgresoPorClave(missionKey);
            yaNotificado = true;
        }
    }
}
