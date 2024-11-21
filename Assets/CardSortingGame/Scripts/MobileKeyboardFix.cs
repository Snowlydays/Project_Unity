using UnityEngine;
using TMPro;

public class MobileKeyboardFix : MonoBehaviour
{
    private TMP_InputField inputField;
    private TouchScreenKeyboard keyboard;

    void Start()
    {
        inputField = GetComponent<TMP_InputField>();
        
        // TextMeshProのイベントリスナーに独自のフォーカスハンドラーを追加
        inputField.onSelect.AddListener(OpenKeyboardOnMobile);
    }

    void OpenKeyboardOnMobile(string text)
    {
        if (Application.isMobilePlatform)
        {
            // スマートフォンの場合、キーボードを開く
            keyboard = TouchScreenKeyboard.Open(inputField.text, TouchScreenKeyboardType.Default);
        }
    }
}