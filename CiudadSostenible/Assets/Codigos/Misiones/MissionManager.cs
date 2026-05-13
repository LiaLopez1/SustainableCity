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

    [Header("Misiones")]
    public List<Mission> misiones;

    [Header("HUD")]
    public TextMeshProUGUI missionTitleHUD;     // Para el título: "MISSION: ..."
    public TextMeshProUGUI missionProgressHUD;  // Para progreso "x/y" por requisito

    [Header("Progreso global")]
    [Tooltip("Cantidad de misiones ya cumplidas al iniciar. Si pones 3, se inician completas 0,1,2 y arrancas en la 3.")]
    public int misionesCompletadas = 0;

    // public GameObject triggerBloqueoMaquina; // Ejemplo para desbloqueos condicionales

    private int misionActualIndex = 0;
    private MissionUIController uiController;

    #region Ciclo de vida
    void Start()
    {
        uiController = FindObjectOfType<MissionUIController>();

        // 1) Aplica las 'misionesCompletadas' que definas en el Inspector (clave para pruebas)
        if (misionesCompletadas > 0)
        {
            // true = reiniciar progreso de la misión actual (primera no completada)
            AplicarMisionesCompletadas(misionesCompletadas, reiniciarProgresoDeLaActual: true);
        }

        // 2) Además, respeta las que fueron marcadas "completas desde el editor" (consecutivas)
        while (misionActualIndex < misiones.Count && misiones[misionActualIndex].marcarComoCompletaDesdeEditor)
        {
            var m = misiones[misionActualIndex];
            m.completada = true;

            // (Opcional) Llevar contadores al objetivo por coherencia visual
            foreach (var r in m.requisitos) r.cantidadActual = r.cantidadObjetivo;
            foreach (var c in m.clavesRequeridas) c.cantidadActual = c.cantidadObjetivo;

            misionActualIndex++;
            misionesCompletadas = Mathf.Max(misionesCompletadas, misionActualIndex);
        }

        // 3) Muestra lo que haya quedado como misión actual (o mensaje de todo completado)
        MostrarMisionActual();
        if (uiController != null) uiController.ActualizarExclamacion();
    }
    #endregion

    #region API de progreso (ítems / claves)
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

        if (!huboProgreso) return;

        if (MisionCompleta(misionActual))
        {
            CompletarYAvanzar();
        }
        else
        {
            ActualizarTextoHUD();
        }
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

        if (!huboProgreso) return;

        if (MisionCompleta(misionActual))
        {
            CompletarYAvanzar();
        }
        else
        {
            ActualizarTextoHUD();
        }
    }
    #endregion

    #region Lógica de misión / UI
    private bool MisionCompleta(Mission m)
    {
        // Ítems
        foreach (var req in m.requisitos)
            if (req.cantidadActual < req.cantidadObjetivo) return false;

        // Claves
        foreach (var req in m.clavesRequeridas)
            if (req.cantidadActual < req.cantidadObjetivo) return false;

        return true;
    }

    private void CompletarYAvanzar()
    {
        var m = misiones[misionActualIndex];
        m.completada = true;
        misionesCompletadas++;
        misionActualIndex++;

        MostrarMisionActual();
        if (uiController != null) uiController.ActualizarExclamacion();

        // VerificarDesbloqueos();
    }

    private void MostrarMisionActual()
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
                // Muestra números de progreso (si ya fue "revelada")
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
                // Oculta números si aún no se "mostró" al jugador
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
    #endregion

    #region Helpers de control y pruebas
    /// <summary>
    /// Marca completas las primeras 'n' misiones y sitúa el índice en la siguiente.
    /// Si 'reiniciarProgresoDeLaActual' es true, limpia contadores de la misión actual.
    /// </summary>
    public void AplicarMisionesCompletadas(int n, bool reiniciarProgresoDeLaActual = true)
    {
        if (misiones == null || misiones.Count == 0) return;

        n = Mathf.Clamp(n, 0, misiones.Count);

        // Marca completas 0..n-1
        for (int i = 0; i < n; i++)
        {
            var m = misiones[i];
            m.completada = true;

            // (Opcional) Deja contadores al tope para coherencia visual si se consulta su UI
            foreach (var r in m.requisitos) r.cantidadActual = r.cantidadObjetivo;
            foreach (var c in m.clavesRequeridas) c.cantidadActual = c.cantidadObjetivo;
        }

        // Posiciónate en la siguiente
        misionActualIndex = n;
        misionesCompletadas = n;

        // Si existe misión actual, puedes empezarla "limpia"
        if (misionActualIndex < misiones.Count && reiniciarProgresoDeLaActual)
        {
            ResetProgreso(misiones[misionActualIndex]);
        }

        // Refresca pantallas
        MostrarMisionActual();
        if (uiController != null) uiController.ActualizarExclamacion();
    }

    /// <summary>
    /// Cambia en tiempo de ejecución cuántas misiones se consideran cumplidas,
    /// reubicando el índice y refrescando la UI.
    /// </summary>
    public void SetMisionesCompletadasEnRuntime(int n, bool reiniciarProgresoDeLaActual = true)
    {
        AplicarMisionesCompletadas(n, reiniciarProgresoDeLaActual);
    }

    [ContextMenu("Aplicar progreso global")]
    public void AplicarProgresoGlobalDesdeInspector()
    {
        AplicarMisionesCompletadas(misionesCompletadas, true);
    }

    /// <summary>
    /// Limpia contadores y marca una misión como no completada.
    /// </summary>
    private void ResetProgreso(Mission m)
    {
        foreach (var r in m.requisitos) r.cantidadActual = 0;
        foreach (var c in m.clavesRequeridas) c.cantidadActual = 0;
        m.completada = false;

        // Si prefieres ocultar progreso numérico hasta revelar:
        // m.fueMostradaAlJugador = false;
    }

    // Ejemplo de verificación de desbloqueos
    /*
    void VerificarDesbloqueos()
    {
        if (misionActualIndex > 0 && triggerBloqueoMaquina != null)
        {
            triggerBloqueoMaquina.SetActive(false);
        }
    }
    */
    #endregion
}
