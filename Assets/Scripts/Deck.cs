using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Deck : Stack, IPointerClickHandler {
    public IEnumerator Deal() {
        foreach (var stack in PlayManager.instance.stacks) {
            if (stack.LockedIn()) {
                continue;
            }
            var card = TopCard();
            if (card == null) {
                yield break;
            }
            card.Flip(false);
            yield return StartCoroutine(stack.AddCardSync(card, true));
        }
        AlignCollider();
    }

    protected override void RevealTop() {
        AlignCollider();
    }

    public void AlignCollider() {
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
        StartCoroutine(Deal());
    }

    public override bool CanAdd(Card card) {
        return false;
    }
}
