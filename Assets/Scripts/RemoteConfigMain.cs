using UnityEngine;
using Unity.Services.RemoteConfig;

public class RemoteConfigMain : MonoBehaviour
{
    public struct GameConfigData
    {
        public string groundColor;
        public float playerScale;
        public int playerSpeed;
        // NUEVO: Variable booleana
        public bool enableTrail;
    }

    public GameConfigData currentConfig;
    public System.Action<GameConfigData> OnConfigFetched;

    public void FetchRemoteConfig()
    {
        Debug.Log("Obteniendo Remote Config...");
        RemoteConfigService.Instance.FetchCompleted += ApplyRemoteSettings;
        RemoteConfigService.Instance.FetchConfigs(new userAttributes(), new appAttributes());
    }

    void ApplyRemoteSettings(ConfigResponse configResponse)
    {
        // Strings, Floats, Ints previos
        currentConfig.groundColor = RemoteConfigService.Instance.appConfig.GetString("groundColor", "FF0000");
        currentConfig.playerScale = RemoteConfigService.Instance.appConfig.GetFloat("playerScale", 1.0f);
        currentConfig.playerSpeed = RemoteConfigService.Instance.appConfig.GetInt("playerSpeed", 5);

        // NUEVO: Obtener el booleano (por defecto false)
        currentConfig.enableTrail = RemoteConfigService.Instance.appConfig.GetBool("enableTrail", false);

        Debug.Log($"Config Cargada: Trail={currentConfig.enableTrail}");

        OnConfigFetched?.Invoke(currentConfig);
    }

    public struct userAttributes { }
    public struct appAttributes { }

    private void OnDestroy()
    {
        RemoteConfigService.Instance.FetchCompleted -= ApplyRemoteSettings;
    }
}