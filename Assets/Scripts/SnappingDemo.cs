using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using EnhancedUI.EnhancedScroller;
using EnhancedUI;
using System.Linq;
using TMPro;

namespace EnhancedScrollerDemos.SnappingDemo
{
    public class SnappingDemo : MonoBehaviour
    {
        [SerializeField] private SlotController[] _slotControllers;
        [SerializeField] private Coroutine[] _spinCoroutines;
        [SerializeField] private int[] _predeterminedResult;
        [SerializeField] private int[] _snappedDataIndices;
        [SerializeField] private int _snapCount;
        public int remainPullNumber;
        [SerializeField] private Sprite[] slotSprites;
        [SerializeField] private Button[] stopButtons;
        [SerializeField] private Button spinButton;
        [SerializeField] private float minVelocity;
        [SerializeField] private float maxVelocity;
        [SerializeField] private float spinInterval = 0.4f;
        [SerializeField] private float spinDuration = 3f;
        [SerializeField] private float baseWinProbability = 0.1f;
        [SerializeField] private TMP_Text resultText;
        [SerializeField] private bool[] _isSlotStopped;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text expText;

        private UIController uiController;
        private LevelSystem levelSystem;

        void Awake()
        {
            uiController = GetComponent<UIController>();
            remainPullNumber = PlayerPrefs.GetInt("remainPullNumber");
            Application.targetFrameRate = 60;
            _slotControllers = gameObject.GetComponentsInChildren<SlotController>();
            _snappedDataIndices = new int[_slotControllers.Length];
            _isSlotStopped = new bool[_slotControllers.Length];
            _spinCoroutines = new Coroutine[_slotControllers.Length];
            for (int i = 0; i < _slotControllers.Length; i++)
            {
                _slotControllers[i].scroller.scrollerSnapped = ScrollerSnapped;
                _slotControllers[i].scroller.snapping = true;
                int index = i;
                stopButtons[i].onClick.AddListener(() => StopButton_OnClick(index));
                stopButtons[i].interactable = false;
            }
            spinButton.onClick.AddListener(SpinButton_OnClick);

            levelSystem = new LevelSystem();
            LoadLevelData();
            UpdateLevelUI();
        }

        private static bool AutoSpin()
        {
            return Settings.instance != null && Settings.instance.IsAutoSpinOn;
        }

        void Start()
        {
            foreach (var slotController in _slotControllers)
            {
                slotController.Reload(slotSprites);
            }
        }

        public void SpinButton_OnClick()
        {
            VibrationController.VibrateSelectionChanged();
            if (remainPullNumber <= 0)
            {
                Debug.Log("残りプル数がありません");
                uiController.LackCreditPanelOpen();
                return;
            }
            else
            {
                remainPullNumber--;
                PlayerPrefs.SetInt("remainPullNumber", remainPullNumber);
                uiController.RemainCreditTextUpdate();
            }
            ResetSlots();
            //レバーによるスロットの運命を決定
            DetermineResult();
            StartCoroutine(SpinAll());
            levelSystem.AddExp(10);
            UpdateLevelUI();
            SaveLevelData();
        }

        private void ResetSlots()
        {
            _snapCount = 0;
            for (int i = 0; i < _slotControllers.Length; i++)
            {
                _isSlotStopped[i] = false;
                stopButtons[i].interactable = false;
                if (_spinCoroutines[i] != null)
                {
                    StopCoroutine(_spinCoroutines[i]);
                    _spinCoroutines[i] = null;
                }
            }
        }

        private void StopButton_OnClick(int slotIndex)
        {
            if (!AutoSpin() && !_isSlotStopped[slotIndex])
            {
                StopSlot(slotIndex);
            }
        }

        private void StopSlot(int slotIndex)
        {
            if (_spinCoroutines[slotIndex] != null)
            {
                StopCoroutine(_spinCoroutines[slotIndex]);
                _spinCoroutines[slotIndex] = null;
            }
            _slotControllers[slotIndex].scroller.JumpToDataIndex(_predeterminedResult[slotIndex]);
            _isSlotStopped[slotIndex] = true;
            stopButtons[slotIndex].interactable = false;
            int displayNumber = _predeterminedResult[slotIndex] + 1;
            if (_isSlotStopped.All(stopped => stopped))
            {
                CheckResult();
                EnableSpinButton();
            }
        }

        private void DetermineResult()
        {
            float levelBonus = levelSystem.GetWinProbabilityBonus();
            bool isWin = Random.value < (baseWinProbability + levelBonus);
            _predeterminedResult = new int[_slotControllers.Length];
            if (isWin)
            {
                int winningNumber = Random.Range(0, slotSprites.Length);
                for (int i = 0; i < _slotControllers.Length; i++)
                {
                    _predeterminedResult[i] = winningNumber;
                }
                //40%の確率で特殊効果を発動
                if (Random.value < 0.4f) SpecialGachaEffect();
            }
            else
            {
                for (int i = 0; i < _slotControllers.Length; i++)
                {
                    _predeterminedResult[i] = Random.Range(0, slotSprites.Length);
                }
                if (_predeterminedResult.All(x => x == _predeterminedResult[0]))
                {
                    _predeterminedResult[_predeterminedResult.Length - 1] = (_predeterminedResult[0] + 1) % slotSprites.Length;
                }
                //0.1%の確率で特殊効果を発動
                if (Random.value < 0.001f) SpecialGachaEffect();
            }
            Debug.Log($"決定された数字: {string.Join(", ", _predeterminedResult.Select(x => x + 1))}");
        }

        private void SpecialGachaEffect()
        {
            Debug.Log("特殊効果発動！");
            int specialEffectIndex = Random.Range(0, 8);
            switch (specialEffectIndex)
            {
                case 1:
                    //振動させる
                    VibrationController.VibrateHeavy();
                    break;
                case 2:
                    //フラッシュさせる
                    break;
                case 3:
                    //スロットを逆に回す
                    break;
                case 4:
                    //スロットを遅く回す
                    break;
                case 5:
                    //スロットを速く回す
                    break;
                case 6:
                    //ボタンを赤にする
                    for (int i = 0; i < stopButtons.Length; i++)
                    {
                        stopButtons[i].GetComponent<Image>().color = Color.red;
                    }
                    break;
                case 7:

                default:
                    break;
            }
        }

        private IEnumerator SpinAll()
        {
            spinButton.interactable = false;
            resultText.text = "スピン中...";
            for (int i = 0; i < _slotControllers.Length; i++)
            {
                _spinCoroutines[i] = StartCoroutine(SpinSlot(_slotControllers[i], _predeterminedResult[i], i));
                if (!AutoSpin()) stopButtons[i].interactable = true;
            }
            if (AutoSpin())
            {
                yield return new WaitForSeconds(spinDuration);
                for (int i = 0; i < _slotControllers.Length; i++)
                {
                    if (!_isSlotStopped[i]) StopSlot(i);
                }
            }
        }

        private IEnumerator SpinSlot(SlotController slotController, int finalIndex, int slotNumber)
        {
            while (!_isSlotStopped[slotNumber])
            {
                slotController.scroller.JumpToDataIndex(Random.Range(0, slotSprites.Length));
                yield return null;
            }
        }

        private void ScrollerSnapped(EnhancedScroller scroller, int cellIndex, int dataIndex, EnhancedScrollerCellView cellView)
        {
            _snapCount++;
            if (_snapCount == _slotControllers.Length && !AutoSpin())
            {
                EnableSpinButton();
            }
        }

        private void EnableSpinButton()
        {
            spinButton.interactable = true;
            resultText.text = "スピン終了";
            foreach (var button in stopButtons)
            {
                button.interactable = false;
            }
        }

        private void CheckResult()
        {
            int[] displayNumbers = _predeterminedResult.Select(i => i + 1).ToArray();
            Debug.Log($"Final Slot Results: {string.Join(", ", displayNumbers)}");

            var s1 = displayNumbers[0];
            var s2 = displayNumbers[1];
            var s3 = displayNumbers[2];

            string result;

            if (s1 == s2 && s2 == s3)
            {
                switch (s1)
                {
                    case 1:
                        // 経験値を獲得する
                        result = "1が3つ揃いました！小当たり！";
                        break;
                    case 3:
                        // 課金クーポンを獲得
                        result = "3が3つ揃いました！中当たり！";
                        break;
                    case 5:
                        //　次回の当選確率を上昇
                        result = "5が3つ揃いました！大当たり！";
                        break;
                    case 7:
                        result = "777が揃いました！超大当たり！";
                        break;
                    default:
                        //クレジットを3追加する
                        uiController.AddPullCredit(3).Forget();
                        result = $"{s1}が3つ揃いました！当たり！";
                        break;
                }
            }
            else
            {
                result = $"結果: {s1}, {s2}, {s3}。はずれ！";
            }
            resultText.text = result;
        }

        private void LoadLevelData()
        {
            levelSystem.CurrentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
            levelSystem.CurrentExp = PlayerPrefs.GetInt("CurrentExp", 0);
            levelSystem.ExpToNextLevel = PlayerPrefs.GetInt("ExpToNextLevel", 100);
        }

        private void SaveLevelData()
        {
            PlayerPrefs.SetInt("CurrentLevel", levelSystem.CurrentLevel);
            PlayerPrefs.SetInt("CurrentExp", levelSystem.CurrentExp);
            PlayerPrefs.SetInt("ExpToNextLevel", levelSystem.ExpToNextLevel);
            PlayerPrefs.Save();
        }

        private void UpdateLevelUI()
        {
            levelText.text = $"Level: {levelSystem.CurrentLevel}";
            expText.text = $"EXP: {levelSystem.CurrentExp} / {levelSystem.ExpToNextLevel}";
        }
    }

    public class LevelSystem
    {
        public int CurrentLevel { get; set; }
        public int CurrentExp { get; set; }
        public int ExpToNextLevel { get; set; }

        private const int BaseExpToNextLevel = 100;
        private const float ExpGrowthRate = 1.5f;

        public LevelSystem()
        {
            CurrentLevel = 1;
            CurrentExp = 0;
            ExpToNextLevel = BaseExpToNextLevel;
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
            ExpToNextLevel = (int)(ExpToNextLevel * ExpGrowthRate);
        }

        public float GetWinProbabilityBonus()
        {
            return (CurrentLevel - 1) * 0.01f; // 1レベルごとに1%ずつ当選確率上昇
        }
    }
}