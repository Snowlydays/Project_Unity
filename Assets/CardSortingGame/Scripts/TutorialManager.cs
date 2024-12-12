using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace CardSortingGame.Scripts
{
    public class TutorialManager : MonoBehaviour
    {
        private bool isFirstTutorialClosed = false;
        private int currentShowTutorialNumber = 0;
        private const int TutorialPageSize = 4;

        [SerializeField] private Transform tutorialPage;
        [SerializeField] private Transform[] tutorialPageObjects = new Transform[TutorialPageSize];

        [SerializeField] private Button prevButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button showTutorialButton;
        [SerializeField] private Button closeTutorialButton;

        [SerializeField] private Sprite prevDisabledSprite;
        [SerializeField] private Sprite prevEnabledSprite;
        [SerializeField] private Sprite nextDisabledSprite;
        [SerializeField] private Sprite nextEnabledSprite;
        [SerializeField] private Sprite[] tutorialSprites = new Sprite[TutorialPageSize];
        
        private void Awake()
        {
            isFirstTutorialClosed = false;
            prevButton.onClick.AddListener(OnPrevButtonClicked);
            nextButton.onClick.AddListener(OnNextButtonClicked);
            showTutorialButton.onClick.AddListener(OnShowTutorialButtonClicked);
            closeTutorialButton.onClick.AddListener(OnCloseTutorialButtonClicked);

            ResetTutorialPage();
        }
        
        private void OnNextButtonClicked()
        {
            if (currentShowTutorialNumber < TutorialPageSize-1)
            {
                currentShowTutorialNumber++;
                UpdateTutorialUI();
                SoundManager.PlaySEnum(3);
            }
        }

        private void OnPrevButtonClicked()
        {
            if (currentShowTutorialNumber > 0)
            {
                currentShowTutorialNumber--;
                UpdateTutorialUI();
                SoundManager.PlaySEnum(3);
            }
        }

        private void OnShowTutorialButtonClicked()
        {
            tutorialPage.gameObject.SetActive(true);
            SoundManager.PlaySEnum(2);
        }
        private void OnCloseTutorialButtonClicked()
        {
            if (!isFirstTutorialClosed)
            {
                FindObjectOfType<TimerController>().StartDrawBar();
                isFirstTutorialClosed = true;
            }
            ResetTutorialPage();
            tutorialPage.gameObject.SetActive(false);
            SoundManager.PlaySEnum(0);
        }
        private void UpdateTutorialUI()
        {
            Image tutorialPageImage = tutorialPage.GetComponent<Image>();
            Image prevImage = prevButton.GetComponent<Image>();
            Image nextImage = nextButton.GetComponent<Image>();
            tutorialPageImage.sprite = tutorialSprites[currentShowTutorialNumber];
            prevImage.sprite = (currentShowTutorialNumber <= 0 ? prevDisabledSprite : prevEnabledSprite);
            nextImage.sprite = (currentShowTutorialNumber >= TutorialPageSize ? nextDisabledSprite : nextEnabledSprite);

            foreach (Transform obj in tutorialPageObjects) obj.gameObject.SetActive(false);
            tutorialPageObjects[currentShowTutorialNumber].gameObject.SetActive(true);
        }
        
        private void ResetTutorialPage()
        {
            currentShowTutorialNumber = 0;
            UpdateTutorialUI();
        }
    }
}