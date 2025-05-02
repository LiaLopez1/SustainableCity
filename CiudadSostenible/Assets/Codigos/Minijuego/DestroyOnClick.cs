using UnityEngine;

public class DestroyOnClick : MonoBehaviour
{
    private void OnMouseDown()
    {
        Destroy(gameObject);
    }
}
