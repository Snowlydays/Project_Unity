using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainSystemScript : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]public GameObject CardObject;//カードのオブジェクト
    //オブジェクトはプレファブ化して単一で管理、操作したい場合は以下の配列で行う。
    //後々これをビリヤードよろしく自作クラスで管理できると上々



    GameObject[] mycard = new GameObject[7];//自分の手札
    GameObject[] othercard = new GameObject[7];//相手の手札
    void Start()
    {
        //開始時にカードオブジェクトのクローンを作成、配列で管理して並び替え等可能にしていきたい。
        for(int i = 0; i < 7; i++)
        {
            mycard[i]=Instantiate(CardObject, new Vector3(1.5f*(float)(3-i),-0.75f,0.0f), Quaternion.identity);
            mycard[i].GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 0f);
            othercard[i]=Instantiate(CardObject, new Vector3(1.5f*(float)(3-i),1.05f,0.0f), Quaternion.identity);
            othercard[i].GetComponent<SpriteRenderer>().color = new Color(1f, 0f, 0f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //メインカメラを取得し、フェーズによってその背景色を変更する。
        //フェーズの取得はNetworkSystemが管理するphase変数から参照する。
        if(NetworkSystem.phase==2){
            Camera.main.backgroundColor = new Color(255f, 152f/255f, 226f/255f);
        }else{
            Camera.main.backgroundColor = new Color(1f, 1f, 1f);
        }
    }


    //networkvariableはアイテムが配られる時、アイテムを使用するとき、詠唱をするときにのみ使い、
    //それ以外の場面では使わない(使う必要がない)
}

/*追記

ちょっと調べてみたら、エディターにおけるGameのプレビュー画面の左上タブにある「Game」を「Simulator」
に変更することでスマホ等の多機種で表示した際にどんな画面になるかがわかるようになっていた。
また、画面サイズを拡大縮小する際、canvas上のものはアスペクト比に則る設定にすれば自動的にそのサイズに合うように
UIなどのサイズを変えてくれるが、canvas上で描画しないオブジェクト類はこの影響を受けないため、
例えば画面を小さくするとカードのサイズだけそのままにUIが縮小するため、相対的にカードのサイズが
大きくなる問題が発生することなども発見した(Simulatorでスマホ機種で表示してみるとカードだけ非常に大きくなる)

また、CanvasのRender Modeを「Screen Space - Camera」にしてMain Cameraを対象にするとCamera上に
エディターで直接配置できる(右上に馬鹿でかい枠が出て来ず、それの中で操作をする必要がない)旨のことを言ったが、
これをそのまま「Screen Space - Overlay」にすると右上の馬鹿でかい枠にScreen Space - Cameraで置いた配置、比率と全く同じ形に直して
置いてくれる仕様も見つけたので、canvas上で描画されないオブジェクトと合わせて配置を考えたい時に活用できそうにも思える。

なお、Screen Space - OverlayにしないとUIが一番上に表示されない(カードオブジェクトがUIより前面に表示される)問題が起きたので、
完成形についてはcanvasのRender ModeをScreen Space - Overlayにするのが妥当と思われる。
*/