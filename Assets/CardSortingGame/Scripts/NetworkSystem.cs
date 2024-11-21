using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEditor;
using Unity.Collections;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;

public class NetworkSystem : NetworkBehaviour
{
    // フェーズごとの数字を変数で管理
    const int initialPhase = 0;
    const int itemPhase = 1;
    const int questionPhase = 2;
    const int itemUsingPhase = 3;
    
    // フェーズを管理するNetWrokVariable
    private NetworkVariable<int> netphase = new NetworkVariable<int>(0);

    // プレイヤーのReady状態を管理するNetworkVariables
    private NetworkVariable<bool> netHostReady = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> netClientReady = new NetworkVariable<bool>(false);
    
    public NetworkVariable<bool> netIsHostAttacking = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> netIsClientAttacking = new NetworkVariable<bool>(false);
    
    private NetworkVariable<bool> netIsHostAscending = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> netIsClientAscending = new NetworkVariable<bool>(false);

    private NetworkVariable<int> netIsHostWaiting = new NetworkVariable<int>(0);
    private NetworkVariable<int> netIsClientWaiting = new NetworkVariable<int>(0);

    public static int cardNum = 7;// 盤面上にあるカードの枚数
    private int ITEM_NUM = 6; // ゲーム内のアイテム数

    // カードのNetworkList
    private NetworkList<int> netHostCard;
    private NetworkList<int> netClientCard;

    // アイテムのNetworkList
    private NetworkList<bool> netHostItems;
    private NetworkList<bool> netClientItems;

    // ログのNetworkList
    private NetworkList<FixedString64Bytes> logDataList;
    
    // アイテムのNetworkList(使用順などもわかるもの)
    private NetworkList<int> netHostItemSelects;
    private NetworkList<int> netClientItemSelects;

    //カードは要素数が変わることがないのでarray、アイテムは常に要素数が変化するのでlist管理

    public static int[] hostCard = new int[cardNum];//カード配列取得用変数
    public static int[] clientCard = new int[cardNum];

    public static int phase = 0;
    public static bool hostReady = false;
    public static bool clientReady = false;

    public int hostWaiting = 0;
    public int clientWaiting = 0;
    
    private PhaseManager phaseManager;
    public ItemPhaseManager itemPhaseManager;
    public QutstionController questionController;
    public ItemUsingManager itemUsingManager;
    private LogMenuController logMenuController;
    public InformationManager informationManager;

    // ローカルプレイヤーの準備状態が変わった時に発火するイベント
    public event Action<bool> OnLocalReadyStateChanged;
    
    void Awake()
    {
        //各種NetworkList初期化
        netHostCard = new NetworkList<int>();
        netClientCard = new NetworkList<int>();
        netHostItems = new NetworkList<bool>();
        netClientItems = new NetworkList<bool>();
        logDataList = new NetworkList<FixedString64Bytes>();
        netHostItemSelects = new NetworkList<int>();
        netClientItemSelects = new NetworkList<int>();
    }
    
    public override void OnDestroy()
    {
        //接続終了時にNetworkListを破棄
        netHostCard?.Dispose();
        netClientCard?.Dispose();
        netHostItems?.Dispose();
        netClientItems?.Dispose();
        logDataList?.Dispose();
        netHostItemSelects?.Dispose();
        netClientItemSelects?.Dispose();

        netphase?.Dispose();
        netHostReady?.Dispose();
        netClientReady?.Dispose();
        netIsHostAttacking?.Dispose();
        netIsClientAttacking?.Dispose();
        netIsHostAscending?.Dispose();
        netIsClientAscending?.Dispose();
        netIsHostWaiting?.Dispose();
        netIsClientWaiting?.Dispose();
    }

    public override void OnNetworkSpawn()
    {
        StartCoroutine(InitializeNetworkSystem());
    }

    IEnumerator InitializeNetworkSystem()
    {
        // シーンを追加読み込み (非同期)
        AsyncOperation loadItemPhase = SceneManager.LoadSceneAsync("Scenes/SoAPhaseScenes/ItemPhase", LoadSceneMode.Additive);
        AsyncOperation loadQuestionPhase = SceneManager.LoadSceneAsync("Scenes/SoAPhaseScenes/QuestionPhase", LoadSceneMode.Additive);
        AsyncOperation loadItemUsingPhase = SceneManager.LoadSceneAsync("Scenes/SoAPhaseScenes/ItemUsingPhase", LoadSceneMode.Additive);

        // ロードが完了するまで待機
        yield return loadItemPhase;
        yield return loadQuestionPhase;
        yield return loadItemUsingPhase;
        
        phaseManager = FindObjectOfType<PhaseManager>();
        itemPhaseManager = FindObjectOfType<ItemPhaseManager>();
        questionController = FindObjectOfType<QutstionController>();
        itemUsingManager = FindObjectOfType<ItemUsingManager>();
        logMenuController = FindObjectOfType<LogMenuController>();
        informationManager = FindObjectOfType<InformationManager>();
        
        // イベント追加
        Debug.Log("NetworkSystem.OnNetworkSpawn");
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
            RequestCheckAllPlayersReadyServerRpc();
            if(IsHost)OnLocalReadyStateChanged?.Invoke(newParam);
        };

        netClientReady.OnValueChanged += (bool oldParam, bool newParam) =>
        {
            clientReady = newParam;
            Debug.Log($"clientのReady状態が {newParam} になりました。");
            RequestCheckAllPlayersReadyServerRpc();
            if (!IsHost)OnLocalReadyStateChanged?.Invoke(newParam);
        };

        netIsHostWaiting.OnValueChanged +=  (int oldParam, int newParam) =>
        {
            hostWaiting=newParam;
        };

        netIsClientWaiting.OnValueChanged +=  (int oldParam, int newParam) =>
        {
            clientWaiting=newParam;
        };

        netHostCard.OnListChanged += OnNetHostCardChanged;
        netClientCard.OnListChanged += OnNetClientCardChanged;

        netHostItems.OnListChanged += OnNetHostItemChanged;
        netClientItems.OnListChanged += OnNetClientItemChanged;

        logDataList.OnListChanged += OnNetLogChanged;
        
        netHostItemSelects.OnListChanged += netHostItemSelectsChanged;
        netClientItemSelects.OnListChanged += netClientItemSelectsChanged;
        
        // カードの初期化
        if(IsServer){
            //カード配列の初期設定(ここでは純粋なランダム入れ替え)
            for(int i=0;i<cardNum;i++){
                netHostCard.Add(i+1);
                netClientCard.Add(i+1);
            }
            ShuffleCards(netHostCard);
            ShuffleCards(netClientCard);
        }
        
        CardsManager cardsManager = FindObjectOfType<CardsManager>();
        if (IsServer)
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

        // アイテムの初期化
        if (IsServer)
        {
            for (int i = 0; i < ITEM_NUM; i++)
            {
                netHostItems.Add(false);
                netClientItems.Add(false);
            }
        }
        Debug.Log("NetworkSystemの初期化完了");
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
        for(int i=0;i < netClientCard.Count;i++){
            clientCard[i]=netClientCard[i];
            log+=netClientCard[i].ToString()+",";
        }

        //クライアントのカードログ 以降上記と同様
        //Debug.Log("Client Card is :"+log.Remove(log.Length-1)); 
    }
    
    private void OnNetHostItemChanged(NetworkListEvent<bool> changeEvent)
    {
        int len = Mathf.Min(netHostItems.Count, ITEM_NUM);
        for (int i = 0; i < len; i++)
        {
            if (IsHost) itemPhaseManager.myItems[i] = netHostItems[i];
            else itemPhaseManager.otherItems[i] = netHostItems[i];
        }
        itemPhaseManager.UpdateInventoryUI();
    }

    private void OnNetClientItemChanged(NetworkListEvent<bool> changeEvent)
    {
        int len = Mathf.Min(netClientItems.Count, ITEM_NUM);
        for (int i = 0; i < len; i++)
        {
            if (IsHost) itemPhaseManager.otherItems[i] = netClientItems[i]; 
            else itemPhaseManager.myItems[i] = netClientItems[i]; 
        }
        itemPhaseManager.UpdateInventoryUI();
    }

    public void OnNetLogChanged(NetworkListEvent<FixedString64Bytes> changeEvent)
    {
        logMenuController.myLogs.Clear();
        logMenuController.opponentLogs.Clear();
        logMenuController.allLogs.Clear();
        foreach(var txt in logDataList)
        {
            string dat = txt.ToString();
            string id = dat.Substring(dat.Length - 2);
            dat = dat.Remove(dat.Length - 2);
            if((IsHost && id == "/h") || (!IsHost && id == "/c"))
            {
                logMenuController.myLogs.Add(dat);
                logMenuController.allLogs.Add("あなた: " + dat);
            }
            else
            {
                logMenuController.allLogs.Add("相手: " + dat);
                logMenuController.opponentLogs.Add(dat);
            }
        }
    }
    
    private void netHostItemSelectsChanged(NetworkListEvent<int> changeEvent)
    {
        int len = Mathf.Min(netHostItemSelects.Count, ITEM_NUM);
        if (IsHost) itemUsingManager.myItems = new int[len];
        else itemUsingManager.otherItems = new int[len];
        if(len<=0)return;
        for (int i = 0; i < len; i++)
        {
            if (IsHost) itemUsingManager.myItems[i] = netHostItemSelects[i];
            else itemUsingManager.otherItems[i] = netHostItemSelects[i];
        }
    }

    private void netClientItemSelectsChanged(NetworkListEvent<int> changeEvent)
    {
        int len = Mathf.Min(netClientItemSelects.Count, ITEM_NUM);
        if (IsHost) itemUsingManager.otherItems = new int[len];
        else itemUsingManager.myItems = new int[len];
        if(len<=0)return;
        for (int i = 0; i < len; i++)
        {
            if (IsHost) itemUsingManager.otherItems[i] = netClientItemSelects[i];
            else itemUsingManager.myItems[i] = netClientItemSelects[i];
        }
    }

    public void ChangeItems(int pos, bool value)
    {
        if (IsHost) netHostItems[pos] = value;
        else ChangeClientItemServerRpc(pos, value);
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

    public void ToggleAttackedReset()
    {
        if(IsClient)ResetAttackStatusServerRpc();
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void RequestCheckAllPlayersReadyServerRpc()
    {
        Debug.Log("サーバがCheckAllPlayersReady()を実行");
        CheckAllPlayersReady();
    }
    
    // すべてのプレイヤーがReadyかをチェックし、フェーズを進める
    private void CheckAllPlayersReady()
    {
        if (hostReady && clientReady)
        {
            switch (phase)
            {
                case initialPhase:
                    ChangePhase(itemPhase);
                    break;

                case itemPhase:
                    ChangePhase(itemUsingPhase);
                    break;

                case itemUsingPhase:
                    ChangePhase(questionPhase);
                    break;

                case questionPhase:
                    ChangePhase(initialPhase);
                    break;
            }
            if(IsHost)ResetReadyStates();
        }
        else if(hostReady || clientReady)
        {
            // 相手の行動を待っている時、その情報を表示
            if (phase != initialPhase)
            {
                Dictionary<int, string> infoDict = new Dictionary<int, string> { { itemPhase, "相手のアイテム選択を待っています..." }, { questionPhase, "相手の質問を待っています..." }, { itemUsingPhase, "相手のアイテム使用を待っています..." } };
                if (hostReady) {
                    informationManager.SetInformationText(infoDict[phase]);
                }else{
                    SetInformationTextClientRpc(infoDict[phase]);  
                } 
            }
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
        //実行するたびに切り替わるのではなく強制的にreadyの状態をtrueにする処理に変更
        if (IsHost)
        {
            netHostReady.Value = true;
            ChangeHostWaitingServerRPC(0);
        }
        else
        {
            ClientReadyChange();
            ChangeClientWaitingServerRPC(0);
        }
    }

    public void DisconnectServer(){
        //サーバー切断用メソッド
        //NetworkManager.Singleton.Shutdown();
        /*if (NetworkManager.Singleton != null)
        {
            Destroy(NetworkManager.Singleton);
        }*/
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
            ToMoveSceneClientRpc("Scenes/ResultsScenes/DrawScene");
            DisconnectServer();
            SceneManager.LoadScene("Scenes/ResultsScenes/DrawScene");
        }
        else if (hostAsc)
        {
            Debug.Log("ホストのカードが昇順: ホストの勝利");
            // ホストの勝利処理を実装
            ToMoveSceneClientRpc("Scenes/ResultsScenes/LoseScene");
            DisconnectServer();
            SceneManager.LoadScene("Scenes/ResultsScenes/WinScene");
        }
        else if (clientAsc)
        {
            Debug.Log("クライアントのカードが昇順: クライアントの勝利");
            // クライアントの勝利処理を実装
            ToMoveSceneClientRpc("Scenes/ResultsScenes/WinScene");
            DisconnectServer();
            SceneManager.LoadScene("Scenes/ResultsScenes/LoseScene");
        }
        else
        {
            Debug.Log("どちらのプレイヤーもカードが昇順ではない");
            if (hostAttacked)
            {
                Debug.Log("攻撃失敗！");
                Log("攻撃失敗！");
                informationManager.SetInformationText("攻撃失敗！");
            }
            if (clientAttacked)SetInformationTextClientRpc("攻撃失敗！");
        }
    }

    [ClientRpc]
    private void SetInformationTextClientRpc(string message)
    {
        if (!IsHost)
        {
            Debug.Log($"SetInformationTextClientRpc: {message}");
            informationManager.SetInformationText(message);
        }
    }
    

    [Unity.Netcode.ClientRpc(RequireOwnership = false)]
    public void ToMoveSceneClientRpc(string scenename)
    { 
        SceneManager.LoadScene(scenename);
        DisconnectServer();
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
        /*if(Input.GetKeyDown(KeyCode.Alpha4))
        {
            string host_log = "host\n",client_log="client\n";
            for (int i = 0; i < ITEM_NUM; i++)
            {
                host_log += netHostItems[i];
                client_log += netClientItems[i];
            }
            Debug.Log(host_log);
            Debug.Log(client_log);
        }
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
        }*/
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
        netClientReady.Value = true;
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
    
    // カードをスワップするServerRpcメソッド
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
    
    [ServerRpc(RequireOwnership = false)]
    public void ResetAttackStatusServerRpc()
    {
        netIsHostAttacking.Value = false;
        netIsClientAttacking.Value = false;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeClientItemServerRpc(int pos, bool value)
    { 
        netClientItems[pos] = value;
    }

    public void Log(string str)
    {
        if(IsHost)
        {
            LogHost(str);
        }
        else
        {
            LogClientServerRpc(str);
        }
    }

    [Unity.Netcode.ServerRpc(RequireOwnership = false)]
    public void LogClientServerRpc(string str)
    {
        FixedString64Bytes fstr = new FixedString64Bytes(str + "/c");
        logDataList.Add(fstr);
    }

    public void LogHost(string str)
    {
        FixedString64Bytes fstr = new FixedString64Bytes(str + "/h");
        logDataList.Add(fstr);
    }
    
    //netItemSelectsの中身を変更するメソッド群(引数にarrayを入れることでそのarrayの中身通りに変更する)
    public void ChangeItemSelects(int[] array)
    {
        Debug.Log("アイテム同期中");
        if (IsHost){
            ChangeHostItemSelects(array);
        }else{
            ChangeClientItemSelectsServerRPC(array);
        }
    }

    public void ChangeHostItemSelects(int[] array)
    {
        netHostItemSelects.Clear();
        int len = array.Length;
        if(len>0){
            for(int i=0;i<len;i++)
            {
                netHostItemSelects.Add(array[i]);
            }
        }
        ClientSetsHostItemClientRpc();
        Debug.Log("ホスト適応完了");
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeClientItemSelectsServerRPC(int[] array)
    { 
        netClientItemSelects.Clear();
        int len = array.Length;
        if(len>0){
            for(int i=0;i<len;i++)
            {
                netClientItemSelects.Add(array[i]);
            }
        }
        ClientSetsClientItemClientRpc();
        Debug.Log("クライアント適応完了");
    }

    //onListChangedメソッドはホスト側にしかトリガーされない問題があるらしく
    //その対策としてホストがonListChanged内で変更を適応させた後
    //クライアントに同じ操作を要求させるために以下のメソッドを使う
    [Unity.Netcode.ClientRpc(RequireOwnership = false)]
    public void ClientSetsHostItemClientRpc(){
        if (IsOwner) return;
        int len = Mathf.Min(netHostItemSelects.Count, ITEM_NUM);
        if (IsHost) itemUsingManager.myItems = new int[len];
        else itemUsingManager.otherItems = new int[len];
        if(len>0){
            for (int i = 0; i < len; i++)
            {
                if (IsHost) itemUsingManager.myItems[i] = netHostItemSelects[i];
                else itemUsingManager.otherItems[i] = netHostItemSelects[i];
            }
        }
        //ホストのreadyをtrueにする。
        HostSetReadyServerRpc();
    }

    [Unity.Netcode.ClientRpc(RequireOwnership = false)]
    public void ClientSetsClientItemClientRpc(){
        int len = Mathf.Min(netClientItemSelects.Count, ITEM_NUM);
        Debug.Log(len);
        if (IsHost) itemUsingManager.otherItems = new int[len];
        else itemUsingManager.myItems = new int[len];
        if(len>0){
            for (int i = 0; i < len; i++)
            {
                if (IsHost) itemUsingManager.otherItems[i] = netClientItemSelects[i];
                else itemUsingManager.myItems[i] = netClientItemSelects[i];
            }
        }
        //クライアントのreadyをtrueにする
        ClientSetReadyClientRpc();
    }

    //同期ずれ対策のためのもの
    //クライアントが実行しようがホストが実行しようが
    //「クライアントの」readyをtrueにする(ホストも同様)
    //というメソッドが欲しかったため増設
    [Unity.Netcode.ClientRpc(RequireOwnership = false)]
    public void ClientSetReadyClientRpc()
    { 
        if (IsServer) return;//クライアントをreadyしたいので、ホストが実行しようとしたら飛ばす
        ToggleReady();
    }

    [Unity.Netcode.ServerRpc(RequireOwnership = false)]
    public void HostSetReadyServerRpc()
    { 
        ToggleReady();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeHostWaitingServerRPC(int num)
    {
        netIsHostWaiting.Value=num;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeClientWaitingServerRPC(int num)
    { 
        netIsClientWaiting.Value=num;
    }
}