using Unity.Netcode;
using UnityEngine;

// Requerimos el componente para evitar errores si no está en el prefab
[RequireComponent(typeof(TrailRenderer))]
public class SimplePlayerController : NetworkBehaviour
{
    public NetworkVariable<float> netMoveSpeed = new NetworkVariable<float>(5f);
    public NetworkVariable<float> netScale = new NetworkVariable<float>(1f);

    // NUEVO: Variable de red booleana
    public NetworkVariable<bool> netEnableTrail = new NetworkVariable<bool>(false);

    private TrailRenderer trailRenderer;

    private void Awake()
    {
        trailRenderer = GetComponent<TrailRenderer>();
        // Lo desactivamos por defecto hasta que el servidor diga lo contrario
        trailRenderer.enabled = false;
    }

    public override void OnNetworkSpawn()
    {
        ApplyScale(netScale.Value);
        ApplyTrail(netEnableTrail.Value); // Aplicar estado inicial del trail

        netScale.OnValueChanged += (prev, curr) => ApplyScale(curr);

        // NUEVO: Escuchar cambios en el booleano
        netEnableTrail.OnValueChanged += (prev, curr) => ApplyTrail(curr);
    }

    private void ApplyScale(float scale)
    {
        transform.localScale = Vector3.one * scale;
    }

    // NUEVO: Lógica para activar/desactivar la estela
    private void ApplyTrail(bool isEnabled)
    {
        if (trailRenderer != null)
        {
            trailRenderer.enabled = isEnabled;
            // Configuración rápida visual para que se note
            trailRenderer.time = 0.5f;
            trailRenderer.startWidth = 0.5f * transform.localScale.x;
            trailRenderer.endWidth = 0f;
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 moveInput = new Vector3(h, 0, v);
        transform.Translate(moveInput * netMoveSpeed.Value * Time.deltaTime);
    }
}