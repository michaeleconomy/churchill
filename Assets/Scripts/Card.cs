using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.EventSystems;

public class Card : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IEndDragHandler, IDragHandler {
    public static string spade = "♠",
        heart = "♥",
        club = "♣",
        diamond = "♦";
    
    public static string[] suits = new[] {spade, heart, club, diamond};

    public bool faceDown = true;
    public Color blackColor, redColor;
    public Sprite backSprite, frontSprite;
    public Collider2D fullCollider, topCollider;

    public TextMesh[] suitTexts, numberTexts;
    public float speed, doubleClickSpeed;

    [NonSerialized]
    public int number, deck;
    [NonSerialized]
    public string suit;

    [NonSerialized]
    public Card cardOnTop, parentCard;

    [NonSerialized]
    public Stack stack;

    [NonSerialized]
    public SortingGroup sortingGroup;

    private bool dragging;

    private SpriteRenderer spriteRenderer;
    private DateTime lastClickAt = DateTime.Now;

    private void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        sortingGroup = GetComponent<SortingGroup>();
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

    public void Flip(bool faceDown, bool isUndo = false) {
        if (this.faceDown == faceDown) {
            return;
        }
        this.faceDown = faceDown;
        if (!isUndo) {
            GameStateManager.instance.RecordFlip(this);
        }
        Refresh();
    }

    public bool Black {
        get { return suit == spade || suit == club; }
    }

    private Color GetColor() {
        return Black ? blackColor : redColor;
    }

    public bool Sequential() {
        if (faceDown) {
            return false;
        }
        if (cardOnTop == null) {
            return true;
        }
        return cardOnTop.number + 1 == number && cardOnTop.Black != Black && cardOnTop.Sequential();
    }

    public void Refresh() {
        UpdateColliders();
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

    public void UpdateColliders() {
        topCollider.enabled = Sequential();
        fullCollider.enabled = cardOnTop == null;
    }

    private int NumParents() {
        if (parentCard == null) {
            return 0;
        }
        return 1 + parentCard.NumParents();
    }

    private Vector3 GetPosition() {
        if (stack == null) {
            return Vector3.zero;
        }
        return stack.transform.position + stack.offset.In3d() * NumParents();
    }

    public void MoveFast() {
        transform.position = GetPosition();
        sortingGroup.sortingOrder = NumParents();
        transform.parent = parentCard?.transform ?? stack.transform;
    }

    public IEnumerator Move() {
        var heading = GetPosition() - transform.position;
        var distance = heading.magnitude;
        var direction = heading / distance;

        sortingGroup.sortingOrder = 99;
        transform.parent = null;
        while(true) {
            var moveDistance = speed * Time.deltaTime;
            distance -= moveDistance;
            if (distance <= 0) {
                break;
            }
            transform.position += direction * moveDistance;

            yield return null;
        }
        MoveFast();
    }

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) {
        if (PlayManager.locked) {
            return;
        }
        sortingGroup.sortingOrder = 99;
        transform.parent = null;
        dragging = true;
    }

    void IDragHandler.OnDrag(PointerEventData eventData) {
        if (!dragging) {
            return;
        }
        var worldPos = eventData.pressEventCamera.ScreenToWorldPoint(eventData.position);
        worldPos.z = 0;
        transform.position = worldPos;
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData) {
        if (!dragging) {
            return;
        }
        dragging = false;
        sortingGroup.sortingOrder = NumParents();

        transform.parent = parentCard?.transform ?? stack.transform;
        var newStack = GetOverlappingStack();
        if (newStack == null || newStack == stack || PlayManager.locked) {
            StartCoroutine(Move()); // return the card
            return;
        }
        newStack.AddCard(this);
    }

    private Stack GetOverlappingStack() {
        var results = new List<Collider2D>();
        var colEnabled = fullCollider.enabled;
        fullCollider.enabled = true;
        fullCollider.OverlapCollider(new ContactFilter2D(), results);
        fullCollider.enabled = colEnabled;
        var cards = results.Select(r => r.GetComponent<Card>()).OfType<Card>();
        var stacks = cards.Select(c => c.stack).
            Union(results.Select(r => r.GetComponent<Stack>()).OfType<Stack>()).
            Where(s => s.CanAdd(this));
        if (stacks.Empty()) {
            return null;
        }
        var closest = stacks.First();
        var closestDistance = closest.transform.position.Distance(transform.position);
        foreach (var stack in stacks) {
            var distance = stack.transform.position.Distance(transform.position);
            if (distance < closestDistance) {
                closestDistance = distance;
                closest = stack;
            }
        }
        return closest;
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData) {
        var now = DateTime.Now;
        var diff = now - lastClickAt;
        if (diff.TotalSeconds <= doubleClickSpeed) {
            StartCoroutine(DoubleClick());
        }
        lastClickAt = now;
    }

    private IEnumerator DoubleClick() {
        if (PlayManager.locked || cardOnTop != null || stack is FinalStack || stack is Deck) {
            yield break;
        }
        foreach (var finalStack in PlayManager.instance.finalStacks) {
            if (finalStack.CanAdd(this)) {
                PlayManager.locked = true;
                yield return StartCoroutine(finalStack.AddCardSync(this));
                GameStateManager.instance.RecordUndo();
                PlayManager.locked = false;
                break;
            }
        }
    }

    public void SetStacks(Stack stack) {
        this.stack = stack;
        if (cardOnTop != null) {
            cardOnTop.SetStacks(stack);
        }
    }

    public override string ToString() {
        return number + " " + suit + " " + deck;
    }
}