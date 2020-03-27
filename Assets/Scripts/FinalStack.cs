using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalStack : Stack {
    public override bool CanAdd(Card card) {
        if (cards.Count == 0) {
            return card.number == 1;
        }
        return TopCard().suit == card.suit && card.number == cards.Count + 1;
    }

    public bool Full() {
        return cards.Count == 13;
    }
}