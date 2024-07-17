using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Random = UnityEngine.Random;
using TMPro;

public class PlayerController : NetworkBehaviour
{
    private NetworkList<int> HostNetworkData;
    private NetworkList<int> ClientNetworkData;
    [SerializeField]
    private string hostTextObjectName = "HostText"; // ホスト用TextMeshProオブジェクトの名前
    private TextMeshProUGUI hostText;

    [SerializeField]
    private string clientTextObjectName = "ClientText"; // クライアント用TextMeshProオブジェクトの名前
    private TextMeshProUGUI clientText;


    void Awake()
    {
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
        Debug.Log("HostNetworkData changed:");
        UpdateHostText();
    }

    private void OnClientNetworkDataChanged(NetworkListEvent<int> changeEvent)
    {
        Debug.Log("ClientNetworkData changed:");
        UpdateClientText();
    }

    void ShuffleCards(NetworkList<int> array)
    {
        for (int i = array.Count - 1; i > 0; i--)
        {
            var j = Random.Range(0, i + 1); // UnityEngineのRandomを使用
            (array[i], array[j]) = (array[j], array[i]); // スワップ
        }
        for (int i = 0; i < array.Count; i++)
        {
            Debug.Log(array[i]);
        }
    }

    void ClientShuffleCards()
    {
        ClientShuffleCardsServerRpc();
    }

    [Unity.Netcode.ServerRpc(RequireOwnership = false)]
    void ClientShuffleCardsServerRpc()
    {
        for (int i = ClientNetworkData.Count - 1; i > 0; i--)
        {
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
        HostNetworkData?.Dispose();
        ClientNetworkData?.Dispose();
    }

    void AddListtoCard(List<int> list, NetworkList<int> array)
    {
        for (int i = 0; i < array.Count; i++)
        {
            list.Add(array[i]);
        }
    }

    void SetListtoCard(List<int> list, NetworkList<int> array)
    {
        for (int i = 0; i < array.Count; i++)
        {
            list[i] = array[i];
        }
    }

    //GameObject[] MyCards = new GameObject[3];

    private void Start()
    {
        //hosttext = GameObject.Find("TextH");
        //clienttext = GameObject.Find("TextC");

        //GameObject[] objects = Resources.LoadAll<GameObject>("Cards");//フォルダCardsにあるプレファブ済みオブジェクトを配列取得

        GameObject hostTextObject = GameObject.Find(hostTextObjectName);
            if (hostTextObject != null)
                hostText = hostTextObject.GetComponent<TextMeshProUGUI>();

            GameObject clientTextObject = GameObject.Find(clientTextObjectName);
            if (clientTextObject != null)
                clientText = clientTextObject.GetComponent<TextMeshProUGUI>();

        if (IsHost)
        {
            //clonetextobj = Instantiate(textobj, new Vector3(-344, 769, 0), Quaternion.identity);
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
        }
    }

    private void Update()
    {
        if (IsOwner == false)
        {
            return;
        }

        if (IsHost)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                ShuffleCards(HostNetworkData); //並び替え
                UpdateHostText();
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                ClientShuffleCards(); //並び替え
            }
        }
    }

    private void UpdateHostText()
    {
        hostText.text = "Host Network Data: ";
        foreach (var item in HostNetworkData)
        {
            hostText.text += item.ToString() + " ";
        }
    }

    private void UpdateClientText()
    {
        clientText.text = "Client Network Data: ";
        foreach (var item in ClientNetworkData)
        {
            clientText.text += item.ToString() + " ";
        }
    }
}
