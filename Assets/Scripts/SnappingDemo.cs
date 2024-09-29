using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using TMPro;
using Random = UnityEngine.Random;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace EnhancedScrollerDemos.SnappingDemo
{
    public class SnappingDemo : MonoBehaviour
    {
        [SerializeField] private Button spinButton;
        [SerializeField] private TMP_Text resultText;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text expText;
        [SerializeField] private TMP_Text remainPullText;

        private LevelSystem levelSystem;
        public int remainPullNumber;
        private const float WinningProbability = 0.5f;
        private const float BASE_WIN_PROBABILITY = 0.1f;
        private const int MAX_RESULT = 7;
        [SerializeField] private FirebaseInitializer firebaseInitializer;

        [SerializeField] private Transform cardParent;
        [SerializeField] private GameObject cardPrefab;
        private List<Sprite> backSprites = new List<Sprite>();

        private bool isSpinning = false;
        private SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        [SerializeField] private UIController uiController;

        [Serializable]
        private class CardInfo
        {
            public string title;
            public string subTitle;
        }

        [Serializable]
        private class CardDataList
        {
            public List<CardInfo> cards;
        }

        private CardDataList cardData;

        private void Awake()
        {
            remainPullNumber = PlayerPrefs.GetInt("remainPullNumber", 10);
            spinButton.onClick.AddListener(SpinButton_OnClick);
            levelSystem = new LevelSystem();
            LoadLevelData();
            UpdateLevelUI();
            LoadCardData();
        }

        private void Start()
        {
            UpdateRemainPullText();
            LoadBackSprites();
            UpdateSpinButtonInteractable();
        }

        private void OnDestroy()
        {
            semaphore.Dispose();
        }

        private void LoadCardData()
        {
            TextAsset jsonFile = Resources.Load<TextAsset>("CardData");
            if (jsonFile != null) cardData = JsonUtility.FromJson<CardDataList>(jsonFile.text);
        }

        private void LoadBackSprites()
        {
            Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites");
            backSprites.AddRange(sprites);
        }

        public void SpinButton_OnClick()
        {
            if (isSpinning || remainPullNumber <= 0)
            {
                if (remainPullNumber <= 0) uiController.LackCreditPanelOpen();
                return;
            }
            SpinCoroutine().Forget();
        }

        private async UniTaskVoid SpinCoroutine()
        {
            if (!await semaphore.WaitAsync(TimeSpan.FromSeconds(0.1)))
            {
                Debug.Log("Spin already in progress");
                return;
            }

            try
            {
                isSpinning = true;
                UpdateSpinButtonInteractable();

                remainPullNumber--;
                PlayerPrefs.SetInt("remainPullNumber", remainPullNumber);
                UpdateRemainPullText();

                await firebaseInitializer.IncrementGlobalSlotCount();
                levelSystem.AddExp(10);
                UpdateLevelUI();
                SaveLevelData();

                // Update YourSpinCount
                int spinCount = PlayerPrefs.GetInt("LocalCounter") + 1;
                PlayerPrefs.SetInt("LocalCounter", spinCount);
                PlayerPrefs.Save();
                uiController.YourSpinCountTextUpdate();

                // Simulate spinning animation
                await UniTask.Delay(TimeSpan.FromSeconds(1));

                await CreateCardPrefab();

                // Update UI
                uiController.RemainCreditTextUpdate();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error during spin: {ex.Message}");
            }
            finally
            {
                isSpinning = false;
                UpdateSpinButtonInteractable();
                semaphore.Release();
            }
        }

        private void UpdateRemainPullText()
        {
            remainPullText.text = $"残り占い回数: {remainPullNumber}";
            uiController.RemainCreditTextUpdate();
        }

        private void LoadLevelData()
        {
            levelSystem.CurrentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
            levelSystem.CurrentExp = PlayerPrefs.GetInt("CurrentExp", 0);
        }

        private void SaveLevelData()
        {
            PlayerPrefs.SetInt("CurrentLevel", levelSystem.CurrentLevel);
            PlayerPrefs.SetInt("CurrentExp", levelSystem.CurrentExp);
            PlayerPrefs.Save();
        }

        private void UpdateLevelUI()
        {
            levelText.text = $"霊力レベル: {levelSystem.CurrentLevel}";
            expText.text = $"カルマポイント: {levelSystem.CurrentExp} / {levelSystem.ExpToNextLevel}";
        }

        public async UniTask CreateCardPrefab()
        {
            foreach (Transform child in cardParent)
            {
                Destroy(child.gameObject);
            }

            if (cardData == null || cardData.cards == null || cardData.cards.Count == 0)
            {
                Debug.LogError("Card data is not loaded or empty");
                return;
            }

            int cardSpriteIndex = Random.Range(0, backSprites.Count);
            int cardDetailIndex = await GetCardDetail();

            CardInfo cardInfo = cardData.cards[cardDetailIndex];
            GameObject cardObject = Instantiate(cardPrefab, transform.position, Quaternion.identity, cardParent);
            Card card = cardObject.GetComponent<Card>();
            card.SetCardDetail(backSprites[cardSpriteIndex], cardInfo.title, cardInfo.subTitle);
        }

        private async UniTask<int> GetCardDetail()
        {
            if (Random.value < WinningProbability)
            {
                await firebaseInitializer.ResetCounter();
                int winningAmount = await firebaseInitializer.CalculateWinningAmount();
                await firebaseInitializer.RecordWinning(winningAmount);
                uiController.TotalWinningCountTextUpdate();
                uiController.NewsTextUpdate(winningAmount);
                return 0;
            }
            return Random.Range(0, cardData.cards.Count);
        }

        private void UpdateSpinButtonInteractable()
        {
            spinButton.interactable = !isSpinning && remainPullNumber > 0;
        }

        public void AddPullNumber(int amount)
        {
            remainPullNumber += amount;
            PlayerPrefs.SetInt("remainPullNumber", remainPullNumber);
            UpdateRemainPullText();
            UpdateSpinButtonInteractable();
        }
    }
    public class LevelSystem
    {
        public int CurrentLevel { get; set; }
        public int CurrentExp { get; set; }
        public int ExpToNextLevel { get; private set; }

        private const int BaseExpToNextLevel = 200;
        private const float ExpGrowthRate = 1.8f;
        private const float ExpCurveStrength = 1.2f;

        public LevelSystem()
        {
            CurrentLevel = 1;
            CurrentExp = 0;
            CalculateExpToNextLevel();
        }

        public void AddExp(int exp)
        {
            CurrentExp += exp;
            while (CurrentExp >= ExpToNextLevel)
            {
                LevelUp();
            }
        }

        private void LevelUp()
        {
            CurrentLevel++;
            CurrentExp -= ExpToNextLevel;
            CalculateExpToNextLevel();
        }

        private void CalculateExpToNextLevel()
        {
            ExpToNextLevel = (int)(BaseExpToNextLevel * Mathf.Pow(ExpGrowthRate, CurrentLevel - 1) * Mathf.Pow(CurrentLevel, ExpCurveStrength));
        }
    }
}