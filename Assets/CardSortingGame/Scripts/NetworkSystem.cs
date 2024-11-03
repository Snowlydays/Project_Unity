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
    
    // フェーズを管理するNetWrokVariable
    private NetworkVariable<int> netphase = new NetworkVariable<int>(0);

    // プレイヤーのReady状態を管理するNetworkVariables
    private NetworkVariable<bool> netHostReady = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> netClientReady = new NetworkVariable<bool>(false);
    
    public NetworkVariable<bool> netIsHostAttacking = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> netIsClientAttacking = new NetworkVariable<bool>(false);
    
    private NetworkVariable<bool> netIsHostAscending = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> netIsClientAscending = new NetworkVariable<bool>(false);

    public static int cardNum = 7;// 盤面上にあるカードの枚数

    // カードのNetworkList
    private NetworkList<int> netHostCard;
    private NetworkList<int> netClientCard;

    // アイテムのNetworkList
    private NetworkList<int> netHostItem;
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
    private ItemPhaseManager itemPhaseManager;

    void Awake()
    {
        //各種NetworkList初期化
        netHostCard = new NetworkList<int>();
        netClientCard = new NetworkList<int>();
        netHostItem = new NetworkList<int>();
        netClientItem = new NetworkList<int>();
        
        phaseManager = FindObjectOfType<PhaseManager>();
    }

    public override void OnDestroy()
    {
        //接続終了時にNetworkListを破棄
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
        
        CardsManager cardsManager = FindObjectOfType<CardsManager>();;
        if (IsHost)
        {
            for (int i = 0; i < cardsManager.myCards.Count; i++)
            {
                ChangeHostCardList(i, cardsManager.myCards[i].cardNum);
            }
        }
        else
        {
            for (int i = 0; i < cardsManager.myCards.Count; i++)
            {
                ChangeClientCardList(i, cardsManager.myCards[i].cardNum);
            }
        }
    }

    private void OnNetHostCardChanged(NetworkListEvent<int> changeEvent)
    {
        string log=" ";
        for(int i=0;i<netHostCard.Count;i++){
            hostCard[i]=netHostCard[i];
            log+=netHostCard[i].ToString()+",";
        }

        //このログで現在のホストのカードの状態を見れる
        //ただコンソール画面を埋め尽くすほど大量に出力するので、基本コメントアウト推奨
        // Debug.Log("Host Card is :"+log.Remove(log.Length-1));
    }

    private void OnNetClientCardChanged(NetworkListEvent<int> changeEvent)
    {
        string log=" ";
        for(int i=0;i<netClientCard.Count;i++){
            clientCard[i]=netClientCard[i];
            log+=netClientCard[i].ToString()+",";
        }

        //クライアントのカードログ 以降上記と同様
        //Debug.Log("Client Card is :"+log.Remove(log.Length-1)); 
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

        //ホストのアイテムログ 以降上記と同様
        //Debug.Log("Host Item is :"+log.Remove(log.Length-1)); 
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

        //クライアントのアイテムログ 以降上記と同様
        //Debug.Log("Client Item is :"+log.Remove(log.Length-1)); 
    }

    public void ChangeHostCardList(int pos, int value)
    {
        if (pos < 0 || pos >= netHostCard.Count) return;
        
        netHostCard[pos] = value;
    }

    public void ChangeClientCardList(int pos, int value)
    {
        ChangeClientCardServerRpc(pos, value);
    }
    
    public void ChangePhase(int nextPhase)
    {
        if (IsHost)
        {
            netphase.Value = nextPhase;
        }
    }

    public void ToggleAttacked()
    {
        if (IsClient)
        {
            ToggleAttackedServerRpc(IsHost);
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

    public void HandleAttackAction(bool hostAttacked, bool clientAttacked)
    {
        bool hostAsc = netIsHostAscending.Value;
        bool clientAsc = netIsClientAscending.Value;
        if(hostAttacked)hostAsc = CheckAscending(hostCard);
        if(clientAttacked)clientAsc = CheckAscending(clientCard);

        netIsHostAscending.Value = hostAsc;
        netIsClientAscending.Value = clientAsc;

        if (hostAsc && clientAsc)
        {
            Debug.Log("両プレイヤーのカードが昇順: 引き分け");
            // 引き分けの処理を実装
        }
        else if (hostAsc)
        {
            Debug.Log("ホストのカードが昇順: ホストの勝利");
            // ホストの勝利処理を実装
        }
        else if (clientAsc)
        {
            Debug.Log("クライアントのカードが昇順: クライアントの勝利");
            // クライアントの勝利処理を実装
        }
        else
        {
            Debug.Log("どちらのプレイヤーもカードが昇順ではない");
            // 必要に応じて処理を実装
        }
    }

    private bool CheckAscending(int[] cards)
    {
        for (int i = 0; i < cards.Length - 1; i++)
            if (cards[i] > cards[i + 1]) return false;
        return true;
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
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                Debug.Log("alpha3");
                for (int i = 0; i < netClientCard.Count; i++)
                {
                    Debug.Log($"client{i}番目のカードの値: {netClientCard[i]}");
                }
                for (int i = 0; i < netHostCard.Count; i++)
                {
                    Debug.Log($"host{i}番目のカードの値: {netHostCard[i]}");
                }
            }
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
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                Debug.Log("alpha3");
                for (int i = 0; i < netClientCard.Count; i++)
                {
                    Debug.Log($"client{i}番目のカードの値: {netClientCard[i]}");
                }
                for (int i = 0; i < netHostCard.Count; i++)
                {
                    Debug.Log($"host{i}番目のカードの値: {netHostCard[i]}");
                }
            }
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

    
    [ServerRpc(RequireOwnership = false)]
    void ToggleAttackedServerRpc(bool isHost)
    {
        if (isHost)
        {
            netIsHostAttacking.Value = !netIsHostAttacking.Value;
            if(netIsHostAttacking.Value)Debug.Log("NetworkSystem.ToggleAttackedServerRpc host攻撃: true");
            else Debug.Log("NetworkSystem.ToggleAttackedServerRpc host攻撃: false");
        }
        else
        { 
            netIsClientAttacking.Value = !netIsClientAttacking.Value;
            if(netIsClientAttacking.Value)Debug.Log("NetworkSystem.ToggleAttackedServerRpc client攻撃: true");
            else Debug.Log("NetworkSystem.ToggleAttackedServerRpc client攻撃: false");
        }

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
    
    // カードをスワップするServerRpcメソッド（必要に応じて）
    [ServerRpc(RequireOwnership = false)]
    public void SwapHostCardServerRpc(int indexA, int indexB)
    {
        if (indexA >= 0 && indexA < netHostCard.Count && indexB >= 0 && indexB < netHostCard.Count)
        {
            (netHostCard[indexA], netHostCard[indexB]) = (netHostCard[indexB], netHostCard[indexA]);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SwapClientCardServerRpc(int indexA, int indexB)
    {
        if (indexA >= 0 && indexA < netClientCard.Count && indexB >= 0 && indexB < netClientCard.Count)
        {
            (netClientCard[indexA], netClientCard[indexB]) = (netClientCard[indexB], netClientCard[indexA]);
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void ChangeClientCardServerRpc(int pos, int value)
    {
        if (pos < 0 || pos >= netClientCard.Count) return;
        netClientCard[pos] = value;
    }
}