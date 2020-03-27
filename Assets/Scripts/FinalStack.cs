using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalStack : Stack {
    public override bool CanAdd(Card card) {
        if (bottomCard == null) {
            return card.number == 1;
        }
        return TopCard().suit == card.suit && card.number == TopCard().number + 1;
    }

    public bool Full() {
        return TopCard().number == 13;
    }
}