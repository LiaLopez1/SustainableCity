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
    private Vector3 posicionInicialReal;
    private Quaternion rotacionInicialReal;
    private Vector3 posicionInicioMovimiento;
    private float ladoAnterior = 0f;
    private int vueltasCompletadas = 0;

    private PaperBowlManager bowlManager;

    private void Start()
    {
        posicionInicialReal = transform.position;
        rotacionInicialReal = transform.rotation;

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

        posicionInicioMovimiento = spawnPointDestino.position;

        transform.position = posicionInicioMovimiento;
        transform.rotation = Quaternion.Euler(rotacionDeseada);

        permitirMovimiento = true;
        yaMovido = true;
    }

    private void Update()
    {
        if (permitirMovimiento && Input.GetMouseButton(0))
        {
            float inputX = Input.GetAxis("Mouse X");
            Vector3 nuevaPos = transform.position + Vector3.right * (-inputX) * velocidadMovimiento * Time.deltaTime;

            float desplazamiento = nuevaPos.x - posicionInicioMovimiento.x;
            desplazamiento = Mathf.Clamp(desplazamiento, -rangoMovimientoX, rangoMovimientoX);
            transform.position = new Vector3(posicionInicioMovimiento.x + desplazamiento, transform.position.y, transform.position.z);

            float ladoActual = Mathf.Sign(desplazamiento);

            if (ladoAnterior != 0f && ladoActual != ladoAnterior)
            {
                vueltasCompletadas++;
                ladoAnterior = ladoActual;

                if (vueltasCompletadas >= 4)
                {
                    bowlManager.DestruirPrimerPapel();
                    vueltasCompletadas = 0;
                }

                if (!bowlManager.HayObjetosDentro())
                {
                    transform.position = posicionInicialReal;
                    transform.rotation = rotacionInicialReal;

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
