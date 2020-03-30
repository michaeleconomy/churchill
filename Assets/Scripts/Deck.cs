using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Deck : Stack, IPointerClickHandler {
    public TextMesh cardsLeftText;

    protected override void OnCardsChanged() {
        cardsLeftText.text = "" + Count();
    }

    protected override void RevealTop() {
        UpdateColliders();
    }


    protected override void UpdateColliders() {
        base.UpdateColliders();
        var top = TopCard();
        if (top != null) {
            col.offset = top.transform.position - transform.position;
        }
        else {
            col.offset = Vector3.zero;
        }
        col.enabled = true;
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData) {
        if (PlayManager.locked) {
            return;
        }
        PlayManager.locked = true;
        StartCoroutine(Deal());
    }


    private IEnumerator Deal() {
        foreach (var stack in PlayManager.instance.stacks) {
            if (stack.LockedIn()) {
                continue;
            }
            var card = TopCard();
            if (card == null) {
                PlayManager.locked = false;
                yield break;
            }
            card.Flip(false);
            yield return StartCoroutine(stack.AddCardSync(card, true));
        }
        GameStateManager.instance.RecordUndo();
        PlayManager.locked = false;
    }


    public override bool CanAdd(Card card) {
        return false;
    }
}
