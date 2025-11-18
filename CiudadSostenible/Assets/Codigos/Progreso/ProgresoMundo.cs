using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ProgresoMundo : MonoBehaviour
{
    [Header("Referencias")]
    public MissionManager missionManager;
    public Slider sliderContaminacion;
    public Image imagenEstado;

    [Header("Niebla")]
    public PollutionFogController fogController;

    [Header("Control de Basura")]
    public BasuraSpawner basuraSpawner;

    [Header("Configuración")]
    public int totalMisiones = 15;

    [Header("Cambio de mapa final")]
    public GameObject mapaViejo;
    public GameObject mapaNuevo;

    [Header("Panel final al completar todas las misiones")]
    public GameObject panelFinal;

    [Header("Misiones donde ocurren estos eventos")]
    [Tooltip("Número de misiones completadas necesarias para cambiar de mapa.")]
    public int misionCambioMapa = 15;

    [Tooltip("Número de misiones completadas necesarias para mostrar el panel final.")]
    public int misionPanelFinal = 15;

    [Header("Colores del slider")]
    public Color colorNormal;
    public Color colorAdvertencia;
    public Color colorPeligro;

    [Header("Sprites de estado")]
    public Sprite imagenNormal;
    public Sprite imagenAdvertencia;
    public Sprite imagenPeligro;

    [Header("Items de Tienda")]
    public List<ShopItem> shopItems;

    [Header("Máquinas siempre activas")]
    public List<GameObject> maquinasSiempreDesbloqueadas;

    [Header("Máquinas Interactuables")]
    public GameObject panelCompartido;
    public List<MaquinaInteractuable> maquinas;

    [Header("Productos 3D a controlar")]
    public List<Product3DEntry> productos3D;

    private Image fillImage;
    private bool mapaFinalActivado = false;
    private int _layerIgnoreRaycast = -1;

    // Cache de renderers objetivo por producto (padre o hijos)
    private readonly Dictionary<Product3DEntry, List<Renderer>> _objetivoRenderers = new();

    void Awake()
    {
        // Calcula el índice una vez ya en runtime (permitido)
        _layerIgnoreRaycast = LayerMask.NameToLayer("Ignore Raycast");
    }

    [System.Serializable]
    public class ShopItem
    {
        public Button itemButton;
        public int misionRequerida;
        public Color colorBloqueado = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        [HideInInspector] public bool desbloqueado;
    }

    [System.Serializable]
    public class MaquinaInteractuable
    {
        [Tooltip("Lista de todos los scripts CameraSwitch de esta máquina")]
        public List<CameraSwitch> cameraSwitches;
        public int misionRequerida;
        [HideInInspector] public bool desbloqueado;
    }

    [System.Serializable]
    public class Product3DEntry
    {
        [Header("Producto base")]
        public ProductoComprable3D producto;
        public int misionRequerida = 1;

        [Header("Materiales")]
        public Material materialNormal;
        public Material materialBloqueado;

        [Header("Bloqueo por imagen (reemplaza texto)")]
        [Tooltip("Image (UI) que se mostrará cuando el producto esté bloqueado (Canvas world-space recomendado).")]
        public Image imagenBloqueado;
        [Tooltip("Sprite opcional para el icono de bloqueo (si se deja vacío, se usa el sprite ya asignado al Image).")]
        public Sprite spriteBloqueado;

        [Header("¿El objeto asignado es un PADRE?")]
        [Tooltip("Marca si el 'producto' es un contenedor y los modelos reales están en sus hijos.")]
        public bool esPadre = false;

        [Tooltip("Si está activo, se buscan automaticamente los Renderers en los hijos.")]
        public bool autoDetectarHijos = true;

        [Tooltip("Si es > 0 y autoDetectarHijos está activo, limita a los primeros N hijos con Renderer encontrados.")]
        public int cantidadHijos = 0; // 0 = todos

        [Tooltip("Si NO quieres autodetectar, arrastra aquí los Renderers de los hijos a afectar.")]
        public List<Renderer> hijosRenderers = new List<Renderer>();

        [HideInInspector] public bool _cached; // interno
        [HideInInspector] public List<Collider> _colliders = new List<Collider>();
        [HideInInspector] public int _originalLayer = -1;

    }

    void Start()
    {
        // Slider inicial
        if (sliderContaminacion != null)
        {
            fillImage = sliderContaminacion.fillRect?.GetComponent<Image>();
            sliderContaminacion.minValue = 0f;
            sliderContaminacion.maxValue = 1f;
        }

        // Inicializar items de tienda
        foreach (var item in shopItems)
            if (item.itemButton != null)
                item.itemButton.interactable = false;

        // Inicializar máquinas
        foreach (var maquina in maquinas)
            foreach (var cs in maquina.cameraSwitches)
                if (cs != null && !maquinasSiempreDesbloqueadas.Contains(cs.gameObject))
                    cs.enabled = false;

        // Inicializar productos 3D (bloqueados por defecto) + cache de renderers objetivo
        foreach (var entry in productos3D)
        {
            var prod = entry.producto;
            if (prod == null) continue;
            prod.desbloqueado = false;

            // Cachear renderers objetivo
            _objetivoRenderers[entry] = CalcularRenderersObjetivo(entry);

            // Configurar imagen de bloqueo (oculta al inicio)
            if (entry.imagenBloqueado != null)
            {
                if (entry.spriteBloqueado != null)
                    entry.imagenBloqueado.sprite = entry.spriteBloqueado;
                entry.imagenBloqueado.enabled = false;
                entry.imagenBloqueado.gameObject.SetActive(false);
            }

            // Aplicar material bloqueado al inicio
            AplicarMaterial(entry, estaDesbloqueado: false);

            // Cachear colliders del producto (padre + hijos)
            entry._colliders.Clear();
            if (entry.producto != null)
            {
                entry._colliders.AddRange(entry.producto.GetComponentsInChildren<Collider>(true));
                if (entry._originalLayer == -1) entry._originalLayer = entry.producto.gameObject.layer;
            }

            // Arrancan bloqueados visualmente…
            prod.desbloqueado = false;
            // …y también sin raycast/click
            AplicarInteractuable(entry, false);

        }

        ActualizarUI();
    }

    void Update()
    {
        ActualizarUI();
    }

    void ActualizarUI()
    {
        if (missionManager == null || sliderContaminacion == null || totalMisiones <= 0)
            return;

        int completadas = missionManager.misionesCompletadas;
        float valorSlider = Mathf.Clamp01(1f - (float)completadas / totalMisiones);

        // Slider y colores
        sliderContaminacion.value = valorSlider;
        if (fillImage != null)
            fillImage.color = valorSlider <= 0.33f ? colorNormal : valorSlider <= 0.66f ? colorAdvertencia : colorPeligro;

        // Imagen de estado
        if (imagenEstado != null)
            imagenEstado.sprite = valorSlider <= 0.33f ? imagenNormal : valorSlider <= 0.66f ? imagenAdvertencia : imagenPeligro;

        // Desbloquear items de tienda
        foreach (var item in shopItems)
            if (!item.desbloqueado && completadas >= item.misionRequerida)
                DesbloquearItem(item);

        // Desbloquear máquinas
        foreach (var maquina in maquinas)
            if (!maquina.desbloqueado && completadas >= maquina.misionRequerida)
                DesbloquearMaquina(maquina);

        // Controlar productos 3D: material y visibilidad de imagen de bloqueo
        foreach (var entry in productos3D)
        {
            var prod = entry.producto;
            if (prod == null) continue;

            bool shouldUnlock = completadas >= entry.misionRequerida;
            prod.desbloqueado = shouldUnlock;

            AplicarMaterial(entry, shouldUnlock);

            // Mostrar/ocultar imagen de bloqueo según cámara activa y estado
            if (entry.imagenBloqueado != null)
            {
                bool camaraParentActive = prod.cameraInteractiva != null &&
                    prod.cameraInteractiva.transform.parent != null &&
                    prod.cameraInteractiva.transform.parent.gameObject.activeInHierarchy;

                bool mostrar = !shouldUnlock && camaraParentActive;

                entry.imagenBloqueado.enabled = mostrar;
                entry.imagenBloqueado.gameObject.SetActive(mostrar);
            }

            AplicarInteractuable(entry, shouldUnlock);
        }

        // Niebla
        if (fogController != null)
            fogController.SetFogDensityByContamination(valorSlider);

        // Cambio de mapa final
        // Cambio de mapa (controlado por misión específica)
        if (!mapaFinalActivado && completadas >= misionCambioMapa)
        {
            mapaFinalActivado = true;
            if (mapaViejo != null) mapaViejo.SetActive(false);
            if (mapaNuevo != null) mapaNuevo.SetActive(true);
        }

        // Panel final (controlado por misión específica)
        if (panelFinal != null && completadas >= misionPanelFinal)
        {
            panelFinal.SetActive(true);
        }


        // Actualizar spawner de basura
        ActualizarSpawnerDeBasura(completadas);
    }

    // ---------- Helpers de productos 3D ----------

    private List<Renderer> CalcularRenderersObjetivo(Product3DEntry entry)
    {
        var lista = new List<Renderer>();
        if (entry.producto == null) return lista;

        if (!entry.esPadre)
        {
            var r = entry.producto.GetComponent<Renderer>();
            if (r != null) lista.Add(r);
            return lista;
        }

        // Si es padre:
        if (!entry.autoDetectarHijos)
        {
            // Usar los que el diseñador asigne manualmente
            foreach (var r in entry.hijosRenderers)
                if (r != null) lista.Add(r);
            return lista;
        }

        // Autodetectar hijos con Renderer
        var todos = entry.producto.GetComponentsInChildren<Renderer>(true);
        foreach (var r in todos)
        {
            // Opcional: si NO quieres incluir el renderer del padre, filtra:
            if (r.gameObject == entry.producto.gameObject) continue;
            lista.Add(r);
        }

        // Limitar a N si cantidadHijos > 0
        if (entry.cantidadHijos > 0 && lista.Count > entry.cantidadHijos)
            lista = lista.GetRange(0, entry.cantidadHijos);

        return lista;
    }

    private void AplicarMaterial(Product3DEntry entry, bool estaDesbloqueado)
    {
        if (!_objetivoRenderers.TryGetValue(entry, out var renderers) || renderers == null)
        {
            renderers = CalcularRenderersObjetivo(entry);
            _objetivoRenderers[entry] = renderers;
        }

        var mat = estaDesbloqueado ? entry.materialNormal : entry.materialBloqueado;
        if (mat == null) return;

        // Aplicar a todos los renderers objetivo
        for (int i = 0; i < renderers.Count; i++)
        {
            var r = renderers[i];
            if (r == null) continue;

            // Usamos sharedMaterial para evitar instancias en tiempo de juego (puedes cambiar a .material si necesitas copias por objeto)
            r.sharedMaterial = mat;
        }
    }

    void DesbloquearItem(ShopItem item)
    {
        item.desbloqueado = true;
        if (item.itemButton != null)
            item.itemButton.interactable = true;
    }

    void DesbloquearMaquina(MaquinaInteractuable maquina)
    {
        maquina.desbloqueado = true;
        foreach (var cs in maquina.cameraSwitches)
        {
            if (cs != null)
            {
                cs.enabled = true;
                Debug.Log($"Máquina desbloqueada! Script CameraSwitch activado en {cs.gameObject.name}");
            }
        }
    }

    public bool MaquinaEstaDesbloqueada(GameObject maquinaGO)
    {
        foreach (var maquina in maquinas)
            if (maquina.desbloqueado && maquina.cameraSwitches.Exists(cs => cs.gameObject == maquinaGO))
                return true;
        return maquinasSiempreDesbloqueadas != null && maquinasSiempreDesbloqueadas.Contains(maquinaGO);
    }

    void ActualizarSpawnerDeBasura(int misionesCompletadas)
    {
        if (basuraSpawner == null || basuraSpawner.tiposBasura == null) return;

        if (misionesCompletadas < 4)
        {
            AsignarProbabilidades(new float[] { 70f, 20f, 10f, 0f });
            basuraSpawner.cantidadMaximaBasura = 20;
        }
        else if (misionesCompletadas < 8)
        {
            AsignarProbabilidades(new float[] { 20f, 70f, 10f, 0f });
            basuraSpawner.cantidadMaximaBasura = 18;
        }
        else if (misionesCompletadas < 11)
        {
            AsignarProbabilidades(new float[] { 20f, 20f, 60f, 0f });
            basuraSpawner.cantidadMaximaBasura = 15;
        }
        else if (misionesCompletadas < 13)
        {
            AsignarProbabilidades(new float[] { 5f, 5f, 5f, 85f });
            basuraSpawner.cantidadMaximaBasura = 15;
        }
        else if (misionesCompletadas < 16)
        {
            AsignarProbabilidades(new float[] { 22f, 22f, 22f, 34f });
            basuraSpawner.cantidadMaximaBasura = 15;
        }
        else if (misionesCompletadas < 25)
        {
            AsignarProbabilidades(new float[] { 40f, 20f, 25f, 15f });
            basuraSpawner.cantidadMaximaBasura = 10;
        }
        else
        {
            basuraSpawner.cantidadMaximaBasura = 0;
        }
    }

    void AsignarProbabilidades(float[] nuevasProbs)
    {
        for (int i = 0; i < basuraSpawner.tiposBasura.Count; i++)
            basuraSpawner.tiposBasura[i].probabilidad = i < nuevasProbs.Length ? nuevasProbs[i] : 0f;
    }

    void AsignarProbabilidadesUniformes()
    {
        int totalTipos = basuraSpawner.tiposBasura.Count;
        float prob = 100f / totalTipos;
        foreach (var tipo in basuraSpawner.tiposBasura)
            tipo.probabilidad = prob;
    }

    private void AplicarInteractuable(Product3DEntry entry, bool enable)
    {
        if (entry == null || entry.producto == null) return;

        // Colliders ON/OFF
        if (entry._colliders != null)
        {
            for (int i = 0; i < entry._colliders.Count; i++)
            {
                var c = entry._colliders[i];
                if (c != null) c.enabled = enable;
            }
        }

        // Capa: usar Ignore Raycast al bloquear (solo si existe la capa)
        var go = entry.producto.gameObject;

        if (!enable)
        {
            if (_layerIgnoreRaycast != -1)
                go.layer = _layerIgnoreRaycast; // Bloqueado => ignora raycast
                                                // si -1, deja la capa actual (no hacemos nada)
        }
        else
        {
            if (entry._originalLayer >= 0)
                go.layer = entry._originalLayer; // Restaurar capa original
        }
    }

}
