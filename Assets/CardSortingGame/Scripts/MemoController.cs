using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MemoController : MonoBehaviour
{
	private Texture2D m_texture;
    private Vector2 m_prePos;
    private Vector2 m_touchPos;
    private float m_clickTime, m_preClickTime;
    private bool isEraserMode = false;
    private Color defaultColor;
    private Color penColor;
	[SerializeField]private RawImage m_image = null;
    [SerializeField]private int penWidth = 8;
    [SerializeField]private int penHeight = 8;
    [SerializeField]private int eraserWidth = 30;
    [SerializeField]private int eraserHeight = 30;

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

        Color drawColor;
        int width; //ペンの太さ(ピクセル)
        int height; //ペンの太さ(ピクセル)
        // ペンと消しゴムを切り替え
        if(isEraserMode)
        {
            drawColor = defaultColor;
            width = eraserWidth;
            height = eraserHeight;
        }
        else
        {
            drawColor = penColor;
            width = penWidth;
            height = penHeight;
        }
        
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
                        m_texture.SetPixel(x, y, drawColor); //線を描画
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

        int width; //ペンの太さ(ピクセル)
        int height; //ペンの太さ(ピクセル)
        // ペンと消しゴムを切り替え
        if(isEraserMode)
        {
            width = eraserWidth;
            height = eraserHeight;
        }
        else
        {
            width = penWidth;
            height = penHeight;
        }
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
                    m_texture.SetPixel(x, y, (isEraserMode ? defaultColor : penColor)); //点を描画
                }
            }
        }
        m_texture.Apply();
    }

    // ペンと消しゴムを切り替える関数
    public void SetPenMode()
    {
        isEraserMode = false;
        Debug.Log("pen mode");
    }

    public void SetEraserMode()
    {
        isEraserMode = true;
        Debug.Log("eraser mode");
    }

    //テクスチャを初期化する関数
    public void DefaultTexture()
    {
        var rect = m_image.gameObject.GetComponent<RectTransform>().rect;
        // デフォルトの色をセット
        defaultColor = new Color(1f, 1f, 1f, 0f);
        penColor = new Color(0.15f, 0.4f, 0.8f, 1f);
        for(int w = 0; w < (int)rect.width; w++)
        {
            for (int h = 0; h < (int)rect.height; h++)
            {
                m_texture.SetPixel(w, h, defaultColor);
            }
        }
        m_texture.Apply();
    }

    private void Start()
    {
        var rect = m_image.gameObject.GetComponent<RectTransform>().rect;
        m_image.color=new Vector4(1f,1f,1f,1f);
        m_texture = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGBA32, false);
        m_image.texture = m_texture;

        DefaultTexture();
    }
}