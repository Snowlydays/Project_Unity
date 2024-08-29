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
using TMPro;

public class ManagerUIscript : MonoBehaviour
{
    //ここでrelayの接続をしている。
    TMP_InputField inputField;//これはjoincodeを入力するためのテキストフィールド
    private TextMeshProUGUI codeText;//joincodeをテキスト表示するためのテキストオブジェクト
    bool connected=false;//ホストかクライアントで既に接続したかどうかのbool変数
    string Code;//joincodeを格納する変数
    async void Awake(){
        await UnityServices.InitializeAsync();//UnityServicesの初期化 これをしないとダメらしい

        //networklistのOnlistChangedのように、ユーザーのサインインが確認されたら以下のラムダ式が実行されるようになっている。
        AuthenticationService.Instance.SignedIn += () =>
        {
            //ユーザーサインイン時の処理
            Debug.Log("Signed In " + AuthenticationService.Instance.PlayerId);
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();//匿名サインイン

        inputField = GameObject.Find("InputText").GetComponent<TMP_InputField>();//sceneからテキストフィールドを取得

        GameObject codeTextObject = GameObject.Find("CodeText");//sceneからjoincodeを表示するテキストを取得
            if (codeTextObject != null)
                codeText = codeTextObject.GetComponent<TextMeshProUGUI>();
        codeText.text = "JoinCode: ";
    }

    public void InputName()
    {
        Code = inputField.text;//テキストフィールドに文字が入れられるたびにその文字がcodeに代入される
    }
    
    void Update()
    {
        //スペースを押したらホストとして、バックスペースを押したらクライアントとして接続を開始する。
        //joincodeで文字を入力する都合上、キー入力で行うのは現状不便なので、いずれは
        //クリック形式で行えるようにしたい
        if(connected==false){
            if(Input.GetKeyDown(KeyCode.Space)){
                //NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
                CreateHost();
            }
            if(Input.GetKeyDown(KeyCode.Backspace)){
                JoinClient(Code);
            }
        }
    }


    public async void CreateHost(){
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(1);//アロケーションの作成
            //アロコーションとは部屋のようなもので、各自が接続をする空間を用意するようなもの。第一引数でクライアントの接続人数を設定できる
            //ここでは1対1のカードゲームなので、ホストで一人、クライアントで一人の計二人を接続可能人数に設定

            //部屋に入るためのコードを取得
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            //ここで
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
                );
            
            Debug.Log(joinCode);
            codeText.text = "JoinCode:";
            codeText.text += joinCode;
            NetworkManager.Singleton.StartHost();
            connected=true;
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void JoinClient(string joinCode)
	{
		try {
			// コードをもとに部屋に参加
			Debug.Log($"Joining... (code: {joinCode})");

            //ここでjoincodeを元にホストが開いている部屋に参加する
            //ちなみに自身はここの部屋に参加する部分で404 not foundエラーが出て原因究明に頭を悩ませていたが、
            //公式のドキュメントを見ながらパッケージを再インストールしたらなぜか普通にできた。
            //パッケージが正常にインストールできていなかったか、その後networkManagerのプレファブを直したりしたことでうまくいった?詳しい原因は不明
            //今後再び404エラーが出る可能性もある
			JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

			NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
                );

            NetworkManager.Singleton.StartClient();

            connected=true;
		}
		catch (RelayServiceException e) {
			Debug.LogException(e);
            Debug.Log("接続に失敗しました!");
		}
	}

    //クライアントがホストに接続する際にする事前処理
    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        // 追加の承認手順が必要な場合は、追加の手順が完了するまでこれを true に設定します
        // true から false に遷移すると、接続承認応答が処理されます。
        response.Pending = false;

        //最大人数をチェック(この場合はホスト含めて2人まで)
        if (NetworkManager.Singleton.ConnectedClients.Count >= 3)
        {
            response.Approved = false;//接続を許可しない
            response.Pending = false;
            return;
        }

        //ここからは接続成功クライアントに向けた処理
        response.Approved = true;//接続を許可

        //PlayerObjectを生成するかどうか
        response.CreatePlayerObject = true;

        //生成するPrefabハッシュ値。nullの場合NetworkManagerに登録したプレハブが使用される
        response.PlayerPrefabHash = null;

        //PlayerObjectをスポーンする位置(nullの場合Vector3.zero)
        var position = new Vector3(0, 0, 0);
        response.Position = position;

        //PlayerObjectをスポーン時の回転 (nullの場合Quaternion.identity)
        response.Rotation = Quaternion.identity;

        response.Pending = false;
    }
}