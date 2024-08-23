using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using EnhancedUI.EnhancedScroller;
using EnhancedUI;
using System.Linq;

namespace EnhancedScrollerDemos.SnappingDemo
{
    public class SnappingDemo : MonoBehaviour
    {
        private SlotController[] _slotControllers;
        private int[] _snappedDataIndices;
        private int _snapCount;

        public float minVelocity;
        public float maxVelocity;
        public int sevenIndex = 6;
        public Sprite[] slotSprites;
        public Button spinButton;
        public float spinInterval = 0.4f;
        public float spinDuration = 3f;
        public float winProbability = 0.1f; // 当選確率（0.1 = 10%）

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

        public void SpinButton_OnClick2()
        {
            StartCoroutine(SpinAll());
        }

        private IEnumerator SpinAll()
        {
            _snapCount = 0;
            spinButton.interactable = false;

            bool isWin = Random.value < winProbability;

            for (int i = 0; i < _slotControllers.Length; i++)
            {
                StartCoroutine(SpinSlot(_slotControllers[i], isWin));
                yield return new WaitForSeconds(spinInterval);
            }
        }

        private IEnumerator SpinSlot(SlotController slotController, bool isWin)
        {
            float elapsedTime = 0f;
            while (elapsedTime < spinDuration)
            {
                slotController.scroller.JumpToDataIndex(Random.Range(0, slotSprites.Length));
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            int finalIndex = isWin ? sevenIndex : Random.Range(0, slotSprites.Length);
            slotController.scroller.JumpToDataIndex(finalIndex);
            CheckResult();
        }

        private void ScrollerSnapped(EnhancedScroller scroller, int cellIndex, int dataIndex, EnhancedScrollerCellView cellView)
        {
            _snapCount++;
            _snappedDataIndices[_snapCount - 1] = dataIndex;
            if (_snapCount == _slotControllers.Length)
            {
                CheckResult();
                spinButton.interactable = true;
            }
        }

        private void CheckResult()
        {
            Debug.Log($"Slot Results: {string.Join(", ", _snappedDataIndices)}");

            if (_snappedDataIndices.All(index => index == sevenIndex))
            {
                Debug.Log("7が当選！");
            }
            else
            {
                Debug.Log("はずれ");
            }
        }
    }
}