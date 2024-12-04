using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChooseServerManager : NetworkBehaviour
{
    bool hoststart=false;
    bool clientstart=false;
    TMP_InputField inputField;
    string joinCode="";
    private TextMeshProUGUI codeText;

    GameObject waitObj;
    GameObject joinButton;
    GameObject backButton;
    private GameObject connectPanel;
    private Button roomJoinButtonComponent;
    private Button backButtonComponent;
    public GameObject connectionErrorImage;

    private const string PASSWORD_CHARS = 
        "0123456789abcdefghijklmnopqrstuvwxyz";

    public AudioClip decideSound;
    public AudioClip cancelSound;
    public GameObject SoundObject;
    public GameObject soundManager;

    public GameObject loadObject;

    public static string GeneratePassword( int length )
    {
        var sb  = new System.Text.StringBuilder( length );
        var r   = new System.Random();

        for ( int i = 0; i < length; i++ )
        {
            int     pos = r.Next( PASSWORD_CHARS.Length );
            char    c   = PASSWORD_CHARS[ pos ];
            sb.Append( c );
        }

        return sb.ToString();
    }

    void Awake(){
        inputField = GameObject.Find("InputText").GetComponent<TMP_InputField>();

        waitObj = GameObject.Find("ConnectWaiting");
        joinButton = GameObject.Find("RoomJoinButton");
        connectPanel = GameObject.Find("ConnectPanel");
        backButton = GameObject.Find("BackButton");
        connectionErrorImage = connectPanel.transform.Find("ConnectionErrorImage").gameObject;
        connectionErrorImage.SetActive(false);

        if(GameObject.Find("SoundManager")==null){
            GameObject obj=Instantiate(soundManager);
            obj.name = soundManager.name;
        }

        if (joinButton != null)
        {
            roomJoinButtonComponent = joinButton.GetComponent<Button>();
            if (roomJoinButtonComponent != null)roomJoinButtonComponent.onClick.AddListener(OnRoomJoinButtonClicked);
        }
        if (backButton != null)
        {
            backButtonComponent = backButton.GetComponent<Button>();
            if (backButtonComponent != null)
            {
                backButtonComponent.onClick.AddListener(OnBackButtonClicked);
                Debug.Log("Listener added to BackButton.");
            }
        }
        
        GameObject codeTextObject = GameObject.Find("JoinCodeText");//sceneからjoincodeを表示するテキストを取得
        if (codeTextObject != null) codeText = codeTextObject.GetComponent<TextMeshProUGUI>();
        
        waitObj.SetActive(false);
        connectPanel.SetActive(false);
        loadObject.SetActive(false);
    }

    async void Start(){
       
        await UnityServices.InitializeAsync();
       
        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in: " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();


        NetworkManager.Singleton.OnClientConnectedCallback += ClientConnected;
    }

    private void OnRoomJoinButtonClicked()
    {
        if (connectPanel != null)
        {
            connectPanel.SetActive(true);
            GameObject soundobj=Instantiate(SoundObject);
            soundobj.GetComponent<PlaySound>().PlaySE(decideSound);
        }
    }
    
    public void OnBackButtonClicked()
    {
        if (connectPanel != null)
        {
            connectPanel.SetActive(false);
            GameObject soundobj=Instantiate(SoundObject);
            soundobj.GetComponent<PlaySound>().PlaySE(cancelSound);
        }
    }
    
    private void ClientConnected(ulong clientId)
    {
        /*
        クライアント接続の場合は直接移動
        ホストの場合はサーバー内にいるクライアントの数がホスト自身を含め2以上になったら
        移動する
        */
        Debug.Log("テスト");

        if(clientstart){
            NetworkManager.Singleton.SceneManager.LoadScene("CardSortingGame", LoadSceneMode.Single);
        }else{
            if(NetworkManager.Singleton.ConnectedClients.Count > 1){
                waitObj.SetActive(false);
                loadObject.SetActive(true);
                NetworkManager.Singleton.SceneManager.LoadScene("CardSortingGame", LoadSceneMode.Single);
            }
        }
    }
    
    public void InputName()
    {
        joinCode = inputField.text;//テキストフィールドに文字が入れられるたびにその文字がcodeに代入される
    }

    public void EndHostConnect(){
        if(hoststart){
            hoststart=false;
            NetworkManager.Singleton.Shutdown();
            waitObj.SetActive(false);
            codeText.text="接続を中断しました";
            GameObject soundobj=Instantiate(SoundObject);
            soundobj.GetComponent<PlaySound>().PlaySE(cancelSound);
        }
    }
    
    public async void ToHost(){
        if(hoststart==false & clientstart==false){
            hoststart=true;
            GameObject soundobj=Instantiate(SoundObject);
            soundobj.GetComponent<PlaySound>().PlaySE(decideSound);
            try
            {
                NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
                
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(1);

                string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

                RelayServerData relayServerData = new RelayServerData(allocation, "wss");
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
                /*NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
                );*/

                Debug.Log(joinCode);

                codeText.text=$"{joinCode}";

                NetworkManager.Singleton.StartHost();

                waitObj.SetActive(true);
            }
            catch (RelayServiceException e)
            {
                Debug.Log(e);
                hoststart=false;
                codeText.text="ホストとして部屋を建てられませんでした";
            }
        }
    }

    public async void ToClient(){
        if(hoststart==false & clientstart==false){
            GameObject soundobj=Instantiate(SoundObject);
            soundobj.GetComponent<PlaySound>().PlaySE(decideSound);
            if(joinCode==""){
                return;
            }
            connectionErrorImage.SetActive(false);
            clientstart=true;
            try {
                Debug.Log($"Joining... (code: {joinCode})");

                JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

                RelayServerData relayServerData = new RelayServerData(joinAllocation, "wss");
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

                NetworkManager.Singleton.StartClient();

                codeText.text="接続中です...";

                loadObject.SetActive(true);
            }
            catch (RelayServiceException e) {
                Debug.LogException(e);
                Debug.Log("接続に失敗しました!");
                clientstart=false;
                codeText.text="入室に失敗しました";
                connectionErrorImage.SetActive(true);
            }
        }
    }

    /*
    void Update(){
        if(IsServer){
            Debug.Log("テストです");
            if(NetworkManager.Singleton.ConnectedClients.Count > 1){
                SceneManager.LoadScene("CardSortingGame"); 
            }
        }
    }*/

    //クライアントがホストに接続する際にする事前処理
    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        // 追加の承認手順が必要な場合は、追加の手順が完了するまでこれを true に設定します
        // true から false に遷移すると、接続承認応答が処理されます。
        response.Pending = true;

        //最大人数をチェック(この場合はホスト含めて2人まで)
        if (NetworkManager.Singleton.ConnectedClients.Count >= 3)
        {
            response.Approved = false;//接続を許可しない
            response.Pending = false;
            return;
        }

        //ここからは接続成功クライアントに向けた処理
        response.Approved = true;//接続を許可

        response.Pending = false;
    }
}
