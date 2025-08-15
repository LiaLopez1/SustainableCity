using UnityEngine;

public class BellPopPlayer : MonoBehaviour
{
    [SerializeField] private RectTransform bellRect;
    [SerializeField] private float popScaleUp = 1.18f;
    [SerializeField] private float popTimeUp = 0.12f;
    [SerializeField] private float popTimeDown = 0.12f;

    private Coroutine routine;

    void Awake()
    {
        if (bellRect == null) bellRect = GetComponent<RectTransform>();
    }

    public void Play()
    {
        if (!isActiveAndEnabled) return; // seguridad
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(Pop());
    }

    private System.Collections.IEnumerator Pop()
    {
        Vector3 baseS = Vector3.one;
        Vector3 upS = Vector3.one * Mathf.Max(1f, popScaleUp);

        float t = 0f;
        while (t < popTimeUp)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / popTimeUp);
            float e = k * k * (3f - 2f * k);
            bellRect.localScale = Vector3.LerpUnclamped(baseS, upS, e);
            yield return null;
        }

        t = 0f;
        while (t < popTimeDown)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / popTimeDown);
            float e = k * k * (3f - 2f * k);
            bellRect.localScale = Vector3.LerpUnclamped(upS, baseS, e);
            yield return null;
        }

        bellRect.localScale = baseS;
        routine = null;
    }
}
