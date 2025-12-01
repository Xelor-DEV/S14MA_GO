using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public GameObject playerPrefab;
    public RemoteConfigMain remoteConfigLoader;
    public Material groundMaterial;

    private float configPlayerSpeed = 5f;
    private float configPlayerScale = 1f;
    private bool configEnableTrail = false; // NUEVO: Cache local

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            remoteConfigLoader.OnConfigFetched += HandleConfigLoaded;
            remoteConfigLoader.FetchRemoteConfig();
        }
    }

    private void HandleConfigLoaded(RemoteConfigMain.GameConfigData data)
    {
        if (ColorUtility.TryParseHtmlString("#" + data.groundColor, out Color newCol))
        {
            groundMaterial.color = newCol;
        }

        configPlayerScale = data.playerScale;
        configPlayerSpeed = (float)data.playerSpeed;
        configEnableTrail = data.enableTrail; // NUEVO: Guardar dato

        ulong localId = NetworkManager.Singleton.LocalClientId;
        SpawnPlayerRpc(localId);
    }

    [Rpc(SendTo.Server)]
    public void SpawnPlayerRpc(ulong id)
    {
        Vector3 spawnPos = new Vector3(Random.Range(-3, 3), 0, Random.Range(-3, 3));
        GameObject playerInstance = Instantiate(playerPrefab, spawnPos, Quaternion.identity);

        SimplePlayerController controller = playerInstance.GetComponent<SimplePlayerController>();

        if (controller != null)
        {
            controller.netMoveSpeed.Value = configPlayerSpeed;
            controller.netScale.Value = configPlayerScale;

            // NUEVO: Asignar el valor booleano a la red
            controller.netEnableTrail.Value = configEnableTrail;
        }

        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(id);
    }

    private void OnDestroy()
    {
        if (remoteConfigLoader != null)
            remoteConfigLoader.OnConfigFetched -= HandleConfigLoaded;
    }
}