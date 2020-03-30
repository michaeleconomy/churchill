using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardBackChooser : MonoBehaviour {
    public Text totalWinsText;
    public CardBackItem itemPrefab;
    public Transform backsTransform;

    public void Show() {
        totalWinsText.text = "" + Stats.instance.Wins;
        backsTransform.DeleteChildren();
        foreach (var cardBack in CardBacks.instance.cardBacks) {
            var item = Instantiate(itemPrefab, backsTransform);
            item.Initialize(cardBack);
        }
        gameObject.SetActive(true);
    }
}