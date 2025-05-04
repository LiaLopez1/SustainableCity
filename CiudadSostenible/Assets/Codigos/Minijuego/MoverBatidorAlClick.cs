using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class MoverBatidorAlClick : MonoBehaviour
{
    public Transform spawnPointDestino;
    public Vector3 rotacionDeseada = new Vector3(-40f, 180f, 0f);
    public float rangoMovimientoX = 0.15f;
    public float velocidadMovimiento = 2f;
    public LayerMask bowlLayer;
    public TextMeshProUGUI mensajeTMP;

    private bool yaMovido = false;
    private bool permitirMovimiento = false;
    private Vector3 posicionInicial;
    private float ladoAnterior = 0f;
    private int vueltasCompletadas = 0;

    private PaperBowlManager bowlManager;
    private List<GameObject> papelesDentroDelBowl = new List<GameObject>();

    private void Start()
    {
        posicionInicial = transform.position;

        Collider[] cercanos = Physics.OverlapSphere(transform.position, 10f);
        foreach (var col in cercanos)
        {
            if (col.gameObject.layer == LayerMask.NameToLayer("WaterBowl"))
            {
                bowlManager = col.GetComponent<PaperBowlManager>();
                break;
            }
        }
    }

    private void OnMouseDown()
    {
        if (yaMovido) return;

        if (bowlManager == null || !bowlManager.EstaLleno())
        {
            if (mensajeTMP != null)
            {
                mensajeTMP.gameObject.SetActive(true);
                CancelInvoke(nameof(EsconderTMP));
                Invoke(nameof(EsconderTMP), 2f);
            }
            return;
        }

        transform.position = spawnPointDestino.position;
        transform.rotation = Quaternion.Euler(rotacionDeseada);
        posicionInicial = transform.position;

        permitirMovimiento = true;
        yaMovido = true;

        papelesDentroDelBowl = bowlManager.GetListaPapeles();
    }

    private void Update()
    {
        if (permitirMovimiento && Input.GetMouseButton(0))
        {
            float inputX = Input.GetAxis("Mouse X");
            Vector3 nuevaPos = transform.position + Vector3.right * (-inputX) * velocidadMovimiento * Time.deltaTime;

            float desplazamiento = nuevaPos.x - posicionInicial.x;
            desplazamiento = Mathf.Clamp(desplazamiento, -rangoMovimientoX, rangoMovimientoX);
            transform.position = new Vector3(posicionInicial.x + desplazamiento, transform.position.y, transform.position.z);

            float ladoActual = Mathf.Sign(desplazamiento);

            if (ladoAnterior != 0f && ladoActual != ladoAnterior)
            {
                vueltasCompletadas++;
                ladoAnterior = ladoActual;

                if (vueltasCompletadas >= 4)
                {
                    List<GameObject> papelesActuales = bowlManager.GetListaPapeles();
                    if (papelesActuales.Count > 0)
                    {
                        GameObject papel = papelesActuales[0];
                        if (papel != null)
                        {
                            Destroy(papel);
                            vueltasCompletadas = 0;
                        }
                    }
                }


                if (papelesDentroDelBowl.Count == 0)
                {
                    transform.position = posicionInicial;
                    permitirMovimiento = false;
                    yaMovido = false;
                }
            }
            else if (ladoAnterior == 0f)
            {
                ladoAnterior = ladoActual;
            }
        }
    }

    private void EsconderTMP()
    {
        if (mensajeTMP != null)
        {
            mensajeTMP.gameObject.SetActive(false);
        }
    }
}
