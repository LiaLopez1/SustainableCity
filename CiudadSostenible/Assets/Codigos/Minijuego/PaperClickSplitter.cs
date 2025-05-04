using UnityEngine;

public class PaperClickSplitter : MonoBehaviour
{
    private int currentStep = 0;
    private Transform[] tiras;
    private float[] desplazamientos = { 0.20f, 0.13f, 0.06f };

    void Start()
    {
        tiras = new Transform[3];
        tiras[0] = transform.Find("Tira4");
        tiras[1] = transform.Find("Tira3");
        tiras[2] = transform.Find("Tira2");
    }

    void OnMouseDown()
    {
        if (currentStep < tiras.Length)
        {
            Transform tira = tiras[currentStep];
            if (tira != null)
            {
                float offset = desplazamientos[currentStep];
                tira.position += Vector3.forward * offset;
            }
            currentStep++;
        }
        else
        {
            Debug.Log("✅ 3 tiras separadas, Tira1 permanece intacta.");
        }
    }
}
