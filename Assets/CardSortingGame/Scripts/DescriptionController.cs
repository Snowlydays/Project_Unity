using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace CardSortingGame.Scripts
{
    public class DescriptionController : MonoBehaviour
    {
        private const int DescriptionPageSize = 5;
        private int curPage = 0;

        [SerializeField] private Transform descriptionPage;
        [SerializeField] private Transform[] descriptionPageObjects = new Transform[DescriptionPageSize];

        [SerializeField] private Button prevButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button showDescriptionButton;
        [SerializeField] private Button closeDescriptionButton;

        [SerializeField] private Sprite prevDisabledSprite;
        [SerializeField] private Sprite prevEnabledSprite;
        [SerializeField] private Sprite nextDisabledSprite;
        [SerializeField] private Sprite nextEnabledSprite;

        private void Awake()
        {
            ResetDescriptionPage();
            descriptionPage.gameObject.SetActive(false);
            prevButton.onClick.AddListener(OnPrevButtonClicked);
            nextButton.onClick.AddListener(OnNextButtonClicked);
            showDescriptionButton.onClick.AddListener(OnShowDescriptionButtonClicked);
            closeDescriptionButton.onClick.AddListener(OnCloseDescriptionButtonClicked);
        }

        private void OnNextButtonClicked()
        {
            if (curPage < DescriptionPageSize - 1)
            {
                curPage++;
                UpdateDescriptionUI();
                SoundManager.PlaySEnum(3);
            }
        }

        private void OnPrevButtonClicked()
        {
            if (curPage > 0)
            {
                curPage--;
                UpdateDescriptionUI();
                SoundManager.PlaySEnum(3);
            }
        }

        private void OnShowDescriptionButtonClicked()
        {
            descriptionPage.gameObject.SetActive(true);
            SoundManager.PlaySEnum(2);
        }

        private void OnCloseDescriptionButtonClicked()
        {
            ResetDescriptionPage();
            descriptionPage.gameObject.SetActive(false);
            SoundManager.PlaySEnum(0);
        }

        private void UpdateDescriptionUI()
        {
            Image prevImage = prevButton.GetComponent<Image>();
            Image nextImage = nextButton.GetComponent<Image>();
            prevImage.sprite = (curPage <= 0 ? prevDisabledSprite : prevEnabledSprite);
            nextImage.sprite = (curPage >= DescriptionPageSize-1 ? nextDisabledSprite : nextEnabledSprite);

            foreach (Transform obj in descriptionPageObjects) obj.gameObject.SetActive(false);
            descriptionPageObjects[curPage].gameObject.SetActive(true);
        }

        private void ResetDescriptionPage()
        {
            curPage = 0;
            UpdateDescriptionUI();
        }
    }
}