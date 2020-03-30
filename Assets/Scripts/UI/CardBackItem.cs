using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardBackItem : MonoBehaviour {
    public Image cardImage, highlight;
    public GameObject lockImage;
    public Text winsToUnlockText;


    private CardBack cardBack;

    public void Initialize(CardBack cardBack) {
        this.cardBack = cardBack;
        cardImage.sprite = cardBack.backSprite;

        if (cardBack.Locked) {
            winsToUnlockText.text = "" + cardBack.winsToUnlock;
            lockImage.SetActive(true);
        }
        else {
            lockImage.SetActive(false);
        }
        highlight.enabled =  cardBack.Selected;
    }

    public void Select() {
        if (cardBack.Locked) {
            DialogView.Prompt("You need " + cardBack.WinsNeeded + " more wins to unlock this cardback");
            return;
        }
        CardBacks.instance.Select(cardBack.backSprite);
        GetComponentInParent<CardBackChooser>()?.gameObject.SetActive(false);
    }

}