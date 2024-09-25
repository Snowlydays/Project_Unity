using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Random = UnityEngine.Random;
using TMPro;

public class PlayerController : NetworkBehaviour
{
    //送受信するためのnetworklist変数、ここにカードの並びをintのリストで管理し送受信する
    private NetworkList<int> HostNetworkData;
    private NetworkList<int> ClientNetworkData;

    //以下はNetworkListが送受信できているかをテストするテキスト群
    [SerializeField]
    private string hostTextObjectName = "HostText"; // ホスト用TextMeshProオブジェクトの名前
    private TextMeshProUGUI hostText;

    [SerializeField]
    private string clientTextObjectName = "ClientText"; // クライアント用TextMeshProオブジェクトの名前
    private TextMeshProUGUI clientText;

    private bool isShufflingHost = false;
    private bool isShufflingClient = false;

    void Awake()
    {
        //どうやらちゃんとclient側でも初期化していなかったのがバグの原因だった
        HostNetworkData = new NetworkList<int>();
        ClientNetworkData = new NetworkList<int>();
    }

    public override void OnNetworkSpawn()
    {
        // NetworkListの初期化と変更イベントの登録
        HostNetworkData.OnListChanged += OnHostNetworkDataChanged;
        ClientNetworkData.OnListChanged += OnClientNetworkDataChanged;
    }

    private void OnHostNetworkDataChanged(NetworkListEvent<int> changeEvent)
    {
        //データの変更を検知したらテキストにそれを反映。
        if (!isShufflingHost)
        {
            Debug.Log("HostNetworkData changed:");
            UpdateHostText();
        }
    }
    
    private void OnClientNetworkDataChanged(NetworkListEvent<int> changeEvent)
    {
        //データの変更を検知したらテキストにそれを反映。
        if (!isShufflingClient)
        {
            Debug.Log("ClientNetworkData changed:");
            UpdateClientText();
        }
    }

    void ShuffleCards(NetworkList<int> array)
    {
        /*
        networklistの中身が変化するたびにonlistchangedが発生するので、booleanでそれを制限
        なお、ShuffleCardsメソッド(clientも含む)はiとjの入れ替えで「二回」変更するので、
        これでもonlistchangedが二回発生することに注意
        */
        isShufflingHost=true;
        
        for (int i = array.Count - 1; i > 0; i--)
        {
            if(i==1)isShufflingHost=false;
            var j = Random.Range(0, i + 1); // UnityEngineのRandomを使用
            (array[i], array[j]) = (array[j], array[i]); // スワップ
        }
    }

    //メソッド名をわかりやすくするためこのような構造にしている
    void ClientShuffleCards()
    {
        ClientShuffleCardsServerRpc();
    }

    //クライアントがnetworklistに変更を施すには末尾に「ServerRpc」とついた専用のメソッドと、その上に
    //[Unity.Netcode.ServerRpc(RequireOwnership = false)]をつけないとならない。
    [Unity.Netcode.ServerRpc(RequireOwnership = false)]
    void ClientShuffleCardsServerRpc()
    {
        isShufflingClient=true;
        for (int i = ClientNetworkData.Count - 1; i > 0; i--)
        {
            if(i==1)isShufflingClient=false;
            var j = Random.Range(0, i + 1); // UnityEngineのRandomを使用
            (ClientNetworkData[i], ClientNetworkData[j]) = (ClientNetworkData[j], ClientNetworkData[i]); // スワップ
        }
        for (int i = 0; i < ClientNetworkData.Count; i++)
        {
            Debug.Log(ClientNetworkData[i]);
        }
    }

    public override void OnDestroy()
    {
        //networklistはどこかしらのタイミングで破棄されないとエラーを出すため、このオブジェクトが消えたら全てのnetworklistを削除する
        //オブジェクトが消える=接続が切断されたという判断かつ、相手がいなければ試合継続不可能になるため、このようにしている
        HostNetworkData?.Dispose();
        ClientNetworkData?.Dispose();
    }

    //元々networklistが適切に送受信できているかをカードオブジェクトを作って確認するつもりだったが、さまざまな制約や困難があり断念

    //GameObject[] MyCards = new GameObject[3];

    private void Start()
    {
        //scene内にあるtextオブジェクトを取得し、networklistの情報をテキスト表示するための下準備をする
        //クライアント側だとなぜかここの取得ができてなかったりなど非常に不安定
        //逆にエラーなしでちゃんとできる時もある。コードを何もしてないのにも関わらず
        GameObject hostTextObject = GameObject.Find(hostTextObjectName);
            if (hostTextObject != null)
                hostText = hostTextObject.GetComponent<TextMeshProUGUI>();

            GameObject clientTextObject = GameObject.Find(clientTextObjectName);
            if (clientTextObject != null)
                clientText = clientTextObject.GetComponent<TextMeshProUGUI>();

        if (IsHost)
        {
            //ホストとしてplayerが生成された場合、networklistのデータを構築しておく。
            //この時、client側のものもホスト側で処理する。
            for (int i = 0; i < 3; i++) //カードデータ用意
            {
                int cardNum = i + 1;
                HostNetworkData.Add(cardNum);
            }
            ShuffleCards(HostNetworkData);

            for (int i = 0; i < 3; i++) //カードデータ用意
            {
                int cardNum = i + 1;
                ClientNetworkData.Add(cardNum);
            }
            ShuffleCards(ClientNetworkData); //並び替え

            UpdateHostText();
            UpdateClientText();
        }else{
            //クライアントとしてplayerが生成された時点で、networklistはすでに構築されているので、テキストに表示させる処理だけする。
            UpdateHostText();
            UpdateClientText();
        }
    }

    private void Update()
    {
        if (IsOwner == false)
        {
            //オーナーじゃなかったらスキップする(オーナーとはそのplayerオブジェクトが自分のものかどうかを判定するもの)
            //これを介さないと、scene内に存在する全てのplayerオブジェクトが下のコードを実行してしまうので、これで制限している。
            return;
        }

        if (IsHost)
        {
            //ホストとして開始していたら、ホストが持つnetworklistデータをシャッフルする
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                ShuffleCards(HostNetworkData); //並び替え
                //UpdateHostText();
            }
        }
        else
        {
            //クライアントとして開始していたら、クライアントが持つnetworklistデータをシャッフルする
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                ClientShuffleCards(); //並び替え
            }
        }
    }

    private void UpdateHostText()
    {
        //networklistのデータをテキスト化する
        hostText.text = "Host Network Data: ";
        foreach (var item in HostNetworkData)
        {
            hostText.text += item.ToString() + " ";
        }
    }

    private void UpdateClientText()
    {
        //networklistのデータをテキスト化する
        clientText.text = "Client Network Data: ";
        foreach (var item in ClientNetworkData)
        {
            clientText.text += item.ToString() + " ";
        }
    }
}

/*
networklistからデータを取り出したり変更したりする方法について

networklistは名前の通りリスト、つまり動的配列であるため、要素数に制限がなく、AddメソッドやInsertメソッドで要素追加を行う。
逆にnetworklistからデータを取り出したい場合は、networklistに対してforeach文を使ったり、変数名.[0]で0番目の要素を取得できたりなどできる。
取得に関しては制限なく、クライアント側で専用のメソッドを作る必要などもない(おそらく)

networklistにGameObjectを格納する方法については調べても見つからなかった(そもそも不可能か、できてもかなり複雑か)ので
簡単に実装できるint型のみの構造にし、「0番目の要素にはn番のカードがいる」という形でカードの並びを表現する形にした。
なので、「相手の1つ目のカードの番号を知りたい」と思った時は、相手のカードの変数.[0]で取得できる。

当初はこのnetworklistのデータを普通のlistデータにコピーし、その変数をグローバル化することで他のオブジェクトやスクリプトも間接的に
networklistのデータにアクセスできるようにしたかったのだが、うまくいかなかったのでその時点では断念(主に普通のlistをグローバル化する操作ができなかった)
これについてはlistデータをグローバル化する方法はおそらく存在するので、relayの追求が完了したらここも追求したい。

今出ているエラーについて

どうやらClient側だとOnListChangedがトリガーされないことがあるらしく、これに類するバグが起きている
深刻なのはリストデータがクライアント側で正常に表示されないバグ(HostListは最初のデータが、ClientListはそもそもずっと表示されない)が起きている。
逆にいうとこれ以外で不備はない。OnListChangedをクライアント側でもトリガーさせる方法はまだうまく見つかっていない。

上についてはnetworklistをクライアントでもちゃんと初期化するように変更したら治った。
onistchangedがトリガーされていないかどうかはエラーに関係なかったと言える。

同じようなエラーが再発した場合はClientRPCを用いてサーバーがlistの変更を検知したら
サーバーがクライアントに表示メソッドを処理するよう実行を変えてみる方法もあるので、それも考えたい
*/