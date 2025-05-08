using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MissionManager : MonoBehaviour
{
    [System.Serializable]
    public class Mission
    {
        public string nombreMision;
        [TextArea] public string descripcion;

        [System.Serializable]
        public class RequisitoItem
        {
            public ItemData item;
            public int cantidadObjetivo;
            [HideInInspector] public int cantidadActual = 0;
        }

        public List<RequisitoItem> requisitos = new List<RequisitoItem>();

        [HideInInspector] public bool completada = false;
        public bool marcarComoCompletaDesdeEditor = false;

        [HideInInspector] public bool fueMostradaAlJugador = false;

        [System.Serializable]
        public class RequisitoClave
        {
            public string clave;
            public int cantidadObjetivo;
            [HideInInspector] public int cantidadActual = 0;
        }

        public List<RequisitoClave> clavesRequeridas = new List<RequisitoClave>();

    }

    public List<Mission> misiones;

    public TextMeshProUGUI missionTitleHUD; // Para "MISIÓN:"
    public TextMeshProUGUI missionProgressHUD; // Para el progreso tipo (2/5)

    public int misionesCompletadas = 0;

    //public GameObject triggerBloqueoMaquina; // Para desbloqueo condicional

    private int misionActualIndex = 0;

    private MissionUIController uiController;

    void Start()
    {
        uiController = FindObjectOfType<MissionUIController>();

        // Saltar misiones marcadas como completadas
        while (misionActualIndex < misiones.Count && misiones[misionActualIndex].marcarComoCompletaDesdeEditor)
        {
            misiones[misionActualIndex].completada = true;
            misionActualIndex++;
        }

        MostrarMisionActual();
        if (uiController != null) uiController.ActualizarExclamacion();
    }

    public void AgregarProgreso(ItemData item, int cantidad)
    {
        if (misionActualIndex >= misiones.Count) return;

        Mission misionActual = misiones[misionActualIndex];

        if (misionActual.completada) return;

        bool huboProgreso = false;

        foreach (var req in misionActual.requisitos)
        {
            if (req.item == item)
            {
                req.cantidadActual += cantidad;
                if (req.cantidadActual > req.cantidadObjetivo)
                    req.cantidadActual = req.cantidadObjetivo;

                huboProgreso = true;
            }
        }

        if (huboProgreso)
        {
            bool todosCompletos = true;
            foreach (var req in misionActual.requisitos)
            {
                if (req.cantidadActual < req.cantidadObjetivo)
                {
                    todosCompletos = false;
                    break;
                }
            }

            if (todosCompletos)
            {
                misionActual.completada = true;
                misionesCompletadas++;
                misionActualIndex++;

                //VerificarDesbloqueos();
                MostrarMisionActual(); // Cambia a la nueva misión

                if (uiController != null)
                    uiController.ActualizarExclamacion(); // Ahora se basa en la nueva misión
            }

            else
            {
                ActualizarTextoHUD();
            }
        }
    }

    void MostrarMisionActual()
    {
        if (misionActualIndex < misiones.Count)
        {
            ActualizarTextoHUD();
        }
        else
        {
            if (missionTitleHUD != null)
                missionTitleHUD.text = "¡all the missions complete!";

            if (missionProgressHUD != null)
                missionProgressHUD.text = "";
        }
    }


    public void ActualizarTextoHUD()
    {
        if (misionActualIndex >= misiones.Count) return;

        Mission m = misiones[misionActualIndex];

        if (missionTitleHUD != null)
            missionTitleHUD.text = $"MISSION: {m.nombreMision}";

        if (missionProgressHUD != null)
        {
            if (m.fueMostradaAlJugador)
            {
                string progreso = "";
                foreach (var req in m.requisitos)
                {
                    progreso += $"{req.cantidadActual}/{req.cantidadObjetivo}\n";
                }
                foreach (var req in m.clavesRequeridas)
                {
                    progreso += $"{req.cantidadActual}/{req.cantidadObjetivo}\n";
                }
                missionProgressHUD.text = progreso;
            }
            else
            {
                missionProgressHUD.text = "";
            }
        }

    }



    public Mission ObtenerMisionActual()
    {
        if (misionActualIndex < misiones.Count)
            return misiones[misionActualIndex];
        else
            return null;
    }

    public void AgregarProgresoPorClave(string clave)
    {
        if (misionActualIndex >= misiones.Count) return;

        Mission misionActual = misiones[misionActualIndex];
        if (misionActual.completada) return;

        bool huboProgreso = false;

        foreach (var req in misionActual.clavesRequeridas)
        {
            if (req.clave == clave)
            {
                req.cantidadActual++;
                if (req.cantidadActual > req.cantidadObjetivo)
                    req.cantidadActual = req.cantidadObjetivo;

                huboProgreso = true;
            }
        }

        if (huboProgreso)
        {
            bool todosCompletos = true;

            // Verifica los ítems
            foreach (var req in misionActual.requisitos)
            {
                if (req.cantidadActual < req.cantidadObjetivo)
                {
                    todosCompletos = false;
                    break;
                }
            }

            // Verifica las claves
            foreach (var req in misionActual.clavesRequeridas)
            {
                if (req.cantidadActual < req.cantidadObjetivo)
                {
                    todosCompletos = false;
                    break;
                }
            }

            if (todosCompletos)
            {
                misionActual.completada = true;
                misionesCompletadas++;
                misionActualIndex++;

                MostrarMisionActual();
                if (uiController != null)
                    uiController.ActualizarExclamacion();
            }
            else
            {
                ActualizarTextoHUD();
            }
        }
    }


    /*void VerificarDesbloqueos()
    {
        // Ejemplo: desbloquear máquina cuando se completa la misión 0
        if (misionActualIndex > 0 && triggerBloqueoMaquina != null)
        {
            triggerBloqueoMaquina.SetActive(false);
        }
    }*/
}
