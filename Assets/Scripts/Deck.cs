using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Deck : Stack {
    public void Deal() {
        foreach (var stack in PlayManager.instance.stacks) {
            if (stack.LockedIn()) {
                continue;
            }
            var card = TopCard();
            if (card == null) {
                return;
            }
            card.Flip(false);
            stack.AddCard(card, true);
        }
    }

    protected override void RevealTop() {}

    public override bool CanAdd(Card card) {
        return false;
    }

    public void Shuffle() {
        cards.Shuffle();
    }
}
