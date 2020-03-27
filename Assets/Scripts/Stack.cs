using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stack : MonoBehaviour {
    public Vector2 offset;

    [NonSerialized]
    public Card bottomCard;

    protected BoxCollider2D col;

    private void Awake() {
        col = GetComponent<BoxCollider2D>();
    }

    public void AddCard(Card card, bool force = false) {
        StartCoroutine(AddCardSync(card, force));
    }

    private bool Contains(Card card) {
        for (var other = bottomCard; other != null; other = other.cardOnTop) {
            if (other == card) {
                return true;
            }
        }
        return false;
    }

    public int Count() {
        var count = 0;
        for (var other = bottomCard; other != null; other = other.cardOnTop) {
            count++;
        }
        return count;
    } 
        
    public IEnumerator AddCardSync(Card card, bool force = false, bool fast = false) {
        if (Contains(card)) {
            yield break;
        }
        if (!force && !CanAdd(card)) {
            card.Return();
            yield break;
        }
        var pos = transform.position + offset.In3d() * Count();
        if (fast) {
            card.transform.position = pos;
        }
        else {
            yield return StartCoroutine(card.MoveTo(pos));
        }
        var oldTopCard = TopCard();
        if (card.parentCard != null) {
            card.parentCard.cardOnTop = null;
        }
        card.parentCard = oldTopCard;
        if (oldTopCard != null) {
            oldTopCard.fullCollider.enabled = false;
            oldTopCard.cardOnTop = card;
            card.transform.parent = oldTopCard.transform;
        }
        else {
            bottomCard = card;
            card.transform.parent = transform;
        }
        col.enabled = false;
        var oldStack = card.stack;
        card.stack = this;
        if (oldStack != null) {
            if (oldStack.bottomCard == card) {
                oldStack.bottomCard = null;
            }
            oldStack.RevealTop();
        }
        card.sortingGroup.sortingOrder = Count();
        UpdateTopColliders();
    }

    private void UpdateTopColliders() {
        for (var card = bottomCard; card != null; card = card.cardOnTop) {
            card.UpdateTopCollider();
        }
    }

    protected virtual void RevealTop() {
        if (bottomCard == null) {
            col.enabled = true;
        }
        else {
            TopCard().Flip(false);
        }
    }

    public bool LockedIn() {
        return bottomCard != null && bottomCard.number == 13 && bottomCard.faceDown;
    }

    public virtual bool CanAdd(Card card) {
        if (card.stack is DevilsSix) {
            return false;
        }
        if (bottomCard == null) {
            return card.number == 13;
        }
        Debug.Log("top card: " + TopCard());
        return TopCard().number == card.number + 1 &&
            card.Black != TopCard().Black;
    }

    public Card TopCard() {
        if (bottomCard == null) {
            return null;
        }
        var card = bottomCard;
        while (card.cardOnTop != null) {
            card = card.cardOnTop;
        }
        return card;
    }
}
