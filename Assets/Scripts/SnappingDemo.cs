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
        private SlotController[] _slotControllers;
        private int[] _snappedDataIndices;
        private int _snapCount;
        public float minVelocity;
        public float maxVelocity;
        public Sprite[] slotSprites;
        public Button spinButton;
        public Button[] stopButtons;
        public float spinInterval = 0.4f;
        public float spinDuration = 3f;
        public float winProbability = 0.1f;
        public TMP_Text resultText;
        public bool isAutomaticMode = false;

        private int[] _predeterminedResult;
        private bool[] _isSlotStopped;
        private Coroutine[] _spinCoroutines;

        void Awake()
        {
            Application.targetFrameRate = 60;
            _slotControllers = gameObject.GetComponentsInChildren<SlotController>();
            _snappedDataIndices = new int[_slotControllers.Length];
            _isSlotStopped = new bool[_slotControllers.Length];
            _spinCoroutines = new Coroutine[_slotControllers.Length];

            for (int i = 0; i < _slotControllers.Length; i++)
            {
                _slotControllers[i].scroller.scrollerSnapped = ScrollerSnapped;
                _slotControllers[i].scroller.snapping = true;
                int index = i; // キャプチャする変数
                stopButtons[i].onClick.AddListener(() => StopButton_OnClick(index));
            }

            spinButton.onClick.AddListener(SpinButton_OnClick);
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
            DetermineResult();
            StartCoroutine(SpinAll());
        }

        private void StopButton_OnClick(int slotIndex)
        {
            if (!isAutomaticMode && !_isSlotStopped[slotIndex])
            {
                StopSlot(slotIndex);
            }
        }

        private void StopSlot(int slotIndex)
        {
            if (_spinCoroutines[slotIndex] != null)
            {
                StopCoroutine(_spinCoroutines[slotIndex]);
            }
            _slotControllers[slotIndex].scroller.JumpToDataIndex(_predeterminedResult[slotIndex]);
            _isSlotStopped[slotIndex] = true;
            stopButtons[slotIndex].interactable = false;

            int displayNumber = _predeterminedResult[slotIndex] + 1;
            Debug.Log($"スロット {slotIndex + 1} の結果: {displayNumber}");

            if (_isSlotStopped.All(stopped => stopped))
            {
                CheckResult();
            }
        }

        private void DetermineResult()
        {
            bool isWin = Random.value < winProbability;
            _predeterminedResult = new int[_slotControllers.Length];

            if (isWin)
            {
                int winningNumber = Random.Range(0, slotSprites.Length);
                for (int i = 0; i < _slotControllers.Length; i++)
                {
                    _predeterminedResult[i] = winningNumber;
                }
            }
            else
            {
                for (int i = 0; i < _slotControllers.Length; i++)
                {
                    _predeterminedResult[i] = Random.Range(0, slotSprites.Length);
                }
                // 確実に揃わないようにする
                if (_predeterminedResult.All(x => x == _predeterminedResult[0]))
                {
                    _predeterminedResult[_predeterminedResult.Length - 1] = (_predeterminedResult[0] + 1) % slotSprites.Length;
                }
            }

            Debug.Log($"Predetermined Result: {string.Join(", ", _predeterminedResult.Select(x => x + 1))}");
        }

        private IEnumerator SpinAll()
        {
            _snapCount = 0;
            spinButton.interactable = false;
            resultText.text = "スピン中...";

            for (int i = 0; i < _slotControllers.Length; i++)
            {
                _isSlotStopped[i] = false;
                stopButtons[i].interactable = true;
                _spinCoroutines[i] = StartCoroutine(SpinSlot(_slotControllers[i], _predeterminedResult[i], i));
                if (isAutomaticMode)
                {
                    yield return new WaitForSeconds(spinInterval);
                }
            }

            if (isAutomaticMode)
            {
                yield return new WaitForSeconds(spinDuration);
                for (int i = 0; i < _slotControllers.Length; i++)
                {
                    if (!_isSlotStopped[i])
                    {
                        StopSlot(i);
                    }
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
            _snappedDataIndices[_snapCount - 1] = dataIndex;
            if (_snapCount == _slotControllers.Length)
            {
                CheckResult();
                spinButton.interactable = true;
                resultText.text = "スピン終了";
            }
        }

        private void CheckResult()
        {
            int[] displayNumbers = _snappedDataIndices.Select(i => i + 1).ToArray();
            Debug.Log($"Slot Results: {string.Join(", ", displayNumbers)}");

            var s1 = displayNumbers[0];
            var s2 = displayNumbers[1];
            var s3 = displayNumbers[2];

            string result;

            if (s1 == s2 && s2 == s3)
            {
                if (s1 == 7)
                {
                    result = "777が揃いました！大当たり！";
                }
                else
                {
                    result = $"{s1}が3つ揃いました！当たり！";
                }
            }
            else
            {
                result = $"結果: {s1}, {s2}, {s3}。はずれ！";
            }

            Debug.Log(result);
            resultText.text = result;
        }
    }
}