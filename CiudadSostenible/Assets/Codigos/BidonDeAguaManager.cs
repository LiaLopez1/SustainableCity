using System.Collections.Generic;
using UnityEngine;

public class BidonDeAguaManager : MonoBehaviour
{
    public static BidonDeAguaManager Instance { get; private set; }

    private HashSet<ItemData> bidonesLlenos = new HashSet<ItemData>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    public void LlenarBidon(ItemData item)
    {
        if (item != null && !bidonesLlenos.Contains(item))
            bidonesLlenos.Add(item);
    }

    public bool EstaLleno(ItemData item)
    {
        return item != null && bidonesLlenos.Contains(item);
    }

    public void VaciarBidon(ItemData item)
    {
        if (item != null)
            bidonesLlenos.Remove(item);
    }
}
