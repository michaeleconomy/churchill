using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Card : MonoBehaviour, IPointerClickHandler, IEndDragHandler, IDragHandler {
    public static string spade = "♠",
        heart = "♥",
        club = "♣",
        diamond = "♦";
    
    public static string[] suits = new[] {spade, heart, club, diamond};

    public bool faceDown = true;
    public Color blackColor, redColor;
    public Sprite backSprite, frontSprite;

    public TextMesh[] suitTexts, numberTexts;
    public float speed, doubleClickSpeed;

    [NonSerialized]
    public int number;
    [NonSerialized]
    public string suit;

    public Stack stack;

    private Vector2 oldPostition;

    private SpriteRenderer spriteRenderer;
    private DateTime lastClickAt = DateTime.Now;

    private void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public string StyledNumber() {
        if (number == 1) {
            return "A";
        }
        if (number == 11) {
            return "J";
        }
        if (number == 12) {
            return "Q";
        }
        if (number == 13) {
            return "K";
        }
        return number.ToString();
    }

    public void Flip(bool faceDown) {
        this.faceDown = faceDown;
        Refresh();
    }

    public bool Black {
        get { return suit == spade || suit == club; }
    }

    private Color GetColor() {
        return Black ? blackColor : redColor;
    }

    public void Refresh() {
        if (faceDown) {
            spriteRenderer.sprite = backSprite;
            foreach (var suitText in suitTexts) {
                suitText.text = "";
            }
            foreach (var numberText in numberTexts) {
                numberText.text = "";
            }
            return;
        }
        spriteRenderer.sprite = frontSprite;
        foreach (var suitText in suitTexts) {
            suitText.text = suit;
            suitText.color = GetColor();
        }
        foreach (var numberText in numberTexts) {
            numberText.text = StyledNumber();
            numberText.color = GetColor();
        }
    }

    public void Return() {
        StartCoroutine(MoveTo(oldPostition));
    }

    public IEnumerator MoveTo(Vector2 pos) {
        var heading = pos - transform.position.In2d();
        var distance = heading.magnitude;
        var direction = heading / distance;
        while(true) {
            var moveDistance = speed * Time.deltaTime;
            distance -= moveDistance;
            if (distance <= 0) {
                break;
            }
            transform.position += direction.In3d() * moveDistance;

            yield return null;
        }
        transform.position = pos;
        oldPostition = pos;
    }

    void IDragHandler.OnDrag(PointerEventData eventData) {
        var worldPos = eventData.pressEventCamera.ScreenToWorldPoint(eventData.position);
        worldPos.z = 0;
        transform.position = worldPos;
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData) {
        var newStack = GetOverlappingStack();
        if (newStack == null || newStack == stack) {
            Return();
            return;
        }
        newStack.AddCard(this);
    }

    private Stack GetOverlappingStack() {
        var col = GetComponent<Collider2D>();
        var results = new List<Collider2D>();
        col.OverlapCollider(new ContactFilter2D(), results);
        var cards = results.Select(r => r.GetComponent<Card>()).OfType<Card>();
        if (!cards.Any()) {
            return null;
        }
        var closest = cards.First();
        var closestDistance = closest.transform.position.Distance(transform.position);
        foreach (var card in cards) {
            var distance = closest.transform.position.Distance(transform.position);
            if (distance < closestDistance) {
                closestDistance = distance;
                closest = card;
            }
        }
        return closest.stack;
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData) {
        var now = DateTime.Now;
        var diff = now - lastClickAt;
        if (diff.TotalSeconds <= doubleClickSpeed) {
            DoubleClick();
        }
        else {
            Click();
        }
        lastClickAt = now;
    }

    private void Click() {
        if (stack is Deck) {
            var deck = (Deck)stack;
            deck.Deal();
        }
    }

    private void DoubleClick() {
        Debug.Log("dbl click");
    }

    public override string ToString() {
        return number + suit;
    }
}