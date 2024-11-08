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

public class ChooseServerManager : NetworkBehaviour
{
    bool hoststart=false;
    bool clientstart=false;
    TMP_InputField inputField;
    string joinCode;

    async void Awake(){
        InitializationOptions hostOptions = new InitializationOptions().SetProfile("host");
        InitializationOptions clientOptions = new InitializationOptions().SetProfile("client");
       
        await UnityServices.InitializeAsync(hostOptions);
       
        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in: " + AuthenticationService.Instance.PlayerId);
        };

        if (AuthenticationService.Instance.IsAuthorized)
        {
            Debug.Log("Authorized");
            AuthenticationService.Instance.SignOut();
            await UnityServices.InitializeAsync(clientOptions);
        }
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        inputField = GameObject.Find("InputText").GetComponent<TMP_InputField>();

        //サーバーにクライアントが接続したら実行されるメソッド
        //注意として、ホストは「サーバーでもありクライアントでもある」という立ち位置なので
        //ホストが接続を開始した場合もこれが実行される
        //ホスト接続なのにIsClientがtrueになるのはこれが理由
        //基本的に自分がホストかクライアントかを識別したい場合は
        //IsServerを使うといいかも
        NetworkManager.Singleton.OnClientConnectedCallback += ClientConnected;
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
            SceneManager.LoadScene("CardSortingGame"); 
        }else{
            if(NetworkManager.Singleton.ConnectedClients.Count > 1){
                SceneManager.LoadScene("CardSortingGame"); 
            }
        }
    }

    public void InputName()
    {
        joinCode = inputField.text;//テキストフィールドに文字が入れられるたびにその文字がcodeに代入される
    }
    
    public async void ToHost(){
        if(hoststart==false & clientstart==false){
            hoststart=true;
            try
            {
                NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
                
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(1);

                string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

                RelayServerData relayServerData = new RelayServerData(allocation, "wss");
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

                Debug.Log(joinCode);

                NetworkManager.Singleton.StartHost();
            }
            catch (RelayServiceException e)
            {
                Debug.Log(e);
                hoststart=false;
            }
        }
    }

    public async void ToClient(){
        if(hoststart==false & clientstart==false){
            clientstart=true;
            try {
                Debug.Log($"Joining... (code: {joinCode})");

                JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

                RelayServerData relayServerData = new RelayServerData(joinAllocation, "wss");
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

                NetworkManager.Singleton.StartClient();
            }
            catch (RelayServiceException e) {
                Debug.LogException(e);
                Debug.Log("接続に失敗しました!");
                clientstart=false;
            }
        }
    }

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
