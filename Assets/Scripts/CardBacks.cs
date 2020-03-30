using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CardBacks : MonoBehaviour {
    public static CardBacks instance;

    public List<CardBack> cardBacks;

    [NonSerialized]
    public Sprite selectedCardback;

    private void Awake() {
        instance = this;
        var cardBackName = PlayerPrefs.GetString("cardBack", null);
        var cardBack = cardBacks.First(c => c.backSprite.name == cardBackName);
        if (cardBack != null) {
            selectedCardback = cardBack.backSprite;
        }
        else {
            selectedCardback = cardBacks.First().backSprite;
        }
    }


    public void Select(Sprite cardBack) {
        selectedCardback = cardBack;
        PlayerPrefs.GetString("cardBack", cardBack.name);
        foreach (var card in PlayManager.instance.allCards) {
            card.backSprite = selectedCardback;
            card.Refresh();
        }
    }

}