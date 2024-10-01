using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class LoadingAnimPlayer : MonoBehaviour
{
    private const float DURATION = 0.5f;

    void Start()
    {
        Image[] circles = GetComponentsInChildren<Image>();
        for (var i = 0; i < circles.Length; i++)
        {
            var angle = -2 * Mathf.PI * i / circles.Length;
            circles[i].rectTransform.anchoredPosition = Vector2.zero;
            Sequence sequence = DOTween.Sequence()
                .SetLoops(-1, LoopType.Yoyo)
                .AppendInterval(DURATION / 4)
                .Append(circles[i].rectTransform.DOAnchorPos(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 50f, DURATION / 2))
                .AppendInterval(DURATION / 4);
            sequence.Play();
        }

        Sequence sequenceParent = DOTween.Sequence()
                .SetLoops(-1, LoopType.Incremental)
                .Append(transform.DOLocalRotate(Vector3.forward * (180f / circles.Length), DURATION / 4))
                .AppendInterval(DURATION / 2)
                .Append(transform.DOLocalRotate(Vector3.forward * (180f / circles.Length), DURATION / 4));
        sequenceParent.Play();
    }
}