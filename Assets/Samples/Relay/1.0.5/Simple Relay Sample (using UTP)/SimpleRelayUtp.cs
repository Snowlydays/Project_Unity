using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Unity Transport Protocol (UTP) で Relay Allocation パッケージを使用する方法を示す簡単なサンプルです。
/// UTP をホストまたは参加プレイヤーとして使用し、接続フロー全体をカバーする方法を示します。
/// ボーナスとして、HostからPlayersへのRelayメッセージの簡単なデモンストレーションが含まれています。
/// </summary>
public class SimpleRelayUtp : MonoBehaviour
{
    // GUI GameObjects

    /// <summary>
    /// ホストのプレイヤーIDを表示するテキストボックス。
    /// </summary>
    public Text HostPlayerIdText;

    /// <summary>
    /// プレイヤーのプレイヤーIDを表示するテキストボックス。
    /// </summary>
    public Text PlayerPlayerIdText;

    /// <summary>
    /// 地域を表示するドロップダウン。
    /// </summary>
    public Dropdown RegionsDropdown;

    /// <summary>
    /// 割り当てIDを表示するテキストボックス。
    /// </summary>
    public Text HostAllocationIdText;

    /// <summary>
    /// 参加コードを表示するテキストボックス。
    /// </summary>
    public Text JoinCodeText;

    /// <summary>
    /// HostのRelayサーバーに参加するためにPlayerが入力するJoin Codeの入力フィールドです。
    /// </summary>
    public InputField JoinCodeInput;

    /// <summary>
    /// 参加した割り当ての割り当てIDを表示するテキストボックスです。
    /// </summary>
    public Text PlayerAllocationIdText;

    /// <summary>
    // 参加した割り当ての割り当て ID を表示するテキストボックス。
    /// </summary>
    public Text PlayerRegionText;

    /// <summary>
    /// Host がバインドされているかどうかを表示するテキストボックス。
    /// </summary>
    public Text HostBoundText;

    /// <summary>
    /// Playerがバインドされているかどうかを表示するテキストボックス。
    /// </summary>
    public Text PlayerBoundText;

    /// <summary>
    /// プレーヤーがホストに接続されているかどうかを表示するテキストボックス。
    /// </summary>
    public Text PlayerConnectedText;

    /// <summary>
    /// HostからPlayerに送信するメッセージの入力フィールド。
    /// </summary>
    public InputField HostMessageInput;

    /// <summary>
    /// プレイヤーからホストへ送信するメッセージの入力フィールドです。
    /// </summary>
    public InputField PlayerMessageInput;

    /// <summary>
    /// MainMenuPanelゲームオブジェクトへの参照。
    /// </summary>
    public GameObject MainMenuPanel;

    /// <summary>
    /// HostPanel ゲームオブジェクトへの参照。
    /// </summary>
    public GameObject HostPanel;

    /// <summary>
    /// PlayerPanelゲームオブジェクトへの参照。
    /// </summary>
    public GameObject PlayerPanel;

    /// <summary>
    /// Hostが受信した最新のメッセージを表示するテキストボックス。
    /// </summary>
    public Text HostMessageReceivedText;

    /// <summary>
    /// Hostの接続プレイヤー数を表示するテキストボックス。
    /// </summary>
    public Text HostConnectedPlayersText;

    /// <summary>
    /// プレイヤーによって受信された最新のメッセージを表示するテキストボックス。
    /// </summary>
    public Text PlayerMessageReceivedText;

    // GUI変数群
    string joinCode = "n/a";
    string playerId = "Not signed in";
    string autoSelectRegionName = "auto-select (QoS)";
    int regionAutoSelectIndex = 0;
    List<Region> regions = new List<Region>();
    List<string> regionOptions = new List<string>();
    string hostLatestMessageReceived;
    string playerLatestMessageReceived;

    // Allocationオブジェクト
    Allocation hostAllocation;
    JoinAllocation playerAllocation;

    // Control vars
    bool isHost;
    bool isPlayer;

    // UTP vars
    NetworkDriver hostDriver;
    NetworkDriver playerDriver;
    NativeList<NetworkConnection> serverConnections;
    NetworkConnection clientConnection;

    async void Start()
    {
        // GUIをメイン・メニューに設定する
        MainMenuPanel.SetActive(true);
        HostPanel.SetActive(false);
        PlayerPanel.SetActive(false);

        // 入力フィールドでEnterキーを使用できるようにする
        AddEnterKeyListenerToInputField(HostMessageInput, OnHostSendMessage);
        AddEnterKeyListenerToInputField(PlayerMessageInput, OnPlayerSendMessage);
        AddEnterKeyListenerToInputField(JoinCodeInput, OnJoin);

        // Unityサービスを初期化する
        await UnityServices.InitializeAsync();
    }

    void AddEnterKeyListenerToInputField(InputField inputField, Action clickButton)
    {
        inputField.onEndEdit.AddListener(_ =>
        {
            // Submit == Enter key
            if (Input.GetButton("Submit"))
            {
                clickButton();
                EventSystem.current.SetSelectedGameObject(inputField.gameObject);
            }
        });
    }

    void Update()
    {
        if (isHost)
        {
            UpdateHost();
            UpdateHostUI();
        }
        else if (isPlayer)
        {
            UpdatePlayer();
            UpdatePlayerUI();
        }
    }

    void OnDestroy()
    {
        // 終了時にオブジェクトをクリーンアップする
        // networklistなどもクリーンアップする操作が必要
        if (isHost)
        {
            hostDriver.Dispose();
            serverConnections.Dispose();
        }
        else if (isPlayer)
        {
            playerDriver.Dispose();
        }
    }

    void UpdateHostUI()
    {
        HostPlayerIdText.text = playerId;
        RegionsDropdown.interactable = regions.Count > 0;
        RegionsDropdown.options?.Clear();
        RegionsDropdown.AddOptions(new List<string> {autoSelectRegionName});  // インデックス0は常に自動選択（QoSを使う）
        //QoSの詳細はGetRegionOrQosDefaultメソッドにて
        RegionsDropdown.AddOptions(regionOptions);
        if (!String.IsNullOrEmpty(hostAllocation?.Region))
        {
            if (regionOptions.Count == 0)
            {
                RegionsDropdown.AddOptions(new List<String>(new[] { hostAllocation.Region }));
            }
            RegionsDropdown.value = RegionsDropdown.options.FindIndex(option => option.text == hostAllocation.Region);
        }
        HostAllocationIdText.text = hostAllocation?.AllocationId.ToString();
        JoinCodeText.text = joinCode;
        HostBoundText.text = hostDriver.IsCreated ? hostDriver.Bound.ToString() : false.ToString();
        HostConnectedPlayersText.text = serverConnections.IsCreated ? serverConnections.Length.ToString() : 0.ToString();
        HostMessageReceivedText.text = hostLatestMessageReceived;
    }

    void UpdatePlayerUI()
    {
        PlayerPlayerIdText.text = playerId;
        PlayerAllocationIdText.text = playerAllocation?.AllocationId.ToString();
        PlayerRegionText.text = playerAllocation?.Region;
        PlayerBoundText.text = playerDriver.IsCreated ? playerDriver.Bound.ToString() : false.ToString();
        PlayerConnectedText.text = clientConnection.IsCreated.ToString();
        PlayerMessageReceivedText.text = playerLatestMessageReceived;
    }

    /// <summary>
    /// ホストクライアントとしてゲーム開始ボタンがクリックされたときのイベントハンドラです。
    /// </summary>
    public void OnStartClientAsHost()
    {
        MainMenuPanel.SetActive(false);
        HostPanel.SetActive(true);
        isHost = true;
    }

    /// <summary>
    /// クライアントとしてゲーム開始ボタンがクリックされたときのイベントハンドラです。
    /// </summary>
    public void OnStartClientAsPlayer()
    {
        MainMenuPanel.SetActive(false);
        PlayerPanel.SetActive(true);
        isPlayer = true;
    }

    /// <summary>
    /// サインインボタンがクリックされたときのイベントハンドラです。
    /// </summary>
    public async void OnSignIn()
    {
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        playerId = AuthenticationService.Instance.PlayerId;

        Debug.Log($"Signed in. Player ID: {playerId}");
    }

    /// <summary>
    /// Get Regionsボタンがクリックされたときのイベントハンドラです。
    /// </summary>
    public async void OnRegion()
    {
        Debug.Log("Host - Getting regions.");
        var allRegions = await RelayService.Instance.ListRegionsAsync();
        regions.Clear();
        regionOptions.Clear();
        foreach (var region in allRegions)
        {
            Debug.Log(region.Id + ": " + region.Description);
            regionOptions.Add(region.Id);
            regions.Add(region);
        }
    }

    string GetRegionOrQosDefault()
    {
        /// 地域リストが空の場合、または自動選択/QoSが選択された場合は、NULL（地域/QoSを自動選択することを示す）を返す。
        //ここでいう地域は、リレーサーバーのサーバーがある地域を指す？と思われる
        if (!regions.Any() || RegionsDropdown.value == regionAutoSelectIndex)
        {
            return null;
        }
        //選択されたリージョンを使用（最初のオプションが自動選択/QoSのため、ドロップダウン内のオフセット-1）
        return regions[RegionsDropdown.value - 1].Id;
    }

    /// <summary>
    // 割り当てボタンがクリックされたときのイベントハンドラです。
    /// </summary>
    public async void OnAllocate()
    {
        Debug.Log("Host - Creating an allocation. Upon success, I have 10 seconds to BIND to the Relay server that I've allocated.");

        // 使用する地域を決定する（ユーザー選択または自動選択/QoS）
        string region = GetRegionOrQosDefault();
        Debug.Log($"The chosen region is: {region ?? autoSelectRegionName}");

        // 最大接続数を設定する。最大100まで設定できますが、接続するプレイヤーが増えるほど、帯域幅/レイテンシへの影響が大きくなることに注意してください。
        int maxConnections = 4;

        // 重要： アロケーションが作成されたら、10秒以内にBINDしなければなりません。
        hostAllocation = await RelayService.Instance.CreateAllocationAsync(maxConnections, region);
        Debug.Log($"Host Allocation ID: {hostAllocation.AllocationId}, region: {hostAllocation.Region}");

        // サーバ（ホスト）の NetworkConnection リストを初期化します。
        // このリストオブジェクトは、接続されたプレーヤーを表す NetworkConnections を管理します。
        serverConnections = new NativeList<NetworkConnection>(maxConnections, Allocator.Persistent);
    }

    /// <summary>
    /// Bind Host to Relay (UTP) ボタンがクリックされたときのイベントハンドラです。
    /// </summary>
    public void OnBindHost()
    {
        Debug.Log("Host - Binding to the Relay server using UTP.");

        // Allocation レスポンスから Relay サーバーデータを抽出します。
        var relayServerData = new RelayServerData(hostAllocation, "udp");

        // Relayサーバーデータを使用してNetworkSettingsを作成します。
        var settings = new NetworkSettings();
        settings.WithRelayParameters(ref relayServerData);

        // NetworkSettings からホストの NetworkDriver を作成します。
        hostDriver = NetworkDriver.Create(settings);

        // Relayサーバーにバインドします。
        if (hostDriver.Bind(NetworkEndpoint.AnyIpv4) != 0)
        {
            Debug.LogError("Host client failed to bind");
        }
        else
        {
            if (hostDriver.Listen() != 0)
            {
                Debug.LogError("Host client failed to listen");
            }
            else
            {
                Debug.Log("Host client bound to Relay server");
            }
        }
    }

    /// <summary>
    /// Get Join Codeボタンがクリックされたときのイベントハンドラ。
    /// </summary>
    public async void OnJoinCode()
    {
        Debug.Log("Host - Getting a join code for my allocation. I would share that join code with the other players so they can join my session.");

        try
        {
            joinCode = await RelayService.Instance.GetJoinCodeAsync(hostAllocation.AllocationId);
            Debug.Log("Host - Got join code: " + joinCode);
        }
        catch (RelayServiceException ex)
        {
            Debug.LogError(ex.Message + "\n" + ex.StackTrace);
        }
    }

    /// <summary>
    /// 参加ボタンがクリックされた時のイベントハンドラです。
    /// </summary>
    public async void OnJoin()
    {
        // 最初にそれぞれの入力フィールドに参加コードを入力します。
        if (String.IsNullOrEmpty(JoinCodeInput.text))
        {
            Debug.LogError("Please input a join code.");
            return;
        }

        Debug.Log("Player - Joining host allocation using join code. Upon success, I have 10 seconds to BIND to the Relay server that I've allocated.");

        try
        {
            playerAllocation = await RelayService.Instance.JoinAllocationAsync(JoinCodeInput.text);
            Debug.Log("Player Allocation ID: " + playerAllocation.AllocationId);
        }
        catch (RelayServiceException ex)
        {
            Debug.LogError(ex.Message + "\n" + ex.StackTrace);
        }
    }

    /// <summary>
    //// Bind Player to Relay(UTP)ボタンがクリックされた時のイベントハンドラです。
    /// </summary>
    public void OnBindPlayer()
    {
        Debug.Log("Player - Binding to the Relay server using UTP.");

        // Join AllocationレスポンスからRelayサーバーデータを抽出します。
        var relayServerData = new RelayServerData(playerAllocation, "udp");

        // Relayサーバーデータを使用してNetworkSettingsを作成します。
        var settings = new NetworkSettings();
        settings.WithRelayParameters(ref relayServerData);

        // NetworkSettings オブジェクトから Player の NetworkDriver を作成します。
        playerDriver = NetworkDriver.Create(settings);

        // Relayサーバーにバインドします。
        if (playerDriver.Bind(NetworkEndpoint.AnyIpv4) != 0)
        {
            Debug.LogError("Player client failed to bind");
        }
        else
        {
            Debug.Log("Player client bound to Relay server");
        }
    }

    /// <summary>
    /// プレーヤーをリレーに接続する（UTP）ボタンがクリックされたときのイベントハンドラです。
    /// </summary>
    public void OnConnectPlayer()
    {
        Debug.Log("Player - Connecting to Host's client.");

        // Sends a connection request to the Host Player.
        clientConnection = playerDriver.Connect();
    }

    /// <summary>
    /// ホストからリレーへのメッセージ送信（UTP）ボタンがクリックされたときのイベントハンドラです。
    /// </summary>
    public void OnHostSendMessage()
    {
        if (serverConnections.Length == 0)
        {
            Debug.LogError("No players connected to send messages to.");
            return;
        }

        // 入力フィールドからメッセージを取得するか、プレースホルダ・テキストをデフォルトにします。
        var msg = !String.IsNullOrEmpty(HostMessageInput.text) ? HostMessageInput.text : HostMessageInput.placeholder.GetComponent<Text>().text;

        // このサンプルでは、接続されているすべてのクライアントにメッセージをブロードキャストします。
        for (int i = 0; i < serverConnections.Length; i++)
        {
            if (hostDriver.BeginSend(serverConnections[i], out var writer) == 0)
            {
                // メッセージを送信します。FixedString32 以外にも、さまざまな型を使用できます。
                writer.WriteFixedString32(msg);
                hostDriver.EndSend(writer);
            }
        }
    }

    /// <summary>
    /// プレイヤーからホストへのメッセージ送信(UTP)ボタンがクリックされたときのイベントハンドラです。
    /// </summary>
    public void OnPlayerSendMessage()
    {
        if (!clientConnection.IsCreated)
        {
            Debug.LogError("Player is not connected. No Host client to send message to.");
            return;
        }

        // 入力フィールドからメッセージを取得するか、プレースホルダ・テキストをデフォルトにします。
        var msg = !String.IsNullOrEmpty(PlayerMessageInput.text) ? PlayerMessageInput.text : PlayerMessageInput.placeholder.GetComponent<Text>().text;
        if (playerDriver.BeginSend(clientConnection, out var writer) == 0)
        {
            // メッセージを送信します。FixedString32 以外にも、さまざまな型を使用できます。
            writer.WriteFixedString32(msg);
            playerDriver.EndSend(writer);
        }
    }

    /// <summary>
    /// DisconnectPlayers（UTP）ボタンがクリックされたときのイベントハンドラです。
    /// </summary>
    public void OnDisconnectPlayers()
    {
        if (serverConnections.Length == 0)
        {
            Debug.LogError("No players connected to disconnect.");
            return;
        }

        // このサンプルでは、単純に接続されているクライアントをすべて切断します。
        for (int i = 0; i < serverConnections.Length; i++)
        {
            // これは接続先のクライアントに切断イベントを送信します、
            // ホストから切断されたことを通知します。
            hostDriver.Disconnect(serverConnections[i]);

            // ここでは、宛先クライアントの NetworkConnection をデフォルト値に設定します。
            // これは、HostのUpdateループで古い接続として認識され、削除されます。
            serverConnections[i] = default(NetworkConnection);
        }
    }

    /// <summary>
    /// 切断（UTP）ボタンがクリックされたときのイベントハンドラ。
    /// </summary>
    public void OnDisconnect()
    {
        // Hostクライアントに切断イベントを送信します、
        // 切断することを知らせます。
        playerDriver.Disconnect(clientConnection);

        // 現在の接続への参照をオーバーライドして削除します。
        clientConnection = default(NetworkConnection);
    }

    void UpdateHost()
    {
        // ホストがまだバインドされていない場合は、更新ロジックをスキップします。
        if (!hostDriver.IsCreated || !hostDriver.Bound)
        {
            return;
        }

        // これにより、Relayサーバーへのバインディングが維持されます、
        // 非アクティブによるタイムアウトを防ぎます。
        hostDriver.ScheduleUpdate().Complete();

        // 古くなった接続をクリーンアップします。
        for (int i = 0; i < serverConnections.Length; i++)
        {
            if (!serverConnections[i].IsCreated)
            {
                Debug.Log("Stale connection removed");
                serverConnections.RemoveAt(i);
                --i;
            }
        }

        // 着信クライアント接続を受け付けます。
        NetworkConnection incomingConnection;
        while ((incomingConnection = hostDriver.Accept()) != default(NetworkConnection))
        {
            // 要求PlayerをserverConnectionsリストに追加する。
            // これはまた、要求側PlayerにConnectイベントを送り返す、
            // 受諾を確認する手段として。
            Debug.Log("Accepted an incoming connection.");
            serverConnections.Add(incomingConnection);
        }

        // すべての接続からのイベントを処理する。
        for (int i = 0; i < serverConnections.Length; i++)
        {
            Assert.IsTrue(serverConnections[i].IsCreated);

            // イベント・キューを解決します。
            NetworkEvent.Type eventType;
            while ((eventType = hostDriver.PopEventForConnection(serverConnections[i], out var stream)) != NetworkEvent.Type.Empty)
            {
                switch (eventType)
                {
                    // リレーイベントを処理します。
                    case NetworkEvent.Type.Data:
                        FixedString32Bytes msg = stream.ReadFixedString32();
                        Debug.Log($"Server received msg: {msg}");
                        hostLatestMessageReceived = msg.ToString();
                        break;

                    // 接続切断イベントを処理します。
                    case NetworkEvent.Type.Disconnect:
                        Debug.Log("Server received disconnect from client");
                        serverConnections[i] = default(NetworkConnection);
                        break;
                }
            }
        }
    }

    void UpdatePlayer()
    {
        // Player がまだバインドされていない場合は、更新ロジックをスキップします。
        if (!playerDriver.IsCreated || !playerDriver.Bound)
        {
            return;
        }

        // これにより、Relayサーバーへのバインディングが維持されます、
        // 非アクティブによるタイムアウトを防ぎます。
        playerDriver.ScheduleUpdate().Complete();

        // イベントキューを解決します。
        NetworkEvent.Type eventType;
        while ((eventType = clientConnection.PopEvent(playerDriver, out var stream)) != NetworkEvent.Type.Empty)
        {
            switch (eventType)
            {
                // リレーイベントを処理します。
                case NetworkEvent.Type.Data:
                    FixedString32Bytes msg = stream.ReadFixedString32();
                    Debug.Log($"Player received msg: {msg}");
                    playerLatestMessageReceived = msg.ToString();
                    break;

                // 接続イベントを処理します。
                case NetworkEvent.Type.Connect:
                    Debug.Log("Player connected to the Host");
                    break;

                // 切断イベントを処理する。
                case NetworkEvent.Type.Disconnect:
                    Debug.Log("Player got disconnected from the Host");
                    clientConnection = default(NetworkConnection);
                    break;
            }
        }
    }
}
