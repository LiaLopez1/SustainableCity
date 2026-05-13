using UnityEngine;

public class PollutionFogController : MonoBehaviour
{
    [Header("Valores de niebla")]
    [Header("Partículas de niebla")]
    public ParticleSystem fogParticles;

    public float maxParticleRate = 300f;

    private float initialDensity = 0.09f;
    private float targetDensity = 0.0f;
    private float reductionSpeed = 0.01f;

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

    void Update()
    {
        if (currentDensity > targetDensity)
        {
            currentDensity -= reductionSpeed * Time.deltaTime;
            currentDensity = Mathf.Max(currentDensity, targetDensity);
            RenderSettings.fogDensity = currentDensity;
        }

    }

    // Puedes llamar esta funci�n para aumentar la niebla si algo contamina el ambiente
    public void IncreasePollution(float amount)
    {
        currentDensity += amount;
        currentDensity = Mathf.Min(currentDensity, initialDensity);
        RenderSettings.fogDensity = currentDensity;
    }

    public void SetFogDensityByContamination(float contaminationLevel)
    {
        contaminationLevel = Mathf.Clamp01(contaminationLevel);

        // ---------- Fog clásico ----------
        currentDensity = initialDensity * contaminationLevel;

        RenderSettings.fogDensity = currentDensity;

        RenderSettings.fog = currentDensity > 0.0001f;

        // ---------- Partículas ----------
        if (fogParticles != null)
        {
            var emission = fogParticles.emission;

            emission.rateOverTime = maxParticleRate * contaminationLevel;

            // Detener completamente si llega a 0
            if (contaminationLevel <= 0.001f)
            {
                fogParticles.Stop();
            }
            else if (!fogParticles.isPlaying)
            {
                fogParticles.Play();
            }
        }
    }

}
