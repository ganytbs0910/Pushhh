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
        public float spinInterval = 0.4f;
        public float spinDuration = 3f;
        public float winProbability = 0.1f;

        public TMP_Text resultText;

        void Awake()
        {
            Application.targetFrameRate = 60;
            _slotControllers = gameObject.GetComponentsInChildren<SlotController>();
            _snappedDataIndices = new int[_slotControllers.Length];
            foreach (var slotController in _slotControllers)
            {
                slotController.scroller.scrollerSnapped = ScrollerSnapped;
                slotController.scroller.snapping = true;
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
            StartCoroutine(SpinAll());
        }

        private IEnumerator SpinAll()
        {
            _snapCount = 0;
            spinButton.interactable = false;
            resultText.text = "スピン中...";

            bool isWin = Random.value < winProbability;

            for (int i = 0; i < _slotControllers.Length; i++)
            {
                StartCoroutine(SpinSlot(_slotControllers[i], isWin, i + 1));
                yield return new WaitForSeconds(spinInterval);
            }
        }

        private IEnumerator SpinSlot(SlotController slotController, bool isWin, int slotNumber)
        {
            float elapsedTime = 0f;
            while (elapsedTime < spinDuration)
            {
                slotController.scroller.JumpToDataIndex(Random.Range(0, slotSprites.Length));
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            int finalIndex = isWin ? 6 : Random.Range(0, slotSprites.Length);
            slotController.scroller.JumpToDataIndex(finalIndex);

            int displayNumber = finalIndex + 1;
            Debug.Log($"スロット {slotNumber} の結果: {displayNumber}");
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