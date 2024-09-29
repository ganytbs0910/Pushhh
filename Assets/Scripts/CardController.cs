using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class CardController : MonoBehaviour
{
    [SerializeField] private Transform cardParent;
    [SerializeField] private GameObject cardPrefab;
    private List<Sprite> backSprites = new List<Sprite>();
    [SerializeField] private List<String> title = new List<String>();
    [TextArea(3, 15)][SerializeField] private List<String> subTitle = new List<String>();

    void Start()
    {

    }
}