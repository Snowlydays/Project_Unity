using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Random = UnityEngine.Random;

public class NetworkSystem : NetworkBehaviour
{
    // フェーズごとの数字を変数で管理
    const int initialPhase = 0;
    const int itemPhase = 1;
    const int questionPhase = 2;

    private ItemPhaseManager itemPhaseManager;

    // フェーズを管理するNetWrokVariable
    private NetworkVariable<int> netphase = new NetworkVariable<int>(0);

    // プレイヤーのReady状態を管理するNetworkVariables
    private NetworkVariable<bool> netHostReady = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> netClientReady = new NetworkVariable<bool>(false);

    public static int cardNum = 7;//盤面上にあるカードの枚数

    private NetworkList<int> netHostCard;//カードのnetworklist
    private NetworkList<int> netClientCard;

    private NetworkList<int> netHostItem;//アイテムのnetworklist
    private NetworkList<int> netClientItem;

    //カードは要素数が変わることがないのでarray、アイテムは常に要素数が変化するのでlist管理

    public static int[] hostCard = new int[cardNum];//カード配列取得用変数
    public static int[] clientCard = new int[cardNum];

    public static List<int> hostItem = new List<int>();//アイテム配列取得用変数
    public static List<int> clientItem = new List<int>();

    public static int phase = 0;
    public static bool hostReady = false;
    public static bool clientReady = false;
    
    private PhaseManager phaseManager;

    private void Awake()
    {
        phaseManager = FindObjectOfType<PhaseManager>();
    }

    void Awake()
    {
        //各種networklist初期化
        netHostCard = new NetworkList<int>();
        netClientCard = new NetworkList<int>();
        netHostItem = new NetworkList<int>();
        netClientItem = new NetworkList<int>();
    }

    public override void OnDestroy()
    {
        //接続終了時にnetworklistを破棄
        netHostCard?.Dispose();
        netClientCard?.Dispose();
        netHostItem?.Dispose();
        netClientItem?.Dispose();
    }

    public override void OnNetworkSpawn()
    {
        netphase.OnValueChanged += (int oldParam, int newParam) =>
        {
            phase = newParam;
            Debug.Log($"フェーズが {newParam} になりました。");
            phaseManager.HandlePhaseChange(newParam);
        };

        netHostReady.OnValueChanged += (bool oldParam, bool newParam) =>
        {
            hostReady = newParam;
            Debug.Log($"hostのReady状態が {newParam} になりました。");
            CheckAllPlayersReady();
        };

        netClientReady.OnValueChanged += (bool oldParam, bool newParam) =>
        {
            clientReady = newParam;
            Debug.Log($"clientのReady状態が {newParam} になりました。");
            CheckAllPlayersReady();
        };

        netHostCard.OnListChanged += OnNetHostCardChanged;
        netClientCard.OnListChanged += OnNetClientCardChanged;

        netHostItem.OnListChanged += OnNetHostItemChanged;
        netClientItem.OnListChanged += OnNetClientItemChanged;

        if(IsHost){
            //カード配列の初期設定(ここでは純粋なランダム入れ替え)
            for(int i=0;i<cardNum;i++){
                netHostCard.Add(i+1);
                netClientCard.Add(i+1);
            }
            ShuffleCards(netHostCard);
            ShuffleCards(netClientCard);

        }
    }

    private void OnNetHostCardChanged(NetworkListEvent<int> changeEvent)
    {
        string log=" ";
        for(int i=0;i<netHostCard.Count;i++){
            hostCard[i]=netHostCard[i];
            log+=netHostCard[i].ToString()+",";
        }

        //Debug.Log("Host Card is :"+log.Remove(log.Length-1));
        //このログで現在のホストのカードの状態を見れる
        //ただコンソール画面を埋め尽くすほど大量に出力するので、基本コメントアウト推奨
    }

    private void OnNetClientCardChanged(NetworkListEvent<int> changeEvent)
    {
        string log=" ";
        for(int i=0;i<netClientCard.Count;i++){
            clientCard[i]=netClientCard[i];
            log+=netClientCard[i].ToString()+",";
        }

       //Debug.Log("Client Card is :"+log.Remove(log.Length-1)); 
       //クライアントのカードログ 以降上記と同様
    }

    private void OnNetHostItemChanged(NetworkListEvent<int> changeEvent)
    {
        string log=" ";
        hostItem = new List<int>();
        if(netHostItem?.Count>0){
            for(int i=0;i<netHostItem.Count;i++){
                hostItem.Add(netHostItem[i]);
                log+=netHostItem[i].ToString()+",";
            }
        }

        //Debug.Log("Host Item is :"+log.Remove(log.Length-1)); 
        //ホストのアイテムログ 以降上記と同様
    }

    private void OnNetClientItemChanged(NetworkListEvent<int> changeEvent)
    {
        string log=" ";
        clientItem = new List<int>();
        if(netClientItem?.Count>0){
            for(int i=0;i<netClientItem.Count;i++){
                clientItem.Add(netClientItem[i]);
                log+=netClientItem[i].ToString()+",";
            }
        }

        //Debug.Log("Client Item is :"+log.Remove(log.Length-1)); 
        //クライアントのアイテムログ 以降上記と同様
    }

    public void ChangePhase(int nextPhase)
    {
        if (IsHost)
        {
            netphase.Value = nextPhase;
        }
    }
    
    // すべてのプレイヤーがReadyかをチェックし、フェーズを進める
    public void CheckAllPlayersReady()
    {
        if (hostReady && clientReady)
        {
            switch (phase)
            {
                case initialPhase:
                    ChangePhase(itemPhase);
                    break;

                case itemPhase:
                    ChangePhase(questionPhase);
                    break;

                case questionPhase:
                    ChangePhase(initialPhase);
                    break;
            }
            ResetReadyStates();
        }
    }

    // プレイヤーのReadyフラグを初期化
    private void ResetReadyStates()
    {
        hostReady = false;
        clientReady = false;
        netHostReady.Value = false;
        netClientReady.Value = false;
    }

    public void ToggleReady()
    {
        if (IsHost)
        {
            netHostReady.Value = !netHostReady.Value;
        }
        else
        {
            ClientReadyChange();
        }
    }

    void Start()
    {

    }

    void ShuffleCards(NetworkList<int> array)
    {
        for (int i = array.Count - 1; i > 0; i--)
        {
            var j = Random.Range(0, i + 1); 
            (array[i], array[j]) = (array[j], array[i]); 
        }
    }

    void Update()
    {
        if (IsHost)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                netphase.Value = 0;
                Debug.Log("host changed phase 0");
            }
            else if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                netphase.Value = 1;
                Debug.Log("host changed phase 1");
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                netphase.Value = 2;
                Debug.Log("host changed phase 2");
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                ClientPhaseChange(0);
                Debug.Log("client changed phase 0");
            }
            else if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                ClientPhaseChange(1);
                Debug.Log("client changed phase 1");
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                ClientPhaseChange(2);
                Debug.Log("client changed phase 2");
            }
        }
    }

    void ClientReadyChange()
    {
        ClientReadyChangeServerRpc();
    }

    [Unity.Netcode.ServerRpc(RequireOwnership = false)]
    void ClientReadyChangeServerRpc()
    {
        netClientReady.Value = !netClientReady.Value;
    }

    void ClientPhaseChange(int num)
    {
        ClientPhaseChangeServerRpc(num);
    }

    [Unity.Netcode.ServerRpc(RequireOwnership = false)]
    void ClientPhaseChangeServerRpc(int num)
    {
        netphase.Value = num;
    }
}