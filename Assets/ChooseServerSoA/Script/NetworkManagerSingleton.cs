using UnityEngine;
using Unity.Netcode;

public class NetworkManagerSingleton : NetworkManager
{
    public static NetworkManagerSingleton Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // シーンが変更されてもオブジェクトが削除されないように変更
    }
}