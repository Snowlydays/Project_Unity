using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class Helper : MonoBehaviour
{

    [DllImport("__Internal")]
    private static extern void SetCookieJS(string key,float value);
 
    [DllImport("__Internal")]
    private static extern string GetCookieValueJS(string key);

    //エディター上ではcookie関連の関数は使えないことに留意
    public static int SetCookie(string key,float value){
        //cookieに値を設定するメソッド
        if(CheckWebGLPlatform())SetCookieJS(key,value);
        return 0;
    }

    public static float GetCookieValue(string key){
        //cookieから所定のキーを検索し、該当するキーがあったら紐づいている値を返す
        //見つからなければ-1を返す
        if(CheckWebGLPlatform()){
            string valuestr=GetCookieValueJS(key);
            if(valuestr!=null){
                return float.Parse(valuestr);
            }else{
                return -1f;
            }
        }else{
            return -1f;
        }
    }

    //html上でゲームが起動しているかどうか確認するメソッド
    public static bool CheckWebGLPlatform()
    {
        return Application.platform == RuntimePlatform.WebGLPlayer;
    }
}