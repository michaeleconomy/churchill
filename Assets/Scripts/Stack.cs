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
        StartCoroutine(AddCardSync(card, force, false, true));
    }
        
    public void AddCardFast(Card card, bool force = false, bool isUndo = false) {
        if (Contains(card)) {
            Debug.LogWarning(name + " already contains card: " + card);
            card.MoveFast();
            return;
        }
        if (!force && !CanAdd(card)) {
            card.MoveFast();
            return;
        }

        DoAddCard(card, isUndo);
        card.MoveFast();
        PostAdd(card);
    }
        
    public IEnumerator AddCardSync(Card card, bool force = false, bool isUndo = false, bool doLock = false) {
        if (Contains(card)) {
            Debug.LogWarning(name + " already contains card: " + card);
            yield return StartCoroutine(card.Move());
            yield break;
        }
        if (!force && !CanAdd(card)) {
            yield return StartCoroutine(card.Move());
            yield break;
        }
        if (doLock) {
            PlayManager.locked = true;
        }
        DoAddCard(card, isUndo);
        yield return StartCoroutine(card.Move());
        PostAdd(card);

        if (doLock) {
            GameStateManager.instance.RecordUndo();
            PlayManager.locked = false;
        }
    }

    private void DoAddCard(Card card, bool isUndo) {
        var oldTopCard = TopCard();
        if (card.parentCard != null) {
            card.parentCard.cardOnTop = null;
        }
        card.parentCard = oldTopCard;
        if (oldTopCard != null) {
            oldTopCard.cardOnTop = card;
            card.transform.parent = oldTopCard.transform;
        }
        else {
            bottomCard = card;
            card.transform.parent = transform;
        }
        col.enabled = false;
        var oldStack = card.stack;
        card.SetStacks(this);
        if (!isUndo) {
            GameStateManager.instance.RecordMove(card, oldStack, this);
        }
        if (oldStack != null) {
            if (oldStack.bottomCard == card) {
                oldStack.bottomCard = null;
            }
            oldStack.RevealTop();
            oldStack.OnCardsChanged();
        }
        OnCardsChanged();
    }

    private void PostAdd(Card card) {
        UpdateColliders();
        PlayManager.instance.IntegrityCheck();
        PlayManager.instance.WinCheck();
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

    protected virtual void OnCardsChanged() {}

    private void UpdateColliders() {
        for (var card = bottomCard; card != null; card = card.cardOnTop) {
            card.UpdateColliders();
        }
    }

    protected virtual void RevealTop() {
        if (bottomCard == null) {
            col.enabled = true;
        }
        else {
            TopCard().Flip(false);
        }
        UpdateColliders();
    }

    public bool LockedIn() {
        return bottomCard != null && bottomCard.number == 13 && !bottomCard.faceDown;
    }

    public virtual bool CanAdd(Card card) {
        if (card.stack is DevilsSix) {
            return false;
        }
        if (bottomCard == null) {
            return card.number == 13;
        }
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

    public override string ToString() {
        return name;
    }
}
