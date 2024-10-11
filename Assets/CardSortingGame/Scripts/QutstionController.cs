using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class QutstionController : MonoBehaviour
{
    GameObject bg;
    public GameObject confirmButton;  // 決定ボタンの参照
    private List<GameObject> selectedCards = new List<GameObject>();  // 選択されたカードのリスト
    private Color originalColor = Color.white;  // デフォルトのカードの色
    private Color selectedColor = Color.yellow; // 選択されたカードの色

    // Start is called before the first frame update
    void Start()
    {
        bg = GameObject.Find("QuestioningBG");

        confirmButton = GameObject.Find("ConfirmButton");
    }

    private GameObject selectedCard1 = null;
    private GameObject selectedCard2 = null;
    
    // phaseの値によってオブジェクトのActive状態を変更する関数
    void ManageActive()
    {
        if(NetworkSystem.phase == 2 && !bg.activeSelf)
        {
            bg.SetActive(true);
            
            // MainSystemScriptからクローンカードを取得
            MainSystemScript mainSystem = FindObjectOfType<MainSystemScript>();
            GameObject[] clonedCards = mainSystem.CloneMyCardsAsUI(); // クローンカードを取得
            
            if(clonedCards == null)
            {
                Debug.LogError("clonedCards are null!");
            }

            // キャンバスを探す
            Canvas canvas = GameObject.Find("QuestionCanvas").GetComponent<Canvas>();
            
            foreach(GameObject card in clonedCards)
            {
                // キャンバスにカードを追加
                card.transform.SetParent(canvas.transform);

                // カードをボタンとして設定
                Button cardButton = card.GetComponent<Button>();
                cardButton.onClick.AddListener(() => ToggleCardSelection(card));
            }
            
            // 決定ボタンのクリックイベント設定
            confirmButton.GetComponent<Button>().onClick.AddListener(CompareSelectedCards);
        }
        else if(NetworkSystem.phase != 2 && bg.activeSelf) 
        {
            // クローンカードのUIを削除する処理
            GameObject[] clonedCards = GameObject.FindGameObjectsWithTag("ClonedCard");
            foreach (GameObject card in clonedCards)Destroy(card);

            bg.SetActive(false);
        }
    }
    
    // カード選択状態の切り替え関数
    void ToggleCardSelection(GameObject card)
    {
        // カードが既に選択されている場合、選択を解除する
        if(selectedCards.Contains(card))
        {
            DeselectCard(card);
        }
        else
        {
            // 2枚を超えた選択は許可しない
            if(selectedCards.Count < 2)SelectCard(card);
        }
    }

    void SelectCard(GameObject card)
    {
        Debug.Log(selectedCards.Count);
        // カードを選択状態にし、色を変更
        selectedCards.Add(card);
        card.GetComponent<Image>().color = selectedColor;  // 色を変更
    }
    
    void DeselectCard(GameObject card)
    {
        // カードの選択を解除し、色を元に戻す
        selectedCards.Remove(card);
        card.GetComponent<Image>().color = originalColor;  // 元の色に戻す
    }

    void CheckSelection()
    {
        if (selectedCard1 != null && selectedCard2 != null)
        {
            Debug.Log("Selected cards: " + selectedCard1.name + " and " + selectedCard2.name);

            // 比較結果を表示する処理
            CompareCards(selectedCard1, selectedCard2);
        }
    }

    void OnCardClicked(GameObject card)
    {
        SelectCard(card);
        CheckSelection();
    }

    void CompareSelectedCards()
    {
        if (selectedCards.Count == 2)
        {
            CompareCards(selectedCards[0], selectedCards[1]);
            
            // 比較後に選択状態をリセット
            foreach (GameObject card in selectedCards)
            {
                card.GetComponent<Image>().color = originalColor;  // 色を元に戻す
            }
            selectedCards.Clear();
        }
        else
        {
            Debug.Log("Please select exactly two cards to compare.");
        }
    }

    void CompareCards(GameObject card1, GameObject card2)
    {
        // カード比較ロジックをここに実装
        Debug.Log("Comparing " + card1.name + " with " + card2.name);
    }
    
    // Update is called once per frame
    void Update()
    {
        ManageActive();

        // クリックして選択する
        if(Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if(hit.collider != null && hit.collider.gameObject.CompareTag("ClonedCard"))
            {
                SelectCard(hit.collider.gameObject);
                CheckSelection();
            }
        }
    }
}
