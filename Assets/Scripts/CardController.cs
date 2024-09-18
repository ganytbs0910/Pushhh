using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardController : MonoBehaviour
{

    [SerializeField] private Transform cardParent;
    [SerializeField] private GameObject cardPrefab;
    private List<Sprite> backSprites = new List<Sprite>();
    [SerializeField] private List<String> title = new List<String>();
    [TextArea(3, 15)][SerializeField] private List<String> subTitle = new List<String>();

    void Start()
    {
        int num = Resources.LoadAll<Sprite>("Sprites").Length;
        for (int i = 0; i < num; i++)
        {
            backSprites.Add(Resources.Load<Sprite>($"Sprites/{i}"));
        }
    }

    public void CreateCardPrefab(int cardNum)
    {
        //cardParentの子オブジェクトを全削除
        foreach (Transform n in cardParent)
        {
            Destroy(n.gameObject);
        }
        Card card = Instantiate(cardPrefab, transform.position, Quaternion.identity, cardParent).GetComponent<Card>();
        card.SetCardDetail(backSprites[cardNum], title[cardNum], subTitle[cardNum]);
    }
}