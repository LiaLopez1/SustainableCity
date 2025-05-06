using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ProgresoMundo : MonoBehaviour
{
    [Header("Referencias")]
    public MissionManager missionManager;
    public Slider sliderContaminacion;
    public Image imagenEstado;

    [Header("Configuración")]
    public int totalMisiones = 15;

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

    [Header("Máquinas Interactuables")]
    public List<MaquinaInteractuable> maquinas;

    private Image fillImage;

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
}