using UnityEngine;

public class PollutionFogController : MonoBehaviour
{
    [Header("Valores de niebla")]
    public float initialDensity = 0.06f;
    public float targetDensity = 0.0f;
    public float reductionSpeed = 0.01f;

    private float currentDensity;

    void Start()
    {
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(140f / 255f, 121f / 255f, 0f); // #8C7900
        RenderSettings.fogMode = FogMode.Exponential;
        currentDensity = initialDensity;
        RenderSettings.fogDensity = currentDensity;
    }

    void Update()
    {
        if (currentDensity > targetDensity)
        {
            currentDensity -= reductionSpeed * Time.deltaTime;
            currentDensity = Mathf.Max(currentDensity, targetDensity);
            RenderSettings.fogDensity = currentDensity;
        }
    }

    // Puedes llamar esta función para aumentar la niebla si algo contamina el ambiente
    public void IncreasePollution(float amount)
    {
        currentDensity += amount;
        currentDensity = Mathf.Min(currentDensity, initialDensity);
        RenderSettings.fogDensity = currentDensity;
    }
}
