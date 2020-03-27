using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stack : MonoBehaviour {
    public Vector2 offset;

    [NonSerialized]
    public readonly List<Card> cards = new List<Card>();

    public void AddCard(Card card, bool force = false) {
        StartCoroutine(AddCardSync(card, force));
    }
        
    public IEnumerator AddCardSync(Card card, bool force = false) {
        if (cards.Contains(card)) {
            yield break;
        }
        if (!force && !CanAdd(card)) {
            card.Return();
            yield break;
        }
        Debug.Log("Adding " + card + " to " + name);
        var pos = transform.position + offset.In3d() * cards.Count;
        yield return StartCoroutine(card.MoveTo(pos));
        cards.Add(card);
        if (card.stack != null) {
            card.stack.cards.Remove(card);
            card.stack.RevealTop();
        }
        card.stack = this;
    }

    protected virtual void RevealTop() {
        if (cards.Count == 0) {
            return;
        }
        TopCard().Flip(false);
    }

    public bool LockedIn() {
        return BottomCard().number == 13 && !BottomCard().faceDown;
    }

    public virtual bool CanAdd(Card card) {
        if (cards.Count == 0) {
            return card.number == 13;
        }
        return TopCard().number == card.number + 1 &&
            card.Black != TopCard().Black;
    }

    public Card TopCard() {
        return cards.Last();
    }
    protected Card BottomCard() {
        return cards.Last();
    }
}
