using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class ItemLongPressHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    public float longPressThreshold = 0.5f; // 長押しとみなす時間（秒）

    public string itemDescription; // アイテムの説明
    public GameObject descriptionPopupPrefab; // 説明ポップアップのプレハブ

    private bool isPressed = false;
    private Coroutine longPressCoroutine;
    private GameObject activePopup;

    [SerializeField] private Transform popupParentTransform;

    public static GameObject currentActivePopup = null;

    // 長押しが検出されたかどうかの真偽値
    private bool hasLongPressed = false;

    private Toggle toggle;
    
    public void Initialize(string description, GameObject popupPrefab, Transform popupParent)
    {
        itemDescription = description;
        descriptionPopupPrefab = popupPrefab;
        popupParentTransform = popupParent;
    }

    public bool HasLongPressed
    {
        get { return hasLongPressed; }
        set { hasLongPressed = value; }
    }

    private void Awake()
    {
        toggle = GetComponent<Toggle>();
        if (toggle == null)
        {
            Debug.LogError("Toggleコンポーネントが見つかりません。");
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        hasLongPressed = false;
        longPressCoroutine = StartCoroutine(LongPress());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
        if (longPressCoroutine != null)
        {
            StopCoroutine(longPressCoroutine);
        }

        if (hasLongPressed)
        {
            // 長押しが検出された場合、トグルの状態変更を防ぐ
            eventData.pointerPress = null;
            eventData.eligibleForClick = false;
            hasLongPressed = false;
            eventData.Use();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (hasLongPressed)
        {
            // 長押しが検出された場合、タップイベントを無視してイベントを消費する
            hasLongPressed = false;
            eventData.Use();
            return;
        }

        // ポップアップが表示されている場合は閉じる
        if (activePopup != null)
        {
            ClosePopup();
        }
    }

    private IEnumerator LongPress()
    {
        yield return new WaitForSeconds(longPressThreshold);
        if (isPressed)
        {
            hasLongPressed = true;
            ToggleDescriptionPopup();
        }
    }

    private void ToggleDescriptionPopup()
    {
        if (currentActivePopup != null && currentActivePopup != activePopup)
        {
            // 別のポップアップが開いている場合は閉じる
            Destroy(currentActivePopup);
            currentActivePopup = null;
        }

        if (activePopup == null)
        {
            ShowDescriptionPopup();
        }
    }

    private void ShowDescriptionPopup()
    {
        if (descriptionPopupPrefab != null && itemDescription != null && popupParentTransform != null)
        {
            activePopup = Instantiate(descriptionPopupPrefab, popupParentTransform);

            RectTransform popupRect = activePopup.GetComponent<RectTransform>();
            RectTransform toggleRect = GetComponent<RectTransform>();

            Vector3 toggleWorldPos = toggleRect.position;
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                popupParentTransform as RectTransform,
                toggleWorldPos,
                null,
                out localPoint
            );

            // ポップアップの位置を設定
            float offsetY = (toggleRect.rect.height * 0.5f) + (popupRect.rect.height * 0.5f) - 10f;
            Vector2 offset = new Vector2(0, offsetY);

            popupRect.anchoredPosition = localPoint + offset;

            // アイテムの説明を設定
            TMP_Text itemDescText = activePopup.GetComponentInChildren<TMP_Text>();
            itemDescText.text = itemDescription;
            
            // Image descImage = activePopup.GetComponentInChildren<Image>();
            // if (descImage != null)
            // {
            //     descImage.sprite = itemDescription;
            //     descImage.SetNativeSize();
            // }

            activePopup.transform.SetAsLastSibling();
            currentActivePopup = activePopup;

            Debug.Log("DescriptionPopup表示させました。");
        }
    }


    public void ClosePopup()
    {
        if (activePopup != null)
        {
            Destroy(activePopup);
            activePopup = null;
            currentActivePopup = null;
            Debug.Log("DescriptionPopup閉じました。");
        }
    }

    private void Update()
    {
        if (activePopup != null)
        {
            bool shouldClosePopup = false;

            // マウス入力の検出
            if (Input.GetMouseButtonDown(0)) // 左クリック
            {
                shouldClosePopup = !IsPointerOverUIElement(Input.mousePosition);
            }

            // タッチ入力の検出
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    shouldClosePopup = !IsPointerOverUIElement(touch.position);
                }
            }

            if (shouldClosePopup)
            {
                ClosePopup();
            }
        }
    }

    private bool IsPointerOverUIElement(Vector2 position)
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = position
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject == activePopup || result.gameObject.transform.IsChildOf(activePopup.transform))
            {
                return true;
            }
        }

        return false;
    }

}
