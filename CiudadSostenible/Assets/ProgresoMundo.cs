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

    void Start()
    {
        // Configuración inicial del slider
        if (sliderContaminacion != null)
        {
            fillImage = sliderContaminacion.fillRect?.GetComponent<Image>();
            sliderContaminacion.minValue = 0f;
            sliderContaminacion.maxValue = 1f;
        }

        // Inicializar items de tienda
        foreach (var item in shopItems)
        {
            if (item.itemButton != null)
            {
                item.itemButton.interactable = false;
            }
        }

        // Inicializar máquinas (desactivar todos los CameraSwitch)
        foreach (var maquina in maquinas)
        {
            foreach (var cameraSwitch in maquina.cameraSwitches)
            {
                if (cameraSwitch != null)
                {
                    // Saltar si está en la lista de siempre activas
                    if (maquinasSiempreDesbloqueadas.Contains(cameraSwitch.gameObject))
                    {
                        continue;
                    }

                    cameraSwitch.enabled = false;
                }
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
        if (missionManager == null || sliderContaminacion == null || totalMisiones <= 0) return;

        int completadas = missionManager.misionesCompletadas;
        float valorSlider = Mathf.Clamp01(1f - (float)completadas / totalMisiones);

        // Actualizar slider
        sliderContaminacion.value = valorSlider;

        // Actualizar colores del slider
        if (fillImage != null)
        {
            fillImage.color = valorSlider <= 0.33f ? colorNormal :
                            valorSlider <= 0.66f ? colorAdvertencia :
                            colorPeligro;
        }

        // Actualizar imagen de estado
        if (imagenEstado != null)
        {
            imagenEstado.sprite = valorSlider <= 0.33f ? imagenNormal :
                                 valorSlider <= 0.66f ? imagenAdvertencia :
                                 imagenPeligro;
        }

        // Actualizar items de tienda
        foreach (var item in shopItems)
        {
            if (!item.desbloqueado && completadas >= item.misionRequerida)
            {
                DesbloquearItem(item);
            }
        }

        // Actualizar máquinas
        foreach (var maquina in maquinas)
        {
            if (!maquina.desbloqueado && completadas >= maquina.misionRequerida)
            {
                DesbloquearMaquina(maquina);
            }
        }

        if (fogController != null)
        {
            fogController.SetFogDensityByContamination(valorSlider); // valorSlider ya representa la contaminación
        }

        if (!mapaFinalActivado && completadas >= totalMisiones)
        {
            mapaFinalActivado = true;

            if (mapaViejo != null) mapaViejo.SetActive(false);
            if (mapaNuevo != null) mapaNuevo.SetActive(true);
            if (panelFinal != null) panelFinal.SetActive(true);
        }

        ActualizarSpawnerDeBasura(completadas);

    }

    void DesbloquearItem(ShopItem item)
    {
        item.desbloqueado = true;

        if (item.itemButton != null)
        {
            item.itemButton.interactable = true;
        }
    }

    void DesbloquearMaquina(MaquinaInteractuable maquina)
    {
        maquina.desbloqueado = true;

        foreach (var cameraSwitch in maquina.cameraSwitches)
        {
            if (cameraSwitch != null)
            {
                cameraSwitch.enabled = true;
                Debug.Log($"Máquina desbloqueada! Script CameraSwitch activado en {cameraSwitch.gameObject.name}");
            }
        }
    }

    public bool MaquinaEstaDesbloqueada(GameObject maquinaGO)
    {
        // Revisión por misión completada
        foreach (var maquina in maquinas)
        {
            if (maquina.desbloqueado && maquina.cameraSwitches.Exists(cs => cs.gameObject == maquinaGO))
            {
                return true;
            }
        }

        // Revisión por lista de siempre activas
        if (maquinasSiempreDesbloqueadas != null && maquinasSiempreDesbloqueadas.Contains(maquinaGO))
        {
            return true;
        }

        return false;
    }


    void ActualizarSpawnerDeBasura(int misionesCompletadas)
    {
        if (basuraSpawner == null || basuraSpawner.tiposBasura == null) return;

        if (misionesCompletadas < 4)
        {
            AsignarProbabilidades(new float[] { 70f, 15f, 15f, 0f }); 
            basuraSpawner.cantidadMaximaBasura = 15;
        }
        
        else if (misionesCompletadas < 8)
        {
            AsignarProbabilidades(new float[] { 35f, 35f, 30f ,0f });
            basuraSpawner.cantidadMaximaBasura = 20;
        }
        
        else if (misionesCompletadas < 11)
        {
            AsignarProbabilidades(new float[] { 22f, 22f, 22f, 34f });
            basuraSpawner.cantidadMaximaBasura = 15;
        }

        else if (misionesCompletadas < 23)
        {
            AsignarProbabilidades(new float[] { 25f, 25f,25f , 25f });
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
        {
            if (i < nuevasProbs.Length)
                basuraSpawner.tiposBasura[i].probabilidad = nuevasProbs[i];
            else
                basuraSpawner.tiposBasura[i].probabilidad = 0f; // explícito
        }
    }

    void AsignarProbabilidadesUniformes()
    {
        int totalTipos = basuraSpawner.tiposBasura.Count;
        float prob = 100f / totalTipos;

        foreach (var tipo in basuraSpawner.tiposBasura)
        {
            tipo.probabilidad = prob;
        }
    }

}