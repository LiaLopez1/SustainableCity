using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

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
        public ProductoComprable3D producto;
        public int misionRequerida = 1;
        public Material materialNormal;
        public Material materialBloqueado;
        public TMP_Text textBloqueado;
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

        // Inicializar productos 3D (bloqueados por defecto)
        foreach (var entry in productos3D)
        {
            var prod = entry.producto;
            if (prod == null) continue;
            prod.desbloqueado = false;
            var rend = prod.GetComponent<Renderer>();
            if (entry.materialBloqueado != null)
                rend.material = entry.materialBloqueado;
            if (entry.textBloqueado != null)
            {
                entry.textBloqueado.text = $"Bloqueado\nMisión {entry.misionRequerida}";
                entry.textBloqueado.gameObject.SetActive(false);  // oculto inicialmente
            }
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

        // Desbloquear productos 3D y controlar visibilidad de texto bloqueado
        foreach (var entry in productos3D)
        {
            var prod = entry.producto;
            if (prod == null) continue;
            bool shouldUnlock = completadas >= entry.misionRequerida;
            prod.desbloqueado = shouldUnlock;
            var rend = prod.GetComponent<Renderer>();
            if (shouldUnlock)
            {
                if (entry.materialNormal != null)
                    rend.material = entry.materialNormal;
            }
            else
            {
                if (entry.materialBloqueado != null)
                    rend.material = entry.materialBloqueado;
            }

            if (entry.textBloqueado != null)
            {
                bool camaraParentActive = prod.cameraInteractiva != null &&
                    prod.cameraInteractiva.transform.parent != null &&
                    prod.cameraInteractiva.transform.parent.gameObject.activeInHierarchy;
                bool mostrar = !shouldUnlock && camaraParentActive;
                entry.textBloqueado.gameObject.SetActive(mostrar);
                if (mostrar)
                    entry.textBloqueado.text = $"Bloqueado\nMisión {entry.misionRequerida}";
            }
        }

        // Niebla
        if (fogController != null)
            fogController.SetFogDensityByContamination(valorSlider);

        // Cambio de mapa final
        if (!mapaFinalActivado && completadas >= totalMisiones)
        {
            mapaFinalActivado = true;
            if (mapaViejo != null) mapaViejo.SetActive(false);
            if (mapaNuevo != null) mapaNuevo.SetActive(true);
            if (panelFinal != null) panelFinal.SetActive(true);
        }

        // Actualizar spawner de basura
        ActualizarSpawnerDeBasura(completadas);
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
}
