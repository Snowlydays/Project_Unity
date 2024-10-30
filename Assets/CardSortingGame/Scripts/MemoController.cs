using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MemoController : MonoBehaviour
{
	[SerializeField]private RawImage m_image = null;
	private Texture2D m_texture;
    [SerializeField]private int m_width = 4;
    [SerializeField]private int m_height = 4;
    private Vector2 m_prePos;
    private Vector2 m_touchPos;
    private float m_clickTime, m_preClickTime;

	public void OnDrag(BaseEventData arg) //線を描画
    {
        PointerEventData _event = arg as PointerEventData; //タッチの情報取得
        
        // 押されているときの処理
		// スクリーン座標をテクスチャ上の座標に変換
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            m_image.rectTransform,
            _event.position,
            _event.pressEventCamera,
            out m_touchPos
        ); //現在のポインタの座標
        m_touchPos += new Vector2(m_texture.width / 2, m_texture.height / 2);// ローカル座標をテクスチャの左下基準に変換
		m_clickTime = _event.clickTime; //最後にクリックイベントが送信された時間を取得

        float disTime = m_clickTime - m_preClickTime; //前回のクリックイベントとの時差

        int width  = m_width;  //ペンの太さ(ピクセル)
        int height = m_height; //ペンの太さ(ピクセル)

        var dir  = m_prePos - m_touchPos; //直前のタッチ座標との差
        if(disTime > 0.01) dir = new Vector2(0,0); //0.1秒以上間隔があいたらタッチ座標の差を0にする

        var dist = (int)dir.magnitude; //タッチ座標ベクトルの絶対値

        dir = dir.normalized; //正規化
        
        //指定のペンの太さ(ピクセル)で、前回のタッチ座標から今回のタッチ座標まで塗りつぶす
        for(int d = 0; d < dist; d++)
        {
            var p_pos = m_touchPos + dir * d; //paint position
            p_pos.y -= height/2.0f;
            p_pos.x -= width/2.0f;
            for(int h = 0; h < height; h++)
            {
                int y = (int)(p_pos.y + h);
                if(y < 0 || y > m_texture.height) continue; //タッチ座標がテクスチャの外の場合、描画処理を行わない

                for(int w = 0; w < width; w++)
                {
                    int x = (int)(p_pos.x + w);
                    if(x >= 0 && x <= m_texture.width)
                    {
                        m_texture.SetPixel(x, y, Color.black); //線を描画
                    }
                }
            }
        }
        m_texture.Apply();
        m_prePos = m_touchPos;
        m_preClickTime = m_clickTime;
	}

    public void OnTap(BaseEventData arg) //点を描画
    {
        PointerEventData _event = arg as PointerEventData; //タッチの情報取得
        
        // 押されているときの処理
		RectTransformUtility.ScreenPointToLocalPointInRectangle(
            m_image.rectTransform,
            _event.position,
            _event.pressEventCamera,
            out m_touchPos
        );//現在のポインタの座標
        m_touchPos += new Vector2(m_texture.width / 2, m_texture.height / 2);// ローカル座標をテクスチャの左下基準に変換
        int width = m_width;  //ペンの太さ(ピクセル)
        int height = m_height; //ペンの太さ(ピクセル)

        var p_pos = m_touchPos; //paint position
        p_pos.y -= height/2.0f;
        p_pos.x -= width/2.0f;

        for(int h = 0; h < height; h++)
        {
            int y = (int)(p_pos.y + h);
            if(y < 0 || y > m_texture.height)continue; //タッチ座標がテクスチャの外の場合、描画処理を行わない
            for(int w = 0; w < width; w++)
            {
                int x = (int)(p_pos.x + w);
                if(x >= 0 && x <= m_texture.width)
                {
                    m_texture.SetPixel(x, y, Color.black); //点を描画
                }
            }
        }       
        m_texture.Apply();
    }

    private void Start()
    {
        var rect = m_image.gameObject.GetComponent<RectTransform>().rect;
        m_texture = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGBA32, false);

        m_image.texture = m_texture;
    }
}