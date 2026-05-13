using UnityEngine;

public class PollutionFogController : MonoBehaviour
{
    [Header("Valores de niebla")]
    public float initialDensity = 0.09f;
    public float targetDensity = 0.0f;
    public float reductionSpeed = 0.01f;

    private float currentDensity;

    void Start()
    {
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(140f / 255f, 121f / 255f, 0f); // #8C7900
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 0f;
        RenderSettings.fogEndDistance = 300f;

        currentDensity = initialDensity;
        RenderSettings.fogDensity = currentDensity;
    }

    /*void Update()
    {
        if (currentDensity > targetDensity)
        {
            currentDensity -= reductionSpeed * Time.deltaTime;
            currentDensity = Mathf.Max(currentDensity, targetDensity);
            RenderSettings.fogDensity = currentDensity;
        }

    }*/

    // Puedes llamar esta funci�n para aumentar la niebla si algo contamina el ambiente
    public void IncreasePollution(float amount)
    {
        currentDensity += amount;
        currentDensity = Mathf.Min(currentDensity, initialDensity);
        RenderSettings.fogDensity = currentDensity;
    }

   public void SetFogDensityByContamination(float contaminationLevel)
    {
        // Asegurar rango 0-1
        contaminationLevel = Mathf.Clamp01(contaminationLevel);

        // Convertir contaminación a densidad
        currentDensity = Mathf.Lerp(0f, initialDensity, contaminationLevel);

        // Aplicar densidad
        RenderSettings.fogDensity = currentDensity;

        // Apagar completamente la niebla cuando llegue a 0
        RenderSettings.fog = currentDensity > 0.0001f;

        Debug.Log("Fog Density: " + currentDensity +
                " | Contaminación: " + contaminationLevel);
    }

}
