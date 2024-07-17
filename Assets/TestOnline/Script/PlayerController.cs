using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Random = UnityEngine.Random;
using UnityEngine.UI;
using TMPro;

public class PlayerController : NetworkBehaviour
{
    private NetworkList<int> HostNetworkData;
    private NetworkList<int> ClientNetworkData;
    //[SerializeField]
    //private GameObject textobj;
    //GameObject clonetextobj;
    //public GameObject clienttext = GameObject.Find("TextC")
    void Awake()
    {
        HostNetworkData = new NetworkList<int>();
        ClientNetworkData = new NetworkList<int>();


    }
    void ShuffleCards(NetworkList<int> array)
    {
        for (int i = array.Count - 1; i > 0; i--)
        {
            var j = Random.Range(0, i + 1); // UnityEnginのRandomを使用
            (array[i], array[j]) = (array[j], array[i]); // スワップ
        }
        for (int i = 0; i < array.Count; i++)
        {
            Debug.Log(array[i]);
        }
    }
    void ClientShuffleCards(){
        ClientShuffleCardsServerRpc();
    }
    [Unity.Netcode.ServerRpc(RequireOwnership = false)]
    void ClientShuffleCardsServerRpc()
    {
        for (int i = ClientNetworkData.Count - 1; i > 0; i--)
        {
            var j = Random.Range(0, i + 1); // UnityEnginのRandomを使用
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

    void AddListtoCard(List<int> list,NetworkList<int> array)
    {
        for (int i = 0; i < array.Count; i++)
        {
            list.Add(array[i]);
        }
    }

    void SetListtoCard(List<int> list,NetworkList<int> array)
    {
        for (int i = 0; i < array.Count; i++)
        {
            list[i]=array[i];
        }
    }

    //GameObject[] MyCards = new GameObject[3];

    private void Start()
    {
        //hosttext = GameObject.Find("TextH");
        //clienttext = GameObject.Find("TextC");
        
        //GameObject[] objects = Resources.LoadAll<GameObject>("Cards");//フォルダCardsにあるプレファブ済みオブジェクトを配列取得

        if(IsHost)
        {
            //clonetextobj = Instantiate(textobj,new Vector3(-344,769,0),Quaternion.identity);
            for (int i = 0; i < objects.Length; i++)//カードデータ用意
            {
                int cardNum = i+1;
                HostNetworkData.Add(cardNum);
            }
            ShuffleCards(HostNetworkData);
            //AddListtoCard(CardController.HostCardList,HostNetworkData);
            /*for(int i=0;i<HostNetworkData.Count;i++)//ゲーム画面上に生成
            {
                GameObject instance = Instantiate(objects[HostNetworkData[i]-1],
                    new Vector3(-1.2f+1.2f*i,-2.0f,0.0f),
                    Quaternion.identity);
                MyCards[i]=instance;
                var networkObj = instance.GetComponent<NetworkObject>();
                networkObj.Spawn(true);
            }*/

            for (int i = 0; i < objects.Length; i++)//カードデータ用意
            {
                int cardNum = i+1;
                ClientNetworkData.Add(cardNum);
            }
            ShuffleCards(ClientNetworkData);//並び替え
            //AddListtoCard(CardController.ClientCardList,ClientNetworkData);
            /*HostNetworkData.OnListChanged += (NetworkListEvent<int> changed) =>
            {
                    Debug.Log(string.Join(",", changed.Value));
            };
            ClientNetworkData.OnListChanged += (NetworkListEvent<int> changed) =>
            {
                    Debug.Log(string.Join(",", changed.Value));
            };*/
        }else{
            /*for(int i=0;i<ClientNetworkData.Count;i++)//ゲーム画面上に生成
            {
                GameObject instance = Instantiate(objects[ClientNetworkData[i]-1],
                    new Vector3(-1.2f+1.2f*i,2.0f,0.0f),
                    Quaternion.identity);
                MyCards[i]=instance;
                var networkObj = instance.GetComponent<NetworkObject>();
                networkObj.Spawn(true);
            }*/
            /*for (int i = 0; i < objects.Length; i++)//カードデータ用意
            {
                int cardNum = int.Parse(objects[i].name.Substring(5));
                ClientNetworkData.Value[i] = cardNum;
            }
            for (int i = 0; i < ClientNetworkData.Value.Length; i++)Debug.Log(ClientNetworkData.Value[i]);
            ShuffleCards(ClientNetworkData.Value);//並び替え
            for (int i = 0; i < ClientNetworkData.Value.Length; i++)Debug.Log(i.ToString()+ " "+ClientNetworkData.Value[i].ToString());
            for(int i=0;i<ClientNetworkData.Value.Length;i++)//ゲーム画面上に生成
            {
                ClientCards[i].Add(
                    Instantiate(objects[ClientNetworkData.Value[i]-1],
                    new Vector3(-1.2f+1.2f*i,-2.0f,0.0f),
                    Quaternion.identity);
                );
            }*/
        }
    }
    private void Update()
    {
        
        if (IsOwner == false)
        {
            return;
        }

        if(IsHost)
        {
            if(Input.GetKeyDown(KeyCode.UpArrow)){
                ShuffleCards(HostNetworkData);//並び替え
                //SetListtoCard(CardController.HostCardList,HostNetworkData);
                /*for (int i = 0; i < HostNetworkData.Count; i++)
                {
                    Vector3 pos = new Vector3(-1.2f+1.2f*(HostNetworkData[i]-1),-2.0f,0.0f);
                    MyCards[i].transform.position = pos;
                }*/
            }
            /*clonetextobj.GetComponent<TextMeshProUGUI>().text ="";
            if(HostNetworkData?.Count>0){
                for (int i = 0; i < HostNetworkData.Count; i++)
                {
                    clonetextobj.GetComponent<TextMeshProUGUI>().text +=HostNetworkData[i].ToString() + " ";
                }
            }*/
        }else{
            if(Input.GetKeyDown(KeyCode.UpArrow)){
                ClientShuffleCards();//並び替え
                //SetListtoCard(CardController.ClientCardList,ClientNetworkData);
                /*for (int i = 0; i < ClientNetworkData.Count; i++)
                {
                    Vector3 pos = new Vector3(-1.2f+1.2f*(ClientNetworkData[i]-1),2.0f,0.0f);
                    MyCards[i].transform.position = pos;
                }*/
            }
        }
    }
}